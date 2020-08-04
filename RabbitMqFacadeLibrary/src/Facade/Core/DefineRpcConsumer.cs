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
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace com.PureRomance.RabbitMqFacadeLibrary.Facade
{
    public partial class RabbitMqEndpoint: IAsyncDisposable, IDisposable
    {
        private void DefineRpcConsumer(string exchangeName, Guid connectedExchangeId, int ttl = 2000000)
        {
            ttl = 10000000;
            var args = new Dictionary<string, object> {{DictionaryKey_QueueTtl, ttl}};
            ReturnChannelQueueTtl = ttl;
            ReturnChannel = _RabbitIn.CreateModel();
            ReturnChannelQueueName = $"RPC:{exchangeName}:{connectedExchangeId}";
            ReturnChannel.QueueDeclare(ReturnChannelQueueName, false, false, true, args);
            ReturnChannelConsumer = new AsyncEventingBasicConsumer(_rpcResponseChannel);
            ReturnChannelConsumer.Received += RpcReturnChannelConsumerOnReceivedAsync;
            ReturnChannel.BasicConsume(ReturnChannelQueueName, true, ReturnChannelConsumer);
            ReturnChannelLatch = new SemaphoreSlim(0, 1);
            
            
            VerboseLoggingHandler.Log($"Reply queue created, name='{ReturnChannelQueueName}'");
            
        }
    }
}