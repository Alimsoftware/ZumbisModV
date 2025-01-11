using System;
using System.Windows.Forms;
using GTA;
using GTA.Native;
using GTA.UI;
using LemonUI.Menus;
using ZumbisModV.Controllers;
using ZumbisModV.Extensions;
using ZumbisModV.PlayerManagement;
using ZumbisModV.Scripts;
using ZumbisModV.Static;
using ZumbisModV.Zumbis;

namespace ZumbisModV
{
    public class ModController : Script
    {
        private static readonly LemonUI.ObjectPool pool = new LemonUI.ObjectPool();

        private Keys _menuKey = Keys.F10;
        public NativeMenu MainMenu { get; private set; }
        public static ModController Instance { get; private set; }

        public ModController()
        {
            Initialize();
        }

        private void Initialize()
        {
            Instance = this;
            Config.Check();
            Relationships.SetRelationships();
            LoadSave();
            ConfigureMenu();
            Tick += OnTick;
            KeyDown += OnKeyDown;
            Logger.LogInfo("ModController initialized.");
            Notification.Show("GTA V Zumbis v1.0 by ~b~Ali~y~m~g~software ~w~loaded!", true);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (MenuController.MenuPool.AreAnyVisible || e.KeyCode != _menuKey)
                return;
            MainMenu.Visible = !MainMenu.Visible;

            //if (e.KeyCode == Keys.F10)
            // {
            //    GTA.UI.Screen.ShowSubtitle("~g~Ativado", 2500);
            //   MainMenu.Visible = !MainMenu.Visible;
            // }
        }

        private void OnTick(object sender, EventArgs e)
        {
            // este método é chamado a cada tick do jogo (cerca de 60 vezes por segundo)
            // é usado para realizar verificações ou atualizações repetidas
            pool.Process();
            if (Game.WasCheatStringJustEntered("zumbi"))
            {
                MainMenu.Visible = true;
            }
        }

        private void LoadSave()
        {
            _menuKey = Settings.GetValue("keys", "zombies_menu_key", _menuKey);
            ZumbiPed.ZombieDamage = Settings.GetValue(
                section: "zombies",
                name: "zombie_damage",
                defaultvalue: ZumbiPed.ZombieDamage
            );
            Settings.SetValue("keys", "zombies_menu_key", _menuKey);
            Settings.SetValue("zombies", "zombie_damage", ZumbiPed.ZombieDamage);
            Settings.Save();
        }

