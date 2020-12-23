﻿using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using WireGuardAPI;
using WireGuardServerForWindows.Properties;

namespace WireGuardServerForWindows.Models
{
    public class WireGuardExePrerequisite : PrerequisiteItem
    {
        public WireGuardExePrerequisite() : base
        (
            title: Resources.WireGuardExe,
            successMessage: Resources.WireGuardExeFound,
            errorMessage: Resources.WireGuardExeNotFound,
            resolveText: Resources.InstallWireGuard,
            configureText: Resources.UninstallWireGuard
        )
        {
        }

        public override bool Fulfilled
        {
            get
            {
                _wireGuardExe ??= new WireGuardExe();
                return _wireGuardExe.Exists;
            }
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

            Task.Run(WaitForFulfilled);

            Mouse.OverrideCursor = null;
        }

        public override void Configure()
        {
            Mouse.OverrideCursor = Cursors.Wait;

            _wireGuardExe.ExecuteCommand(new UninstallCommand());
            Refresh();

            Mouse.OverrideCursor = null;
        }

        private readonly string wireGuardExeDownload = @"https://download.wireguard.com/windows-client/wireguard-installer.exe";
        private WireGuardExe _wireGuardExe;
    }
}
