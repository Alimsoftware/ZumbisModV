using System;
using System.Collections.Generic;
using System.Linq;
using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;
using LemonUI.Menus;
using ZumbisModV.Extensions;
using ZumbisModV.Interfaces;
using ZumbisModV.PlayerManagement;
using ZumbisModV.Static;
using ZumbisModV.Wrappers;

namespace ZumbisModV.Scripts
{
    public class MapInteraction : Script
    {
        private const int AmmoPerPart = 10;
        private readonly float _enemyRangeForSleeping = 50f;
        private readonly int _sleepHours = 8;
        private readonly NativeMenu _weaponsMenu = new NativeMenu(
            "Caixa de Armas",
            "SELECIONE UMA OPÇÃO"
        );
        private readonly NativeMenu _storageMenu;
        private readonly NativeMenu _myWeaponsMenu;
        private readonly NativeMenu _craftWeaponsMenu = new NativeMenu(
            "Bancada de Trabalho",
            "SELECIONE UMA OPÇÃO"
        );
        private readonly Dictionary<WeaponGroup, int> _requiredAmountDictionary;

        public MapInteraction()
        {
            PlayerMap.Interacted += MapOnInteracted;
            MenuController.MenuPool.Add(_weaponsMenu);
            MenuController.MenuPool.Add(_craftWeaponsMenu);
            //_storageMenu = MenuController.MenuPool.Add(_weaponsMenu);
            //_myWeaponsMenu = MenuController.MenuPool.Add(_weaponsMenu);
            _enemyRangeForSleeping = Settings.GetValue(
                "map_interaction",
                "enemy_range_for_sleeping",
                _enemyRangeForSleeping
            );
            _sleepHours = Settings.GetValue("map_interaction", "sleep_hours", _sleepHours);
            Settings.SetValue(
                "map_interaction",
                "enemy_range_for_sleeping",
                _enemyRangeForSleeping
            );
            Settings.SetValue("map_interaction", "sleep_hours", _sleepHours);
            _requiredAmountDictionary = new Dictionary<WeaponGroup, int>
            {
                { WeaponGroup.Sniper, 2 },
                { WeaponGroup.Heavy, 5 },
                { WeaponGroup.MG, 3 },
                { WeaponGroup.PetrolCan, 1 },
            };
            Aborted += OnAborted;
        }

        private static Ped PlayerPed => Database.PlayerPed;

        private static Player Player => Database.Player;

        private static void OnAborted(object sender, EventArgs eventArgs)
        {
            PlayerPed.IsVisible = true;
            PlayerPed.IsPositionFrozen = false;
            Player.CanControlCharacter = true;
            if (!PlayerPed.IsDead)
            {
                Screen.FadeIn(0);
            }
        }

        private void MapOnInteracted(MapProp mapProp, InventoryItemBase inventoryItem)
        {
            BuildableInventoryItem item = inventoryItem as BuildableInventoryItem;
            bool flag = item == null;
            if (!flag)
            {
                string id = item.Id;
                if (!(id == "Barraca"))
                {
                    if (!(id == "Caixa de Armas"))
                    {
                        if (id == "Bancada de Trabalho")
                        {
                            CraftAmmo();
                        }
                    }
                    else
                    {
                        UseWeaponsCrate(mapProp);
                    }
                }
                else
                {
                    Sleep(mapProp.Position);
                }

                if (item.IsDoor)
                {
                    Prop prop = Entity.FromHandle(mapProp.Handle) as Prop;
                    if (prop != null)
                    {
                        prop.SetStateOfDoor(!prop.GetDoorLockState(), DoorState.Closed);
                    }
                    else
                    {
                        Notification.Show("Prop inválido ou não encontrado.");
                    }
                }
            }
        }

