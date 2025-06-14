using System;
using System.Collections.Generic;
using System.Linq;
using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;
using LemonUI.Menus;
using ZumbisModV;
using ZumbisModV.Extensions;
using ZumbisModV.Static;
using ZumbisModV.Utils;
using ZumbisModV.Wrappers;

namespace ZumbisModV.PlayerManagement
{
    public class PlayerGroupManager : Script
    {
        private readonly NativeMenu _pedMenu;
        private Ped _selectedPed;
        private PedCollection _peds;
        private readonly Dictionary<Ped, PedTask> _pedTasks = new Dictionary<Ped, PedTask>();

        public PlayerGroupManager()
        {
            Instance = this;
            _pedMenu = new NativeMenu("Guarda", "SELECIONE UMA OPÇÃO");
            MenuController.MenuPool.Add(_pedMenu);
            _pedMenu.Closing += (sender, e) =>
            {
                _selectedPed = null;
            };

            // Cria uma lista de strings baseada no enum PedTask
            string[] pedTaskArray = Enum.GetNames(typeof(PedTask));
            var tasksItem = new NativeListItem<String>(
                "Tarefas",
                "Dê aos pedestres uma tarefa específica para realizar.",
                pedTaskArray
            );

            tasksItem.Activated += (sender, item) =>
            {
                if (_selectedPed != null)
                {
                    PedTask task = (PedTask)tasksItem.SelectedIndex;
                    SetTask(_selectedPed, task);
                }
            };
            NativeItem uimenuItem = new NativeItem(
                "Aplicar aos Próximos.",
                "Aplique a tarefa selecionada a pedestres próximos em um raio de 50 metros."
            );
            uimenuItem.Activated += (sender, item) =>
            {
                PedTask task = (PedTask)tasksItem.SelectedIndex;
                List<Ped> peds = (
                    from ped in PlayerPed.PedGroup
                    where ped.Position.VDist(PlayerPosition) < 50f
                    select ped
                ).ToList();
                peds.ForEach(
                    (Ped ped) =>
                    {
                        SetTask(ped, task);
                    }
                );
            };
            NativeItem uimenuItem2 = new NativeItem("Dar Arma", "Dê a este ped sua arma atual.");
            uimenuItem2.Activated += (sender, item) =>
            {
                bool flag = _selectedPed == null;
                if (!flag)
                {
                    TradeWeapons(PlayerPed, _selectedPed);
                }
            };
            NativeItem uimenuItem3 = new NativeItem("Tomar Arma", "Tome a arma atual do ped.");
            uimenuItem3.Activated += (sender, item) =>
            {
                bool flag = _selectedPed == null;
                if (!flag)
                {
                    TradeWeapons(_selectedPed, PlayerPed);
                }
            };

            string[] guardTaskArray = Enum.GetNames(typeof(PedTask));
            var globalTasks = new NativeListItem<String>(
                "Tarefas dos Guardas",
                "Dê a todos os guardas uma tarefa específica para executar.",
                guardTaskArray
            );
            globalTasks.Activated += (sender, item) =>
            {
                PedTask task = (PedTask)globalTasks.SelectedIndex;
                List<Ped> peds = PlayerPed.PedGroup.ToList<Ped>();
                peds.ForEach(
                    (Ped ped) =>
                    {
                        SetTask(ped, task);
                    }
                );
            };
            _pedMenu.Add(tasksItem);
            _pedMenu.Add(uimenuItem);
            _pedMenu.Add(uimenuItem2);
            _pedMenu.Add(uimenuItem3);
            ModController.Instance.MainMenu.Add(globalTasks);
            Tick += OnTick;
            Aborted += OnAborted;
        }

        public Ped PlayerPed
        {
            get { return Database.PlayerPed; }
        }

        public Vector3 PlayerPosition
        {
            get { return Database.PlayerPosition; }
        }

        public static PlayerGroupManager Instance { get; private set; }

