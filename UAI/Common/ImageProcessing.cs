using System.Drawing.Imaging;
using System.Drawing;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV;

namespace UAI.Common
{

    public class ImageProcessor
    {
        public static Bitmap ApplyGaussianBlur(Bitmap image, int amount)
        {
            if (amount < 1) amount = 1; // Ensure amount is at least 1

            Bitmap blurredImage = new Bitmap(image.Width, image.Height);

            // Define a Gaussian kernel based on the amount of blur
            double[,] kernel = CreateGaussianKernel(amount);

            int kernelSize = kernel.GetLength(0);
            int radius = kernelSize / 2;

            // Lock the source and destination images for direct pixel access
            BitmapData srcData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData destData = blurredImage.LockBits(new Rectangle(0, 0, blurredImage.Width, blurredImage.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* srcPtr = (byte*)srcData.Scan0;
                byte* destPtr = (byte*)destData.Scan0;
                int stride = srcData.Stride;

                for (int y = radius; y < image.Height - radius; y++)
                {
                    for (int x = radius; x < image.Width - radius; x++)
                    {
                        double[] rgbSum = new double[3];
                        double alphaSum = 0;
                        double kernelSum = 0;

                        for (int ky = -radius; ky <= radius; ky++)
                        {
                            for (int kx = -radius; kx <= radius; kx++)
                            {
                                int pixelPosX = (x + kx) * 4;
                                int pixelPosY = y + ky;

                                byte* pixelPtr = srcPtr + (pixelPosY * stride) + pixelPosX;

                                double kernelValue = kernel[ky + radius, kx + radius];

                                rgbSum[0] += pixelPtr[0] * kernelValue; // Blue
                                rgbSum[1] += pixelPtr[1] * kernelValue; // Green
                                rgbSum[2] += pixelPtr[2] * kernelValue; // Red
                                alphaSum += pixelPtr[3] * kernelValue;  // Alpha
                                kernelSum += kernelValue;
                            }
                        }

                        int destIndex = (y * stride) + (x * 4);

                        destPtr[destIndex + 0] = (byte)(rgbSum[0] / kernelSum); // Blue
                        destPtr[destIndex + 1] = (byte)(rgbSum[1] / kernelSum); // Green
                        destPtr[destIndex + 2] = (byte)(rgbSum[2] / kernelSum); // Red
                        destPtr[destIndex + 3] = (byte)(alphaSum / kernelSum);  // Alpha
                    }
                }
            }

            // Unlock the bits
            image.UnlockBits(srcData);
            blurredImage.UnlockBits(destData);

            return blurredImage;
        }

        public static Bitmap ResizeBitmap(Bitmap original, int newWidth, int newHeight)
        {
            Bitmap resizedBitmap = new Bitmap(newWidth, newHeight);
            using (Graphics graphics = Graphics.FromImage(resizedBitmap))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(original, 0, 0, newWidth, newHeight);
            }
            return resizedBitmap;
        }
        private static Bitmap ResizeArrayToBitmap(float[,,] array, int newWidth, int newHeight)
        {
            int channels = array.GetLength(0);
            int height = array.GetLength(1);
            int width = array.GetLength(2);

            Bitmap originalBitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            // Copy array data to bitmap
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int r = (int)(array[0, y, x] * 255);
                    //int g = (int)(array[1, y, x] * 255);
                    //int b = (int)(array[2, y, x] * 255);
                    originalBitmap.SetPixel(x, y, Color.FromArgb(r, r, r));
                }
            }

            // Resize using a third-party library or native method
            Bitmap resizedBitmap = new Bitmap(originalBitmap, new Size(newWidth, newHeight));

