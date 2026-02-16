namespace DriftsHelper
{
    public class UVProfileProvider : ExternalTimelineProviderBase
    {
        public class UVStep : ExternalStepBase
        {
            public UVStep(double timestamp, bool isOn) : base(timestamp)
            {
                IsOn = isOn;
            }

            public bool IsOn { get; }
            public override string Name => $"UV_{(IsOn ? "ON" : "OFF")}";
        }

        public const string DefaultFilter = "*.uv";

        public UVProfileProvider(string folderPath, string filter = DefaultFilter) : base(folderPath, filter)
        {
            InnerObject = File.ReadAllText(FilePath);
        }

        protected string InnerObject;
        
        public override IEnumerable<UVStep> GetStepTimes()
        {
            string arrayInitializer = InnerObject.Split('=', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim()).First(x => x.StartsWith('{')).TrimEnd(';').Replace("\r", "");
            var values = arrayInitializer.Trim(' ', '{', '}', '\r', '\n').Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
            double accumulator = 0;
            bool isOn = false;
            foreach (var item in values)
            {
                isOn = !isOn;
                string mutableItem = item;
                double duration;
                if (item.Contains('/')) //Handle comments
                {
                    var lines = item.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    if (lines.Length > 1)
                    {
                        
                    }
                }
                if (mutableItem.Contains('+')) //Handle simple math
                {
                    
                }
                else
                {
                    duration = double.Parse(mutableItem);
                }
                accumulator += duration;
                yield return new UVStep(accumulator, isOn);
            }
        }
    }
}