using System;

namespace DAL.Entity
{
    public enum ModelVersionStatus
    {
        Candidate,
        Active,
        Rejected,
        Archived
    }

    public class ModelVersion
    {
        public int Id { get; set; }

        public string ModelName { get; set; } = string.Empty;

        public string Version { get; set; } = string.Empty;

        public string OnnxPath { get; set; } = string.Empty;

        public string? PtPath { get; set; }

        public string? TrainingDatasetPath { get; set; }

        public string? OldDatasetPath { get; set; }

        public string? TrainingRunPath { get; set; }

        public string? ExperimentName { get; set; }

        public double? Precision { get; set; }

        public double? Recall { get; set; }

        public double? Map50 { get; set; }

        public double? Map5095 { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ActivatedAt { get; set; }

        public ModelVersionStatus Status { get; set; } = ModelVersionStatus.Candidate;

        public bool IsActive { get; set; }

        public string? Comment { get; set; }
    }
}
