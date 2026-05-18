namespace DriftsHelper
{
    public class UVProfileProvider : ExternalTimelineProviderBase
    {
        public static UVProfileProvider? TryCreate(string folderPath, double timelineOffset = 0)
        {
            try
            {
                return new UVProfileProvider(folderPath, timelineOffset);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't create UV profile provider: " + ex.Message);
                return null;
            }
        }

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

        public UVProfileProvider(string folderPath, double timelineOffset = 0, string filter = DefaultFilter) : base(folderPath, filter, timelineOffset)
        {
            InnerObject = File.ReadAllText(FilePath);
        }

        protected string InnerObject;
        protected static IEnumerable<Tuple<double, bool>> GetStepTimes(string InnerObject)
        {
            string arrayInitializer = InnerObject.Split('=', 2, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim()).First(x => x.StartsWith('{')).TrimEnd(';').Replace("\r", "");
            var values = arrayInitializer.Trim(' ', '{', '}', '\n').Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
            double accumulator = 0;
            bool isOn = false;
            foreach (var item in values)
            {
                string mutableItem = item;
                double duration;
                isOn = !isOn;
                if (mutableItem.Contains('+')) //Handle simple math
                {
                    var operands = mutableItem.Split('+').Select(x => x.Trim());
                    duration = operands.Aggregate((double)0, (d, s) => d + double.Parse(s));
                }
                else
                {
                    duration = double.Parse(mutableItem);
                }
                accumulator += duration;
                yield return new Tuple<double, bool>(accumulator, isOn);
            }
        }

        protected override IEnumerable<UVStep> GetRawSteps()
        {
            return GetStepTimes(InnerObject).Select(x => new UVStep(x.Item1, x.Item2));
        }
    }
}