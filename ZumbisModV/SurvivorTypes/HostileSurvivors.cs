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
        private readonly PedGroup _group = new PedGroup();
        private readonly List<Ped> _peds = new List<Ped>();
        private Vehicle _vehicle;

        public override void Update()
        {
            if (_peds.Count <= 0)
            {
                Complete();
            }
        }

        public override void SpawnEntities()
        {
            Logger.LogInfo("SpawnEntities() iniciado.");
            if (Database.Random == null || Database.WeaponHashes == null)
            {
                Logger.LogError("Database ou seus membros não estão inicializados.");
                return;
            }

            Vector3 spawnPoint = GetSpawnPoint();

            if (!IsValidSpawn(spawnPoint))
            {
                Logger.LogError("Ponto de spawn inválido.");
                return;
            }
            Vehicle vehicle = World.CreateVehicle(
                Database.GetRandomVehicleModel(),
                spawnPoint,
                Database.Random.Next(1, 360)
            );

            if (vehicle == null)
            {
                Complete();
            }
            else
            {
                _vehicle = vehicle;
                Blip blip = _vehicle.AddBlip();
                blip.Name = "Veículo Inimigo";
                blip.Sprite = BlipSprite.PlayerstateKeyholder;
                blip.Color = BlipColor.Red;
                EntityEventWrapper entityEventWrapper = new EntityEventWrapper(_vehicle);
                entityEventWrapper.Died += VehicleWrapperOnDied;
                entityEventWrapper.Updated += VehicleWrapperOnUpdated;
                for (int i = 0; i < vehicle.PassengerCapacity + 1; i++)
                {
                    bool flag3 = _group.MemberCount >= 6;
                    if (!flag3)
                    {
                        if (vehicle.IsSeatFree(VehicleSeat.Any))
                        {
                            Ped ped = vehicle.CreateRandomPedOnSeat(
                                (i == 0) ? VehicleSeat.Driver : VehicleSeat.Any
                            );

                            if (ped != null)
                            {
                                // Configurar ped
                                Blip blip2 = ped.AddBlip();
                                blip2.Name = "Inimigo";
                                ped.RelationshipGroup = Relationships.HostileRelationship;
                                ped.SetAlertness(Alertness.FullyAlert);
                                ped.SetCombatAttributes(CombatAttributes.AlwaysFight, true);

                                // Dar arma ao ped
                                WeaponHash weapon = Database.WeaponHashes[
                                    Database.Random.Next(Database.WeaponHashes.Length)
                                ];
                                ped.Weapons.Give(weapon, 25, true, true);

                                // Verificar e adicionar ao grupo e lista
                                if (_group == null)
                                {
                                    Logger.LogError("_group não está inicializado.");
                                    continue;
                                }
                                _group.Add(ped, i == 0);

                                if (_peds == null)
                                {
                                    Logger.LogError("_peds não está inicializado.");
                                    continue;
                                }
                                _peds.Add(ped);

                                // Configurar eventos do ped
                                EntityEventWrapper entityEventWrapper2 = new EntityEventWrapper(
                                    ped
                                );
                                entityEventWrapper2.Died += PedWrapperOnDied;
                                entityEventWrapper2.Updated += PedWrapperOnUpdated;
                                entityEventWrapper2.Disposed += PedWrapperOnDisposed;
                            }
                        }
                    }
                }
                Notification.Show("~r~Hostis~s~ por perto!");
            }
        }

        private void PedWrapperOnDisposed(EntityEventWrapper sender, Entity entity)
        {
            if (_peds.Contains(entity as Ped))
            {
                _peds.Remove(entity as Ped);
            }
        }

        private void VehicleWrapperOnUpdated(EntityEventWrapper sender, Entity entity)
        {
            if (entity != null)
            {
                entity.AttachedBlip.Alpha = (_vehicle.Driver.Exists() ? 255 : 0);
            }
        }

        private void VehicleWrapperOnDied(EntityEventWrapper sender, Entity entity)
        {
            entity.AttachedBlip?.Delete(); // Remove o Blip, se existir
            sender.Dispose();

            if (_vehicle != null)
            {
                _vehicle.MarkAsNoLongerNeeded();
                _vehicle = null;
            }
        }

        private void PedWrapperOnUpdated(EntityEventWrapper sender, Entity entity)
        {
            Ped ped = entity as Ped;
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

        private void PedWrapperOnDied(EntityEventWrapper sender, Entity entity)
        {
            entity.AttachedBlip?.Delete();

            _peds.Remove(entity as Ped);
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
