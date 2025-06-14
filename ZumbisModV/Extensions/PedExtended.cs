using System;
using GTA;
using GTA.Native;
using ZumbisModV.Wrappers;

namespace ZumbisModV.Extensions
{
    public static class PedExtended
    {
        internal static readonly string[] SpeechModifierNames = new string[37]
        {
            "SPEECH_PARAMS_STANDARD",
            "SPEECH_PARAMS_ALLOW_REPEAT",
            "SPEECH_PARAMS_BEAT",
            "SPEECH_PARAMS_FORCE",
            "SPEECH_PARAMS_FORCE_FRONTEND",
            "SPEECH_PARAMS_FORCE_NO_REPEAT_FRONTEND",
            "SPEECH_PARAMS_FORCE_NORMAL",
            "SPEECH_PARAMS_FORCE_NORMAL_CLEAR",
            "SPEECH_PARAMS_FORCE_NORMAL_CRITICAL",
            "SPEECH_PARAMS_FORCE_SHOUTED",
            "SPEECH_PARAMS_FORCE_SHOUTED_CLEAR",
            "SPEECH_PARAMS_FORCE_SHOUTED_CRITICAL",
            "SPEECH_PARAMS_FORCE_PRELOAD_ONLY",
            "SPEECH_PARAMS_MEGAPHONE",
            "SPEECH_PARAMS_HELI",
            "SPEECH_PARAMS_FORCE_MEGAPHONE",
            "SPEECH_PARAMS_FORCE_HELI",
            "SPEECH_PARAMS_INTERRUPT",
            "SPEECH_PARAMS_INTERRUPT_SHOUTED",
            "SPEECH_PARAMS_INTERRUPT_SHOUTED_CLEAR",
            "SPEECH_PARAMS_INTERRUPT_SHOUTED_CRITICAL",
            "SPEECH_PARAMS_INTERRUPT_NO_FORCE",
            "SPEECH_PARAMS_INTERRUPT_FRONTEND",
            "SPEECH_PARAMS_INTERRUPT_NO_FORCE_FRONTEND",
            "SPEECH_PARAMS_ADD_BLIP",
            "SPEECH_PARAMS_ADD_BLIP_ALLOW_REPEAT",
            "SPEECH_PARAMS_ADD_BLIP_FORCE",
            "SPEECH_PARAMS_ADD_BLIP_SHOUTED",
            "SPEECH_PARAMS_ADD_BLIP_SHOUTED_FORCE",
            "SPEECH_PARAMS_ADD_BLIP_INTERRUPT",
            "SPEECH_PARAMS_ADD_BLIP_INTERRUPT_FORCE",
            "SPEECH_PARAMS_FORCE_PRELOAD_ONLY_SHOUTED",
            "SPEECH_PARAMS_FORCE_PRELOAD_ONLY_SHOUTED_CLEAR",
            "SPEECH_PARAMS_FORCE_PRELOAD_ONLY_SHOUTED_CRITICAL",
            "SPEECH_PARAMS_SHOUTED",
            "SPEECH_PARAMS_SHOUTED_CLEAR",
            "SPEECH_PARAMS_SHOUTED_CRITICAL",
        };

        public static void PlayPain(this Ped ped, int type)
        {
            Function.Call(Hash.PLAY_PAIN, ped.Handle, type, 0, 0);
        }

        public static void PlayFacialAnim(this Ped ped, string animSet, string animName)
        {
            Function.Call(Hash.PLAY_FACIAL_ANIM, ped.Handle, animName, animSet);
        }

        public static bool HasBeenDamagedByMelee(this Ped ped)
        {
            return Function.Call<bool>(Hash.HAS_ENTITY_BEEN_DAMAGED_BY_WEAPON, ped.Handle, 0, 1);
        }

        public static bool HasBeenDamagedBy(this Ped ped, WeaponHash weapon)
        {
            return Function.Call<bool>(
                Hash.HAS_ENTITY_BEEN_DAMAGED_BY_WEAPON,
                ped.Handle,
                (int)weapon,
                0
            );
        }

