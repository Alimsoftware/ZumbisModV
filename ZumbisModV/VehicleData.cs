using System;
using System.Collections.Generic;
using System.Drawing;
using GTA;
using GTA.Math;
using GTA.Native;
using ZumbisModV.Interfaces;

namespace ZumbisModV
{
    [Serializable]
    public class VehicleData : ISpatial, IHandleable, ZumbisModV.Interfaces.IDeletable
    {
        public int Handle { get; set; }
        public int Hash { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Position { get; set; }
        public VehicleColor PrimaryColor { get; set; }
        public VehicleColor SecondaryColor { get; set; }
        public int Health { get; set; }
        public float EngineHealth { get; set; }
        public float Heading { get; set; }
        public VehicleNeonLight[] NeonLights { get; set; }
        public List<Tuple<VehicleMod, int>> Mods { get; set; }
        public VehicleToggleMod[] ToggleMods { get; set; }
        public VehicleWindowTint WindowTint { get; set; }
        public VehicleWheelType WheelType { get; set; }
        public Color NeonColor { get; set; }
        public int Livery { get; set; }
        public bool Wheels1 { get; set; }
        public bool Wheels2 { get; set; }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="VehicleData"/>.
        /// </summary>
        /// <param name="handle">O identificador exclusivo do objeto.</param>
        /// <param name="hash">O hash representando o tipo do objeto.</param>
        /// <param name="rotation">A rotação do objeto no espaço 3D.</param>
        /// <param name="position">A posição do objeto no espaço 3D.</param>
        /// <param name="primaryColor">A cor primária do objeto.</param>
        /// <param name="secondaryColor">A cor secundária do objeto.</param>
        /// <param name="health">O nível de saúde do objeto.</param>
        /// <param name="engineHealth">A saúde do motor do objeto.</param>
        /// <param name="heading">A direção ou orientação do objeto.</param>
        /// <param name="neonLights">A presença de luzes neon no objeto.</param>
        /// <param name="mods">Os modificadores ou atualizações aplicadas ao objeto.</param>
        /// <param name="toggleMods">A capacidade de alternar os modificadores do objeto.</param>
        /// <param name="windowTint">O nível de escurecimento das janelas do objeto.</param>
        /// <param name="wheelType">O tipo de rodas do objeto.</param>
        /// <param name="neonColor">A cor das luzes neon do objeto.</param>
        /// <param name="livery">A pintura ou adesivo aplicado ao objeto.</param>
        /// <param name="wheels1">A configuração das rodas dianteiras do objeto.</param>
        /// <param name="wheels2">A configuração das rodas traseiras do objeto.</param>

        public VehicleData(
            int handle,
            int hash,
            Vector3 rotation,
            Vector3 position,
            VehicleColor primaryColor,
            VehicleColor secondaryColor,
            int health,
            float engineHealth,
            float heading,
            VehicleNeonLight[] neonLights,
            List<Tuple<VehicleMod, int>> mods,
            VehicleToggleMod[] toggleMods,
            VehicleWindowTint windowTint,
            VehicleWheelType wheelType,
            Color neonColor,
            int livery,
            bool wheels1,
            bool wheels2
        )
        {
            Handle = handle;
            Hash = hash;
            Rotation = rotation;
            Position = position;
            PrimaryColor = primaryColor;
            SecondaryColor = secondaryColor;
            Health = health;
            EngineHealth = engineHealth;
            Heading = heading;
            NeonLights = neonLights;
            Mods = mods ?? new List<Tuple<VehicleMod, int>>();
            ToggleMods = toggleMods;
            WindowTint = windowTint;
            WheelType = wheelType;
            NeonColor = neonColor;
            Livery = livery;
            Wheels1 = wheels1;
            Wheels2 = wheels2;
        }

        public bool Exists()
        {
            return Function.Call<bool>(GTA.Native.Hash.DOES_ENTITY_EXIST, Handle);
        }

        public void Delete()
        {
            int handle = Handle;
            if (Exists())
            {
                Function.Call(GTA.Native.Hash.DELETE_ENTITY, handle);
                Handle = 0;
            }
        }
    }
}
