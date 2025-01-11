using System;
using System.Collections.Generic;
using GTA;
using GTA.Math;

namespace ZumbisModV
{
    [Serializable]
    public class WeaponStorageInventoryItem : BuildableInventoryItem
    {
        public List<Weapon> WeaponsList { get; set; }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="WeaponStorageInventoryItem"/>.
        /// </summary>
        /// <param name="amount">A quantidade inicial do item.</param>
        /// <param name="maxAmount">A quantidade máxima permitida do item.</param>
        /// <param name="id">O identificador único do item.</param>
        /// <param name="description">A descrição detalhada do item.</param>
        /// <param name="propName">O nome do objeto associado ao item.</param>
        /// <param name="blipSprite">O ícone de mapa utilizado para o item.</param>
        /// <param name="blipColor">A cor do ícone de mapa utilizado para o item.</param>
        /// <param name="groundOffset">A altura do item em relação ao solo.</param>
        /// <param name="interactable">Indica se o item pode ser interagido.</param>
        /// <param name="isDoor">Indica se o item é uma porta.</param>
        /// <param name="canBePickedUp">Indica se o item pode ser coletado.</param>
        /// <param name="weaponsList">A lista de armas associadas ao item.</param>

        public WeaponStorageInventoryItem(
            int amount,
            int maxAmount,
            string id,
            string description,
            string propName,
            BlipSprite blipSprite,
            BlipColor blipColor,
            Vector3 groundOffset,
            bool interactable,
            bool isDoor,
            bool canBePickedUp,
            List<Weapon> weaponsList
        )
            : base(
                amount,
                maxAmount,
                id,
                description,
                propName,
                blipSprite,
                blipColor,
                groundOffset,
                interactable,
                isDoor,
                canBePickedUp
            )
        {
            WeaponsList = weaponsList;
        }
    }
}
