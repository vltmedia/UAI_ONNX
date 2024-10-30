using System;
public partial class ONNXProcessorConfig : UAIFunctionObject
{
  

    public string onnxModelPath = "res://models/unet.onnx";  // Path to the ONNX model
    public ONNXMetaData onnxMetaData;

    public string onnxProcessor = "res://";
    public string settingsUI = "res://";

}