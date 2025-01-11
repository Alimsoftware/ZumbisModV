using System;

namespace ZumbisModV.Interfaces
{
    public interface IUpdatable
    {
        void Tick();

        void Abort();
    }
}
