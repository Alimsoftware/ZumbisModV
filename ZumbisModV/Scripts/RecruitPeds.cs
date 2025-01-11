using System;
using GTA;
using GTA.Math;
using GTA.UI;
using ZumbisModV.Extensions;
using ZumbisModV.Static;
using ZumbisModV.SurvivorTypes;
using ZumbisModV.Utils;

namespace ZumbisModV.Scripts
{
    public class RecruitPeds : Script
    {
        public const float InteractDistance = 1.5f;

        public RecruitPeds() => Tick += new EventHandler(OnTick);

        private static Ped PlayerPed => Database.PlayerPed;

        private static Vector3 PlayerPosition => Database.PlayerPosition;

        private static void OnTick(object sender, EventArgs eventArgs)
        {
            if (MenuController.MenuPool.AreAnyVisible || PlayerPed.PedGroup.MemberCount >= 6)
                return;
            Ped closest = World.GetClosest(PlayerPosition, World.GetNearbyPeds(PlayerPed, 1.5f));
            if (
                closest == null
                || closest.IsDead
                || closest.IsInCombatAgainst(PlayerPed)
                || closest.GetRelationshipWithPed(PlayerPed) == Relationship.Hate
                || closest.RelationshipGroup != Relationships.FriendlyRelationship
                || closest.PedGroup == PlayerPed.PedGroup
            )
                return;
            Game.DisableControlThisFrame(Control.Enter);
            UiExtended.DisplayHelpTextThisFrame("Pressione ~INPUT_ENTER~ para recrutar este ped.");
            if (!GTAUtils.IsDisabledControlJustPressed(Control.Enter))
                return;
            if (FriendlySurvivors.Instance != null)
                FriendlySurvivors.Instance.RemovePed(closest);
            closest.Recruit(PlayerPed);
            if (PlayerPed.PedGroup.MemberCount < 6)
                return;
            Notification.Show("Você atingiu a quantidade máxima de ~b~guardas~s~.");
        }
    }
}
