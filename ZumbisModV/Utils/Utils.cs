using System.Drawing;
using GTA;
using GTA.Math;
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

        public static string checkUpdate()
        {
            try
            {
                var url = "https://pastebin.com/raw/KwPVxJGM";
                return (new System.Net.WebClient()).DownloadString(url);
            }
            catch
            {
                //GTA.UI.Notification.Show(e.ToString());
                return "Failed";
            }
        }

        public static void EsferaTop()
        {
            Vector3 pos = Game.Player.Character.Position;

            World.DrawMarker(
                MarkerType.DebugSphere,
                pos,
                Vector3.Zero,
                Vector3.Zero,
                new Vector3(3.5f, 3.5f, 3.5f),
                Color.FromArgb(120, 255, 0, 0)
            );
        }

        public static Vector3 GetPlayerLookingDirection(Vector3 camPosition)
        {
            if (World.RenderingCamera.IsActive)
            {
                GTA.UI.Notification.Show("render camera acticve");
                camPosition = World.RenderingCamera.Position;
                return World.RenderingCamera.Direction;
            }
            else
            {
                float pitch = Function.Call<float>(Hash.GET_GAMEPLAY_CAM_RELATIVE_PITCH);
                float heading = Function.Call<float>(Hash.GET_GAMEPLAY_CAM_RELATIVE_HEADING);

                camPosition = Function.Call<Vector3>(Hash.GET_GAMEPLAY_CAM_COORD);
                return camPosition;
                //return (Game.Player.Character.Rotation + new Rotator(pitch, 0, heading)).ToVector().ToNormalized();
            }
        }

        public static Vector2 World3DToScreen2d(Vector3 pos)
        {
            OutputArgument outputArgument = new OutputArgument();
            OutputArgument outputArgument2 = new OutputArgument();
            Function.Call<bool>(
                Hash.GET_SCREEN_COORD_FROM_WORLD_COORD,
                pos.X,
                pos.Y,
                pos.Z,
                outputArgument,
                outputArgument2
            );
            return new Vector2(
                outputArgument.GetResult<float>(),
                outputArgument2.GetResult<float>()
            );
        }

        public static void DrawHackPanelText(
            string message,
            float x,
            float y,
            float scale,
            Color color,
            bool centre
        )
        {
            Function.Call(Hash.SET_TEXT_FONT, 6);
            Function.Call(Hash.SET_TEXT_SCALE, scale, scale);
            Function.Call(Hash.SET_TEXT_COLOUR, color.R, color.G, color.B, 220);
            Function.Call(Hash.SET_TEXT_WRAP, 0.0, 1.0);
            Function.Call(Hash.SET_TEXT_CENTRE, centre);
            Function.Call(Hash.SET_TEXT_DROPSHADOW, 2, 2, 0, 0, 0);
            Function.Call(Hash.SET_TEXT_EDGE, 1, 1, 1, 1, 205);
            Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_TEXT, "STRING");
            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, message);
            Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_TEXT, x, y);
        }
    }
}
