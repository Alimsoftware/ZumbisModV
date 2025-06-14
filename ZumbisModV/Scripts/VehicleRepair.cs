using System;
using System.Drawing;
using GTA;
using GTA.Math;
using GTA.Native;
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

        private bool isRepairing = false;
        private int repairStartTime = 0;
        private int _repairTimeMs = 7500;

        public VehicleRepair()
        {
            _repairTimeMs = Settings.GetValue(
                "interaction",
                "vehicle_repair_time_ms",
                _repairTimeMs
            );
            Settings.SetValue("interaction", "vehicle_repair_time_ms", _repairTimeMs);
            Settings.Save();
            Tick += OnTick;
            Aborted += OnAborted;
        }

        private static Ped PlayerPed => Database.PlayerPed;

        private static void OnAborted(object sender, EventArgs e)
        {
            PlayerPed.Task.ClearAll();
        }

        private void DrawRepairProgressBar(
            float x,
            float y,
            float width,
            float height,
            float progress,
            Color bgColor,
            Color fgColor
        )
        {
            // Fundo
            Function.Call(
                Hash.DRAW_RECT,
                x,
                y,
                width,
                height,
                bgColor.R,
                bgColor.G,
                bgColor.B,
                bgColor.A
            );
            // Preenchido
            float filledWidth = width * progress;
            float filledX = x - (width / 2) + (filledWidth / 2);
            Function.Call(
                Hash.DRAW_RECT,
                filledX,
                y,
                filledWidth,
                height,
                fgColor.R,
                fgColor.G,
                fgColor.B,
                fgColor.A
            );
        }

        public static float Clamp(float value, float min, float max) =>
            (value < min) ? min
            : (value > max) ? max
            : value;

        private int GetRepairTime(Vehicle vehicle)
        {
            if (vehicle == null || !vehicle.Exists())
                return 10000;

            float health = Math.Max(vehicle.EngineHealth, 0); // Nunca menor que 0

            if (health < 300f)
                return 15000;
            if (health < 700f)
                return 10000;
            return 7000;
        }

        private void OnTick(object sender, EventArgs e)
        {
            int elapsed = Game.GameTime - repairStartTime;
            float progress = Clamp((float)elapsed / _repairTimeMs, 0f, 1f);

            if (!Database.PlayerInVehicle)
            {
                Vehicle closestVehicle = World.GetClosestVehicle(Database.PlayerPosition, 3.0f);
                _repairTimeMs = GetRepairTime(closestVehicle);

                if (_item == null)
                {
                    _item = PlayerInventory.Instance.ItemFromName("Vehicle Repair Kit");
                    _item.Amount = 10;
                }

                if (_selectedVehicle != null)
                {
                    if (isRepairing)
                    {
                        //Desenha barra de progresso
                        DrawRepairProgressBar(
                            0.5f,
                            0.85f,
                            0.2f,
                            0.02f,
                            progress,
                            Color.Gray,
                            Color.Green
                        );

                        // Desenha texto opcional
                        Utils.GTAUtils.DrawHackPanelText(
                            $"Reparando veículo... {Math.Round(progress * 100)}%",
                            0.5f,
                            0.81f,
                            0.5f,
                            Color.White,
                            true
                        );
                    }

                    Vector3 boneCoord = _selectedVehicle.Bones["engine"].Position;
                    Vector2 vector6 = GTAUtils.World3DToScreen2d(boneCoord);

                    GTAUtils.DrawHackPanelText(
                        string.Format("Saúde do motor {0}", _selectedVehicle.EngineHealth),
                        vector6.X,
                        vector6.Y + 0.1f,
                        0.36f,
                        Color.White,
                        true
                    );

                    Game.DisableControlThisFrame(Control.Attack);

                    UiExtended.DisplayHelpTextThisFrame(
                        "Pressione ~INPUT_ATTACK~ para cancelar.",
                        false
                    );

                    // Se o botão Attack for clicado, cancela
                    if (GTAUtils.IsDisabledControlJustPressed(Control.Attack))
                    {
                        CancelRepair();
                        return;
                    }

                    // Se o veículo não existe mais, cancela
                    if (!_selectedVehicle.Exists())
                    {
                        CancelRepair();
                        return;
                    }

                    // Se o veículo se afastou demais, cancela
                    if (!PlayerPed.IsInRange(_selectedVehicle.Position, 5.0f))
                    {
                        CancelRepair();
                        return;
                    }
                    else if (PlayerPed.TaskSequenceProgress == -1)
                    {
                        //Verifica se terminou
                        if (elapsed >= _repairTimeMs)
                        {
                            isRepairing = false;
                        }
                        _selectedVehicle.EngineHealth = 1000f;
                        _selectedVehicle.Repair();
                        _selectedVehicle.Doors[VehicleDoorIndex.Hood].Close(false);
                        _selectedVehicle = null;
                        PlayerInventory.Instance?.AddItem(_item, -1, ItemType.Item);
                        Notification.Show($"Itens: ~r~-1 {_item.Id}");
                    }
                }
                else if (closestVehicle != null)
                {
                    // Verifica se o veículo é um carro, o motor tem saúde suficiente ou algum menu está aberto
                    if (
                        closestVehicle.Model.IsCar
                        || closestVehicle.EngineHealth < 900f
                        || MenuController.MenuPool.AreAnyVisible
                    )
                    {
                        // Verifica se o veículo não está de cabeça para baixo e possui o osso "engine"
                        if (!closestVehicle.IsUpsideDown && closestVehicle.Bones.Contains("engine"))
                        {
                            // Pega a coordenada do osso do motor
                            Vector3 boneCoord = closestVehicle.Bones["engine"].Position;

                            Vector2 vector6 = GTAUtils.World3DToScreen2d(boneCoord);

                            GTAUtils.DrawHackPanelText(
                                string.Format("Saúde do motor {0}", closestVehicle.EngineHealth),
                                vector6.X,
                                vector6.Y + 0.1f,
                                0.36f,
                                Color.White,
                                true
                            );

                            // Verifica se a coordenada do osso é válida e se o jogador está próximo o suficiente

                            if (boneCoord != Vector3.Zero || PlayerPed.IsInRange(boneCoord, 1.5f))
                            {
                                // Verifica se o jogador tem o item necessário para o conserto
                                if (!PlayerInventory.Instance.HasItem(_item, ItemType.Item))
                                {
                                    UiExtended.DisplayHelpTextThisFrame(
                                        "Você precisa de um kit de reparo de veículo para consertar este motor.",
                                        false
                                    );
                                    return;
                                }

                                // Desativa o controle de interação e exibe a mensagem de ajuda
                                Game.DisableControlThisFrame(Control.Context);
                                UiExtended.DisplayHelpTextThisFrame(
                                    "Pressione ~INPUT_CONTEXT~ para reparar o motor.",
                                    false
                                );

                                // Verifica se o botão de interação foi pressionado
                                if (GTAUtils.IsDisabledControlJustPressed(Control.Context))
                                {
                                    // Abre o capô e prepara o jogador para realizar a animação
                                    closestVehicle
                                        ?.Doors[VehicleDoorIndex.Hood]
                                        .Open(false, false);

                                    PlayerPed?.Weapons?.Select(WeaponHash.Unarmed, true);

                                    // Determina a posição e a rotação para o movimento do jogador
                                    Vector3 targetPosition =
                                        boneCoord + closestVehicle.ForwardVector;
                                    float targetHeading = (
                                        closestVehicle.Position - Database.PlayerPosition
                                    ).ToHeading();

                                    // Cria a sequência de tarefas
                                    TaskSequence taskSequence = new TaskSequence();
                                    taskSequence.AddTask.ClearAllImmediately();
                                    taskSequence.AddTask.GoTo(targetPosition, 1500);
                                    taskSequence.AddTask.AchieveHeading(targetHeading, 2000);
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

                                    // Executa a sequência de tarefas
                                    PlayerPed.Task.PerformSequence(taskSequence);
                                    taskSequence.Dispose();

                                    // Marca o veículo como selecionado
                                    _selectedVehicle = closestVehicle;
                                    Wait(3000);
                                    isRepairing = true;
                                    repairStartTime = Game.GameTime + 400;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CancelRepair()
        {
            isRepairing = false;
            PlayerPed.Task.ClearAllImmediately();
            _selectedVehicle?.Doors[VehicleDoorIndex.Hood].Close(false);
            _selectedVehicle = null;
            Notification.Show("~r~Reparo cancelado.");
        }
    }
}
