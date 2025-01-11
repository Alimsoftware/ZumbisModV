using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;

namespace ZumbisModV.Interfaces
{
    public interface IAnimatable
    {
        string AnimationDict { get; set; }

        string AnimationName { get; set; }

        AnimationFlags AnimationFlags { get; set; }

        int AnimationDuration { get; set; }
    }
}
