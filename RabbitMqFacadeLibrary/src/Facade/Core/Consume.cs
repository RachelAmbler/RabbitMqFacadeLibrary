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
using System.Threading.Tasks;
using com.PureRomance.RabbitMqFacadeLibrary.Parameters;
using RabbitMQ.Client;

namespace com.PureRomance.RabbitMqFacadeLibrary.Facade
{
    public partial class RabbitMqEndpoint: IAsyncDisposable, IDisposable
    {
        public void Listen()
        {
            if (EndpointType != EndpointTypeEnum.Consumer)
            {
                var e = new InvalidOperationException("Attempt to listen on a non-consumer prohibited.");
                VerboseLoggingHandler.Log(e);
                throw e;
            }
            Channel.BasicConsume(QueueName,  ((ConsumerParameters) (ChannelParameters)).AutoAckMode == ConsumerParameters.AutoAckModeEnum.Auto , Consumer);
        }

        public async Task SendAck(ulong deliveryTag)
        {
            await Task.Delay(0, LocalCancellationToken);
            Channel.BasicAck(deliveryTag, false);
        }
        
        public async Task SendNack(ulong deliveryTag, bool requeue)
        {
            await Task.Delay(0, LocalCancellationToken);
            Channel.BasicNack(deliveryTag, false, requeue);
        }
        
        public async Task SendCancel(string consumerTag, bool requeue)
        {
            await Task.Delay(0, LocalCancellationToken);
            Channel.BasicCancel(consumerTag);
        }
        
         public async Task<bool> ReplyAsync<T>(T message, IDictionary<string, object> headers = null)
        {
            await Task.Delay(0, LocalCancellationToken);
            if (EndpointType != EndpointTypeEnum.RpcConsumer)
            {
                var e = new InvalidOperationException("Attempt to reply to a message on a non Rpc Consumer prohibited.");
                VerboseLoggingHandler.Log(e);
                throw e;
            }

            var bMessage = EnsureByteArrayFromGeneric(message);
            if (bMessage == null)
                return false;
            
            VerboseLoggingHandler.Log($"Replying to message");
            var args = new Dictionary<string, object>();
            
            if (_rpcResponseChannel == null)
            {
                VerboseLoggingHandler.Log($"Response queue not open yet. Creating one now");
                if(QueueTtlValues.ContainsKey(ReplyToQueueName))
                    args.Add(DictionaryKey_QueueTtl, QueueTtlValues[ReplyToQueueName]);
                VerboseLoggingHandler.Log($"Using requested ttl='{QueueTtlValues[ReplyToQueueName]}'");
                _rpcResponseChannel = _RabbitOut.CreateModel();
                // Use the same TTL value as was used to create the queue on the other side
                _rpcResponseChannel.QueueDeclare(ReplyToQueueName, false, false, true, args);
                VerboseLoggingHandler.Log($"Queue ready");
                
            }

            var messageProperties = _rpcResponseChannel.CreateBasicProperties();
            var rp = ReplyPriority;
            if (rp > 0)
                rp--;
            messageProperties.Priority = rp;
            messageProperties.Headers = headers;
            messageProperties.Type = MessageTypeFragmentsReply;
            messageProperties.Persistent = false;
            VerboseLoggingHandler.Log($"Publishing reply");
            _rpcResponseChannel.BasicPublish("", ReplyToQueueName, true, messageProperties,  bMessage);
            VerboseLoggingHandler.Log($"Sent");
            return true;

        }
    }
}