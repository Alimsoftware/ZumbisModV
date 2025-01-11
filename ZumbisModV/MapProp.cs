using System;
using System.Collections.Generic;
using GTA;
using GTA.Math;
using GTA.Native;
using ZumbisModV.Interfaces;

namespace ZumbisModV
{
    [Serializable]
    public class MapProp
        : IMapObject,
            IIdentifier,
            IProp,
            ISpatial,
            IHandleable,
            Interfaces.IDeletable
    {
        public string Id { get; set; }
        public string PropName { get; set; }
        public BlipSprite BlipSprite { get; set; }
        public BlipColor BlipColor { get; set; }
        public Vector3 GroundOffset { get; set; }
        public bool Interactable { get; set; }
        public bool IsDoor { get; set; }
        public bool CanBePickedUp { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Position { get; set; }
        public int Handle { get; set; }
        public List<Weapon> Weapons { get; set; }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="MapProp"/>.
        /// </summary>
        /// <param name="id">O identificador único do item.</param>
        /// <param name="propName">O nome do objeto associado ao item.</param>
        /// <param name="blipSprite">O ícone de mapa utilizado para o item.</param>
        /// <param name="blipColor">A cor do ícone de mapa utilizado para o item.</param>
        /// <param name="groundOffset">A altura do item em relação ao solo.</param>
        /// <param name="interactable">Indica se o item pode ser interagido.</param>
        /// <param name="isDoor">Indica se o item é uma porta.</param>
        /// <param name="canBePickedUp">Indica se o item pode ser coletado.</param>
        /// <param name="rotation">A rotação do item no espaço 3D.</param>
        /// <param name="position">A posição do item no espaço 3D.</param>
        /// <param name="handle">O identificador exclusivo do objeto.</param>
        /// <param name="weapons">A lista de armas associadas ao item.</param>

        public MapProp(
            string id,
            string propName,
            BlipSprite blipSprite,
            BlipColor blipColor,
            Vector3 groundOffset,
            bool interactable,
            bool isDoor,
            bool canBePickedUp,
            Vector3 rotation,
            Vector3 position,
            int handle,
            List<Weapon> weapons
        )
        {
            Id = id;
            PropName = propName;
            BlipSprite = blipSprite;
            BlipColor = blipColor;
            GroundOffset = groundOffset;
            Interactable = interactable;
            IsDoor = isDoor;
            CanBePickedUp = canBePickedUp;
            Rotation = rotation;
            Position = position;
            Handle = handle;
            Weapons = weapons;
        }

        public bool Exists()
        {
            return Function.Call<bool>(Hash.DOES_ENTITY_EXIST, Handle);
        }

        public void Delete()
        {
            int handle = Handle;
            Function.Call(Hash.DELETE_ENTITY, handle);
            Handle = handle;
        }
    }
}
