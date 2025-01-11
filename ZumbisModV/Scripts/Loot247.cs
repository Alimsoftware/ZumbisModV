using System;
using System.Collections.Generic;
using System.Linq;
using GTA;
using GTA.Math;
using GTA.UI;
using ZumbisModV.Extensions;
using ZumbisModV.Interfaces;
using ZumbisModV.PlayerManagement;
using ZumbisModV.Static;
using ZumbisModV.Utils;

namespace ZumbisModV.Scripts
{
    public class Loot247 : Script, ISpawner
    {
        public const float InteractDistance = 1.5f;
        private readonly List<Blip> _blips = new List<Blip>();
        private readonly List<Prop> _lootedShelfes = new List<Prop>();
        private readonly int[] _propHashes;

        public Loot247()
        {
            Instance = this;
            _propHashes = new int[]
            {
                Game.GenerateHash("v_ret_247shelves01"),
                Game.GenerateHash("v_ret_247shelves02"),
                Game.GenerateHash("v_ret_247shelves03"),
                Game.GenerateHash("v_ret_247shelves04"),
                Game.GenerateHash("v_ret_247shelves05"),
            };
            Tick += OnTick;
            Aborted += (sender, args) => Clear();
        }

        public static Loot247 Instance { get; private set; }

        public bool Spawn { get; set; }

        private static Vector3 PlayerPosition => Database.PlayerPosition;

        private static Ped PlayerPed => Database.PlayerPed;

        private void OnTick(object sender, EventArgs e)
        {
            SpawnBlips();
            ClearBlips();
            LootShops();
        }

        private void LootShops()
        {
            if (Spawn)
            {
                if (!PlayerPed.IsPlayingAnim("oddjobs@shop_robbery@rob_till", "loop"))
                {
                    IEnumerable<Prop> source = World
                        .GetNearbyProps(PlayerPosition, 15f)
                        .Where(new Func<Prop, bool>(IsShelf));
                    Prop closest = World.GetClosest(PlayerPosition, source.ToArray());
                    if (closest != null)
                    {
                        if (closest.Position.VDist(PlayerPosition) < 1.5f)
                        {
                            Game.DisableControlThisFrame(Control.Context);
                            UiExtended.DisplayHelpTextThisFrame(
                                "Pressione ~INPUT_CONTEXT~ para saquear a prateleira.",
                                false
                            );
                            if (GTAUtils.IsDisabledControlJustPressed(Control.Context))
                            {
                                _lootedShelfes.Add(closest);
                                bool isRandomLoot = Database.Random.NextDouble() > 0.3;
                                string itemName = isRandomLoot
                                    ? "Alimentos Embalados"
                                    : "Água Limpa";
                                PlayerInventory.Instance.PickupItem(
                                    item: PlayerInventory.Instance.ItemFromName(itemName),
                                    type: ItemType.Item
                                );

                                PlayerPed.Task.PlayAnimation(
                                    "oddjobs@shop_robbery@rob_till",
                                    "loop"
                                );

                                PlayerPed.Heading = (closest.Position - PlayerPosition).ToHeading();
                                Notification.Show($"Você encontrou ~g~{itemName}~s~.");
                            }
                        }
                    }
                }
            }
        }

        private bool IsShelf(Prop prop)
        {
            return _propHashes.Contains(prop.Model.Hash) && !_lootedShelfes.Contains(prop);
        }

        private void ClearBlips()
        {
            if (!Spawn)
            {
                Clear();
            }
        }

        private void SpawnBlips()
        {
            if (_blips.Count >= Database.Shops247Locations.Length)
                return;

            foreach (Vector3 shop247Location in Database.Shops247Locations)
            {
                Blip blip = World.CreateBlip(shop247Location);
                blip.Sprite = BlipSprite.Store;
                blip.Name = "Loja";
                blip.IsShortRange = true;
                _blips.Add(blip);
            }
        }

        public void Clear()
        {
            foreach (var blip in _blips)
            {
                blip.Delete();
            }
            _blips.Clear();
        }
    }
}
