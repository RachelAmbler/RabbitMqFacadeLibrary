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

namespace com.PureRomance.RabbitMqFacadeLibrary.Facade
{
    public partial class RabbitMqEndpoint: IAsyncDisposable, IDisposable
    {
        public async Task<bool> Ping(string routingKeyOrTopicName = "", IDictionary<string, object> headers = null, MessageParameters mp = null)
        {
            VerboseLoggingHandler.Log($"Sending Hello ping");
            var response = await SendMessageInternal(routingKeyOrTopicName, ConvertStringToMessage(MessageContent_Ping), headers, MessageType.RequireAck, mp);
            return response != null && ConvertMessageToString(response) == MessageContent_PingResponse;
        }
        
        public async Task<byte[]> SendRpcMessageAsync<T>(T message, string routingKeyOrTopicName = "", IDictionary<string, object> headers = null, MessageParameters mp = null)
        {
            var bMessage = EnsureByteArrayFromGeneric(message);

            if (bMessage != null)
                return await SendMessageInternal(routingKeyOrTopicName, bMessage, headers, MessageType.RequireResponse, mp);

            return null;
        }
        
        public async Task<bool> SendMessageAwaitAckAsync<T>(T message, string routingKeyOrTopicName = "", IDictionary<string, object> headers = null, MessageParameters mp = null)
        {
            var bMessage = EnsureByteArrayFromGeneric(message);
            if (bMessage != null)
            {
                var response = await SendMessageInternal(routingKeyOrTopicName, bMessage, headers, MessageType.RequireAck, mp);
                return response != null && ConvertMessageToString(response) == MessageContent_PingResponse;
            }

            return false;
        }
        
        public async Task SendMessageAsync<T>(T message, string routingKeyOrTopicName = "", IDictionary<string, object> headers = null, MessageParameters mp = null)
        {
            var bMessage = EnsureByteArrayFromGeneric(message);
            if (bMessage != null)
                await SendMessageInternal(routingKeyOrTopicName, bMessage, headers, MessageType.Normal, mp);
            
        }
    }
}