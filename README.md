# BTX_CAC_Compatibility

Modifies BTX (and BTXMinusWeapons, if present) for CAC.

Installation: (requires working BTX installation)
 - Download CustomBundle https://github.com/BattletechModders/CustomBundle
 - Remove BTMLColorLOSMod and MechResizer from your BTX installation (as CAC and CU basically do the same)
 - Add CustomAmmoCategories, CustomComponents, CustomLocalization and CustomUnits to your mods folder (only add these 4 from CustomBundle, the rest is not needed)
 - Remove the folder CustomAmmoCategories/StreamingAssets
 - Add BTX_CAC_Compatibility, overriding the settings of CustomAmmoCategories, CustomUnits, MissionControl and BiggerDrops


Weapon list:
 - PPC: added FI OFF mode
 - ER PPC: -
 - Snub PPC: single projectile, damage falloff over medium range
 
 - AC: fixed firing speed, only one visual projectile
 - LBX: added cluster/slug ammo, cluster ammo uses CAC shells instead of multiple projectiles
 - UAC: fixed firing speed, only one visual projectile, added AC mode (1 shot, but +4 acc)
 - Gauss: -
 
 - Laser: -
 - ER Laser: -
 - Pulse Laser: fixed animation
 - Tag: -
 
 - LRM: added hotload mode
 - Artemis IV LRM: added hotload mode, changed acc to +4 in direct fire (+0 indirect), added clustering
 
 - SRM: added inferno ammo (inferno causes fires everywhere)
 - Artemis IV SRM: added inferno ammo, added clustering, +4 acc
 - Streak SRM: added inferno ammo, added streak effect+clustering
 - Infernos: Broken, use SRM inferno ammo instead (TODO change them in itemcollections)
 - Narc: -
 
 - Flamer: added forestfires
 - MG: added double speed mode (double shots, -4acc, +heat)
 
 TODO edit texts

If you want Urban vehicles to leave blood on destruction, look at CACs settings and change "DrawBloodChance" to 0.3
