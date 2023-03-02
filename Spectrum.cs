using System;

namespace DriftsHelper;

public class Spectrum
{
    private List<Point> InnerObject;

    public Spectrum(string name, int capacity = 4)
    {
        InnerObject = new List<Point>(capacity);
        Name = name;
    }

    public int Count { get => InnerObject.Count; }
    public string Name { get; }

    public Point this[int index]
    {
        get => InnerObject[index];
    }
    public IEnumerable<Point> Points => InnerObject;

    public void Add(double x, double y)
    {
        InnerObject.Add(new Point(x, y));
    }
    public void Sort()
    {
        InnerObject = InnerObject.OrderBy(x => x.X).ToList();
    }
}