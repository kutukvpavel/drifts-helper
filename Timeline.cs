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
    }
}