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
using System.Text;
using System.Threading;
using RabbitMQ.Client;

namespace com.PureRomance.RabbitMqFacadeLibrary.Facade
{
    public partial class RabbitMqEndpoint: IAsyncDisposable, IDisposable
    {
        private static string ConvertMessageToString(ReadOnlyMemory<byte> data) => Encoding.Unicode.GetString(data.ToArray());
        public static string ConvertMessageToString(byte[] data) => Encoding.Unicode.GetString(data);
        public static byte[] ConvertStringToMessage(string message) => Encoding.Unicode.GetBytes(message);
        private static string DefineQueueName(string exchangeName, string exchangeType, string routeKeyOrTopicName, bool isConsumer)
        {
            var eType = exchangeType.Substring(0, 1).ToUpper();
            if (!string.IsNullOrEmpty(routeKeyOrTopicName))
                routeKeyOrTopicName = $":[{routeKeyOrTopicName}]";
            
            var d = isConsumer ? "C" : "E";
            return $"{d}:{exchangeName}:{eType}{routeKeyOrTopicName}";
        }
        
        private static byte[] EnsureByteArrayFromGeneric<T>(T data)
        {
            byte[] ret = null;
            if (typeof(T) == typeof(string))
                ret = ConvertStringToMessage(data.ToString());
            if (typeof(T) == typeof(byte[]))
                ret = (byte[])(object) data;

            return ret;
        }
        
        private static string EnsureStringFromGeneric<T>(T data)
        {
            var ret = string.Empty;
            if (typeof(T) == typeof(byte[]))
                ret = ConvertMessageToString((byte[])(object) data);
            if (typeof(T) == typeof(string))
                ret = data.ToString();

            return ret;
        }
        
        internal static AmqpTimestamp AmqpTimestampNow => new AmqpTimestamp(new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero).ToUnixTimeSeconds()); 
        internal static DateTime TimestampNowUtc(AmqpTimestamp timestamp) => new DateTime(1970,1,1,0,0,0,0, System.DateTimeKind.Utc).AddSeconds(timestamp.UnixTime);
        internal static DateTime TimestampNowLocal(AmqpTimestamp timestamp) => TimestampNowUtc(timestamp).ToLocalTime(); 

        private static string GetFullExchangeType(string exchangeType)
        {
            return exchangeType.ToLower() switch
            {
                    "d" => RabbitMQ.Client.ExchangeType.Direct,
                    "t" => RabbitMQ.Client.ExchangeType.Topic,
                    "f" => RabbitMQ.Client.ExchangeType.Fanout,
                    _ => RabbitMQ.Client.ExchangeType.Headers
            };
        }

        private CancellationToken GetToken(CancellationToken token)
        {
            if (EndpointType == EndpointTypeEnum.Publisher || EndpointType == EndpointTypeEnum.RpcPublisher)
                return token == CancellationToken.None ? _RabbitOutCts.Token : token;
            return token == CancellationToken.None ? _RabbitInCts.Token : token;
        }
    }
}