using BLL.Constants;
using BLL.Interface;

namespace BLL.Service
{
    public class RolePermissionService : IRolePermissionService
    {
        public bool CanManageStaff(string role)
            => IsAdmin(role);

        public bool CanManagePatients(string role)
            => IsNurse(role);

        public bool CanOpenMedicalCard(string role)
            => IsNurse(role) || IsRadiologist(role) || IsSurgeon(role);

        public bool CanCreateExamination(string role)
            => IsNurse(role);

        public bool CanDeleteExamination(string role)
            => IsRadiologist(role);

        public bool CanDeletePatient(string role)
            => IsNurse(role);

        public bool CanViewImages(string role)
            => IsNurse(role) || IsRadiologist(role) || IsSurgeon(role);

        public bool CanUploadImages(string role)
            => IsRadiologist(role);

        public bool CanWorkWithAi(string role)
            => IsRadiologist(role);

        public bool CanReviewAiResult(string role)
            => IsRadiologist(role);

        public bool CanCreateManualMarkup(string role)
            => IsRadiologist(role);

        public bool CanWriteRadiologistConclusion(string role)
            => IsRadiologist(role);

        public bool CanWriteSurgeonConclusion(string role)
            => IsSurgeon(role);

        public bool CanManageRetraining(string role)
            => IsAdmin(role);

        private static bool IsAdmin(string role)
            => NormalizeRole(role) == RoleNames.Admin;

        private static bool IsNurse(string role)
            => NormalizeRole(role) == RoleNames.Nurse;

        private static bool IsRadiologist(string role)
            => NormalizeRole(role) == RoleNames.Radiologist;

        private static bool IsSurgeon(string role)
            => NormalizeRole(role) == RoleNames.Surgeon;

        private static string NormalizeRole(string role)
        {
            return role switch
            {
                "Адміністратор" => RoleNames.Admin,
                "Медсестра" => RoleNames.Nurse,
                "Рентгенолог" => RoleNames.Radiologist,
                "Хірург" => RoleNames.Surgeon,
                _ => role
            };
        }
    }
}
