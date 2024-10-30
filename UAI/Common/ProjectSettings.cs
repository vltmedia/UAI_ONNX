
namespace UAI.Common
{
    public class ProjectSettings
    {
        // Singleton instance
        private static ProjectSettings _singleton;
        public static ProjectSettings Instance => _singleton ??= new ProjectSettings();

        public bool IsChanged { get; private set; }
        public static string ResourcePath { get  {
                return AppPath;
            } }
        public static string TempPath { get {
                var temp = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/UAI";
                if (!System.IO.Directory.Exists(temp))
                {
                    System.IO.Directory.CreateDirectory(temp);
                }
                return temp;
            } }
        public static string AppPath { get {
                return AppDomain.CurrentDomain.BaseDirectory;
            } }
        public string ProjectDataDirName { get; private set; }
        public bool IsProjectLoaded { get; private set; }

        public enum ChannelViewMode
        {
            Original,
            ChannelOnly, ChannelWithColor, ChannelWithTexture, Composite
        }

        public enum ChannelSelectMode
        {
            Single, Find, FindMask, FindColor, FindTexture,
        }

        public class AutoloadInfo
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public bool IsSingleton { get; set; }
        }

        private Dictionary<string, object> _settings = new Dictionary<string, object>();
        private Dictionary<string, PropertyInfo> _customPropInfo = new Dictionary<string, PropertyInfo>();
        private Dictionary<string, AutoloadInfo> _autoloads = new Dictionary<string, AutoloadInfo>();

        // Error simulation (replace this with a more structured error handling if needed)
        public enum Error
        {
            Ok,
            Failed,
            NotFound
        }

        private ProjectSettings() { }

        public void SetSetting(string name, object value)
        {
            _settings[name] = value;
            IsChanged = true;
        }

        public object GetSetting(string name, object defaultValue = null)
        {
            return _settings.TryGetValue(name, out var value) ? value : defaultValue;
        }

        public bool HasSetting(string name)
        {
            return _settings.ContainsKey(name);
        }

        public void AddAutoload(string name, AutoloadInfo autoload)
        {
            _autoloads[name] = autoload;
        }

        public void RemoveAutoload(string name)
        {
            _autoloads.Remove(name);
        }
        public static string GlobalizePath(string name)
        {
            if(name.Contains("user://"))
            {
                return name.Replace("user://", TempPath);
            }
            if(name.Contains("res://"))
            {
                return name.Replace("res://", AppPath);
            }
            return System.IO.Path.GetFullPath(name);
        }

        public bool HasAutoload(string name)
        {
            return _autoloads.ContainsKey(name);
        }

        public AutoloadInfo GetAutoload(string name)
        {
            return _autoloads.TryGetValue(name, out var autoload) ? autoload : null;
        }

        public Error LoadSettingsText(string path)
        {
            // Implement loading logic (e.g., parse file as JSON)
            return Error.Ok;
        }

        public Error SaveSettingsText(string path)
        {
            // Implement saving logic (e.g., serialize to JSON)
            return Error.Ok;
        }

        public void ClearSetting(string name)
        {
            if (_settings.ContainsKey(name))
            {
                _settings.Remove(name);
                IsChanged = true;
            }
        }

        // Replace GLOBAL_DEF macros with helper methods
        public static object GlobalDef(string name, object defaultValue, bool restartIfChanged = false)
        {
            return Instance.GetSetting(name, defaultValue) ?? defaultValue;
        }
    }

    public class PropertyInfo
    {
        public string Name { get; set; }
        public Type PropertyType { get; set; }
    }
}