        private void ConfigureMenu()
        {
            MainMenu = new NativeMenu("~r~Zumbi~w~Mod~g~V", "por ~b~Ali~y~m~g~software", "");
            pool.Add(MainMenu);
            NativeCheckboxItem cbInfec = new NativeCheckboxItem(
                title: "Modo Infecção",
                description: "Ativa/Desaativa o apocalipse",
                check: false
            );
            cbInfec.CheckboxChanged += (sender, e) =>
            {
                ZombieCreator.Runners = cbInfec.Checked;
                Loot247.Instance.Spawn = cbInfec.Checked;
                WorldController.Configure = cbInfec.Checked;
                AnimalSpawner.Instance.Spawn = cbInfec.Checked;

                if (cbInfec.Checked)
                    return;
                WorldExtended.ClearAreaOfEverything(Database.PlayerPosition, 10000f);
                Function.Call(Hash.REQUEST_IPL, "cs3_07_mpgates");
                ZombieVehicleSpawner.Instance.Spawn = cbInfec.Checked;
            };

            NativeCheckboxItem cbZRapid = new NativeCheckboxItem(
                title: "Zumbis Rápidos",
                description: "Ativar/desativar zumbis corredores.",
                check: false
            );
            cbZRapid.CheckboxChanged += (sender, e) =>
            {
                ZombieCreator.Runners = cbZRapid.Checked;
            };

            NativeCheckboxItem cbEletr = new NativeCheckboxItem(
                title: "Eletricidade",
                description: "Ativa/desativa o modo blackout.",
                check: true
            );
            cbEletr.CheckboxChanged += (sender, e) =>
            {
                World.Blackout = !cbEletr.Checked;
                GTA.UI.Screen.ShowSubtitle(
                    string.Format("Blackout {0}", cbEletr.Checked ? "~r~desativado" : "~g~ativado"),
                    2500
                );
                if (!cbEletr.Checked)
                {
                    RemoveAmbientSounds();
                }
                Logger.LogInfo(string.Format("Updated {0}", cbEletr.Checked));
            };

            NativeCheckboxItem cbSobrev = new NativeCheckboxItem(
                "Sobreviventes",
                "Ativar/desativar sobreviventes.",
                false
            );
            cbSobrev.CheckboxChanged += (sender, isChecked) =>
            {
                SurvivorController.Instance.Spawn = cbSobrev.Checked;
            };

            NativeCheckboxItem cbStats = new NativeCheckboxItem(
                "Estatísticas",
                "Ativar/desativar estatísticas.",
                true
            );
            cbStats.CheckboxChanged += (sender, isChecked) => {
                // /PlayerStats.UseStats = cbStats.Checked;
            };

            NativeItem menuLoad = new NativeItem(
                "Carregar",
                "Carregar o mapa, seus veículos e seus guarda-costas."
            );
            var badge = new BadgeSet("commonmenu", "shop_health_icon_a", "shop_health_icon_b");
            menuLoad.LeftBadgeSet = badge;
            menuLoad.Activated += (sender, item) => {
                // PlayerMap.Instance.Deserialize();
                // PlayerVehicles.Instance.Deserialize();
                //PlayerGroupManager.Instance.Deserialize();
            };

            NativeItem menuSave = new NativeItem(
                "Salvar",
                "Salvar o veículo em que você está atualmente."
            );
            var car = new BadgeSet("commonmenu", "shop_garage_icon_a", "shop_garage_icon_b");
            menuSave.LeftBadgeSet = car;
            menuSave.Activated += (sender, item) =>
            {
                if (
                    Database.PlayerCurrentVehicle == null
                    || (
                        Database.PlayerCurrentVehicle != null
                        && !Database.PlayerCurrentVehicle.Exists()
                    )
                )
                {
                    Notification.Show("Você não está em um veículo.");
                }
                else
                {
                    //PlayerVehicles.Instance.SaveVehicle(Database.PlayerCurrentVehicle);
                }
            };

            NativeItem menuSaveAll = new NativeItem(
                "Salvar Tudo",
                "Salvar todos os veículos marcados e suas posições."
            );
            var car2 = new BadgeSet("commonmenu", "shop_garage_icon_a", "shop_garage_icon_b");
            menuSaveAll.LeftBadgeSet = car2;
            menuSaveAll.Activated += (sender, item) => {
                //PlayerVehicles.Instance.Serialize(true);
            };

            NativeItem menuSaveG = new NativeItem(
                "Salvar Tudo",
                "Salvar o grupo de peds (guardas) do jogador."
            );
            var mask2 = new BadgeSet("commonmenu", "shop_mask_icon_a", "shop_mask_icon_b");
            menuSaveG.LeftBadgeSet = mask2;
            menuSaveG.Activated += (sender, item) => {
                //PlayerGroupManager.Instance.SavePeds();
            };
            MainMenu.Add(cbInfec);
            MainMenu.Add(cbZRapid);
            MainMenu.Add(cbEletr);
            MainMenu.Add(cbSobrev);
            MainMenu.Add(cbStats);
            MainMenu.Add(menuLoad);
            MainMenu.Add(menuSave);
            MainMenu.Add(menuSaveAll);
            MainMenu.Add(menuSaveG);
        }

