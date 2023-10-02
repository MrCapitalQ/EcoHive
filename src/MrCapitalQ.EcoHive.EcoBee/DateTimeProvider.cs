using System.Diagnostics.CodeAnalysis;

namespace MrCapitalQ.EcoHive.EcoBee
{
    [ExcludeFromCodeCoverage]
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset UtcNow()
        {
            return DateTimeOffset.UtcNow;
        }
    }
}
