using System;

namespace ZumbisModV.Interfaces
{
    public interface ICraftable
    {
        CraftableItemComponent[] RequiredComponents { get; set; }
    }
}
