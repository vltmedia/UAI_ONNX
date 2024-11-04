using System.Drawing.Imaging;
using System.Drawing;

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
