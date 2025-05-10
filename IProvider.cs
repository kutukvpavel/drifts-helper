namespace DriftsHelper;

public interface IProvider
{
    public string Comment { get; }
    public List<Spectrum> Spectra { get; }
    public int TotalSpectra => Spectra.Count;
}