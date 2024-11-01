
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Reg;
using FaceParser;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using UAI.Common;
using UAI.ConsoleApp;

namespace UAI.Common.AI
{
    public class OnnxProcessor: UAIComponent
{
        public ONNXProcessorConfig config;  // Path to the ONNX model

        public InferenceSession _session;
        public virtual string categoryName { get { return config != null ? config.metadata.path : null; } }
        public virtual string inputName { get { return config != null ? config.onnxMetaData.Inputs[0].name : null; } }
        public virtual string outputName { get { return config != null ? config.onnxMetaData != null ? config.onnxMetaData.Outputs[0].name : null : null; } }
        public virtual string[] labels { get; set; }

        public virtual string onnxModelPath
        {
            get
            {

                if (config == null)
                {
                    return null;
                }
                if (config.onnxModelPath == null)
                {
                    return null;
                }
                return config.onnxModelPath;

            }
            set
            {

                if (config == null)
                {
                    config = new ONNXProcessorConfig();
                }
                config.onnxModelPath = value;

            }
        }
        public Bitmap inputTexture;

        public string[] inputFileExtensions = new string[] { "*.png", "*.jpg" };
        public Vector2I inputImageSize = new Vector2I(1024, 1024);  // The size of the input image to the ONNX model

        public bool isSelected = false;

        public Bitmap resultTexture;
        public SelectState selected = SelectState.Unselected;


        public string id = "";

        public string userModelPath = "";

        public string userAbsoluteModelPath { get { return ProjectSettings.GlobalizePath(userModelPath); } }
        public string onnxAbsoluteModelPath { get { return ProjectSettings.GlobalizePath(onnxModelPath); } }
        public string modelBaseName { get { return System.IO.Path.GetFileNameWithoutExtension(onnxModelPath); } }


        public virtual UAIFunctionResultType type { get { return UAIFunctionResultType.IMAGE; } set { type = UAIFunctionResultType.IMAGE; } }

        public OnnxProcessor(string modelPath)
    {
        onnxModelPath = modelPath;
        _session = new InferenceSession(onnxModelPath);
    }

    public List<NamedOnnxValue> CreateOnnxInput(Tensor<float> inputTensor)
    {
        return new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor(inputName, inputTensor)
        };
    }

        public override void Start()
        {
            base.Start();
            if (config == null)
            {
                config = new ONNXProcessorConfig();
            }
            ONNXMetaData oNNXMetaData = new ONNXMetaData();
            oNNXMetaData.Inputs.Add(new ONNXIO() { name = "input", dimensions = new List<int>() { 1, 3, 512, 512 } });
            oNNXMetaData.Outputs.Add(new ONNXIO() { name = "output", dimensions = new List<int>() { 1, 3, 512, 512 } });
            config.onnxMetaData = oNNXMetaData;
        }
        public void SetInputName(string name, string value)
        {
            if (config == null)
            {
                config = new ONNXProcessorConfig();
            }
            var match = config.onnxMetaData.Inputs.Find(x => x.name == name);
            if (match != null)
            {
                match.name = value;
            }
        }

        public void SetOutputName(string name, string value)
        {
            if (config == null)
            {
                config = new ONNXProcessorConfig();
            }
            var match = config.onnxMetaData.Outputs.Find(x => x.name == name);
            if (match != null)
            {
                match.name = value;
            }
        }

        public void SetInputDimensions(string name, List<int> value)
        {
            if (config == null)
            {
                config = new ONNXProcessorConfig();
            }
            var match = config.onnxMetaData.Inputs.Find(x => x.name == name);
            if (match != null)
            {
                match.dimensions = value;
            }
        }

        public void SetOutputDimensions(string name, List<int> value)
        {
            if (config == null)
            {
                config = new ONNXProcessorConfig();
            }
            var match = config.onnxMetaData.Outputs.Find(x => x.name == name);
            if (match != null)
            {
                match.dimensions = value;
            }
        }
        public virtual async Task RunOnnxInference()
    {
        Console.WriteLine("Running inference...");
        // Example of running inference:
        // using var results = _session.Run(inputs);
    }

    public Bitmap LoadImage(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        return new Bitmap(filePath);
    }

