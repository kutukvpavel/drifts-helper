using YamlDotNet.Serialization;

namespace DriftsHelper
{
    public class TempProfileProvider : ExternalTimelineProviderBase
    {
        public class TemperatureStep : ExternalStepBase
        {
            public TemperatureStep(double timestamp, double temperature) : base(timestamp)
            {
                Temperature = temperature;
            }

            public double Temperature { get; }
            public override string Name => $"{Temperature:F0}°C";
        }

        public const string DefaultFilter = "*.yaml";
        public const double AssumedInitialTemp = 30.0;

        public TempProfileProvider(string folderPath, double timelineOffset = 0, string filter = DefaultFilter) : base(folderPath, filter, timelineOffset)
        {
            InnerObject = Profile.Load(FilePath);
        }

        protected Profile InnerObject;
        protected override IEnumerable<TemperatureStep> GetRawSteps()
        {
            double accumulator = InnerObject.InitialWaitSeconds ?? 0;
            double temp = AssumedInitialTemp;
            foreach (var item in InnerObject.Segments)
            {
                switch (item.Type)
                {
                    case SegmentTypes.Isothermal:
                        accumulator += item.Total!.Value;
                        break;
                    case SegmentTypes.CalculatedRamp:
                        accumulator += item.CalculateRampDuration(temp);
                        break;
                    default: throw new InvalidOperationException("Unsupported TempProServer scripting method!"); 
                }
                temp = item.T;
                yield return new TemperatureStep(accumulator, temp);
            }
        }

        /****
            THE FOLLOWING IS COPIED FROM TempProServer project:
        **/

        public enum SegmentTypes
        {
            Isothermal,
            CalculatedRamp,
            ControlledRamp
        }

        [YamlSerializable]
        protected class ProfileSegment
        {
            public ProfileSegment() { }

            public double T { get; set; } //Target temp, degC
            public double? Ramp { get; set; } //Ramp rate, degC/min
            public bool? ControlRamp { get; set; } //True == wait until ramp target is actually reached
            public int? Total { get; set; } //Total duration, including theoretical ramp, seconds
            public int? Soak { get; set; } //Soak duration, wait until target temp is actually reached, seconds

            [YamlIgnore]
            public SegmentTypes Type {
                get {
                    if (Ramp != null)
                    {
                        if (ControlRamp ?? false)
                        {
                            return SegmentTypes.ControlledRamp;
                        }
                        return SegmentTypes.CalculatedRamp;
                    }
                    return SegmentTypes.Isothermal;
                }
            }

            public int CalculateRampDuration(double initialTemp)
            {
                if (Type == SegmentTypes.Isothermal)
                    throw new InvalidOperationException("Ramp duration can not be calculated for segment types other than CalculatedRamp.");
                return (int)Math.Ceiling(Math.Abs(initialTemp - T) / Ramp!.Value * 60.0);
            }
        }

        [YamlSerializable]
        protected class Profile
        {
            public static Profile Load(string path)
            {
                var deserializer = new DeserializerBuilder().Build();
                using var reader = new StreamReader(path);
                return deserializer.Deserialize<Profile>(reader);
            }

            public Profile() {
                Segments = Array.Empty<ProfileSegment>();
            }

            public int? InitialWaitSeconds { get; set; }
            public double? CommonRampRate { get; set; } //Enforce a single ramp rate for all segments (makes isothermals ramps!)
            public double? LimitRampRate { get; set; }
            public double? AfterScriptT { get; set; } //Temperature to set after script completion (no time constraints)
            public ProfileSegment[] Segments { get; set; }
            public bool EnableLog { get; set; } = true;
        }
    }
}