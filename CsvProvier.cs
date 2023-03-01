using System;
using System.Globalization;
using System.IO;
using CsvHelper;

namespace DriftsHelper;

public class CsvProvider : IProvider
{
    public static string Filter { get; set; } = "*.csv";

    public CsvProvider(string folderPath)
    {
        Comment = Path.GetDirectoryName(folderPath) ?? "null";
        Spectra = new List<Spectrum>();
        foreach (var item in Directory.EnumerateFiles(folderPath, Filter).OrderBy(x => x))
        {
            using TextReader t = new StreamReader(item);
            using CsvReader r = new(t, CultureInfo.InvariantCulture);
            Spectrum s = new();
            r.Read();
            r.ReadHeader();
            while (r.Read())
            {
                s.Add(new Point(r.GetField<double>(0), r.GetField<double>(1)));
            }
            Spectra.Add(s);
        }
    }

    public List<Spectrum> Spectra { get; }

    public string Comment { get; }
}