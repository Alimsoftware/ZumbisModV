using System;
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
        // ===============================
        // 🔗 Propriedades principais
        // ===============================

        protected Ped Ped { get; }

        public int Handle => Ped?.Handle ?? -1;
        public Vector3 Position => Ped?.Position ?? Vector3.Zero;
        public bool IsDead => Ped == null || !Ped.Exists() || Ped.IsDead;

        // ===============================
        // 🔥 Configurações globais
        // ===============================

        public const int MovementUpdateInterval = 5;

        public static int ZombieDamage = 15;
        public static float SensingRange = 120f;
        public static float SilencerEffectiveRange = 15f;
        public static float BehindZombieNoticeDistance = 5f;
        public static float RunningNoticeDistance = 25f;
        public static float AttackRange = 1.2f;
        public static float VisionDistance = 35f;
        public static float WanderRadius = 100f;

        // ===============================
        // 🔔 Eventos
        // ===============================

        public event Action<Ped> GoToTarget;
        public event Action<Ped> AttackTarget;

        // ===============================
        // 🎭 Comportamento e estado
        // ===============================

        public virtual bool PlayAudio { get; set; }
        public virtual string MovementStyle { get; set; } = "move_m@injured";

        private Ped _target;
        private bool _goingToTarget;
        private bool _attackingTarget;
        private DateTime _nextMovementUpdateTime;

        private readonly EntityEventWrapper _eventWrapper;

        // ===============================
        // 🚀 Construtor
        // ===============================

        protected ZumbiPed(Ped ped)
        {
            if (ped == null || !ped.Exists())
            {
                Logger.LogWarning("Tentativa de infectar um ped inválido.");
                throw new ArgumentException("Ped fornecido é inválido.");
            }
            Ped = ped;

            _eventWrapper = new EntityEventWrapper(ped);
            _eventWrapper.Died += OnDied;
            _eventWrapper.Updated += Update;
            _eventWrapper.Aborted += Abort;

            _nextMovementUpdateTime = DateTime.UtcNow;
            GoToTarget += OnGoToTarget;
            AttackTarget += OnAttackTarget;
        }

        // ===============================
        // 🎯 Target e movimentação
        // ===============================

        public Ped Target
        {
            get => _target;
            private set
            {
                if (value == null && _target != null)
                {
                    Ped?.Task?.WanderAround(Position, WanderRadius);
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
                if (value && CanAttack())
                    AttackTarget?.Invoke(Target);
                _attackingTarget = value;
            }
        }

        private bool CanAttack()
        {
            return !Ped.IsRagdoll
                && !IsDead
                && !Ped.IsClimbing
                && !Ped.IsFalling
                && !Ped.IsBeingStunned
                && !Ped.IsGettingUp;
        }

        // ===============================
        // 📜 Métodos obrigatórios
        // ===============================

        public abstract void OnAttackTarget(Ped target);
        public abstract void OnGoToTarget(Ped target);

        // ===============================
        // 🔗 Eventos internos
        // ===============================

        private void OnDied(object sender, EntityEventArgs e)
        {
            Ped.AttachedBlip?.Delete();

            if (
                !ZombieVehicleSpawner.Instance.IsInvalidZone(e.Entity.Position)
                && ZombieVehicleSpawner.Instance.IsValidSpawn(e.Entity.Position)
            )
            {
                ZombieVehicleSpawner.Instance.SpawnBlocker.Add(e.Entity.Position);
            }
        }

        public void Abort(object sender, EntityEventArgs e)
        {
            //Ped?.Delete();
            // Este método agora é mais focado em limpar recursos do ZumbiPed e desinscrever eventos.
            // A decisão de deletar o Ped físico é mais controlada pelo Update.

            if (_eventWrapper != null)
            {
                _eventWrapper.Died -= OnDied;
                _eventWrapper.Updated -= Update;
                _eventWrapper.Aborted -= Abort;
            }

            Ped?.AttachedBlip?.Delete(); // Limpar blips ainda é bom.

            // A remoção de Ped?.Delete() daqui é crucial se Abort é chamado indiscriminadamente.
            // Se Abort é chamado especificamente quando você QUER que o Ped seja deletado (mesmo vivo),
            // então a chamada a Ped.Delete() pode permanecer, mas o fluxo de chamada precisa ser revisto.
            // Pela lógica atual, é mais seguro que Update controle o Delete.
            // Ped?.Delete(); // <-- CONSIDERE REMOVER ou tornar condicional.

            // Resetar estado interno
            _target = null;
            _goingToTarget = false;
            _attackingTarget = false;
        }

        // ===============================
        // ♻️ Loop de atualização
        // ===============================

        public void Update(object _, EntityEventArgs e)
        {
            // if (IsDead || Ped == null)
            //{
            // Abort(_, __);
            // return;
            // }
            // 1. Se a referência ao Ped foi perdida ou a entidade não existe mais no jogo
            //    (por exemplo, despawnado pelo próprio jogo, e não pela nossa lógica de morte)
            if (Ped == null || !Ped.Exists())
            {
                // O Ped sumiu por alguma razão externa ou nunca foi válido.
                // Precisamos limpar a instância ZumbiPed e parar de processá-la.
                // Desinscreva os eventos para evitar chamadas futuras.
                if (_eventWrapper != null)
                {
                    _eventWrapper.Died -= OnDied;
                    _eventWrapper.Updated -= Update;
                    _eventWrapper.Aborted -= Abort;
                    // Se o EntityEventWrapper gerencia uma lista de entidades para atualizar,
                    // ele deve ser notificado para remover este Ped.
                }
                // Não é necessário chamar Ped.Delete() aqui, pois o Ped já não existe.
                return; // Para o processamento desta instância ZumbiPed.
            }

            // 2. Se o Ped está morto (foi morto pelo jogador, por exemplo)
            if (Ped.IsDead) // Usar Ped.IsDead diretamente para clareza
            {
                // O zumbi está morto. O corpo deve permanecer no mundo.
                // NÃO chamamos Ped.Delete() aqui.
                // O evento OnDied (se você tiver lógica lá como dropar loot) já deve ter sido chamado.
                // Idealmente, o EntityEventWrapper deveria parar de chamar Update para Peds mortos
                // ou você precisaria de uma flag interna no ZumbiPed para não processar mais a IA.
                // Por agora, apenas retornamos para não executar a lógica de IA de zumbi vivo.
                return;
            }

            // 3. LÓGICA PARA ZUMBIS VIVOS ABAIXO

            // Lógica de despawn para zumbis VIVOS que estão muito longe e fora da tela
            if (Position.VDist(Database.PlayerPosition) > 120.0 && !Ped.IsOnScreen)
            {
                // Para zumbis vivos que estão muito longe, podemos deletá-los para performance.
                Ped.Delete();
                // Após o delete, no próximo tick, Ped.Exists() será falso e o primeiro if tratará a limpeza do ZumbiPed.
                return;
            }

            // --- Restante da sua lógica para zumbis VIVOS ---

            if (PlayAudio && Ped.IsRunning)
            {
                PlayZombieAudio();
            }

            GetTarget();
            UpdateWalkStyle();

            // Se o zumbi está pegando fogo (e ainda não está morto, embora o check Ped.IsDead acima já cobriria)
            if (Ped.IsOnFire)
            {
                Ped.Kill(); // Isso fará com que Ped.IsDead seja true no próximo frame,
                // e o bloco if(Ped.IsDead) acima cuidará disso.
            }

            Ped.StopAmbientSpeechThisFrame();

            if (!PlayAudio)
            {
                Ped.StopSpeaking(true);
            }

            HandleCombatLogic();
        }

        private void HandleCombatLogic()
        {
            if (Target == null)
                return;

            float distance = Position.VDist(Target.Position);

            if (distance > AttackRange)
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

        private void PlayZombieAudio()
        {
            Ped.DisablePainAudio(false);
            Ped.PlayPain(8);
            Ped.PlayFacialAnim("facials@gen_male@base", "burning_1");
        }

        // ===============================
        // ☣️ Infecção
        // ===============================

        public void InfectTarget(Ped target)
        {
            if (target == null || target.IsPlayer)
                return;

            bool canInfect = target.Health > target.MaxHealth / 4;
            if (!canInfect)
            {
                target.SetToRagdoll(3000);
                ZombieCreator.InfectPed(target, Ped.MaxHealth, true);
                ForgetTarget();
                target.LeaveGroup();
                target.Weapons.Drop();
                EntityEventWrapper.Dispose(target);
            }
        }

        public void ForgetTarget() => Target = null;

        // ===============================
        // 🚶‍♂️ Movimento
        // ===============================

        private void UpdateWalkStyle()
        {
            if (DateTime.UtcNow >= _nextMovementUpdateTime)
            {
                Ped?.SetMovementAnimSet(MovementStyle);
                _nextMovementUpdateTime = DateTime.UtcNow.AddSeconds(MovementUpdateInterval);
            }
        }

        // ===============================
        // 🔍 Detecção de alvo
        // ===============================

        private void GetTarget()
        {
            var nearbyPeds = World.GetNearbyPeds(Ped, SensingRange).Where(IsGoodTarget).ToArray();

            var closest = World.GetClosest(Position, nearbyPeds);

            if (
                closest != null
                && (Ped.HasClearLineOfSight(closest, VisionDistance) || CanHearPed(closest))
            )
            {
                Target = closest;
            }
            else if ((Target != null && !IsGoodTarget(Target)) || closest != Target)
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

        private bool IsGoodTarget(Ped ped)
        {
            return ped != null
                && ped.Exists()
                && ped.GetRelationshipWithPed(Ped) == Relationship.Hate;
        }

        // ===============================
        // 🔗 Operadores e overrides
        // ===============================

        public override bool Equals(object obj)
        {
            return obj is ZumbiPed other && Equals(other);
        }

        protected bool Equals(ZumbiPed other)
        {
            return other != null && Equals(Ped, other.Ped);
        }

        public override int GetHashCode()
        {
            return Ped?.GetHashCode() ?? 0;
        }

        public bool Equals(Ped other) => Ped == other;

        public static implicit operator Ped(ZumbiPed zumbi) => zumbi?.Ped;
    }
}
