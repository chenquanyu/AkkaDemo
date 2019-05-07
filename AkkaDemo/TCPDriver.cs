using Akka.Actor;
using Akka.IO;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AkkaDemo
{
    public class TelnetClient : UntypedActor
    {
        public TelnetClient(string host, int port)
        {
            var endpoint = new DnsEndPoint(host, port);
            Context.System.Tcp().Tell(new Tcp.Connect(endpoint));
        }

        protected override void OnReceive(object message)
        {
            if (message is Tcp.Connected)
            {
                var connected = message as Tcp.Connected;
                Console.WriteLine("Connected to {0}", connected.RemoteAddress);

                // Register self as connection handler
                Sender.Tell(new Tcp.Register(Self));
                ReadConsoleAsync();
                Become(Connected(Sender));
            }
            else if (message is Tcp.CommandFailed)
            {
                Console.WriteLine("Connection failed");
            }
            else Unhandled(message);
        }

        private UntypedReceive Connected(IActorRef connection)
        {
            return message =>
            {
                if (message is Tcp.Received)  // data received from network
                {
                    var received = message as Tcp.Received;
                    Console.WriteLine(Encoding.ASCII.GetString(received.Data.ToArray()));
                }
                else if (message is string)   // data received from console
                {
                    connection.Tell(Tcp.Write.Create(ByteString.FromString((string)message + "\n")));
                    ReadConsoleAsync();
                }
                else if (message is Tcp.PeerClosed)
                {
                    Console.WriteLine("Connection closed");
                }
                else Unhandled(message);
            };
        }

        private void ReadConsoleAsync()
        {
            Task.Factory.StartNew(self => Console.In.ReadLineAsync().PipeTo((ICanTell)self), Self);
        }
    }

    public class EchoServer : UntypedActor
    {
        public EchoServer(int port)
        {
            Context.System.Tcp().Tell(new Tcp.Bind(Self, new IPEndPoint(IPAddress.Any, port)));
        }

        protected override void OnReceive(object message)
        {
            if (message is Tcp.Bound)
            {
                var bound = message as Tcp.Bound;
                Console.WriteLine("Listening on {0}", bound.LocalAddress);
            }
            else if (message is Tcp.Connected)
            {
                var connection = Context.ActorOf(Props.Create(() => new EchoConnection(Sender)));
                Sender.Tell(new Tcp.Register(connection));
            }
            else Unhandled(message);
        }
    }

    public class EchoConnection : UntypedActor
    {
        private readonly IActorRef _connection;

        public EchoConnection(IActorRef connection)
        {
            _connection = connection;
        }

        protected override void OnReceive(object message)
        {
            if (message is Tcp.Received)
            {
                var received = message as Tcp.Received;
                if (received.Data[0] == 'x')
                    Context.Stop(Self);
                else
                    _connection.Tell(Tcp.Write.Create(received.Data));
            }
            else Unhandled(message);
        }
    }

}
