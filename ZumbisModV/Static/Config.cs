using System;
using System.IO;
using GTA;
using GTA.UI;

namespace ZumbisModV.Static
{
    public class Config
    {
        public static string VersionId = "1.0.0";

        public const string ScriptFilePath = "./scripts/";

        public const string IniFilePath = "./scripts/ZumbisModV.ini";

        public const string InventoryFilePath = "./scripts/Inventory.dat";

        public const string MapFilePath = "./scripts/Map.dat";

        public const string VehicleFilePath = "./scripts/Vehicles.dat";

        public const string GuardsFilePath = "./scripts/Guards.dat";

        public static void Check()
        {
            ScriptSettings scriptSettings = ScriptSettings.Load("./scripts/ZumbisModV.ini");

            if (scriptSettings.GetValue("mod", "version_id", "0") != VersionId)
            {
                if (File.Exists("./scripts/ZumbisModV.ini"))
                {
                    File.Delete("./scripts/ZumbisModV.ini");
                }

                if (File.Exists("./scripts/Inventory.dat"))
                {
                    File.Delete("./scripts/Inventory.dat");
                }
                Notification.Show(
                    string.Format(
                        "Atualizando ZumbisModV para a versão ~g~{0}~s~. Substituindo o arquivo de inventário ",
                        VersionId
                    ) + " já que há novos itens.",
                    true
                );
                scriptSettings.SetValue("mod", "version_id", VersionId);
                scriptSettings.Save();
            }
        }
    }
}
