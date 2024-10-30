using Emgu.CV.Cuda;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Stream = System.IO.Stream;

public enum ChannelViewMode
{
    Original,
    ChannelOnly, ChannelWithColor, ChannelWithTexture, Composite
}

public enum ChannelSelectMode
{
    Single, Find, FindMask, FindColor, FindTexture,
}


[System.Serializable]
public class TextureFrame
{
    public Bitmap beauty = null;
    public List<TextureChannel> channels = new List<TextureChannel>();
    public void SetBeauty(Bitmap beauty)
    {
        this.beauty = beauty;
    }
    public void AddChannel(TextureChannel channel)
    {
        channels.Add(channel);
    }
    public void RemoveChannel(TextureChannel channel)
    {
        channels.Remove(channel);
    }
    public void RemoveChannel(int index)
    {
        channels.RemoveAt(index);
    }
    public void ClearChannels()
    {
        channels.Clear();
    }
    public List<TextureChannel> GetChannels()
    {
        return channels;
    }
    public void SetChannels(List<TextureChannel> channels)
    {
        this.channels = channels;
    }
    public TextureChannel GetChannel(int index)
    {
        return channels[index];
    }
    public void SetChannel(int index, TextureChannel channel)
    {
        channels[index] = channel;
    }
    public int GetChannelCount()
    {
        return channels.Count;
    }
    public void CreateChannel(Bitmap tex, string name)
    {
        TextureChannel channel = new TextureChannel(tex, name);
        channels.Add(channel);
    }
    public void CreateChannel(TextureChannel channel)
    {
        channels.Add(channel);
    }

    public TextureChannel GetChannelByName(string name)
    {
       return channels.Find(x => x.name == name);

    }
    public TextureChannel GetChannelById(string id)
    {
        return channels.Find(x => x.id == id);

    }
    public void RemoveChannelByName(string name)
    {
        channels.RemoveAll(x => x.name == name);
    }
    public void RemoveChannelById(string id)
    {
        channels.RemoveAll(x => x.id == id);
    }

   
    public void Save(string path)
    {
       SerializableTextureFrame frame = new SerializableTextureFrame(this);
        var ser = JsonConvert.SerializeObject(frame);
        File.WriteAllText(path, ser);
    }
    public void Load(string path)
    {
        string json = File.ReadAllText(path);
        SerializableTextureFrame frame = JsonConvert.DeserializeObject<SerializableTextureFrame>(json);
        var converted = frame.ToTextureFrame();
         channels.Clear();
        channels.AddRange(converted.channels);
        beauty = converted.beauty;

    }


}
[System.Serializable]
public  class TextureChannel 
{
    public Bitmap texture = null;
    public string name = "";
    public string id = "";
    public System.Collections.Generic.Dictionary<string, object> metadata = new System.Collections.Generic.Dictionary<string, object>();

    public void SetData(TextureChannel data)
    {
        texture = data.texture;
        name = data.name;
        id = data.id;
    }

    public TextureChannel GetData()
    {
        return this;
    }

    public void SetTexture(Bitmap texture)
    {
        this.texture = texture;
    }

    public Bitmap GetTexture()
    {
        return this.texture;
    }

    public void SetName(string name)
    {
        this.name = name;
    }

    public string GetName()
    {
        return this.name;
    }

    public void SetId(string id)
    {
        this.id = id;
    }
    public string GetId()
    {


        return this.id;
    }

    public TextureChannel()
    {
        Guid newUuid = Guid.NewGuid();
        this.id = newUuid.ToString();

    }

    public TextureChannel(Bitmap texture, string name)
    {
        Guid newUuid = Guid.NewGuid();
        this.texture = texture;
        this.name = name;
    }

}

[Serializable]
public class SerializableTextureChannel
{
    public byte[] textureData; // This will hold the Bitmap as a byte array
    public string name;
    public string id;
    public System.Collections.Generic.Dictionary<string, object> metadata = new System.Collections.Generic.Dictionary<string, object>();

    public static byte[] BitmapToByteArray(Bitmap texture)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            texture.Save(ms, System.Drawing.Imaging.ImageFormat.Png); // Save Bitmap to PNG format
            return ms.ToArray();
        }
    }
    public static Bitmap ByteArrayToBitmap(byte[] data)
    {
        using (MemoryStream ms = new MemoryStream(data))
        {
            return new Bitmap(ms); // Load Bitmap from byte array
        }
    }
    public SerializableTextureChannel()
    {
    }
    public SerializableTextureChannel(TextureChannel channel)
    {
        textureData = BitmapToByteArray(channel.texture); // Convert texture to byte array
        name = channel.name;
        id = channel.id;
        metadata = channel.metadata;
    }

    public TextureChannel ToTextureChannel()
    {
        TextureChannel channel = new TextureChannel();
        channel.texture = ByteArrayToBitmap(textureData); // Convert byte array back to Bitmap
        channel.name = name;
        channel.id = id;
        channel.metadata = metadata;
        return channel;
    }
    public static byte[] Texture2DToByteArray(Bitmap texture)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            texture.Save(ms, System.Drawing.Imaging.ImageFormat.Png); // Save Bitmap to PNG format
            return ms.ToArray();
        }
    }
}

[Serializable]
public class SerializableTextureFrame
{
    public byte[] beautytextureData; // This will hold the Bitmap as a byte array
    public List<SerializableTextureChannel> channels = new List<SerializableTextureChannel>();

    public SerializableTextureFrame()
    {

    }
    public SerializableTextureFrame(TextureFrame frame)
    {
        foreach (var channel in frame.channels)
        {
            channels.Add(new SerializableTextureChannel(channel));
        }
        if (frame.beauty != null)
        {
            beautytextureData = SerializableTextureChannel.BitmapToByteArray(frame.beauty);
        }
    }


    public TextureFrame ToTextureFrame()
    {
        TextureFrame frame = new TextureFrame();
        foreach (var serializableChannel in channels)
        {
            frame.channels.Add(serializableChannel.ToTextureChannel());
        }
        if (beautytextureData != null)
        {
            frame.beauty = SerializableTextureChannel.ByteArrayToBitmap(beautytextureData);
        }
        return frame;
    }
}



