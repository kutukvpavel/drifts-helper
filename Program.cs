using System;
using System.Collections;
using System.Globalization;
using CommandLine;

namespace DriftsHelper // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void ProcessFolder(Options o, string folderPath, string fileName)
        {
            Console.WriteLine($"Scanning input dir {folderPath}");
            CsvProvider p = new(folderPath);

            Console.WriteLine("Running preprocessor...");
            Processing e = new(p);

            List<IntegrationResult>? results = null;
            if (o.Regions != null && o.Regions.Any())
            {
                Console.WriteLine("Integrating...");
                results = new();
                foreach (var item in o.Regions)
                {
                    try
                    {
                        var splt = item.Split(',');
                        var reg = new Region(splt[0], splt[1]);
                        Console.Write("Method: ");
                        Console.WriteLine(o.PeakInsteadOfIntegrate ? "Peak" : "Integrate");
                        results.Add(o.PeakInsteadOfIntegrate ? 
                            e.PeakSpectra(reg.Start, reg.Stop) : 
                            e.IntegrateSpectra(reg.Start, reg.Stop));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }

            List<Spectrum>? diffSpectra = null;
            if (o.DifferenceSpectraPairs != null && o.DifferenceSpectraPairs.Any())
            {
                Console.WriteLine("Subtracting...");
                diffSpectra = new List<Spectrum>();
                foreach (var item in o.DifferenceSpectraPairs)
                {
                    try
                    {
                        var spltNameIndexes = item.Split('=');
                        var spltIndexes = spltNameIndexes[1].Split(',');
                        if (spltIndexes.Length < 2) throw new ArgumentException($"Warning: malformed diff spectra argument '{item}'!");
                        diffSpectra.Add(e.SubtractSpectra(int.Parse(spltIndexes[0]), int.Parse(spltIndexes[1]), spltNameIndexes[0]));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }

            Console.WriteLine("Writing output file...");
            try
            {
                if (results != null) Storage.StoreIntegralCurves(folderPath, fileName, results);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            try
            {
                if (diffSpectra != null) Storage.StoreDiffSpectra(folderPath, fileName, diffSpectra);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed((o) => {

                if (o.ParentFolder != null)
                {
                    o.FolderPath = Directory.GetDirectories(o.ParentFolder);
                }
                
                if (o.FolderPath != null)
                {
                    foreach (var item in o.FolderPath)
                    {
                        const string checkForInnerFolder = "spectra";

                        try
                        {
                            string p;
                            var subdirs = Directory.EnumerateDirectories(item);
                            if (subdirs.Any(x => x.EndsWith(checkForInnerFolder)))
                            {
                                Console.WriteLine("Folder was found to contain a subfolder 'spectra', assuming nested folders.");
                                p = Path.Combine(item, checkForInnerFolder);
                            }
                            else
                            {
                                p = item;
                            }
                            ProcessFolder(o, p, o.OutputFileName ?? $"{new DirectoryInfo(item).Name}.csv");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                }

                Console.WriteLine("Done.");
            });
        }
    }

    class Region
    {
        public Region(string start, string stop) : 
            this(double.Parse(start, CultureInfo.InvariantCulture), double.Parse(stop, CultureInfo.InvariantCulture))
        {

        }
        public Region(double start, double stop)
        {
            Start = start;
            Stop = stop;
        }

        public double Start {get;}
        public double Stop {get;}
    }

    class Options
    {
        [Option('p', "parent", Required = false)]
        public string? ParentFolder {get;set;}
        [Option('f', "folder", Required = false)]
        public IEnumerable<string>? FolderPath {get;set;}
        [Option('r', "regions", Required = false, Default = null)]
        public IEnumerable<string>? Regions {get;set;}
        [Option('o', "output", Required = false)]
        public string? OutputFileName {get;set;}
        [Option('m', "method")]
        public bool PeakInsteadOfIntegrate {get;set;}
        [Option('d', "diff", Required = false, Default = null)]
        public IEnumerable<string>? DifferenceSpectraPairs {get;set;}
    }
}
