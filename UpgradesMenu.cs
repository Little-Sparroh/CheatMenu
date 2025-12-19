using BepInEx.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Pigeon.Movement;
using UnityEngine;

public static class UpgradesMenu
{
    private static ManualLogSource Logger = Plugin.Logger;

    public static void CreateUpgradesMenu(MenuMod2Menu parentMenu)
    {
        Logger.LogInfo("About to create gear and character menus");

        const string debugPattern =
            @"(_test_|_dev_|_wip|debug|temp|placeholder|todo|_old|_backup|_copy|\.skinasset$|^test_|roachard)";
        MenuMod2Menu upgradesMenu = new MenuMod2Menu("UPGRADES", parentMenu);

        var gearByType = Global.Instance.AllGear
            .Where(g => g.Info.Upgrades.Count > 0)
            .GroupBy(g => g.GearType)
            .OrderBy(g => g.Key.ToString());

        foreach (var gearGroup in gearByType)
        {
            var typeName = gearGroup.Key.ToString();
            var gearTypeMenu = new MenuMod2Menu(typeName, upgradesMenu);

            foreach (var gear in gearGroup.OrderBy(g => g.Info.Name))
            {
                var gearInfo = gear.Info;
                var individualGearMenu = new MenuMod2Menu(gearInfo.Name, gearTypeMenu);

                var allGearUpgrades = gearInfo.Upgrades
                .Where(u => !Regex.IsMatch(u.Name, debugPattern, RegexOptions.IgnoreCase) || Plugin.sparrohMode)
                    .OrderByDescending(u => u.Rarity)
                    .ThenBy(u => Plugin.CleanRichText(u.Name))
                    .ToList();

                foreach (var upgrade in allGearUpgrades)
                {
                    if (upgrade.UpgradeType == Upgrade.Type.Cosmetic)
                    {
                        var button = individualGearMenu.addButton(Plugin.CleanRichText(upgrade.Name),
                            () => giveCosmetic(upgrade, gear));
                        button.changeColour(upgrade.Color);
                    }
                    else
                    {
                        var button = individualGearMenu.addButton(Plugin.CleanRichText(upgrade.Name),
                            () => giveUpgrade(upgrade, gear));
                        button.changeColour(upgrade.Color);
                    }
                }
            }
        }

        MenuMod2Menu charactersMenu = new MenuMod2Menu("Characters", upgradesMenu);
        foreach (var character in Global.Instance.Characters)
        {
            var gearInfo = character.Info;
            var individualCharMenu = new MenuMod2Menu(gearInfo.Name, charactersMenu);

            var allCharacterUpgrades = character.Info.Upgrades
                .Where(u => !Regex.IsMatch(u.Name, debugPattern, RegexOptions.IgnoreCase) || Plugin.sparrohMode)
                .OrderByDescending(u => u.Rarity)
                .ThenBy(u => Plugin.CleanRichText(u.Name))
                .ToList();

            foreach (var upgrade in allCharacterUpgrades)
            {
                if (upgrade.UpgradeType == Upgrade.Type.Cosmetic)
                {
                    var button = individualCharMenu.addButton(Plugin.CleanRichText(upgrade.Name),
                        () => giveCosmetic(upgrade, character));
                    button.changeColour(upgrade.Color);
                }
                else
                {
                    var button = individualCharMenu.addButton(Plugin.CleanRichText(upgrade.Name),
                        () => giveCharacterUpgrade(upgrade, character));
                    button.changeColour(upgrade.Color);
                }
            }
        }

        MenuMod2Menu specificGeneric = new MenuMod2Menu("Universal", charactersMenu);

        try
        {
            var allUpgrades = PlayerData.GetAllUpgrades(Global.Instance);
            GenericPlayerUpgrade[] allGenericUpgrades = allUpgrades.Where(u => u.Upgrade is GenericPlayerUpgrade).Select(u => u.Upgrade as GenericPlayerUpgrade).ToArray();

            HashSet<Upgrade> skillTreeUpgrades = new HashSet<Upgrade>();
            foreach (var character in Global.Instance.Characters)
            {
                var skillTree = character.SkillTree;
                SkillTreeUpgradeUI[] treeUpgradesUI = skillTree.GetComponentsInChildren<SkillTreeUpgradeUI>();
                skillTreeUpgrades.UnionWith(treeUpgradesUI.Select(ui => ui.Upgrade));
            }

            HashSet<Upgrade> characterSpecificUpgrades = new HashSet<Upgrade>();
            foreach (var character in Global.Instance.Characters)
            {
                characterSpecificUpgrades.UnionWith(character.Info.Upgrades);
            }

            var genericUpgrades = allGenericUpgrades
                .Where(u => u.UpgradeType != Upgrade.Type.Cosmetic &&
                            (!Regex.IsMatch(u.Name, debugPattern, RegexOptions.IgnoreCase) || Plugin.sparrohMode) &&
                            !characterSpecificUpgrades.Contains(u))
                .ToList();


            foreach (var u in genericUpgrades)
            {
            }

            Dictionary<string, Upgrade> uniqueGenerics = new Dictionary<string, Upgrade>();
            foreach (var upgrade in genericUpgrades)
            {
                if (!uniqueGenerics.ContainsKey(upgrade.APIName))
                {
                    uniqueGenerics[upgrade.APIName] = upgrade;
                }
            }

            if (uniqueGenerics.Any())
            {
                foreach (var kvp in uniqueGenerics.OrderByDescending(kvp => kvp.Value.Rarity).ThenBy(kvp => kvp.Key))
                {
                    var upgrade = kvp.Value;

                    bool isOwned = false;

                    var button = specificGeneric.addButton(Plugin.CleanRichText(upgrade.Name),
                        () => giveUniversalUpgrade(upgrade));
                    button.changeColour(isOwned ? Color.green : upgrade.Color);
                }
            }
            else
            {
                specificGeneric.addButton("No generics found", () => { });
            }
        }
        catch (System.Exception ex)
        {
            Logger.LogError($"Exception in universal upgrade menu creation: {ex.Message}\n{ex.StackTrace}");
        }
    }

