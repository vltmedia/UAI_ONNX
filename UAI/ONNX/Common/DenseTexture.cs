

using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using TinyEXR;
using UAI.UI;

public partial class DenseTexture 
{

    public TextureFrame frame = new TextureFrame();
    public Bitmap baseTexture { get { return frame.beauty; } set { frame.beauty = value; } }
    public Bitmap texture { get { return channelViewMode == ChannelViewMode.Original ? baseTexture : frame.channels[channelIndex].texture; }  set { baseTexture = value; } }
    
    public int channelIndex = 0;
    
    public ChannelViewMode channelViewMode = ChannelViewMode.Original;
    
    public ChannelSelectMode channelSelectMode = ChannelSelectMode.Single;
    
    public delegate void OnAlphaFoundEventHandler(Bitmap tex);
    // Called when the node enters the scene tree for the first time.
   
    public Bitmap GetTexture()
    {
        return texture;
    }
    public event EventHandler<Bitmap> OnAlphaFound;

    private void EmitSignal(Bitmap tex)
    {
        OnAlphaFound?.Invoke(this, tex);
    }
    public void Clone(DenseTexture other)
    {
        frame = other.frame;
        baseTexture = other.baseTexture;
        texture = other.texture;
        channelIndex = other.channelIndex;
        channelViewMode = other.channelViewMode;
        channelSelectMode = other.channelSelectMode;
    }
    //public void UpdateTexture(Bitmap tex)
    //{
    //    Image image = tex.GetImage();
    //}
    public void UpdateTexture(Bitmap tex)
    {
        texture = tex;
    }
    public void SplitBitmap(ref ScanlineExrWriter scanlineExrWriter, Bitmap texture, string Name)
    {
        SplitBitmap(ref scanlineExrWriter, baseTexture, new System.Collections.Generic.Dictionary<string, string> {
         { "R", $"{Name}.R" }, { "G", $"{Name}.G" }, { "B", $"{Name}.B" }, { "A", $"{Name}.A" }
        });

    }
    public void SplitBitmap(ref ScanlineExrWriter scanlineExrWriter, Bitmap texture, string Name, List<string> channels)
    {
        System.Collections.Generic.Dictionary<string, string> dict = new System.Collections.Generic.Dictionary<string, string>();

        for (int i = 0; i < channels.Count; i++)
        {
            dict.Add(channels[i], $"{Name}.{channels[i]}");
        }
        SplitBitmap(ref scanlineExrWriter, baseTexture, dict);


    }
    public void SplitBitmap(ref ScanlineExrWriter scanlineExrWriter, Bitmap texture, System.Collections.Generic.Dictionary<string, string> channels)
    {

        int width = texture.Width;
        int height = texture.Height;
        byte[] r = new byte[width * height];
        byte[] g = new byte[width * height];
        byte[] b = new byte[width * height];
        byte[] a = new byte[width * height];
        string RName = "";
        string GName = "";
        string BName = "";
        string AName = "";
        channels.TryGetValue("R", out  RName);
        channels.TryGetValue("G", out  GName);
        channels.TryGetValue("B", out  BName);
        channels.TryGetValue("A", out  AName);
        bool useR = channels.ContainsKey("R");
        bool useG = channels.ContainsKey("G");
        bool useB = channels.ContainsKey("B");
        bool useA = channels.ContainsKey("A");
        var image = texture;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Get the color of the pixel at (x, y)

                Color pixel = image.GetPixel(x, y);

                // Convert each component to a byte and store it in the respective array
                int index = y * width + x;
                if (useR)
                {
                    r[index] = (byte)(pixel.R * 255);
                }
                if (useG)
                {
                    g[index] = (byte)(pixel.G * 255);
                }
                if (useB)
                {
                    b[index] = (byte)(pixel.B * 255);
                }
                if (useA)
                {
                    a[index] = (byte)(pixel.A * 255);
                }

            }
        }
        if (useR)
        {
            scanlineExrWriter.AddChannel(RName, ExrPixelType.Float, r, ExrPixelType.Float);
        }
        if (useG)
        {
            scanlineExrWriter.AddChannel(GName, ExrPixelType.Float, g, ExrPixelType.Float);
        }
        if (useB)
        {
            scanlineExrWriter.AddChannel(BName, ExrPixelType.Float, b, ExrPixelType.Float);
        }
        if (useA)
        {
            scanlineExrWriter.AddChannel(AName, ExrPixelType.Float, a, ExrPixelType.Float);
        }

    }
    public void SaveEXR(string path)
    {
        ScanlineExrWriter scanlineExrWriter = new TinyEXR.ScanlineExrWriter();
        SplitBitmap(ref scanlineExrWriter, baseTexture, "Beauty");

        for (int i = 0; i < frame.channels.Count; i++)
        {
            SplitBitmap(ref scanlineExrWriter, frame.channels[i].texture, frame.channels[i].name, new List<string>() {"R"});

        }
        scanlineExrWriter.SetSize(frame.channels[0].texture.Width, frame.channels[0].texture.Height);
        scanlineExrWriter.Save(path);
    }
    public void LoadEXR(string path)
    {
        //Scanl scanlineExrReader = new TinyEXR.ScanlineExrReader();
        //scanlineExrReader.Load(path);
        //baseTexture = new Bitmap();
        //baseTexture.CreateFromImage(new Image(scanlineExrReader.GetChannel("Beauty")));
        //for (int i = 0; i < scanlineExrReader.GetChannelCount(); i++)
        //{
        //    TextureChannel channel = new TextureChannel();
        //    channel.texture = new Bitmap();
        //    channel.texture.CreateFromImage(new Image(scanlineExrReader.GetChannel(i)));
        //    channel.name = scanlineExrReader.GetChannelName(i);
        //    frame.channels.Add(channel);
        //}
    }
    public void ProcessTexturesClicked( Vector2 localPos, Vector2 RectSize, Vector2 mousePos , bool RedFallback = false, PopupMenu popupMenu = null)
    {
        List<Bitmap> list = new List<Bitmap>();

        int i = 0;
        if (popupMenu != null)
        {
            // Show the popup menu at the mouse position
            popupMenu.Clear();
        }
        foreach (var tex in frame.channels)
        {
            if (TextureNotBlackAtPoint(tex.texture, localPos, RectSize, RedFallback))
            {
                list.Add(tex.texture);
                if (popupMenu != null)
                {
                    // Show the popup menu at the mouse position
                    popupMenu.AddItem(tex.name, i);
                }
            }
            i++;
        }
        //if (list.Count > 0)
        //{
        //    foreach (var tex in list)
        //    {
        //        var indx = frame.channels.FindIndex(x => x.texture == tex);
        //        if (indx != channelIndex)
        //        {

        //            channelIndex = indx;

                 
        //            EmitSignal(SignalName.OnAlphaFound, tex);
        //            return;
        //        }
        //    }
        //    EmitSignal(SignalName.OnAlphaFound, frame.channels[channelIndex].texture);
        //}
        if (popupMenu != null)
        {

            // Show the popup menu at the mouse position
            popupMenu.SetPosition(mousePos);
            // Show the popup menu at the mouse position
            popupMenu.Popup();
        }
        EmitSignal(frame.channels[channelIndex].texture);
    }
    public void ProcessTextureClicked(Bitmap tex, Vector2 localPos, Vector2 RectSize, bool RedFallback = false)
    {
        if (TextureHasAlphaAtPoint(tex, localPos, RectSize, RedFallback))
        {
            EmitSignal(tex);
        }
    }

    public bool TextureHasAlphaAtPoint(Bitmap tex,Vector2 localPos, Vector2 RectSize, bool RedFallback = false)
    {
        // Check if the texture exists and the mouse is inside the texture bounds
        if (tex != null && localPos.X >= 0 && localPos.Y >= 0 && localPos.X < RectSize.X && localPos.Y < RectSize.Y)
        {
            // Get the UV coordinates relative to the texture
            Vector2 uv = localPos / RectSize * new Vector2(tex.Size.Width, tex.Height);
            Vector2 Vector2I = new Vector2((int)uv.X, (int)uv.Y);
            // Get the texture image (make sure to create an image from the texture)
            var image = tex;

            if (image != null)
            {
                // Lock the image to read pixel data

                Color pixelColor = image.GetPixel((int)Vector2I.X, (int)Vector2I.Y);

                // Unlock the image after reading the pixel

                // Check if the alpha value is greater than 0
                if (pixelColor.A > 0.0f)
                {
                    // Handle the click here (e.g., printing or custom logic)
                    return true;
                }
                else if (RedFallback)
                {
                   if (pixelColor.R > 0.0f)
                    {
                        return true;
                    }
                }
                else
                {
                    return false;

                }
                return false;

            }
        }
        return false;

    }

    public bool TextureNotBlackAtPoint(Bitmap tex,Vector2 localPos, Vector2 RectSize, bool RedFallback = false)
    {
        // Check if the texture exists and the mouse is inside the texture bounds
        if (tex != null && localPos.X >= 0 && localPos.Y >= 0 && localPos.X < RectSize.X && localPos.Y < RectSize.Y)
        {
            // Get the UV coordinates relative to the texture
            Vector2 uv = localPos / RectSize * new Vector2(tex.Size.Width, tex.Height);
            Vector2 Vector2I = new Vector2((int)uv.X, (int)uv.Y);
            // Get the texture image (make sure to create an image from the texture)
            var image = tex;

            if (image != null)
            {
                // Lock the image to read pixel data

                Color pixelColor = image.GetPixel((int)Vector2I.X, (int)Vector2I.Y);

                // Unlock the image after reading the pixel

                // Check if the alpha value is greater than 0
                if (pixelColor.G > 0.0f)
                {
                    // Handle the click here (e.g., printing or custom logic)
                    return true;
                }
                else if (RedFallback)
                {
                   if (pixelColor.R > 0.0f)
                    {
                        return true;
                    }
                }
                else
                {
                    return false;

                }
                return false;

            }
        }
        return false;

    }

   
}
