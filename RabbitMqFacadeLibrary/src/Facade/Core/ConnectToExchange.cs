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
using com.PureRomance.RabbitMqFacadeLibrary.Exceptions;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client;


namespace com.PureRomance.RabbitMqFacadeLibrary.Facade
{
    public partial class RabbitMqEndpoint: IAsyncDisposable, IDisposable
    {
        private void ConnectToExchange()
        {
            var args = new Dictionary<string, object>();
        
            if (ChannelParameters.Ttl != -1)
                args.Add(DictionaryKey_QueueTtl, ChannelParameters.Ttl);
            
            var element = $"exchange {ExchangeName}";
            try
            {
                Channel.ExchangeDeclare(ExchangeName, ExchangeType, ChannelParameters.Durable, ChannelParameters.AutoDelete);
                element = $"queue {QueueName}";
                Channel.QueueDeclare(QueueName, ChannelParameters.Durable, false, ChannelParameters.AutoDelete, args);
            }
            
            catch (OperationInterruptedException oie)
            {
                var e = new ChannelOpenFailureException($"Unable to create the {EndpointType} {element}: " + oie.Message, oie);
                VerboseLoggingHandler.Log(e);
                throw e;
            }
            
            Channel.BasicAcks += ChannelOnBasicAcks;
            Channel.BasicNacks += ChannelOnBasicNacks;
            Channel.BasicReturn += ChannelOnBasicReturn;
            Channel.ModelShutdown += ChannelOnModelShutdown;
            Channel.CallbackException += ChannelOnCallbackException;

        }
    }
}