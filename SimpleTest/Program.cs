using System;
using System.Threading;
using System.Threading.Tasks;
using com.PureRomance.RabbitMqFacadeLibrary.EventArguments;
using com.PureRomance.RabbitMqFacadeLibrary.Facade;
using com.PureRomance.RabbitMqFacadeLibrary.Handlers;
using com.PureRomance.RabbitMqFacadeLibrary.Parameters;

namespace SimpleTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var mp = new MessageParameters()
            {
                    AutoAck = false,
                    Durable = true,
                    Mandatory = true,
                    Persistent = true,
                    Priority = 3,
                    Resilient = true,
                    TimeOut = 32000
            };
            var ep = new PublisherParameters()
            {
                    AutoDelete = false,
                    Durable = true,
                    AcceptReplies = true,
                    ReplyQueueTtl = 8000,
                    Ttl = 8000,
                    EnableConfirmSelect = true
            };
            Console.Write("Enter server name to connect to: ");
            var broker = Console.ReadLine();
            Console.Write("Login Name: ");
            var login = Console.ReadLine();
            Console.Write("Password: ");
            var password = Console.ReadLine();
			
            var cts = new CancellationTokenSource();
            var lh = new LoggingHandler()
                    {
                            UseVerboseLogging = true,
                            VerboseLoggingDelegate = ConsoleLoggers.LogToConsole,
                            VerboseExceptionLoggingDelegate = ConsoleLoggers.LogExceptionToConsole
                    }
                    ;
            RabbitMqEndpoint.VerboseLoggingHandler = lh;
            
            RabbitMqEndpoint.ConnectToRabbit(broker, "/", login, password, 0, "RabbitMqFacadeTest1", cts);
            
            var cp = new ConsumerParameters()
            {
                    AutoAckMode = ConsumerParameters.AutoAckModeEnum.Manual,
                    AutoDelete = false,
                    Durable = true
            };
            
            var consumer = RabbitMqEndpoint.NewInboundConsumer("test", "d", "d", cp);

            consumer.IncomingMessage += OnIncomingMessage;
            
            consumer.Listen();
            
            var publisher = RabbitMqEndpoint.NewOutboundPublisher("test", "d",  "d", ep, mp);
            
            var response = await publisher.SendRpcMessageAsync("My name is Bob");
            if(response == null)
                Console.WriteLine("No one there!");
            else
                Console.WriteLine(RabbitMqEndpoint.ConvertMessageToString(response));

            response = await publisher.SendRpcMessageAsync("My name is Sue");
            if(response == null)
                Console.WriteLine("No one there!");
            else
                Console.WriteLine(RabbitMqEndpoint.ConvertMessageToString(response));

            response = await publisher.SendRpcMessageAsync("Ribbit");
            if(response == null)
                Console.WriteLine("No one there!");
            else
                Console.WriteLine(RabbitMqEndpoint.ConvertMessageToString(response));

            response = await publisher.SendRpcMessageAsync("My name is Rachel");
            if(response == null)
                Console.WriteLine("No one there!");
            else
                Console.WriteLine(RabbitMqEndpoint.ConvertMessageToString(response));

            Console.ReadLine();

        }

        static async void OnIncomingMessage(object sender, IncomingRabbitMqMessageEventArgs ea)
        {
            Console.WriteLine($"------> In the event Message='{RabbitMqEndpoint.ConvertMessageToString(ea.Message)}' sent='{ea.MessageSentUtc.ToLocalTime()}'");
            var message = RabbitMqEndpoint.ConvertMessageToString(ea.Message);

            if (ea.RequiresAck)
            {
                Console.WriteLine("------> Sending a basic Ack");
                await ((RabbitMqEndpoint) sender).SendAck(ea.DeliveryTag);
            }

            if (ea.IsRpc)
            {
            
            
                var reply = "Dunno what to say...";
                if (message.StartsWith("My name is "))
                {
                    var name = message.Replace("My name is ", "");
                    reply = $"Hello to you, {name}!";
                }

                var me1 = (RabbitMqEndpoint) sender;        
                await me1.ReplyAsync<string>(reply, null);
        
            }
        }
    }
}