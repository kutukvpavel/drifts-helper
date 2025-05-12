namespace DriftsHelper;

public interface IProvider
{
    public string Comment { get; }
    public List<Spectrum> Spectra { get; }
    public int TotalSpectra => Spectra.Count;
    /// <summary>
    /// Provides spectrum number-based access, starts with 1
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Spectrum this[int index]
    {
        get => Spectra[index + 1];
    } 
}