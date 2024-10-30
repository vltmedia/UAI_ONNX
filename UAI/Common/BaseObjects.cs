

using Newtonsoft.Json;

namespace UAI.Common
{
    public class  Vector
    {
        public float x;
        public float X { get { return x; } set { x = value; } }
    }
    public class VectorI { 
        public int x;
    
        public int X { get { return x; } set { x = value; } }
    
    }
    public class Vector2 : Vector {
    
    public float y;
        public float Y { get { return y; } set { y = value; } }
        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;

        }
    }

    public class Vector2I : VectorI
    {

        public int y;
        public int Y { get { return y; } set { y = value; } }
        public Vector2I(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class Vector3 : Vector2
    {
        public float z;
        public float Z { get { return z; } set { z = value; } }
        public Vector3(float x, float y, float z) : base(x, y)
        {
            this.z = z;
        }
    }

    public class Vector3I : Vector2I
    {
        public int z;
        public int Z { get { return z; } set { z = value; } }
        public Vector3I(int x, int y, int z) : base(x, y)
        {
            this.z = z;
        }
    }

    public class Vector4 : Vector3
    {
        public float w;
        public float W { get { return w; } set { w = value; } }
        public Vector4(float x, float y, float z, float w) : base(x, y, z)
        {
            this.w = w;
        }
    }

    public class Vector4I : Vector3I
    {
        public int w;
        public int W { get { return w; } set { w = value; } }
        public Vector4I(int x, int y, int z, int w) : base(x, y, z)
        {
            this.w = w;
        }
    }

    [System.Serializable]
    public class MediaItem
    {
        public string media = "";
        public string name = "";
        public string type = "";
        public string id = "";
        public string prompt = "";
    }
    [System.Serializable]
    public class MultiMediaItem
    {
        public List<MediaItem> media = new List<MediaItem>();

        public void Save(string path)
        {
            // Save to file
            var stringified = JsonConvert.SerializeObject(this);
            File.WriteAllText(path, stringified);
        }
        public void Load(string path)
        {
            // Load from file
            string json = File.ReadAllText(path);
            MultiMediaItem loaded = JsonConvert.DeserializeObject<MultiMediaItem>(json);
            media.Clear();
            media.AddRange(loaded.media);
        }
        }
}
