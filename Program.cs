using System;
using System.Collections;
using System.Globalization;
using System.Text;
using CommandLine;

namespace DriftsHelper // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        const string EndLiteral = "end";

        static string CheckSymbolicIndex(Options o, string splt, int lastSpectrumIndex, Timeline? t, string? startIndex = null)
        {
            if (splt == EndLiteral)
            {
                return lastSpectrumIndex.ToString();
            }
            int? step = t?.GetExternalStepByName(splt, startIndex)?.ScanIndex;
            if (step.HasValue)
            {
                step -= o.TimelineConservative;
                if (step < 1) step = 1;
            }
            return step?.ToString() ?? splt;
        }

        static void ProcessFolder(Options o, string spectraFolderPath, string configFolderPath, string fileName)
        {
            spectraFolderPath = Path.GetFullPath(spectraFolderPath);
            Console.WriteLine($"Scanning input dir {spectraFolderPath}");
            CsvProvider p = new(spectraFolderPath);
            Timeline? t = null;
            if (o.UseTimelineProviders)
            {
                var prf = new PrfTimingProvider(configFolderPath, o.SecondsPerSpectrum);
                List<IExternalTimelineProvider?> externalProviders = new(3)
                {
                    TempProfileProvider.TryCreate(configFolderPath, o.TemperatureProfileOffset),
                    UVProfileProvider.TryCreate(configFolderPath, o.UvProfileOffset),
                    GasProfileProvider.TryCreate(configFolderPath, o.GasProfileOffset)
                };
                t = new Timeline(prf, externalProviders.Where(x => x != null).Cast<IExternalTimelineProvider>().ToArray());
                Console.WriteLine("Found the following externally-provided steps:");
                StringBuilder timelineDescriptionBuilder = new();
                foreach (var item in t.ExternalSteps)
                {
                    timelineDescriptionBuilder.AppendLine(item.ToString());
                }
                string timelineDescription = timelineDescriptionBuilder.ToString();
                Console.Write(timelineDescription);
                File.WriteAllText(Path.Combine(configFolderPath, "timeline.txt"), timelineDescription);
            }

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
                        string startIndex = spltIndexes[0];
                        spltIndexes[0] = CheckSymbolicIndex(o, spltIndexes[0], e.LastSpectrumIndex, t);
                        spltIndexes[1] = CheckSymbolicIndex(o, spltIndexes[1], e.LastSpectrumIndex, t, startIndex);
                        diffSpectra.Add(e.SubtractSpectra(int.Parse(spltIndexes[0]), int.Parse(spltIndexes[1]), spltNameIndexes[0]));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Subtraction failed: {ex}");
                    }
                }
            }

            try
            {
                if (results != null) 
                {
                    Console.WriteLine("Writing integration output file...");
                    Storage.StoreIntegralCurves(spectraFolderPath, fileName, results);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            try
            {
                if (diffSpectra != null)
                {
                    Console.WriteLine("Writing subtraction output file...");
                    Storage.StoreDiffSpectra(spectraFolderPath, fileName, diffSpectra);
                }
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
                    Console.WriteLine($"Scanning parent folder '{o.ParentFolder}'...");
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
                            ProcessFolder(o, p, item, o.OutputFileName ?? $"{new DirectoryInfo(item).Name}.csv");
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

        [Option('t', "timeline", Required = false, Default = false)]
        public bool UseTimelineProviders {get;set;}
        [Option('s', "speed", Required = false, Default = 158.3417665)]
        public double SecondsPerSpectrum {get;set;} //Default is for 30 accumulations per spectrum (~2.6 min/spectrum)
        [Option('u', "uv-offset", Required = false, Default = 0)]
        public double UvProfileOffset {get;set;}
        [Option('g', "gas-offset", Required = false, Default = 0)]
        public double GasProfileOffset {get;set;}
        [Option("temp-offset", Required = false, Default = 0)]
        public double TemperatureProfileOffset {get;set;}
        [Option('c', "conservative", Required = false, Default = 0)]
        public int TimelineConservative {get;set;}
    }
}
