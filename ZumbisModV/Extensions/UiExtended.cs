using System;
using GTA.Native;
using ZumbisModV.Static;

namespace ZumbisModV.Extensions
{
    public static class UiExtended
    {
        public static bool IsAnyHelpTextOnScreen()
        {
            return Function.Call<bool>(Hash.IS_HELP_MESSAGE_BEING_DISPLAYED, 0);
        }

        public static void ClearAllHelpText()
        {
            Function.Call(Hash.CLEAR_ALL_HELP_MESSAGES, 0);
        }

        public static void DisplayHelpTextThisFrame(string helpText, bool ignoreMenus = false)
        {
            if (!ignoreMenus && MenuController.MenuPool.AreAnyVisible)
            {
                return;
            }

            Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_HELP, "CELL_EMAIL_BCON");

            for (int startIndex = 0; startIndex < helpText.Length; startIndex += 99)
                Function.Call(
                    Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME,
                    helpText.Substring(startIndex, Math.Min(99, helpText.Length - startIndex))
                );
            Function.Call(
                Hash.END_TEXT_COMMAND_DISPLAY_HELP,
                0,
                0,
                Function.Call<bool>(Hash.IS_HELP_MESSAGE_BEING_DISPLAYED, 0) ? 0 : 1,
                -1
            );
        }

        public static void DisplayHelpTextThisFrame2(string helpText, bool formal = false)
        {
            Function.Call(Hash.DISPLAY_HELP_TEXT_THIS_FRAME, helpText, formal);
        }

        public static void DisplayHelpTextThisFrame3(string helpText, bool ignoreMenus = false)
        {
            if (!ignoreMenus && MenuController.MenuPool.AreAnyVisible)
                return;
            Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_HELP, "CELL_EMAIL_BCON");
            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, helpText);
            Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_HELP, 0, 0, 1, -1);
        }
    }
}
