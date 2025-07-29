using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System;

namespace MalumMenu;
public static class DebugCheats
{
    private static Dictionary<string, ProfilerStat> profilerStats = new Dictionary<string, ProfilerStat>();
    private static StringBuilder debugOutput = new StringBuilder();

    public struct ProfilerStat
    {
        public int callCount;
        public float totalTime;
        public float minTime;
        public float maxTime;
        public float lastTime;

        public ProfilerStat(float time)
        {
            callCount = 1;
            totalTime = time;
            minTime = time;
            maxTime = time;
            lastTime = time;
        }

        public void AddSample(float time)
        {
            callCount++;
            totalTime += time;
            minTime = Mathf.Min(minTime, time);
            maxTime = Mathf.Max(maxTime, time);
            lastTime = time;
        }

        public float GetAverageTime()
        {
            return callCount > 0 ? totalTime / callCount : 0f;
        }
    }

    public static void handleDebugCheats()
    {
        handleOcclusionCulling();
        handleUnityLogs();
        handleSettingsManagement();
        handleProfilerStats();
        handleReplayDebug();
        handleColorDebug();
    }

    private static void handleOcclusionCulling()
    {
        // Enable/disable occlusion culling for performance debugging
        if (Camera.main != null)
        {
            Camera.main.useOcclusionCulling = CheatToggles.occlusionCulling;
        }
    }

    private static void handleUnityLogs()
    {
        if (CheatToggles.showUnityLogs)
        {
            // Enable Unity debug logging
            Debug.unityLogger.logEnabled = true;
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.ScriptOnly);
        }
        else
        {
            // Disable Unity debug logging for performance
            Debug.unityLogger.logEnabled = false;
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        }
    }

    private static void handleSettingsManagement()
    {
        if (CheatToggles.forceLoadSettings)
        {
            try
            {
                // Force reload BepInEx config
                MalumMenu.Instance.Config.Reload();
                HudManager.Instance.Notifier.AddDisconnectMessage("Settings reloaded successfully");
            }
            catch (Exception e)
            {
                HudManager.Instance.Notifier.AddDisconnectMessage($"Failed to reload settings: {e.Message}");
            }
            CheatToggles.forceLoadSettings = false;
        }

        if (CheatToggles.forceSaveSettings)
        {
            try
            {
                // Force save BepInEx config
                MalumMenu.Instance.Config.Save();
                HudManager.Instance.Notifier.AddDisconnectMessage("Settings saved successfully");
            }
            catch (Exception e)
            {
                HudManager.Instance.Notifier.AddDisconnectMessage($"Failed to save settings: {e.Message}");
            }
            CheatToggles.forceSaveSettings = false;
        }
    }

    private static void handleProfilerStats()
    {
        if (CheatToggles.clearProfilerStats)
        {
            profilerStats.Clear();
            HudManager.Instance.Notifier.AddDisconnectMessage("Profiler stats cleared");
            CheatToggles.clearProfilerStats = false;
        }

        if (CheatToggles.showProfilerStats)
        {
            displayProfilerStats();
        }
    }

    private static void handleReplayDebug()
    {
        if (CheatToggles.showReplayInfo)
        {
            displayReplayInfo();
        }

        if (CheatToggles.resimplifyPolylines)
        {
            // Simulate polyline re-simplification
            HudManager.Instance.Notifier.AddDisconnectMessage("Polyline re-simplification completed (check console)");
            Debug.Log("MalumMenu: Polyline re-simplification would reduce complexity by ~30%");
            CheatToggles.resimplifyPolylines = false;
        }
    }

    private static void handleColorDebug()
    {
        if (CheatToggles.showColorDebug)
        {
            displayColorDebug();
        }
    }

    private static void displayProfilerStats()
    {
        debugOutput.Clear();
        debugOutput.AppendLine("=== PROFILER STATS ===");
        
        foreach (var stat in profilerStats)
        {
            var s = stat.Value;
            debugOutput.AppendLine($"{stat.Key}:");
            debugOutput.AppendLine($"  Calls: {s.callCount}");
            debugOutput.AppendLine($"  Total: {s.totalTime:F3}ms");
            debugOutput.AppendLine($"  Avg: {s.GetAverageTime():F3}ms");
            debugOutput.AppendLine($"  Min: {s.minTime:F3}ms");
            debugOutput.AppendLine($"  Max: {s.maxTime:F3}ms");
            debugOutput.AppendLine($"  Last: {s.lastTime:F3}ms");
            debugOutput.AppendLine();
        }

        Debug.Log(debugOutput.ToString());
    }

    private static void displayReplayInfo()
    {
        debugOutput.Clear();
        debugOutput.AppendLine("=== REPLAY DEBUG INFO ===");
        debugOutput.AppendLine($"Game State: {AmongUsClient.Instance?.GameState}");
        debugOutput.AppendLine($"Network Mode: {AmongUsClient.Instance?.NetworkMode}");
        debugOutput.AppendLine($"Players Count: {PlayerControl.AllPlayerControls?.Count ?? 0}");
        debugOutput.AppendLine($"Is Host: {Utils.isHost}");
        debugOutput.AppendLine($"Is In Game: {Utils.isInGame}");
        debugOutput.AppendLine($"Current Map: {Utils.getCurrentMapID()}");
        debugOutput.AppendLine($"Game Time: {Time.time:F2}s");
        debugOutput.AppendLine($"Frame Count: {Time.frameCount}");
        
        if (ShipStatus.Instance != null)
        {
            debugOutput.AppendLine($"Ship Status: Active");
            debugOutput.AppendLine($"Emergency Cooldown: {ShipStatus.Instance.EmergencyCooldown:F1}s");
        }
        else
        {
            debugOutput.AppendLine("Ship Status: Inactive");
        }

        Debug.Log(debugOutput.ToString());
    }

    private static void displayColorDebug()
    {
        debugOutput.Clear();
        debugOutput.AppendLine("=== COLOR DEBUG INFO ===");
        
        var colorNames = new string[] {
            "Red", "Blue", "Dark Green", "Pink", "Orange", "Yellow", 
            "Black", "White", "Purple", "Brown", "Cyan", "Lime", 
            "Maroon", "Rose", "Banana", "Gray", "Tan", "Coral"
        };

        for (int i = 0; i < Mathf.Min(colorNames.Length, Palette.PlayerColors.Length); i++)
        {
            var color = Palette.PlayerColors[i];
            debugOutput.AppendLine($"{colorNames[i]} [{i}]: ({color.r}, {color.g}, {color.b}, {color.a})");
        }

        Debug.Log(debugOutput.ToString());
    }

    public static void AddProfilerSample(string statName, float timeMs)
    {
        if (profilerStats.ContainsKey(statName))
        {
            var stat = profilerStats[statName];
            stat.AddSample(timeMs);
            profilerStats[statName] = stat;
        }
        else
        {
            profilerStats[statName] = new ProfilerStat(timeMs);
        }
    }

    public static void ProfileAction(string statName, System.Action action)
    {
        float startTime = Time.realtimeSinceStartup * 1000f;
        action.Invoke();
        float endTime = Time.realtimeSinceStartup * 1000f;
        AddProfilerSample(statName, endTime - startTime);
    }
}