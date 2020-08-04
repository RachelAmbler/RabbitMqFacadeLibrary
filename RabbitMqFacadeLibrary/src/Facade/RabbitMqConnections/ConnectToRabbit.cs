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
using System.Threading.Tasks;
using com.PureRomance.RabbitMqFacadeLibrary.Exceptions;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace com.PureRomance.RabbitMqFacadeLibrary.Facade
{
    public partial class RabbitMqEndpoint: IAsyncDisposable, IDisposable
    {
        public static bool ConnectToRabbit(string host, string virtualHost, string userName, string password, int port, string applicationName, CancellationTokenSource inOrInAndOutCts = null, CancellationTokenSource outCts = null)
        {
            return ConnectToRabbit(new List<string> {host}, virtualHost, userName, password, port, applicationName, 6000, 30, inOrInAndOutCts, outCts);
        }
        public static bool ConnectToRabbit(List<string> hosts, string virtualHost, string userName, string password, int port, string applicationName, int connectionRetryWait = 6000, int retries = 60, CancellationTokenSource inOrInAndOutCts = null, CancellationTokenSource outCts = null)
        {
            if (Initialized) return true;
            
            var factory = new ConnectionFactory
            {
                    UserName = userName,
                    Password = password,
                    VirtualHost = virtualHost,
                    DispatchConsumersAsync = true,
                    RequestedHeartbeat = new TimeSpan(0, 0, 10),
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                    Port = port switch
                    {
                            0 => AmqpTcpEndpoint.UseDefaultPort,
                            -1 => AmqpTcpEndpoint.DefaultAmqpSslPort,
                            _ => port
                    }
            };

            _RabbitIn = Connect(_RabbitIn, "Consumer", retries, connectionRetryWait);
            _RabbitOut = Connect(_RabbitOut, "Exchange", retries, connectionRetryWait);
            _RabbitInCts = inOrInAndOutCts ?? new CancellationTokenSource();
            _RabbitOutCts = outCts ?? inOrInAndOutCts;

            return Initialized;

            IConnection Connect(IConnection c, string type, int r, int t)
            {
                Exception le = null;
                if (c != null && c.IsOpen) return c;
                while (c == null || (!c.IsOpen && --r != 0))
                {
                    try
                    {
                        c = factory.CreateConnection(hosts, applicationName);
                        if (c.IsOpen)
                            return c;
                    }
                    catch (BrokerUnreachableException e)
                    {
                        Task.Delay(t);
                        le = e;
                    }
                    catch (Exception e)
                    {
                        le = e;
                        break;
                    }
                }

                var ex = new CannotConnectToRabbitException($"Unable to make {type} connections to rabbit", le);
                VerboseLoggingHandler.Log(ex);
                throw ex;
            }
        }
    }
}