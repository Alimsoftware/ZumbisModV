using GTA.Native;

namespace ZumbisModV.Extensions
{
    public static class ScriptExtended
    {
        public static void TerminateScriptByName(string name)
        {
            if (Function.Call<bool>(Hash.DOES_SCRIPT_EXIST, name))
            {
                Function.Call(Hash.TERMINATE_ALL_SCRIPTS_WITH_THIS_NAME, name);
            }
        }
    }
}
