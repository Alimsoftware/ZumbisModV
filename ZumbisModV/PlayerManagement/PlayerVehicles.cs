using System;
using System.Collections.Generic;
using System.Linq;
using GTA;
using GTA.Native;
using GTA.UI;
using ZumbisModV.Static;
using ZumbisModV.Wrappers;

namespace ZumbisModV.PlayerManagement
{
    public class PlayerVehicles : Script
    {
        private VehicleCollection _vehicleCollection;
        private List<VehicleMod> VehicleMods;
        private readonly List<Vehicle> _vehicles = new List<Vehicle>();
        public static PlayerVehicles Instance { get; private set; }

        public PlayerVehicles()
        {
            Instance = this;
            Aborted += OnAborted;
        }

        private void OnAborted(object sender, EventArgs eventArgs)
        {
            _vehicleCollection
                .ToList()
                .ForEach(
                    (VehicleData vehicle) =>
                    {
                        vehicle.Delete();
                    }
                );
        }

        public void Deserialize()
        {
            try
            {
                if (_vehicleCollection == null)
                {
                    VehicleCollection vehicleCollection;

                    try
                    {
                        vehicleCollection = Serializer.Deserialize<VehicleCollection>(
                            "./scripts/Vehicles.dat"
                        );
                    }
                    catch (Exception ex)
                    {
                        Notification.Show("Erro ao carregar os dados dos veículos: " + ex.Message);
                        vehicleCollection = new VehicleCollection();
                    }

                    _vehicleCollection = vehicleCollection ?? new VehicleCollection();

                    _vehicleCollection.ListChanged += (VehicleCollection sender) =>
                        Serialize(false);

                    foreach (VehicleData vehicleData in _vehicleCollection)
                    {
                        Vehicle vehicle = World.CreateVehicle(
                            vehicleData.Hash,
                            vehicleData.Position
                        );
                        VehicleModCollection mods = vehicle.Mods;
                        if (vehicle == null)
                        {
                            Notification.Show("Falha ao carregar o veículo.");
                            continue;
                        }
                        try
                        {
                            mods.PrimaryColor = vehicleData.PrimaryColor;
                            mods.SecondaryColor = vehicleData.SecondaryColor;
                            vehicle.Health = vehicleData.Health;
                            vehicle.EngineHealth = vehicleData.EngineHealth;
                            vehicle.Rotation = vehicleData.Rotation;
                            vehicleData.Handle = vehicle.Handle;

                            AddKit(vehicle, vehicleData);

                            AddBlipToVehicle(vehicle);
                            _vehicles.Add(vehicle);
                            vehicle.IsPersistent = true;

                            EntityEventWrapper entityEventWrapper = new EntityEventWrapper(vehicle);
                            entityEventWrapper.Died += WrapperOnDied;
                        }
                        catch (Exception ex)
                        {
                            Notification.Show("Erro ao aplicar dados ao veículo: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Notification.Show("Erro geral ao carregar os veículos: " + ex.Message);
            }
        }

        private static void AddKit(Vehicle vehicle, VehicleData data)
        {
            if (data != null)
            {
                if (vehicle != null)
                {
                    vehicle.Mods.InstallModKit();
                    VehicleNeonLight[] neonLights = data.NeonLights;
                    if (neonLights != null)
                    {
                        neonLights
                            .ToList()
                            .ForEach(
                                (VehicleNeonLight h) =>
                                {
                                    vehicle.Mods.SetNeonLightsOn(h, true);
                                }
                            );
                    }
                    List<Tuple<VehicleMod, int>> mods = data.Mods;

                    if (mods != null)
                    {
                        mods.ForEach(
                            (Tuple<VehicleMod, int> m) =>
                            {
                                // Aplica o mod ao veículo usando a função nativa
                                Function.Call(
                                    Hash.SET_VEHICLE_MOD,
                                    vehicle.Handle,
                                    (int)m.Item1.Type,
                                    m.Item2,
                                    true
                                );
                            }
                        );
                    }

                    VehicleToggleMod[] toggleMods = data.ToggleMods;
                    if (toggleMods != null)
                    {
                        toggleMods
                            .ToList()
                            .ForEach(
                                (VehicleToggleMod h) =>
                                {
                                    // vehicle.ToggleMod(h, true);
                                    h.IsInstalled = true;
                                }
                            );
                    }
                    vehicle.Mods.WindowTint = data.WindowTint;
                    vehicle.Mods.WheelType = data.WheelType;
                    vehicle.Mods.NeonLightsColor = data.NeonColor;
                    Function.Call(Hash.SET_VEHICLE_LIVERY, vehicle.Handle, data.Livery);
                }
            }
        }

        public void Serialize(bool notify = false)
        {
            if (_vehicleCollection == null)
            {
                return; // Retorna imediatamente se não houver coleção.
            }

            try
            {
                UpdateVehicleData(); // Atualiza os dados da coleção.

                Serializer.Serialize("./scripts/Vehicles.dat", _vehicleCollection);

                if (notify)
                {
                    string message =
                        _vehicleCollection.Count <= 0 ? "Sem veículos." : "~p~Veículos~s~ salvos!";
                    Notification.Show(message);
                }
            }
            catch (Exception ex)
            {
                // Notifica ou loga o erro.
                Notification.Show("Erro ao salvar os veículos: " + ex.Message);
            }
        }

        private void UpdateVehicleData()
        {
            if (_vehicleCollection == null || _vehicleCollection.Count <= 0)
            {
                return; // Retorna se não houver dados na coleção.
            }

            foreach (var vehicleData in _vehicleCollection)
            {
                // Procura o veículo correspondente na lista de veículos.
                var vehicle = _vehicles.FirstOrDefault(v => v.Handle == vehicleData.Handle);

                if (vehicle != null)
                {
                    // Atualiza os dados específicos do veículo.
                    UpdateDataSpecific(vehicleData, vehicle);
                }
            }
        }

        private static void UpdateDataSpecific(VehicleData vehicleData, Vehicle vehicle)
        {
            if (vehicleData == null || vehicle == null)
            {
                Notification.Show(
                    "Dados ou veículo inválidos. Não foi possível atualizar os dados."
                );
                return;
            }

            try
            {
                vehicleData.Position = vehicle.Position;
                vehicleData.Rotation = vehicle.Rotation;
                vehicleData.Health = vehicle.Health;
                vehicleData.EngineHealth = vehicle.EngineHealth;

                // Propriedades de cor verificadas para segurança
                if (vehicle.Mods != null)
                {
                    vehicleData.PrimaryColor = vehicle.Mods.PrimaryColor;
                    vehicleData.SecondaryColor = vehicle.Mods.SecondaryColor;
                }
                else
                {
                    Notification.Show("Veículo não possui modificações de cor.");
                }
            }
            catch (Exception ex)
            {
                Notification.Show("Erro ao atualizar dados do veículo: " + ex.Message);
            }
        }

        public void SaveVehicle(Vehicle vehicle)
        {
            bool flag = _vehicleCollection == null;
            if (flag)
            {
                Deserialize();
            }
            VehicleData saved = _vehicleCollection.ToList().Find(v => v.Handle == vehicle.Handle);

            if (saved != null)
            {
                UpdateDataSpecific(saved, vehicle);
                Serialize(true);
            }
            else
            {
                VehicleNeonLight[] neonHashes = (VehicleNeonLight[])
                    Enum.GetValues(typeof(VehicleNeonLight));
                neonHashes = neonHashes
                    .Where(new Func<VehicleNeonLight, bool>(vehicle.Mods.IsNeonLightsOn))
                    .ToArray();
                VehicleMod[] modHashes = (VehicleMod[])Enum.GetValues(typeof(VehicleMod));
                List<Tuple<VehicleMod, int>> mods = new List<Tuple<VehicleMod, int>>();
                modHashes
                    .ToList()
                    .ForEach(
                        (VehicleMod mod) =>
                        {
                            int index = Function.Call<int>(
                                Hash.GET_VEHICLE_MOD,
                                vehicle.Handle,
                                (int)mod.Type
                            );

                            if (index != -1)
                            {
                                mods.Add(new Tuple<VehicleMod, int>(mod, index));
                            }
                        }
                    );
                VehicleToggleMod[] toggleModHashes = (VehicleToggleMod[])
                    Enum.GetValues(typeof(VehicleToggleMod));

                toggleModHashes = toggleModHashes
                    .Where(toggleMod =>
                    {
                        // Verifica se o mod de toggle está ativado no veículo
                        return toggleMod.IsInstalled; // Utiliza o IsInstalled para verificar se o mod está ativado
                    })
                    .ToArray();

                VehicleData item = new VehicleData(
                    handle: vehicle.Handle,
                    hash: vehicle.Model.Hash,
                    rotation: vehicle.Rotation,
                    position: vehicle.Position,
                    primaryColor: vehicle.Mods.PrimaryColor,
                    secondaryColor: vehicle.Mods.SecondaryColor,
                    health: vehicle.Health,
                    engineHealth: vehicle.EngineHealth,
                    heading: vehicle.Heading,
                    neonLights: neonHashes,
                    mods: mods,
                    toggleMods: toggleModHashes,
                    windowTint: vehicle.Mods.WindowTint,
                    wheelType: vehicle.Mods.WheelType,
                    neonColor: vehicle.Mods.NeonLightsColor,
                    livery: Function.Call<int>(Hash.GET_VEHICLE_LIVERY, vehicle.Handle),
                    wheels1: Function.Call<bool>(
                        Hash.GET_VEHICLE_MOD_VARIATION,
                        vehicle.Handle,
                        23
                    ),
                    wheels2: Function.Call<bool>(Hash.GET_VEHICLE_MOD_VARIATION, vehicle.Handle, 24)
                );
                _vehicleCollection.Add(item);
                _vehicles.Add(vehicle);
                vehicle.IsPersistent = true;
                EntityEventWrapper wrapper = new EntityEventWrapper(vehicle);
                wrapper.Died += WrapperOnDied;
                AddBlipToVehicle(vehicle);
            }
        }

        private static void AddBlipToVehicle(Vehicle vehicle)
        {
            if (vehicle == null)
            {
                Notification.Show("Veículo inválido. Não foi possível adicionar um blip.");
                return;
            }

            try
            {
                Blip blip = vehicle.AddBlip();
                blip.Sprite = GetSprite(vehicle);
                blip.Color = BlipColor.PurpleDark;
                blip.Name = string.IsNullOrEmpty(vehicle.LocalizedName)
                    ? "Veículo Desconhecido"
                    : vehicle.LocalizedName;
                blip.Scale = 0.85f;
            }
            catch (Exception ex)
            {
                Notification.Show("Erro ao adicionar blip ao veículo: " + ex.Message);
            }
        }

        private static BlipSprite GetSprite(Vehicle vehicle)
        {
            if (vehicle.ClassType == VehicleClass.Motorcycles)
            {
                return BlipSprite.PersonalVehicleBike;
            }
            else if (vehicle.ClassType == VehicleClass.Boats)
            {
                return BlipSprite.Boat;
            }
            else if (vehicle.ClassType == VehicleClass.Helicopters)
            {
                return BlipSprite.Helicopter;
            }
            else if (vehicle.ClassType == VehicleClass.Planes)
            {
                return BlipSprite.Plane;
            }
            else
            {
                return BlipSprite.PersonalVehicleCar; // Valor padrão para outras classes
            }
        }

        private void WrapperOnDied(object sender, EntityEventArgs e)
        {
            Notification.Show("Seu veículo foi ~r~destruído~s~!");

            _vehicleCollection.Remove(
                _vehicleCollection.ToList().Find((VehicleData v) => v.Handle == e.Entity.Handle)
            );

            e.Entity?.AttachedBlip?.Delete();

            (sender as EntityEventWrapper)?.Dispose();
        }
    }
}
