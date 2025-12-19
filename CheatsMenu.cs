using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using System.Reflection;
using Pigeon.Movement;
using UnityEngine;

public static class CheatsMenu
{
    private static ManualLogSource Logger = Plugin.Logger;

    public static void CreateCheatsMenu(MenuMod2Menu parentMenu)
    {
        Logger.LogInfo("Creating CHEATS menu");

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
        cheatsMenu.addButton("Give max resources", () => giveAllResoruces());
    }

    public static void toggleGod(MM2Button button = null)
    {
        if (Plugin.god)
        {
            Player.LocalPlayer.SetMaxHealth(37.5f);
            MethodInfo setHealthClient = typeof(Player).GetMethod("SetHealth_Client", BindingFlags.NonPublic | BindingFlags.Instance);
            setHealthClient?.Invoke(Player.LocalPlayer, new object[] { 37.5f });
            MethodInfo setHealthOwner = typeof(Player).GetMethod("SetHealth_Owner", BindingFlags.NonPublic | BindingFlags.Instance);
            setHealthOwner?.Invoke(Player.LocalPlayer, new object[] { 37.5f });
            Plugin.god = false;
        }
        else
        {
            Player.LocalPlayer.SetMaxHealth(999999f);
            MethodInfo setHealthClient = typeof(Player).GetMethod("SetHealth_Client", BindingFlags.NonPublic | BindingFlags.Instance);
            setHealthClient?.Invoke(Player.LocalPlayer, new object[] { 999999f });
            MethodInfo setHealthOwner = typeof(Player).GetMethod("SetHealth_Owner", BindingFlags.NonPublic | BindingFlags.Instance);
            setHealthOwner?.Invoke(Player.LocalPlayer, new object[] { 999999f });
            Plugin.god = true;
        }
        if (button != null)
        {
            if (Plugin.god)
            {
                button.changeColour(Color.green);
            }
            else
            {
                button.changeColour(Color.red);
            }
        }
    }

    public static void toggleSprintFast(MM2Button b = null)
    {
        if (Plugin.sprintFast)
        {
            Player.LocalPlayer.DefaultMoveSpeed = 10;
            Plugin.sprintFast = false;
        }
        else
        {
            Player.LocalPlayer.DefaultMoveSpeed = 100;
            Plugin.sprintFast = true;
        }
        if (b != null)
        {
            if (Plugin.sprintFast)
            {
                b.changeColour(Color.green);
            }
            else
            {
                b.changeColour(Color.red);
            }
        }
    }

    public static void toggleSuperJump(MM2Button b = null)
    {
        if (Plugin.superJump)
        {
            FieldInfo field = typeof(Player).GetField("jumpSpeed", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(Player.LocalPlayer, 14f);
            Plugin.superJump = false;
        }
        else
        {
            FieldInfo field = typeof(Player).GetField("jumpSpeed", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(Player.LocalPlayer, 100f);
            Plugin.superJump = true;
        }
        if (b != null)
        {
            if (Plugin.superJump)
            {
                b.changeColour(Color.green);
            }
            else
            {
                b.changeColour(Color.red);
            }
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

    public static void setAirJump(bool enabled, MM2Button b = null) // TODO BROKEN
    {
        if (enabled)
        {
            FieldInfo airJumps = typeof(Player).GetField("airJumps", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo airJumpSpeed = typeof(Player).GetField("airJumpSpeed", BindingFlags.NonPublic | BindingFlags.Instance);
            object airJumpsValue = airJumps.GetValue(Player.LocalPlayer);
            if (Convert.ToInt32(airJumpsValue) != 100)
                Plugin.previousAirJumps = Convert.ToInt32(airJumpsValue);
            object airJumpSpeedValue = airJumpSpeed.GetValue(Player.LocalPlayer);
            float currentSpeed = Convert.ToSingle(airJumpSpeedValue);
            if (currentSpeed != 100f)
                Plugin.previousAirJumpSpeed = currentSpeed;
            airJumps.SetValue(Player.LocalPlayer, 100);

            FieldInfo jumpSpeed = typeof(Player).GetField("jumpSpeed", BindingFlags.NonPublic | BindingFlags.Instance);
            airJumpSpeed.SetValue(Player.LocalPlayer, jumpSpeed.GetValue(Player.LocalPlayer));
        }
        else
        {
            FieldInfo field = typeof(Player).GetField("airJumps", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(Player.LocalPlayer, Plugin.previousAirJumps);
            FieldInfo airJumpSpeedField = typeof(Player).GetField("airJumpSpeed", BindingFlags.NonPublic | BindingFlags.Instance);
            airJumpSpeedField.SetValue(Player.LocalPlayer, Plugin.previousAirJumpSpeed);
        }
    }

    public static void enemySpawning(bool enabled, MM2Button b = null)
    {
        var em = Plugin.GetEnemyManager();
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
        var em = Plugin.GetEnemyManager();
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

    public static void killAllEnemies(MM2Button b = null)
    {
        var em = Plugin.GetEnemyManager();
        if (em != null)
            em.KillAllEnemies_Server();
    }

    public static void cleanUpParts(MM2Button b = null)
    {
        List<EnemyPart> enemyParts = GameObject.FindObjectsOfType<EnemyPart>().ToList();
        foreach (var part in enemyParts)
        {
            part.Kill(DamageFlags.Despawn);
        }
    }

    public static void cleanUpCollectables(MM2Button b = null)
    {
        List<ClientCollectable> collectables = GameObject.FindObjectsOfType<ClientCollectable>().ToList();
        foreach (var part in collectables)
        {
            part.DespawnTrackedObject();
        }
    }

    public static void giveAllResoruces(MM2Button b = null)
    {
        var allResources = Global.Instance.PlayerResources;
        foreach (var resource in allResources)
        {
            PlayerData.Instance.AddResource(resource, resource.Max);
        }
    }
}
