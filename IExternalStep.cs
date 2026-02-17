namespace DriftsHelper
{
    public class ExternalStepComparer : IComparer<IExternalStep>
    {
        public int Compare(IExternalStep? x, IExternalStep? y)
        {
            if (x == null || y == null) throw new NullReferenceException();
            if (x.InternalTimestamp == y.InternalTimestamp) return 0;
            return (x.InternalTimestamp > y.InternalTimestamp) ? 1: -1;
        }
    }

    public interface IExternalStep
    {
        public static ExternalStepComparer Comparer { get; } = new();

        public int ScanIndex { get; set; }
        public double InternalTimestamp { get; set; }
        public double ExternalTimeStamp { get; }
        public string Name { get; }
        public string InternalizedName { get; }
    }
}