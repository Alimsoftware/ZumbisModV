using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;
using LemonUI.Menus;
using LemonUI.Scaleform;
using ZumbisModV.Interfaces;
using ZumbisModV.Static;
using ZumbisModV.Utils;

namespace ZumbisModV
{
    [Serializable]
    public class Inventory
    {
        public const float InteractDist = 2.5f;
        public List<InventoryItemBase> Items = new List<InventoryItemBase>();
        public List<InventoryItemBase> Resources = new List<InventoryItemBase>();

        private static Vector3 PlayerPosition => Database.PlayerPosition;
        private static Ped PlayerPed => Database.PlayerPed;
        public bool DeveloperMode { get; set; }

        [NonSerialized]
        public readonly MenuType MenuType;

        [NonSerialized]
        public NativeMenu InventoryMenu;

        [NonSerialized]
        public NativeMenu ResourceMenu;

        [field: NonSerialized]
        public event CraftedItemEvent TryCraft;

        [field: NonSerialized]
        public event ItemUsedEvent ItemUsed;

        [field: NonSerialized]
        public event AddedItemEvent AddedItem;

        public Inventory(MenuType menuType, bool ignoreContainers = false)
        {
            MenuType = menuType;
            InventoryItemBase resource_Alcohol = new InventoryItemBase(
                amount: 0,
                maxAmount: 20,
                id: "Álcool",
                description: "Um líquido inflamável volátil incolor."
            );
            InventoryItemBase resource_Battery = new InventoryItemBase(
                amount: 0,
                maxAmount: 25,
                id: "Bateria",
                description: "Um recurso que pode fornecer uma carga elétrica."
            );
            InventoryItemBase resource_Binding = new InventoryItemBase(
                amount: 0,
                maxAmount: 25,
                id: "Adesivo",
                description: "Um adesivo forte."
            );
            InventoryItemBase resource_Bottle = new InventoryItemBase(
                amount: 0,
                maxAmount: 10,
                id: "Garrafa",
                description: "Um recipiente usado para armazenar bebidas ou outros líquidos."
            );
            InventoryItemBase resource_Cloth = new InventoryItemBase(
                amount: 0,
                maxAmount: 25,
                id: "Pano",
                description: "Tecido trançado ou feltrado."
            );
            InventoryItemBase resource_DirtyWater = new InventoryItemBase(
                amount: 0,
                maxAmount: 25,
                id: "Água Suja",
                description: "Líquido obtido de uma fonte de água não potável."
            );
            InventoryItemBase resource_Metal = new InventoryItemBase(
                amount: 0,
                maxAmount: 25,
                id: "Metal",
                description: "É um maldito metal."
            );
            InventoryItemBase resource_Wood = new InventoryItemBase(
                amount: 0,
                maxAmount: 25,
                id: "Madeira",
                description: "É uma maldita madeira."
            );
            InventoryItemBase resource_Plastic = new InventoryItemBase(
                amount: 0,
                maxAmount: 25,
                id: "Plástico",
                description: "Um material sintético feito de uma ampla variedade de polímeros orgânicos."
            );
            InventoryItemBase resource_RawMetal = new InventoryItemBase(
                amount: 0,
                maxAmount: 15,
                id: "Carne Crua",
                description: "Pode ser cozido para criar ~g~Carne Cozida~s~."
            );
            InventoryItemBase resource_Matches = new InventoryItemBase(
                amount: 0,
                maxAmount: 10,
                id: "Matches",
                description: "Pode ser usado para criar fogo."
            );
            InventoryItemBase resource_WeaponParts = new InventoryItemBase(
                amount: 25,
                maxAmount: 25,
                id: "Peças de Armas",
                description: "Usado para criar componentes de armas, e armas. (Fabricação de armas em breve)"
            );
            InventoryItemBase resource_VehicleParts = new InventoryItemBase(
                amount: 0,
                maxAmount: 25,
                id: "Peças de Veículos",
                description: "Usado para reparar veículos."
            );
            UsableInventoryItem usableInventoryItem_Bandage = new UsableInventoryItem(
                amount: 0,
                maxAmount: 10,
                id: "Bandagem",
                description: "Uma tira de material usada para curar uma ferida ou proteger uma parte ferida do corpo.",
                itemEvents: new UsableItemEvent[2]
                {
                    new UsableItemEvent(ItemEvent.GiveHealth, 25),
                    new UsableItemEvent(ItemEvent.GiveArmor, 15),
                }
            )
            {
                RequiredComponents = new CraftableItemComponent[3]
                {
                    new CraftableItemComponent(resource_Binding, 1),
                    new CraftableItemComponent(resource_Alcohol, 2),
                    new CraftableItemComponent(resource_Cloth, 2),
                },
            };
            CraftableInventoryItem craftableInventoryItem_Supressor = new CraftableInventoryItem(
                amount: 0,
                maxAmount: 5,
                id: "Supressor",
                description: "Pode ser usado para suprimir um rifle, pistola, espingarda ou SMG.",
                validation: (
                    () =>
                    {
                        WeaponComponentHash[] suppressorComponents = new WeaponComponentHash[]
                        {
                            WeaponComponentHash.AtPiSupp02, // Supressor para pistolas
                            WeaponComponentHash.AtPiSupp, // Supressor para pistolas
                            WeaponComponentHash.AtArSupp, // Supressor para rifles
                            WeaponComponentHash.AtArSupp02, // Supressor para rifles
                            WeaponComponentHash.AtSrSupp, // Supressor para snipers
                            WeaponComponentHash.AtSrSupp03, // Supressor para snipers

                            // Supressor para SMGs
                            // Supressor para LMGs
                        };

                        GTA.Weapon weapon = PlayerPed.Weapons.Current;

                        if (weapon == null || weapon.Hash == WeaponHash.Unarmed)
                        {
                            Notification.Show("Nenhuma arma selecionada!");
                            return false;
                        }

                        // Itera sobre os componentes
                        foreach (WeaponComponentHash componentHash in suppressorComponents)
                        {
                            WeaponComponent component = weapon.Components[componentHash];

                            // Verifica se o componente é compatível com a arma
                            bool isCompatible = Function.Call<bool>(
                                Hash.DOES_WEAPON_TAKE_WEAPON_COMPONENT,
                                weapon.Hash,
                                componentHash
                            );

                            // Se o componente for compatível e ainda não estiver ativado
                            if (isCompatible && !component.Active)
                            {
                                // Ativa o componente
                                component.Active = true;
                                Notification.Show($"Supressor equipado com sucesso!");
                                return true;
                            }
                        }

                        Notification.Show("Você não pode equipar isso agora.");
                        return false;
                    }
                )
            )
            {
                RequiredComponents = new CraftableItemComponent[2]
                {
                    new CraftableItemComponent(resource_WeaponParts, 2),
                    new CraftableItemComponent(resource_Binding, 1),
                },
            };

            BuildableInventoryItem buildableInventoryItem_SandBlock = new BuildableInventoryItem(
                amount: 0,
                maxAmount: 5,
                id: "Bloco de Areia",
                description: "Usado para fornecer cobertura em situações de combate",
                propName: "prop_mb_sandblock_02",
                blipSprite: BlipSprite.Standard,
                blipColor: BlipColor.White,
                groundOffset: Vector3.Zero,
                interactable: false,
                isDoor: false,
                canBePickedUp: true
            )
            {
                RequiredComponents = new CraftableItemComponent[4]
                {
                    new CraftableItemComponent(resource_Metal, 3),
                    new CraftableItemComponent(resource_Binding, 2),
                    new CraftableItemComponent(resource_Cloth, 1),
                    new CraftableItemComponent(resource_Wood, 2),
                },
            };
            BuildableInventoryItem buildableInventoryItem_WorkBench = new BuildableInventoryItem(
                0,
                maxAmount: 2,
                id: "Bancada de Trabalho",
                description: "Pode ser usado para fabricar munição.",
                propName: "prop_tool_bench02",
                blipSprite: BlipSprite.AmmuNation,
                blipColor: BlipColor.Yellow,
                groundOffset: Vector3.Zero,
                interactable: true,
                isDoor: false,
                canBePickedUp: true
            )
            {
                RequiredComponents = new CraftableItemComponent[4]
                {
                    new CraftableItemComponent(resource_Metal, 15),
                    new CraftableItemComponent(resource_Wood, 5),
                    new CraftableItemComponent(resource_Plastic, 5),
                    new CraftableItemComponent(resource_Binding, 5),
                },
            };
            BuildableInventoryItem buildableInventoryItem_Gate = new BuildableInventoryItem(
                amount: 0,
                maxAmount: 3,
                id: "Gate",
                description: "Um portão de metal que pode ser aberto por veículos ou pedestres.",
                propName: "prop_gate_prison_01",
                blipSprite: BlipSprite.Standard,
                blipColor: BlipColor.White,
                groundOffset: Vector3.Zero,
                interactable: true,
                isDoor: true,
                canBePickedUp: true
            )
            {
                RequiredComponents = new CraftableItemComponent[3]
                {
                    new CraftableItemComponent(resource_Metal, 5),
                    new CraftableItemComponent(resource_Binding, 3),
                    new CraftableItemComponent(resource_Battery, 1),
                },
            };
            WeaponInventoryItem weaponInventoryItem_Molotov = new WeaponInventoryItem(
                amount: 0,
                maxAmount: 25,
                id: "Molotov",
                description: "Uma arma incendiária improvisada baseada em garrafa.",
                ammo: 1,
                weaponHash: WeaponHash.Molotov,
                weaponComponents: null
            )
            {
                RequiredComponents = new CraftableItemComponent[3]
                {
                    new CraftableItemComponent(resource_Alcohol, 1),
                    new CraftableItemComponent(resource_Cloth, 1),
                    new CraftableItemComponent(resource_Bottle, 1),
                },
            };
            WeaponInventoryItem weaponInventoryItem_Knife = new WeaponInventoryItem(
                amount: 0,
                maxAmount: 1,
                id: "Knife",
                description: "Uma faca improvisada.",
                ammo: 1,
                weaponHash: WeaponHash.Dagger,
                weaponComponents: null
            )
            {
                RequiredComponents = new CraftableItemComponent[2]
                {
                    new CraftableItemComponent(resource_Metal, 3),
                    new CraftableItemComponent(resource_Binding, 1),
                },
            };
            WeaponInventoryItem weaponInventoryItem_Flashlight = new WeaponInventoryItem(
                amount: 0,
                maxAmount: 5,
                id: "Flashlight",
                description: "Uma luz portátil alimentada por bateria.",
                ammo: 1,
                weaponHash: WeaponHash.Flashlight,
                weaponComponents: null
            )
            {
                RequiredComponents = new CraftableItemComponent[3]
                {
                    new CraftableItemComponent(resource_Battery, 4),
                    new CraftableItemComponent(resource_Plastic, 4),
                    new CraftableItemComponent(resource_Binding, 4),
                },
            };
            FoodInventoryItem foodInventoryItem_CookedMeat = new FoodInventoryItem(
                amount: 0,
                maxAmount: 15,
                id: "Carne Cozida",
                description: "Pode ser criado cozinhando carne crua.",
                animationDict: "mp_player_inteat@burger",
                animationName: "mp_player_int_eat_burger",
                animationFlags: (AnimationFlags)16,
                animationDuration: -1,
                foodType: FoodType.Food,
                restorationAmount: 0.25f
            )
            {
                RequiredComponents = new CraftableItemComponent[2]
                {
                    new CraftableItemComponent(resource_RawMetal, 1),
                    new CraftableItemComponent(resource_Alcohol, 2),
                },
                NearbyResource = NearbyResource.CampFire,
            };
            FoodInventoryItem foodInventoryItem_PackagedFood = new FoodInventoryItem(
                amount: 0,
                maxAmount: 15,
                id: "Alimentos Embalados",
                description: "Geralmente obtido em lojas ao redor de Los Santos.",
                animationDict: "mp_player_inteat@pnq",
                animationName: "loop",
                animationFlags: AnimationFlags.UpperBodyOnly,
                animationDuration: -1,
                foodType: FoodType.SpecialFood,
                restorationAmount: 0.3f
            );
            FoodInventoryItem foodInventoryItem_CleanWater = new FoodInventoryItem(
                amount: 0,
                maxAmount: 15,
                id: "Água Limpa",
                description: "Pode ser feito com água suja ou obtido em lojas de Los Santos.",
                animationDict: "mp_player_intdrink",
                animationName: "loop_bottle",
                animationFlags: AnimationFlags.UpperBodyOnly,
                animationDuration: -1,
                foodType: FoodType.Water,
                restorationAmount: 0.35f
            )
            {
                RequiredComponents = new CraftableItemComponent[1]
                {
                    new CraftableItemComponent(resource_DirtyWater, 1),
                },
                NearbyResource = NearbyResource.CampFire,
            };
            BuildableInventoryItem buildableInventoryItem_Tent = new BuildableInventoryItem(
                amount: 1,
                maxAmount: 5,
                id: "Barraca",
                description: "Abrigo portátil feito de tecido, sustentado por uma ou mais estacas e esticado por cordas ou laços presos a estacas cravadas no solo.",
                propName: "prop_skid_tent_01",
                blipSprite: BlipSprite.CaptureHouse,
                blipColor: BlipColor.White,
                groundOffset: Vector3.Zero,
                interactable: true,
                isDoor: false,
                canBePickedUp: true
            )
            {
                RequiredComponents = new CraftableItemComponent[3]
                {
                    new CraftableItemComponent(resource_Wood, 3),
                    new CraftableItemComponent(resource_Cloth, 4),
                    new CraftableItemComponent(resource_Binding, 3),
                },
            };
            BuildableInventoryItem buildableInventoryItem_CampFire = new BuildableInventoryItem(
                amount: 1,
                maxAmount: 5,
                id: "Fogueira de Acampamento",
                description: "Uma fogueira ao ar livre num acampamento, usada para cozinhar e como ponto focal para atividades sociais.",
                propName: "prop_beach_fire",
                blipSprite: BlipSprite.Standard,
                blipColor: BlipColor.White,
                groundOffset: Vector3.Zero,
                interactable: false,
                isDoor: false,
                canBePickedUp: true
            )
            {
                RequiredComponents = new CraftableItemComponent[3]
                {
                    new CraftableItemComponent(resource_Wood, 3),
                    new CraftableItemComponent(resource_Alcohol, 1),
                    new CraftableItemComponent(resource_Matches, 1),
                },
            };
            BuildableInventoryItem buildableInventoryItem_Wall = new BuildableInventoryItem(
                amount: 0,
                maxAmount: 15,
                id: "Parede",
                description: "Uma parede de madeira que pode ser utilizada para criar abrigos.",
                propName: "prop_fncconstruc_01d",
                blipSprite: BlipSprite.Standard,
                blipColor: BlipColor.White,
                groundOffset: Vector3.Zero,
                interactable: false,
                isDoor: false,
                canBePickedUp: true
            )
            {
                RequiredComponents = new CraftableItemComponent[2]
                {
                    new CraftableItemComponent(resource_Wood, 4),
                    new CraftableItemComponent(resource_Binding, 3),
                },
            };
            BuildableInventoryItem buildableInventoryItem_Barrier = new BuildableInventoryItem(
                amount: 0,
                maxAmount: 10,
                id: "Barreira",
                description: "Uma barreira de madeira que pode ser usada para bloquear lacunas na sua zona segura.",
                propName: "prop_fncwood_16b",
                blipSprite: BlipSprite.Standard,
                blipColor: BlipColor.White,
                groundOffset: Vector3.Zero,
                interactable: false,
                isDoor: false,
                canBePickedUp: true
            )
            {
                RequiredComponents = new CraftableItemComponent[2]
                {
                    new CraftableItemComponent(resource_Wood, 5),
                    new CraftableItemComponent(resource_Binding, 2),
                },
            };
            BuildableInventoryItem buildableInventoryItem_Door = new BuildableInventoryItem(
                amount: 0,
                maxAmount: 5,
                id: "Porta",
                description: "Uma barreira articulada, deslizante ou giratória na entrada de um edifício, sala ou veículo, ou na estrutura de um armário.",
                propName: "ex_p_mp_door_office_door01",
                blipSprite: BlipSprite.Standard,
                blipColor: BlipColor.White,
                groundOffset: Vector3.Zero,
                interactable: true,
                isDoor: true,
                canBePickedUp: true
            )
            {
                RequiredComponents = new CraftableItemComponent[3]
                {
                    new CraftableItemComponent(resource_Wood, 3),
                    new CraftableItemComponent(resource_Binding, 1),
                    new CraftableItemComponent(resource_Metal, 1),
                },
            };
            CraftableInventoryItem craftableInventoryItem_VehicleRepairKit =
                new CraftableInventoryItem(
                    amount: 0,
                    maxAmount: 10,
                    id: "Vehicle Repair Kit",
                    description: "Usado para reparar motores de veículos.",
                    validation: (
                        () =>
                        {
                            Notification.Show("Você só pode usar isso ao reparar um veículo.");
                            return false;
                        }
                    )
                )
                {
                    RequiredComponents = new CraftableItemComponent[3]
                    {
                        new CraftableItemComponent(resource_VehicleParts, 5),
                        new CraftableItemComponent(resource_Metal, 5),
                        new CraftableItemComponent(resource_Binding, 2),
                    },
                };
            Items.AddRange(
                new InventoryItemBase[17]
                {
                    usableInventoryItem_Bandage,
                    weaponInventoryItem_Molotov,
                    weaponInventoryItem_Knife,
                    weaponInventoryItem_Flashlight,
                    foodInventoryItem_CookedMeat,
                    foodInventoryItem_PackagedFood,
                    foodInventoryItem_CleanWater,
                    buildableInventoryItem_Tent,
                    buildableInventoryItem_CampFire,
                    buildableInventoryItem_Wall,
                    buildableInventoryItem_Barrier,
                    buildableInventoryItem_Door,
                    buildableInventoryItem_Gate,
                    buildableInventoryItem_SandBlock,
                    buildableInventoryItem_WorkBench,
                    craftableInventoryItem_Supressor,
                    craftableInventoryItem_VehicleRepairKit,
                }
            );
            Resources.AddRange(
                new InventoryItemBase[13]
                {
                    resource_Binding,
                    resource_Alcohol,
                    resource_Cloth,
                    resource_Bottle,
                    resource_Metal,
                    resource_Wood,
                    resource_Battery,
                    resource_Plastic,
                    resource_RawMetal,
                    resource_DirtyWater,
                    resource_Matches,
                    resource_WeaponParts,
                    resource_VehicleParts,
                }
            );
            Items.Sort((c1, c2) => string.Compare(c1.Id, c2.Id, StringComparison.Ordinal));
            Resources.Sort((c1, c2) => string.Compare(c1.Id, c2.Id, StringComparison.Ordinal));
            LoadMenus();
            if (ignoreContainers)
                return;
            WeaponStorageInventoryItem storageInventoryItem = new WeaponStorageInventoryItem(
                amount: 0,
                maxAmount: 1,
                id: "Caixa de Armas",
                description: "Uma caixa usada especificamente para armazenar armas.",
                propName: "hei_prop_carrier_crate_01a",
                blipSprite: BlipSprite.AssaultRifle,
                blipColor: BlipColor.White,
                groundOffset: Vector3.Zero,
                interactable: true,
                isDoor: false,
                canBePickedUp: true,
                weaponsList: new List<Weapon>()
            );
            storageInventoryItem.RequiredComponents = new CraftableItemComponent[4]
            {
                new CraftableItemComponent(resource_Metal, 15),
                new CraftableItemComponent(resource_Wood, 15),
                new CraftableItemComponent(resource_Plastic, 15),
                new CraftableItemComponent(resource_Binding, 10),
            };
            Items.Add(storageInventoryItem);
        }

