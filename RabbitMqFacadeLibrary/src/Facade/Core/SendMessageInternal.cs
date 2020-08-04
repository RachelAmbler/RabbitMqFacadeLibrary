/*
    RabbitMqFacadeLibrary - a simple to consume front end to RabbitMq using the RabbitMqClient
    Copyright (C) 2020 PureRomance, LLC

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using com.PureRomance.RabbitMqFacadeLibrary.Parameters;
using RabbitMQ.Client;

namespace com.PureRomance.RabbitMqFacadeLibrary.Facade
{
    public partial class RabbitMqEndpoint: IAsyncDisposable, IDisposable
    {
         private async Task<byte[]> SendMessageInternal(string routingKeyOrTopicName, byte[] message, IDictionary<string, object> headers, MessageType messageType, MessageParameters p = null)
        {
            if (EndpointType != EndpointTypeEnum.Publisher)
            {
                var e = new InvalidOperationException("Attempt to send a message on a consumer prohibited.");
                VerboseLoggingHandler.Log(e);
                throw e;
            }
            
            p ??= DefaultMessageParameters;
            if (string.IsNullOrEmpty(routingKeyOrTopicName))
                routingKeyOrTopicName = RoutingKeyOrTopicName;
            VerboseLoggingHandler.Log($"SendMessageInternal sending message, type='{messageType}', routingKeyOrTopicName='{routingKeyOrTopicName}', durable='{p.Durable}', mandatory='{p.Mandatory}', persistent='{p.Persistent}', priority='{p.Priority}', autoAck='{p.AutoAck}', resilient='{p.Resilient}', timeout='{p.TimeOut}' (ms)");
            headers ??= new Dictionary<string, object>();
            

            byte[] ret = null;

            var messageTypeText = new StringBuilder();
            
            var messageProperties = Channel.CreateBasicProperties();
            messageProperties.Type = MessageTypeFragmentsChat;
            messageProperties.Timestamp = AmqpTimestampNow;
            messageProperties.Priority = p.Priority;
            messageProperties.Persistent = p.Persistent;
            messageProperties.Expiration = p.TimeOut.ToString();
            if(p.Durable)
                messageProperties.DeliveryMode = 2;
            
            messageProperties.CorrelationId = ConversationId.ToString();

            if (!p.AutoAck)
                messageTypeText.Append((MessageTypeFragmentsRequestAmqAck));

            if ((messageType & MessageType.RequireAck) == MessageType.RequireAck)
                messageTypeText.Append(MessageTypeFragmentsRequestAck);
            if ((messageType & MessageType.RequireResponse) == MessageType.RequireResponse)
                messageTypeText.Append(MessageTypeFragmentsRequestReply);
            headers.Add(DictionaryKey_PassedQueueTtl, ReturnChannelQueueTtl);
            
            messageProperties.Type = messageTypeText.ToString();
            messageProperties.Headers = headers;
            
            try
            {
                ReturnData = null;
                messageProperties.ReplyTo = ReturnChannelQueueName;
                VerboseLoggingHandler.Log($"Publishing now");
                Channel.BasicPublish(ExchangeName, routingKeyOrTopicName, p.Mandatory, messageProperties, message);
                //_channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 0, 0, timeout.Value));
                if(((PublisherParameters)(ChannelParameters)).EnableConfirmSelect)
                    try
                    {
                        Channel.WaitForConfirms();
                    }
                    catch (InvalidOperationException e1)
                    {
                        if (e1.Message == "Confirms not selected")
                        {
                            var e = new InvalidOperationException("Invalid configuration. WaitForConfirms behavior requested, but Exchange not configured for them.", e1);
                            VerboseLoggingHandler.Log(e);
                            throw e;
                        }
                        else
                        {
                            VerboseLoggingHandler.Log(e1);
                            throw e1;
                        }
                    }

                if (messageType == MessageType.Normal)
                    return null;
                    
                VerboseLoggingHandler.Log($"Published. LatchCount='{ReturnChannelLatch.CurrentCount}', timeout='{p.TimeOut}' (ms)");
                await ReturnChannelLatch.WaitAsync(p.TimeOut, LocalCancellationToken);
                VerboseLoggingHandler.Log($"Released. LatchCount='{ReturnChannelLatch.CurrentCount}'");
            }
            finally
            {
                if (messageType != MessageType.Normal)
                {
                    VerboseLoggingHandler.Log($"Completed. LatchCount='{ReturnChannelLatch.CurrentCount}'");
                    ret = ReturnData;
                }

            }

            return ret;
        }
    }
}