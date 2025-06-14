using GTA;
using GTA.Native;

namespace ZumbisModV.Extensions
{
    public static class VehicleExtended
    {
        public static VehicleClass GetModelClass(Model vehicleModel)
        {
            return Function.Call<VehicleClass>(Hash.GET_VEHICLE_CLASS_FROM_NAME, vehicleModel.Hash);
        }
    }
}
