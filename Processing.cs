using System;

namespace DriftsHelper;

public class Processing
{
    public static double SubtractXTolerance {get;set;} = 0.1;
    public static int BaselineMarkerAveraging { get; set; } = 15;
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
    public static double FindPeakValue(Spectrum s, double start, double end)
    {
        double peak = double.MinValue;
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
            if (item.Y > peak) peak = item.Y;
        }
        baseline /= 2 * BaselineMarkerAveraging;
        peak -= baseline;
        return peak;
    }

    //PRIVATE
    private readonly IProvider _Provider;

    //PUBLIC
    public int LastSpectrumIndex => _Provider.TotalSpectra - 1;

    public Processing(IProvider p)
    {
        _Provider = p;
        foreach (var item in p.Spectra)
        {
            item.Sort();
        }
    }
    public IntegrationResult IntegrateSpectra(double start, double end)
    {
        var res = new List<double>(_Provider.Spectra.Count);
        foreach (var item in _Provider.Spectra)
        {
            res.Add(IntegrateRegion(item, start, end));
        }
        return new IntegrationResult(_Provider.Comment, start, end, res);
    }
    public IntegrationResult PeakSpectra(double start, double end)
    {
        var res = new List<double>(_Provider.Spectra.Count);
        foreach (var item in _Provider.Spectra)
        {
            res.Add(FindPeakValue(item, start, end));
        }
        return new IntegrationResult(_Provider.Comment, start, end, res);
    }
    public Spectrum SubtractSpectra(int index, int from, string name)
    {
        try
        {
            var sub = _Provider[index];
            var bas = _Provider[from];
            int len = sub.Count;
            if (len != bas.Count) throw new NotSupportedException("Variable length spectra not supported yet");
            Spectrum res = new(name, len);
            for (int i = 0; i < len; i++)
            {
                if (Math.Abs(sub[i].X - bas[i].X) > SubtractXTolerance)
                {
                    Console.WriteLine("Warning: X axes do not match for specified spectra!");
                }
                res.Add(sub[i].X, bas[i].Y - sub[i].Y);
            }
            return res;
        }
        catch (IndexOutOfRangeException)
        {
            Console.WriteLine($"ERROR: problem with spectra pair {name} = ({index},{from})");
            throw;
        }
    }
}