using GTA;
using GTA.Native;

namespace ZumbisModV.Utils
{
    public static class GTAUtils
    {
        public static bool IsDisabledControlJustPressed(Control control)
        {
            return Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 0, control)
                && !Function.Call<bool>(Hash.IS_CONTROL_ENABLED, 0, control);
        }

        public static bool IsDisabledControlPressed(int index, Control control)
        {
            return IsControlPressed(index, control) && !IsControlEnabled(0, control);
        }

        public static bool IsControlPressed(int index, Control control)
        {
            return Function.Call<bool>(Hash.IS_DISABLED_CONTROL_PRESSED, 0, (int)control);
        }

        public static bool IsControlEnabled(int index, Control control)
        {
            return Function.Call<bool>(Hash.IS_CONTROL_ENABLED, 0, (int)control);
        }
    }
}
