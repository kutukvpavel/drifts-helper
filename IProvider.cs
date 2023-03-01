using System;
using CsvHelper;

namespace DriftsHelper;

public interface IProvider
{
    public string Comment { get; }
    public List<Spectrum> Spectra { get; }
}