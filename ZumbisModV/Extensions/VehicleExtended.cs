﻿using GTA;
using GTA.Native;

namespace ZumbisModV.Extensions
{
    public static class VehicleExtended
    {
        public static VehicleClass GetModelClass(Model vehicleModel)
        {
            return (VehicleClass)
                Function.Call<int>(
                    Hash.GET_VEHICLE_CLASS_FROM_NAME,
                    new InputArgument[] { vehicleModel.Hash }
                );
        }
    }
}
