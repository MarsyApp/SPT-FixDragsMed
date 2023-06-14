using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using Newtonsoft.Json.Linq;

namespace FixDragsMed
{
    [BepInPlugin("com.MarsyApp.FixDragsMed", "MarsyApp-FixDragsMed", "1.0.0")]
    public class FixDragsMed : BaseUnityPlugin
    {
        public static List<string> listDragsMedicines = new List<string>();
        
        public static bool checkMedKitInDragList(string id)
        {
            return listDragsMedicines.Contains(id);
        }
        private void Awake()
        {
            LoadConfig();
            Patcher.PatchAll();
            Logger.LogInfo($"Plugin FixDragsMed is loaded!");
        }
        
        private void OnDestroy()
        {
            Patcher.UnpatchAll();
            Logger.LogInfo($"Plugin FixDragsMed is unloaded!");
        }
        
        private void LoadConfig()
        {
            string modPath = Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(FixDragsMed)).Location);
            modPath.Replace('\\', '/');
            
            /*listDragsMedicines.Add("544fb25a4bdc2dfb738b4567");
            listDragsMedicines.Add("5751a25924597722c463c472");
            listDragsMedicines.Add("60098af40accd37ef2175f27");
            listDragsMedicines.Add("5e831507ea0a7c419c2f9bd9");
            listDragsMedicines.Add("5e8488fa988a8701445df1e4");
            listDragsMedicines.Add("544fb3364bdc2d34748b456a");
            listDragsMedicines.Add("5af0454c86f7746bf20992e8");
            listDragsMedicines.Add("5755356824597772cb798962");
            listDragsMedicines.Add("590c661e86f7741e566b646a");
            listDragsMedicines.Add("544fb45d4bdc2dee738b4568");
            listDragsMedicines.Add("590c678286f77426c9660122");
            listDragsMedicines.Add("60098ad7c2240c0fe85c570a");
            listDragsMedicines.Add("590c657e86f77412b013051d");
            listDragsMedicines.Add("5d02778e86f774203e7dedbe");
            listDragsMedicines.Add("5d02797c86f774203f38e30a");*/
            
            
            try
            {
                JObject config = JObject.Parse(File.ReadAllText(modPath + "/Config.json"));

                Logger.LogInfo("Loading configs...");
                Logger.LogInfo("Configs loaded: " + config.ToString());
                
                if (config["listDragsMedicines"] != null)
                {
                    listDragsMedicines = config["listDragsMedicines"].ToObject<List<string>>();
                    Logger.LogInfo("Configs loaded: " + listDragsMedicines.Count);
                    Logger.LogInfo("Configs loaded: " + listDragsMedicines.ToString());
                }

                Logger.LogInfo("Configs loaded");
            }
            catch (FileNotFoundException)
            {
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error loading config: {ex.Message}");
            }
        
        }
    }
}
