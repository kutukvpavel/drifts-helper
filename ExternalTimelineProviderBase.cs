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

            public int ScanIndex { get; set; } = -1;
            public double InternalTimestamp { get; set; } = double.NaN;
            public double ExternalTimeStamp { get; }
            public string InternalizedName => $"{Name} @ {InternalTimestamp:F0} s";

            public abstract string Name { get; }

            public override string ToString()
            {
                return $"{InternalizedName} @ Scan #{ScanIndex + 1}";
            }
        }

        public ExternalTimelineProviderBase(string folderPath, string filter, double timelineOffset = 0)
        {
            TimelineOffset = timelineOffset;
            var files = Directory.GetFiles(folderPath, filter);
            if (files.Length != 1)
            {
                throw new InvalidOperationException($"Unable to find profile to be used (filter: {filter})!");
            }
            FilePath = files[0];
        }

        protected string FilePath;
        protected abstract IEnumerable<IExternalStep> GetRawSteps();

        public double TimelineOffset { get; }
        public IEnumerable<IExternalStep> GetSteps()
        {
            foreach (var item in GetRawSteps())
            {
                item.InternalTimestamp = item.ExternalTimeStamp + TimelineOffset;
                yield return item;
            }
        }
    }
}