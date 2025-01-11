using System;
using System.Collections.Generic;
using System.Linq;
using GTA;
using GTA.Math;
using GTA.UI;
using ZumbisModV.DataClasses;
using ZumbisModV.Extensions;
using ZumbisModV.Static;
using ZumbisModV.Wrappers;

namespace ZumbisModV.SurvivorTypes
{
    public class FriendlySurvivors : Survivors
    {
        public FriendlySurvivors()
        {
            Instance = this;
        }

        public static FriendlySurvivors Instance { get; private set; }
        private readonly List<Ped> _peds = new List<Ped>();
        private readonly PedGroup _pedGroup = new PedGroup();

        public void RemovePed(Ped item)
        {
            if (_peds.Contains(item))
            {
                _peds.Remove(item);
                item.LeaveGroup();
                Blip currentBlip = item.AttachedBlip;
                if (currentBlip != null)
                {
                    currentBlip.Delete();
                }
                EntityEventWrapper.Dispose(item);
            }
        }

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

            int num = Database.Random.Next(3, 6);
            Vector3 spawnPoint = GetSpawnPoint();

            if (!IsValidSpawn(spawnPoint))
            {
                Logger.LogError("Ponto de spawn inválido.");
                return;
            }
            for (int i = 0; i <= num; i++)
            {
                Ped ped = World.CreateRandomPed(spawnPoint.Around(5f));

                if (ped == null)
                {
                    Logger.LogError("Falha ao criar ped em SpawnEntities.");
                    continue; // Pule para o próximo ped, mas continue o loop
                }

                // Configurar ped
                Blip blip = ped.AddBlip();
                blip.Color = BlipColor.Blue;
                blip.Name = "Sobrevivente";
                ped.RelationshipGroup = Relationships.FriendlyRelationship;
                ped.Task.FightAgainstHatedTargets(9000f);
                ped.SetAlertness(Alertness.FullyAlert);
                ped.SetCombatAttributes(CombatAttributes.AlwaysFight, true);

                // Dar arma ao ped
                WeaponHash weapon = Database.WeaponHashes[
                    Database.Random.Next(Database.WeaponHashes.Length)
                ];
                ped.Weapons.Give(weapon, 25, true, true);

                // Verificar e adicionar ao grupo e lista
                if (_pedGroup == null)
                {
                    Logger.LogError("_pedGroup não está inicializado.");
                    continue;
                }
                _pedGroup.Add(ped, i == 0);
                _pedGroup.Formation = Formation.Loose;

                if (_peds == null)
                {
                    Logger.LogError("_peds não está inicializado.");
                    continue;
                }
                _peds.Add(ped);

                // Configurar eventos do ped
                EntityEventWrapper entityEventWrapper = new EntityEventWrapper(ped);
                entityEventWrapper.Died += EventWrapperOnDied;
                entityEventWrapper.Disposed += EventWrapperOnDisposed;
            }
            Notification.Show("Sobreviventes ~b~amigáveis~s~ por perto.", true);
        }

        private void EventWrapperOnDisposed(EntityEventWrapper sender, Entity entity)
        {
            if (_peds.Contains(entity as Ped))
            {
                _peds.Remove(entity as Ped);
            }
        }

        private void EventWrapperOnDied(EntityEventWrapper sender, Entity entity)
        {
            _peds.Remove(entity as Ped);
            entity.AttachedBlip?.Delete();
            entity.MarkAsNoLongerNeeded();
            sender.Dispose();
        }

        public override void CleanUp()
        {
            _peds.ForEach(ped =>
            {
                ped.AttachedBlip?.Delete();
                ped.MarkAsNoLongerNeeded();
                EntityEventWrapper.Dispose(ped);
            });
        }

        public override void Abort()
        {
            foreach (var ped in _peds.ToList())
            {
                ped.Delete();
                _peds.Remove(ped);
            }
        }
    }
}
