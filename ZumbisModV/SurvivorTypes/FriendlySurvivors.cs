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
        private List<Ped> _peds = new List<Ped>();
        private PedGroup _pedGroup = new PedGroup();

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
                OnCompleted();
            }
        }

        public override void SpawnEntities()
        {
            int num = Database.Random.Next(3, 6);
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

            for (int i = 0; i < num; i++)
            {
                Ped ped = World.CreateRandomPed(spawnPoint.Around(5f));

                if (ped is null || !ped.Exists())
                {
                    Logger.LogError("Falha ao criar um ped.");
                    continue;
                }
                // Configurar ped
                try
                {
                    Blip blip = ped.AddBlip();
                    blip.Color = BlipColor.Blue;
                    blip.Name = "Sobrevivente";
                    ped.RelationshipGroup = Relationships.FriendlyRelationship;
                    ped.Task.FightAgainstHatedTargets(9000f);
                    ped.SetAlertness(Alertness.FullyAlert);
                    ped.SetCombatAttributess(Extensions.CombatAttributes.AlwaysFight, true);

                    // Dar arma ao ped
                    WeaponHash weapon = Database.WeaponHashes[
                        Database.Random.Next(Database.WeaponHashes.Length)
                    ];
                    ped.Weapons.Give(weapon, 25, true, true);

                    _pedGroup.Add(ped, i == 0);
                    _pedGroup.Formation = Formation.Loose;
                    _peds.Add(ped);

                    // Configurar eventos do ped
                    EntityEventWrapper entityEventWrapper = new EntityEventWrapper(ped);
                    entityEventWrapper.Died += EventWrapperOnDied;
                    entityEventWrapper.Disposed += EventWrapperOnDisposed;
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Erro ao configurar ped: {ex.Message}");
                }
            }

            Notification.Show("Sobreviventes ~b~amigáveis~s~ por perto.", true);
        }

        private void EventWrapperOnDisposed(object sender, EntityEventArgs e)
        {
            if (_peds.Contains(e.Entity as Ped))
            {
                _peds.Remove(e.Entity as Ped);
            }
        }

        private void EventWrapperOnDied(object sender, EntityEventArgs e)
        {
            _peds.Remove(e.Entity as Ped);
            e.Entity.AttachedBlip?.Delete();
            e.Entity.MarkAsNoLongerNeeded();
            (sender as EntityEventWrapper).Dispose();
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