        private void CraftAmmo()
        {
            _craftWeaponsMenu.Clear();
            // Obtém todos os valores da enum WeaponGroup
            WeaponGroup[] allWeaponGroups = (WeaponGroup[])Enum.GetValues(typeof(WeaponGroup));
            // Filtra os valores indesejados
            var noAmmoGroups = allWeaponGroups
                .Where(w =>
                    w != WeaponGroup.PetrolCan
                    && w != WeaponGroup.Unarmed
                    && w != WeaponGroup.Melee
                    && w != WeaponGroup.Parachute
                    && w != WeaponGroup.DigiScanner
                    && w != WeaponGroup.NightVision
                    && w != WeaponGroup.FireExtinguisher
                    && w != (WeaponGroup)unchecked((uint)-942583726)
                )
                .ToList();

            // Adiciona o novo valor
            noAmmoGroups.Add(WeaponGroup.AssaultRifle);

            // Converte de volta para um array, se necessário
            WeaponGroup[] finalGroups = noAmmoGroups.ToArray();

            for (int i = 0; i < finalGroups.Length; i++)
            {
                var weaponGroup = finalGroups[i];
                var weaponGroupName =
                    weaponGroup == WeaponGroup.AssaultRifle
                        ? "Rifle de Assalto"
                        : weaponGroup.ToString();

                NativeItem menuItem = new NativeItem(
                    weaponGroupName,
                    string.Format("Criar munição para {0}", weaponGroup)
                );
                var ammoBadge = new BadgeSet("commonmenu", "shop_ammo_icon_a", "shop_ammo_icon_b");
                menuItem.LeftBadgeSet = ammoBadge;

                int required = GetRequiredPartsForWeaponGroup(weaponGroup);
                menuItem.Description = string.Format(
                    "Peças de armas necessárias: ~y~{0}~s~",
                    required
                );

                _craftWeaponsMenu.Add(menuItem);

                menuItem.Activated += (sender, selectedItem) =>
                {
                    var inventoryItemBase = PlayerInventory.Instance.ItemFromName("Weapon Parts");
                    if (inventoryItemBase == null)
                    {
                        Notification.Show("Não há peças de armas suficientes.");
                        return;
                    }
                    if (inventoryItemBase.Amount >= required)
                    {
                        GTA.Weapon weapon = PlayerPed.Weapons[
                            Array.Find(
                                (WeaponHash[])Enum.GetValues(typeof(WeaponHash)),
                                weaponHash =>
                                    PlayerPed.Weapons.HasWeapon(weaponHash)
                                    && PlayerPed.Weapons[weaponHash].Group == weaponGroup
                            )
                        ];

                        if (weapon == null)
                            return;

                        int ammoToAdd = 10 * required;

                        if (weapon.Ammo + ammoToAdd > weapon.MaxAmmo)
                        {
                            weapon.Ammo = weapon.MaxAmmo;
                        }
                        else
                        {
                            weapon.Ammo += ammoToAdd;
                            PlayerInventory.Instance.AddItem(
                                inventoryItemBase,
                                -required,
                                ItemType.Recurso
                            );
                        }
                    }
                };
            }
            _craftWeaponsMenu.Visible = !_craftWeaponsMenu.Visible;
        }

        private int GetRequiredPartsForWeaponGroup(WeaponGroup group)
        {
            return _requiredAmountDictionary.ContainsKey(group)
                ? _requiredAmountDictionary[group]
                : 1;
        }

        private void UseWeaponsCrate(MapProp prop)
        {
            if (prop?.Weapons == null)
                return; // Retorna se o prop ou a lista de armas for nulo
            _weaponsMenu.SelectedIndexChanged += (newMenu, forward) =>
            {
                if (newMenu == _storageMenu)
                {
                    TradeOffWeapons(prop, prop.Weapons, _storageMenu, true);
                }
                else if (newMenu == _myWeaponsMenu)
                {
                    var playerWeapons = new List<Weapon>();
                    WeaponHash[] weaponHashes = (WeaponHash[])Enum.GetValues(typeof(WeaponHash));
                    WeaponComponent[] weaponComponents = (WeaponComponent[])
                        Enum.GetValues(typeof(WeaponComponent));
                    foreach (var hash in weaponHashes)
                    {
                        if (hash == WeaponHash.Unarmed)
                            continue;
                        if (PlayerPed.Weapons.HasWeapon(hash))
                        {
                            var playerWeapon = PlayerPed.Weapons[hash];
                            // Lista de componentes ativos
                            var activeComponents = new List<WeaponComponent>();
                            foreach (var component in weaponComponents)
                            {
                                // Verifica se o componente está ativo para a arma
                                bool isComponentActive = Function.Call<bool>(
                                    Hash.HAS_PED_GOT_WEAPON_COMPONENT,
                                    PlayerPed.Handle,
                                    (uint)hash,
                                    (uint)component.ComponentHash
                                );

                                if (isComponentActive)
                                {
                                    activeComponents.Add(component);
                                }
                            }

                            // Cria a arma com seus componentes ativos
                            var weapon = new Weapon(
                                playerWeapon.Ammo,
                                hash,
                                activeComponents.ToArray()
                            );
                            playerWeapons.Add(weapon);
                        }
                    }

                    MapInteraction.TradeOffWeapons(prop, playerWeapons, _myWeaponsMenu, false);
                }
            };
            _weaponsMenu.Visible = !_weaponsMenu.Visible;
        }

