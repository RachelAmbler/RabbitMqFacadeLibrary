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

namespace com.PureRomance.RabbitMqFacadeLibrary.Facade
{
    public partial class RabbitMqEndpoint: IAsyncDisposable, IDisposable
    {
        private static readonly string MessageTypeFragmentsRequestAmqAck = ".Request.AmqAck";
        private static readonly string MessageTypeFragmentsRequestAck = ".Request.Ack";
        private static readonly string MessageTypeFragmentsRequestReply = ".Request.Reply";
        private static readonly string MessageTypeFragmentsReply = ".Returned.Reply";
        private static readonly string MessageTypeFragmentsChat = ".Chat";

        private const string DictionaryKey_PassedQueueTtl = "pr-rpcqueue-ttl";
        private const string DictionaryKey_QueueTtl = "x-message-ttl";
        private const string DictionaryKey_ManualAck = "pr-ack-manual";

        private const string MessageContent_Ping = "Ping!";
        private const string MessageContent_PingResponse = "Pong!";
    }
}