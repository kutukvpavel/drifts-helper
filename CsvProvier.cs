using System;
using System.Globalization;
using System.IO;
using CsvHelper;

namespace DriftsHelper;

public class CsvProvider : IProvider
{
    private static FileStreamOptions Options {get;} = new FileStreamOptions() {
        Access = FileAccess.Read
    };

    public static string Filter { get; set; } = "*.csv";

    public CsvProvider(string folderPath)
    {
        Comment = folderPath;
        Spectra = new List<Spectrum>();
        foreach (var item in Directory.EnumerateFiles(folderPath, Filter).OrderBy(x => x))
        {
            using TextReader t = new StreamReader(item, Options);
            using CsvReader r = new(t, CultureInfo.InvariantCulture);
            Spectrum s = new();
            r.Read();
            r.ReadHeader();
            while (r.Read())
            {
                s.Add(r.GetField<double>(0), r.GetField<double>(1));
            }
            Spectra.Add(s);
        }
    }

    public List<Spectrum> Spectra { get; }

    public string Comment { get; }
}