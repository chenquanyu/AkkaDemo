using AkkaDemo;
//using System;
using Xunit;
using Akka.Actor;
using Akka.TestKit;
using System.Collections.Generic;
using System;
using Akka.Util.Internal;
using Xunit.Abstractions;
using Akka.TestKit.NUnit3;

namespace AkkaTest
{
    public class UnitTest1 : TestKit
    {
        private readonly ITestOutputHelper output;

        public UnitTest1(ITestOutputHelper output)
        {
            this.output = output;
        }

        //[Fact]
        //public void Device_actor_must_reply_with_empty_reading_if_no_temperature_is_known()
        //{
        //    var probe = CreateTestProbe();
        //    var deviceActor = Sys.ActorOf(Device.Props("group", "device"));

        //    deviceActor.Tell(new ReadTemperature(requestId: 42), probe.Ref);
        //    var response = probe.ExpectMsg<RespondTemperature>();
        //    response.RequestId.Should().Be(42);
        //    response.Value.Should().BeNull();
        //}


        [Fact]
        public void DeviceGroupQuery_must_return_temperature_value_for_working_devices()
        {
            output.WriteLine("AAA");
            var requester = CreateTestProbe();

            var device1 = CreateTestProbe();
            var device2 = CreateTestProbe();

            var queryActor = Sys.ActorOf(DeviceGroupQuery.Props(
                actorToDeviceId: new Dictionary<IActorRef, string> { [device1.Ref] = "device1", [device2.Ref] = "device2" },
                requestId: 1,
                requester: requester.Ref,
                timeout: TimeSpan.FromSeconds(3)
            ));

            device1.ExpectMsg<ReadTemperature>(read => read.RequestId == 0);
            device2.ExpectMsg<ReadTemperature>(read => read.RequestId == 0);

            queryActor.Tell(new RespondTemperature(requestId: 0, value: 1.0), device1.Ref);
            queryActor.Tell(new RespondTemperature(requestId: 0, value: 2.0), device2.Ref);

            requester.ExpectMsg<RespondAllTemperatures>(msg =>
                msg.Temperatures["device1"].AsInstanceOf<Temperature>().Value == 1.0 &&
                msg.Temperatures["device2"].AsInstanceOf<Temperature>().Value == 2.0 &&
                msg.RequestId == 1);
        }

    }

}
