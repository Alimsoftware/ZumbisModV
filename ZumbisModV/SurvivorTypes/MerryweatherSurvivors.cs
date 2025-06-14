using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GTA;
using GTA.Math;
using GTA.UI;
using LemonUI;
using LemonUI.Elements;
using LemonUI.Scaleform;
using LemonUI.TimerBars;
using ZumbisModV.DataClasses;
using ZumbisModV.Extensions;
using ZumbisModV.PlayerManagement;
using ZumbisModV.Static;
using ZumbisModV.Wrappers;

namespace ZumbisModV.SurvivorTypes
{
    public class MerryweatherSurvivors : Survivors
    {
        public const float InteractDistance = 2.3f;
        private const int BlipRadius = 145;
        private readonly int _timeOut;
        private DataClasses.ParticleEffect _particle;
        private PedGroup _pedGroup = new PedGroup();
        private List<Ped> _peds = new List<Ped>();
        private Blip _blip;
        private Prop _prop;
        private DropType _dropType;
        private bool _notify;
        private Vector3 _dropZone;
        private readonly TimerBarCollection _timerBarPool;
        private readonly TimerBarProgress _timerBar;
        private float _currentTime;
        private readonly PedHash[] _pedHashes;
        private readonly WeaponHash[] _weapons;

        public MerryweatherSurvivors(int timeout)
        {
            // Inicializa o array de hashes de pedestres
            _pedHashes = new PedHash[]
            {
                PedHash.Armoured01SMM,
                PedHash.ArmGoon01GMM,
                PedHash.Armoured02SMM,
            };
            // Inicializa o array de hashes de armas
            _weapons = new WeaponHash[]
            {
                WeaponHash.CarbineRifle,
                WeaponHash.PumpShotgun,
                WeaponHash.Pistol,
                WeaponHash.SMG,
                WeaponHash.AssaultRifle,
            };

            _timerBar = new TimerBarProgress("TEMPO RESTANTE");
            _timeOut = timeout;
            _currentTime = _timeOut;
        }

        public override void Update()
        {
            if (_prop == null || !_prop.Exists())
                return;

            TryInteract(_prop);
            UpdateTimer();

            if (CantSeeCrate())
                return;
            // Substitua por:
            if (_prop.AttachedBlip == null || !_prop.AttachedBlip.Exists())
            {
                Blip blip1 = _prop.AddBlip();
                blip1.Sprite = BlipSprite.CrateDrop;
                blip1.Color = BlipColor.Yellow;
                blip1.Name = "Caixa de Drop";
                // Remove o blip anterior, se houver
                _blip?.Delete();
                _blip = blip1; // Atualiza a referência ao novo blip
            }

            foreach (Ped ped in _peds)
            {
                Blip blip2 = ped.AddBlip();
                blip2.Color = BlipColor.Yellow;
                blip2.Name = "Segurança Merryweather";
            }
        }

        private void UpdateTimer()
        {
            if (PlayerPosition.VDist(_dropZone) < 145f)
            {
                if (!_notify)
                {
                    BigMessage message = new BigMessage("~r~Entrando na Zona Hostil", "")
                    {
                        Type = MessageType.MissionPassedOldGen,
                    };

                    // Opcional: Efeito visual ao entrar na zona
                    World.AddExplosion(PlayerPosition, ExplosionType.Rocket, 0f, 1f);
                    _notify = true;
                }
                if (MenuController.TimerBars.Contains(_timerBar))
                {
                    MenuController.TimerBars.Remove(_timerBar);
                }
            }
            else
            {
                if (!MenuController.TimerBars.Contains(_timerBar))
                {
                    MenuController.TimerBars.Add(_timerBar);
                }

                _timerBar.Progress = _currentTime / (float)_timeOut;
                _currentTime -= Game.LastFrameTime;
                if (_currentTime <= 0f)
                {
                    OnCompleted();
                    Notification.Show("~r~Falha~s~ ao recuperar a caixa.");
                }
            }
        }

        private bool CantSeeCrate()
        {
            if (_prop == null)
                return true;

            if (_prop.AttachedBlip == null || !_prop.AttachedBlip.Exists())
                return true;

            if (PlayerPosition == null)
                return true;

            return !_prop.IsOnScreen
                || _prop.IsOccluded
                || _prop.Position.VDist(PlayerPosition) > 50f;
        }

