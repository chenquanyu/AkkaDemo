using Akka.Actor;
using Akka.IO;
using System;
using System.Net;

namespace AkkaDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var system = ActorSystem.Create("example");
            var manager = system.Tcp();

            var server = system.ActorOf(Props.Create(() => new EchoServer(8093)));

            var client = system.ActorOf(Props.Create(() => new TelnetClient("172.25.162.193", 8093)));

            Console.ReadKey();

        }
    }
}
