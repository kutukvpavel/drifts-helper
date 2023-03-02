using System;
using System.Globalization;
using System.IO;
using CsvHelper;

namespace DriftsHelper;

public static class Storage
{
    public static void Store(string folderPath, string fileName, Result[] r)
    {
        string p = Path.Combine(Path.GetFullPath("..", folderPath), fileName);
        using TextWriter t = new StreamWriter(p);
        using CsvWriter w = new(t, CultureInfo.InvariantCulture);
        int len = r[0].Integrals.Count;
        foreach (var item in r)
        {
            w.WriteField(item.Comment);
        }
        w.NextRecord();
        foreach (var item in r)
        {
            w.WriteField(item.RegionStart);
        }
        w.NextRecord();
        foreach (var item in r)
        {
            w.WriteField("-");
        }
        w.NextRecord();
        foreach (var item in r)
        {
            w.WriteField(item.RegionStop);
        }
        w.NextRecord();
        w.NextRecord();
        for (int i = 0; i < len; i++)
        {
            foreach (var item in r)
            {
                w.WriteField(item.Integrals[i]);
            }
            w.NextRecord();
        }
    }
}