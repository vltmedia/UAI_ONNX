using System;
using System.Collections.Generic;
[System.Serializable]
public class ONNXMetaData 
{
    public List<ONNXIO> Inputs = new List<ONNXIO>();
    public List<ONNXIO> Outputs = new List<ONNXIO>();


}