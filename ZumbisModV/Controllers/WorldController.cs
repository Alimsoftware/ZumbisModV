using System;
using System.Linq;
using GTA;
using GTA.Math;
using GTA.Native;
using ZumbisModV.Extensions;
using ZumbisModV.Static;

namespace ZumbisModV.Controllers
{
    public class WorldController : Script
    {
        private bool _reset;

        public WorldController()
        {
            Tick += new EventHandler(OnTick);
            Aborted += new EventHandler(WorldController.OnAborted);
        }

        public static bool Configure { get; set; }

        public static bool StopPedsFromSpawning { get; set; }

        public Vector3 PlayerPosition => Database.PlayerPosition;

        private static void OnAborted(object sender, EventArgs e) => WorldController.Reset();

        private static void Reset()
        {
            Function.Call(Hash.CAN_CREATE_RANDOM_COPS, true);
            Function.Call(Hash.SET_RANDOM_BOATS, true);
            Function.Call(Hash.SET_RANDOM_TRAINS, true);
            Function.Call(Hash.SET_GARBAGE_TRUCKS, true);
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (Configure)
            {
                WorldExtended.ClearCops(10000f);
                WorldExtended.SetScenarioPedDensityThisMultiplierFrame(0.0f);
                WorldExtended.SetVehicleDensityMultiplierThisFrame(0.0f);
                WorldExtended.SetRandomVehicleDensityMultiplierThisFrame(0.0f);
                WorldExtended.SetParkedVehicleDensityMultiplierThisFrame(0.0f);
                WorldExtended.SetPedDensityThisMultiplierFrame(0.0f);
                WorldExtended.SetScenarioPedDensityThisMultiplierFrame(0.0f);
                Game.MaxWantedLevel = 0;
                Vehicle[] allVehicles = World
                    .GetAllVehicles()
                    .Where(v => !v.IsPersistent)
                    .ToArray();
                Vehicle[] planes = Array.FindAll(
                    allVehicles,
                    v => v.ClassType == VehicleClass.Planes
                );
                Vehicle[] trains = Array.FindAll(
                    allVehicles,
                    v => v.ClassType == VehicleClass.Trains
                );
                Array.ForEach(
                    Array.FindAll(
                        allVehicles,
                        v => v.Driver != null && v.Driver.Exists() && !v.Driver.IsPlayer
                    ),
                    vehicle => vehicle.Delete()
                );

                Array.ForEach(
                    planes,
                    plane =>
                    {
                        if (
                            plane.Driver == null
                            || !plane.Driver.Exists()
                            || plane.Driver.IsPlayer
                            || plane.Driver.IsDead
                        )
                            return;

                        plane.Driver.Kill();
                    }
                );

                Array.ForEach(
                    trains,
                    train => Function.Call(Hash.SET_TRAIN_SPEED, train.Handle, 0.0f)
                );
                ScriptExtended.TerminateScriptByName("re_prison");
                ScriptExtended.TerminateScriptByName("am_prison");
                ScriptExtended.TerminateScriptByName("gb_biker_free_prisoner");
                ScriptExtended.TerminateScriptByName("re_prisonvanbreak");
                ScriptExtended.TerminateScriptByName("am_vehicle_spawn");
                ScriptExtended.TerminateScriptByName("am_taxi");
                ScriptExtended.TerminateScriptByName("audiotest");
                ScriptExtended.TerminateScriptByName("freemode");
                ScriptExtended.TerminateScriptByName("re_prisonerlift");
                ScriptExtended.TerminateScriptByName("am_prison");
                ScriptExtended.TerminateScriptByName("re_lossantosintl");
                ScriptExtended.TerminateScriptByName("re_armybase");
                ScriptExtended.TerminateScriptByName("restrictedareas");
                ScriptExtended.TerminateScriptByName("stripclub");
                ScriptExtended.TerminateScriptByName("re_gangfight");
                ScriptExtended.TerminateScriptByName("re_gang_intimidation");
                ScriptExtended.TerminateScriptByName("spawn_activities");
                ScriptExtended.TerminateScriptByName("am_vehiclespawn");
                ScriptExtended.TerminateScriptByName("traffick_air");
                ScriptExtended.TerminateScriptByName("traffick_ground");
                ScriptExtended.TerminateScriptByName("emergencycall");
                ScriptExtended.TerminateScriptByName("emergencycalllauncher");
                ScriptExtended.TerminateScriptByName("clothes_shop_sp");
                ScriptExtended.TerminateScriptByName("gb_rob_shop");
                ScriptExtended.TerminateScriptByName("gunclub_shop");
                ScriptExtended.TerminateScriptByName("hairdo_shop_sp");
                ScriptExtended.TerminateScriptByName("re_shoprobbery");
                ScriptExtended.TerminateScriptByName("shop_controller");
                ScriptExtended.TerminateScriptByName("re_crashrescue");
                ScriptExtended.TerminateScriptByName("re_rescuehostage");
                ScriptExtended.TerminateScriptByName("fm_mission_controller");
                ScriptExtended.TerminateScriptByName("player_scene_m_shopping");
                ScriptExtended.TerminateScriptByName("shoprobberies");
                ScriptExtended.TerminateScriptByName("re_atmrobbery");
                ScriptExtended.TerminateScriptByName("ob_vend1");
                ScriptExtended.TerminateScriptByName("ob_vend2");
                Function.Call(Hash.STOP_ALARM, "PRISON_ALARMS", 0);
                Function.Call(
                    Hash.CLEAR_AMBIENT_ZONE_STATE,
                    "AZ_COUNTRYSIDE_PRISON_01_ANNOUNCER_GENERAL",
                    0,
                    0
                );
                Function.Call(
                    Hash.CLEAR_AMBIENT_ZONE_STATE,
                    "AZ_COUNTRYSIDE_PRISON_01_ANNOUNCER_WARNING",
                    0,
                    0
                );
                Function.Call(
                    Hash.SET_STATE_OF_CLOSEST_DOOR_OF_TYPE,
                    Function.Call<int>(Hash.GET_HASH_KEY, "prop_gate_prison_01"),
                    1845f,
                    2605f,
                    45f,
                    false,
                    0,
                    0
                );
                Function.Call(
                    Hash.SET_LOCKED_UNSTREAMED_IN_DOOR_OF_TYPE,
                    Function.Call<int>(Hash.GET_HASH_KEY, "prop_gate_prison_01"), // Pegando o hash da prop
                    1819.27f, // Coordenada X
                    2608.53f, // Coordenada Y
                    44.61f, // Coordenada Z
                    false, // Se a porta está bloqueada
                    0, // Tipo de modelo ou outra identificação
                    0 // Outro valor (possivelmente relacionado ao estado ou operação da porta)
                );

                if (!_reset)
                    return;
                Function.Call(Hash.CAN_CREATE_RANDOM_COPS, false);
                Function.Call(Hash.SET_RANDOM_BOATS, false);
                Function.Call(Hash.SET_RANDOM_TRAINS, false);
                Function.Call(Hash.SET_GARBAGE_TRUCKS, false);
                Function.Call(Hash.SET_DISTANT_CARS_ENABLED, false);
                _reset = false;
            }
            else
            {
                if (_reset)
                    return;
                Reset();
                _reset = true;
            }
        }
    }
}
