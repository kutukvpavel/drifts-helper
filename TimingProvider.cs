namespace DriftsHelper
{
    public class TimingProvider
    {
        public const string PrfFilter = $"*.prf.csv";

        public TimingProvider(string folderPath)
        {
            var csvp = new CsvProvider(folderPath, PrfFilter);
            if (csvp.Spectra.Count != 1) throw new InvalidDataException("Check prf files");
            InnerObject = new IntensityProfile(csvp.Spectra[1]);
        }

        protected IntensityProfile InnerObject;
    }
}