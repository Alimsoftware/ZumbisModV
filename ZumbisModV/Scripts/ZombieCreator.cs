using System;
using GTA;
using GTA.Native;
using ZumbisModV.Extensions;
using ZumbisModV.Static;
using ZumbisModV.Zumbis;
using ZumbisModV.Zumbis.ZumbiTypes;

namespace ZumbisModV.Scripts
{
    public static class ZombieCreator
    {
        public static bool Runners { get; set; }

        public static ZumbiPed InfectPed(Ped ped, int health, bool overrideAsFastZombie = false)
        {
            ConfigurePedForZombie(ped);
            SetZombieAttributes(ped);
            float zombieSpeedFactor = DetermineZombieSpeedFactor(overrideAsFastZombie);
            ped.Health = ped.MaxHealth = health;

            if (zombieSpeedFactor > 0.0f && ZombieCreator.Runners)
                return new Runner(ped);

            return new Walker(ped);
        }

        private static void ConfigurePedForZombie(Ped ped)
        {
            // Desabilitando animações e eventos não necessários
            ped.CanPlayGestures = false;
            ped.SetCanPlayAmbientAnims(false);
            ped.SetCanEvasiveDive(false);
            ped.SetPathCanUseLadders(false);
            ped.SetPathCanClimb(false);
            ped.DisablePainAudio(true);
            ped.ApplyDamagePack(0.0f, 1f, DamagePack.BigHitByVehicle);
            ped.ApplyDamagePack(0.0f, 1f, DamagePack.ExplosionMed);
            ped.DiesOnLowHealth = false;
            ped.SetAlertness(Alertness.Nuetral);
            ped.SetCombatAttributes(CombatAttributes.AlwaysFight, true);
            Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, ped.Handle, 0, 0);
            ped.SetConfigFlag(281, true);
            ped.Task.WanderAround(ped.Position, ZumbiPed.WanderRadius);
            ped.AlwaysKeepTask = true;
            ped.BlockPermanentEvents = true;
            ped.IsPersistent = false;
            ped.AttachedBlip?.Delete();
            ped.IsPersistent = true;
            ped.RelationshipGroup = Relationships.InfectedRelationship;
        }

        private static void SetZombieAttributes(Ped ped)
        {
            // Configura o zumbi para o tipo de ataque adequado (Walker ou Runner)
            float num = 0.055f;
            if (IsNightFall())
                num = 0.5f;

            TimeSpan currentDayTime = World.CurrentTimeOfDay;
            if (currentDayTime.Hours >= 20 || currentDayTime.Hours <= 3)
                num = 0.4f;
        }

        private static float DetermineZombieSpeedFactor(bool overrideAsFastZombie)
        {
            return Database.Random.NextDouble() < 0.4 || overrideAsFastZombie ? 0.4f : 0.0f;
        }

        public static bool IsNightFall()
        {
            if (!ZombieCreator.Runners)
                return false;

            TimeSpan currentDayTime = World.CurrentTimeOfDay;
            return currentDayTime.Hours >= 20 || currentDayTime.Hours <= 3;
        }
    }
}
