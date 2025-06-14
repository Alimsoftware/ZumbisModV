using System;
using System.Linq;
using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;
using ZumbisModV.Extensions;
using ZumbisModV.Static;

namespace ZumbisModV.Controllers
{
    public class WorldController : Script
    {
        private bool _reset;

        public WorldController()
        {
            Tick += OnTick;
            Aborted += OnAborted;
        }

        public static bool Configure { get; set; }
        public static bool Blackout { get; set; }
        public static bool StopPedsFromSpawning { get; set; }

        public Vector3 PlayerPosition => Database.PlayerPosition;

        private static void OnAborted(object sender, EventArgs e) => Reset();

        private static void Reset()
        {
            Function.Call(Hash.CAN_CREATE_RANDOM_COPS, true);
            Function.Call(Hash.SET_RANDOM_BOATS, true);
            Function.Call(Hash.SET_RANDOM_TRAINS, true);
            Function.Call(Hash.SET_GARBAGE_TRUCKS, true);
        }

        private void BlackoutOn(bool value)
        {
            RemoveAmbientSounds();
            Function.Call(Hash.SET_ARTIFICIAL_VEHICLE_LIGHTS_STATE, !value);
            World.Blackout = value;
            Function.Call(Hash.SET_STATIC_EMITTER_ENABLED, "LOS_SANTOS", !value);

            //Notification.PostTicker(
            //   string.Format("Blackout {0}", value ? "~g~ativado" : "~r~desativado"),
            //    true,
            //    false
            // );

            OverrideVehicleLights(value);
        }

        private void OverrideVehicleLights(bool vehicleLightsOff)
        {
            foreach (Vehicle vehicle in World.GetAllVehicles(Array.Empty<Model>()))
            {
                if (vehicle == null || !vehicle.IsEngineRunning)
                    continue;

                if (vehicleLightsOff)
                {
                    Function.Call(Hash.SET_VEHICLE_LIGHTS, vehicle.Handle, 2); // Turn on normal
                    Function.Call(Hash.SET_VEHICLE_LIGHT_MULTIPLIER, vehicle.Handle, 10f);
                    Function.Call(Hash.SET_VEHICLE_HEADLIGHT_SHADOWS, vehicle.Handle, 3);
                }
            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            BlackoutOn(Blackout);
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

                /*Vehicle[] allVehicles = World
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
                );*/



                // ... other code

                var allVehicles = World.GetAllVehicles().Where(v => !v.IsPersistent).ToArray();

                var planes = allVehicles.Where(v => v.ClassType == VehicleClass.Planes).ToArray();
                var trains = allVehicles.Where(v => v.ClassType == VehicleClass.Trains).ToArray();
                var drivers = allVehicles
                    .Where(v => v.Driver?.Exists() == true && !v.Driver.IsPlayer)
                    .ToArray();

                foreach (var vehicle in drivers)
                {
                    vehicle.Delete();
                }

                foreach (var plane in planes)
                {
                    if (
                        plane.Driver?.Exists() == true
                        && !plane.Driver.IsPlayer
                        && !plane.Driver.IsDead
                    )
                    {
                        plane.Driver.Kill();
                    }
                }

                foreach (var train in trains)
                {
                    Function.Call(Hash.SET_TRAIN_SPEED, train.Handle, 0.0f);
                }

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

                if (_reset)
                {
                    Function.Call(Hash.CAN_CREATE_RANDOM_COPS, false);
                    Function.Call(Hash.SET_RANDOM_BOATS, false);
                    Function.Call(Hash.SET_RANDOM_TRAINS, false);
                    Function.Call(Hash.SET_GARBAGE_TRUCKS, false);
                    Function.Call(Hash.SET_DISTANT_CARS_ENABLED, false);
                    _reset = false;
                }
            }
            else if (!_reset)
            {
                Reset();
                _reset = true;
            }
        }