        public static Bone LastDamagedBone(this Ped ped)
        {
            // Variável para armazenar o número retornado pela função
            int num = 0;

            // Chama a função para obter a resposta
            bool result = Function.Call<bool>(Hash.GET_PED_LAST_DAMAGE_BONE, ped.Handle, num);

            // Se o resultado for verdadeiro, retorna o valor do osso, caso contrário, retorna 0
            return result ? (Bone)num : Bone.SkelRoot;
        }

        public static void SetPathAvoidWater(this Ped ped, bool toggle)
        {
            Function.Call(Hash.SET_PED_PATH_PREFER_TO_AVOID_WATER, ped.Handle, toggle ? 1 : 0);
        }

        public static void SetStealthMovement(this Ped ped, bool toggle)
        {
            Function.Call(Hash.SET_PED_STEALTH_MOVEMENT, toggle ? 1 : 0, "DEFAULT_ACTION");
        }

        public static bool GetStealthMovement(this Ped ped)
        {
            return Function.Call<bool>(Hash.GET_PED_STEALTH_MOVEMENT, ped.Handle);
        }

        public static void SetComponentVariation(
            this Ped ped,
            ComponentId id,
            int drawableId,
            int textureId,
            int paletteId
        )
        {
            Function.Call(
                Hash.SET_PED_COMPONENT_VARIATION,
                ped.Handle,
                (int)id,
                drawableId,
                textureId,
                paletteId
            );
        }

        public static int GetDrawableVariation(this Ped ped, ComponentId id)
        {
            return Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, ped.Handle, (int)id);
        }

        public static int GetNumberOfDrawableVariations(this Ped ped, ComponentId id)
        {
            return Function.Call<int>(
                Hash.GET_NUMBER_OF_PED_DRAWABLE_VARIATIONS,
                ped.Handle,
                (int)id
            );
        }

