# UAI ONNX
ONNX Runner built for C# projects that target .Net8.0 or later. This library is built on top of the ONNX Runtime C# API and provides a simple way to load and run ONNX models in C#.
```Microsoft.ML.OnnxRuntime.GPU``` is not installed since that should be handled by the next application that uses this library.

# Installation
You can restore the Nuget dependencies by running the following command in the project directory:
```bash
dotnet restore
```
You will need to copy the contents of ```UAI\thirdParty\openCV``` to the output directory of your app. This is because the OpenCV dlls are not included in the Nuget package.

# Usage

## OnnxImageProcessor
The ```OnnxImageProcessor``` class is a base class that can be extended to create custom image processing models. The class provides methods to load an ONNX model, preprocess input images, run inference, and post-process the output tensors. The class is designed to be used with image processing models that take an image as input and return an output.

```csharp
using Microsoft.ML.OnnxRuntime;
using System.Drawing;
using UAI.Common.AI;
namespace UAI.AI.Models.FaceParsing
{
public class FaceParsing : OnnxImageProcessor
{
    public override string[] labels { get { return labels_; } set { labels_ = value; } }

    public string[] labels_ = new string[] { "Background", "Skin", "Nose", "Glasses", "Left Eye", "Right Eye", "Left Brow", "Right Brow",   "Left Ear", "Right Ear",
          "Mouth", "Upper Lip", "Lower Lip","Hair" , "Hat", "Earring", "Necklace", "Clothing" };

        public FaceParsing(string modelPath) : base(modelPath)
        {
          

        }

        public override void Start()
	{
		base.Start();

            SetInputName("input","pixel_values");
            SetInputDimensions("pixel_values", new List<int>() { 1, 3, 512, 512 });
            SetOutputName("output","logits");
            SetOutputDimensions("logits", new List<int>() { 1, 18, 512, 512 });
	}

  
	public override async Task RunOnnxInference()
        {

            await base.RunOnnxInference();

            // Setup Ouptuts based on inputImage
            config.onnxMetaData.Inputs[0].dimensions = new List<int>() { 1, 3, inputImageSize.x, inputImageSize.y };
            config.onnxMetaData.Outputs[0].dimensions = new List<int>() { 1, 18, inputImageSize.x, inputImageSize.y };
            var outputPath = FaceParserRuntime.args.outputPath;

            // PreProcess Data
            var inputData = CreateOnnxInput(MatToTensor(Texture2DToMat(inputTexture, inputImageSize)));

            // Process Image
            IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = _session.Run(inputData, CreateOnnxOutput());
            
            // Process the Output Data
            var rest = GetResultTensors(results);
            List<Bitmap> bitmaps = ConvertTensorsToBitmaps(inputImageSize, rest);


            // Save the Masks and JSON if the options are set
            ProcessSaveMasks(bitmaps, outputPath, FaceParserRuntime.args.saveMasks);
            ProcessJSONOutput(bitmaps, outputPath +"/" + Path.GetFileNameWithoutExtension(FaceParserRuntime.args.inputPath) + "_masks.json", FaceParserRuntime.args.saveJson);

            // Send Inference Finished Signal
            await SendInferenceFinished();
        }

       
        public virtual async Task SendInferenceFinished()
        {
           Console.WriteLine("Inference Finished");
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void Update(float delta)
	{
	}
}
}


```