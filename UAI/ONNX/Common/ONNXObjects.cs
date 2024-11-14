using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[System.Serializable]
public enum SelectState
    {
        None,
        Selected,
        Unselected
    }

[System.Serializable]
public enum FramesState
    {
        Image,
        Frames,
        Video
    }
