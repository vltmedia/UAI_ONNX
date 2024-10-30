

using System;
using System.Drawing;
[System.Serializable]
public class UAIFunctionResults
{
    public  UAIFunctionResult image;
    public  UAIFunctionResult video;
    public  UAIFunctionResult audio;
    public  UAIFunctionResult binary;
    public  UAIFunctionResult text;
    public  DenseTexture imageTexture { get { return image.texture; } set { image.texture = value; } }
    
    public UAIFunctionResults()
    {
        image = new UAIFunctionResult();
        video = new UAIFunctionResult();
        audio = new UAIFunctionResult();
        binary = new UAIFunctionResult();
        text = new UAIFunctionResult();
    }

}
public class UAIFunctionRuntimeResults
{
    public static UAIFunctionResults main;
    public static UAIFunctionResults temp;
}

[System.Serializable]
public class UAIFunctionResult
{
    public UAIFunctionResultType type = UAIFunctionResultType.IMAGE;
    public string name = "";
    public string id = "";
    public string channel = "R";
    public DenseTexture texture;
    public string filePath = "";
    public string metaData = "";
    public string data = "";

    public UAIFunctionResult()
    {
        if (type == UAIFunctionResultType.IMAGE)
        {
            texture = new DenseTexture();
        }
    }

    public void AddChannel(TextureChannel channel)
    {
        texture.frame.channels.Add(channel);
    }
    public void AddChannel(string labelName, Bitmap texture)
    {
        AddChannel(new TextureChannel() { name = labelName, texture = texture });

    }

}

[System.Serializable]
public enum UAIFunctionResultType 
{
  IMAGE,VIDEO,AUDIO,BINARY,TEXT  

}