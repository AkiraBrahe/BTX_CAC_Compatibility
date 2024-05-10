﻿using AccessExtension;
using BattleTech;
using BattleTech.Save;
using BattleTech.UI;
using CustAmmoCategories;
using CustomActivatableEquipment;
using HarmonyLib;
using HBS.Logging;
using InControl;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UIWidgets;
using UnityEngine;

[assembly: AssemblyVersion("0.1.22.0")]

namespace BTX_CAC_CompatibilityDll
{
    class Main
    {
        public static Settings Sett;
        public static ILog Log;

        public static void Init(string directory, string settingsJSON)
        {
            Log = HBS.Logging.Logger.GetLogger("BTX_CAC_Compatibility");
            try
            {
                Sett = JsonConvert.DeserializeObject<Settings>(settingsJSON);
            }
            catch (Exception e)
            {
                Sett = new Settings();
                Log.LogException(e);
            }
            if (Sett.LogLevelLog)
                HBS.Logging.Logger.SetLoggerLevel("BTX_CAC_Compatibility", LogLevel.Log);
            Harmony harmony = new Harmony("com.github.mcb5637.BTX_CAC_Compatibility");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            AccessExtensionPatcher.PatchAll(harmony, Assembly.GetExecutingAssembly());
            AbstractActor_InitStats.Patch(harmony);
            AbstractActor_IndirectImmune.Patch(harmony);
            CU2ComponentFix.Patch(harmony);
            if (Sett.FixDropslotsInOldSaves)
                SimGameState_InitStats.Patch(harmony);
            //harmony.Patch(
            //    AccessTools.Method(typeof(EffectManager), "CreateEffect", new Type[] { typeof(EffectData), typeof(string), typeof(int), typeof(ICombatant), typeof(ICombatant), typeof(WeaponHitInfo), typeof(int), typeof(bool) }),
            //    null, new HarmonyMethod(AccessTools.Method(typeof(MarkEffects_Patch), "Postfix")), null);
            //harmony.Patch(AccessTools.DeclaredMethod(typeof(Mech), "DamageLocation"), new HarmonyMethod(AccessTools.Method(typeof(Main), "LogDamageLoc")), null, null);


            if (Sett.MECompat)
                return;

            InfernoExplode.Patch(harmony);

            try
            {
                // artemis
                //Unpatch(harmony, AccessTools.DeclaredMethod(typeof(AttackDirector.AttackSequence), "GetClusteredHits"), "BEX.BattleTech.Extended_CE");
                //Type[] ptypes = new Type[] { typeof(Vector3), typeof(Mech), typeof(float), typeof(ArmorLocation), typeof(float), typeof(float), typeof(ArmorLocation), typeof(float) };
                //Unpatch(harmony, AccessTools.DeclaredMethod(typeof(HitLocation), "GetAdjacentHitLocation", ptypes), "BEX.BattleTech.Extended_CE");
                //ptypes = new Type[] { typeof(Vector3), typeof(Vehicle), typeof(float), typeof(VehicleChassisLocations), typeof(float), typeof(float), typeof(VehicleChassisLocations), typeof(float) };
                //Unpatch(harmony, AccessTools.DeclaredMethod(typeof(HitLocation), "GetAdjacentHitLocation", ptypes), "BEX.BattleTech.Extended_CE");
                // streak
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(MissileLauncherEffect), "AllMissilesComplete"), "BEX.BattleTech.Extended_CE");
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(MissileLauncherEffect), "LaunchMissile"), "BEX.BattleTech.Extended_CE");
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(AttackDirector.AttackSequence), "GetIndividualHits"), "BEX.BattleTech.Extended_CE");
                // stepped hit chance
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(ToHit), "GetUMChance"), "BEX.BattleTech.Extended_CE");
                // called shot nerf
                //Unpatch(harmony, AccessTools.DeclaredMethod(typeof(Pilot), "InitAbilities"), "BEX.BattleTech.Extended_CE", false, true, false);
                // TSM
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(Mech), "OnActivationEnd"), "BEX.BattleTech.Extended_CE", false, true, false); // prefix is heat shutdown
                // lbx/uac
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(Contract), "CompleteContract"), "BEX.BattleTech.Extended_CE", true, true, true,
                    "System.Void Extended_CE.WeaponModes+Contract_CompleteContract.Prefix(BattleTech.Contract __instance)");

                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(AttackEvaluator), "MakeAttackOrder"), "BEX.BattleTech.Extended_CE");
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(AITeam), "TurnActorProcessActivation"), "BEX.BattleTech.Extended_CE");
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(CombatHUDWeaponSlot), "OnPointerUp"), "BEX.BattleTech.Extended_CE");
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(Weapon), "get_AmmoCategoryValue"), "BEX.BattleTech.Extended_CE");
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(CombatHUDWeaponSlot), "RefreshDisplayedWeapon"), "BEX.BattleTech.Extended_CE", true, false, false); // transpiler is coil fix
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(MechValidationRules), "ValidateMechHasAppropriateAmmo"), "BEX.BattleTech.Extended_CE");

                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(MechStatisticsRules), "CalculateTonnage"), "BEX.BattleTech.Extended_CE");
                Unpatch(harmony, AccessTools.DeclaredMethod(typeof(MechLabMechInfoWidget), "CalculateTonnage"), "BEX.BattleTech.Extended_CE");
            }
            catch (Exception e)
            {
                Log.LogError(e.ToString());
            }

            try
            {
                foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (a.GetName().Name.Equals("MechQuirks"))
                    {
                        Type t = a.GetType("Quirks.Tooltips.QuirkToolTips");
                        harmony.Patch(t.GetMethod("DetailMechQuirks", BindingFlags.Static | BindingFlags.NonPublic), null, new HarmonyMethod(typeof(QuirkToolTips_DetailMechQuirks), "Postfix"), null);
                    }
                }
                //harmony.Patch(AccessTools.GetDeclaredConstructors(typeof(IndexOutOfRangeException)).Single((x)=>x.GetParameters().Length==0), null, new HarmonyMethod(typeof(Main), "ExCtorLog"));
            }
            catch (Exception e)
            {
                Log.LogError(e.ToString());
            }

            ToHitModifiersHelper.registerModifier("BTX_CAC_Compatibility_ECM", "ECM", true, true, ECM_Effect, null);
            CustomComponents.Registry.RegisterSimpleCustomComponents(Assembly.GetExecutingAssembly());
        }

        private static float ECM_Effect(ToHit tohit, AbstractActor attacker, Weapon wep, ICombatant target, Vector3 apos, Vector3 tpos, LineOfFireLevel lof, MeleeAttackType mat, bool calledshot)
        {
            float mod = 0;
            if (target is AbstractActor at && at.StatCollection.GetValue<float>("DefendedByECM") > 0)
            {
                mod += 4;
                if (at.Combat.EffectManager.GetAllEffectsTargetingWithBaseID(at, "StatusEffect-NARC-IncomingAttBonus").Count > 0)
                    mod += 3;
            }
            return mod;
        }

        private static void Unpatch(Harmony harmony, MethodBase b, string id, bool pre = true, bool post = true, bool trans = true, string onlyUnpatch = null)
        {
            //FileLog.Log($"checking to unpatch: {b.FullName()} {pre} {post} {trans}");
            Patches pa = Harmony.GetPatchInfo(b);
            if (pa == null)
            {
                //FileLog.Log("no patch attached");
                return;
            }
            if (pre)
                UnpatchCheckList(harmony, b, id, pa.Prefixes, onlyUnpatch);
            if (post)
                UnpatchCheckList(harmony, b, id, pa.Postfixes, onlyUnpatch);
            if (trans)
                UnpatchCheckList(harmony, b, id, pa.Transpilers, onlyUnpatch);
        }

        private static void UnpatchCheckList(Harmony harmony, MethodBase b, string id, IEnumerable<Patch> pa, string onlyUnpatch)
        {
            foreach (Patch p in pa)
            {
                MethodInfo patch = p.PatchMethod;
                if (p.owner.Equals(id) && (onlyUnpatch == null || onlyUnpatch.Equals(patch.FullName())))
                {
                    harmony.Unpatch(b, patch);
                    //FileLog.Log($"found: {patch.FullName()}");
                }
                else
                {
                    //FileLog.Log($"no match: {patch.FullName()} {p.owner}");
                }
            }
        }

        public static void ExCtorLog(Exception __instance)
        {
            string t = new StackTrace().ToString();
            Log.Log(t);
        }

        //public static void LogDamageLoc(int originalHitLoc, ArmorLocation aLoc)
        //{
        //    FileLog.Log($"{originalHitLoc} -> {aLoc}");
        //}
    }

    [HarmonyPatch(typeof(AuraPreviewRecord), "RecalculateStealthPips")]
    public class AuraPreviewRecord_RecalculateStealthPips
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo re = AccessTools.PropertyGetter(typeof(AbstractActor), nameof(AbstractActor.StealthPipsCurrent));
            MethodInfo add = AccessTools.Method(typeof(Dictionary<AbstractActor, int>), nameof(Dictionary<AbstractActor, int>.Add));
            foreach (CodeInstruction c in instructions)
            {
                if ((c.opcode == OpCodes.Callvirt || c.opcode == OpCodes.Call) && (MethodInfo)c.operand == re)
                {
                    c.operand = AccessTools.PropertyGetter(typeof(AbstractActor), nameof(AbstractActor.StealthPipsTotal));
                }
                if ((c.opcode == OpCodes.Callvirt || c.opcode == OpCodes.Call) && (MethodInfo)c.operand == add)
                {
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Math), nameof(Math.Max), new Type[] { typeof(int), typeof(int) }));
                }
                yield return c;
            }
        }
    }
    [HarmonyPatch(typeof(Mech), "NukeStructureLocation")]
    [HarmonyAfter("io.mission.customunits")]
    public class Mech_NukeStructureLocation
    {
        public static void Prefix(ref bool __runOriginal, Mech __instance)
        {
            if (__instance is CustomUnits.CustomMech)
                __runOriginal = false;
        }
    }

    [HarmonyPatch(typeof(CustAmmoCategoriesPatches.Mech_ApplyHeatSinks), "Prefix")]
    public class Mech_ApplyHeatSinks_Prefix
    {
        private static float Get(Mech m)
        {
            return m.StatCollection.GetStatistic("HeatSinkCapacityMult").Value<float>();
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo re = AccessTools.Field(typeof(HeatConstantsDef), nameof(HeatConstantsDef.GlobalHeatSinkMultiplier));
            MethodInfo add = AccessTools.Method(typeof(Mech_ApplyHeatSinks_Prefix), nameof(Mech_ApplyHeatSinks_Prefix.Get));
            foreach (CodeInstruction c in instructions)
            {
                if (c.opcode == OpCodes.Ldfld && (FieldInfo)c.operand == re)
                {
                    yield return c;

                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, add);
                    yield return new CodeInstruction(OpCodes.Mul);

                    continue;
                }

                yield return c;
            }
        }
    }
    [HarmonyPatch(typeof(CustomUnits.LanceConfiguratorPanel_LoadLanceConfiguration), "Prefix")]
    public class LanceConfiguratorPanel_LoadLanceConfiguration_Prefix
    {
        private static SpawnableUnit[] GetLanceUnits(LanceConfiguration cfg, string teamdef, LanceConfiguratorPanel inst)
        {
            SpawnableUnit[] r = cfg.GetLanceUnits(teamdef);
            var slots = Traverse.Create(inst).Field("loadoutSlots").GetValue<LanceLoadoutSlot[]>();
            int len = Math.Min(r.Length, slots.Length);
            int maxunits = inst.activeContract.Override.maxNumberOfPlayerUnits;
            for (int i = maxunits; i < slots.Length; ++i)
            {
                slots[i].SetLockState(LanceLoadoutSlot.LockState.Full); // set locked, cu does not do it anymore
            }
            Array.Resize(ref r, Math.Min(len, maxunits)); // remove any more mechs/pilots
            return r;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo re = AccessTools.Method(typeof(LanceConfiguration), nameof(LanceConfiguration.GetLanceUnits));
            MethodInfo getunits = AccessTools.Method(typeof(LanceConfiguratorPanel_LoadLanceConfiguration_Prefix), nameof(GetLanceUnits));
            foreach (CodeInstruction c in instructions)
            {
                if ((c.opcode == OpCodes.Call || c.opcode == OpCodes.Callvirt) && (MethodInfo)c.operand == re)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, getunits);

                    continue;
                }

                yield return c;
            }
        }
    }
    [HarmonyPatch(typeof(Mech), nameof(Mech.MaxMeleeEngageRangeDistance), MethodType.Getter)]
    public static class Mech_MaxMeleeEngageRangeDistance
    {
        public static void Postfix(Mech __instance, ref float __result)
        {
            if (__instance.CanShootAfterSprinting)
                __result = __instance.MaxSprintDistance;
        }
    }
    [HarmonyPatch]
    public class Pathing_MeleeGrid
    {
        private static PathNodeGrid GetMeleeGrid(Pathing __instance)
        {
            return __instance.OwningActor.CanShootAfterSprinting ? __instance.SprintingGrid() : __instance.WalkingGrid();
        }

        [HarmonyPatch(typeof(Pathing), nameof(Pathing.getGrid))]
        [HarmonyPatch(typeof(Pathing), nameof(Pathing.GetMeleeDestsForTarget))]
        [HarmonyPatch(typeof(Pathing), nameof(Pathing.SetMeleeTarget))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var prev = AccessTools.PropertyGetter(typeof(Pathing), "MeleeGrid");
            var rep = AccessTools.Method(typeof(Pathing_MeleeGrid), nameof(GetMeleeGrid));
            foreach (var c in instructions)
            {
                if (c.operand as MethodInfo == prev)
                    c.operand = rep;
                yield return c;
            }
        }
    }
}
