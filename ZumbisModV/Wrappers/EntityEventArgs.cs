using System;
using GTA;

namespace ZumbisModV.Wrappers
{
    public class EntityEventArgs : EventArgs
    {
        public EntityEventArgs(Entity entity)
        {
            Entity = entity;
        }

        public Entity Entity { get; }
    }
}
