using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using System.Reflection;
using Pigeon.Movement;
using UnityEngine;

public static class CheatsMenu
{
    public static void CreateCheatsMenu(MenuMod2Menu parentMenu)
    {
        MenuMod2Menu cheatsMenu = new MenuMod2Menu("CHEATS", parentMenu);

        {
            MM2Button button = null;
            button = cheatsMenu.addButton("Godmode", () => toggleGod(button)).changeColour(Color.red);
        }
        {
            MM2Button button = null;
            button = cheatsMenu.addButton("Super sprint", () => toggleSprintFast(button))
                .changeColour(Color.red);
        }
        {
            MM2Button button = null;
            button = cheatsMenu.addButton("Super jump", () => toggleSuperJump(button))
                .changeColour(Color.red);
        }
        {
            MM2Button button = null;
            button = cheatsMenu.addButton("Toggle enemy spawning", () => toggleSpawning(button))
                .changeColour(Color.green);
        }
        cheatsMenu.addButton("Kill all enemies", () => killAllEnemies());
        cheatsMenu.addButton("Spawn swarm", () => SpawnMenu.spawnSwarm(10));
        cheatsMenu.addButton("Clean up parts", () => cleanUpParts());
        cheatsMenu.addButton("Clean up collectables", () => cleanUpCollectables());
        cheatsMenu.addButton("Clear lost loot upgrades", () => clearLostLootUpgrades());
        cheatsMenu.addButton("Give max resources", () => giveAllResoruces());
    }

