using GTA;
using GTA.Native;

namespace ZumbisModV.Extensions
{
    public static class GameExtended
    {
        public static void DisableWeaponWheel()
        {
            Game.DisableControlThisFrame(Control.WeaponWheelLeftRight); //13
            Game.DisableControlThisFrame(Control.WeaponWheelNext); //14
            Game.DisableControlThisFrame(Control.WeaponWheelPrev); //15
            Game.DisableControlThisFrame(Control.WeaponWheelUpDown); //12
            Game.DisableControlThisFrame(Control.NextWeapon); //262
            Game.DisableControlThisFrame(Control.DropWeapon); //56
            Game.DisableControlThisFrame(Control.PrevWeapon); //261
            Game.DisableControlThisFrame(Control.WeaponSpecial); //53
            Game.DisableControlThisFrame(Control.WeaponSpecial2); //54
            Game.DisableControlThisFrame(Control.SelectWeapon); //37
        }

        public static int GetMobilePhoneId()
        {
            return Function.Call<int>(Hash.GET_MOBILE_PHONE_RENDER_ID);
        }
    }
}