        public static bool IsSubttaskActive(this Ped ped, Subtask task)
        {
            return Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, ped, (int)task);
        }

        public static bool IsDriving(this Ped ped)
        {
            return ped.IsSubttaskActive(Subtask.DrivingWandering)
                || ped.IsSubttaskActive(Subtask.DrivingGoingToDestinationOrEscorting);
        }

        public static void SetPathCanUseLadders(this Ped ped, bool toggle)
        {
            Function.Call(Hash.SET_PED_PATH_CAN_USE_LADDERS, ped.Handle, toggle ? 1 : 0);
        }

        public static void SetPathCanClimb(this Ped ped, bool toggle)
        {
            Function.Call(Hash.SET_PED_PATH_CAN_USE_CLIMBOVERS, ped.Handle, toggle ? 1 : 0);
        }

        public static void SetMovementAnimSet(this Ped ped, string animation)
        {
            if (ped == null || string.IsNullOrEmpty(animation))
                return; // Previne erros caso o ped ou animação sejam inválidos.

            // Solicita o carregamento da animação, se não estiver carregada
            Function.Call(Hash.REQUEST_ANIM_SET, animation);

            // Aguarda até que o conjunto de animações esteja carregado
            while (!Function.Call<bool>(Hash.HAS_ANIM_SET_LOADED, animation))
            {
                Script.Yield(); // Dá uma pausa para que o script continue executando outras operações.
            }

            // Define o clipset de movimento para o ped
            Function.Call(Hash.SET_PED_MOVEMENT_CLIPSET, ped.Handle, animation, 1048576000);
        }

        public static void RemoveElegantly(this Ped ped)
        {
            Function.Call(Hash.REMOVE_PED_ELEGANTLY, ped.Handle);
        }

        public static void SetRagdollOnCollision(this Ped ped, bool toggle)
        {
            Function.Call(Hash.SET_PED_RAGDOLL_ON_COLLISION, ped.Handle, toggle);
        }

        public static void SetAlertness(this Ped ped, Alertness alertness)
        {
            Function.Call(Hash.SET_PED_ALERTNESS, ped.Handle, (int)alertness);
        }

        public static void SetCombatAblility(this Ped ped, CombatAbility ability)
        {
            Function.Call(Hash.SET_PED_COMBAT_ABILITY, ped.Handle, (int)ability);
        }

        public static void SetCanEvasiveDive(this Ped ped, bool toggle)
        {
            Function.Call(Hash.SET_PED_CAN_EVASIVE_DIVE, ped.Handle, toggle ? 1 : 0);
        }

        public static void StopAmbientSpeechThisFrame(this Ped ped)
        {
            if (!ped.IsAmbientSpeechPlaying())
                return;
            Function.Call(Hash.STOP_CURRENT_PLAYING_AMBIENT_SPEECH, ped.Handle);
        }

        public static bool IsAmbientSpeechPlaying(this Ped ped)
        {
            return Function.Call<bool>(Hash.IS_AMBIENT_SPEECH_PLAYING, ped.Handle);
        }

        public static void DisablePainAudio(this Ped ped, bool toggle)
        {
            Function.Call(Hash.DISABLE_PED_PAIN_AUDIO, ped.Handle, toggle ? 1 : 0);
        }

        public static void StopSpeaking(this Ped ped, bool shaking)
        {
            Function.Call(Hash.STOP_PED_SPEAKING, ped.Handle, shaking ? 1 : 0);
        }

        public static void SetCanPlayAmbientAnims(this Ped ped, bool toggle)
        {
            Function.Call(Hash.SET_PED_CAN_PLAY_AMBIENT_ANIMS, ped.Handle, toggle ? 1 : 0);
        }

        public static void SetCombatAttributess(
            this Ped ped,
            CombatAttributes attribute,
            bool enabled
        )
        {
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, ped.Handle, (int)attribute, enabled);
        }

        public static void SetPathAvoidFires(this Ped ped, bool toggle)
        {
            Function.Call(Hash.SET_PED_PATH_AVOID_FIRE, ped.Handle, toggle ? 1 : 0);
        }

        public static void ApplyDamagePack(
            this Ped ped,
            float damage,
            float multiplier,
            DamagePack damagePack
        )
        {
            Function.Call(
                Hash.APPLY_PED_DAMAGE_PACK,
                ped.Handle,
                damagePack.ToString(),
                damage,
                multiplier
            );
        }

        public static void SetCanAttackFriendlies(this Ped ped, FirendlyFireType type)
        {
            // Define o valor de ataque a amigos dependendo do tipo
            bool canAttack = type == FirendlyFireType.CanAttack;

            // Chama a função com os parâmetros necessários
            Function.Call(
                Hash.SET_CAN_ATTACK_FRIENDLY,
                ped.Handle,
                canAttack, // Valor calculado com base no tipo
                false // O valor do terceiro parâmetro sempre é false
            );
        }

        public static void PlayAmbientSpeech(
            this Ped ped,
            string speechName,
            SpeechModifier modifier = SpeechModifier.Standard
        )
        {
            // Verifica se o modificador está dentro do intervalo válido
            if (
                modifier < SpeechModifier.Standard
                || modifier >= (SpeechModifier)PedExtended.SpeechModifierNames.Length
            )
                throw new ArgumentOutOfRangeException(nameof(modifier));

            // Chama a função para tocar o discurso ambiental
            Function.Call(
                Hash.PLAY_PED_AMBIENT_SPEECH_NATIVE,
                ped.Handle,
                speechName,
                SpeechModifierNames[(int)modifier]
            );
        }

        public static void Recruit(
            this Ped ped,
            Ped leader,
            bool canBeTargeted,
            bool invincible,
            int accuracy
        )
        {
            // Verifica se o líder é nulo e retorna se for o caso
            if (leader == null)
                return;

            // Faz o ped deixar o grupo atual
            ped.LeaveGroup();

            // Desabilita ragdoll e limpa tarefas
            ped.SetRagdollOnCollision(false);
            ped.Task.ClearAll();

            // Obtém o grupo do líder e ajusta a separação
            PedGroup currentPedGroup = leader.PedGroup;
            currentPedGroup.SeparationRange = (float)int.MaxValue;

            // Adiciona o líder e o ped ao grupo, se necessário
            if (!currentPedGroup.Contains(leader))
                currentPedGroup.Add(leader, true);
            if (!currentPedGroup.Contains(ped))
                currentPedGroup.Add(ped, false);

            // Define as propriedades do ped
            ped.CanBeTargetted = canBeTargeted;
            ped.Accuracy = accuracy;
            ped.IsInvincible = invincible;
            ped.IsPersistent = true;
            ped.RelationshipGroup = leader.RelationshipGroup;
            ped.NeverLeavesGroup = true;

            // Remove o blip atual, se houver, e cria um novo
            ped.AttachedBlip?.Delete();
            Blip blip = ped.AddBlip();
            blip.Color = BlipColor.Blue;
            blip.Scale = 0.7f;
            blip.Name = "Amigo";

            // Define o evento de morte para remover o blip e liberar recursos
            var wrapper = new EntityEventWrapper(ped);
            wrapper.Died += (sender, args) =>
            {
                args.Entity.AttachedBlip?.Delete();
                wrapper.Dispose();
            };

            // Reproduz o som de saudação
            ped.PlayAmbientSpeech("GENERIC_HI");
        }

        public static void Recruit(this Ped ped, Ped leader, bool canBeTargetted)
        {
            ped.Recruit(leader, canBeTargetted, false, 100);
        }

        public static void Recruit(this Ped ped, Ped leader) => ped.Recruit(leader, true);

        public static void SetCombatRange(this Ped ped, CombatRange range)
        {
            Function.Call(Hash.SET_PED_COMBAT_RANGE, ped.Handle, (int)range);
        }

        public static void SetCombatMovement(this Ped ped, CombatMovement movement)
        {
            Function.Call(Hash.SET_PED_COMBAT_MOVEMENT, ped.Handle, (int)movement);
        }

        public static void ClearFleeAttributes(this Ped ped)
        {
            Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, ped.Handle, 0, 0);
        }

        public static bool IsUsingAnyScenario(this Ped ped)
        {
            return Function.Call<bool>(Hash.IS_PED_USING_ANY_SCENARIO, ped.Handle);
        }

        public static bool CanHearPlayer(this Ped ped, Player player)
        {
            return Function.Call<bool>(Hash.CAN_PED_HEAR_PLAYER, player.Handle, ped.Handle);
        }

        public static void SetHearingRange(this Ped ped, float hearingRange)
        {
            Function.Call(Hash.SET_PED_HEARING_RANGE, ped.Handle, hearingRange);
        }

        public static bool IsCurrentWeaponSileced(this Ped ped)
        {
            return Function.Call<bool>(Hash.IS_PED_CURRENT_WEAPON_SILENCED, ped.Handle);
        }

        public static void Jump(this Ped ped)
        {
            Function.Call(
                Hash.TASK_JUMP,
                ped.Handle, // Usando Handle diretamente, sem necessidade de conversão
                true, // Valor booleano verdadeiro
                0, // Parâmetro adicional (aqui pode ser ajustado conforme necessário)
                0
            );
        }

        public static void SetToRagdoll(this Ped ped, int time)
        {
            Function.Call(
                Hash.TASK_GO_TO_ENTITY,
                ped.Handle, // Handle da entidade
                time, // Tempo do ragdoll
                0, // Parametro extra (pode ser ajustado conforme necessário)
                0, // Parametro extra (pode ser ajustado conforme necessário)
                0, // Parametro extra (pode ser ajustado conforme necessário)
                0, // Parametro extra (pode ser ajustado conforme necessário)
                0
            );
        }
    }
}