        public void LoadMenus()
        {
            InventoryMenu = new NativeMenu("Inventário", "SELECIONE UMA OPÇÃO", "Seus itens");
            ResourceMenu = new NativeMenu(
                "Recursos",
                "SELECIONE UMA OPÇÃO",
                "Recursos disponíveis"
            );
            AddItemsToMenu(InventoryMenu, Items, ItemType.Item);
            AddItemsToMenu(ResourceMenu, Resources, ItemType.Recurso);
            MenuController.MenuPool.Add(InventoryMenu);
            MenuController.MenuPool.Add(ResourceMenu);
            RefreshMenu();
            if (MenuType != 0)
                return;
            var createButton = new InstructionalButton("Projeto", Control.Enter);
            var projectButton = new InstructionalButton("Criar", Control.LookBehind);
            InventoryMenu.Buttons.Add(createButton);
            InventoryMenu.Buttons.Add(projectButton);
        }

        private void OnProjectButtonPressed(object sender, EventArgs e)
        { // Exibir a mensagem quando o botão "Projeto" for pressionado
            Notification.Show("Você pressionou o botão Projeto!");
        }

        public void RefreshMenu()
        {
            UpdateMenuSpecific(InventoryMenu, Items, MenuType == MenuType.Player);
            UpdateMenuSpecific(ResourceMenu, Resources, false);
        }

