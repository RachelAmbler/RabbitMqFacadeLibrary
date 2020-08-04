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

namespace com.PureRomance.RabbitMqFacadeLibrary.Handlers
{
    public  class LoggingHandler
    {
        public delegate void VerboseLogging(string message);
        public delegate void VerboseExceptionLogging(Exception e);
        
        public bool UseVerboseLogging { get; set; }
        
        public VerboseLogging VerboseLoggingDelegate { get; set; }
        public VerboseExceptionLogging VerboseExceptionLoggingDelegate { get; set; }

        internal void Log(string message)
        {
            if(UseVerboseLogging)
                VerboseLoggingDelegate?.Invoke(message);
        }
        
        internal void Log(Exception e)
        {
            if(UseVerboseLogging)
                VerboseExceptionLoggingDelegate?.Invoke(e);
        }
        

        public LoggingHandler()
        {
            UseVerboseLogging = false;
            VerboseLoggingDelegate = null;
            VerboseExceptionLoggingDelegate = null;
        }
    }
}