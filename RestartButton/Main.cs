using ABI_RC.Core;
using ABI_RC.Core.Player;
using MelonLoader;
using RestartButton;
using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using ButtonAPI = ChilloutButtonAPI.ChilloutButtonAPIMain;
using Main = RestartButton.Main;

[assembly: MelonInfo(typeof(Main), Guh.Name, Guh.Version, Guh.Author, Guh.DownloadLink)]
[assembly: MelonGame("Alpha Blend Interactive", "ChilloutVR")]

namespace RestartButton;

public static class Guh {
    public const string Name = "RestartButton";
    public const string Author = "Animal & Bluscream";
    public const string Version = "1.1.0";
    public const string DownloadLink = "https://github.com/Aniiiiiimal/CVRMods";
}

public class Main : MelonMod {
    public const string bat_template = @"
taskkill /f /im {0}
timeout /t {1}
start """" {2}
";
    public MelonPreferences_Entry keybind;
    public MelonPreferences_Entry vr_failsave;
    public MelonPreferences_Entry KeepRunningSetting;

    public override void OnApplicationStart() {
        MelonPreferences_Category cat = MelonPreferences.CreateCategory(Guh.Name);
        keybind = cat.CreateEntry<KeyCode>("restart_bind", KeyCode.End, "Restart Key Bind", "Key to press to restart game");
        vr_failsave = cat.CreateEntry<bool>("vr_failsave", false, "VR Failsave", "Failsave option to detect VR even if the command line arguments don't provide it");
        KeepRunningSetting = cat.CreateEntry<bool>("KeepRunning", false, "Keep CVR Running", "Always restart CVR when it's being shut down");
        ButtonAPI.OnInit += () => {
            ChilloutButtonAPI.UI.SubMenu menu = ButtonAPI.MainPage.AddSubMenu("Restart Game");
            menu.AddButton("Restart", "Restart ChilloutVR", () => {
                RestartGame();
            });
        };
    }

    public void RestartGame() {
        LoggerInstance.Warning("Restarting!");
        MelonPreferences.Save();
        string filename = Process.GetCurrentProcess().ProcessName + ".exe";
        string args = string.Join(" ", Environment.GetCommandLineArgs());
        MelonLogger.Warning("Game start args: " + args);
        if ((bool)vr_failsave.BoxedValue && PlayerSetup.Instance._inVr) args += " -vr";
        File.WriteAllText("restart.bat", string.Format(bat_template, filename, 3, args.Replace("%", "%%")));
        Process.Start(new ProcessStartInfo() { FileName = (File.Exists("reconnect.bat") ? "reconnect.bat" : "restart.bat"), CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden });
        RootLogic.Instance.QuitApplication();
        Environment.Exit(0);
    }

    public override void OnUpdate() {
        if (!Input.GetKeyDown((KeyCode)keybind.BoxedValue)) {
            return;
        }
        try {
            RestartGame();
        } catch (Exception e) {
            LoggerInstance.Error($"Failed to restart: {e}");
        }
    }

    public override void OnApplicationQuit() {
        if ((bool)KeepRunningSetting.BoxedValue) {
            RestartGame();
        }
    }
}