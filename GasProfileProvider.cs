namespace DriftsHelper
{
    public class GasProfileProvider : UVProfileProvider
    {
        public class GasStep : UVStep
        {
            public GasStep(double timestamp, bool isOn) : base(timestamp, isOn)
            {
                
            }
            public override string Name => $"Gas_{(IsOn ? "ON" : "OFF")}";
        }

        public new const string DefaultFilter = "*.gas";
        public GasProfileProvider(string folderPath, double timelineOffset = 0, string filter = DefaultFilter) : base(folderPath, timelineOffset, filter)
        {

        }

        protected override IEnumerable<GasStep> GetRawSteps()
        {
            return GetStepTimes(InnerObject).Select(x => new GasStep(x.Item1, x.Item2));
        }
    }
}