using System;

namespace DriftsHelper;

public class Result
{
    public Result(string path, double start, double stop, List<double> integrals)
    {
        FolderPath = path;
        RegionStart = start;
        RegionStop = stop;
        Integrals = integrals;
    }

    public string FolderPath { get; }
    public double RegionStart { get; }
    public double RegionStop { get; }
    public List<double> Integrals { get; }
}