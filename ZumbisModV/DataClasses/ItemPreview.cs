using System;
using System.Linq;
using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;
using ZumbisModV.Extensions;
using ZumbisModV.Scripts;
using ZumbisModV.Static;
using ZumbisModV.Utils;

namespace ZumbisModV.DataClasses
{
    public class ItemPreview
    {
        private Vector3 _currentOffset;
        private Prop _currentPreview;
        private Prop _resultProp;
        private bool _preview;
        private bool _isDoor;
        private string _currnetPropHash;

        public bool PreviewComplete { get; private set; }

        public ItemPreview()
        {
            ScriptEventHandler.Instance.RegisterScript(OnTick);
            ScriptEventHandler.Instance.Aborted += (sender, args) => Abort();
        }

        public void OnTick(object sender, EventArgs eventArgs)
        {
            if (_preview)
            {
                CreateItemPreview();
            }
        }

        public Prop GetResult() => _resultProp;

        public void StartPreview(string propHash, Vector3 offset, bool isDoor)
        {
            if (!_preview)
            {
                _preview = true;
                _currnetPropHash = propHash;
                _isDoor = isDoor;
            }
        }

        private void CreateItemPreview()
        {
            if (_currentPreview == null)
            {
                PreviewComplete = false;
                _currentOffset = Vector3.Zero;
                Prop prop = World.CreateProp(
                    _currnetPropHash,
                    new Vector3(),
                    new Vector3(),
                    false,
                    false
                );
                if (prop == null)
                {
                    Notification.Show(
                        string.Format(
                            "Falha ao carregar a propriedade, mesmo após a solicitação.\nNome da propriedade: {0}",
                            _currnetPropHash
                        )
                    );
                    _resultProp = null;
                    _preview = false;
                    PreviewComplete = true;
                }
                else
                {
                    prop.IsCollisionEnabled = false;
                    _currentPreview = prop;
                    _currentPreview.Opacity = 150;
                    Database.PlayerPed.Weapons.Select(WeaponHash.Unarmed, true);
                    _resultProp = null;
                }
            }
            else
            {
                UiExtended.DisplayHelpTextThisFrame(
                    "Pressione ~INPUT_AIM~ para cancelar.\nPressione ~INPUT_ATTACK~ para colocar o item.",
                    true
                );
                Game.DisableControlThisFrame(Control.Aim); //25
                Game.DisableControlThisFrame(Control.Attack); //24
                Game.DisableControlThisFrame(Control.Attack2); //257
                Game.DisableControlThisFrame(Control.ParachuteBrakeLeft); //152
                Game.DisableControlThisFrame(Control.ParachuteBrakeRight); //153
                Game.DisableControlThisFrame(Control.Cover); //44
                Game.DisableControlThisFrame(Control.Phone); //27
                Game.DisableControlThisFrame(Control.PhoneUp); //172
                Game.DisableControlThisFrame(Control.PhoneDown); //173
                Game.DisableControlThisFrame(Control.Sprint); //21
                GameExtended.DisableWeaponWheel();
                if (GTAUtils.IsDisabledControlPressed(0, Control.Aim))
                {
                    _currentPreview.Delete();
                    _currentPreview = (_resultProp = null);
                    _preview = false;
                    PreviewComplete = true;
                    ScriptEventHandler.Instance.UnregisterScript(new EventHandler(OnTick));
                }
                else
                {
                    Vector3 position = GameplayCamera.Position;
                    Vector3 direction = GameplayCamera.Direction;
                    Vector3 hitCoords = World
                        .Raycast(
                            position,
                            position + direction * 15f,
                            IntersectFlags.Everything,
                            Database.PlayerPed
                        )
                        .HitPosition;

                    if (
                        hitCoords != Vector3.Zero
                        && hitCoords.DistanceTo(Database.PlayerPosition) > 1.5f
                    )
                    {
                        DrawScaleForms();
                        float num = Game.IsControlPressed(Control.Sprint) ? 1.5f : 1f;
                        if (Game.IsControlPressed(Control.ParachuteBrakeLeft))
                        {
                            Vector3 rotation = _currentPreview.Rotation;
                            rotation.Z += Game.LastFrameTime * 50f * num;
                            _currentPreview.Rotation = rotation;
                        }
                        else if (Game.IsControlPressed(Control.ParachuteBrakeRight))
                        {
                            Vector3 rotation = _currentPreview.Rotation;
                            rotation.Z -= Game.LastFrameTime * 50f * num;
                            _currentPreview.Rotation = rotation;
                        }
                        if (Game.IsControlPressed(Control.PhoneUp))
                            _currentOffset.Z += Game.LastFrameTime * num;
                        else if (Game.IsControlPressed(Control.PhoneDown))
                            _currentOffset.Z -= Game.LastFrameTime * num;
                        _currentPreview.Position = hitCoords + _currentOffset;
                        _currentPreview.IsVisible = true;
                        if (
                            Game.IsControlJustPressed(Control.Attack)
                            && !Game.IsControlEnabled(Control.Attack)
                        )
                            return;
                        _currentPreview.ResetOpacity();
                        _resultProp = _currentPreview;
                        _resultProp.IsCollisionEnabled = true;
                        _resultProp.IsPositionFrozen = !_isDoor;
                        _preview = false;
                        _currentPreview = null;
                        _currnetPropHash = string.Empty;
                        PreviewComplete = true;
                        ScriptEventHandler.Instance.UnregisterScript(new EventHandler(OnTick));
                    }
                    else
                        _currentPreview.IsVisible = false;
                }
            }
        }

        private static void DrawScaleForms()
        {
            Scaleform scaleform = new Scaleform("instructional_buttons");
            scaleform.CallFunction("CLEAR_ALL", 0);
            scaleform.CallFunction("TOGGLE_MOUSE_BUTTONS", 0);
            scaleform.CallFunction("CREATE_CONTAINER", 0);
            scaleform.CallFunction(
                "SET_DATA_SLOT",
                0,
                Function.Call<string>(Hash.GET_CONTROL_INSTRUCTIONAL_BUTTONS_STRING, 2, 152, 0),
                string.Empty
            );
            scaleform.CallFunction(
                "SET_DATA_SLOT",
                1,
                Function.Call<string>(Hash.GET_CONTROL_INSTRUCTIONAL_BUTTONS_STRING, 2, 153, 0),
                "Girar"
            );
            scaleform.CallFunction(
                "SET_DATA_SLOT",
                2,
                Function.Call<string>(Hash.GET_CONTROL_INSTRUCTIONAL_BUTTONS_STRING, 2, 172, 0),
                string.Empty
            );
            scaleform.CallFunction(
                "SET_DATA_SLOT",
                3,
                Function.Call<string>(Hash.GET_CONTROL_INSTRUCTIONAL_BUTTONS_STRING, 2, 173, 0),
                "Elevar/Abaixar"
            );
            scaleform.CallFunction(
                "SET_DATA_SLOT",
                4,
                Function.Call<string>(Hash.GET_CONTROL_INSTRUCTIONAL_BUTTONS_STRING, 2, 21, 0),
                "Acelerar"
            );
            scaleform.CallFunction("DRAW_INSTRUCTIONAL_BUTTONS", -1);
            scaleform.Render2D();
        }

        public void Abort()
        {
            _currentPreview?.Delete();
        }
    }
}
