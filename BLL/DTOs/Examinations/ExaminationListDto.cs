using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.Examinations
{
    public class ExaminationListDto
    {
        public int Id { get; set; }

        public string ExaminationDate { get; set; }

        public int ImagesCount { get; set; }

        public string ImagesInfo => ImagesCount == 0
            ? "Знімків немає"
            : $"Знімків: {ImagesCount}";

        public string RadiologistConclusion { get; set; }

        public string SurgeonConclusion { get; set; }
    }
}
