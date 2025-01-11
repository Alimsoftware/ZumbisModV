using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;
using LemonUI.Menus;
using ZumbisModV.DataClasses;
using ZumbisModV.Extensions;
using ZumbisModV.Interfaces;
using ZumbisModV.Static;

namespace ZumbisModV.PlayerManagement
{
    public class PlayerInventory : Script
    {
        public const float InteractDistance = 1.5f;
        private readonly NativeMenu _mainMenu = new NativeMenu(
            "INVENTÁRIO",
            "INVENTÁRIO E RECURSOS"
        );
        private readonly List<Ped> _lootedPeds = new List<Ped>();
        private Inventory _inventory;
        private readonly Keys _inventoryKey = Keys.I;

        public static event OnUsedFoodEvent FoodUsed;

        public static event OnUsedWeaponEvent WeaponUsed;

        public static event OnUsedBuildableEvent BuildableUsed;

        public static event OnLootedEvent LootedPed;

        public PlayerInventory()
        {
            _inventoryKey = Settings.GetValue("keys", "inventory_key", _inventoryKey);
            Settings.SetValue("keys", "inventory_key", _inventoryKey);
            Settings.Save();

            // Carrega o inventário de um arquivo, ou cria um novo caso falhe ao carregar
            try
            {
                _inventory =
                    Serializer.Deserialize<Inventory>("./scripts/Inventory.dat")
                    ?? new Inventory(MenuType.Player, false);
            }
            catch (Exception ex)
            {
                // Log da exceção para depuração
                Logger.LogError($"Erro ao carregar o inventário: {ex.Message}");

                // Cria um novo inventário padrão em caso de erro
                _inventory = new Inventory(MenuType.Player, false);
            }
            //_inventory =
            //Serializer.Deserialize<Inventory>("./scripts/Inventory.dat")
            // ?? new Inventory(MenuType.Player);
            _inventory.LoadMenus();
            Instance = this;
            MenuController.MenuPool.Add(_mainMenu);
            NativeCheckboxItem cbEdit = new NativeCheckboxItem(
                "Modo Edição",
                "Permita-se pegar objetos.",
                true
            );
            cbEdit.CheckboxChanged += (
                (sender, isChecked) => //PlayerMap.Instance.EditMode = isChecked
                    Logger.LogInfo("PlayerMap.Instance.EditMode = isChecked")
            );

            NativeItem itemMenu = new NativeItem(
                "Menu Principal",
                "Navegue até o menu principal. (Para usuários de gamepad)"
            );
            itemMenu.Activated += (
                (sender, e) =>
                {
                    MenuController.MenuPool.HideAll();
                    ModController.Instance.MainMenu.Visible = true;
                }
            );
            NativeCheckboxItem cbDevMode = new NativeCheckboxItem(
                "Modo Desenvolvedor",
                "Habilite/desabilite itens e recursos infinitos.",
                _inventory.DeveloperMode
            );
            cbDevMode.CheckboxChanged += (
                (item, isChecked) =>
                {
                    if (_inventory == null)
                        return;
                    if (cbDevMode.Checked)
                    {
                        string userInput = Game.GetUserInput(WindowTitle.EnterMessage20, "", 12);
                        if (string.IsNullOrEmpty(userInput) || userInput.ToLower() != "michael")
                        {
                            cbDevMode.Checked = false;
                            Notification.Show(
                                "Dica: O primeiro nome do marido de Tamara Greenway."
                            );
                            return;
                        }
                    }
                    _inventory.DeveloperMode = cbDevMode.Checked;
                    if (!cbDevMode.Checked)
                    {
                        _inventory.Items.ForEach(i => i.Amount = 0);
                        _inventory.Resources.ForEach(i => i.Amount = 0);
                        _inventory.RefreshMenu();
                    }
                    else
                    {
                        Notification.Show("Modo Desenvolvedor: ~g~Ativado~s~");
                    }
                    Serializer.Serialize("./scripts/Inventory.dat", _inventory);
                }
            );
            _mainMenu.AddSubMenu(_inventory.InventoryMenu).Title = "Inventário";
            _mainMenu.AddSubMenu(_inventory.ResourceMenu).Title = "Recursos";
            _mainMenu.Add(cbEdit);
            _mainMenu.Add(cbDevMode);

            _inventory.ItemUsed += InventoryOnItemUsed;
            _inventory.AddedItem += (item, amount) =>
                Serializer.Serialize("./scripts/Inventory.dat", _inventory);
            Tick += OnTick;
            KeyUp += OnKeyUp;
            LootedPed += OnLootedPed;
        }

        public static PlayerInventory Instance { get; private set; }

        private static Ped PlayerPed => Database.PlayerPed;

        private static Vector3 PlayerPosition => Database.PlayerPosition;