    public Tensor<float> PreprocessImageToTensor(Bitmap image)
    {
        int width = image.Width;
        int height = image.Height;
        var data = new float[1 * 3 * height * width];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixel = image.GetPixel(x, y);
                data[(0 * 3 * height * width) + (0 * height * width) + (y * width) + x] = (pixel.R / 255f * 2f) - 1f;
                data[(0 * 3 * height * width) + (1 * height * width) + (y * width) + x] = (pixel.G / 255f * 2f) - 1f;
                data[(0 * 3 * height * width) + (2 * height * width) + (y * width) + x] = (pixel.B / 255f * 2f) - 1f;
            }
        }
        return new DenseTensor<float>(data, new[] { 1, 3, height, width });
    }

    public List<Tensor<float>> ExtractChannels(Tensor<float> outputTensor)
    {
        List<Tensor<float>> channels = new List<Tensor<float>>();
        int height = outputTensor.Dimensions[2];
        int width = outputTensor.Dimensions[3];

        for (int y = 0; y < outputTensor.Dimensions[1]; y++)
        {
            channels.Add(ExtractTensorOutput(outputTensor, 0, y, height, width));
        }
        return channels;
    }

    private Tensor<float> ExtractTensorOutput(Tensor<float> outputTensor, int batchIndex, int channelIndex, int height, int width)
    {
        float[] data = new float[height * width];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = batchIndex * (3 * height * width) + channelIndex * (height * width) + y * width + x;
                data[y * width + x] = outputTensor.GetValue(index);
            }
        }
        return new DenseTensor<float>(data, new[] { height, width });
    }
        public Tensor<float> MatToTensor(Mat mat)
        {
            // Ensure the Mat is in RGB format
            if (mat.NumberOfChannels != 3)
            {
                throw new ArgumentException("The input Mat must have 3 channels (RGB/BGR).");
            }

            int height = mat.Rows;
            int width = mat.Cols;
            int channels = mat.NumberOfChannels; // Should be 3 (for RGB)

            // Initialize a float array to hold the pixel data in CHW format
            float[] tensorData = new float[1 * channels * height * width];  // 1 for batch size

            // Get pixel data from Mat
            byte[] pixelData = new byte[height * width * channels];
            System.Runtime.InteropServices.Marshal.Copy(mat.DataPointer, pixelData, 0, pixelData.Length);

            // Convert BGR to RGB and reorder the data into CHW format
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int matIndex = (y * width + x) * channels;  // Mat's BGR order

                    // Tensor's CHW format (batch_size, channels, height, width)
                    tensorData[(0 * channels * height * width) + (0 * height * width) + (y * width) + x] = pixelData[matIndex + 2] / 255.0f;  // Red
                    tensorData[(0 * channels * height * width) + (1 * height * width) + (y * width) + x] = pixelData[matIndex + 1] / 255.0f;  // Green
                    tensorData[(0 * channels * height * width) + (2 * height * width) + (y * width) + x] = pixelData[matIndex] / 255.0f;      // Blue
                }
            }

            // Return a DenseTensor<float> with shape [1, 3, height, width] (batch size 1, 3 channels)
            return new DenseTensor<float>(tensorData, new[] { 1, 3, height, width });
        }
        public static Bitmap SaveTensorAsGrayscaleBitmap(Tensor<float> tensor)
        {
            // Ensure the tensor has the expected shape (height, width)
            if (tensor.Dimensions.Length != 2)
            {
                throw new ArgumentException("Expected tensor shape: (height, width)");
            }

            int height = tensor.Dimensions[0];
            int width = tensor.Dimensions[1];
            Bitmap bitmap = new Bitmap(width, height);
            // Create a new Bitmap object with the specified dimensions
           
                // Iterate through each pixel in the tensor and set the corresponding pixel in the Bitmap
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // Get the intensity value from the tensor, assume it's normalized between [0, 1]
                        int intensity = (int)(tensor[y, x] * 255);

                        // Clamp intensity to the 0-255 range
                        intensity = Math.Clamp(intensity, 0, 255);

                        // Create a grayscale color (R=G=B=intensity)
                        Color color = Color.FromArgb(intensity, intensity, intensity);

                        // Set the pixel color in the Bitmap
                        bitmap.SetPixel(x, y, color);
                    }
                }

                // Save the Bitmap to the specified file path
            return bitmap;

        }
        public static Bitmap SaveTensorAsBitmap(Tensor<float> tensor)
        {
            // Ensure the tensor has the expected shape (batch_size, channels, height, width)
            if (tensor.Dimensions.Length != 4 || tensor.Dimensions[1] != 3)
            {
                throw new ArgumentException("Expected tensor shape: (batch_size, 3, height, width)");
            }

            int height = tensor.Dimensions[2];
            int width = tensor.Dimensions[3];

            // Create a new Bitmap object
            using (Bitmap bitmap = new Bitmap(width, height))
            {
                // Iterate through each pixel in the tensor and set the corresponding pixel in the Bitmap
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // Get the RGB values from the tensor
                        int r = (int)(tensor[0, 0, y, x] * 255); // Red channel
                        int g = (int)(tensor[0, 1, y, x] * 255); // Green channel
                        int b = (int)(tensor[0, 2, y, x] * 255); // Blue channel

                        // Clamp values to the 0-255 range
                        r = Math.Clamp(r, 0, 255);
                        g = Math.Clamp(g, 0, 255);
                        b = Math.Clamp(b, 0, 255);

                        // Set the pixel color in the Bitmap
                        bitmap.SetPixel(x, y, Color.FromArgb(r, g, b));
                    }
                }

                // Save the Bitmap to the specified file path
                return bitmap;
            }
        }
        public Mat TensorRToMat(Tensor<float> tensor, Vector2I imageSize)
        {
            var img2 = SaveTensorAsGrayscaleBitmap(tensor);

            img2.Save("P:\\temp\\aiface\\img2.png");

            //File.WriteAllBytes("P:\\temp\\aiface\\img.png", pixelData);
            // Create the Mat from the pixel data (height, width, 1 channel for grayscale)
            Mat mat = new Mat(img2.Height, img2.Width, DepthType.Cv8U, 1);  // 1 channel for grayscale
            //mat = new Mat(img2, Emgu.CV.CvEnum.ImreadModes.Grayscale);
            return mat;
        }

        public virtual void EnsureModelInUserPath()
        {
            // Check if the model is already copied to user://
            if (!File.Exists(ProjectSettings.GlobalizePath(userModelPath)))
            {
                Console.WriteLine("Model not found in user://, copying from res://");

                // Create the models directory in user:// if it doesn't exist

                // Use Godot's FileAccess class to open and copy the model
                if (File.Exists(onnxModelPath) != null)
                {
                    byte[] modelData = File.ReadAllBytes(onnxModelPath);
                    var outputDir = System.IO.Path.GetDirectoryName(userAbsoluteModelPath);
                    if (!Directory.Exists(outputDir))
                    {
                        Directory.CreateDirectory(outputDir);
                    }
                    // Write the model data to user:// using System.IO.File
                    System.IO.File.WriteAllBytes(ProjectSettings.GlobalizePath(userModelPath), modelData);
                    Console.WriteLine("Model copied to :" + userAbsoluteModelPath);
                }
                else
                {
                    Console.WriteLine("Failed to open model in res://");
                }

                Console.WriteLine("Model copied to user://");
            }
            else
            {
                Console.WriteLine("Model already exists in " + userAbsoluteModelPath);
            }
        }
        public Bitmap ResizeBitmap(Bitmap originalBitmap, int newWidth, int newHeight)
        {
            // Create a new empty bitmap with the specified dimensions
            Bitmap resizedBitmap = new Bitmap(newWidth, newHeight);

            // Draw the original bitmap onto the resized bitmap
            using (Graphics g = Graphics.FromImage(resizedBitmap))
            {
                // Optional: Set quality settings for better output
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                // Draw the image with new dimensions
                g.DrawImage(originalBitmap, 0, 0, newWidth, newHeight);
            }

            return resizedBitmap;
        }
        public Mat Texture2DToMat(Bitmap texture, Vector2I targetSize)
        {

            // Get the image from the texture
            var image = texture;
            if (image.PixelFormat != PixelFormat.Format24bppRgb)
            {
                // Create a new Bitmap with RGB8 format
                Bitmap rgb8Bitmap = new Bitmap(image.Width, image.Height, PixelFormat.Format24bppRgb);

                // Draw the original bitmap onto the new one to convert it
                using (Graphics g = Graphics.FromImage(rgb8Bitmap))
                {
                    g.DrawImage(image, 0, 0, image.Width, image.Height);
                }

                image.Dispose();  // Optionally dispose of the original if no longer needed
                image = rgb8Bitmap; // Replace original with RGB8 version
            }

            //image = ResizeBitmap(image, targetSize.X, targetSize.Y);
            // Get width and height
            int width = image.Width;
            int height = image.Height;

            // Create a new Mat (Emgu.CV Mat) with the same width and height, 3 channels (BGR format)
            Mat mat = new Mat(height, width, Emgu.CV.CvEnum.DepthType.Cv8U, 3); // 8-bit, 3-channel image (BGR)

            // Access pixel data in the Mat using DataPointer
            byte[] pixelData = new byte[width * height * 3]; // 3 channels for BGR
            IntPtr dataPointer = mat.DataPointer;  // Pointer to the Mat data

            // Copy pixel data from Godot image to the Mat
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var pixel = image.GetPixel(x, y);

                    // Calculate index in the pixelData array (Mat uses BGR format)
                    int index = (y * width + x) * 3;
                    pixelData[index] = (byte)(pixel.B * 255);  // Blue channel
                    pixelData[index + 1] = (byte)(pixel.G * 255);  // Green channel
                    pixelData[index + 2] = (byte)(pixel.R * 255);  // Red channel
                }
            }

            // Copy the pixel data to the Mat using Marshal.Copy
            System.Runtime.InteropServices.Marshal.Copy(pixelData, 0, dataPointer, pixelData.Length);
            return mat;
        }
        public List<string> CreateOnnxOutput()
        {
            return new List<string> { outputName };
        }

        public List<Tensor<float>> GetResultTensors(IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results)
        {
            using (var result = results.FirstOrDefault())
            {
                if (result == null || !(result.Value is Tensor<float>))
                {
                    throw new ArgumentException("Expected a Tensor<float> in the results.");
                }
                var val = result.Value;
                Tensor<float> outputTensor = result.AsTensor<float>();

                return ExtractChannels(outputTensor);
            }
        }

        public Bitmap PostprocessOutputToBitmap(float[] outputData, int width, int height)
    {
        Bitmap outputImage = new Bitmap(width, height, PixelFormat.Format24bppRgb);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int pixelIndex = y * width + x;
                int r = (int)((outputData[pixelIndex * 3 + 0] + 1f) / 2f * 255f);
                int g = (int)((outputData[pixelIndex * 3 + 1] + 1f) / 2f * 255f);
                int b = (int)((outputData[pixelIndex * 3 + 2] + 1f) / 2f * 255f);

                r = Math.Clamp(r, 0, 255);
                g = Math.Clamp(g, 0, 255);
                b = Math.Clamp(b, 0, 255);

                outputImage.SetPixel(x, y, Color.FromArgb(r, g, b));
            }
        }

        return outputImage;
    }
        public Vector4 RandomColor()
        {
            Random random = new Random();
            float r = (float)random.NextDouble();
            float g = (float)random.NextDouble();
            float b = (float)random.NextDouble();
            Vector4 color = new Vector4(r, g, b, 1.0f);
            return color;
        }

        public Bitmap MatRToTexture2D(Mat mat)
        {
            // Get width and height from the Mat
            int width = mat.Width;
            int height = mat.Height;

            // Create a new Godot Image
            //Image image = Image.CreateEmpty(width, height, false, Image.Format.Rgb8);  // Create an RGB8 Godot image
            Bitmap image = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            // Access pixel data from the Mat using DataPointer
            byte[] pixelData = new byte[width * height * mat.NumberOfChannels]; // 3 channels for BGR
            IntPtr dataPointer = mat.DataPointer;  // Pointer to the Mat data

            // Copy the pixel data from the Mat into the byte array
            System.Runtime.InteropServices.Marshal.Copy(dataPointer, pixelData, 0, pixelData.Length);

            // Set the pixel data in the Godot Image
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Calculate index in the pixelData array (Mat uses BGR format)
                    int index = (y * width + x) * mat.NumberOfChannels;

                    byte b = pixelData[index];     // Blue channel
                                                   //byte g = pixelData[index + 1]; // Green channel
                                                   //byte r = pixelData[index + 2]; // Red channel

                    // Convert to Godot's RGB format and set the pixel
                    Color pixel = Color.FromArgb((int)(b / 255.0f), (int)(b / 255.0f), (int)(b / 255.0f));
                    image.SetPixel(x, y, pixel);
                }
            }
            return image;
        }
        public List<Bitmap> ConvertTensorsToBitmaps(Vector2I origImageSize, List<Tensor<float>> rest)
        {
            List < Bitmap > bitmaps = new List<Bitmap>();
            int index = 0;
            foreach (var tensor in rest)
            {
                // Random Color
                //Vector4 color = RandomColor();
                bitmaps.Add(TensorToBitmapMask(origImageSize, index, tensor));
                //resultRect.AddChannel(labelName, t);

                index += 1;
            }
            return bitmaps;
        }
        public void ProcessSaveMasks(List<Bitmap> bitmaps, string outputPath, bool saveMasks = true)
        {
            if (saveMasks)
            {
                SaveImagesToFile(bitmaps, outputPath, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        public void ProcessJSONOutput(List<Bitmap> bitmaps, string outputPath, bool saveJson = true)
        {
            if (saveJson)
            {

                MultiMediaItem multiMediaItem = new MultiMediaItem();
                int indx = 0;
                foreach (var bitmap in bitmaps)
                {
                    string label = "mask";
                    if (indx < labels.Length)
                    {
                        label = labels[indx];
                    }
                    MediaItem mediaItem = new MediaItem();
                    mediaItem.media = OnnxProcessor.BitmapToBase64String(bitmap, ImageFormat.Png);
                    mediaItem.name = label;
                    mediaItem.type = label;
                    mediaItem.prompt = label;
                    multiMediaItem.media.Add(mediaItem);
                    indx++;
                }
                multiMediaItem.Save(outputPath );
            }
        }

        public void SaveImagesToFile(List<Bitmap> bitmaps, string path, ImageFormat imageFormat )
        {
            string ext = imageFormat.ToString().ToLower();
            for (int i = 0; i < bitmaps.Count; i++)
            {
                var label = "mask";
                try
                {
                    label = labels[i];
                }
                catch
                {

                }
                string fileName = $"{path}/{label}_{i.ToString("D4")}.{ext}";
                bitmaps[i].Save(fileName, imageFormat);
            }
        }
        public static string BitmapToBase64String(Bitmap bitmap, System.Drawing.Imaging.ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Save the bitmap to the MemoryStream in the specified format
                bitmap.Save(ms, format);

                // Convert the byte array in memory to a Base64 string
                return Convert.ToBase64String(ms.ToArray());
            }
        }
        private Bitmap TensorToBitmapMask(Vector2I origImageSize, int index, Tensor<float> tensor)
        {
            var indd = index + 2;
            Vector2I generatedSize = new Vector2I(tensor.Dimensions[1], tensor.Dimensions[0]);
            var img = SaveTensorAsGrayscaleBitmap(tensor);
            img = ResizeBitmap(img, origImageSize.X, origImageSize.Y);
            string labelName = "";
            if (labels.Length > index)
            {
                labelName = labels[index];
            }

            return img;
        }

        public void Dispose()
    {
        _session?.Dispose();
    }

        public virtual void Select()
        {
            isSelected = true;
        }

        public virtual void Deselect()
        {
            isSelected = false;
        }

        public static Bitmap CreateCompositeMask(List<Bitmap> masks)
        {
            if (masks == null || masks.Count == 0)
                throw new ArgumentException("Mask list is empty or null.");

            // Use the dimensions of the first mask for the composite
            int width = masks[0].Width;
            int height = masks[0].Height;

            // Create a new composite bitmap with the same dimensions
            Bitmap composite = new Bitmap(width, height);

            // Initialize all pixels to black in the composite
            using (Graphics g = Graphics.FromImage(composite))
            {
                g.Clear(Color.Black);
            }

            // Combine each mask into the composite
            foreach (var mask in masks)
            {
                if (mask.Width != width || mask.Height != height)
                    throw new ArgumentException("All masks must have the same dimensions.");

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Color maskPixel = mask.GetPixel(x, y);
                        Color compositePixel = composite.GetPixel(x, y);

                        // If the current mask pixel is white, set the composite pixel to white
                        if (maskPixel.R == 255 && maskPixel.G == 255 && maskPixel.B == 255)
                        {
                            composite.SetPixel(x, y, Color.White);
                        }
                    }
                }
            }

            return composite;
        }

        // Function to subtract white areas in bgMask from the composite bitmap
        public static Bitmap SubtractMask(Bitmap composite, Bitmap bgMask)
        {
            if (composite.Width != bgMask.Width || composite.Height != bgMask.Height)
                throw new ArgumentException("Composite and bgMask must have the same dimensions.");

            // Create a new bitmap to store the result
            Bitmap result = new Bitmap(composite.Width, composite.Height);

            for (int y = 0; y < composite.Height; y++)
            {
                for (int x = 0; x < composite.Width; x++)
                {
                    Color compositePixel = composite.GetPixel(x, y);
                    Color bgMaskPixel = bgMask.GetPixel(x, y);

                    // If bgMask pixel is white, set result pixel to black, else keep the composite pixel
                    if (bgMaskPixel.R == 255 && bgMaskPixel.G == 255 && bgMaskPixel.B == 255)
                    {
                        result.SetPixel(x, y, Color.Black); // Subtract the white area
                    }
                    else
                    {
                        result.SetPixel(x, y, compositePixel); // Keep the original composite pixel
                    }
                }
            }

            return result;
        }
    }
}
