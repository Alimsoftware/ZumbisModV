using System;
using System.Linq;
using GTA;
using GTA.Math;
using GTA.UI;
using ZumbisModV.Extensions;
using ZumbisModV.Scripts;
using ZumbisModV.Static;
using ZumbisModV.Utils;

namespace ZumbisModV.PlayerManagement
{
    public class PlayerMap : Script
    {
        public static event InteractedEvent Interacted;
        public const float InteractDistance = 3f;
        private Map _map;
        public delegate void InteractedEvent(MapProp mapProp, InventoryItemBase inventoryItem);

        public static PlayerMap Instance { get; private set; }

        public bool EditMode { get; set; } = true;

        public PlayerMap()
        {
            Instance = this;
            Tick += OnTick;
            Aborted += OnAborted;
            PlayerInventory.BuildableUsed += InventoryOnBuildableUsed;
        }

        public Vector3 PlayerPosition
        {
            get { return Database.PlayerPosition; }
        }

        public void Deserialize()
        {
            if (_map == null)
            {
                _map = Serializer.Deserialize<Map>("./scripts/Map.dat") ?? new Map(); // Operador ?? (null-coalescing)

                _map.ListChanged += (int count) =>
                {
                    Serializer.Serialize("./scripts/Map.dat", _map);
                };
                LoadProps();
            }
        }

        private void LoadProps()
        {
            if (_map.Any())
            {
                foreach (MapProp mapProp in _map)
                {
                    Model model = mapProp.PropName;

                    if (!model.Request(1000))
                    {
                        Notification.Show(
                            string.Format(
                                "Tentei solicitar ~y~{mapProp.PropName}~s~  mas falhou.",
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
                            mapProp.Handle = prop.Handle;

                            if (mapProp.BlipSprite == BlipSprite.Standard)
                            {
                                Blip blip = prop.AddBlip();
                                blip.Sprite = mapProp.BlipSprite;
                                blip.Color = mapProp.BlipColor;
                                blip.Name = mapProp.Id;
                                ZombieVehicleSpawner.Instance.SpawnBlocker.Add(mapProp.Position);
                            }
                        } //Hash.CREATE_OBJECT_NO_OFFSET
                    }
                }
            }
        }

        private void InventoryOnBuildableUsed(BuildableInventoryItem item, Prop newProp)
        {
            // Verifica se o mapa foi inicializado. Se não, desserializa.
            if (_map is null)
            {
                Deserialize();
            }

            MapProp mapProp; // Declara a variável aqui para usar fora do escopo do 'if'

            // Verifica se o item é do tipo WeaponStorageInventoryItem e realiza o cast se for.
            if (item is WeaponStorageInventoryItem storageItem)
            {
                // Cria um novo MapProp com a lista de armas do item de armazenamento.
                mapProp = new MapProp(
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
                    storageItem.WeaponsList // Usa a lista de armas do item de armazenamento.
                );
            }
            else
            {
                // Cria um novo MapProp sem lista de armas (para outros tipos de itens).
                mapProp = new MapProp(
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
                    null // Sem lista de armas.
                );
            }

            _map.Add(mapProp); // Adiciona o novo prop ao mapa.
            ZombieVehicleSpawner.Instance.SpawnBlocker.Add(mapProp.Position); // Adiciona a posição ao bloqueador de spawn.
        }

        private void OnAborted(object sender, EventArgs eventArgs)
        {
            _map.Clear();
        }

        private void OnTick(object sender, EventArgs eventArgs)
        {
            if (_map != null && _map.Any())
            {
                if (!MenuController.MenuPool.AreAnyVisible)
                {
                    MapProp closest = World.GetClosest(PlayerPosition, _map.ToArray());

                    if (closest != null && closest.CanBePickedUp)
                    {
                        float dist = closest.Position.VDist(PlayerPosition);

                        if (dist <= 3f)
                        {
                            TryUseMapProp(closest);
                        }
                    }
                }
            }
        }

        private void TryUseMapProp(MapProp mapProp)
        {
            bool canPickUpAndEditMode = mapProp.CanBePickedUp && EditMode;
            bool notPickUpAndNotInteractable = !canPickUpAndEditMode && !mapProp.Interactable;
            if (!notPickUpAndNotInteractable)
            {
                bool canPickUp = canPickUpAndEditMode;
                if (canPickUp)
                {
                    Game.DisableControlThisFrame(Control.Context);
                }

                bool interactable = mapProp.Interactable;
                if (interactable)
                {
                    DisableAttackActions();
                }

                GameExtended.DisableWeaponWheel();

                string helpText = "";
                if (canPickUp)
                {
                    helpText += string.Format(
                        "Pressione ~INPUT_CONTEXT~ para pegar o {0}.\n",
                        mapProp.Id
                    );
                }
                else if (!EditMode)
                {
                    helpText += "Você não está no modo de edição.\n";
                }

                if (interactable)
                {
                    helpText += string.Format(
                        "Pressione ~INPUT_ATTACK~ para {0}.",
                        mapProp.IsDoor ? "Bloquear/Desbloquear" : "interagir"
                    );
                }

                UiExtended.DisplayHelpTextThisFrame(helpText, false);

                bool isAttackJustPressedAndInteractable =
                    GTAUtils.IsDisabledControlJustPressed(Control.Attack) && mapProp.Interactable;
                if (isAttackJustPressedAndInteractable)
                {
                    InteractedEvent interacted = Interacted;
                    if (interacted != null)
                    {
                        interacted(mapProp, PlayerInventory.Instance.ItemFromName(mapProp.Id));
                    }
                }
                bool notAttackPressedOrNotPickUp =
                    !GTAUtils.IsDisabledControlJustPressed(Control.Attack)
                    || !mapProp.CanBePickedUp;
                if (!notAttackPressedOrNotPickUp)
                {
                    bool pickupFailed = !PlayerInventory.Instance.PickupItem(
                        PlayerInventory.Instance.ItemFromName(mapProp.Id),
                        ItemType.Item
                    );
                    if (!pickupFailed)
                    {
                        mapProp.Delete();
                        _map.Remove(mapProp);
                        ZombieVehicleSpawner.Instance.SpawnBlocker.Remove(mapProp.Position);
                    }
                }
            }
        }

        public bool Find(Prop prop)
        {
            if (_map == null)
            {
                return false;
            }

            return _map.Contains(prop);
        }

        private static void DisableAttackActions()
        {
            Game.DisableControlThisFrame(Control.Attack2);
            Game.DisableControlThisFrame(Control.Attack);
            Game.DisableControlThisFrame(Control.Aim);
        }

        public void NotifyListChanged()
        {
            _map.NotifyListChanged();
        }
    }
}