            return resizedBitmap;
        }
        public static Bitmap InvertBitmap(Bitmap original)
        {
            // Create a new Bitmap object to store the inverted image
            Bitmap invertedImage = new Bitmap(original.Width, original.Height);

            // Loop through each pixel in the image
            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    // Get the pixel color at (x, y)
                    Color originalColor = original.GetPixel(x, y);

                    // Invert the color (255 - each color component)
                    Color invertedColor = Color.FromArgb(
                        originalColor.A,                   // Preserve the alpha channel
                        255 - originalColor.R,             // Invert the red component
                        255 - originalColor.G,             // Invert the green component
                        255 - originalColor.B              // Invert the blue component
                    );

                    // Set the inverted color to the new image
                    invertedImage.SetPixel(x, y, invertedColor);
                }
            }

            return invertedImage;
        }




        public static Mat SmoothAndThreshold(Mat mask, int kernelSize = 5, double thresholdValue = 128)
        {
            Mat smoothedMask = new Mat();
            Mat binaryMask = new Mat();

            // Apply Gaussian blur
            CvInvoke.GaussianBlur(mask, smoothedMask, new System.Drawing.Size(kernelSize, kernelSize), 0);

            // Apply threshold
            CvInvoke.Threshold(smoothedMask, binaryMask, thresholdValue, 255, ThresholdType.Binary);

            return binaryMask;
        }


        public static Mat CleanMask(Mat mask)
        {
            Mat cleanedMask = new Mat();

            // Create a structuring element (kernel) for morphological operations
            Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new System.Drawing.Size(3, 3), new System.Drawing.Point(-1, -1));

            // Apply morphological operations
            CvInvoke.MorphologyEx(mask, cleanedMask, MorphOp.Open, kernel, new System.Drawing.Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            CvInvoke.MorphologyEx(cleanedMask, cleanedMask, MorphOp.Close, kernel, new System.Drawing.Point(-1, -1), 1, BorderType.Default, new MCvScalar());

            return cleanedMask;
        }
        public static List<Bitmap> LoadVideoAsFrames(string videoPath)
        {
            List<Bitmap> bitmaps = new List<Bitmap>();

            // Initialize the VideoCapture object
            using (VideoCapture capture = new VideoCapture(videoPath))
            {
                if (!capture.IsOpened)
                {
                    Console.WriteLine("Failed to open video file.");
                    return bitmaps;
                }

                Mat frame = new Mat();
                while (true)
                {
                    // Read the next frame
                    capture.Read(frame);

                    // If the frame is empty, break the loop
                    if (frame.IsEmpty)
                        break;

                    // Convert the Mat to Bitmap and add it to the list
                    Bitmap bitmap = frame.ToBitmap();
                    bitmaps.Add(bitmap);
                }
            }

            return bitmaps;
        }

        public static bool CombineBitmapsToVideo(List<Bitmap> bitmaps, string outputPath, int framerate)
        {
            if (bitmaps == null || bitmaps.Count == 0)
            {
                Console.WriteLine("No bitmaps provided to combine.");
                return false;
            }

            int width = bitmaps[0].Width;
            int height = bitmaps[0].Height;

            // Create the VideoWriter object
            using (VideoWriter writer = new VideoWriter(
                outputPath,
                VideoWriter.Fourcc('H', '2', '6', '4'), // H.264 codec
                framerate,
                new System.Drawing.Size(width, height),
                true)) // 'true' for color video
            {
                if (!writer.IsOpened)
                {
                    Console.WriteLine("Failed to open video writer.");
                    return false;
                }

                foreach (var bitmap in bitmaps)
                {
                    if (bitmap.Width != width || bitmap.Height != height)
                    {
                        Console.WriteLine("Bitmap size mismatch. All bitmaps must have the same dimensions.");
                        return false;
                    }

                    using (Mat frame = bitmap.ToMat())
                    {
                        writer.Write(frame);
                    }
                }
            }
            return true;

        }


        public static Mat ApplyMedianBlur(Mat mask, int kernelSize = 5)
        {
            Mat blurredMask = new Mat();
            CvInvoke.MedianBlur(mask, blurredMask, kernelSize);
            return blurredMask;
        }

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
        public static double[,] CreateGaussianKernel(int radius)
        {
            int size = 2 * radius + 1;
            double[,] kernel = new double[size, size];
            double sigma = radius / 2.0;
            double sigma2 = sigma * sigma;
            double piSigma2 = 2 * Math.PI * sigma2;
            double normalization = 1.0 / piSigma2;
            double sum = 0;

            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    double exponent = -(x * x + y * y) / (2 * sigma2);
                    kernel[y + radius, x + radius] = normalization * Math.Exp(exponent);
                    sum += kernel[y + radius, x + radius];
                }
            }

            // Normalize kernel to ensure the sum is 1
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    kernel[y, x] /= sum;
                }
            }

            return kernel;
        }

        public static Bitmap ApplyChannelAsAlpha(Bitmap bmp1, Bitmap bmp2, int channelIndex = 2)
        {
            // Ensure both bitmaps have the same dimensions
            if (bmp1.Width != bmp2.Width || bmp1.Height != bmp2.Height)
                throw new ArgumentException("Bitmaps must have the same dimensions.");

            // Create a new bitmap with the same dimensions as bmp1
            Bitmap result = new Bitmap(bmp1.Width, bmp1.Height, PixelFormat.Format32bppArgb);

            // Lock bits for bmp1, bmp2, and the result to access pixel data directly
            BitmapData bmp1Data = bmp1.LockBits(new Rectangle(0, 0, bmp1.Width, bmp1.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData bmp2Data = bmp2.LockBits(new Rectangle(0, 0, bmp2.Width, bmp2.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData resultData = result.LockBits(new Rectangle(0, 0, result.Width, result.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* bmp1Ptr = (byte*)bmp1Data.Scan0;
                byte* bmp2Ptr = (byte*)bmp2Data.Scan0;
                byte* resultPtr = (byte*)resultData.Scan0;

                int bytesPerPixel = 4; // For 32bppArgb format

                for (int y = 0; y < bmp1.Height; y++)
                {
                    for (int x = 0; x < bmp1.Width; x++)
                    {
                        int index = (y * bmp1Data.Stride) + (x * bytesPerPixel);

                        // Copy RGB from bmp1
                        resultPtr[index + 0] = bmp1Ptr[index + 0]; // Blue
                        resultPtr[index + 1] = bmp1Ptr[index + 1]; // Green
                        resultPtr[index + 2] = bmp1Ptr[index + 2]; // Red

                        // Set alpha channel to the red channel of bmp2
                        resultPtr[index + 3] = bmp2Ptr[index + channelIndex]; // Alpha from Red channel of bmp2
                    }
                }
            }

            // Unlock bits
            bmp1.UnlockBits(bmp1Data);
            bmp2.UnlockBits(bmp2Data);
            result.UnlockBits(resultData);

            return result;
        }
    }
}
