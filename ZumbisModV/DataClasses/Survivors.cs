using GTA;
using GTA.Math;
using ZumbisModV.Extensions;
using ZumbisModV.Scripts;
using ZumbisModV.Static;

namespace ZumbisModV.DataClasses
{
    public abstract class Survivors
    {
        public static float MaxSpawnDistance = 650f;
        public static float MinSpawnDistance = 50f; //400f;
        public static float DeleteRange = 1000f;

        public virtual event SurvivorCompletedEvent Completed;

        public Ped PlayerPed => Database.PlayerPed;

        public Vector3 PlayerPosition => Database.PlayerPosition;

        public abstract void Update();

        public abstract void SpawnEntities();

        public abstract void CleanUp();

        public abstract void Abort();

        public void Complete()
        {
            Completed?.Invoke(this);
        }

        public bool IsValidSpawn(Vector3 spawn)
        {
            if (Database.PlayerPosition == null)
            {
                Logger.LogError($"PlayerPosition is null. Unable to validate spawn at {spawn}.");
                return false;
            }

            if (ZombieVehicleSpawner.Instance == null)
            {
                Logger.LogError(
                    $"ZombieVehicleSpawner.Instance is null. Unable to validate spawn at {spawn}."
                );
                return false;
            }

            bool isFarEnough = spawn.VDist(PlayerPosition) >= MinSpawnDistance;
            bool isNotInInvalidZone = !ZombieVehicleSpawner.Instance.IsInvalidZone(spawn);

            if (isFarEnough && isNotInInvalidZone)
            {
                return true;
            }

            Logger.LogInfo($"Spawn at {spawn} is invalid. Completing process.");
            Complete();
            return false;
        }

        public Vector3 GedtSpawnPoint()
        {
            Vector3 spawn = PlayerPosition.Around(MaxSpawnDistance);
            return World.GetNextPositionOnStreet(spawn);
        }

        public Vector3 GetSpawnPoint()
        {
            const int maxAttempts = 10;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Vector3 randomPosition = PlayerPosition.Around(MaxSpawnDistance);
                Vector3 streetPosition = World.GetNextPositionOnStreet(randomPosition);

                // Verifica se a posição está longe o suficiente do jogador
                if (streetPosition.DistanceTo(PlayerPosition) >= MinSpawnDistance)
                {
                    return streetPosition;
                }
            }

            // Fallback: Retorna a posição do jogador se nenhuma válida for encontrada
            return PlayerPosition;
        }

        public delegate void SurvivorCompletedEvent(Survivors survivors);
    }
}
