namespace DriftsHelper
{
    public interface IExternalStep
    {
        public double ExternalTimeStamp { get; }
        public string Name { get; }

        public string GetInternalizedName(double internalTimestamp);
    }
}