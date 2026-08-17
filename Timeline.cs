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
                try
                {
                    item.ScanIndex = Prf.GetSpectrumIndex(item.InternalTimestamp);
                }
                catch (ArgumentOutOfRangeException)
                {
                    Console.WriteLine($"Timeline for '{prf.Name}' is longer than the experiment! The timeline will be cut short.");
                    break;
                }
            }
            ExternalSteps = ExternalSteps.TakeWhile(x => x.ScanIndex >= 0).ToList();
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
                if (!index.HasValue) return default;
                return GetExternalStepByName(name, index);
            }
            return ExternalSteps.FirstOrDefault(x => (x.Name == name) || (x.InternalizedName == name));
        }
    }
}