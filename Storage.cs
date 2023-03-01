using System;
using System.Globalization;
using System.IO;
using CsvHelper;

namespace DriftsHelper;

public static class Storage
{
    public static string FileName {get;set;} = "integrals.csv";

    public static void Store(string folderPath, Result r)
    {
        string p = Path.Combine(folderPath, FileName);
        using TextWriter t = new StreamWriter(p);
        using CsvWriter w = new(t, CultureInfo.InvariantCulture);
        w.WriteField(r.RegionStart);
        w.WriteField(r.RegionStop);
        w.NextRecord();
        w.NextRecord();
        w.WriteRecords(r.Integrals);
    }
}