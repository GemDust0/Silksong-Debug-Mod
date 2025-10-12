using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GlobalEnums;
using GlobalSettings;
using HarmonyLib;
using HutongGames.PlayMaker.Actions;
using InControl;
using InControl.UnityDeviceProfiles;
using System;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.PlayerLoop;
using static GamepadVibrationMixer.GamepadVibrationEmission;
using static PlayerDataTest;
using static SteelSoulQuestSpot;
using static ToolCrest;

[BepInPlugin("SilksongDebugMod", "Silksong Debug Mod", "1.0.0")]
public class SilksongMod : BaseUnityPlugin
{
    private static BepInEx.Logging.ManualLogSource logger;
    private static ConfigEntry<string> equippedCrest;
    private static ConfigEntry<int> unlockedHunter;
    private static ConfigEntry<bool> unlockedReaper;
    private static ConfigEntry<bool> unlockedWanderer;
    private static ConfigEntry<bool> unlockedWarrior;
    private static ConfigEntry<bool> unlockedToolmaster;
    private static ConfigEntry<bool> unlockedWitch;
    private static ConfigEntry<bool> unlockedSpell;
    private static ConfigEntry<bool> needolin;
    private static ConfigEntry<bool> deepElegy;
    private static ConfigEntry<bool> dash;
    private static ConfigEntry<bool> drifter;
    private static ConfigEntry<bool> walljump;
    private static ConfigEntry<bool> doubleJump;
    private static ConfigEntry<bool> needleArt;
    private static ConfigEntry<bool> soar;
    private static ConfigEntry<bool> clawline;
    private static ConfigEntry<int> rosaries;
    private static ConfigEntry<int> shards;
    private static ConfigEntry<bool> uncappedRosaries;
    private static ConfigEntry<bool> uncappedShards;
    private static ConfigEntry<bool> canChangeEquipsAnywhere;
    private static ConfigEntry<bool> infiniteSilk;
    private static ConfigEntry<bool> infiniteHealth;
    private static ConfigEntry<bool> invincible;

    /* TODO
     * defeated[]
     * disableSilkAbilities, HasSilkSpecial
     * FleaGames[]
     * Has[]Map
     * HasQuill, QuillState
     * HasPin[]
     * HasMarker[]
     * HasMelody[]
     * HasSlabKey[]
    */

