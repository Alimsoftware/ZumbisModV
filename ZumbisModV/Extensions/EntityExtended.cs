using GTA;
using GTA.Native;

namespace ZumbisModV.Extensions
{
    public static class EntityExtended
    {
        public static bool IsPlayingAnim(this Entity entity, string animSet, string animName)
        {
            return Function.Call<bool>(
                Hash.IS_ENTITY_PLAYING_ANIM,
                entity.Handle,
                animSet,
                animName,
                3
            );
        }

        public static void Fade(this Entity entity, bool state)
        {
            Function.Call(Hash.NETWORK_FADE_IN_ENTITY, entity.Handle, state ? 1 : 0);
        }

        public static bool HasClearLineOfSight(
            this Entity entity,
            Entity target,
            float visionDistance
        )
        {
            return Function.Call<bool>(
                    Hash.HAS_ENTITY_CLEAR_LOS_TO_ENTITY_IN_FRONT,
                    entity.Handle,
                    target.Handle
                )
                && entity.Position.VDist(target.Position) < visionDistance;
        }
    }
}
