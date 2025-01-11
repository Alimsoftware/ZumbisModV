using System;
using GTA;
using GTA.Math;
using GTA.UI;
using ZumbisModV.Extensions;
using ZumbisModV.PlayerManagement;
using ZumbisModV.Static;
using ZumbisModV.Utils;

namespace ZumbisModV.Scripts
{
    public class VehicleRepair : Script
    {
        private Vehicle _selectedVehicle;
        private InventoryItemBase _item;
        private readonly int _repairTimeMs = 7500;

        public VehicleRepair()
        {
            _repairTimeMs = Settings.GetValue(
                "interaction",
                "vehicle_repair_time_ms",
                _repairTimeMs
            );
            Settings.SetValue("interaction", "vehicle_repair_time_ms", _repairTimeMs);
            Settings.Save();
            Tick += new EventHandler(OnTick);
            Aborted += OnAborted;
        }

        private static Ped PlayerPed => PlayerPed;

        private static void OnAborted(object sender, EventArgs e)
        {
            PlayerPed.Task.ClearAll();
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!Database.PlayerInVehicle)
            {
                Vehicle closestVehicle = World.GetClosestVehicle(Database.PlayerPosition, 20f);

                if (_item == null)
                {
                    _item = PlayerInventory.Instance.ItemFromName("Vehicle Repair Kit");
                }

                if (_selectedVehicle != null)
                {
                    Game.DisableControlThisFrame(Control.Attack);
                    UiExtended.DisplayHelpTextThisFrame(
                        "Pressione ~INPUT_ATTACK~ para cancelar.",
                        false
                    );

                    if (GTAUtils.IsDisabledControlJustPressed(Control.Attack))
                    {
                        //PlayerPed.Task.ClearAll();
                        PlayerPed.Task.ClearAllImmediately();
                        //VehicleDoorCollection doors = _selectedVehicle.Doors;
                        //doors[VehicleDoorIndex.Hood].Close(false);
                        _selectedVehicle.Doors[VehicleDoorIndex.Hood].Close(false);
                        _selectedVehicle = null;

                        //PlayerPed.Task.ClearAllImmediately();
                        //_selectedVehicle.CloseDoor(4, false);
                        //_selectedVehicle = null;
                    }
                    else if (PlayerPed.TaskSequenceProgress == -1)
                    {
                        _selectedVehicle.EngineHealth = 1000f;
                        _selectedVehicle.Doors[VehicleDoorIndex.Hood].Close(false);
                        _selectedVehicle = null;
                        PlayerInventory.Instance.AddItem(_item, -1, ItemType.Item);
                        Notification.Show("Itens: -~r~1");
                    }
                }
                else if (closestVehicle != null)
                {
                    if (
                        closestVehicle.Model.IsCar
                        || closestVehicle.EngineHealth >= 1000f
                        || MenuController.MenuPool.AreAnyVisible
                    )
                    {
                        if (!closestVehicle.IsUpsideDown)
                        {
                            if (closestVehicle.Bones.Contains("engine"))
                            {
                                Vector3 boneCoord = closestVehicle.Bones["engine"].Position;

                                if (
                                    boneCoord != Vector3.Zero
                                    || PlayerPed.IsInRange(boneCoord, 0.1f)
                                )
                                {
                                    if (!PlayerInventory.Instance.HasItem(_item, ItemType.Item))
                                    {
                                        UiExtended.DisplayHelpTextThisFrame(
                                            "Você precisa de um kit de reparo de veículo para consertar este motor.",
                                            false
                                        );
                                    }
                                    else
                                    {
                                        Game.DisableControlThisFrame(Control.Context);
                                        UiExtended.DisplayHelpTextThisFrame(
                                            "Pressione ~INPUT_CONTEXT~ para reparar o motor.",
                                            false
                                        );

                                        if (GTAUtils.IsDisabledControlJustPressed(Control.Context))
                                        {
                                            closestVehicle.Delete();
                                            Notification.Show("Deletado: -~r~1");
                                            /*closestVehicle
                                                .Doors[VehicleDoorIndex.Hood]
                                                .Open(false, false);
                                            PlayerPed.Weapons.Select(WeaponHash.Unarmed, true);

                                            Vector3 vector =
                                                boneCoord + closestVehicle.ForwardVector;
                                            float num = (
                                                closestVehicle.Position - Database.PlayerPosition
                                            ).ToHeading();
                                            TaskSequence taskSequence = new TaskSequence();
                                            taskSequence.AddTask.ClearAllImmediately();
                                            taskSequence.AddTask.GoTo(vector, 1500);
                                            taskSequence.AddTask.AchieveHeading(num, 2000);
                                            taskSequence.AddTask.PlayAnimation(
                                                animDict: "mp_intro_seq@",
                                                animName: "mp_mech_fix",
                                                blendInSpeed: 8f,
                                                blendOutSpeed: -8f,
                                                duration: _repairTimeMs,
                                                flags: AnimationFlags.Loop,
                                                playbackRate: 1f
                                            );
                                            taskSequence.AddTask.ClearAll();
                                            taskSequence.Close();
                                            PlayerPed.Task.PerformSequence(taskSequence);
                                            taskSequence.Dispose();
                                            _selectedVehicle = closestVehicle;
                                            */
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
