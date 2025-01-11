using System;
using System.Diagnostics;
using System.Linq;
using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;
using ZumbisModV;
using ZumbisModV.Extensions;
using ZumbisModV.PlayerManagement;
using ZumbisModV.Scripts;
using ZumbisModV.Static;
using ZumbisModV.Utils;

namespace ZumbisModV.PlayerManagement
{
    // Token: 0x02000081 RID: 129
    public class PlayerMap : Script
    {
        /* Token: 0x1400000D RID: 13
        // (add) Token: 0x060002D0 RID: 720 RVA: 0x0000BD0C File Offset: 0x00009F0C
        // (remove) Token: 0x060002D1 RID: 721 RVA: 0x0000BD40 File Offset: 0x00009F40
        //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static event InteractedEvent Interacted;

        // Token: 0x060002D2 RID: 722 RVA: 0x0000BD74 File Offset: 0x00009F74
        public PlayerMap()
        {
            PlayerMap.Instance = this;
            base.Tick += this.OnTick;
            base.Aborted += this.OnAborted;
            PlayerInventory.BuildableUsed += this.InventoryOnBuildableUsed;
        }

        // Token: 0x17000097 RID: 151
        // (get) Token: 0x060002D3 RID: 723 RVA: 0x00003847 File Offset: 0x00001A47
        // (set) Token: 0x060002D4 RID: 724 RVA: 0x0000384E File Offset: 0x00001A4E
        public static PlayerMap Instance { get; private set; }

        // Token: 0x17000098 RID: 152
        // (get) Token: 0x060002D5 RID: 725 RVA: 0x00003856 File Offset: 0x00001A56
        // (set) Token: 0x060002D6 RID: 726 RVA: 0x0000385E File Offset: 0x00001A5E
        public bool EditMode { get; set; } = true;

        // Token: 0x17000099 RID: 153
        // (get) Token: 0x060002D7 RID: 727 RVA: 0x0000247E File Offset: 0x0000067E
        public Vector3 PlayerPosition
        {
            get { return Database.PlayerPosition; }
        }

        // Token: 0x060002D8 RID: 728 RVA: 0x0000BDD0 File Offset: 0x00009FD0
        public void Deserialize()
        {
            bool flag = this._map != null;
            if (!flag)
            {
                Map des = Serializer.Deserialize<Map>("./scripts/Map.dat");
                bool flag2 = des == null;
                if (flag2)
                {
                    des = new Map();
                }
                this._map = des;
                this._map.ListChanged += delegate(int count)
                {
                    Serializer.Serialize<Map>("./scripts/Map.dat", this._map);
                };
                this.LoadProps();
            }
        }

        // Token: 0x060002D9 RID: 729 RVA: 0x0000BE2C File Offset: 0x0000A02C
        private void LoadProps()
        {
            bool flag = this._map.Count <= 0;
            if (!flag)
            {
                foreach (MapProp mapProp in this._map)
                {
                    Model model = mapProp.PropName;
                    bool flag2 = !model.Request(1000);
                    if (flag2)
                    {
                        Notification.Show(
                            string.Format(
                                "Tentei solicitar ~y~{0}~s~ mas falhou.",
                                mapProp.PropName
                            )
                        );
                    }
                    else
                    {
                        Vector3 position = mapProp.Position;
                        Prop prop = World.CreateProp(
                            model.Hash, // Hash do modelo
                            position, // Posição do objeto
                            mapProp.Rotation, // Rotação do objeto
                            dynamic: true, // Objeto dinâmico
                            placeOnGround: false // Não alinhar ao chão automaticamente
                        );
                        if (prop != null)
                        {
                            prop.IsPositionFrozen = !mapProp.IsDoor;
                            prop.Rotation = mapProp.Rotation;
                        }
                        mapProp.Handle = prop.Handle;
                        bool flag3 = mapProp.BlipSprite == BlipSprite.Standard;
                        if (!flag3)
                        {
                            Blip blip = prop.AddBlip();
                            blip.Sprite = mapProp.BlipSprite;
                            blip.Color = mapProp.BlipColor;
                            blip.Name = mapProp.Id;
                            ZombieVehicleSpawner.Instance.SpawnBlocker.Add(mapProp.Position);
                        }
                    }
                }
            }
        }

        // Token: 0x060002DA RID: 730 RVA: 0x0000BFE0 File Offset: 0x0000A1E0
        private void InventoryOnBuildableUsed(BuildableInventoryItem item, Prop newProp)
        {
            bool flag = this._map == null;
            if (flag)
            {
                this.Deserialize();
            }
            WeaponStorageInventoryItem storageItem = item as WeaponStorageInventoryItem;
            MapProp mapProp = new MapProp(
                item.Id,
                item.PropName,
                item.BlipSprite,
                item.BlipColor,
                item.GroundOffset,
                item.Interactable,
                item.IsDoor,
                item.CanBePickedUp,
                newProp.Rotation,
                newProp.Position,
                newProp.Handle,
                (storageItem != null) ? storageItem.WeaponsList : null
            );
            this._map.Add(mapProp);
            ZombieVehicleSpawner.Instance.SpawnBlocker.Add(mapProp.Position);
        }

        // Token: 0x060002DB RID: 731 RVA: 0x00003867 File Offset: 0x00001A67
        private void OnAborted(object sender, EventArgs eventArgs)
        {
            this._map.Clear();
        }

        // Token: 0x060002DC RID: 732 RVA: 0x0000C080 File Offset: 0x0000A280
        private void OnTick(object sender, EventArgs eventArgs)
        {
            bool flag = this._map == null;
            if (!flag)
            {
                bool flag2 = !this._map.Any<MapProp>();
                if (!flag2)
                {
                    bool flag3 = MenuController.MenuPool.AreAnyVisible;
                    if (!flag3)
                    {
                        MapProp closest = World.GetClosest<MapProp>(
                            this.PlayerPosition,
                            this._map.ToArray<MapProp>()
                        );
                        bool flag4 = closest == null;
                        if (!flag4)
                        {
                            bool flag5 = !closest.CanBePickedUp;
                            if (!flag5)
                            {
                                float dist = closest.Position.VDist(this.PlayerPosition);
                                bool flag6 = dist > 3f;
                                if (!flag6)
                                {
                                    this.TryUseMapProp(closest);
                                }
                            }
                        }
                    }
                }
            }
        }

        // Token: 0x060002DD RID: 733 RVA: 0x0000C124 File Offset: 0x0000A324
        private void TryUseMapProp(MapProp mapProp)
        {
            bool flag = mapProp.CanBePickedUp && this.EditMode;
            bool flag2 = !flag && !mapProp.Interactable;
            if (!flag2)
            {
                bool flag3 = flag;
                if (flag3)
                {
                    Game.DisableControlThisFrame(Control.Context);
                }
                bool interactable = mapProp.Interactable;
                if (interactable)
                {
                    PlayerMap.DisableAttackActions();
                }
                GameExtended.DisableWeaponWheel();
                UiExtended.DisplayHelpTextThisFrame(
                    string.Format(
                        "{0}",
                        flag
                            ? string.Format(
                                "Pressione ~INPUT_CONTEXT~ para pegar o {0}.\n",
                                mapProp.Id
                            )
                            : ((!this.EditMode) ? "Você não está no modo de edição.\n" : "")
                    )
                        + string.Format(
                            "{0}",
                            mapProp.Interactable
                                ? string.Format(
                                    "Pressione ~INPUT_ATTACK~ para {0}.",
                                    mapProp.IsDoor ? "Bloquear/Desbloquear" : "interact"
                                )
                                : ""
                        ),
                    false
                );
                bool flag4 =
                    GTAUtils.IsDisabledControlJustPressed(Control.Attack) && mapProp.Interactable;
                if (flag4)
                {
                    PlayerMap.InteractedEvent interacted = PlayerMap.Interacted;
                    if (interacted != null)
                    {
                        interacted(mapProp, PlayerInventory.Instance.ItemFromName(mapProp.Id));
                    }
                }
                bool flag5 =
                    !GTAUtils.IsDisabledControlJustPressed(Control.Attack)
                    || !mapProp.CanBePickedUp;
                if (!flag5)
                {
                    bool flag6 = !PlayerInventory.Instance.PickupItem(
                        PlayerInventory.Instance.ItemFromName(mapProp.Id),
                        ItemType.Item
                    );
                    if (!flag6)
                    {
                        mapProp.Delete();
                        this._map.Remove(mapProp);
                        ZombieVehicleSpawner.Instance.SpawnBlocker.Remove(mapProp.Position);
                    }
                }
            }
        }

        // Token: 0x060002DE RID: 734 RVA: 0x0000C2A0 File Offset: 0x0000A4A0
        public bool Find(Prop prop)
        {
            return this._map != null && this._map.Contains(prop);
        }

        // Token: 0x060002DF RID: 735 RVA: 0x00003876 File Offset: 0x00001A76
        private static void DisableAttackActions()
        {
            Game.DisableControlThisFrame(Control.Attack2);
            Game.DisableControlThisFrame(Control.Attack);
            Game.DisableControlThisFrame(Control.Aim);
        }

        // Token: 0x060002E0 RID: 736 RVA: 0x00003897 File Offset: 0x00001A97
        public void NotifyListChanged()
        {
            this._map.NotifyListChanged();
        }

        // Token: 0x040001E4 RID: 484
        public const float InteractDistance = 3f;

        // Token: 0x040001E5 RID: 485
        private Map _map;

        // Token: 0x02000082 RID: 130
        // (Invoke) Token: 0x060002E3 RID: 739
        public delegate void InteractedEvent(MapProp mapProp, InventoryItemBase inventoryItem);
        */
    }
}
