using System;
using GTA;
using GTA.Native;
using ZumbisModV.Extensions;
using ZumbisModV.Static;

namespace ZumbisModV.Zumbis.ZumbiTypes
{
    public class Runner : ZumbiPed
    {
        private readonly Ped _ped;
        private bool _jumpAttack;

        // Constantes para animações
        private const string IdleAnimDict = "amb@world_human_bum_wash@male@high@idle_a";
        private const string IdleAnimName = "idle_b";
        private const string RegularAttackAnimDict = "rcmbarry";
        private const string RegularAttackAnimName = "bar_1_teleport_aln";

        public Runner(Ped ped)
            : base(ped)
        {
            _ped = this;
        }

        public override bool PlayAudio { get; set; } = true;

        public override string MovementStyle { get; set; } = "move_m@injured";

        public override void OnAttackTarget(Ped target)
        {
            if (target.IsDead)
            {
                HandleIdleAnimation();
                return;
            }

            if (CanPerformJumpAttack(target))
            {
                PerformJumpAttack(target);
            }
            else
            {
                PerformRegularAttack(target);
            }
        }

        public override void OnGoToTarget(Ped target)
        {
            Function.Call(
                Hash.TASK_GO_TO_ENTITY,
                _ped.Handle,
                target.Handle,
                -1,
                0.0f,
                5f,
                1073741824,
                0
            );
        }

        private void HandleIdleAnimation()
        {
            // Verifica e toca a animação se não estiver em execução
            PlayAnimationIfNotPlaying(IdleAnimDict, IdleAnimName, 8f, -1, AnimationFlags.Loop);
        }

        private bool CanPerformJumpAttack(Ped target)
        {
            // Verifica se o ataque de pulo pode ser realizado
            return Database.Random.NextDouble() < 0.3
                && !_jumpAttack
                && !target.IsPerformingStealthKill
                && !target.IsGettingUp
                && !target.IsRagdoll;
        }

        private void PerformJumpAttack(Ped target)
        {
            // Realiza o ataque de pulo
            _ped.Jump();
            _ped.Heading = (target.Position - Position).ToHeading();
            _jumpAttack = true;
            target.SetToRagdoll(2000);
        }

        private void PerformRegularAttack(Ped target)
        {
            // Executa um ataque regular
            if (IsPedPlayingAnim(RegularAttackAnimDict, RegularAttackAnimName))
                return;

            PlayAnimationIfNotPlaying(
                RegularAttackAnimDict,
                RegularAttackAnimName,
                8f,
                1000,
                AnimationFlags.StayInEndFrame
            );

            if (!target.IsInvincible)
                target.ApplyDamage(ZombieDamage);

            InfectTarget(target);
        }

        private bool IsPedPlayingAnim(string animDict, string animName)
        {
            // Verifica se o ped está tocando uma animação
            return _ped.IsPlayingAnim(animDict, animName);
        }

        private void PlayAnimationIfNotPlaying(
            string animDict,
            string animName,
            float blendInSpeed,
            int duration,
            AnimationFlags flags
        )
        {
            if (!IsPedPlayingAnim(animDict, animName))
            {
                _ped.Task.PlayAnimation(animDict, animName, blendInSpeed, duration, flags);
            }
        }
    }
}