        private void SetTask(Ped ped, PedTask task)
        {
            bool flag = task == (PedTask)(-1);
            if (!flag)
            {
                bool isPlayer = ped.IsPlayer;
                if (!isPlayer)
                {
                    bool flag2 = !_pedTasks.ContainsKey(ped);
                    if (flag2)
                    {
                        _pedTasks.Add(ped, task);
                    }
                    else
                    {
                        _pedTasks[ped] = task;
                    }
                    ped.Task.ClearAll();
                    switch (task)
                    {
                        case PedTask.StandStill:
                            ped.Task.StandStill(-1);
                            break;
                        case PedTask.Guard:
                            ped.Task.GuardCurrentPosition();
                            break;
                        case PedTask.VehicleFollow:
                        {
                            Vehicle closestVehicle = World.GetClosestVehicle(ped.Position, 100f);

                            if (closestVehicle == null)
                            {
                                Notification.Show("Não há nenhum veículo perto deste ped.", true);
                                return;
                            }
                            Function.Call(
                                Hash.TASK_VEHICLE_FOLLOW,
                                ped.Handle,
                                closestVehicle.Handle,
                                PlayerPed.Handle,
                                1074528293,
                                262144,
                                15
                            );
                            break;
                        }
                        case PedTask.Combat:
                            ped.Task.FightAgainstHatedTargets(100f);
                            break;
                        case PedTask.Chill:
                            Function.Call(
                                Hash.TASK_USE_NEAREST_SCENARIO_TO_COORD,
                                ped.Handle,
                                ped.Position.X,
                                ped.Position.Y,
                                ped.Position.Z,
                                100f,
                                -1
                            );
                            break;
                        case PedTask.Leave:
                        {
                            ped?.LeaveGroup();
                            ped?.AttachedBlip?.Delete();
                            ped?.MarkAsNoLongerNeeded();
                            EntityEventWrapper.Dispose(ped);

                            break;
                        }
                    }
                    ped.BlockPermanentEvents = (task == PedTask.Follow);
                }
            }
        }

        private void OnAborted(object sender, EventArgs eventArgs)
        {
            PedGroup group = PlayerPed.PedGroup;
            List<Ped> peds = (from ped in @group where !ped.IsPlayer select ped).ToList<Ped>();
            group?.Dispose();
            while (peds.Count > 0)
            {
                Ped ped2 = peds[0];
                ped2?.Delete();
                peds?.RemoveAt(0);
            }
        }

