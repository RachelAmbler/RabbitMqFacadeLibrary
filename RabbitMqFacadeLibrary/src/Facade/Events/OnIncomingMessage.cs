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
using System.Threading.Tasks;
using RabbitMQ.Client.Events;
using com.PureRomance.RabbitMqFacadeLibrary.EventArguments;
using com.PureRomance.RabbitMqFacadeLibrary.Parameters;

namespace com.PureRomance.RabbitMqFacadeLibrary.Facade
{
    public partial class RabbitMqEndpoint: IAsyncDisposable, IDisposable
    {
        protected virtual async Task OnIncomingMessageAsync(object sender, BasicDeliverEventArgs ea)
        {
            await Task.Delay(0, LocalCancellationToken);
            var ackMode = ((ConsumerParameters) ChannelParameters).AutoAckMode;
            var isRpc = false;
            var requiresAck = false;
            
            VerboseLoggingHandler.Log($"Event OnIncomingMessageAsync triggered For tag='{ea.DeliveryTag}', exchange='{ea.Exchange}', routingKeyOrTopicName='{ea.RoutingKey}', consumerTag='{ea.ConsumerTag}', deliveryTag='{ea.DeliveryTag}', redelivered='{ea.Redelivered}'");
            
            var messageType = ea.BasicProperties.Type;
            if (!ea.BasicProperties.IsCorrelationIdPresent())
            {
                var e = new InvalidOperationException($"Missing correlationId in message from exchange={ea.Exchange} deliveryTag=${ea.DeliveryTag}");
                VerboseLoggingHandler.Log(e);
                throw e;
            }

            VerboseLoggingHandler.Log($"MessageType='{messageType}'");

            if (messageType.Contains(MessageTypeFragmentsRequestAmqAck))
            {
                if (ackMode == ConsumerParameters.AutoAckModeEnum.OnReceipt)
                    Channel.BasicAck(ea.DeliveryTag, false);
                if (ackMode == ConsumerParameters.AutoAckModeEnum.Manual)
                    requiresAck = true;
            }

            if ((messageType.Contains(MessageTypeFragmentsRequestAck) || messageType.Contains(MessageTypeFragmentsRequestReply) && ea.BasicProperties.IsReplyToPresent()))
            {
                isRpc = true;
                EndpointType = EndpointTypeEnum.RpcConsumer;
                ReplyToQueueName = ea.BasicProperties.ReplyTo;
                VerboseLoggingHandler.Log($"Reply requested to '{ReplyToQueueName}'");

                if (ea.BasicProperties.IsHeadersPresent())
                {
                    VerboseLoggingHandler.Log($"Headers found");
                    
                    var headers = ea.BasicProperties.Headers;
                    if (headers.ContainsKey(DictionaryKey_PassedQueueTtl))
                    {
                        var ttl = Convert.ToInt32(headers[DictionaryKey_PassedQueueTtl]);
                        VerboseLoggingHandler.Log($"ttl='{ttl}'");
                        
                        if (QueueTtlValues.ContainsKey(ReplyToQueueName))
                            QueueTtlValues[ReplyToQueueName] = ttl;
                        else
                            QueueTtlValues.Add(ReplyToQueueName, ttl);
                        
                        ea.BasicProperties.Headers.Remove(DictionaryKey_PassedQueueTtl);
                    }
                }

                // Handle basic message acknowledgement here.
                if (messageType.Contains(MessageTypeFragmentsRequestAck) && ConvertMessageToString(ea.Body) == MessageContent_Ping)
                {
                    VerboseLoggingHandler.Log($"Ack'ing the Ping");
                    await ReplyAsync(MessageContent_PingResponse, null);
                }
            }

            VerboseLoggingHandler.Log($"Delegate check - does messageType have '{MessageTypeFragmentsRequestReply}'?");
            if (messageType.Contains(MessageTypeFragmentsRequestReply))
            {
                
                VerboseLoggingHandler.Log($"Confirmed. Fire delegate");
                var args = new IncomingRabbitMqMessageEventArgs(isRpc, requiresAck, ea);
                await Task.Run(() => IncomingMessage?.Invoke(this, args), LocalCancellationToken);
            }
        }
    }
}