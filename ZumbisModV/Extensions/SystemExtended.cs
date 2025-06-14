using GTA.Math;
using GTA.Native;

namespace ZumbisModV.Extensions
{
    public static class SystemExtended
    {
        public static float VDist(this Vector3 v, Vector3 to)
        {
            return Function.Call<float>(Hash.VDIST, v.X, v.Y, v.Z, to.X, to.Y, to.Z);
        }
    }
}
