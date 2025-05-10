using System;
using System.Globalization;
using System.IO;
using CsvHelper;

namespace DriftsHelper;

public static class Storage
{
    private static string GetSavePath(string folderPath, string fileName, string? modifier = null)
    {
        if (modifier != null)
        {
            string n = Path.GetFileNameWithoutExtension(fileName);
            string e = Path.GetExtension(fileName);
            fileName = $"{n}_{modifier}{e}";
        }
        return Path.Combine(Path.GetFullPath("..", folderPath), fileName);
    }

    public static void StoreDiffSpectra(string folderPath, string fileName, List<Spectrum> s)
    {
        string p = GetSavePath(folderPath, fileName, "diff");
        using TextWriter t = new StreamWriter(p);
        using CsvWriter w = new(t, CultureInfo.InvariantCulture);
        int length = s.Max(x => x.Count);
        w.WriteField("X");
        foreach (var item in s)
        {
            w.WriteField(item.Name);
        }
        w.NextRecord();
        for (int i = 0; i < length; i++)
        {
            w.WriteField(s[0][i].X);
            foreach (var item in s)
            {
                w.WriteField(item[i].Y);
            }
            w.NextRecord();
        }
    }
    public static void StoreIntegralCurves(string folderPath, string fileName, List<IntegrationResult> r)
    {
        string p = GetSavePath(folderPath, fileName, "int");
        using TextWriter t = new StreamWriter(p);
        using CsvWriter w = new(t, CultureInfo.InvariantCulture);
        int len = r[0].Integrals.Count;
        w.WriteField(r[0].Comment);
        w.NextRecord();
        foreach (var item in r)
        {
            w.WriteField($"{item.RegionStart}-{item.RegionStop}");
        }
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