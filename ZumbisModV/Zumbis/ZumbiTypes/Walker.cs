using System;
using GTA;
using ZumbisModV.Extensions;

namespace ZumbisModV.Zumbis.ZumbiTypes
{
    public class Walker : ZumbiPed
    {
        private readonly Ped _ped;
        public override string MovementStyle { get; set; } = "move_m@drunk@verydrunk";

        public Walker(Ped ped)
            : base(ped)
        {
            _ped = this;
        }

        private void PlayAnimationIfNotPlaying(
            string animDict,
            string animName,
            float blendInSpeed = 8f,
            int duration = -1,
            int flag = 1
        )
        {
            if (!Ped.IsPlayingAnim(animDict, animName))
            {
                Ped.Task.PlayAnimation(animDict, animName, blendInSpeed, duration, flag);
            }
        }

        public override void OnAttackTarget(Ped target)
        {
            if (target.IsDead)
            {
                PlayAnimationIfNotPlaying("amb@world_human_bum_wash@male@high@idle_a", "idle_b");
            }
            else
            {
                if (!Ped.IsPlayingAnim("rcmbarry", "bar_1_teleport_aln"))
                {
                    Ped.Task.PlayAnimation("rcmbarry", "bar_1_teleport_aln", 8f, 1000, 16);

                    if (!target.IsInvincible)
                    {
                        target.ApplyDamage(ZombieDamage);
                    }

                    InfectTarget(target);
                }
            }
        }

        public override void OnGoToTarget(Ped target)
        {
            Ped.Task.GoTo(target);
        }
    }
}
