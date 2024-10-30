using System;
using System.Collections.Generic;
[System.Serializable]
public partial class UAIFunctionMetadata { 

    public string name;
    public string title;
    public string path;
    public string category;
    public string icon;
    public string modelPath;
    public List<string> tags;
    public string description;
    
    public List<string> inputs;
        
    public List<string> outputs;
    
    public int panel;
    
    public List<UAIFunctionSubwindow> subwindows;
    
    public List<UAIFunction> functions;
    
    public string uuid;
    
    public bool hidden;
    
    public List<string> automatic;
    
    public string documentation;
    
    public bool initialState;
    
    public bool toggleable;
    
    public bool isFX;
    
    public string window;

}