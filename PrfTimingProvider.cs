namespace DriftsHelper
{
    public class PrfTimingProvider
    {
        public const string PrfFilter = $"*.prf.csv";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="scanTimeOffset">Positive = DRIFT acqusition started earlier than experiment scripts, and vice-versa</param>
        /// <param name="fallbackSecondsPerSppectrum"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public PrfTimingProvider(string folderPath, double fallbackSecondsPerSppectrum = double.NaN)
        {
            var csvp = new CsvProvider(folderPath, PrfFilter);
            if (csvp.Spectra.Count != 1)
            {
                if (csvp.Spectra.Count > 1) Console.WriteLine("Multiple *.prf files, using fallback time.");
                if (!double.IsFinite(fallbackSecondsPerSppectrum)) throw new InvalidOperationException("No reliable timing source specified!");
                SecondsPerSectrum = fallbackSecondsPerSppectrum;
            }
            else
            {
                InnerObject = new IntensityProfile(csvp.Spectra[0]);
            }
        }

        protected IntensityProfile? InnerObject;
        protected double SecondsPerSectrum;

        public string Name => InnerObject?.Name ?? "N/A";

        /// <summary>
        /// Provides conservative values 
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public int GetSpectrumIndex(double seconds)
        {
            int ret;
            if (InnerObject != null)
            {
                if (InnerObject.TotalDuration < seconds) throw new ArgumentOutOfRangeException(nameof(seconds));
                ret = InnerObject.ExperimentTimeToSpectrumIndex(seconds);
            }
            else
            {
                ret = (int)Math.Floor(seconds / SecondsPerSectrum);
            }
            if (ret < 0) throw new KeyNotFoundException("This time is not present in the DRIFT timeline!");
            return ret;
        }
    }
}