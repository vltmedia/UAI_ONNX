using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UAI.Common.AI;


public  class OnnxAppState
    {

    public static OnnxProcessor onnxProcessor;
    public static OnnxProcessor processingOnnxProcessor;
    public static List<OnnxProcessor> onnxProcessorsToProcess = new List<OnnxProcessor>();
    public static List<OnnxProcessor> onnxProcessors = new List<OnnxProcessor>();


    public static void SetOnnxProcessor(OnnxProcessor processor)
    {
        onnxProcessor = processor;
        onnxProcessor.Select();
        foreach (OnnxProcessor p in onnxProcessors)
        {
            if (p != processor)
            {
                p.Deselect();
            }
        }
    }
    public static async Task RunOnnxProcessor()
    {

       await onnxProcessor.RunOnnxInference();
    }
    public static void AddOnnxProcessor(OnnxProcessor processor)
    {
        onnxProcessors.Add(processor);

    }
    public static OnnxProcessor GetOnnxProcessor()
    {
        return onnxProcessor;

    }

    public static List<OnnxProcessor> GetOnnxProcessors()
    {
        return onnxProcessors;

    }

    public static async Task RunNextFX()
    {

       if (onnxProcessorsToProcess.Count > 0)
        {
            if(processingOnnxProcessor == null)
            {
                processingOnnxProcessor = onnxProcessorsToProcess.First();
            }
            else
            {
                var index = onnxProcessorsToProcess.IndexOf(processingOnnxProcessor);
                if (index < onnxProcessorsToProcess.Count - 1)
                {
                    processingOnnxProcessor = onnxProcessorsToProcess[index + 1];
                   await  processingOnnxProcessor.RunOnnxInference();
                }
                else
                {
                    ONNXProcessors.Instance.OnnxProcessorsFinished();
                    return;

                }
            }
        }
    
    }



    }
