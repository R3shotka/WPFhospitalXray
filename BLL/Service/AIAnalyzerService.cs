using BLL.DTOs.FractureDetections;
using BLL.Interface;
using Compunet.YoloV8;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Service
{
    public class AIAnalyzerService : IAIAnalyzerService
    {
        private const int ModelInputSize = 768;
        private readonly string[] _classes = { "fracture" };

        private readonly IModelVersionService _modelVersionService;

        public AIAnalyzerService(IModelVersionService modelVersionService)
        {
            _modelVersionService = modelVersionService;
        }

        public string ModelName { get; private set; } = "YOLOv8 fracture detector";

        public string ModelVersion { get; private set; } = "unknown";

        public string ModelPath { get; private set; } = string.Empty;

        public async Task<List<FractureDetectionDto>> AnalyzeImageAsync(string imagePath)
        {
            var activeModel = await _modelVersionService.GetActiveOrCreateDefaultAsync();

            ModelName = activeModel.ModelName;
            ModelVersion = activeModel.Version;
            ModelPath = activeModel.OnnxPath;

            return await Task.Run(() => RunInference(imagePath, activeModel.OnnxPath));
        }

        private List<FractureDetectionDto> RunInference(string imagePath, string modelPath)
        {
            if (!File.Exists(modelPath))
            {
                throw new FileNotFoundException("Файл AI-моделі не знайдено.", modelPath);
            }

            using var session = new InferenceSession(modelPath);

            using var image = new Bitmap(imagePath);
            int originalWidth = image.Width;
            int originalHeight = image.Height;

            float scale = Math.Min((float)ModelInputSize / originalWidth, (float)ModelInputSize / originalHeight);
            int newWidth = (int)(originalWidth * scale);
            int newHeight = (int)(originalHeight * scale);
            int xPad = (ModelInputSize - newWidth) / 2;
            int yPad = (ModelInputSize - newHeight) / 2;

            using var paddedImage = new Bitmap(ModelInputSize, ModelInputSize);
            using (var g = Graphics.FromImage(paddedImage))
            {
                g.Clear(Color.Black);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(image, xPad, yPad, newWidth, newHeight);
            }

            var tensor = new DenseTensor<float>(new[] { 1, 3, ModelInputSize, ModelInputSize });

            for (int y = 0; y < ModelInputSize; y++)
            {
                for (int x = 0; x < ModelInputSize; x++)
                {
                    var pixel = paddedImage.GetPixel(x, y);

                    tensor[0, 0, y, x] = pixel.R / 255.0f;
                    tensor[0, 1, y, x] = pixel.G / 255.0f;
                    tensor[0, 2, y, x] = pixel.B / 255.0f;
                }
            }

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("images", tensor)
            };

            using var results = session.Run(inputs);

            var outputTensor = results.FirstOrDefault()?.AsTensor<float>();
            if (outputTensor == null)
            {
                return new List<FractureDetectionDto>();
            }

            var rawDetections = new List<FractureDetectionDto>();
            int boxCount = outputTensor.Dimensions[2];

            for (int i = 0; i < boxCount; i++)
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

                    float xCenter = outputTensor[0, 0, i];
                    float yCenter = outputTensor[0, 1, i];
                    float width = outputTensor[0, 2, i];
                    float height = outputTensor[0, 3, i];

                    int x = (int)((xCenter - (width / 2f) - xPad) / scale);
                    int y = (int)((yCenter - (height / 2f) - yPad) / scale);
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

            var finalDetections = new List<FractureDetectionDto>();
            var sortedDetections = rawDetections
                .OrderByDescending(d => d.Confidence)
                .ToList();

            foreach (var currentBox in sortedDetections)
            {
                if (currentBox.Confidence < 0.25f)
                {
                    continue;
                }

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

        private float CalculateIoU(FractureDetectionDto boxA, FractureDetectionDto boxB)
        {
            int x1 = Math.Max(boxA.X, boxB.X);
            int y1 = Math.Max(boxA.Y, boxB.Y);
            int x2 = Math.Min(boxA.X + boxA.Width, boxB.X + boxB.Width);
            int y2 = Math.Min(boxA.Y + boxA.Height, boxB.Y + boxB.Height);

            int intersectionWidth = Math.Max(0, x2 - x1);
            int intersectionHeight = Math.Max(0, y2 - y1);

            if (intersectionWidth <= 0 || intersectionHeight <= 0)
            {
                return 0f;
            }

            float areaIntersection = intersectionWidth * intersectionHeight;
            float areaA = boxA.Width * boxA.Height;
            float areaB = boxB.Width * boxB.Height;

            return areaIntersection / (areaA + areaB - areaIntersection);
        }
    }
}