        public void ProcessKeys()
        {
            var flag = MenuType > MenuType.Player;
            if (flag)
            {
                //Notification.Show(MenuType.ToString());
                if (InventoryMenu.Visible)
                {
                    // Notification.Show("InventoryMenu.Visible");
                    Game.DisableControlThisFrame(Control.Enter);
                    Game.DisableControlThisFrame(Control.VehicleExit);
                    Game.DisableControlThisFrame(Control.LookBehind);
                    if (GTAUtils.IsDisabledControlJustPressed(Control.Enter)) //Tecla F
                    {
                        ICraftable selectedInventoryItem = GetSelectedInventoryItem<ICraftable>();
                        if (selectedInventoryItem == null)
                            return;
                        StringBuilder strProj = new StringBuilder("Projeto:\n");
                        if (
                            selectedInventoryItem.RequiredComponents == null
                            || selectedInventoryItem.RequiredComponents.Length == 0
                                && !DeveloperMode
                        )
                        {
                            // Caso não haja componentes requeridos ou DeveloperMode seja falso, não executa a lógica abaixo.
                            return;
                        }
                        Array.ForEach(
                            selectedInventoryItem.RequiredComponents,
                            (
                                i =>
                                    strProj.Append(
                                        string.Format(
                                            "{0}{1}~s~ / {2} {3}\n",
                                            i.Resource.Amount >= i.RequiredAmount ? "~g~" : "~r~",
                                            i.Resource.Amount,
                                            i.RequiredAmount,
                                            i.Resource.Id
                                        )
                                    )
                            )
                        );
                        Notification.Show(strProj.ToString());
                    }
                    else if (GTAUtils.IsDisabledControlJustPressed(Control.LookBehind)) //Tecla C
                    {
                        InventoryItemBase selectedInventoryItem =
                            GetSelectedInventoryItem<InventoryItemBase>();
                        ICraftable craftable = selectedInventoryItem as ICraftable;
                        if (selectedInventoryItem == null)
                            throw new NullReferenceException("item");
                        if (craftable != null)
                        {
                            Craft(selectedInventoryItem, craftable);
                        }
                    }
                }
            }
        }

