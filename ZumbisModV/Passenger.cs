using System.Windows.Forms;
using GTA;
using GTA.UI;

namespace ZumbisModV
{
    public class Passenger : Script
    {
        public Passenger()
        {
            this.KeyDown += OnGPressed;
        }

        private void OnGPressed(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.G)
            {
                var vehicle = World.GetClosestVehicle(Game.Player.Character.Position, 5);
                if (vehicle == null)
                {
                    Notification.Show("Nenhum veículo por perto!", false);
                }
                else
                {
                    Game.Player.Character.Task.EnterVehicle(vehicle, VehicleSeat.Passenger);
                }
            }
        }
    }
}
