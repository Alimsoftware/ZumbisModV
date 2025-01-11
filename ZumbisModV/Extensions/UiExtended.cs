using System;
using GTA.Native;
using ZumbisModV.Static;

namespace ZumbisModV.Extensions
{
    public static class UiExtended
    {
        public static bool IsAnyHelpTextOnScreen()
        {
            return Function.Call<bool>(Hash.IS_HELP_MESSAGE_BEING_DISPLAYED, new InputArgument[0]);
        }

        public static void ClearAllHelpText()
        {
            Function.Call(Hash.CLEAR_ALL_HELP_MESSAGES, new InputArgument[0]);
        }

        public static void DisplayHelpTextThisFrame(string helpText, bool ignoreMenus = false)
        {
            if (!ignoreMenus && MenuController.MenuPool.AreAnyVisible)
                return;
            Function.Call(
                Hash.BEGIN_TEXT_COMMAND_DISPLAY_HELP,
                new InputArgument[1] { "CELL_EMAIL_BCON" }
            );
            for (int startIndex = 0; startIndex < helpText.Length; startIndex += 99)
                Function.Call(
                    Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME,
                    new InputArgument[1]
                    {
                        helpText.Substring(startIndex, Math.Min(99, helpText.Length - startIndex)),
                    }
                );
            Function.Call(
                Hash.END_TEXT_COMMAND_DISPLAY_HELP,
                new InputArgument[4]
                {
                    0,
                    0,
                    Function.Call<bool>(Hash.IS_HELP_MESSAGE_BEING_DISPLAYED, new InputArgument[0])
                        ? 0
                        : 1,
                    -1,
                }
            );
        }
    }
}
