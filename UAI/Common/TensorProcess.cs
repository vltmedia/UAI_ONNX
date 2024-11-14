using Emgu.CV.CvEnum;
using Emgu.CV;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV.Structure;
using System.Drawing.Imaging;
using System.Drawing;

namespace UAI_ONNX.UAI.Common
{
    public class TensorProcess
    {
        public static Tensor<float> ApplyThreshold(Tensor<float> inputTensor)
        {
            int rows = inputTensor.Dimensions[0];
            int cols = inputTensor.Dimensions[1];
            float max = inputTensor.Max();
            float threshold = max - 0.02f;
            Tensor<float> thresholdedTensor = new DenseTensor<float>(new int[] { rows, cols });

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    // Apply threshold: set value to 1 if above the threshold, otherwise 0
                    thresholdedTensor[i, j] = inputTensor[i, j] > threshold ? 1.0f : 0.0f;
                }
            }

            return thresholdedTensor;
        }

        public static List<Tensor<float>> NormalizeTensors(List<Tensor<float>> inputTensors, float threshold = 0.3f)
        {
            List<Tensor<float>> normalizedTensors = new List<Tensor<float>>();
    foreach(var r in inputTensors)
    {
        normalizedTensors.Add( NormalizeTensor(r, threshold));
    }

            return normalizedTensors;
        }
        public static Tensor<float> ApplyGammaCorrection(Tensor<float> inputTensor, float gamma)
        {
            int rows = inputTensor.Dimensions[0];
            int cols = inputTensor.Dimensions[1];
            Tensor<float> gammaCorrectedTensor = new DenseTensor<float>(new int[] { rows, cols });

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    // Apply gamma correction: output = input^(1/gamma)
                    gammaCorrectedTensor[i, j] = (float)Math.Pow(inputTensor[i, j], 1.0 / gamma);
                }
            }

            return gammaCorrectedTensor;
        }
        public static Tensor<float> AdjustContrast(Tensor<float> inputTensor)
        {
            // Find the minimum and maximum values in the tensor
            float minValue = inputTensor.Min();
            float maxValue = inputTensor.Max();

            if (maxValue == minValue)
            {
                // If the min and max are the same, return the original tensor (to avoid division by zero)
                return inputTensor;
            }

            int rows = inputTensor.Dimensions[0];
            int cols = inputTensor.Dimensions[1];
            Tensor<float> adjustedTensor = new DenseTensor<float>(new int[] { rows, cols });

            // Adjust the contrast by mapping min to 0 and max to 1
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    adjustedTensor[i, j] = (inputTensor[i, j] - minValue) / (maxValue - minValue);
                    // Clamp to ensure no values go beyond [0, 1]
                    adjustedTensor[i, j] = Math.Clamp(adjustedTensor[i, j], 0.0f, 1.0f);
                }
            }

            return adjustedTensor;
        }
        public static Tensor<float> NormalizeTensor(Tensor<float> inputTensor, float threshold = 0.3f)
        {
            // Find the minimum and maximum values in the tensor
            float maxValue = inputTensor.Max();
            float minValue = inputTensor.Min();
            if (maxValue == 0)
            {
                // If maxValue is 0, return the original tensor (to avoid division by zero)
                return inputTensor;
            }

            int rows = inputTensor.Dimensions[0];
            int cols = inputTensor.Dimensions[1];
            Tensor<float> normalizedTensor = new DenseTensor<float>(new int[] { rows, cols });
            bool allWhite = true;
            // Normalize the tensor values by dividing each by the maxValue
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    var val = inputTensor[i, j] / maxValue;
                    if(threshold > 0.0f){
                    if (val > threshold)
                    {
                        val = 1.0f;
                    }
                    else
                    {
                        val = 0.0f;
                        allWhite = false;
                    }
                    }
                    //normalizedTensor[i, j] = inputTensor[i, j] / maxValue;
                    // Optional: Clamp the values to ensure they stay within [0, 1]
                    normalizedTensor[i, j] = Math.Clamp(val, 0.0f, 1.0f);
                    //normalizedTensor[i, j] = Math.Clamp(normalizedTensor[i, j], 0.0f, 1.0f);
                }
            }
            if (allWhite)
            {
                normalizedTensor = new DenseTensor<float>(new int[] { rows, cols });
            }

            return normalizedTensor;
        }


    }
}
