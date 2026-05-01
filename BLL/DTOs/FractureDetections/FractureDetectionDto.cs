using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.DTOs.FractureDetections
{
    public class FractureDetectionDto
    {
        public string ClassName { get; set; }   // Назва (наприклад, "fracture")
        public float Confidence { get; set; }   // Впевненість (наприклад, 0.45f)

        // Координати для малювання квадрата
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
