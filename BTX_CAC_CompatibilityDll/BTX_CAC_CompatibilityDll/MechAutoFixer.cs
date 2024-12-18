﻿using BattleTech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomComponents;
using BattleTech.Save.SaveGameStructure;
using HarmonyLib;
using Org.BouncyCastle.Crypto.Parameters;

namespace BTX_CAC_CompatibilityDll
{
    internal class MechAutoFixer
    {
        internal static void Fixer(List<MechDef> mechs)
        {
            foreach (MechDef mech in mechs)
            {
                try
                {
                    SplitAddons(mech);
                }
                catch (Exception e)
                {
                    FileLog.Log($"except mech {mech.Description.Id} {e}");
                }
            }
        }

        private static readonly ChassisLocations[] AllLocs = new ChassisLocations[]
        {
            ChassisLocations.LeftTorso,
            ChassisLocations.RightTorso,
            ChassisLocations.CenterTorso,
            ChassisLocations.LeftArm,
            ChassisLocations.RightArm,
            ChassisLocations.LeftLeg,
            ChassisLocations.RightLeg,
            ChassisLocations.Head,
        };
        private static void SplitAddons(MechDef m)
        {
            bool hasChange = false;
            List<MechComponentRef> mechinv = m.Inventory.ToList();
            List<MechComponentRef> fixedinv = m.Chassis.FixedEquipment?.ToList();
            check(mechinv, false);
            if (fixedinv != null)
            {
                check(fixedinv, true);
                CheckTSM(fixedinv, ref hasChange);
            }

            if (hasChange)
            {
                mechinv.RemoveAll((x) => x.IsFixed); // gets re-added by setinv
                if (fixedinv != null)
                    m.Chassis.SetFixedEquipment(fixedinv.ToArray());
                m.SetInventory(mechinv.ToArray());
                m.Chassis.RefreshLocationReferences();
            }

            int slotsleft(ChassisLocations loc)
            {
                int usage = mechinv.Where((x) => x.MountedLocation == loc).Select((x) => {
                    if (x.Def == null)
                        x.RefreshComponentDef();
                    if (x.Def == null)
                    {
                        FileLog.Log($"found null comp {x.ComponentDefID} {x.ComponentDefType} in {m.Description.Id}");
                        return 0;
                    }
                    return x.Def.InventorySize;
                }).Sum();
                int max = m.Chassis.GetLocationDef(loc).InventorySlots;
                return max - usage;
            }
            void check(List<MechComponentRef> l, bool f)
            {
                for (int i = 0; i < l.Count; i++)
                {
                    if (l[i].IsFixed == f && Main.Splits.TryGetValue(l[i].ComponentDefID, out WeaponAddonSplit spl))
                    {
                        l[i].ComponentDefID = spl.WeaponId;
                        l[i].SetComponentDefType(spl.WeaponType);
                        l[i].RefreshComponentDef();
                        if (f)
                            hasChange = true;
                        if (spl.AddonId != null)
                        {
                            ChassisLocations loc = l[i].MountedLocation;
                            if (spl.NotSameLocationRequired && slotsleft(loc) <= 0)
                            {
                                foreach (ChassisLocations loc2 in AllLocs)
                                {
                                    if (slotsleft(loc2) > 0)
                                    {
                                        loc = loc2;
                                        break;
                                    }
                                }
                            }
                            MechComponentRef addon = new MechComponentRef(spl.AddonId, null, spl.AddonType, loc, -1, ComponentDamageLevel.Functional, f)
                            {
                                DataManager = m.DataManager,
                            };
                            if (spl.Link)
                            {
                                string guid = Guid.NewGuid().ToString();
                                l[i].LocalGUID = guid;
                                addon.TargetComponentGUID = guid;
                            }
                            l.Insert(i + 1, addon);
                            hasChange = true;
                        }
                        if (spl.AddSupportHardpoint) // why is LocationDef a struct???
                        {
                            LocationDef[] locs = m.Chassis.GetLocations();
                            for (int j = 0; j < locs.Length; ++j)
                            {
                                if (locs[j].Location == l[i].MountedLocation)
                                {
                                    LocationDef loc = locs[j];
                                    loc.Hardpoints = loc.Hardpoints.Append(new HardpointDef(WeaponCategoryEnumeration.GetSupport(), false)).ToArray();
                                    locs[j] = loc;
                                    hasChange = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void CheckTSM(List<MechComponentRef> fix, ref bool hasChange)
        {
            bool first = true;
            foreach (MechComponentRef c in fix)
            {
                if (c.ComponentDefID == "Gear_Triple_Strength_Myomer" || c.ComponentDefID == "Gear_Triple_Strength_Myomer_3" || c.ComponentDefID == "Gear_TSM_Prototype_Bergan")
                {
                    if (first)
                    {
                        first = false;
                        continue;
                    }
                    hasChange = true;
                    c.ComponentDefID += "_Idle";
                    c.RefreshComponentDef();
                }
            }
        }

        internal static void Register()
        {
#if !DEBUG
            AutoFixer.Shared.RegisterMechFixer(Fixer);
#endif
        }
    }
}
