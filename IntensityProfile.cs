namespace DriftsHelper
{
    public class IntensityProfile : Spectrum
    {
        public IntensityProfile(string name, int capacity = 4) : base(name, capacity)
        {
            
        }
        public IntensityProfile(Spectrum s) : base (s.Name, s.Count)
        {
            InnerObject = s.Points;
        }

        /// <summary>
        /// Is meant to return a conservative start index of an experiment step
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns>-1 if not found, 0 if specified time precedes intesnity profile start</returns>
        public int ExperimentTimeToSpectrumIndex(double seconds)
        {
            int index = 1;
            foreach (var item in InnerObject)
            {
                if (item.X >= seconds)
                {
                    return index - 1;
                }
                index++;
            }
            return -1;
        }
    }
}