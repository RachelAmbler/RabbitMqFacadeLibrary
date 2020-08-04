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
using com.PureRomance.RabbitMqFacadeLibrary.Parameters;
using RabbitMQ.Client.Events;

namespace com.PureRomance.RabbitMqFacadeLibrary.Facade
{
    public partial class RabbitMqEndpoint: IAsyncDisposable, IDisposable
    {
        public static RabbitMqEndpoint NewInboundConsumer(string name, string type, string routingKeyOrTopicName = "", ConsumerParameters p = null, CancellationToken token = default(CancellationToken))
        {
            p ??= new ConsumerParameters();
            var exchangeType = GetFullExchangeType(type);
            var ret = new RabbitMqEndpoint
            {
                    Channel = _RabbitIn.CreateModel(),
                    EndpointType = EndpointTypeEnum.Consumer,
                    ExchangeName = name,
                    LocalCancellationToken = _RabbitInCts?.Token ?? token,
                    QueueTtlValues = new Dictionary<string, int>(),
                    ChannelParameters = p,
                    ExchangeType = GetFullExchangeType(type),
                    QueueName = DefineQueueName(name, exchangeType, routingKeyOrTopicName, true),
            };

            VerboseLoggingHandler.Log($"Building a {exchangeType} inbound consumer, name='{name}', routingKeyOrTopicName='{routingKeyOrTopicName}', durable='{p.Durable}', ttl='{p.Ttl}', autoDelete='{p.AutoDelete}', autoAckMode='{p.AutoAckMode}'");
            ret.ConnectToExchange();

            try
            {
                VerboseLoggingHandler.Log($"Binding queue '{ret.QueueName}'");
                ret.Channel.QueueBind(ret.QueueName, name, routingKeyOrTopicName, new Dictionary<string, object>());
            }
            catch (Exception e)
            {
                VerboseLoggingHandler.Log(e);
                throw e;
            }

            VerboseLoggingHandler.Log("Initiating the consumer");
            ret.Consumer = new AsyncEventingBasicConsumer(ret.Channel);
            
            ret.Consumer.Received += ret.OnIncomingMessageAsync;
            
            VerboseLoggingHandler.Log("Consumer ready");
            
            return ret;
        }
    }
}