    public static void giveAllUpgrades(MM2Button b = null)
    {
        foreach (var gear in Global.Instance.AllGear)
        {
            var gearInfo = gear.Info;
            foreach (var upgrade in gearInfo.Upgrades)
            {
                if (upgrade.UpgradeType != Upgrade.Type.Invalid && upgrade.UpgradeType != Upgrade.Type.Cosmetic)
                {
                    var iUpgrade = new UpgradeInstance(upgrade, gear);
                    PlayerData.CollectInstance(iUpgrade, PlayerData.UnlockFlags.Hidden);
                    iUpgrade.Unlock(true);
                }
            }
        }
        Plugin.SendTextChatMessageToClient("All upgrades for weapons are added silently.");
    }

    public static void giveAllCosmetics(MM2Button b = null)
    {
        const string debugPattern = @"(_test_|_dev_|_wip|debug|temp|placeholder|todo|_old|_backup|_copy|\.skinasset$|^test_)";
        foreach (var gear in Global.Instance.AllGear)
        {
            var gearInfo = gear.Info;
            foreach (var upgrade in gearInfo.Upgrades)
            {
                if (upgrade.UpgradeType != Upgrade.Type.Cosmetic ||
                    Regex.IsMatch(upgrade.Name, debugPattern, RegexOptions.IgnoreCase))
                    continue;
                var iUpgrade = new UpgradeInstance(upgrade, gear);
                PlayerData.CollectInstance(iUpgrade, PlayerData.UnlockFlags.Hidden);
                iUpgrade.Unlock(true);
            }
        }

        foreach (var gear in Global.Instance.Characters)
        {
            var gearInfo = gear.Info;
            foreach (var upgrade in gearInfo.Upgrades)
            {
                if (upgrade.UpgradeType != Upgrade.Type.Cosmetic ||
                    Regex.IsMatch(upgrade.Name, debugPattern, RegexOptions.IgnoreCase))
                    continue;

                var iUpgrade = new UpgradeInstance(upgrade, gear);
                PlayerData.CollectInstance(iUpgrade, PlayerData.UnlockFlags.Hidden);
                iUpgrade.Unlock(true);
            }
        }

        if (Global.Instance.DropPod != null)
        {
            var dropPodUpgradable = (IUpgradable)Global.Instance.DropPod;
            var gearInfo = dropPodUpgradable.Info;
            if (gearInfo != null)
            {
                foreach (var upgrade in gearInfo.Upgrades)
                {
                    if (upgrade.UpgradeType == Upgrade.Type.Cosmetic &&
                        !Regex.IsMatch(upgrade.Name, debugPattern, RegexOptions.IgnoreCase))
                    {
                        var iUpgrade = new UpgradeInstance(upgrade, dropPodUpgradable);
                        PlayerData.CollectInstance(iUpgrade, PlayerData.UnlockFlags.Hidden);
                        iUpgrade.Unlock(true);
                    }
                }
            }
        }

        Plugin.SendTextChatMessageToClient("All cosmetics for characters, weapons, and drop pod are added silently.");
    }

    public static void giveCosmetic(Upgrade upgrade, IUpgradable gear, MM2Button b = null)
    {
        var iUpgrade = new UpgradeInstance(upgrade, gear);
        PlayerData.CollectInstance(iUpgrade, PlayerData.UnlockFlags.Hidden);
        iUpgrade.Unlock(true);
    }

    public static void giveUpgrade(Upgrade upgrade, IUpgradable gear, MM2Button b = null)
    {
        var iUpgrade = new UpgradeInstance(upgrade, gear);
        PlayerData.CollectInstance(iUpgrade);
        iUpgrade.Unlock(true);
    }

    public static void giveCharacterUpgrade(Upgrade upgrade, IUpgradable character, MM2Button b = null)
    {
        UpgradeInstance upgradeInstance = PlayerData.CollectInstance(character, upgrade, PlayerData.UnlockFlags.Hidden);
        upgradeInstance.Seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        upgradeInstance.Unlock(false);
        PlayerData.Instance.TotalSkillPointsSpent += 1;
        if (character is Character charObj && charObj.SkillTree != null)
        {
            charObj.SkillTree.Refresh();
        }
    }

    public static void giveUniversalUpgrade(Upgrade upgrade, MM2Button b = null)
    {
        IUpgradable gear = Global.Instance;
        UpgradeInstance upgradeInstance = new UpgradeInstance(upgrade, gear);
        PlayerData.CollectInstance(upgradeInstance, PlayerData.UnlockFlags.Hidden);
        upgradeInstance.Seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        upgradeInstance.Unlock(true);
        ((PlayerUpgrade)upgrade).Apply(Player.LocalPlayer, upgradeInstance);
        Plugin.SendTextChatMessageToClient("Universal upgrade added silently.");
    }
}