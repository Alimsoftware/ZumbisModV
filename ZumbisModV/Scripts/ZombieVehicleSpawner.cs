using System;
using System.Collections.Generic;
using System.Linq;
using GTA;
using GTA.Math;
using GTA.Native;
using ZumbisModV.DataClasses;
using ZumbisModV.Extensions;
using ZumbisModV.Interfaces;
using ZumbisModV.Static;

namespace ZumbisModV.Scripts
{
    public class ZombieVehicleSpawner : Script, ISpawner
    {
        //private const int SpawnBlockingDistance = 150;
        //private const int MaxDistanceFromPlayer = 500; // Limite máximo para spawn de entidades
        private const float WindowSmashChance = 0.5f;
        private const float DoorOpenChance = 0.2f;

        //private readonly int _maxVehicles;
        //private readonly int _maxZombies;
        //private readonly int _spawnDistance;
        //private readonly int _minSpawnDistance;
        //private readonly int _zombieHealth;
        //private bool _nightFall;
        //private List<Ped> _peds;
        //private List<Vehicle> _vehicles;

        // Token: 0x040001C6 RID: 454
        public const int SpawnBlockingDistance = 150;

        // Token: 0x040001C7 RID: 455
        private readonly int _maxVehicles = 10;

        // Token: 0x040001C8 RID: 456
        private readonly int _maxZombies = 30;

        // Token: 0x040001C9 RID: 457
        private readonly int _minVehicles = 1;

        // Token: 0x040001CA RID: 458
        private readonly int _minZombies = 7;

        // Token: 0x040001CB RID: 459
        private readonly int _spawnDistance = 75;

        // Token: 0x040001CC RID: 460
        private readonly int _minSpawnDistance = 50;

        // Token: 0x040001CD RID: 461
        private readonly int _zombieHealth = 750;

        // Token: 0x040001CE RID: 462
        private bool _nightFall;

        // Token: 0x040001CF RID: 463
        private List<Ped> _peds = new List<Ped>();

        // Token: 0x040001D0 RID: 464
        private List<Vehicle> _vehicles = new List<Vehicle>();

        // Token: 0x040001D1 RID: 465
        private readonly VehicleClass[] _classes;

        // Token: 0x040001D2 RID: 466
        public string[] InvalidZoneNames;

        public SpawnBlocker SpawnBlocker { get; }

        public ZombieVehicleSpawner()
        {
            //_maxVehicles = 10; // Defina um valor padrão
            //_maxZombies = 20; // Defina um valor padrão
            //_spawnDistance = 50; // Defina um valor padrão
            //_minSpawnDistance = 10; // Defina um valor padrão
            //_zombieHealth = 100; // Defina um valor padrão
            _peds = new List<Ped>();
            _vehicles = new List<Vehicle>();

            VehicleClass[] array = new VehicleClass[3]
            {
                VehicleClass.Sedans,
                VehicleClass.Compacts,
                VehicleClass.Utility,
            };
            InvalidZoneNames = new string[]
            {
                "Los Santos International Airport",
                "Fort Zancudo",
                "Bolingbroke Penitentiary",
                "Davis Quartz",
                "Palmer-Taylor Power Station",
                "RON Alternates Wind Farm",
                "Terminal",
                "Humane Labs and Research",
            };

            SpawnBlocker = new SpawnBlocker();

            Instance = this;

            _minZombies = Settings.GetValue<int>("spawning", "min_spawned_zombies", _minZombies);
            _maxZombies = Settings.GetValue<int>("spawning", "max_spawned_zombies", _maxZombies);
            _minVehicles = Settings.GetValue<int>("spawning", "min_spawned_vehicles", _minVehicles);
            _maxVehicles = Settings.GetValue<int>("spawning", "max_spawned_vehicles", _maxVehicles);
            _spawnDistance = Settings.GetValue<int>("spawning", "spawn_distance", _spawnDistance);
            _minSpawnDistance = Settings.GetValue<int>(
                "spawning",
                "min_spawn_distance",
                _minSpawnDistance
            );
            _zombieHealth = Settings.GetValue<int>("zombies", "zombie_health", _zombieHealth);
            Settings.SetValue<int>("spawning", "min_spawned_zombies", _minZombies);
            Settings.SetValue<int>("spawning", "max_spawned_zombies", _maxZombies);
            Settings.SetValue<int>("spawning", "min_spawned_vehicles", _minVehicles);
            Settings.SetValue<int>("spawning", "max_spawned_vehicles", _maxVehicles);
            Settings.SetValue<int>("spawning", "spawn_distance", _spawnDistance);
            Settings.SetValue<int>("spawning", "min_spawn_distance", _minSpawnDistance);
            Settings.SetValue<int>("zombies", "zombie_health", _zombieHealth);
            Settings.Save();
            Tick += OnTick;
            Aborted += (sender, args) => ClearAll();

            Interval = 100;
        }

        public bool Spawn { get; set; }

