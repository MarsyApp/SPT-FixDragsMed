using System;
using System.Collections.Generic;
using System.Reflection;
using Aki.Reflection.Patching;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using UnityEngine;

namespace FixDragsMed
{
    class Patcher
    {
        public static void PatchAll()
        {
            new PatchManager().RunPatches();
        }
        
        public static void UnpatchAll()
        {
            new PatchManager().RunUnpatches();
        }
    }

    public class PatchManager
    {
        public PatchManager()
        {
            this._patches = new List<ModulePatch>
            {
                new ItemViewPatches.CanApplyItemPath(),
                new ItemViewPatches.TryGetBodyPartToApplyPath(),
                new ItemViewPatches.ApplyItemPath(),
                new ItemViewPatches.DoMedEffectPath(),
                new ItemViewPatches.method_1Path(),
                new ItemViewPatches.HasPartsToApplyPath(),
                new ItemViewPatches.method_9Path()
            };
        }

        public void RunPatches()
        {
            foreach (ModulePatch patch in this._patches)
            {
                patch.Enable();
            }
        }
        
        public void RunUnpatches()
        {
            foreach (ModulePatch patch in this._patches)
            {
                patch.Disable();
            }
        }

        private readonly List<ModulePatch> _patches;
    }

    public static class ItemViewPatches
    {
        public class CanApplyItemPath : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return typeof(HealthControllerClass).GetMethod("CanApplyItem", BindingFlags.Instance | BindingFlags.Public);
            }

