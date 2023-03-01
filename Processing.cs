using System;

namespace DriftsHelper;

public class Processing
{
    public static int BaselineMarkerAveraging { get; set; } = 3;
    public static double IntegrateRegion(Spectrum s, double start, double end)
    {
        double integral = 0;
        double baseline = 0;
        int i = 0;
        for (; i < s.Count; i++)
        {
            if (s[i].X < start) continue;
            for (int j = i - BaselineMarkerAveraging; j < i; j++)
            {
                baseline += s[j].Y;
            }
            break;
        }
        if (i == s.Count) throw new ArgumentOutOfRangeException(nameof(start), "Requested region is outside of spectrum bounds");
        for (; i < s.Count; i++)
        {
            var item = s[i];
            if (item.X >= end)
            {
                for (int j = i; j < i + BaselineMarkerAveraging; j++)
                {
                    baseline += s[j].Y;
                }
                break;
            }
            integral += item.Y * (item.X - s[i - 1].X);
        }
        baseline /= 2 * BaselineMarkerAveraging;
        baseline *= end - start;
        integral -= baseline;
        return integral;
    }

    //PRIVATE
    private readonly IProvider _Provider;

    //PUBLIC
    public Processing(IProvider p)
    {
        _Provider = p;
    }
    public Result IntegrateSpectra(double start, double end)
    {
        var res = new List<double>(_Provider.Spectra.Count);
        foreach (var item in _Provider.Spectra)
        {
            res.Add(IntegrateRegion(item, start, end));
        }
        return new Result(_Provider.Comment, start, end, res);
    }
}