        private static Ped PlayerPed => Database.PlayerPed;
        private static Vector3 PlayerPosition => Database.PlayerPosition;

        public static ZombieVehicleSpawner Instance { get; private set; }

        private void OnTick(object sender, EventArgs e)
        {
            if (Spawn)
            {
                if (!MenuController.MenuPool.AreAnyVisible)
                {
                    if (ZombieCreator.IsNightFall() && !_nightFall)
                    {
                        UiExtended.DisplayHelpTextThisFrame(
                            "O anoitecer se aproxima. Os zumbis são muito mais ~r~agressivos~s~ à noite."
                        );
                        _nightFall = true;
                    }
                    else if (!ZombieCreator.IsNightFall())
                    {
                        _nightFall = false;
                    }
                }
                SpawnVehicles();
                SpawnPeds();
            }
            else
            {
                ClearAll();
            }
        }

        private void SpawnPeds()
        {
            _peds = _peds.Where(Exists).ToList();
            if (_peds.Count >= _maxZombies)
                return;

            for (int i = 0; i < _maxZombies - _peds.Count; i++)
            {
                Vector3 spawnPoint = PlayerPosition.Around(_spawnDistance);
                spawnPoint = World.GetNextPositionOnStreet(spawnPoint);

                if (IsValidSpawn(spawnPoint))
                {
                    Ped randomPed = World.CreateRandomPed(spawnPoint);
                    if (randomPed != null)
                    {
                        _peds.Add(ZombieCreator.InfectPed(randomPed, _zombieHealth));
                    }
                }
            }
        }

        private void SpawnVehicles()
        {
            _vehicles = _vehicles.Where(Exists).ToList();
            if (_vehicles.Count >= _maxVehicles)
                return;

            for (int i = 0; i < _maxVehicles - _vehicles.Count; i++)
            {
                Vector3 spawnPoint = PlayerPosition.Around(_spawnDistance);
                spawnPoint = World.GetNextPositionOnStreet(spawnPoint);

                if (IsInvalidZone(spawnPoint) || !IsValidSpawn(spawnPoint))
                    continue;

                Vector3 vehiclePosition = spawnPoint.Around(2.5f);
                if (
                    vehiclePosition.IsOnScreen()
                    || vehiclePosition.VDist(PlayerPosition) < _minSpawnDistance
                )
                {
                    continue;
                }

                Vehicle vehicle = World.CreateVehicle(
                    Database.GetRandomVehicleModel(),
                    vehiclePosition
                );
                if (vehicle != null)
                {
                    vehicle.EngineHealth = 0.0f;
                    vehicle.DirtLevel = 14f;
                    SmashRandomWindow(vehicle);
                    if (Database.Random.NextDouble() < WindowSmashChance)
                    {
                        SmashRandomWindow(vehicle);
                    }
                    if (Database.Random.NextDouble() < DoorOpenChance)
                    {
                        OpenRandomDoor(vehicle);
                    }

                    vehicle.Heading = Database.Random.Next(1, 360);
                    _vehicles.Add(vehicle);
                }
            }
        }

        private static void OpenRandomDoor(Vehicle vehicle)
        {
            var doors = vehicle.Doors.ToArray();
            var availableDoors = doors.Where(door => !door.IsOpen).ToArray();

            if (availableDoors.Length > 0)
            {
                var randomDoor = availableDoors[Database.Random.Next(availableDoors.Length)];
                randomDoor.Open(instantly: true);
            }
        }

        private static void SmashRandomWindow(Vehicle vehicle)
        {
            // Criar uma lista para armazenar as janelas intactas
            List<VehicleWindow> intactWindows = new List<VehicleWindow>();

            // Iterar pelas janelas e verificar se estão intactas
            foreach (VehicleWindowIndex windowIndex in Enum.GetValues(typeof(VehicleWindowIndex)))
            {
                var window = vehicle.Windows[windowIndex];

                if (window.IsIntact) // Verifica se a janela está intacta
                {
                    intactWindows.Add(window);
                }
            }

            // Se houver janelas intactas
            if (intactWindows.Count > 0)
            {
                // Seleciona uma janela intacta aleatória
                var randomWindow = intactWindows[Database.Random.Next(intactWindows.Count)];

                // Quebra a janela
                randomWindow.Smash();
            }
        }

        public bool IsInvalidZone(Vector3 spawn)
        {
            return InvalidZoneNames.Contains(World.GetZoneLocalizedName(spawn));
        }

        private static bool Exists(Entity entity)
        {
            return entity != null && entity.Exists();
        }

        private void ClearAll()
        {
            foreach (var ped in _peds)
            {
                ped?.Delete();
            }
            _peds.Clear();

            foreach (var vehicle in _vehicles)
            {
                vehicle?.Delete();
            }
            _vehicles.Clear();
        }

        public bool IsValidSpawn(Vector3 spawnPoint)
        {
            return SpawnBlocker.FindIndex(spawn => spawn.VDist(spawnPoint) < 150.0) <= -1;
        }
    }
}
