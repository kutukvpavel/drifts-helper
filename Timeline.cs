namespace DriftsHelper
{
    public class Timeline
    {
        public Timeline(PrfTimingProvider prf, params IExternalTimelineProvider[] providers)
        {
            Prf = prf;
            Providers = providers;
            foreach (var item in providers)
            {
                ExternalSteps.AddRange(item.GetSteps());
            }
            ExternalSteps.Sort(IExternalStep.Comparer);
            foreach (var item in ExternalSteps)
            {
                item.ScanIndex = Prf.GetSpectrumIndex(item.InternalTimestamp);
            }
        }

        protected PrfTimingProvider Prf;
        protected IExternalTimelineProvider[] Providers;

        public List<IExternalStep> ExternalSteps { get; } = new();

        public IExternalStep? GetExternalStepByName(string name, int? startIndex)
        {
            return ExternalSteps.FirstOrDefault(x => ((x.Name == name) || (x.InternalizedName == name)) && x.ScanIndex > startIndex);
        }
        public IExternalStep? GetExternalStepByName(string name, string? startIndex = null)
        {
            if (startIndex != null)
            {
                var index = GetExternalStepByName(startIndex)?.ScanIndex;
                if (!index.HasValue) throw new ArgumentException($"Unable to find the startIndex literal '{startIndex}'");
                return GetExternalStepByName(name, index);
            }
            return ExternalSteps.FirstOrDefault(x => (x.Name == name) || (x.InternalizedName == name));
        }
    }
}