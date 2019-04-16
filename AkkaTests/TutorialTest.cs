using Akka.Actor;
using Akka.TestKit.NUnit3;
using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Text;
using Tutorials.Tutorial1;
using AkkaDemo;
using FluentAssertions;

namespace AkkaTests
{

    public class ActorHierarchyExperiments : TestKit
    {
        [Test]
        public void Create_top_and_child_actor()
        {
            #region print-refs2
            var firstRef = Sys.ActorOf(Props.Create<Tutorials.Tutorial1.PrintMyActorRefActor>(), "first-actor");
            Console.WriteLine($"First: {firstRef}");
            firstRef.Tell("printit", ActorRefs.NoSender);
            #endregion
        }

        [Test]
        public void Start_and_stop_actors()
        {
            #region start-stop2
            var first = Sys.ActorOf(Props.Create<StartStopActor1>(), "first");
            first.Tell("stop");
            #endregion
        }

        [Test]
        public void Supervise_actors()
        {
            #region supervise2
            var supervisingActor = Sys.ActorOf(Props.Create<SupervisingActor>(), "supervising-actor");
            supervisingActor.Tell("failChild");
            #endregion
        }
    }

    public class DeviceSpec : TestKit
    {
        #region device-read-test
        [Test]
        public void Device_actor_must_reply_with_empty_reading_if_no_temperature_is_known()
        {
            var probe = CreateTestProbe();
            var deviceActor = Sys.ActorOf(Device.Props("group", "device"));

            deviceActor.Tell(new ReadTemperature(requestId: 42), probe.Ref);
            var response = probe.ExpectMsg<RespondTemperature>();
            response.RequestId.Should().Be(42);
            response.Value.Should().BeNull();
        }
        #endregion

        #region device-write-read-test
        [Test]
        public void Device_actor_must_reply_with_latest_temperature_reading()
        {
            var probe = CreateTestProbe();
            var deviceActor = Sys.ActorOf(Device.Props("group", "device"));

            deviceActor.Tell(new RecordTemperature(requestId: 1, value: 24.0), probe.Ref);
            probe.ExpectMsg<TemperatureRecorded>(s => s.RequestId == 1);

            deviceActor.Tell(new ReadTemperature(requestId: 2), probe.Ref);
            var response1 = probe.ExpectMsg<RespondTemperature>();
            response1.RequestId.Should().Be(2);
            response1.Value.Should().Be(24.0);

            deviceActor.Tell(new RecordTemperature(requestId: 3, value: 55.0), probe.Ref);
            probe.ExpectMsg<TemperatureRecorded>(s => s.RequestId == 3);

            deviceActor.Tell(new ReadTemperature(requestId: 4), probe.Ref);
            var response2 = probe.ExpectMsg<RespondTemperature>();
            response2.RequestId.Should().Be(4);
            response2.Value.Should().Be(55.0);
        }
        #endregion
    }

}
