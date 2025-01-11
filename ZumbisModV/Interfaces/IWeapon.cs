using System;
using GTA;

namespace ZumbisModV.Interfaces
{
    public interface IWeapon
    {
        int Ammo { get; set; }

        WeaponHash Hash { get; set; }

        WeaponComponent[] Components { get; set; }
    }
}
