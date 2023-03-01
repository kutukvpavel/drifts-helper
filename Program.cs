using System;
using CommandLine;

namespace DriftsHelper // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed((o) => {
                if (o.OutputFileName != null) Storage.FileName = o.OutputFileName;
                
                CsvProvider p = new(o.FolderPath);
                Processing e = new(p);
                Result r = e.IntegrateSpectra(o.RegionStart, o.RegionStop);
                Storage.Store(o.FolderPath, r);
            });
        }
    }

    class Options
    {
        [Option('p', "path", Required = true)]
        public string FolderPath {get;set;}
        [Option('b', "begin", Required = true)]
        public double RegionStart {get;set;}
        [Option('e', "end", Required = true)]
        public double RegionStop {get;set;}
        [Option('o', "out", Required = false, Default = null)]
        public string? OutputFileName {get;set;}
    }
}
