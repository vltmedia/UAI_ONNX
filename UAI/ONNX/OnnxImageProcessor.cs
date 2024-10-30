
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Drawing;
namespace UAI.Common.AI
{
    public partial class OnnxImageProcessor : OnnxProcessor
{

	public Tensor<float> imageTensor;

    public OnnxImageProcessor(string modelPath) : base(modelPath)
    {
            onnxModelPath = modelPath;
            if (config == null)
            {
                config = new ONNXProcessorConfig();
                config.onnxMetaData = new ONNXMetaData();
            }
            config.onnxModelPath = modelPath;
        }

    // Called when the node enters the scene tree for the first time.
    public override void Start()
	{
		base.Start();

		
	}
	
		public void LoadImage(string imagePath)
		{
			// Load to Bitmap
			byte[] imageBytes = File.ReadAllBytes(imagePath);
			using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                inputTexture = new Bitmap(ms);
            }
            }

		public string SaveImage(string outputPath)
        {
            // Save to file
            inputTexture.Save(outputPath);
            return outputPath;
        }

	public override async Task RunOnnxInference()
	{
            inputImageSize = new Vector2I(inputTexture.Width, inputTexture.Height);
            await base.RunOnnxInference();

		//Console.WriteLine("Processing image");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void Update()
    {
        base.Update();
	}
}
}
