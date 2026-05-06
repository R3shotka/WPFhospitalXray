using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.Auth
{
    public class AuthResultDto
    {
        public bool Success { get; set; }
        public string? UserId { get; set; }
        public string? Role { get; set; }
        public string? FullName { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