        public override void SpawnEntities()
        {
            Vector3 spawnPoint = GetSpawnPoint();
            if (!IsValidSpawn(spawnPoint) || spawnPoint == Vector3.Zero)
            {
                Logger.LogError("Ponto de spawn inválido.");
                return;
            }
            DropType[] dropType = (DropType[])Enum.GetValues(typeof(DropType));
            _dropType = dropType[Database.Random.Next(dropType.Length)];

            _prop = World.CreateProp(
                model: _dropType == DropType.Weapons
                    ? "prop_mil_crate_01"
                    : "ex_prop_crate_closed_bc",
                position: spawnPoint,
                rotation: Vector3.Zero,
                dynamic: false,
                placeOnGround: false
            );
            if (_prop == null)
            {
                Notification.Show("Erro ao criar o objeto de drop.");
                OnCompleted();
                return;
            }

            _blip = World.CreateBlip(_prop.Position.Around(45f), 145f);
            if (_blip == null)
            {
                Notification.Show("Erro ao criar o blip.");
                OnCompleted();
                return;
            }
            _blip.Color = BlipColor.Yellow;
            _blip.Alpha = 150;
            _dropZone = _blip.Position;

            _particle = WorldExtended.CreateParticleEffectAtCoord(
                _prop.Position.Around(5f),
                "exp_grd_flare"
            );

            if (_particle == null)
            {
                Notification.Show("Erro ao criar partículas.");
                OnCompleted();
                return;
            }

            _particle.Color = Color.LightGoldenrodYellow;

            // Criação de pedestres
            int num = Database.Random.Next(3, 6);

            if (_pedGroup == null)
                _pedGroup = new PedGroup();
            if (_peds == null)
                _peds = new List<Ped>();

            for (int i = 0; i <= num; i++)
            {
                Vector3 spawnPosition = spawnPoint.Around(10f);
                Logger.LogInfo($"Tentando spawnar ped em {spawnPosition}");
                Ped ped = World.CreatePed(
                    _pedHashes[Database.Random.Next(_pedHashes.Length)],
                    spawnPosition
                );

                if (ped is null || !ped.Exists())
                {
                    Logger.LogError("Falha ao criar um ped.");
                    continue;
                }

                WeaponHash weapon = _weapons[Database.Random.Next(_weapons.Length)];

                if (i > 0)
                    ped.Weapons.Give(weapon, 45, true, true);
                else
                    ped.Weapons.Give(WeaponHash.SniperRifle, 15, true, true);

                ped.Accuracy = 100;
                ped.Task.GuardCurrentPosition();
                ped.RelationshipGroup = Relationships.MilitiaRelationship;

                _pedGroup.Add(ped, i == 0);
                _peds.Add(ped);

                new EntityEventWrapper(ped).Died += PedWrapperOnDied;
            }

            Model vehicleModel = new Model("mesa3");
            Vector3 positionOnStreet = World.GetNextPositionOnStreet(_prop.Position.Around(25f));
            World.CreateVehicle(vehicleModel, positionOnStreet);

            string dropTypeText = _dropType == DropType.Loot ? "loot" : "armas";
            Notification.Show($"Drop de {dropTypeText} ~y~Merryweather~s~ por perto.");
        }

        private void PedWrapperOnDied(object sender, EntityEventArgs e)
        {
            _peds.Remove(e.Entity as Ped);
            e.Entity.MarkAsNoLongerNeeded();
            e.Entity.AttachedBlip?.Delete();
            (sender as EntityEventWrapper).Dispose();
        }

        private void TryInteract(Entity prop)
        {
            if (prop == null || prop.Position.VDist(PlayerPosition) >= 2.2999999523162842)
                return;
            UiExtended.DisplayHelpTextThisFrame("Pressione ~INPUT_CONTEXT~ para saquear.");
            Game.DisableControlThisFrame(Control.Context);
            if (!Game.IsControlJustPressed(Control.Context))
                return;
            prop.Delete();
            switch (_dropType)
            {
                case DropType.Weapons:
                    int num1 = Database.Random.Next(3, 5);
                    int num2 = 0;
                    for (int index = 0; index <= num1; ++index)
                    {
                        WeaponHash[] array = (
                            (IEnumerable<WeaponHash>)Enum.GetValues(typeof(WeaponHash))
                        )
                            .Where(new Func<WeaponHash, bool>(IsGoodHash))
                            .ToArray();
                        if (array.Length != 0)
                        {
                            PlayerPed.Weapons.Give(
                                (WeaponHash)(int)(uint)array[Database.Random.Next(array.Length)],
                                Database.Random.Next(20, 45),
                                false,
                                true
                            );
                            ++num2;
                        }
                    }
                    Notification.Show(string.Format("~g~{0}~s~ armas encontradas.", num2));
                    break;
                case DropType.Loot:
                    int num3 = Database.Random.Next(1, 3);
                    PlayerInventory.Instance.PickupLoot(null, ItemType.Item, num3, num3, 0.4f);
                    break;
            }
            OnCompleted();
        }

        public override void CleanUp()
        {
            _particle.Delete();
            _peds?.ForEach(
                (
                    ped =>
                    {
                        ped.AttachedBlip?.Delete();
                        ped.AlwaysKeepTask = true;
                        ped.IsPersistent = false;
                    }
                )
            );
            _blip?.Delete();
            if (!MenuController.TimerBars.Contains(_timerBar))
                return;
            MenuController.TimerBars.Remove(_timerBar);
        }

        public override void Abort()
        {
            _particle?.Delete();
            _prop?.Delete();
            _peds?.ForEach(ped =>
            {
                ped.AttachedBlip?.Delete();
                ped.Delete();
            });
            _blip?.Delete();
            if (!MenuController.TimerBars.Contains(_timerBar))
                return;
            MenuController.TimerBars.Remove(_timerBar);
        }

        private bool IsGoodHash(WeaponHash hash)
        {
            return hash != WeaponHash.Unarmed
                && hash != WeaponHash.BZGas
                && hash != WeaponHash.Ball
                && hash != WeaponHash.Snowball
                && !PlayerPed.Weapons.HasWeapon(hash);
        }

        private enum DropType
        {
            Weapons,
            Loot,
        }
    }
}
