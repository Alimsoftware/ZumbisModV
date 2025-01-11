using System;
using GTA;
using ZumbisModV.Interfaces;

namespace ZumbisModV
{
    [Serializable]
    public class Weapon : IWeapon
    {
        public int Ammo { get; set; }
        public WeaponHash Hash { get; set; }
        public WeaponComponent[] Components { get; set; }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="Weapon"/>.
        /// </summary>
        /// <param name="ammo">A quantidade de munição inicial do item.</param>
        /// <param name="hash">O hash que representa a arma associada ao item.</param>
        /// <param name="components">Os componentes ou acessórios da arma associados ao item.</param>

        public Weapon(int ammo, WeaponHash hash, WeaponComponent[] components)
        {
            Ammo = ammo;
            Hash = hash;
            Components = components;
        }
    }
}
