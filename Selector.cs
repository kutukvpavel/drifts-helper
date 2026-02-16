namespace DriftsHelper
{
    public class Selector
    {
        public Selector(IEnumerable<IExternalTimelineProvider> providers)
        {
            Providers = providers;
        }

        public IEnumerable<IExternalStep> SelectStepsToProcess()
        {
            
        }

        protected IEnumerable<IExternalTimelineProvider> Providers;
    }
}