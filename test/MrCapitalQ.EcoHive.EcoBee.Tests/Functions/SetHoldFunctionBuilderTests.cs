using MrCapitalQ.EcoHive.EcoBee.Functions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MrCapitalQ.EcoHive.EcoBee.Tests.Functions
{
    public class SetHoldFunctionBuilderTests
    {
        private readonly JsonSerializerOptions _options;

        public SetHoldFunctionBuilderTests()
        {
            _options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        [Fact]
        public void Build_UsingPresetClimate_ReturnsFunction()
        {
            var expected = "{\"type\":\"setHold\",\"params\":{\"holdClimateRef\":\"away\",\"holdType\":\"indefinite\"}}";

            var actual = new SetHoldFunctionBuilder("away").Build();

            Assert.Equal(expected, JsonSerializer.Serialize(actual, _options));
        }

        [Fact]
        public void Build_UsingTemperatures_ReturnsFunction()
        {
            var expected = "{\"type\":\"setHold\",\"params\":{\"coolHoldTemp\":60,\"heatHoldTemp\":70,\"holdType\":\"indefinite\"}}";

            var actual = new SetHoldFunctionBuilder(60, 70).Build();

            Assert.Equal(expected, JsonSerializer.Serialize(actual, _options));
        }

        [Fact]
        public void Build_SetFanspeed_ReturnsFunction()
        {
            var expected = "{\"type\":\"setHold\",\"params\":{\"holdClimateRef\":\"away\",\"fanSpeed\":\"test\",\"holdType\":\"indefinite\"}}";

            var actual = new SetHoldFunctionBuilder("away").SetFanSpeed("test").Build();

            Assert.Equal(expected, JsonSerializer.Serialize(actual, _options));
        }

        [Fact]
        public void Build_HoldIndefinitely_ReturnsFunction()
        {
            var expected = "{\"type\":\"setHold\",\"params\":{\"holdClimateRef\":\"away\",\"holdType\":\"indefinite\"}}";

            var actual = new SetHoldFunctionBuilder("away").HoldIndefinitely().Build();

            Assert.Equal(expected, JsonSerializer.Serialize(actual, _options));
        }

        [Fact]
        public void Build_HoldForDuration_ReturnsFunction()
        {
            var expected = "{\"type\":\"setHold\",\"params\":{\"holdClimateRef\":\"away\",\"holdType\":\"holdHours\",\"holdHours\":1}}";

            var actual = new SetHoldFunctionBuilder("away").HoldForDuration(1).Build();

            Assert.Equal(expected, JsonSerializer.Serialize(actual, _options));
        }

        [Fact]
        public void Build_HoldUntilNextTransition_ReturnsFunction()
        {
            var expected = "{\"type\":\"setHold\",\"params\":{\"holdClimateRef\":\"away\",\"holdType\":\"nextTransition\"}}";

            var actual = new SetHoldFunctionBuilder("away").HoldUntilNextTransition().Build();

            Assert.Equal(expected, JsonSerializer.Serialize(actual, _options));
        }

        [Fact]
        public void Build_HoldForDateRange_ReturnsFunction()
        {
            var expected = "{\"type\":\"setHold\",\"params\":{\"holdClimateRef\":\"away\",\"startDate\":\"0001-01-01\",\"startTime\":\"00:00:00\",\"endDate\":\"9999-12-31\",\"endTime\":\"23:59:59\",\"holdType\":\"dateTime\"}}";

            var actual = new SetHoldFunctionBuilder("away").HoldForDateRange(DateTime.MinValue, DateTime.MaxValue).Build();

            Assert.Equal(expected, JsonSerializer.Serialize(actual, _options));
        }
    }
}