        public bool AddItem(InventoryItemBase item, int amount, ItemType type)
        {
            if (!DeveloperMode)
            {
                if (item.Amount + amount < 0)
                    return false;
                if (item.Amount + amount > item.MaxAmount)
                {
                    Notification.Show(
                        string.Format("Não há espaço suficiente para mais ~g~{0}s~s~.", item.Id),
                        true
                    );
                    return false;
                }
            }
            item.Amount += amount;
            switch (type)
            {
                case ItemType.Recurso:
                    UpdateMenuSpecific(ResourceMenu, Resources, false);
                    break;
                case ItemType.Item:
                    UpdateMenuSpecific(InventoryMenu, Items, MenuType == MenuType.Player);
                    break;
            }
            RefreshMenu();
            AddedItemEvent addedItem = AddedItem;
            if (addedItem != null)
                addedItem(item, item.Amount);
            return true;
        }

        private void Craft(InventoryItemBase item, ICraftable craftable)
        {
            // Verifica pré-condições
            if (MenuType != 0 || item == null || craftable == null)
            {
                Notification.Show("Parâmetros inválidos ou ação não permitida.");
                return;
            }

            // Verifica se o item pode ser criado fora do modo desenvolvedor
            if (!DeveloperMode && (!CanCraftItem(craftable) || item.Amount >= item.MaxAmount))
            {
                Notification.Show("Você não pode criar este item agora.");
                return;
            }

            // Verifica a proximidade de recursos necessários, como uma fogueira
            if (!DeveloperMode && item is FoodInventoryItem foodItem)
            {
                if (foodItem.NearbyResource == NearbyResource.CampFire)
                {
                    Prop[] nearbyProps = World.GetNearbyProps(PlayerPosition, 2.5f, "c");
                    if (nearbyProps == null || nearbyProps.Length == 0)
                    {
                        Notification.Show("Não há ~g~Fogueira de Acampamento~s~ por perto.");
                        return;
                    }
                }
            }

            // Adiciona o item ao inventário
            AddItem(item, 1, ItemType.Item);

            // Consome os componentes necessários
            if (craftable.RequiredComponents != null)
            {
                foreach (var component in craftable.RequiredComponents)
                {
                    AddItem(component.Resource, -component.RequiredAmount, ItemType.Recurso);
                }
            }

            // Dispara o evento de criação, se houver
            CraftedItemEvent tryCraft = TryCraft;
            tryCraft?.Invoke(item);
            Notification.Show("Criando");
        }

