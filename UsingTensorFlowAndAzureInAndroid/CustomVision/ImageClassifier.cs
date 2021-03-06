using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.App;
using Android.Graphics;
using Org.Tensorflow.Contrib.Android;

namespace CustomVision
{
    public class ImageClassifier
    {
        readonly List<string> labels;
        readonly TensorFlowInferenceInterface inferenceInterface;

        public ImageClassifier()
        {
            var assets = Application.Context.Assets;
			inferenceInterface = new TensorFlowInferenceInterface(assets, "model.pb");

            using (var sr = new StreamReader(assets.Open("labels.txt")))
            {
                var content = sr.ReadToEnd();
                labels = content.Split('\n').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
            }

        }

        static readonly int InputSize = 227;
        static readonly string InputName = "Placeholder";
        static readonly string OutputName = "loss";

        public string RecognizeImage(Bitmap bitmap)
        {
            var outputNames = new[] { OutputName };
            var floatValues = GetBitmapPixels(bitmap);
            var outputs = new float[labels.Count];

            inferenceInterface.Feed(InputName, floatValues, 1, InputSize, InputSize, 3);
            inferenceInterface.Run(outputNames);
            inferenceInterface.Fetch(OutputName, outputs);

            var results = new List<Tuple<float, string>>();
            for (var i = 0; i < outputs.Length; ++i)
                results.Add(Tuple.Create(outputs[i], labels[i]));

            return results.OrderByDescending(t => t.Item1).First().Item2;
        }

        static float[] GetBitmapPixels(Bitmap bitmap)
        {
            var floatValues = new float[227 * 227 * 3];

            using (var scaledBitmap = Bitmap.CreateScaledBitmap(bitmap, 227, 227, false))
            {
                using (var resizedBitmap = scaledBitmap.Copy(Bitmap.Config.Argb8888, false))
                {
                    var intValues = new int[227 * 227];
                    resizedBitmap.GetPixels(intValues, 0, resizedBitmap.Width, 0, 0, resizedBitmap.Width, resizedBitmap.Height);

                    for (int i = 0; i < intValues.Length; ++i)
                    {
                        var val = intValues[i];

                        floatValues[i * 3 + 0] = ((val & 0xFF) - 104);
                        floatValues[i * 3 + 1] = (((val >> 8) & 0xFF) - 117);
                        floatValues[i * 3 + 2] = (((val >> 16) & 0xFF) - 123);
                    }

                    resizedBitmap.Recycle();
                }

                scaledBitmap.Recycle();
            }

            return floatValues;
        }
    }
}
