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
            // Desabilitando animações e eventos não necessários
            ped.CanPlayGestures = false;
            ped.SetCanPlayAmbientAnims(false);
            ped.SetCanEvasiveDive(false);
            ped.SetPathCanUseLadders(false);
            ped.SetPathCanClimb(false);
            ped.DisablePainAudio(true);
            ped.ApplyDamagePack(0f, 1f, DamagePack.BigHitByVehicle);
            ped.ApplyDamagePack(0f, 1f, DamagePack.ExplosionMed);
            ped.DiesOnLowHealth = false;
            ped.SetAlertness(Alertness.Neutral);
            ped.SetCombatAttribute(GTA.CombatAttributes.CanFightArmedPedsWhenNotArmed, true);
            Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, ped.Handle, 0, 0);
            ped.SetConfigFlag(PedConfigFlagToggles.DisableGoToWritheWhenInjured, true);
            ped.Task.WanderAround(ped.Position, ZumbiPed.WanderRadius);
            ped.KeepTaskWhenMarkedAsNoLongerNeeded = true;
            ped.BlockPermanentEvents = true;
            ped.IsPersistent = false;
            ped.AttachedBlip?.Delete();
            ped.IsPersistent = true;
            ped.RelationshipGroup = Relationships.InfectedRelationship;

            // Configura o zumbi para o tipo de ataque adequado (Walker ou Runner)
            float chance = 0.055f;
            if (IsNightFall())
            {
                chance = 0.5f;
            }
            else
            {
                TimeSpan currentDayTime = World.CurrentTimeOfDay;

                if (currentDayTime.Hours >= 20 || currentDayTime.Hours <= 3)
                {
                    chance = 0.4f;
                }
            }
            bool isRunner =
                (Database.Random.NextDouble() < (double)chance || overrideAsFastZombie) && Runners;
            if (isRunner)
            {
                return new Runner(ped);
            }
            else
            {
                ped.Health = ped.MaxHealth = health;
                return new Walker(ped);
            }
        }

        public static bool IsNightFall()
        {
            if (!Runners)
                return false;

            TimeSpan currentDayTime = World.CurrentTimeOfDay;
            return currentDayTime.Hours >= 20 || currentDayTime.Hours <= 3;
        }
    }
}
