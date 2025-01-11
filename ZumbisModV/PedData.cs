using System;
using System.Collections.Generic;
using GTA;
using GTA.Math;
using GTA.Native;
using ZumbisModV.Interfaces;
using ZumbisModV.PlayerManagement;

namespace ZumbisModV
{
    [Serializable]
    public class PedData : ISpatial, IHandleable, Interfaces.IDeletable
    {
        public int Handle { get; set; }
        public int Hash { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Position { get; set; }
        public PedTask Task { get; set; }
        public List<Weapon> Weapons { get; set; }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="PedData"/>.
        /// </summary>
        /// <param name="handle">O identificador exclusivo do objeto.</param>
        /// <param name="hash">O hash representando o tipo do objeto.</param>
        /// <param name="rotation">A rotação do objeto no espaço 3D.</param>
        /// <param name="position">A posição do objeto no espaço 3D.</param>
        /// <param name="task">A tarefa ou ação associada ao objeto.</param>
        /// <param name="weapons">A lista de armas associadas ao objeto.</param>

        public PedData(
            int handle,
            int hash,
            Vector3 rotation,
            Vector3 position,
            PedTask task,
            List<Weapon> weapons
        )
        {
            Handle = handle;
            Hash = hash;
            Rotation = rotation;
            Position = position;
            Task = task;
            Weapons = weapons;
        }

        public bool Exists()
        {
            return Function.Call<bool>(GTA.Native.Hash.DOES_ENTITY_EXIST, Handle);
        }

        public void Delete()
        {
            int handle = Handle;
            Function.Call(GTA.Native.Hash.DELETE_ENTITY, handle);
            Handle = handle;
        }
    }
}