            [PatchPostfix]
            private static void PatchPostfix(HealthControllerClass __instance, Item item, EBodyPart bodyPart, ref bool __result)
            {
                Logger.LogInfo($"[MarsyApp-FixDragsMed] Plugin CanApplyItem Used: {__result}");
                
                if (!__result && FixDragsMed.checkMedKitInDragList(item.TemplateId))
                {
                    MethodInfo method_9 = AccessTools.Method(__instance.GetType(), "method_9");
                    EBodyPart? damagedBodyPart = null;
                    bool method_9_result = (bool)method_9.Invoke(__instance, new object[] { item, bodyPart, true, damagedBodyPart });
                    __result = method_9_result;
                    Logger.LogInfo($"[MarsyApp-FixDragsMed] Plugin CanApplyItem Used 1: {__result}");
                }
            }
        }
        
        public class TryGetBodyPartToApplyPath : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(HealthControllerClass), "TryGetBodyPartToApply");
            }

            [PatchPostfix]
            private static void PatchPostfix(HealthControllerClass __instance, Item item, EBodyPart bodyPart, ref EBodyPart? damagedBodyPart, ref bool __result)
            {
                if (!__result && FixDragsMed.checkMedKitInDragList(item.TemplateId))
                {
                    MethodInfo method_9 = AccessTools.Method(__instance.GetType(), "method_9");
                    object[] parameters = { item, bodyPart, false, damagedBodyPart };
                    bool method_9_result = (bool)method_9.Invoke(__instance, parameters);
                    __result = method_9_result;
                    damagedBodyPart = (EBodyPart?)parameters[3];
                    Logger.LogInfo($"[MarsyApp-FixDragsMed] Plugin TryGetBodyPartToApply Used 1: {__result}");
                }
            }
        }
        
        public class ApplyItemPath : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return typeof(HealthControllerClass).GetMethod("ApplyItem", BindingFlags.Instance | BindingFlags.Public);
            }

            [PatchPostfix]
            private static void PatchPostfix(ref bool __instance)
            {
                // __instance = false;
                Logger.LogInfo($"[MarsyApp-FixDragsMed] Plugin ApplyItem Used: {__instance}");
            }
        }
        
        public class DoMedEffectPath : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return typeof(ActiveHealthControllerClass).GetMethod("DoMedEffect", BindingFlags.Instance | BindingFlags.Public);
            }

            [PatchPostfix]
            private static void PatchPostfix(ref IEffect __result)
            {
                // __instance = false;
                Logger.LogInfo($"[MarsyApp-FixDragsMed] Plugin DoMedEffect Used");
            }
        }
        
        public class method_1Path : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return typeof(GClass402).GetMethod("method_1", BindingFlags.Instance | BindingFlags.NonPublic);
            }

            [PatchPostfix]
            private static void PatchPostfix()
            {
                // __instance = false;
                Logger.LogInfo($"[MarsyApp-FixDragsMed] Plugin method_1 Used");
            }
        }

        public class HasPartsToApplyPath : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return typeof(GClass2104<>).MakeGenericType(typeof(IEffect)).GetMethod("HasPartsToApply", BindingFlags.Instance | BindingFlags.Public);
            }

            [PatchPostfix]
            private static void PatchPostfix(GClass2104<IEffect> __instance, ref Item item, ref (bool Result, string Error) __result) 
            {
                Logger.LogInfo($"[MarsyApp-FixDragsMed] Plugin HasPartsToApply Used");
                MethodInfo _method_12 = AccessTools.Method(__instance.GetType(), "method_12");
                MethodInfo _method_10 = AccessTools.Method(__instance.GetType(), "method_10");
                    
                var method_12_result = (ValueTuple<bool, string>)_method_12.Invoke(__instance, new object[] { item });
   
                (bool, string) result = (method_12_result.Item1, method_12_result.Item2);
                
                if (!result.Item1)
                {
                    __result = result;
                    Logger.LogInfo($"[MarsyApp-FixDragsMed] Plugin HasPartsToApply Used 1: {__result}");
                    return;
                }
                
                HealthEffectsComponent itemComponent = item.GetItemComponent<HealthEffectsComponent>();

                if (FixDragsMed.checkMedKitInDragList(item.TemplateId) || (!(item is GClass2385) && string.IsNullOrEmpty(itemComponent?.StimulatorBuffs)))
                {
                    if (item.TryGetItemComponent<FoodDrinkComponent>(out var component))
                    {
                        bool num = component.HpPercent > 0f;
                        __result = (num, num ? string.Empty : "Health/ItemResourceDepleted");
                        Logger.LogInfo($"[MarsyApp-FixDragsMed] Plugin HasPartsToApply Used 2: {__result}");
                        return;
                    }
                    MedKitComponent itemComponent2 = item.GetItemComponent<MedKitComponent>();
                    bool num2 = (bool)_method_10.Invoke(__instance, new object[] { itemComponent, itemComponent2, EBodyPart.Common });
                    __result = (num2, num2 ? string.Empty : "Inventory/IncompatibleItem");
                    Logger.LogInfo($"[MarsyApp-FixDragsMed] Plugin HasPartsToApply Used 3: {__result}");
                    return;
                }
                __result = (true, string.Empty);
                Logger.LogInfo($"[MarsyApp-FixDragsMed] Plugin HasPartsToApply Used 4: {__result}");
            }
        }
        
        public class method_9Path : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(GClass2104<>).MakeGenericType(typeof(IEffect)), "method_9");
            }
            
            private static bool smethod_0(EBodyPart? part, out EBodyPart? result)
            {
                result = part;
                return result.HasValue;
            }

            [PatchPostfix]
            private static void PatchPostfix(GClass2104<IEffect> __instance, Item item,
                EBodyPart bodyPart,
                bool fastSearch,
                out EBodyPart? damagedBodyPart, ref bool __result)
            {
                
                Logger.LogInfo($"[MarsyApp-FixDragsMed] Plugin method_9 Used");
                
                MethodInfo _method_12 = AccessTools.Method(__instance.GetType(), "method_12");
                MethodInfo _method_10 = AccessTools.Method(__instance.GetType(), "method_10");
                MethodInfo _method_11 = AccessTools.Method(__instance.GetType(), "method_11");
                
                var method_12_result = (ValueTuple<bool, string>)_method_12.Invoke(__instance, new object[] { item });
   
                (bool, string) result = (method_12_result.Item1, method_12_result.Item2);
                if (!result.Item1)
                {
                    __result = smethod_0(null, out damagedBodyPart);
                    Logger.LogInfo($"[MarsyApp-FixDragsMed] Plugin method_9 Used 1 {__result}");
                    return;
                }
                if (item.TryGetItemComponent<FoodDrinkComponent>(out var component) && component.HpPercent.Positive() && (bodyPart == EBodyPart.Common || bodyPart == EBodyPart.Head))
                {
                    __result = smethod_0(EBodyPart.Head, out damagedBodyPart);
                    Logger.LogInfo($"[MarsyApp-FixDragsMed] Plugin method_9 Used 2 {__result}");
                    return;
                }
                if (item is GClass2385 && !FixDragsMed.checkMedKitInDragList(item.TemplateId) && bodyPart != EBodyPart.Common)
                {
                    __result = smethod_0(bodyPart, out damagedBodyPart);
                    Logger.LogInfo($"[MarsyApp-FixDragsMed] Plugin method_9 Used 3 {__result}");
                    return;
                }
                MedKitComponent itemComponent = item.GetItemComponent<MedKitComponent>();
                HealthEffectsComponent itemComponent2 = item.GetItemComponent<HealthEffectsComponent>();
                if (fastSearch)
                {
                    damagedBodyPart =((bool)_method_10.Invoke(__instance, new object[] { itemComponent2, itemComponent, bodyPart }) ? new EBodyPart?(bodyPart) : null);
                }
                else
                {
                    damagedBodyPart = (EBodyPart?)_method_11.Invoke(__instance, new object[] { itemComponent2, itemComponent, bodyPart });
                }
                if (item is GClass2385 && !FixDragsMed.checkMedKitInDragList(item.TemplateId))
                {
                    __result = smethod_0(damagedBodyPart ?? EBodyPart.Head, out damagedBodyPart);
                    Logger.LogInfo($"[MarsyApp-FixDragsMed] Plugin method_9 Used 4 {__result}");
                    return;
                }
                if (!string.IsNullOrEmpty(itemComponent2?.StimulatorBuffs) && !FixDragsMed.checkMedKitInDragList(item.TemplateId))
                {
                    __result = smethod_0(EBodyPart.Head, out damagedBodyPart);
                    Logger.LogInfo($"[MarsyApp-FixDragsMed] Plugin method_9 Used 5 {__result}");
                    return;
                }
                __result = damagedBodyPart.HasValue;
                Logger.LogInfo($"[MarsyApp-FixDragsMed] Plugin method_9 Used 6 {__result}");
            }
        }
    }
}
