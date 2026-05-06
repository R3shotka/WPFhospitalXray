namespace DAL.Entity
{
    public class DetectionBox
    {
        public int Id { get; set; }

        public int AnalysisResultId { get; set; }
        public AnalysisResult AnalysisResult { get; set; }

        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public float Confidence { get; set; }

        public string ClassName { get; set; } = string.Empty;

        public bool IsManuallyCorrected { get; set; }

        public string Source { get; set; } = "AI";
    }
}