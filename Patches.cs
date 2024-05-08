using System;
using System.Collections.Generic;
using System.Reflection;
using Aki.Reflection.Patching;
using Comfort.Common;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using HarmonyLib;

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
                new ItemViewPatches.method_7Path()
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
                    MethodInfo method_7 = AccessTools.Method(__instance.GetType(), "method_7");
                    EBodyPart? damagedBodyPart = null;
                    bool method_7_result = (bool)method_7.Invoke(__instance, new object[] { item, bodyPart, true, damagedBodyPart });
                    __result = method_7_result;
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
                Logger.LogInfo($"[MarsyApp-FixDragsMed] Plugin TryGetBodyPartToApply Used start");
                if (!__result && FixDragsMed.checkMedKitInDragList(item.TemplateId))
                {
                    MethodInfo method_7 = AccessTools.Method(__instance.GetType(), "method_7");
                    object[] parameters = { item, bodyPart, false, damagedBodyPart };
                    bool method_7_result = (bool)method_7.Invoke(__instance, parameters);
                    __result = method_7_result;
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
                return typeof(ActiveHealthController).GetMethod("DoMedEffect", BindingFlags.Instance | BindingFlags.Public);
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
                return typeof(GClass417).GetMethod("method_1", BindingFlags.Instance | BindingFlags.Public);
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
                return typeof(GClass2416<>).MakeGenericType(typeof(IEffect)).GetMethod("HasPartsToApply", BindingFlags.Instance | BindingFlags.Public);
            }

            [PatchPostfix]
            private static void PatchPostfix(GClass2416<IEffect> __instance, ref Item item, ref IResult __result) 
            {
                
                Logger.LogInfo($"[MarsyApp-FixDragsMed] Plugin HasPartsToApply Used");
                
                MethodInfo _method_10 = AccessTools.Method(__instance.GetType(), "method_10");
                MethodInfo _method_8 = AccessTools.Method(__instance.GetType(), "method_8");
                
                IResult apply = (IResult)_method_10.Invoke(__instance, new object[] { item });
                if (apply.Failed)
                {
                    __result = apply;
                    Logger.LogInfo($"[MarsyApp-FixDragsMed] Plugin HasPartsToApply Used 1: {__result}");
                    return;
                }
                    
                HealthEffectsComponent itemComponent1 = item.GetItemComponent<HealthEffectsComponent>();
                if (!FixDragsMed.checkMedKitInDragList(item.TemplateId) && (item is GClass2728 || !string.IsNullOrEmpty(itemComponent1?.StimulatorBuffs)))
                {
                    __result = SuccessfulResult.New;
                    Logger.LogInfo($"[MarsyApp-FixDragsMed] Plugin HasPartsToApply Used 2: {__result}");
                    return;
                }
                   
                FoodDrinkComponent component;
                if (item.TryGetItemComponent(out component))
                {
                    if (component.HpPercent <= 0.0)
                    {
                        __result = new FailedResult("Health/ItemResourceDepleted");
                        Logger.LogInfo($"[MarsyApp-FixDragsMed] Plugin HasPartsToApply Used 3: {__result}");
                        return;
                    }
                    
                    __result = SuccessfulResult.New;
                    Logger.LogInfo($"[MarsyApp-FixDragsMed] Plugin HasPartsToApply Used 4: {__result}");
                    return;
                }
                    
                MedKitComponent itemComponent2 = item.GetItemComponent<MedKitComponent>();
                bool num2 = (bool)_method_8.Invoke(__instance, new object[] { itemComponent1, itemComponent2, EBodyPart.Common });
                
                if (!num2)
                {
                    __result = new FailedResult("Inventory/IncompatibleItem");
                    Logger.LogInfo($"[MarsyApp-FixDragsMed] Plugin HasPartsToApply Used 5: {__result}");
                    return;
                }

                __result = SuccessfulResult.New;
                Logger.LogInfo($"[MarsyApp-FixDragsMed] Plugin HasPartsToApply Used 6: {__result}");
            }
        }
        
        public class method_7Path : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(GClass2416<>).MakeGenericType(typeof(IEffect)), "method_7");
            }
            
            public static bool smethod_0(EBodyPart? part, out EBodyPart? result)
            {
                result = part;
                return result.HasValue;
            }

            [PatchPostfix]
            private static void PatchPostfix(GClass2416<IEffect> __instance, Item item,
                EBodyPart bodyPart,
                bool fastSearch,
                out EBodyPart? damagedBodyPart, ref bool __result)
            {
                
                Logger.LogInfo($"[MarsyApp-FixDragsMed] Plugin method_7 Used");

                MethodInfo _method_8 = AccessTools.Method(__instance.GetType(), "method_8"); // bool
                MethodInfo _method_9 = AccessTools.Method(__instance.GetType(), "method_9"); // EBodyPart?
                MethodInfo _method_10 = AccessTools.Method(__instance.GetType(), "method_10"); // IResult


                IResult method_10_result = (IResult)_method_10.Invoke(__instance, new object[] { item });
                if (method_10_result.Failed)
                {
                    __result = smethod_0(new EBodyPart?(), out damagedBodyPart);
                    Logger.LogInfo($"[MarsyApp-FixDragsMed] Plugin method_7 Used 1 {__result}");
                    return;
                }
                
                FoodDrinkComponent component;
                if (item.TryGetItemComponent<FoodDrinkComponent>(out component) && component.HpPercent.Positive() &&
                    (bodyPart == EBodyPart.Common || bodyPart == EBodyPart.Head))
                {
                    __result = smethod_0(new EBodyPart?(EBodyPart.Head), out damagedBodyPart);
                    Logger.LogInfo($"[MarsyApp-FixDragsMed] Plugin method_7 Used 2 {__result}");
                    return;
                }

                if (item is GClass2728 && bodyPart != EBodyPart.Common && !FixDragsMed.checkMedKitInDragList(item.TemplateId))
                {
                    __result = smethod_0(new EBodyPart?(bodyPart), out damagedBodyPart);
                    Logger.LogInfo($"[MarsyApp-FixDragsMed] Plugin method_7 Used 3 {__result}");
                    return;
                }
                    
                MedKitComponent itemComponent1 = item.GetItemComponent<MedKitComponent>();
                HealthEffectsComponent itemComponent2 = item.GetItemComponent<HealthEffectsComponent>();

                if (!fastSearch)
                {
                    damagedBodyPart = (EBodyPart?)_method_9.Invoke(__instance, new object[] { itemComponent2, itemComponent1, bodyPart });
                }
                else
                {
                    damagedBodyPart =(bool)_method_8.Invoke(__instance, new object[] { itemComponent2, itemComponent1, bodyPart }) ? new EBodyPart?(bodyPart) : new EBodyPart?();
                }
                
                if (item is GClass2728 && !FixDragsMed.checkMedKitInDragList(item.TemplateId))
                {
                    __result = smethod_0(damagedBodyPart ?? EBodyPart.Head, out damagedBodyPart);
                    Logger.LogInfo($"[MarsyApp-FixDragsMed] Plugin method_7 Used 4 {__result}");
                    return;
                }

                if (!string.IsNullOrEmpty(itemComponent2?.StimulatorBuffs) && !FixDragsMed.checkMedKitInDragList(item.TemplateId))
                {
                    __result = smethod_0(new EBodyPart?(EBodyPart.Head), out damagedBodyPart);
                    Logger.LogInfo($"[MarsyApp-FixDragsMed] Plugin method_7 Used 5 {__result}");
                    return;
                }
                
                __result = damagedBodyPart.HasValue;
                Logger.LogInfo($"[MarsyApp-FixDragsMed] Plugin method_7 Used 6 {__result}");
            }
        }
    }
}
