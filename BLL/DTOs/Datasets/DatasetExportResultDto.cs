using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.Datasets
{
    public class DatasetExportResultDto
    {
        public string DatasetPath { get; set; } = string.Empty;

        public int TotalRequests { get; set; }

        public int ExportedItems { get; set; }

        public int SkippedItems { get; set; }

        public List<string> Warnings { get; set; } = new();
    }
}