        private void RemoveAmbientSounds()
        {
            Function.Call<bool>(
                Hash.START_AUDIO_SCENE,
                new InputArgument[] { "DLC_MPHEIST_TRANSITION_TO_APT_FADE_IN_RADIO_SCENE" }
            );
            Function.Call<bool>(
                Hash.SET_STATIC_EMITTER_ENABLED,
                new InputArgument[] { "LOS_SANTOS_VANILLA_UNICORN_01_STAGE", false }
            );
            Function.Call<bool>(
                Hash.SET_STATIC_EMITTER_ENABLED,
                new InputArgument[] { "LOS_SANTOS_VANILLA_UNICORN_02_MAIN_ROOM", false }
            );
            Function.Call<bool>(
                Hash.SET_STATIC_EMITTER_ENABLED,
                new InputArgument[] { "LOS_SANTOS_VANILLA_UNICORN_03_BACK_ROOM", false }
            );
            Function.Call<bool>(
                Hash.SET_AMBIENT_ZONE_LIST_STATE_PERSISTENT,
                new InputArgument[] { "AZL_DLC_Hei4_Island_Disabled_Zones", false, true }
            );
            Function.Call<bool>(
                Hash.SET_AMBIENT_ZONE_LIST_STATE_PERSISTENT,
                new InputArgument[] { "AZL_DLC_Hei4_Island_Zones", true, true }
            );
            Function.Call<bool>(
                Hash.SET_SCENARIO_TYPE_ENABLED,
                new InputArgument[] { "WORLD_VEHICLE_STREETRACE", false }
            );
            Function.Call<bool>(
                Hash.SET_SCENARIO_TYPE_ENABLED,
                new InputArgument[] { "WORLD_VEHICLE_SALTON_DIRT_BIKE", false }
            );
            Function.Call<bool>(
                Hash.SET_SCENARIO_TYPE_ENABLED,
                new InputArgument[] { "WORLD_VEHICLE_SALTON", false }
            );
            Function.Call<bool>(
                Hash.SET_SCENARIO_TYPE_ENABLED,
                new InputArgument[] { "WORLD_VEHICLE_POLICE_NEXT_TO_CAR", false }
            );
            Function.Call<bool>(
                Hash.SET_SCENARIO_TYPE_ENABLED,
                new InputArgument[] { "WORLD_VEHICLE_POLICE_CAR", false }
            );
            Function.Call<bool>(
                Hash.SET_SCENARIO_TYPE_ENABLED,
                new InputArgument[] { "WORLD_VEHICLE_POLICE_BIKE", false }
            );
            Function.Call<bool>(
                Hash.SET_SCENARIO_TYPE_ENABLED,
                new InputArgument[] { "WORLD_VEHICLE_MILITARY_PLANES_SMALL", false }
            );
            Function.Call<bool>(
                Hash.SET_SCENARIO_TYPE_ENABLED,
                new InputArgument[] { "WORLD_VEHICLE_MILITARY_PLANES_BIG", false }
            );
            Function.Call<bool>(
                Hash.SET_SCENARIO_TYPE_ENABLED,
                new InputArgument[] { "WORLD_VEHICLE_MECHANIC", false }
            );
            Function.Call<bool>(
                Hash.SET_SCENARIO_TYPE_ENABLED,
                new InputArgument[] { "WORLD_VEHICLE_EMPTY", false }
            );
            Function.Call<bool>(
                Hash.SET_SCENARIO_TYPE_ENABLED,
                new InputArgument[] { "WORLD_VEHICLE_BUSINESSMEN", false }
            );
            Function.Call<bool>(
                Hash.SET_SCENARIO_TYPE_ENABLED,
                new InputArgument[] { "WORLD_VEHICLE_BIKE_OFF_ROAD_RACE", false }
            );
            Function.Call<bool>(
                Hash.START_AUDIO_SCENE,
                new InputArgument[] { "FBI_HEIST_H5_MUTE_AMBIENCE_SCENE" }
            );
            Function.Call<bool>(
                Hash.START_AUDIO_SCENE,
                new InputArgument[] { "CHARACTER_CHANGE_IN_SKY_SCENE" }
            );
            Function.Call<bool>(
                Hash.SET_AUDIO_FLAG,
                new InputArgument[] { "PoliceScannerDisabled", true }
            );
            Function.Call<bool>(
                Hash.SET_AUDIO_FLAG,
                new InputArgument[] { "DisableFlightMusic", true }
            );
            Function.Call<bool>(Hash.SET_RANDOM_EVENT_FLAG, new InputArgument[] { false });
        }
    }
}
