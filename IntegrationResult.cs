using System;

namespace DriftsHelper;

public class IntegrationResult
{
    public IntegrationResult(string comment, double start, double stop, List<double> integrals)
    {
        Comment = comment;
        RegionStart = start;
        RegionStop = stop;
        Integrals = integrals;
    }

    public string Comment { get; }
    public double RegionStart { get; }
    public double RegionStop { get; }
    public List<double> Integrals { get; }
}