        private void Sleep(Vector3 position)
        {
            Ped[] array = World
                .GetNearbyPeds(position, _enemyRangeForSleeping)
                .Where(new Func<Ped, bool>(IsEnemy))
                .ToArray();

            if (!array.Any())
            {
                TimeSpan currentDayTime =
                    World.CurrentTimeOfDay + new TimeSpan(0, _sleepHours, 0, 0);
                PlayerPed.IsVisible = false;
                Player.CanControlCharacter = false;
                PlayerPed.IsPositionFrozen = true;
                Screen.FadeOut(2000);
                Wait(2000);
                World.CurrentTimeOfDay = currentDayTime;
                PlayerPed.IsVisible = true;
                Player.CanControlCharacter = true;
                PlayerPed.IsPositionFrozen = false;
                PlayerPed.ClearBloodDamage();
                Weather[] array2 = (Weather[])Enum.GetValues(typeof(Weather));
                array2 = (
                    from w in array2
                    where
                        w != Weather.Blizzard
                        && w != Weather.Christmas
                        && w != Weather.Snowing
                        && w != Weather.Snowlight
                        && w != Weather.Unknown
                    select w
                ).ToArray();
                Weather weather = array2[Database.Random.Next(array2.Length)];
                World.Weather = weather;
                Wait(2000);
                Screen.FadeIn(2000);
            }
            else
            {
                Notification.Show("Existem ~r~inimigos~s~ por perto.");
                Notification.Show("Marcando-os em seu mapa.");
                Array.ForEach(array, new Action<Ped>(AddBlip));
            }
        }

        private static void AddBlip(Ped ped)
        {
            if (!ped.AttachedBlip.Exists())
            {
                Blip blip = ped.AddBlip();
                blip.Name = "Inimigo";
                EntityEventWrapper entWrapper = new EntityEventWrapper(ped);
                entWrapper.Died += (sender, args) =>
                {
                    Blip currentBlip = args.Entity.AttachedBlip;
                    if (currentBlip != null)
                    {
                        currentBlip.Delete();
                    }
                    (sender as EntityEventWrapper)?.Dispose();
                };
                entWrapper.Aborted += (sender, args) =>
                {
                    Blip currentBlip = args.Entity.AttachedBlip;
                    if (currentBlip != null)
                    {
                        currentBlip.Delete();
                    }
                };
            }
        }

        private static bool IsEnemy(Ped ped)
        {
            return (
                    ped.IsHuman
                    && !ped.IsDead
                    && ped.GetRelationshipWithPed(PlayerPed) == Relationship.Hate
                ) || ped.IsInCombatAgainst(PlayerPed);
        }

        private static void TradeOffWeapons(
            MapProp item,
            List<Weapon> weapons,
            NativeMenu currentMenu,
            bool giveToPlayer
        )
        {
            // Cria a opção "Voltar"
            NativeItem back = new NativeItem("Back");
            back.Activated += (sender, selectedItem) =>
            {
                currentMenu.Back();
            };

            // Limpa o menu atual e adiciona a opção "Voltar"
            currentMenu.Clear();
            currentMenu.Add(back);

            // Ação de notificar que a lista de itens foi alterada
            Action notify = () =>
            {
                PlayerMap.Instance.NotifyListChanged();
            };

            // Adiciona as armas ao menu
            foreach (var weapon in weapons)
            {
                NativeItem menuItem = new NativeItem(string.Format("{0}", weapon.Hash));
                currentMenu.Add(menuItem);

                menuItem.Activated += (sender, selectedItem) =>
                {
                    //var index = currentMenu.SelectedIndex;
                    // currentMenu.Remove(menuItem);

                    // Ação ao ativar o menu de arma
                    if (giveToPlayer)
                    {
                        // Dá a arma para o jogador
                        PlayerPed.Weapons.Give(weapon.Hash, 0, true, true);
                        PlayerPed.Weapons[weapon.Hash].Ammo = weapon.Ammo;

                        // Define os componentes da arma, se houver
                        foreach (var component in weapon.Components)
                        {
                            //PlayerPed.Weapons[weapon.Hash].SetComponent(component, true);
                            Function.Call(
                                Hash.GIVE_WEAPON_COMPONENT_TO_PED,
                                PlayerPed.Handle,
                                weapon.Hash,
                                (uint)component.ComponentHash
                            );
                        }

                        // Remove a arma da lista do item e notifica
                        item.Weapons.Remove(weapon);
                        notify();
                    }
                    else
                    {
                        // Remove a arma do jogador e adiciona ao item
                        PlayerPed.Weapons.Remove(weapon.Hash);
                        item.Weapons.Add(weapon);
                        notify();
                    }
                    currentMenu.Remove(menuItem);
                };
            }
        }
    }
}