        private void OnLootedPed(Ped ped)
        {
            if (ped.IsHuman)
                PickupLoot(ped);
            else
                AnimalLoot(ped);
        }

        private void AnimalLoot(Ped ped)
        {
            if (!PlayerPed.Weapons.HasWeapon(WeaponHash.Knife))
            {
                Notification.Show("Você precisa de uma faca!");
            }
            else
            {
                if (!_inventory.AddItem(ItemFromName("Carne Crua"), 2, ItemType.Recurso))
                    return;
                PlayerPed.Weapons.Select(WeaponHash.Knife, true);
                Notification.Show("Você estripou o animal para obter ~g~carne crua~s~.");
                PlayerPed.Task.PlayAnimation(
                    "amb@world_human_gardener_plant@male@base",
                    "base",
                    8f,
                    3000,
                    (AnimationFlags)0
                );
                _lootedPeds.Add(ped);
            }
        }

        public void PickupLoot(
            Ped ped,
            ItemType type = ItemType.Recurso,
            int amountPerItemMin = 1,
            int amountPerItemMax = 3,
            float successChance = 0.2f
        )
        {
            List<InventoryItemBase> source =
                type == ItemType.Recurso ? _inventory.Resources : _inventory.Items;
            if (source.All((r => r.Amount == r.MaxAmount)))
            {
                Notification.Show(string.Format("Seus {0}s estão cheios!", type));
            }
            else
            {
                int amount = 0;
                source.ForEach(
                    (
                        i =>
                        {
                            if (
                                i.Id == "Carne Cozida"
                                || Database.Random.NextDouble() > successChance
                            )
                                return;
                            int amount1 = Database.Random.Next(amountPerItemMin, amountPerItemMax);
                            if (i.Amount + amount1 > i.MaxAmount)
                                amount1 = i.MaxAmount - i.Amount;
                            _inventory.AddItem(i, amount1, type);
                            amount += amount1;
                        }
                    )
                );
                Notification.Show(
                    string.Format(
                        "{0}",
                        amount > 0
                            ? string.Format("{0}s: +~g~{1}", type, amount)
                            : "Nada encontrado."
                    ),
                    true
                );
                PlayerPed.Task.PlayAnimation("pickup_object", "pickup_low");
                if (ped == null)
                    return;
                _lootedPeds.Add(ped);
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs keyEventArgs)
        {
            if (MenuController.MenuPool.AreAnyVisible || keyEventArgs.KeyCode != _inventoryKey)
                return;
            _mainMenu.Visible = !_mainMenu.Visible;
            // SendMessageToPhone("Alimsoft Service", "Mod cuzinho");
        }

        public void SendMessageToPhone(string sender, string message)
        {
            Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");
            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, message);
            Function.Call(
                Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT,
                "CHAR_LIFEINVADER",
                "CHAR_LIFEINVADER",
                false,
                4,
                sender,
                ""
            );
            Logger.LogInfo("Enviou mensagem para telefone: " + message);
        }

        private void InventoryOnItemUsed(InventoryItemBase item, ItemType type)
        {
            if (item == null || type == ItemType.Recurso)
                return;
            int? eventArgument;
            if (item.GetType() == typeof(FoodInventoryItem))
            {
                FoodInventoryItem foodInventoryItem = (FoodInventoryItem)item;
                PlayerPed.Task.PlayAnimation(
                    foodInventoryItem.AnimationDict,
                    foodInventoryItem.AnimationName,
                    8f,
                    foodInventoryItem.AnimationDuration,
                    foodInventoryItem.AnimationFlags
                );
                OnUsedFoodEvent foodUsed = FoodUsed;
                if (foodUsed != null)
                    foodUsed(foodInventoryItem, foodInventoryItem.FoodType);
            }
            else if (item.GetType() == typeof(WeaponInventoryItem))
            {
                WeaponInventoryItem weapon = (WeaponInventoryItem)item;
                PlayerPed.Weapons.Give(weapon.Hash, weapon.Ammo, true, true);
                OnUsedWeaponEvent weaponUsed = WeaponUsed;
                if (weaponUsed != null)
                    weaponUsed(weapon);
            }
            else if (
                item.GetType() == typeof(BuildableInventoryItem)
                || item.GetType() == typeof(WeaponStorageInventoryItem)
            )
            {
                if (PlayerPed.IsInVehicle())
                {
                    Notification.Show("Você não pode construir enquanto estiver em um veículo!");
                    return;
                }
                BuildableInventoryItem buildableInventoryItem = (BuildableInventoryItem)item;
                ItemPreview itemPreview = new ItemPreview();
                itemPreview.StartPreview(
                    buildableInventoryItem.PropName,
                    buildableInventoryItem.GroundOffset,
                    buildableInventoryItem.IsDoor
                );
                while (!itemPreview.PreviewComplete)
                    Yield();
                Prop result = itemPreview.GetResult();
                if (result == null)
                    return;
                AddBlipToProp(buildableInventoryItem, buildableInventoryItem.Id, result);
                BuildableUsed?.Invoke(buildableInventoryItem, result);
            }
            else if (item.GetType() == typeof(UsableInventoryItem))
            {
                foreach (UsableItemEvent itemEvent in ((UsableInventoryItem)item).ItemEvents)
                {
                    switch (itemEvent.Event)
                    {
                        case ItemEvent.GiveArmor:
                            int numArmor = (itemEvent.EventArgument as int?) ?? 0;
                            PlayerPed.Armor += numArmor;
                            break;
                        case ItemEvent.GiveHealth:
                            int numHealth = (itemEvent.EventArgument as int?) ?? 0;
                            PlayerPed.Health += numHealth;
                            break;
                    }
                }
            }
            else if (item.GetType() == typeof(CraftableInventoryItem))
            {
                CraftableInventoryItem craftableInventoryItem = (CraftableInventoryItem)item;

                if (!craftableInventoryItem.Validation())
                {
                    return;
                }
            }
            _inventory.AddItem(item, -1, type);
        }

