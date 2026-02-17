namespace DriftsHelper
{
    public class Selector
    {
        public struct SelectionResult
        {
            public string Name;
            public int StartSpectrumIndex;
            public int EndSpectrumIndex;
        }
        public class SelectionRule : Tuple<string, string>
        {
            public SelectionRule(string name, string item1, string item2) : base(item1, item2)
            {
                Name = name;
            }

            protected IExternalStep? Matched1;

            public string Name { get; }

            public bool Match1(IExternalStep step)
            {
                if (step.Name.Equals(Item1, StringComparison.InvariantCultureIgnoreCase))
                {
                    Matched1 = step;
                    return true;
                }
                return false;
            }
            public bool Match2(IExternalStep step)
            {
                if (Matched1 == null) throw new InvalidOperationException("The first step has to be matched first!");
                if (step.ScanIndex <= Matched1.ScanIndex) return false;
                if (step.Name.Equals(Item2, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
                return false;
            }
        }

        public const string DefaultFilter = "*.r";

        public Selector(Timeline t, string folderPath, string filter = DefaultFilter)
        {
            InnerObject = t;
            var files = Directory.GetFiles(folderPath, filter);
            if (files.Length != 1) throw new InvalidOperationException($"Unable to find suitable selection rule file!");
            using TextReader tr = new StreamReader(files[0], CsvProvider.Options);
            string? line;
            while ((line = tr.ReadLine()) != null)
            {
                string[] splt = line.Split('=');
                string[] pair = splt[1].Split('-');
                Instructions.Add(new SelectionRule(splt[0], pair[1], pair[0]));
            }
            foreach (var item in Instructions)
            {
                int startStep = -1;
                int endStep = -1;
                int i;
                for (i = 0; i < t.ExternalSteps.Count; i++)
                {
                    var step = t.ExternalSteps[i];
                    if (item.Match1(step))
                    {
                        startStep = step.ScanIndex;
                        break;
                    }
                }
                for (; i < t.ExternalSteps.Count; i++)
                {
                    var step = t.ExternalSteps[i];
                    if (item.Match2(step))
                    {
                        endStep = step.ScanIndex;
                        break;
                    }
                }
                if (startStep < 0 || endStep < 0) throw new KeyNotFoundException("Selector was unable to find specified steps!");
                SelectedSteps.Add(new SelectionResult() {
                    Name = item.Name,
                    StartSpectrumIndex = startStep,
                    EndSpectrumIndex = endStep
                });
            }
        }

        protected Timeline InnerObject;
        protected List<SelectionRule> Instructions = new();

        public List<SelectionResult> SelectedSteps { get; } = new();
    }
}