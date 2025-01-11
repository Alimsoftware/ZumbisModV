using System.Drawing;
using GTA.Native;
using ZumbisModV.Interfaces;

namespace ZumbisModV.DataClasses
{
    public class ParticleEffect : IHandleable, IDeletable
    {
        internal ParticleEffect(int handle) => Handle = handle;

        public int Handle { get; }

        public Color Color
        {
            set
            {
                Function.Call(
                    Hash.SET_PARTICLE_FX_LOOPED_COLOUR,
                    Handle,
                    value.R,
                    value.G,
                    value.B,
                    true
                );
            }
        }

        public bool Exists()
        {
            return Function.Call<bool>(Hash.DOES_PARTICLE_FX_LOOPED_EXIST, Handle);
        }

        public void Delete()
        {
            Function.Call(Hash.REMOVE_PARTICLE_FX, Handle, 1);
        }
    }
}