        private void RemoveAmbientSounds()
        {
            Function.Call(
                Hash.SET_VEHICLE_RADIO_ENABLED,
                Game.Player.Character.CurrentVehicle,
                false
            );
            Function.Call<bool>(
                Hash.START_AUDIO_SCENE,
                "DLC_MPHEIST_TRANSITION_TO_APT_FADE_IN_RADIO_SCENE"
            );
            Function.Call<bool>(
                Hash.SET_STATIC_EMITTER_ENABLED,
                "LOS_SANTOS_VANILLA_UNICORN_01_STAGE",
                false
            );
            Function.Call<bool>(
                Hash.SET_STATIC_EMITTER_ENABLED,
                "LOS_SANTOS_VANILLA_UNICORN_02_MAIN_ROOM",
                false
            );
            Function.Call<bool>(
                Hash.SET_STATIC_EMITTER_ENABLED,
                "LOS_SANTOS_VANILLA_UNICORN_03_BACK_ROOM",
                false
            );
            Function.Call<bool>(
                Hash.SET_AMBIENT_ZONE_LIST_STATE_PERSISTENT,
                "AZL_DLC_Hei4_Island_Disabled_Zones",
                false,
                true
            );
            Function.Call<bool>(
                Hash.SET_AMBIENT_ZONE_LIST_STATE_PERSISTENT,
                "AZL_DLC_Hei4_Island_Zones",
                true,
                true
            );
            Function.Call<bool>(Hash.SET_SCENARIO_TYPE_ENABLED, "WORLD_VEHICLE_STREETRACE", false);
            Function.Call<bool>(
                Hash.SET_SCENARIO_TYPE_ENABLED,
                "WORLD_VEHICLE_SALTON_DIRT_BIKE",
                false
            );
            Function.Call<bool>(Hash.SET_SCENARIO_TYPE_ENABLED, "WORLD_VEHICLE_SALTON", false);
            Function.Call<bool>(
                Hash.SET_SCENARIO_TYPE_ENABLED,
                "WORLD_VEHICLE_POLICE_NEXT_TO_CAR",
                false
            );
            Function.Call<bool>(Hash.SET_SCENARIO_TYPE_ENABLED, "WORLD_VEHICLE_POLICE_CAR", false);
            Function.Call<bool>(Hash.SET_SCENARIO_TYPE_ENABLED, "WORLD_VEHICLE_POLICE_BIKE", false);
            Function.Call<bool>(
                Hash.SET_SCENARIO_TYPE_ENABLED,
                "WORLD_VEHICLE_MILITARY_PLANES_SMALL",
                false
            );
            Function.Call<bool>(
                Hash.SET_SCENARIO_TYPE_ENABLED,
                "WORLD_VEHICLE_MILITARY_PLANES_BIG",
                false
            );
            Function.Call<bool>(Hash.SET_SCENARIO_TYPE_ENABLED, "WORLD_VEHICLE_MECHANIC", false);
            Function.Call<bool>(Hash.SET_SCENARIO_TYPE_ENABLED, "WORLD_VEHICLE_EMPTY", false);
            Function.Call<bool>(Hash.SET_SCENARIO_TYPE_ENABLED, "WORLD_VEHICLE_BUSINESSMEN", false);
            Function.Call<bool>(
                Hash.SET_SCENARIO_TYPE_ENABLED,
                "WORLD_VEHICLE_BIKE_OFF_ROAD_RACE",
                false
            );
            Function.Call<bool>(Hash.START_AUDIO_SCENE, "FBI_HEIST_H5_MUTE_AMBIENCE_SCENE");
            Function.Call<bool>(Hash.START_AUDIO_SCENE, "CHARACTER_CHANGE_IN_SKY_SCENE");
            Function.Call<bool>(Hash.SET_AUDIO_FLAG, "PoliceScannerDisabled", true);
            Function.Call<bool>(Hash.SET_AUDIO_FLAG, "DisableFlightMusic", true);
            Function.Call<bool>(Hash.SET_RANDOM_EVENT_FLAG, false);
        }
    }
}
