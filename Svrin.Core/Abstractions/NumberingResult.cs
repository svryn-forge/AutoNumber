namespace Svrin.Core.Abstractions
{
    public class NumberingResult
    {
        public int Total { get; set; }
        public int Success { get; set; }
        public int Skipped { get; set; }

        public List<string> Details { get; } = new List<string>();
    }
}
