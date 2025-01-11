using System;

namespace ZumbisModV.Interfaces
{
    public interface IValidatable
    {
        Func<bool> Validation { get; set; }
    }
}
