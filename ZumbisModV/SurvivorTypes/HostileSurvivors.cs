using System;
using System.Collections.Generic;
using GTA;
using GTA.Math;
using GTA.UI;
using ZumbisModV.DataClasses;
using ZumbisModV.Extensions;
using ZumbisModV.Static;
using ZumbisModV.Wrappers;

namespace ZumbisModV.SurvivorTypes
{
    public class HostileSurvivors : Survivors
    {
        private PedGroup _pedGroup = new PedGroup();
        private List<Ped> _peds = new List<Ped>();
        private Vehicle _vehicle;

        public override void Update()
        {
            if (_peds.Count <= 0)
            {
                OnCompleted();
            }
        }

        public override void SpawnEntities()
        {
            if (Database.Random == null || Database.WeaponHashes == null)
            {
                Logger.LogError("Database ou seus membros não estão inicializados.");
                return;
            }

            Vector3 spawnPoint = GetSpawnPoint();

            if (!IsValidSpawn(spawnPoint) || spawnPoint == Vector3.Zero)
            {
                Logger.LogError("Ponto de spawn inválido.");
                return;
            }
            if (_pedGroup == null)
                _pedGroup = new PedGroup();
            if (_peds == null)
                _peds = new List<Ped>();

            Vehicle vehicle = World.CreateVehicle(
                Database.GetRandomVehicleModel(),
                spawnPoint,
                Database.Random.Next(1, 360)
            );

            if (vehicle == null)
            {
                Logger.LogError("Falha ao criar veículo.");
                OnCompleted();
                return;
            }

            _vehicle = vehicle;

            var blip = _vehicle.AddBlip();
            blip.Name = "Veículo Inimigo";
            blip.Sprite = BlipSprite.PlayerstateKeyholder;
            blip.Color = BlipColor.Red;

            var entityEventWrapper = new EntityEventWrapper(_vehicle);
            entityEventWrapper.Died += VehicleWrapperOnDied;
            entityEventWrapper.Updated += VehicleWrapperOnUpdated;

            int maxPeds = Math.Max(vehicle.PassengerCapacity + 1, 1);

            for (int i = 0; i < maxPeds; i++)
            {
                if (_pedGroup.MemberCount >= 6)
                    break;

                VehicleSeat seat = (i == 0) ? VehicleSeat.Driver : VehicleSeat.Any;
                Ped ped = vehicle.CreateRandomPedOnSeat(seat);

                if (ped == null || !ped.Exists())
                {
                    Logger.LogError($"Falha ao criar ped no seat {seat}.");
                    continue;
                }

                // Configurar ped
                var blip2 = ped.AddBlip();
                blip2.Name = "Inimigo";

                ped.RelationshipGroup = Relationships.HostileRelationship;
                ped.SetAlertness(Alertness.FullyAlert);
                ped.SetCombatAttributess(Extensions.CombatAttributes.AlwaysFight, true);

                // Dar arma ao ped
                WeaponHash weapon = Database.WeaponHashes[
                    Database.Random.Next(Database.WeaponHashes.Length)
                ];
                ped.Weapons.Give(weapon, 25, true, true);

                // Verificar e adicionar ao grupo e lista
                _pedGroup.Add(ped, i == 0);
                _peds.Add(ped);

                // Configurar eventos do ped
                var entityEventWrapper2 = new EntityEventWrapper(ped);
                entityEventWrapper2.Died += PedWrapperOnDied;
                entityEventWrapper2.Updated += PedWrapperOnUpdated;
                entityEventWrapper2.Disposed += PedWrapperOnDisposed;
            }
            Notification.PostTicker("~r~Hostis~s~ por perto!", true, false);
        }

        private void PedWrapperOnDisposed(object sender, EntityEventArgs e)
        {
            if (_peds.Contains(e.Entity as Ped))
            {
                _peds.Remove(e.Entity as Ped);
            }
        }

        private void VehicleWrapperOnUpdated(object sender, EntityEventArgs e)
        {
            if (e.Entity != null)
            {
                e.Entity.AttachedBlip.Alpha = (_vehicle.Driver.Exists() ? 255 : 0);
            }
        }

        private void VehicleWrapperOnDied(object sender, EntityEventArgs e)
        {
            e.Entity.AttachedBlip?.Delete(); // Remove o Blip, se existir
            (sender as EntityEventWrapper)?.Dispose();

            if (_vehicle != null)
            {
                _vehicle.MarkAsNoLongerNeeded();
                _vehicle = null;
            }
        }

        private void PedWrapperOnUpdated(object sender, EntityEventArgs e)
        {
            Ped ped = e.Entity as Ped;
            bool flag = ped == null;
            if (!flag)
            {
                // Ped está dirigindo um veículo e não está em combate
                var currentVehicle = ped.CurrentVehicle;
                if (currentVehicle?.Driver == ped && !ped.IsInCombat)
                {
                    ped.Task.DriveTo(currentVehicle, PlayerPosition, 25f, 75f);
                }

                // Ped está fora do alcance de exclusão
                if (ped.Position.VDist(PlayerPosition) > Survivors.DeleteRange)
                {
                    ped.Delete();
                    return; // Garantir que não execute código abaixo
                }

                // Atualizar transparência do Blip associado ao ped
                if (ped.AttachedBlip?.Exists() == true)
                {
                    ped.AttachedBlip.Alpha = ped.IsInVehicle() ? 0 : 255;
                }
            }
        }

        private void PedWrapperOnDied(object sender, EntityEventArgs e)
        {
            e.Entity.AttachedBlip?.Delete();

            _peds.Remove(e.Entity as Ped);
        }

        public override void CleanUp()
        {
            // Remover o Blip associado ao veículo, se existir
            _vehicle?.AttachedBlip?.Delete();

            // Descartar o wrapper do veículo, se existir
            EntityEventWrapper.Dispose(_vehicle);
        }

        public override void Abort()
        {
            // Deletar o veículo, se existir
            _vehicle?.Delete();

            // Iterar sobre todos os pedestres e deletá-los
            foreach (var ped in _peds)
            {
                ped?.Delete();
            }

            // Limpar a lista de pedestres
            _peds.Clear();
        }
    }
}
