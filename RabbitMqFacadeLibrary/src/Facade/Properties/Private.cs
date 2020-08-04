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
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace com.PureRomance.RabbitMqFacadeLibrary.Facade
{
    public partial class RabbitMqEndpoint: IAsyncDisposable, IDisposable
    {
        private Guid ExchangeId { get; set; }
        private CancellationToken LocalCancellationToken { get; set; }

        private Dictionary<string, int> QueueTtlValues { get; set; }
        private AsyncEventingBasicConsumer Consumer { get; set; }
        private IChannelParameters ChannelParameters { get; set; }
        private IModel Channel { get; set; }
        private MessageParameters DefaultMessageParameters { get; set; }
        private IModel ReturnChannel { get; set; }
        private int ReturnChannelQueueTtl { get; set; }
        private string ReturnChannelQueueName { get; set; }
        private AsyncEventingBasicConsumer ReturnChannelConsumer { get; set; }
        private SemaphoreSlim ReturnChannelLatch { get; set; }
        private byte[] ReturnData { get; set; }
        private string ExchangeType { get; set; }
        private static bool Initialized => _RabbitIn != null && !_RabbitIn.IsOpen && _RabbitOut != null && !_RabbitOut.IsOpen;
    }
}