    public static void toggleGod(MM2Button button = null)
    {
        try
        {
            if (SparrohPlugin.god)
            {
                Player.LocalPlayer.SetMaxHealth(37.5f);
                MethodInfo setHealthClient = typeof(Player).GetMethod("SetHealth_Client", BindingFlags.NonPublic | BindingFlags.Instance);
                setHealthClient?.Invoke(Player.LocalPlayer, new object[] { 37.5f });
                MethodInfo setHealthOwner = typeof(Player).GetMethod("SetHealth_Owner", BindingFlags.NonPublic | BindingFlags.Instance);
                setHealthOwner?.Invoke(Player.LocalPlayer, new object[] { 37.5f });
                SparrohPlugin.god = false;
            }
            else
            {
                Player.LocalPlayer.SetMaxHealth(999999f);
                MethodInfo setHealthClient = typeof(Player).GetMethod("SetHealth_Client", BindingFlags.NonPublic | BindingFlags.Instance);
                setHealthClient?.Invoke(Player.LocalPlayer, new object[] { 999999f });
                MethodInfo setHealthOwner = typeof(Player).GetMethod("SetHealth_Owner", BindingFlags.NonPublic | BindingFlags.Instance);
                setHealthOwner?.Invoke(Player.LocalPlayer, new object[] { 999999f });
                SparrohPlugin.god = true;
            }
            if (button != null)
            {
                if (SparrohPlugin.god)
                {
                    button.changeColour(Color.green);
                }
                else
                {
                    button.changeColour(Color.red);
                }
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger.LogError($"Exception in toggleGod: {ex.Message}");
        }
    }

    public static void toggleSprintFast(MM2Button b = null)
    {
        try
        {
            if (SparrohPlugin.sprintFast)
            {
                Player.LocalPlayer.DefaultMoveSpeed = 10;
                SparrohPlugin.sprintFast = false;
            }
            else
            {
                Player.LocalPlayer.DefaultMoveSpeed = 100;
                SparrohPlugin.sprintFast = true;
            }
            if (b != null)
            {
                if (SparrohPlugin.sprintFast)
                {
                    b.changeColour(Color.green);
                }
                else
                {
                    b.changeColour(Color.red);
                }
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger.LogError($"Exception in toggleSprintFast: {ex.Message}");
        }
    }

    public static void toggleSuperJump(MM2Button b = null)
    {
        try
        {
            if (SparrohPlugin.superJump)
            {
                FieldInfo field = typeof(Player).GetField("jumpSpeed", BindingFlags.NonPublic | BindingFlags.Instance);
                field.SetValue(Player.LocalPlayer, 14f);
                SparrohPlugin.superJump = false;
            }
            else
            {
                FieldInfo field = typeof(Player).GetField("jumpSpeed", BindingFlags.NonPublic | BindingFlags.Instance);
                field.SetValue(Player.LocalPlayer, 100f);
                SparrohPlugin.superJump = true;
            }
            if (b != null)
            {
                if (SparrohPlugin.superJump)
                {
                    b.changeColour(Color.green);
                }
                else
                {
                    b.changeColour(Color.red);
                }
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger.LogError($"Exception in toggleSuperJump: {ex.Message}");
        }
    }

    public static void setGod(bool enabled, MM2Button b = null)
    {
        if (enabled)
        {
            Player.LocalPlayer.SetMaxHealth(999999f);
            MethodInfo setHealthClient = typeof(Player).GetMethod("SetHealth_Client", BindingFlags.NonPublic | BindingFlags.Instance);
            setHealthClient?.Invoke(Player.LocalPlayer, new object[] { 999999f });
            MethodInfo setHealthOwner = typeof(Player).GetMethod("SetHealth_Owner", BindingFlags.NonPublic | BindingFlags.Instance);
            setHealthOwner?.Invoke(Player.LocalPlayer, new object[] { 999999f });
        }
        else
        {
            Player.LocalPlayer.SetMaxHealth(37.5f);
            MethodInfo setHealthClient = typeof(Player).GetMethod("SetHealth_Client", BindingFlags.NonPublic | BindingFlags.Instance);
            setHealthClient?.Invoke(Player.LocalPlayer, new object[] { 37.5f });
            MethodInfo setHealthOwner = typeof(Player).GetMethod("SetHealth_Owner", BindingFlags.NonPublic | BindingFlags.Instance);
            setHealthOwner?.Invoke(Player.LocalPlayer, new object[] { 37.5f });
        }
    }

    public static void setSprintFast(bool enabled, MM2Button b = null)
    {
        if (enabled)
        {
            Player.LocalPlayer.DefaultMoveSpeed = 100;
        }
        else
        {
            Player.LocalPlayer.DefaultMoveSpeed = 10;
        }
    }

    public static void setSuperJump(bool enabled, MM2Button b = null)
    {
        if (enabled)
        {
            FieldInfo field = typeof(Player).GetField("jumpSpeed", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(Player.LocalPlayer, 100f);
        }
        else
        {
            FieldInfo field = typeof(Player).GetField("jumpSpeed", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(Player.LocalPlayer, 14f);
        }
    }

    public static void enemySpawning(bool enabled, MM2Button b = null)
    {
        var em = SparrohPlugin.GetEnemyManager();
        if (em != null)
        {
            if (enabled)
                em.EnableSpawning();
            else
                em.DisableSpawning();
        }
        else
        {
        }
    }

    public static MM2Button toggleSpawning(MM2Button b = null)
    {
        try
        {
            var em = SparrohPlugin.GetEnemyManager();
            if (em != null)
            {
                FieldInfo field = typeof(EnemyManager).GetField("enableAmbientWave", BindingFlags.NonPublic | BindingFlags.Instance);
                if ((bool)field.GetValue(em))
                {
                    em.DisableSpawning();
                    if (b != null)
                    {
                        b.changeColour(Color.red);
                    }
                }
                else
                {
                    em.EnableSpawning();
                    if (b != null)
                    {
                        b.changeColour(Color.green);
                    }
                }
            }
            else
            {
            }
            return b;
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger.LogError($"Exception in toggleSpawning: {ex.Message}");
            return b;
        }
    }

    public static void killAllEnemies(MM2Button b = null)
    {
        try
        {
            var em = SparrohPlugin.GetEnemyManager();
            if (em != null)
                em.KillAllEnemies_Server();
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger.LogError($"Exception in killAllEnemies: {ex.Message}");
        }
    }

    public static void cleanUpParts(MM2Button b = null)
    {
        try
        {
            List<EnemyPart> enemyParts = GameObject.FindObjectsOfType<EnemyPart>().ToList();
            foreach (var part in enemyParts)
            {
                part.Kill(DamageFlags.Despawn);
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger.LogError($"Exception in cleanUpParts: {ex.Message}");
        }
    }

    public static void cleanUpCollectables(MM2Button b = null)
    {
        try
        {
            List<ClientCollectable> collectables = GameObject.FindObjectsOfType<ClientCollectable>().ToList();
            foreach (var part in collectables)
            {
                part.DespawnTrackedObject();
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger.LogError($"Exception in cleanUpCollectables: {ex.Message}");
        }
    }

    public static void clearLostLootUpgrades(MM2Button b = null)
    {
        try
        {
            PlayerData.Instance.rentedUpgrades.Clear();
            SparrohPlugin.Logger.LogInfo("Cleared all rented upgrades from lost loot machine");
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger.LogError($"Exception in clearLostLootUpgrades: {ex.Message}");
        }
    }

    public static void giveAllResoruces(MM2Button b = null)
    {
        try
        {
            var allResources = Global.Instance.PlayerResources;
            foreach (var resource in allResources)
            {
                PlayerData.Instance.AddResource(resource, resource.Max);
            }
        }
        catch (Exception ex)
        {
            SparrohPlugin.Logger.LogError($"Exception in giveAllResoruces: {ex.Message}");
        }
    }
}
