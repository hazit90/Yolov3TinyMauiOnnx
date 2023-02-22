using System;
using System.Reflection;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;

using PointF = SixLabors.ImageSharp.PointF;
using Color = SixLabors.ImageSharp.Color;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace yolov3TinyMauiOnnx.Model
{
	public class YoloOnnxModel
	{

		//InferenceSession session = null;

		string modelName = "tiny-yolov3-11.onnx";
		string modelDirectory = "yolov3TinyMauiOnnx.Resources.Model.";
		InferenceSession session = null;

        private int modelWidth = 416;
        private int modelHeight = 416;

        private int confidenceThreshold = 50;

        private DebugManagement dm = null;
        //Model will be loaded from dir ($ProjectName).Resources.Model
        //if .onnx file as a Build Action of Embedded resources
        // (right click .onnx file -> Build Action -> EmbeddedResource
        public YoloOnnxModel(DebugManagement debugMan ,string name)
		{
            dm = debugMan;
            if (name.Length > 0 && name.EndsWith(".onnx"))
            {
                modelName = name;
            }
            

            var path = modelDirectory + modelName;
            Assembly assembly = GetType().GetTypeInfo().Assembly;
            using var modelStream = assembly.GetManifestResourceStream(path);

            using var modelMemoryStream = new MemoryStream();
            modelStream.CopyTo(modelMemoryStream);
            var _model = modelMemoryStream.ToArray();

            session = new InferenceSession(_model);

            var inputs = session.InputMetadata;
            var outputs = session.OutputMetadata;

            if (session != null)
                debugMan.SetDebugMessage("model loaded");
            else
                debugMan.SetDebugMessage("model not found");

        }

       

        public Image<Rgb24> RunInference(byte[] imageData)
        {
            if (session == null)
            {
                return null;
            }

            //prepocess image
            var slImage = ResizeImage(imageData);
            Tensor<float> rgbTensor = ConvertToRgbTensorArray(slImage);
            Tensor<float> imgSize = new DenseTensor<float>(new[] { 1, 2 });
            imgSize[0, 0] = (float)slImage.Width;
            imgSize[0, 1] = (float)slImage.Height;

            var namedOnnxValues = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_1", rgbTensor),
                NamedOnnxValue.CreateFromTensor("image_shape", imgSize)
            };

            using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(namedOnnxValues);

            var resultsArray = results.ToArray();
            float[] boxes = resultsArray[0].AsEnumerable<float>().ToArray();
            float[] scores = resultsArray[1].AsEnumerable<float>().ToArray();
            int[] indices = resultsArray[2].AsEnumerable<int>().ToArray();

            List<RectangleF> outBoxes = new List<RectangleF>();
            List<float> outScores = new List<float>();
            List<string> outIndex = new List<string>();

            // Loop over the detected objects and draw the bounding boxes on the image
            for (int i = 0; i < indices.Length; i++)
            {
                int index = indices[i];
                float score = scores[i];
                var box = new RectangleF(boxes[index * 4], boxes[index * 4 + 1], boxes[index * 4 + 2] - boxes[index * 4], boxes[index * 4 + 3] - boxes[index * 4 + 1]);

                outBoxes.Add(box);
                outScores.Add(score);
                outIndex.Add(LabelMap.Labels[index]);
            }

            List<RectangleF> nmsBoxes = new List<RectangleF>();
            List<RectangleF> nmsOutScores = new List<RectangleF>();
            List<RectangleF> nmsOutIndicies = new List<RectangleF>();

            //perform non max suppression
            //NMS(outBoxes, outScores, outIndex, nmsBoxes, nmsOutScores, nmsOutIndicies);

            foreach (var box in nmsBoxes)
            {
                slImage.Mutate(ctx =>
                {
                    // Draw the bounding box
                    ctx.DrawPolygon(Color.Red, 3, new PointF[]
                    {
                                new PointF(box.Left, box.Top),
                                new PointF(box.Right, box.Top),
                                new PointF(box.Right, box.Bottom),
                                new PointF(box.Left, box.Bottom),
                    });
                });
            }



            return slImage;
        }

        public void RunInference(bool imageData)
        {
            //if (session == null)
            //{
            //    return null;
            //}

            ////prepocess image
            //var slImage = ResizeImage(imageData);
            //Tensor<float> rgbTensor = ConvertToRgbTensorArray(slImage);
            //Tensor<float> imgSize = new DenseTensor<float>(new[] { 1, 2 });
            //imgSize[0, 0] = (float)slImage.Width;
            //imgSize[0, 1] = (float)slImage.Height;

            //var namedOnnxValues = new List<NamedOnnxValue>
            //{
            //    NamedOnnxValue.CreateFromTensor("input_1", rgbTensor),
            //    NamedOnnxValue.CreateFromTensor("image_shape", imgSize)
            //};

            //using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(namedOnnxValues);

            //var resultsArray = results.ToArray();
            //float[] boxes = resultsArray[0].AsEnumerable<float>().ToArray();
            //float[] scores = resultsArray[1].AsEnumerable<float>().ToArray();
            //int[] indices = resultsArray[2].AsEnumerable<int>().ToArray();



            ////var _assembly = Assembly.GetExecutingAssembly();
            ////var fontStream = _assembly.GetManifestResourceStream("yolov3TinyMauiOnnx.Resources.Fonts.OpenSans-Bold.ttf");
            ////var collection = new SixLabors.Fonts.FontCollection();
            ////FontDescription fontDescription;
            ////collection.Add(fontStream, out fontDescription);
            ////var font = collection.Find("Arial");


            ////SixLabors.Fonts.Font font = new SixLabors.Fonts.Font(
            //// Define the font and color for drawing the class labels
            ////var font = SystemFonts.CreateFont("OpenSans-Bold", 14);
            //////var font = SystemFonts.CreateFont(;

            ////var color = Color.White;

            //// Loop over the detected objects and draw the bounding boxes on the image
            //for (int i = 0; i < indices.Length; i++)
            //{
            //    int index = indices[i];
            //    float score = scores[i];
            //    var box = new RectangleF(boxes[index * 4], boxes[index * 4 + 1], boxes[index * 4 + 2] - boxes[index * 4], boxes[index * 4 + 3] - boxes[index * 4 + 1]);
            //    slImage.Mutate(ctx => {
            //        // Draw the bounding box
            //        ctx.DrawPolygon(Color.Red, 3, new PointF[]
            //        {
            //            new PointF(box.Left, box.Top),
            //            new PointF(box.Right, box.Top),
            //            new PointF(box.Right, box.Bottom),
            //            new PointF(box.Left, box.Bottom),
            //        });


            //        // Draw the class label and score
            //        //var text = $"Object {i + 1}: Score {score:F2}";
            //        //var textSize = TextMeasurer.Measure(text, new RendererOptions(font));
            //        //var textSize = TextMeasurer.Measure(text, new TextOptions(font));

            //        //var textLocation = new PointF(box.Left, box.Top - textSize.Height);
            //        //ctx.DrawText(text, font, color, textLocation);
            //    });
            //}

            //return slImage;
        }

        private Image<Rgb24> ResizeImage(byte[] imageBytes)
        {
            var slImage = SixLabors.ImageSharp.Image.Load<Rgb24>(imageBytes);
            slImage.Mutate(x => x.Resize(modelWidth, modelHeight));

            dm.SetDebugMessage("image resized");

            return slImage;
        }

        private Tensor<float> ConvertToRgbTensorArray(Image<Rgb24> image)
        {

            Tensor<float> retVal = new DenseTensor<float>(new[] { 1, 3, modelWidth, modelHeight });

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    Span<Rgb24> pixelSpan = accessor.GetRowSpan(y);
                    for (int x = 0; x < accessor.Width; x++)
                    {
                        retVal[0, 0, y, x] = pixelSpan[x].B;
                        retVal[0, 1, y, x] = pixelSpan[x].G;
                        retVal[0, 2, y, x] = pixelSpan[x].R;
                    }
                }
            });

            

            dm.SetDebugMessage("RGB Tensor Array Created");
            return retVal;
        }

        
    }
}

