using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interface
{
    public interface IRolePermissionService
    {
        bool CanManageStaff(string role);
        bool CanManagePatients(string role);
        bool CanOpenMedicalCard(string role);

        bool CanCreateExamination(string role);
        bool CanDeleteExamination(string role);
        bool CanDeletePatient(string role);

        bool CanViewImages(string role);
        bool CanUploadImages(string role);

        bool CanWorkWithAi(string role);
        bool CanReviewAiResult(string role);
        bool CanCreateManualMarkup(string role);

        bool CanWriteRadiologistConclusion(string role);
        bool CanWriteSurgeonConclusion(string role);

        bool CanManageRetraining(string role);
    }
}