    private void Awake()
    {
        logger = BepInEx.Logging.Logger.CreateLogSource("Silksong Debug Mod");
        equippedCrest = Config.Bind(
                "Crest Settings",
                "Equipped Crest",
                "Hunter",
                new ConfigDescription("The Crest you have equipped. Will automatically unlock the crest if not yet unlocked.", new AcceptableValueList<string>(new string[] { "Hunter", "Hunter_v2", "Hunter_v3", "Reaper", "Wanderer", "Warrior", "Toolmaster", "Witch", "Spell" }))
            );
        unlockedHunter = Config.Bind(
                "Crest Settings",
                "Unlocked Hunter Crest",
                1,
                new ConfigDescription("The level of Hunter's Crest you've unlocked", new AcceptableValueRange<int>(0, 3))
            );
        unlockedReaper = Config.Bind(
                "Crest Settings",
                "Unlocked Reaper Crest",
                false,
                "Whether or not you've unlocked the Reaper's Crest"
            );
        unlockedWanderer = Config.Bind(
                "Crest Settings",
                "Unlocked Wanderer Crest",
                false,
                "Whether or not you've unlocked the Wanderer's Crest"
            );
        unlockedWarrior = Config.Bind(
                "Crest Settings",
                "Unlocked Beast Crest",
                false,
                "Whether or not you've unlocked the Beast's Crest"
            );
        unlockedToolmaster = Config.Bind(
                "Crest Settings",
                "Unlocked Architect Crest",
                false,
                "Whether or not you've unlocked the Architect's Crest"
            );
        unlockedWitch = Config.Bind(
                "Crest Settings",
                "Unlocked Witch Crest",
                false,
                "Whether or not you've unlocked the Witch's Crest"
            );
        unlockedSpell = Config.Bind(
                "Crest Settings",
                "Unlocked Shaman Crest",
                false,
                "Whether or not you've unlocked the Shaman's Crest"
            );
        needolin = Config.Bind(
                "Needolin",
                "Needolin",
                false,
                "Whether or not you have Needolin"
            );
        deepElegy = Config.Bind(
                "Needolin",
                "Elegy of the Deep",
                false,
                "Whether or not you have Elegy of the Deep"
            );
        dash = Config.Bind(
                "Abilities",
                "Swift Step",
                false,
                "Whether or not you have Swift Step"
            );
        drifter = Config.Bind(
                "Abilities",
                "Drifter Cloak",
                false,
                "Whether or not you have Drifter cloak"
            );
        walljump = Config.Bind(
                "Abilities",
                "Cling Grip",
                false,
                "Whether or not you have Cling Grip"
            );
        doubleJump = Config.Bind(
                "Abilities",
                "Double Jump",
                false,
                "Whether or not you have Faydown Cloak's double jump"
            );
        needleArt = Config.Bind(
                "Abilities",
                "Needle Art",
                false,
                "Whether or not you have Needle Art"
            );
        soar = Config.Bind(
                "Abilities",
                "Silksoar",
                false,
                "Whether or not you have Silksoar"
            );
        clawline = Config.Bind(
                "Abilities",
                "Clawline",
                false,
                "Whether or not you have Clawline"
            );
        rosaries = Config.Bind(
                "Misc",
                "Rosaries",
                0,
                new ConfigDescription("Amount of rosaries you have", new AcceptableValueRange<int>(0, int.MaxValue))
            );
        shards = Config.Bind(
                "Misc",
                "Shell Shards",
                0,
                new ConfigDescription("Amount of shell shards you have", new AcceptableValueRange<int>(0, int.MaxValue))
            );
        uncappedRosaries = Config.Bind(
                "Misc",
                "Uncapped Rosaries",
                false,
                "Removes the cap to rosaries"
            );
        uncappedShards = Config.Bind(
                "Misc",
                "Uncapped Shards",
                false,
                "Removes the cap to shards"
            );
        canChangeEquipsAnywhere = Config.Bind(
                "Misc",
                "Can Change Equips Anywhere",
                false,
                "Allows you to change equips anywhere"
            );
        infiniteSilk = Config.Bind(
                "Cheats",
                "Infinite Silk",
                false,
                "Whether or not you have infinite silk"
            );
        infiniteHealth = Config.Bind(
                "Cheats",
                "Infinite Health",
                false,
                "Whether or not you have infinite health"
            );
        invincible = Config.Bind(
                "Cheats",
                "Invincible",
                false,
                "Whether or not you are invincible"
            );

        // Bind events
        equippedCrest.SettingChanged += EquippedCrestChanged;
        unlockedHunter.SettingChanged += HunterChanged;
        unlockedReaper.SettingChanged += ReaperChanged;
        unlockedWanderer.SettingChanged += WandererChanged;
        unlockedWarrior.SettingChanged += WarriorChanged;
        unlockedToolmaster.SettingChanged += ToolmasterChanged;
        unlockedWitch.SettingChanged += WitchChanged;
        unlockedSpell.SettingChanged += SpellChanged;
        needolin.SettingChanged += NeedolinChanged;
        deepElegy.SettingChanged += DeepElegyChanged;
        dash.SettingChanged += DashChanged;
        drifter.SettingChanged += DrifterChanged;
        walljump.SettingChanged += WalljumpChanged;
        doubleJump.SettingChanged += DoubleJumpChanged;
        needleArt.SettingChanged += NeedleArtChanged;
        soar.SettingChanged += SoarChanged;
        clawline.SettingChanged += ClawlineChanged;
        rosaries.SettingChanged += RosariesChanged;
        shards.SettingChanged += ShardsChanged;
        canChangeEquipsAnywhere.SettingChanged += ChangeAnywhereChanged;
        infiniteSilk.SettingChanged += InfiniteSilkChanged;


        logger.LogInfo("Plugin loaded and initialized.");

        Harmony.CreateAndPatchAll(typeof(SilksongMod), null);
    }