        private void OnTick(object sender, EventArgs eventArgs)
        {
            _inventory.ProcessKeys();
            GetWater();
            LootDeadPeds();
        }

        private void GetWater()
        {
            if (
                PlayerPed.IsInVehicle()
                || PlayerPed.IsSwimming
                || !PlayerPed.IsInWater
                || PlayerPed.IsPlayingAnim("pickup_object", "pickup_low")
            )
                return;
            InventoryItemBase inventoryItemBase = _inventory.Resources.Find(
                (i => i.Id == "Garrafa")
            );
            if (inventoryItemBase == null || inventoryItemBase.Amount <= 0)
                return;
            Game.DisableControlThisFrame(GTA.Control.Enter);
            if (!Game.IsControlJustPressed(GTA.Control.Enter))
                return;
            PlayerPed.Task.PlayAnimation("pickup_object", "pickup_low");
            AddItem(ItemFromName("Água Suja"), 1, ItemType.Recurso);
            AddItem(inventoryItemBase, -1, ItemType.Recurso);
            Notification.Show("Recursos: -~r~1", true);
            Notification.Show("Recursos: +~g~1", true);
        }

        private void LootDeadPeds()
        {
            if (PlayerPed.IsInVehicle())
                return;
            Ped closest = World.GetClosest(PlayerPosition, World.GetNearbyPeds(PlayerPed, 1.5f));
            if (closest == null || !closest.IsDead || _lootedPeds.Contains(closest))
                return;
            UiExtended.DisplayHelpTextThisFrame("Pressione ~INPUT_CONTEXT~ para saquear.");
            Game.DisableControlThisFrame(GTA.Control.Context);
            if (!Game.IsControlJustPressed(GTA.Control.Context))
                return;
            OnLootedEvent lootedPed = LootedPed;
            if (lootedPed == null)
                return;
            lootedPed(closest);
        }

        private void Controller() { }

        public bool AddItem(InventoryItemBase item, int amount, ItemType type)
        {
            return item != null && _inventory.AddItem(item, amount, type);
        }

        public bool PickupItem(InventoryItemBase item, ItemType type)
        {
            return item != null && _inventory.AddItem(item, 1, type);
        }

        public InventoryItemBase ItemFromName(string id)
        {
            return _inventory?.Items == null || _inventory?.Resources == null
                ? null
                : Array.Find(
                    _inventory.Items.Concat(_inventory.Resources).ToArray(),
                    (i => i.Id == id)
                );
        }

        private static void AddBlipToProp(IProp item, string name, Entity entity)
        {
            if (item.BlipSprite == BlipSprite.Standard)
                return;
            Blip blip = entity.AddBlip();
            blip.Sprite = item.BlipSprite;
            blip.Color = item.BlipColor;
            blip.Name = name;
        }

        public bool HasItem(InventoryItemBase item, ItemType itemType)
        {
            if (item == null)
                return false;
            switch (itemType)
            {
                case ItemType.Recurso:
                    return _inventory.Resources.Contains(item) && item.Amount > 0;
                case ItemType.Item:
                    return _inventory.Items.Contains(item) && item.Amount > 0;
                default:
                    return false;
            }
        }

        public delegate void OnUsedFoodEvent(FoodInventoryItem item, FoodType foodType);

        public delegate void OnUsedWeaponEvent(WeaponInventoryItem weapon);

        public delegate void OnUsedBuildableEvent(BuildableInventoryItem item, Prop newProp);

        public delegate void OnLootedEvent(Ped ped);
    }
}