        private void AddItemsToMenu(NativeMenu menu, List<InventoryItemBase> items, ItemType type)
        {
            items.ForEach(
                (
                    i =>
                    {
                        i.CreateMenuItem();
                        menu.Add(i.MenuItem);
                        i.MenuItem.Activated += (
                            (sender, item) =>
                            {
                                if (
                                    i is WeaponInventoryItem weaponInventoryItem_Knife
                                    && weaponInventoryItem_Knife.Amount > 0
                                    && PlayerPed.Weapons.HasWeapon(weaponInventoryItem_Knife.Hash)
                                )
                                {
                                    Notification.Show("Você já tem essa arma!");
                                }
                                else
                                {
                                    if (i.Amount <= 0)
                                        return;
                                    ItemUsedEvent itemUsed = ItemUsed;
                                    if (itemUsed == null)
                                        return;
                                    itemUsed(i, type);
                                }
                            }
                        );
                    }
                )
            );
        }

        private void UpdateMenuSpecific(
            NativeMenu menu,
            List<InventoryItemBase> collection,
            bool leftBadges
        )
        {
            menu.Items.ForEach(
                (
                    menuItem =>
                    {
                        InventoryItemBase itemFromMenuItem = GetItemFromMenuItem<InventoryItemBase>(
                            collection,
                            menuItem
                        );
                        if (itemFromMenuItem == null)
                            return;
                        //if (((CanCraftItem(itemFromMenuItem as ICraftable) ? 1 : (DeveloperMode ? 1 : 0)) & (leftBadges ? 1 : 0)) != 0)
                        bool flag =
                            (CanCraftItem(itemFromMenuItem as ICraftable) || DeveloperMode)
                            && leftBadges;
                        if (flag)
                        {
                            var icTick = new BadgeSet(
                                "commonmenu",
                                "shop_tick_icon",
                                "shop_tick_icon"
                            );
                            menuItem.LeftBadgeSet = icTick;
                            menuItem.Selected += (sender, e) =>
                            {
                                menuItem.LeftBadge.Color = Color.FromArgb(255, 0, 0, 0);
                            };
                        }
                        else if (leftBadges)
                        {
                            var icLock = new BadgeSet("commonmenu", "shop_lock", "shop_lock");
                            menuItem.LeftBadgeSet = icLock;
                            menuItem.Selected += (sender, e) =>
                            {
                                menuItem.LeftBadge.Color = Color.FromArgb(255, 0, 0, 0);
                            };
                        }
                        menuItem.AltTitle = string.Format(
                            "{0}/{1}",
                            itemFromMenuItem.Amount.ToString(),
                            itemFromMenuItem.MaxAmount
                        );
                    }
                )
            );
        }

        private bool CanCraftItem(ICraftable craftable)
        {
            if (craftable?.RequiredComponents == null)
                return false;
            foreach (CraftableItemComponent requiredComponent in craftable.RequiredComponents)
            {
                InventoryItemBase resource = requiredComponent.Resource;
                if (Resources.Contains(resource))
                {
                    int? amount = Resources.Find((i => resource == i))?.Amount;
                    int requiredAmount = requiredComponent.RequiredAmount;
                    if (amount.GetValueOrDefault() < requiredAmount && amount.HasValue)
                        return false;
                }
            }
            return true;
        }

        private T GetSelectedInventoryItem<T>()
            where T : class
        {
            return GetItemFromMenuItem<T>(Items, InventoryMenu.Items[InventoryMenu.SelectedIndex]);
        }

        private static T GetItemFromMenuItem<T>(
            List<InventoryItemBase> collection,
            NativeItem menuItem
        )
            where T : class
        {
            return collection.Find(i => i.MenuItem == menuItem) as T;
        }

        public delegate void CraftedItemEvent(InventoryItemBase item);

        public delegate void ItemUsedEvent(InventoryItemBase item, ItemType type);

        public delegate void AddedItemEvent(InventoryItemBase item, int newAmount);
    }
}
