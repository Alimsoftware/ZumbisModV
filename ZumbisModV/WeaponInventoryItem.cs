using System;
using GTA;
using ZumbisModV.Interfaces;

namespace ZumbisModV
{
    [Serializable]
    public class WeaponInventoryItem : InventoryItemBase, IWeapon, ICraftable
    {
        public int Ammo { get; set; }
        public WeaponHash Hash { get; set; }
        public CraftableItemComponent[] RequiredComponents { get; set; }
        public WeaponComponent[] Components { get; set; }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="WeaponInventoryItem"/>.
        /// </summary>
        /// <param name="amount">A quantidade inicial do item.</param>
        /// <param name="maxAmount">A quantidade máxima  do item.</param>
        /// <param name="id">O identificador único do item.</param>
        /// <param name="description">A descrição detalhada do item.</param>
        /// <param name="ammo">A quantidade de munição inicial do item.</param>
        /// <param name="weaponHash">O hash que representa a arma associada ao item.</param>
        /// <param name="weaponComponents">Os componentes ou acessórios da arma associados ao item.</param>
        public WeaponInventoryItem(
            int amount,
            int maxAmount,
            string id,
            string description,
            int ammo,
            WeaponHash weaponHash,
            WeaponComponent[] weaponComponents
        )
            : base(amount, maxAmount, id, description)
        {
            Ammo = ammo;
            Hash = weaponHash;
            Components = weaponComponents;
        }
    }
}
