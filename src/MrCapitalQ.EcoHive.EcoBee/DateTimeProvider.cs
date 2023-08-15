namespace MrCapitalQ.EcoHive.EcoBee
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset UtcNow()
        {
            return DateTimeOffset.UtcNow;
        }
    }
}
