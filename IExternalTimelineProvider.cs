namespace DriftsHelper
{
    public interface IExternalTimelineProvider
    {
        public IEnumerable<IExternalStep> GetStepTimes();
    }
}