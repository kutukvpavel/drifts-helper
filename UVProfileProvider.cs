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

        public UVProfileProvider(string folderPath, double timelineOffset = 0, string filter = DefaultFilter) : base(folderPath, filter, timelineOffset)
        {
            InnerObject = File.ReadAllText(FilePath);
        }

        protected string InnerObject;
        private static string StripMultilineComments(string s)
        {
            const string opener = "/*";
            const string closer = "*/";

            int multilineCommentStart = s.IndexOf(opener);
            if (multilineCommentStart < 0) return s;

            int multilineCommentEnd = s.IndexOf(closer);
            if (multilineCommentEnd < 0) throw new InvalidDataException("Multiline comment opened but not closed!");
            s = s.Remove(multilineCommentStart, multilineCommentEnd - multilineCommentStart + closer.Length).Trim('\n', ' ');
            StripMultilineComments(s);
            return s;
        }
        protected static IEnumerable<Tuple<double, bool>> GetStepTimes(string InnerObject)
        {
            string arrayInitializer = InnerObject.Split('=', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim()).First(x => x.StartsWith('{')).TrimEnd(';').Replace("\r", "");
            var values = arrayInitializer.Trim(' ', '{', '}', '\n').Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
            double accumulator = 0;
            bool isOn = false;
            foreach (var item in values)
            {
                isOn = !isOn;
                string mutableItem = item;
                double duration;
                if (item.Contains('/')) //Handle comments
                {
                    mutableItem = StripMultilineComments(item).Replace("\n", ""); //This handles multiline
                    int singleLineCommentStart = mutableItem.IndexOf("//");
                    if (singleLineCommentStart >= 0) //This handles single-line
                    {
                        mutableItem = mutableItem.Remove(singleLineCommentStart).Trim();
                    }
                }
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