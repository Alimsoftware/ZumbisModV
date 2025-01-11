using System;
using System.Collections.Generic;
using System.Linq;
using GTA;
using GTA.Math;
using GTA.UI;
using LemonUI.Menus;
using ZumbisModV.Extensions;
using ZumbisModV.Static;
using ZumbisModV.Wrappers;

namespace ZumbisModV.Scripts
{
    /* Token: 0x0200006B RID: 107
    public class MapInteraction : Script
    {
        // Token: 0x06000265 RID: 613 RVA: 0x00009978 File Offset: 0x00007B78

        // Token: 0x04000198 RID: 408
        private const int AmmoPerPart = 10;

        // Token: 0x04000199 RID: 409
        private readonly float _enemyRangeForSleeping = 50f;

        // Token: 0x0400019A RID: 410
        private readonly int _sleepHours = 8;

        // Token: 0x0400019B RID: 411
        private readonly NativeMenu _weaponsMenu = new NativeMenu("Caixa de Armas", "SELECIONE UMA OPÇÃO");

        // Token: 0x0400019C RID: 412
        private readonly NativeMenu _storageMenu;

        // Token: 0x0400019D RID: 413
        private readonly NativeMenu _myWeaponsMenu;

        // Token: 0x0400019E RID: 414
        private readonly NativeMenu _craftWeaponsMenu = new NativeMenu("Bancada de Trabalho", "SELECIONE UMA OPÇÃO");

        // Token: 0x0400019F RID: 415
        private readonly Dictionary<WeaponGroup, int> _requiredAmountDictionary;
        public MapInteraction()
        {
            PlayerMap.Interacted += this.MapOnInteracted;
            MenuController.MenuPool.Add(_weaponsMenu);
            MenuController.MenuPool.Add(_craftWeaponsMenu);
           _storageMenu = MenuController.MenuPool.Add(_weaponsMenu);
           _myWeaponsMenu = MenuController.MenuPool.Add(_weaponsMenu);
            this._enemyRangeForSleeping = base.Settings.GetValue<float>("map_interaction", "enemy_range_for_sleeping", this._enemyRangeForSleeping);
            this._sleepHours = base.Settings.GetValue<int>("map_interaction", "sleep_hours", this._sleepHours);
            base.Settings.SetValue<float>("map_interaction", "enemy_range_for_sleeping", this._enemyRangeForSleeping);
            base.Settings.SetValue<int>("map_interaction", "sleep_hours", this._sleepHours);
            _requiredAmountDictionary = new Dictionary<WeaponGroup, int>
            {
                {
                    WeaponGroup.Sniper,
                    2
                },
                {
                    WeaponGroup.Heavy,
                    5
                },
                {
                    WeaponGroup.MG,
                    3
                },
                {
                    WeaponGroup.PetrolCan,
                    1
                }
            };
            Aborted += OnAborted;
        }

        // Token: 0x1700008C RID: 140
        // (get) Token: 0x06000266 RID: 614 RVA: 0x00002485 File Offset: 0x00000685
        private static Ped PlayerPed
        {
            get
            {
                return Database.PlayerPed;
            }
        }

        private static Player Player
        {
            get
            {
                return Database.Player;
            }
        }

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
                    this.Sleep(mapProp.Position);
                }
                bool flag2 = !item.IsDoor;
                if (!flag2)
                {
                    Prop prop = new Prop(mapProp.Handle);
                    prop.SetStateOfDoor(!prop.GetDoorLockState(), DoorState.Closed);
                }
            }
        }

        // Token: 0x0600026A RID: 618 RVA: 0x00009BF0 File Offset: 0x00007DF0
        private void CraftAmmo()
        {
            this._craftWeaponsMenu.Clear();
            WeaponGroup[] array = (WeaponGroup[])Enum.GetValues(typeof(WeaponGroup));
            array = (from w in array
                     where w != 1595662460 && w != -1609580060 && w != -728555052 && w != -942583726
                     select w).ToArray<WeaponGroup>();
            List<WeaponGroup> list = array.ToList<WeaponGroup>();
            list.Add(970310034);
            array = list.ToArray();
            WeaponGroup[] array2 = array;
            for (int i = 0; i < array2.Length; i++)
            {
                MapInteraction.<> c__DisplayClass15_1 CS$<> 8__locals1 = new MapInteraction.<> c__DisplayClass15_1();
                CS$<> 8__locals1.weaponGroup = array2[i];
                UIMenuItem uimenuItem = new UIMenuItem(string.Format("{0}", (CS$<> 8__locals1.weaponGroup == 970310034) ? "Rifle de Assalto" : CS$<> 8__locals1.weaponGroup.ToString()), string.Format("Criar munição para {0}", CS$<> 8__locals1.weaponGroup));
            uimenuItem.SetLeftBadge(UIMenuItem.BadgeStyle.Ammo);
            int required = this.GetRequiredPartsForWeaponGroup(CS$<> 8__locals1.weaponGroup);
            uimenuItem.Description = string.Format("Peças de armas necessárias: ~y~{0}~s~", required);
            this._craftWeaponsMenu.AddItem(uimenuItem);
            uimenuItem.Activated += delegate (UIMenu sender, UIMenuItem selectedItem)
            {
                InventoryItemBase inventoryItemBase = PlayerInventory.Instance.ItemFromName("Weapon Parts");
                bool flag = inventoryItemBase == null;
                if (!flag)
                {
                    bool flag2 = inventoryItemBase.Amount >= required;
                    if (flag2)
                    {
                        WeaponHash[] array3 = (WeaponHash[])Enum.GetValues(typeof(WeaponHash));
                        WeaponHash[] array4 = array3;
                        Predicate<WeaponHash> match;
                        if ((match = CS$<> 8__locals1.<> 9__2) == null)
                            {
                            match = (CS$<> 8__locals1.<> 9__2 = ((WeaponHash h) => MapInteraction.PlayerPed.Weapons.HasWeapon(h) && MapInteraction.PlayerPed.Weapons[h].Group == CS$<> 8__locals1.weaponGroup));
                        }
                        WeaponHash weaponHash = Array.Find<WeaponHash>(array4, match);
                        Weapon weapon = MapInteraction.PlayerPed.Weapons[weaponHash];
                        bool flag3 = weapon == null;
                        if (!flag3)
                        {
                            int num = 10 * required;
                            bool flag4 = weapon.Ammo + num > weapon.MaxAmmo;
                            if (!flag4)
                            {
                                MapInteraction.PlayerPed.Weapons.Select(weapon);
                                bool flag5 = weapon.Ammo + num > weapon.MaxAmmo;
                                if (flag5)
                                {
                                    weapon.Ammo = weapon.MaxAmmo;
                                }
                                else
                                {
                                    weapon.Ammo += num;
                                }
                                PlayerInventory.Instance.AddItem(inventoryItemBase, -required, ItemType.Resource);
                            }
                        }
                    }
                    else
                    {
                        UI.Notify("Não há peças de armas suficientes.");
                    }
                }
            };
        }
            _craftWeaponsMenu.Visible = !_craftWeaponsMenu.Visible;
        }

        // Token: 0x0600026B RID: 619 RVA: 0x00009D78 File Offset: 0x00007F78
        private int GetRequiredPartsForWeaponGroup(WeaponGroup group)
        {
            return this._requiredAmountDictionary.ContainsKey(group) ? this._requiredAmountDictionary[group] : 1;
        }

        // Token: 0x0600026C RID: 620 RVA: 0x00009DA8 File Offset: 0x00007FA8
        private void UseWeaponsCrate(MapProp prop)
        {
            MapProp prop2 = prop;
            bool flag = ((prop2 != null) ? prop2.Weapons : null) == null;
            if (!flag)
            {
                _weaponsMenu.OnMenuChange += delegate (NativeMenu oldMenu, NativeMenu newMenu, bool forward)
                {
                    bool flag2 = newMenu == this._storageMenu;
                    if (flag2)
                    {
                        MapInteraction.TradeOffWeapons(prop, prop.Weapons, this._storageMenu, true);
                    }
                    else
                    {
                        bool flag3 = newMenu == this._myWeaponsMenu;
                        if (flag3)
                        {
                            List<Weapon> playerWeapons = new List<Weapon>();
                            WeaponHash[] weaponHashes = (WeaponHash[])Enum.GetValues(typeof(WeaponHash));
                            WeaponComponent[] weaponComponents = (WeaponComponent[])Enum.GetValues(typeof(WeaponComponent));
                            weaponHashes.ToList<WeaponHash>().ForEach(delegate (WeaponHash hash)
                            {
                                bool flag4 = hash == -1569615261;
                                if (!flag4)
                                {
                                    bool flag5 = !MapInteraction.PlayerPed.Weapons.HasWeapon(hash);
                                    if (!flag5)
                                    {
                                        Weapon playerWeapon = MapInteraction.PlayerPed.Weapons[hash];
                                        WeaponComponent[] hasComponents = (from c in weaponComponents
                                                                           where MapInteraction.PlayerPed.Weapons[hash].IsComponentActive(c)
                                                                           select c).ToArray<WeaponComponent>();
                                        Weapon weapon = new Weapon(playerWeapon.Ammo, hash, hasComponents);
                                        playerWeapons.Add(weapon);
                                    }
                                }
                            });
                            MapInteraction.TradeOffWeapons(prop, playerWeapons, this._myWeaponsMenu, false);
                        }
                    }
                };
                this._weaponsMenu.Visible = !this._weaponsMenu.Visible;
            }
        }

        // Token: 0x0600026D RID: 621 RVA: 0x00009E18 File Offset: 0x00008018
        private static void TradeOffWeapons(MapProp item, List<Weapon> weapons, NativeMenu currentMenu, bool giveToPlayer)
        {
            NativeItem back = new NativeItem("Back");
            back.Activated += (NativeMenu sender, NativeItem selectedItem)
            {
                sender.GoBack();
            };
            currentMenu.Clear();
            currentMenu.AddItem(back);
            Action notify = delegate ()
            {
                PlayerMap.Instance.NotifyListChanged();
            };
            weapons.ForEach(delegate (Weapon weapon)
            {
                NativeItem menuItem = new NativeItem(string.Format("{0}", weapon.Hash));
                currentMenu.AddItem(menuItem);
                Action<WeaponComponent> <> 9__4;
                menuItem.Activated += (NativeMenu sender, NativeItem selectedItem) =>
                {
                    currentMenu.RemoveItemAt(currentMenu.CurrentSelection);
                    currentMenu.RefreshIndex();
                    bool giveToPlayer2 = giveToPlayer;
                    if (giveToPlayer2)
                    {
                        MapInteraction.PlayerPed.Weapons.Give(weapon.Hash, 0, true, true);
                        MapInteraction.PlayerPed.Weapons[weapon.Hash].Ammo = weapon.Ammo;
                        List<WeaponComponent> list = weapon.Components.ToList<WeaponComponent>();
                        Action<WeaponComponent> action;
                        if ((action = <> 9__4) == null)
                        {
                            action = (<> 9__4 = delegate (WeaponComponent component)
                            {
                                MapInteraction.PlayerPed.Weapons[weapon.Hash].SetComponent(component, true);
                            });
                        }
                        list.ForEach(action);
                        item.Weapons.Remove(weapon);
                        notify();
                    }
                    else
                    {
                        MapInteraction.PlayerPed.Weapons.Remove(weapon.Hash);
                        item.Weapons.Add(weapon);
                        notify();
                    }
                };
            });
            currentMenu.RefreshIndex();
        }

        // Token: 0x0600026E RID: 622 RVA: 0x00009ED0 File Offset: 0x000080D0
        private void Sleep(Vector3 position)
        {
            Ped[] array = World.GetNearbyPeds(position, this._enemyRangeForSleeping).Where(new Func<Ped, bool>(MapInteraction.IsEnemy)).ToArray<Ped>();
            
            if (!array.Any())
            {
                TimeSpan currentDayTime = World.CurrentTimeOfDay + new TimeSpan(0, _sleepHours, 0, 0);
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
                array2 = (from w in array2
                          where w != Weather.Blizzard && w != Weather.Christmas && w != Weather.Snowing && w != Weather.Snowlight && w != Weather.Unknown
                          select w).ToArray();
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
            bool flag = ped.AttachedBlip.Exists();
            if (!flag)
            {
                Blip blip = ped.AddBlip();
                blip.Name = "Inimigo";
                EntityEventWrapper entWrapper = new EntityEventWrapper(ped);
                entWrapper.Died += delegate (EntityEventWrapper sender, Entity entity)
                {
                    Blip currentBlip = entity.AttachedBlip;
                    if (currentBlip != null)
                    {
                        currentBlip.Delete();
                    }
                    sender.Dispose();
                };
                entWrapper.Aborted += delegate (EntityEventWrapper sender, Entity entity)
                {
                    Blip currentBlip = entity.AttachedBlip;
                    if (currentBlip != null)
                    {
                        currentBlip.Delete();
                    }
                };
            }
        }

        // Token: 0x06000270 RID: 624 RVA: 0x0000A0C0 File Offset: 0x000082C0
        private static bool IsEnemy(Ped ped)
        {
            return (ped.IsHuman && !ped.IsDead && ped.GetRelationshipWithPed(PlayerPed) == Relationship.Hate) || ped.IsInCombatAgainst(MapInteraction.PlayerPed);
        }

    }*/
}
