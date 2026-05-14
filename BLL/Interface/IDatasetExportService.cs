using BLL.DTOs.Datasets;

namespace BLL.Interface
{
    public interface IDatasetExportService
    {
        Task<DatasetExportResultDto> ExportApprovedRequestsAsync();
    }
}