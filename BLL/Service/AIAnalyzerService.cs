using BLL.DTOs.FractureDetections;
using BLL.Interface;
using Compunet.YoloV8;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Service
{
    public class AIAnalyzerService : IAIAnalyzerService
    {
        // Вкажи тут правильний шлях до моделі (можливо, доведеться прописати абсолютний шлях
        // або шлях відносно папки запуску програми)
        private readonly string _modelPath = @"Models\best1.onnx";

        private const int _modelInputSize = 768;
        private readonly string[] _classes = new string[] { "fracture" };

        public async Task<List<FractureDetectionDto>> AnalyzeImageAsync(string imagePath)
        {
            // Виконуємо важку математику у фоновому потоці, щоб не підвисав інтерфейс
            return await Task.Run(() => RunInference(imagePath));
        }

        private List<FractureDetectionDto> RunInference(string imagePath)
        {
            using var session = new InferenceSession(_modelPath);

            using var image = new System.Drawing.Bitmap(imagePath);
            int originalWidth = image.Width;
            int originalHeight = image.Height;

            // Letterbox ресайз (чорні рамки)
            float scale = Math.Min((float)_modelInputSize / originalWidth, (float)_modelInputSize / originalHeight);
            int newWidth = (int)(originalWidth * scale);
            int newHeight = (int)(originalHeight * scale);
            int xPad = (_modelInputSize - newWidth) / 2;
            int yPad = (_modelInputSize - newHeight) / 2;

            using var paddedImage = new System.Drawing.Bitmap(_modelInputSize, _modelInputSize);
            using (var g = System.Drawing.Graphics.FromImage(paddedImage))
            {
                g.Clear(System.Drawing.Color.Black);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(image, xPad, yPad, newWidth, newHeight);
            }

            // Конвертація в тензор
            var tensor = new DenseTensor<float>(new[] { 1, 3, _modelInputSize, _modelInputSize });
            for (int y = 0; y < _modelInputSize; y++)
            {
                for (int x = 0; x < _modelInputSize; x++)
                {
                    var pixel = paddedImage.GetPixel(x, y);
                    tensor[0, 0, y, x] = pixel.R / 255.0f;
                    tensor[0, 1, y, x] = pixel.G / 255.0f;
                    tensor[0, 2, y, x] = pixel.B / 255.0f;
                }
            }

            // Інференс
            var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("images", tensor) };
            using var results = session.Run(inputs);

            var outputTensor = results.FirstOrDefault()?.AsTensor<float>();
            if (outputTensor == null) return new List<FractureDetectionDto>();

            var rawDetections = new List<FractureDetectionDto>();
            int box_count = outputTensor.Dimensions[2];

            // Збираємо сирі результати
            for (int i = 0; i < box_count; i++)
            {
                float confidence = outputTensor[0, 4, i];

                if (confidence >= 0.05f)
                {
                    float maxClassScore = 0f;
                    int maxClassId = 0;
                    for (int c = 0; c < _classes.Length; c++)
                    {
                        float score = outputTensor[0, 4 + c, i];
                        if (score > maxClassScore)
                        {
                            maxClassScore = score;
                            maxClassId = c;
                        }
                    }

                    float x_center = outputTensor[0, 0, i];
                    float y_center = outputTensor[0, 1, i];
                    float width = outputTensor[0, 2, i];
                    float height = outputTensor[0, 3, i];

                    int x = (int)((x_center - (width / 2f) - xPad) / scale);
                    int y = (int)((y_center - (height / 2f) - yPad) / scale);
                    int w = (int)(width / scale);
                    int h = (int)(height / scale);

                    if (maxClassId < _classes.Length && w > 0 && h > 0)
                    {
                        rawDetections.Add(new FractureDetectionDto
                        {
                            ClassName = _classes[maxClassId],
                            Confidence = confidence,
                            X = x,
                            Y = y,
                            Width = w,
                            Height = h
                        });
                    }
                }
            }

            // ==========================================
            // NMS (Видалення дублікатів)
            // ==========================================
            var finalDetections = new List<FractureDetectionDto>();
            var sortedDetections = rawDetections.OrderByDescending(d => d.Confidence).ToList();

            foreach (var currentBox in sortedDetections)
            {
                if (currentBox.Confidence < 0.25f) continue; // Поріг впевненості 25%

                bool shouldKeep = true;

                foreach (var keptBox in finalDetections)
                {
                    if (CalculateIoU(currentBox, keptBox) > 0.45f)
                    {
                        shouldKeep = false;
                        break;
                    }
                }

                if (shouldKeep)
                {
                    finalDetections.Add(currentBox);
                }
            }

            return finalDetections;
        }

        // Математика для NMS переписана під наші чисті DTO (без використання WPF Rect)
        private float CalculateIoU(FractureDetectionDto boxA, FractureDetectionDto boxB)
        {
            int x1 = Math.Max(boxA.X, boxB.X);
            int y1 = Math.Max(boxA.Y, boxB.Y);
            int x2 = Math.Min(boxA.X + boxA.Width, boxB.X + boxB.Width);
            int y2 = Math.Min(boxA.Y + boxA.Height, boxB.Y + boxB.Height);

            int intersectionWidth = Math.Max(0, x2 - x1);
            int intersectionHeight = Math.Max(0, y2 - y1);

            if (intersectionWidth <= 0 || intersectionHeight <= 0) return 0f;

            float areaIntersection = intersectionWidth * intersectionHeight;
            float areaA = boxA.Width * boxA.Height;
            float areaB = boxB.Width * boxB.Height;

            return areaIntersection / (areaA + areaB - areaIntersection);
        }
    }
}
