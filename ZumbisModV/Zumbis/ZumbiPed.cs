using System;
using System.Drawing;
using System.Linq;
using GTA;
using GTA.Math;
using GTA.Native;
using ZumbisModV.Extensions;
using ZumbisModV.Scripts;
using ZumbisModV.Static;
using ZumbisModV.Wrappers;

namespace ZumbisModV.Zumbis
{
    public abstract class ZumbiPed
    {
        // Propriedade principal para encapsular o Ped
        protected Ped Ped { get; }

        // Propriedades derivadas do Ped
        public int Handle => Ped.Handle;
        public Vector3 Position => Ped.Position;
        public bool IsDead => Ped.IsDead;

        // Constantes e propriedades estáticas
        public const int MovementUpdateInterval = 5;
        public static int ZombieDamage = 15;
        public static float SensingRange = 120f;
        public static float SilencerEffectiveRange = 15f;
        public static float BehindZombieNoticeDistance = 5f;
        public static float RunningNoticeDistance = 25f;
        public static float AttackRange = 1.2f;
        public static float VisionDistance = 35f;
        public static float WanderRadius = 100f;

        // Eventos
        public event Action<Ped> GoToTarget;
        public event Action<Ped> AttackTarget;

        // Propriedades específicas do Zumbi
        public virtual bool PlayAudio { get; set; }
        public virtual string MovementStyle { get; set; } = "move_m@injured";

        // Campos privados
        //private readonly Ped _ped;
        private Ped _target;
        private bool _goingToTarget;
        private bool _attackingTarget;
        private DateTime _nextMovementUpdate;
        private DateTime _currentMovementUpdateTime;
        private readonly EntityEventWrapper _eventWrapper;

        protected ZumbiPed(Ped ped)
        {
            if (ped == null || !ped.Exists())
                throw new ArgumentException("Ped fornecido é inválido.");

            Ped = ped;

            _eventWrapper = new EntityEventWrapper(Ped);
            _eventWrapper.Died += OnDied;
            _eventWrapper.Updated += Update;
            _eventWrapper.Aborted += Abort;

            _currentMovementUpdateTime = DateTime.UtcNow;
            GoToTarget += OnGoToTarget;
            AttackTarget += OnAttackTarget;
        }

        public Ped Target
        {
            get => _target;
            private set
            {
                if (value == null && _target == null)
                {
                    Ped.Task.WanderAround(Position, ZumbiPed.WanderRadius);
                    GoingToTarget = false;
                    AttackingTarget = false;
                }
                _target = value;
            }
        }

        public bool GoingToTarget
        {
            get => _goingToTarget;
            set
            {
                if (value && !_goingToTarget)
                    GoToTarget?.Invoke(Target);
                _goingToTarget = value;
            }
        }

        public bool AttackingTarget
        {
            get => _attackingTarget;
            set
            {
                if (
                    value
                    && !Ped.IsRagdoll
                    && !IsDead
                    && !Ped.IsClimbing
                    && !Ped.IsFalling
                    && !Ped.IsBeingStunned
                    && !Ped.IsGettingUp
                )
                    AttackTarget?.Invoke(Target);
                _attackingTarget = value;
            }
        }

        public abstract void OnAttackTarget(Ped target);
        public abstract void OnGoToTarget(Ped target);

        private void OnDied(EntityEventWrapper sender, Entity entity)
        {
            Ped.AttachedBlip?.Delete();
            if (ZombieVehicleSpawner.Instance.IsValidSpawn(entity.Position))
                ZombieVehicleSpawner.Instance.SpawnBlocker.Add(entity.Position);
        }

        public void Update(EntityEventWrapper entityEventWrapper, Entity entity)
        {
            if (Position.VDist(Database.PlayerPosition) > 120.0 && (!Ped.IsOnScreen || IsDead))
                Ped.Delete();
            if (PlayAudio && Ped.IsRunning)
            {
                Ped.DisablePainAudio(false);
                Ped.PlayPain(8);
                Ped.PlayFacialAnim("facials@gen_male@base", "burning_1");
            }
            GetTarget();
            SetWalkStyle();
            if (Ped.IsOnFire && !Ped.IsDead)
                Ped.Kill();
            Ped.StopAmbientSpeechThisFrame();

            if (!PlayAudio)
                Ped.StopSpeaking(true);

            if (Target == null)
                return;

            if (Position.VDist(Target.Position) > ZumbiPed.AttackRange)
            {
                AttackingTarget = false;
                GoingToTarget = true;
            }
            else
            {
                AttackingTarget = true;
                GoingToTarget = false;
            }
        }

        public void Abort(EntityEventWrapper sender, Entity entity) => Ped.Delete();

        public void InfectTarget(Ped target)
        {
            if (target.IsPlayer || target.Health > target.MaxHealth / 4)
                return;
            target.SetToRagdoll(3000);
            ZombieCreator.InfectPed(target, Ped.MaxHealth, true);
            ForgetTarget();
            target.LeaveGroup();
            target.Weapons.Drop();
            EntityEventWrapper.Dispose(target);
        }

        public void ForgetTarget() => _target = null;

        private void SetWalkStyle()
        {
            if (DateTime.UtcNow <= _currentMovementUpdateTime)
                return;
            Ped.SetMovementAnimSet(MovementStyle);
            UpdateTime();
        }

        private void UpdateTime()
        {
            _currentMovementUpdateTime = DateTime.UtcNow + new TimeSpan(0, 0, 0, 5);
        }

        private void GetTarget()
        {
            var nearbyPeds = World.GetNearbyPeds(Ped, SensingRange).Where(IsGoodTarget).ToArray();

            var closest = World.GetClosest(Position, nearbyPeds);
            if (
                closest != null
                && (
                    Ped.HasClearLineOfSight(closest, ZumbiPed.VisionDistance) || CanHearPed(closest)
                )
            )
            {
                Target = closest;
            }
            else if (!IsGoodTarget(Target))
            {
                Target = null;
            }
        }

        private bool CanHearPed(Ped ped)
        {
            float distance = ped.Position.VDist(Position);
            return !IsWeaponWellSilenced(ped, distance)
                || IsBehindZombie(distance)
                || IsRunningNoticed(ped, distance);
        }

        private static bool IsRunningNoticed(Ped ped, float distance) =>
            ped.IsSprinting && distance < RunningNoticeDistance;

        private static bool IsBehindZombie(float distance) => distance < BehindZombieNoticeDistance;

        private static bool IsWeaponWellSilenced(Ped ped, float distance) =>
            !ped.IsShooting || ped.IsCurrentWeaponSileced() && distance > SilencerEffectiveRange;

        private bool IsGoodTarget(Ped ped) => ped.GetRelationshipWithPed(Ped) == Relationship.Hate;

        protected bool Equals(ZumbiPed other)
        {
            return other != null && base.Equals(other) && Equals(Ped, other.Ped);
        }

        public virtual bool Equals(object obj)
        {
            return obj is ZumbiPed other && Equals(other);
        }

        public virtual int GetHashCode() => base.GetHashCode() * 397 ^ (Ped?.GetHashCode() ?? 0);

        public bool Equals(Ped other) => Ped == other;

        public static implicit operator Ped(ZumbiPed v) => v.Ped;
    }
}
