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
using System.Threading;
using com.PureRomance.RabbitMqFacadeLibrary.Parameters;

namespace com.PureRomance.RabbitMqFacadeLibrary.Facade
{
    public partial class RabbitMqEndpoint: IAsyncDisposable, IDisposable
    {
        public static RabbitMqEndpoint NewOutboundPublisher(string name, string type, string routingKeyOrTopicName,  PublisherParameters p = null, MessageParameters defaultMessageParameters = null, CancellationToken token = default(CancellationToken))
        {
            var exchangeType = GetFullExchangeType(type);
            p ??= new PublisherParameters() { AcceptReplies = false, AutoDelete = true, Durable = false, EnableConfirmSelect = false, ReplyQueueTtl = 0, Ttl = 6000};
            defaultMessageParameters ??= new MessageParameters() { AutoAck = false, Durable = false, Mandatory = false, Persistent = false, Priority = 3, Resilient = false, TimeOut = 6000};
            
            var ret = new RabbitMqEndpoint
            {
                    Channel = _RabbitOut.CreateModel(),
                    EndpointType = EndpointTypeEnum.Publisher,
                    ExchangeName = name,
                    QueueName = DefineQueueName(name, exchangeType, "", false),
                    RoutingKeyOrTopicName = routingKeyOrTopicName,
                    ExchangeId = Guid.NewGuid(),
                    LocalCancellationToken = _RabbitOutCts?.Token ?? token,
                    ChannelParameters = p,
                    ExchangeType = GetFullExchangeType(type),
                    DefaultMessageParameters = defaultMessageParameters
            };
            
            VerboseLoggingHandler.Log($"Building a {exchangeType} outbound publisher, name='{name}', routingKeyOrTopicName='{routingKeyOrTopicName}', durable='{p.Durable}', ttl='{p.Ttl}', autoDelete='{p.AutoDelete}', acceptReplies='{p.AcceptReplies}', confirmSelect='{p.EnableConfirmSelect}'");
            ret.ConnectToExchange();
            
            if(p.EnableConfirmSelect)
                ret.Channel.ConfirmSelect();

            if (p.AcceptReplies)
            {
                VerboseLoggingHandler.Log($"Building a return consumer, ExchangeId='{ret.ExchangeId}', replyQueueTtl='{p.ReplyQueueTtl}'");
                ret.DefineRpcConsumer(ret.ExchangeName, ret.ExchangeId, p.ReplyQueueTtl);
            }

            VerboseLoggingHandler.Log("Exchange ready");
            return ret;
        }
    }
}