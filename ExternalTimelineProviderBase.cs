namespace DriftsHelper
{
    public abstract class ExternalTimelineProviderBase : IExternalTimelineProvider
    {
        public abstract class ExternalStepBase : IExternalStep
        {
            public ExternalStepBase(double timestamp)
            {
                ExternalTimeStamp = timestamp;
            }

            public double ExternalTimeStamp { get; }
            public abstract string Name { get; }

            public string GetInternalizedName(double internalTimestamp)
            {
                return $"{Name} @ {internalTimestamp:F0} s";
            }
        }

        public ExternalTimelineProviderBase(string folderPath, string filter)
        {
            var files = Directory.GetFiles(folderPath, filter);
            if (files.Length != 1)
            {
                throw new InvalidOperationException("Unable to find temperature profile to be used!");
            }
            FilePath = files[0];
        }

        protected string FilePath;

        public abstract IEnumerable<IExternalStep> GetStepTimes();
    }
}