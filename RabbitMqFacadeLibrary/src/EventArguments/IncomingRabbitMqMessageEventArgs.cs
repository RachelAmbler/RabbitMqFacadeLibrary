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
using com.PureRomance.RabbitMqFacadeLibrary.Facade;
using RabbitMQ.Client.Events;

namespace com.PureRomance.RabbitMqFacadeLibrary.EventArguments
{
   public class IncomingRabbitMqMessageEventArgs
    {
        public Guid CorrelationId { get; private set; }
        public byte[] Message { get; private set; }
        public bool IsRpc { get; private set; }
        public bool RequiresAck { get; private set; }
        public DateTime MessageSentUtc { get; private set; }
        public DateTime MessageReceivedUtc { get; set; }
        public string ContentType { get; private set; }
        public string ContentEncoding { get; private set; }
        public string SourceExchange { get; private set; }
        public bool Redelivered { get; private set; }
        public string SourceRoutingKeyOrTopicName { get; private set; }
        public ulong DeliveryTag { get; private set; }
        public string ConsumerTag { get; private set; }
        public string MessageId { get; private set; }
        public string MessageType { get; private set; }
        private IDictionary<string, object> Headers { get; set; }

        public IEnumerable<string> GetHeaders()
        {
            if (Headers == null) yield break;
            foreach (var k in Headers.Keys)
                yield return k;
        }

        public object HeaderValue(string key)
        {
            if (Headers != null && Headers.ContainsKey(key))
                return Headers[key];
            else
                return null;
        }

        internal IncomingRabbitMqMessageEventArgs(bool isRpc, bool requiresAck, BasicDeliverEventArgs ea)
        {
            
            CorrelationId = new Guid(ea.BasicProperties.CorrelationId);
            Message = ea.Body.ToArray();
            IsRpc = isRpc;
            RequiresAck = requiresAck;
            MessageSentUtc = RabbitMqEndpoint.TimestampNowUtc(ea.BasicProperties.Timestamp);
            MessageReceivedUtc = DateTime.UtcNow;
            ContentType = ea.BasicProperties.ContentType;
            ContentEncoding = ea.BasicProperties.ContentEncoding;
            Headers = ea.BasicProperties.Headers;
            SourceExchange = ea.Exchange;
            Redelivered = ea.Redelivered;
            SourceRoutingKeyOrTopicName = ea.RoutingKey;
            DeliveryTag = ea.DeliveryTag;
            ConsumerTag = ea.ConsumerTag;
            MessageId = ea.BasicProperties.MessageId;
            MessageType = ea.BasicProperties.Type;

        }
    }
}