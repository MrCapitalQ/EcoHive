using MrCapitalQ.EcoHive.EcoBee.Functions;
using System.Text.Json;

namespace MrCapitalQ.EcoHive.EcoBee.Tests.Functions
{
    public class ResumeProgramFunctionBuilderTests
    {
        [Fact]
        public void Build_Default_ReturnsFunction()
        {
            var actual = new ResumeProgramFunctionBuilder().Build();

            Assert.Equal("{\"type\":\"resumeProgram\",\"params\":{\"resumeAll\":false}}", JsonSerializer.Serialize(actual));
        }

        [Fact]
        public void Build_ResumeAll_ReturnsFunction()
        {
            var actual = new ResumeProgramFunctionBuilder().SetResumeAll(true).Build();

            Assert.Equal("{\"type\":\"resumeProgram\",\"params\":{\"resumeAll\":true}}", JsonSerializer.Serialize(actual));
        }
    }
}
