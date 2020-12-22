﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WireGuardAPI;
using WireGuardServerForWindows.Models;

namespace WireGuardServerForWindows
{
    public class WireGuardExePrerequisite : PrerequisiteItem
    {
        public WireGuardExePrerequisite()
        {
            Title = "WireGuard.exe";
            SuccessMessage = "Found WireGuard.exe in PATH.";
            ErrorMessage = "WireGuard.exe is not found in the PATH. It must be downloaded and installed.";
            ResolveText = "Download and install WireGuard";
            ConfigureText = "Uninstall WireGuard";

            _wireGuardExe = new WireGuardExe();
            Fulfilled = _wireGuardExe.Exists;
        }

        public override void Resolve()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            
            string downloadPath = Path.Combine(Path.GetTempPath(), "wireguard.exe");
            new WebClient().DownloadFile(wireGuardExeDownload, downloadPath);
            Process.Start(new ProcessStartInfo
            {
                FileName = downloadPath,
                Verb = "runas", // For elevation
                UseShellExecute = true // Must be true to use "runas"
            });

            Task.Run(WaitForWireGuardProcess);

            Mouse.OverrideCursor = null;
        }

        public override void Configure()
        {
            Mouse.OverrideCursor = Cursors.Wait;

            _wireGuardExe.ExecuteCommand(new UninstallCommand());
            Refresh();

            Mouse.OverrideCursor = null;
        }

        private async void WaitForWireGuardProcess()
        {
            while (!Fulfilled)
            {
                Refresh();
                await Task.Delay((int)TimeSpan.FromSeconds(1).TotalMilliseconds);
            }
        }

        public override void Refresh()
        {
            Fulfilled = _wireGuardExe.Exists;
        }

        private readonly string wireGuardExeDownload = @"https://download.wireguard.com/windows-client/wireguard-installer.exe";
        private readonly WireGuardExe _wireGuardExe;
    }
}
