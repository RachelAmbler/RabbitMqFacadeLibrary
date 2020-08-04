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
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace com.PureRomance.RabbitMqFacadeLibrary.Facade
{
    public partial class RabbitMqEndpoint: IAsyncDisposable, IDisposable
    {
        private static void ChannelOnBasicReturn(object sender, BasicReturnEventArgs e)
        {
            VerboseLoggingHandler.Log($"Event ChannelOnBasicReturn triggered Exchange='{e.Exchange}', routingKeyOrTopicName='{e.RoutingKey}', replyText='{e.ReplyText}'");
        }

        private static void ChannelOnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            VerboseLoggingHandler.Log($"Event ChannelOnCallbackException triggered: {e.Detail}");
            VerboseLoggingHandler.Log(e.Exception);
        }

        private static void ChannelOnModelShutdown(object sender, ShutdownEventArgs e)
        {
            VerboseLoggingHandler.Log($"Event ChannelOnModelShutdown triggered - Initiator {e.Initiator.ToString()}, classId={e.ClassId} methodId={e.MethodId}, replyCode={e.ReplyCode}. ReplyText '{e.ReplyText}'");
        }

        private static void ChannelOnBasicNacks(object sender, BasicNackEventArgs e)
        {
            VerboseLoggingHandler.Log($"Event ChannelOnBasicNacks triggered For tag='{e.DeliveryTag}', multiple='{e.Multiple}', requeue='{e.Requeue}'");
        }

        private static void ChannelOnBasicAcks(object sender, BasicAckEventArgs e)
        {
            VerboseLoggingHandler.Log($"Event ChannelOnBasicAcks triggered For tag='{e.DeliveryTag}', multiple='{e.Multiple}'");
        }
    }
}