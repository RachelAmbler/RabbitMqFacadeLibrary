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
using RabbitMQ.Client;
using com.PureRomance.RabbitMqFacadeLibrary.Handlers;

namespace com.PureRomance.RabbitMqFacadeLibrary.Facade
{
    public partial class RabbitMqEndpoint: IAsyncDisposable, IDisposable
    {
        public static LoggingHandler VerboseLoggingHandler { get; set; }        
        private static IConnection _RabbitIn;
        private static IConnection _RabbitOut;
        private static CancellationTokenSource _RabbitInCts;
        private static CancellationTokenSource _RabbitOutCts;
        
        private string ReplyToQueueName { get; set; }
        private byte ReplyPriority { get; set; }

        private static object _LogLatch = new object();

        
        private IModel _rpcResponseChannel;
        

        private bool _disposed;
    }
}