    // Modify data upon starting a new save
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerData), "SetupNewPlayerData")]
    private static void SetupNewPlayerDataPostfix(PlayerData __instance) 
    {
        // Start player with desired crest
        __instance.ToolEquips = new ToolCrestsData();
        if (unlockedHunter.Value != 0)
        {
            __instance.ToolEquips.SetData("Hunter", new ToolCrestsData.Data { IsUnlocked = true });
            if (unlockedHunter.Value > 1)
            {
                __instance.ToolEquips.SetData("Hunter_v2", new ToolCrestsData.Data { IsUnlocked = true });
                if (unlockedHunter.Value == 3)
                {
                    __instance.ToolEquips.SetData("Hunter_v3", new ToolCrestsData.Data { IsUnlocked = true });

                }
            }
        }
        if (unlockedReaper.Value)
        {
            __instance.ToolEquips.SetData("Reaper", new ToolCrestsData.Data
            {
                IsUnlocked = true
            });
        }
        if (unlockedWanderer.Value)
        {
            __instance.ToolEquips.SetData("Wanderer", new ToolCrestsData.Data
            {
                IsUnlocked = true
            });
        }
        if (unlockedWarrior.Value)
        {
            __instance.ToolEquips.SetData("Warrior", new ToolCrestsData.Data { IsUnlocked = true });
        }
        if (unlockedToolmaster.Value)
        {
            __instance.ToolEquips.SetData("Toolmaster", new ToolCrestsData.Data { IsUnlocked = true });
        }
        if (unlockedWitch.Value)
        {
            __instance.ToolEquips.SetData("Witch", new ToolCrestsData.Data { IsUnlocked = true });
        }
        if (unlockedSpell.Value)
        {
            __instance.ToolEquips.SetData("Spell", new ToolCrestsData.Data
            {
                IsUnlocked = true
            });
        }
        __instance.CurrentCrestID = equippedCrest.Value;

        // Start the player with other selected starting items
        __instance.hasNeedolin = needolin.Value;
        __instance.hasNeedolinMemoryPowerup = deepElegy.Value;
        __instance.hasDash = dash.Value;
        __instance.hasBrolly = drifter.Value;
        __instance.hasWalljump = walljump.Value;
        __instance.hasDoubleJump = doubleJump.Value;
        __instance.hasChargeSlash = needleArt.Value;
        __instance.hasSuperJump = soar.Value;
        __instance.hasHarpoonDash = clawline.Value;
        __instance.geo = rosaries.Value;
        __instance.ShellShards = shards.Value;
        CheatManager.CanChangeEquipsAnywhere = canChangeEquipsAnywhere.Value;
        __instance.silk = infiniteSilk.Value ? 9 : 0;

        __instance.HasSeenNeedolin = true;
        __instance.HasSeenDash = true;
        __instance.HasSeenWalljump = true;
        __instance.HasSeenSuperJump = true;
        __instance.HasSeenGeo = true;
        __instance.HasSeenShellShards = true;
    }

    // Set values of config to save file when loading an existing one
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerData), "SetupExistingPlayerData")]
    private static void SetupExistingPlayerDataPostfix(PlayerData __instance)
    {
        equippedCrest.Value = __instance.CurrentCrestID;
        if (__instance.ToolEquips.GetData("Hunter_v3").IsUnlocked)
        {
            unlockedHunter.Value = 3;
        }
        else if (__instance.ToolEquips.GetData("Hunter_v2").IsUnlocked)
        {
            unlockedHunter.Value = 2;
        }
        else if (__instance.ToolEquips.GetData("Hunter").IsUnlocked)
        {
            unlockedHunter.Value = 1;
        }
        if (__instance.ToolEquips.GetData("Reaper").IsUnlocked)
        {
            unlockedReaper.Value = true;
        }
        if (__instance.ToolEquips.GetData("Wanderer").IsUnlocked)
        {
            unlockedWanderer.Value = true;
        }
        if (__instance.ToolEquips.GetData("Warrior").IsUnlocked)
        {
            unlockedWarrior.Value = true;
        }
        if (__instance.ToolEquips.GetData("Toolmaster").IsUnlocked)
        {
            unlockedToolmaster.Value = true;
        }
        if (__instance.ToolEquips.GetData("Witch").IsUnlocked)
        {
            unlockedWitch.Value = true;
        }
        if (__instance.ToolEquips.GetData("Spell").IsUnlocked)
        {
            unlockedSpell.Value = true;
        }
        needolin.Value = __instance.hasNeedolin;
        deepElegy.Value = __instance.hasNeedolinMemoryPowerup;
        dash.Value = __instance.hasDash;
        drifter.Value = __instance.hasBrolly;
        walljump.Value = __instance.hasWalljump;
        doubleJump.Value = __instance.hasDoubleJump;
        needleArt.Value = __instance.hasChargeSlash;
        soar.Value = __instance.hasSuperJump;
        clawline.Value = __instance.hasHarpoonDash;
        rosaries.Value = __instance.geo;
        shards.Value = __instance.ShellShards;
        CheatManager.CanChangeEquipsAnywhere = canChangeEquipsAnywhere.Value;
        __instance.silk = infiniteSilk.Value ? __instance.silkMax : __instance.silk;
    }

    // Unlimited Shards and Rosaries
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GlobalSettings.Gameplay), "GetCurrencyCap")]
    private static void GetCurrencyCapPostfix(GlobalSettings.Gameplay __instance, CurrencyType type, ref int __result)
    {
        if ((type == CurrencyType.Money && uncappedRosaries.Value) || (type == CurrencyType.Shard && uncappedShards.Value))
        {
            __result = int.MaxValue;
        }
    }

    // Crests
    private static void EquippedCrestChanged(object sender, EventArgs args){
        PlayerData __instance = PlayerData.instance;
        ToolItemManager.SetEquippedCrest(equippedCrest.Value);
        switch (equippedCrest.Value){
            case "Hunter":
                unlockedHunter.Value = 1;
                Gameplay.HunterCrest3.Unlock();
                break;
            case "Hunter_v2":
                unlockedHunter.Value = 2;
                Gameplay.HunterCrest3.Unlock();
                break;
            case "Hunter_v3":
                unlockedHunter.Value = 3;
                Gameplay.HunterCrest3.Unlock();
                break;
            case "Reaper":
                unlockedReaper.Value = true;
                Gameplay.ReaperCrest.Unlock();
                break;
            case "Wanderer":
                unlockedWanderer.Value = true;
                Gameplay.WandererCrest.Unlock();
                break;
            case "Warrior":
                unlockedWarrior.Value = true;
                Gameplay.WarriorCrest.Unlock();
                break;
            case "Toolmaster":
                unlockedToolmaster.Value = true;
                Gameplay.ToolmasterCrest.Unlock();
                break;
            case "Witch":
                unlockedWitch.Value = true;
                Gameplay.WitchCrest.Unlock();
                break;
            case "Spell":
                unlockedSpell.Value = true;
                Gameplay.SpellCrest.Unlock();
                break;
        }
        ToolItemManager.SendEquippedChangedEvent();
    }
    private static void HunterChanged(object sender, EventArgs args)
    {
        Gameplay.HunterCrest.Unlock();
    }
    private static void ReaperChanged(object sender, EventArgs args)
    {
        Gameplay.ReaperCrest.Unlock();
    }
    private static void WandererChanged(object sender, EventArgs args)
    {
        Gameplay.WandererCrest.Unlock();
    }
    private static void WarriorChanged(object sender, EventArgs args)
    {
        Gameplay.WarriorCrest.Unlock();
    }
    private static void ToolmasterChanged(object sender, EventArgs args)
    {
        Gameplay.ToolmasterCrest.Unlock();
    }
    private static void WitchChanged(object sender, EventArgs args)
    {
        Gameplay.WitchCrest.Unlock();
    }
    private static void SpellChanged(object sender, EventArgs args)
    {
        Gameplay.SpellCrest.Unlock();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ToolCrest), "Unlock")]
    private static void UnlockPostfix(ToolCrest __instance)
    {
        switch (__instance.name)
        {
            case "Hunter":
                if (unlockedHunter.Value == 0)
                {
                    __instance.SaveData = new ToolCrestsData.Data
                    {
                        IsUnlocked = false,
                        Slots = __instance.SaveData.Slots,
                        DisplayNewIndicator = false
                    };
                }
                else if (PlayerData.instance.CurrentCrestID == "Hunter")
                {
                    unlockedHunter.Value = 1;
                    equippedCrest.Value = "Hunter";
                }
                break;
            case "Hunter_v2":
                if (unlockedHunter.Value < 2)
                {
                    __instance.SaveData = new ToolCrestsData.Data
                    {
                        IsUnlocked = false,
                        Slots = __instance.SaveData.Slots,
                        DisplayNewIndicator = false
                    };
                    equippedCrest.Value = "Hunter";
                    if (!(unlockedHunter.Value == 1)) // Check if Hunter 1 is unlocked
                    {
                        Gameplay.HunterCrest.Unlock(); // Revoke if not
                    }
                }
                else if (PlayerData.instance.CurrentCrestID == "Hunter_v2")
                {
                    unlockedHunter.Value = 2;
                    equippedCrest.Value = "Hunter_v2";
                }
                break;
            case "Hunter_v3":
                if (unlockedHunter.Value < 3)
                {
                    __instance.SaveData = new ToolCrestsData.Data
                    {
                        IsUnlocked = false,
                        Slots = __instance.SaveData.Slots,
                        DisplayNewIndicator = false
                    };
                    if (!(unlockedHunter.Value == 2)) // Check if  Hunter 2 is unlocked
                    {
                        Gameplay.HunterCrest2.Unlock(); // Revoke if not
                    } else
                    {
                        equippedCrest.Value = "Hunter_v2";
                    }
                }
                else if (PlayerData.instance.CurrentCrestID == "Hunter_v3")
                {
                    unlockedHunter.Value = 3;
                    equippedCrest.Value = "Hunter_v3";
                }
                break;
            case "Reaper":
                if (!(unlockedReaper.Value == true))
                {
                    __instance.SaveData = new ToolCrestsData.Data
                    {
                        IsUnlocked = false,
                        Slots = __instance.SaveData.Slots,
                        DisplayNewIndicator = false
                    };
                }
                else
                {
                    unlockedReaper.Value = true;
                    equippedCrest.Value = "Reaper";
                }
                break;
            case "Wanderer":
                if (!(unlockedWanderer.Value == true))
                {
                    __instance.SaveData = new ToolCrestsData.Data
                    {
                        IsUnlocked = false,
                        Slots = __instance.SaveData.Slots,
                        DisplayNewIndicator = false
                    };
                }
                else
                {
                    unlockedWanderer.Value = true;
                    equippedCrest.Value = "Wanderer";
                }
                break;
            case "Warrior":
                if (!(unlockedWarrior.Value == true))
                {
                    __instance.SaveData = new ToolCrestsData.Data
                    {
                        IsUnlocked = false,
                        Slots = __instance.SaveData.Slots,
                        DisplayNewIndicator = false
                    };
                }
                else
                {
                    unlockedWarrior.Value = true;
                    equippedCrest.Value = "Warrior";
                }
                break;
            case "Toolmaster":
                if (!(unlockedToolmaster.Value == true))
                {
                    __instance.SaveData = new ToolCrestsData.Data
                    {
                        IsUnlocked = false,
                        Slots = __instance.SaveData.Slots,
                        DisplayNewIndicator = false
                    };
                }
                else
                {
                    unlockedToolmaster.Value = true;
                    equippedCrest.Value = "Toolmaster";
                }
                break;
            case "Witch":
                if (!(unlockedWitch.Value == true))
                {
                    __instance.SaveData = new ToolCrestsData.Data
                    {
                        IsUnlocked = false,
                        Slots = __instance.SaveData.Slots,
                        DisplayNewIndicator = false
                    };
                }
                else
                {
                    unlockedWitch.Value = true;
                    equippedCrest.Value = "Witch";
                }
                break;
            case "Spell":
                if (!(unlockedSpell.Value == true))
                {
                    __instance.SaveData = new ToolCrestsData.Data
                    {
                        IsUnlocked = false,
                        Slots = __instance.SaveData.Slots,
                        DisplayNewIndicator = false
                    };
                }
                else
                {
                    unlockedSpell.Value = true;
                    equippedCrest.Value = "Spell";
                }
                break;
        }
    }
    private static void NeedolinChanged(object sender, EventArgs args)
    {
        PlayerData.instance.hasNeedolin = needolin.Value;
    }
    private static void DeepElegyChanged(object sender, EventArgs args)
    {
        PlayerData.instance.hasNeedolinMemoryPowerup = deepElegy.Value;
    }
    private static void DashChanged(object sender, EventArgs args)
    {
        PlayerData.instance.hasDash = dash.Value;
    }
    private static void DrifterChanged(object sender, EventArgs args)
    {
        PlayerData.instance.hasBrolly = drifter.Value;
    }
    private static void WalljumpChanged(object sender, EventArgs args)
    {
        PlayerData.instance.hasWalljump = walljump.Value;
    }
    private static void DoubleJumpChanged(object sender, EventArgs args)
    {
        PlayerData.instance.hasDoubleJump = doubleJump.Value;
    }
    private static void NeedleArtChanged(object sender, EventArgs args)
    {
        PlayerData.instance.hasChargeSlash = needleArt.Value;
    }
    private static void SoarChanged(object sender, EventArgs args)
    {
        PlayerData.instance.hasSuperJump = soar.Value;
    }
    private static void ClawlineChanged(object sender, EventArgs args)
    {
        PlayerData.instance.hasHarpoonDash = clawline.Value;
    }
    private static void RosariesChanged(object sender, EventArgs args)
    {
        PlayerData.instance.geo = rosaries.Value;
    }
    private static void ShardsChanged(object sender, EventArgs args)
    {
        PlayerData.instance.ShellShards = shards.Value;
    }
    private static void ChangeAnywhereChanged(object sender, EventArgs args)
    {
        CheatManager.CanChangeEquipsAnywhere = canChangeEquipsAnywhere.Value;
    }
    private static void InfiniteSilkChanged(object sender, EventArgs args)
    {
        if (infiniteSilk.Value)
        {
            PlayerData.instance.silk = PlayerData.instance.silkMax;
        }
    }

    private static void InvincibleChanged(object sender, EventArgs args)
    {
        PlayerData.instance.isInvincible = invincible.Value;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(HeroController), "TakeSilk", new System.Type[] {typeof(int), typeof(SilkSpool.SilkTakeSource)})]
    private static void TakeSilkPrefix(HeroController __instance, ref int amount, SilkSpool.SilkTakeSource source)
    {
        if (infiniteSilk.Value)
        {
            amount = 0;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerData), "TakeHealth")]
    private static void TakeHealthPrefix(PlayerData __instance, ref int amount, bool hasBlueHealth, bool allowFracturedMaskBreak)
    {
        if (infiniteHealth.Value)
        {
            amount = 0;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(HeroController), "TakeDamage")]
    private static bool TakeDamagePrefix(HeroController __instance, GameObject go, CollisionSide damageSide, int damageAmount, HazardType hazardType, DamagePropertyFlags damagePropertyFlags = DamagePropertyFlags.None)
    {
        return !invincible.Value;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ToolItemManager), "SetEquippedCrest")]
    private static void SetEquippedCrestPostfix(ToolItemManager __instance, string crestId){
        equippedCrest.Value = crestId;
    }

    void Update()
    {
        PlayerData pd = PlayerData.instance;
        needolin.Value = pd.hasNeedolin;
        deepElegy.Value = pd.hasNeedolinMemoryPowerup;
        dash.Value = pd.hasDash;
        drifter.Value = pd.hasBrolly;
        walljump.Value = pd.hasWalljump;
        doubleJump.Value = pd.hasDoubleJump;
        needleArt.Value = pd.hasChargeSlash;
        soar.Value = pd.hasSuperJump;
        clawline.Value = pd.hasHarpoonDash;
        rosaries.Value = pd.geo;
        shards.Value = pd.ShellShards;
    }
}