        private void OnTick(object sender, EventArgs eventArgs)
        {
            if (!PlayerPed.IsInVehicle())
            {
                if (!MenuController.MenuPool.AreAnyVisible)
                {
                    if (PlayerPed.PedGroup.MemberCount > 0)
                    {
                        Ped[] nearbyPeds = World.GetNearbyPeds(PlayerPed, 1.5f);
                        Ped closest = World.GetClosest(PlayerPosition, nearbyPeds);

                        if (closest != null)
                        {
                            if (!closest.IsInVehicle())
                            {
                                if (closest.PedGroup == PlayerPed.PedGroup)
                                {
                                    Game.DisableControlThisFrame(Control.Context);
                                    UiExtended.DisplayHelpTextThisFrame(
                                        "Pressione ~INPUT_CONTEXT~ para configurar este ped.",
                                        false
                                    );

                                    if (GTAUtils.IsDisabledControlJustPressed(Control.Context))
                                    {
                                        _selectedPed = closest;
                                        _pedMenu.Visible = !_pedMenu.Visible;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void Deserialize()
        {
            if (_peds == null)
            {
                PedCollection des = Serializer.Deserialize<PedCollection>("./scripts/Guards.dat");

                if (des == null)
                {
                    des = new PedCollection();
                }
                _peds = des;
                _peds.ListChanged += (PedCollection count) =>
                {
                    Serializer.Serialize("./scripts/Guards.dat", _peds);
                };
                _peds
                    .ToList()
                    .ForEach(
                        (PedData data) =>
                        {
                            Ped ped = World.CreatePed(data.Hash, data.Position);

                            if (ped != null)
                            {
                                ped.Rotation = data.Rotation;
                                ped.Recruit(PlayerPed);
                                data.Weapons.ForEach(
                                    (Weapon w) =>
                                    {
                                        ped.Weapons.Give(w.Hash, w.Ammo, true, true);
                                    }
                                );
                                data.Handle = ped.Handle;
                                SetTask(ped, data.Task);
                            }
                        }
                    );
            }
        }

        public void SavePeds()
        {
            bool flag = _peds == null;
            if (flag)
            {
                Deserialize();
            }
            List<Ped> list = PlayerPed.PedGroup.ToList(false);

            if (list.Count <= 0)
            {
                Notification.Show("Você não tem guarda-costas.");
            }
            else
            {
                List<PedData> pedDatas = _peds.ToList<PedData>();
                List<PedData> list2 = list.ConvertAll<PedData>(
                        (Ped ped) =>
                        {
                            PedData data = pedDatas.Find(
                                (PedData pedData) => pedData.Handle == ped.Handle
                            );
                            return UpdatePedData(ped, data);
                        }
                    )
                    .ToList();
                list2.ForEach(
                    (PedData data) =>
                    {
                        if (!_peds.Contains(data))
                        {
                            _peds.Add(data);
                        }
                    }
                );
                Serializer.Serialize("./scripts/Guards.dat", _peds);
                Notification.Show("~b~Guardas~s~ salvos!");
            }
        }

        private PedData UpdatePedData(Ped ped, PedData data)
        {
            PedTask task = _pedTasks.ContainsKey(ped) ? _pedTasks[ped] : (PedTask)(-1);

            // Obtendo todos os hashes de armas que o ped tem
            IEnumerable<WeaponHash> hashes =
                from hash in (WeaponHash[])Enum.GetValues(typeof(WeaponHash))
                where ped.Weapons.HasWeapon(hash)
                select hash;

            // Obtendo todos os componentes possíveis
            WeaponComponent[] componentHashes = (WeaponComponent[])
                Enum.GetValues(typeof(WeaponComponent));

            List<Weapon> weapons = hashes
                .ToList()
                .ConvertAll(hash =>
                {
                    // Obtendo a arma do pedestre
                    GTA.Weapon weapon = ped.Weapons[hash];

                    // Aqui tentamos aplicar os componentes manualmente.
                    // NÃO existe uma função nativa para verificar se o componente foi aplicado.
                    // Para simplificação, vamos adicionar todos os componentes como se estivessem presentes.

                    WeaponComponent[] hasComponents = (
                        from componentHash in componentHashes
                        where weapon.Components[componentHash].Active
                        select componentHash
                    ).ToArray();
                    return new Weapon(weapon.Ammo, weapon.Hash, hasComponents);
                })
                .ToList();

            if (data == null)
            {
                data = new PedData(
                    ped.Handle,
                    ped.Model.Hash,
                    ped.Rotation,
                    ped.Position,
                    task,
                    weapons
                );
            }
            else
            {
                data.Position = ped.Position;
                data.Rotation = ped.Rotation;
                data.Task = task;
                data.Weapons = weapons;
            }
            return data;
        }

        private static void TradeWeapons(Ped trader, Ped reviever)
        {
            if (trader.Weapons.Current != trader.Weapons[WeaponHash.Unarmed])
            {
                GTA.Weapon weapon = trader.Weapons.Current;

                if (!reviever.Weapons.HasWeapon(weapon.Hash))
                {
                    if (!reviever.IsPlayer)
                    {
                        reviever.Weapons.Drop();
                    }
                    GTA.Weapon newWeapon = reviever.Weapons.Give(weapon.Hash, 0, true, true);
                    newWeapon.Ammo = weapon.Ammo;
                    newWeapon.InfiniteAmmo = false;
                    trader.Weapons.Remove(weapon);
                }
            }
        }

        private static ZumbisModV.Weapon ConvertToZumbisModWeapon(GTA.Weapon gtaWeapon)
        {
            // Criando um array de componentes, caso necessário (pode ser ajustado conforme a estrutura da arma)
            WeaponComponent[] components = new WeaponComponent[0]; // Supondo que você ainda não esteja usando componentes, mas pode ser ajustado

            // Convertendo a arma do GTA para o formato do ZumbisModV
            ZumbisModV.Weapon zumbisWeapon = new ZumbisModV.Weapon(
                gtaWeapon.Ammo, // Munição
                gtaWeapon.Hash, // Hash da arma
                components // Componentes (ajustar conforme necessário)
            );

            return zumbisWeapon;
        }
    }
}
