using MrCapitalQ.EcoHive.EcoBee.Dtos;

namespace MrCapitalQ.EcoHive.EcoBee.Functions
{
    public class ResumeProgramFunctionBuilder : IThermostatFunctionBuilder
    {
        private bool _resumeAll;

        public ResumeProgramFunctionBuilder SetResumeAll(bool value)
        {
            _resumeAll = value;
            return this;
        }

        public IThermostatFunction Build()
        {
            return new ThermostatFunction()
            {
                Type = "resumeProgram",
                Params = new ThermostatResumeProgramFunctionParams
                {
                    ResumeAll = _resumeAll
                }
            };
        }
    }

}
