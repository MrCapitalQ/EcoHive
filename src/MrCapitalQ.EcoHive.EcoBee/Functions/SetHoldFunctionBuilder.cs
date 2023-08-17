using MrCapitalQ.EcoHive.EcoBee.Dtos;

namespace MrCapitalQ.EcoHive.EcoBee.Functions
{
    public class SetHoldFunctionBuilder : IThermostatFunctionBuilder
    {
        private int? _coolHoldTemp;
        private int? _heatHoldTemp;
        private string? _holdClimateRef;
        private string? _fanSpeed;
        private DateTime? _startDate;
        private DateTime? _endDate;
        private HoldType _holdType = HoldType.Indefinite;
        private int? _holdHours;

        public SetHoldFunctionBuilder(string preset)
        {
            _holdClimateRef = preset;
        }

        public SetHoldFunctionBuilder(int coolAtTemperature, int heatAtTemperature)
        {
            _coolHoldTemp = coolAtTemperature;
            _heatHoldTemp = heatAtTemperature;
        }

        public SetHoldFunctionBuilder SetFanSpeed(string fanSpeed)
        {
            _fanSpeed = fanSpeed;
            return this;
        }

        public SetHoldFunctionBuilder HoldIndefinitely()
        {
            _holdType = HoldType.Indefinite;
            _startDate = null;
            _endDate = null;
            _holdHours = null;
            return this;
        }

        public SetHoldFunctionBuilder HoldUntilNextTransition()
        {
            _holdType = HoldType.NextTransition;
            _startDate = null;
            _endDate = null;
            _holdHours = null;
            return this;
        }

        public SetHoldFunctionBuilder HoldForDuration(int hours)
        {
            _holdType = HoldType.HoldHours;
            _startDate = null;
            _endDate = null;
            _holdHours = hours;
            return this;
        }

        public SetHoldFunctionBuilder HoldForDateRange(DateTime start, DateTime end)
        {
            _holdType = HoldType.DateTime;
            _startDate = start;
            _endDate = end;
            _holdHours = null;
            return this;
        }

        public IThermostatFunction Build()
        {
            return new ThermostatFunction()
            {
                Type = "setHold",
                Params = new ThermostatHoldFunctionParams
                {
                    CoolHoldTemp = _coolHoldTemp,
                    HeatHoldTemp = _heatHoldTemp,
                    HoldClimateRef = _holdClimateRef,
                    FanSpeed = _fanSpeed,
                    StartDate = _startDate?.ToString("yyyy-MM-dd"),
                    StartTime = _startDate?.ToString("HH:mm:ss"),
                    EndDate = _endDate?.ToString("yyyy-MM-dd"),
                    EndTime = _endDate?.ToString("HH:mm:ss"),
                    HoldType = GetHoldTypeString(_holdType),
                    HoldHours = _holdHours
                }
            };
        }

        private static string GetHoldTypeString(HoldType holdType)
        {
            return holdType switch
            {
                HoldType.DateTime => "dateTime",
                HoldType.NextTransition => "nextTransition",
                HoldType.Indefinite => "indefinite",
                HoldType.HoldHours => "holdHours",
                _ => string.Empty,
            };
        }
    }

}
