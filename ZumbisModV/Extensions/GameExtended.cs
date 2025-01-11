using GTA;
using GTA.Native;

namespace ZumbisModV.Extensions
{
    public static class GameExtended
    {
        public static void DisableWeaponWheel()
        {
            Game.DisableControlThisFrame((Control)13);
            Game.DisableControlThisFrame((Control)14);
            Game.DisableControlThisFrame((Control)15);
            Game.DisableControlThisFrame((Control)12);
            Game.DisableControlThisFrame((Control)262);
            Game.DisableControlThisFrame((Control)56);
            Game.DisableControlThisFrame((Control)261);
            Game.DisableControlThisFrame((Control)53);
            Game.DisableControlThisFrame((Control)54);
            Game.DisableControlThisFrame((Control)37);
        }

        public static int GetMobilePhoneId()
        {
            OutputArgument outArg = new OutputArgument();
            Function.Call(Hash.GET_MOBILE_PHONE_RENDER_ID, new InputArgument[] { outArg });
            return outArg.GetResult<int>();
        }
    }
}
