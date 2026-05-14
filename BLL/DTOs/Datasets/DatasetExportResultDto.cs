using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.Datasets
{
    public class DatasetExportResultDto
    {
        public bool IsSuccess { get; set; }

        public bool IsTrainingReady { get; set; }

        public string DatasetPath { get; set; } = string.Empty;

        public string SummaryPath { get; set; } = string.Empty;

        public int TotalRequests { get; set; }

        public int ValidItems { get; set; }

        public int ExportedItems { get; set; }

        public int SkippedItems { get; set; }

        public int TrainItems { get; set; }

        public int ValidationItems { get; set; }

        public int FalsePositiveItems { get; set; }

        public int FalseNegativeItems { get; set; }

        public int CorrectedPositiveItems { get; set; }

        public double BackgroundRatio { get; set; }

        public List<string> Warnings { get; set; } = new();
    }
}
