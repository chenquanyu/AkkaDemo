using Akka.Actor;
using Akka.TestKit.NUnit3;
using System;
using System.Collections.Generic;
using System.Text;
using Tutorials.Tutorial1;
using Xunit;

namespace AkkaTest
{
    public class ActorHierarchyExperiments : TestKit
    {
        [Fact]
        public void Create_top_and_child_actor()
        {
            #region print-refs2
            var firstRef = Sys.ActorOf(Props.Create<PrintMyActorRefActor>(), "first-actor");
            Console.WriteLine($"First: {firstRef}");
            firstRef.Tell("printit", ActorRefs.NoSender);
            #endregion
        }

        [Fact]
        public void Start_and_stop_actors()
        {
            #region start-stop2
            var first = Sys.ActorOf(Props.Create<StartStopActor1>(), "first");
            first.Tell("stop");
            #endregion
        }

        [Fact]
        public void Supervise_actors()
        {
            #region supervise2
            var supervisingActor = Sys.ActorOf(Props.Create<SupervisingActor>(), "supervising-actor");
            supervisingActor.Tell("failChild");
            #endregion
        }
    }
}
