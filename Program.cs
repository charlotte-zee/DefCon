using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using Microsoft.Win32;

class Defcon
{
    static void Main()
    {
        if (!IsAdministrator())
        {
            Console.WriteLine("This application needs to run as administrator.");
            Console.WriteLine("Please run this program as Administrator.");
            RestartAsAdmin();
            return;
        }

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Red;
        string logo = @"
██████╗ ███████╗███████╗ ██████╗ ██████╗ ███╗   ██╗
██╔══██╗██╔════╝██╔════╝██╔════╝██╔═══██╗████╗  ██║
██║  ██║█████╗  █████╗  ██║     ██║   ██║██╔██╗ ██║
██║  ██║██╔══╝  ██╔══╝  ██║     ██║   ██║██║╚██╗██║
██████╔╝███████╗██║     ╚██████╗╚██████╔╝██║ ╚████║
╚═════╝ ╚══════╝╚═╝      ╚═════╝ ╚═════╝ ╚═╝  ╚═══╝

    By ZX_Coder
    github: https://github.com/charlotte-zee?tab=repositories                
        ";

        int windowWidth = Console.WindowWidth;
        string[] logoLines = logo.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in logoLines)
        {
            int spaces = (windowWidth - line.Length) / 2;
            Console.WriteLine(new string(' ', spaces) + line);
        }

        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("\n Options: \n");
        Console.WriteLine("  1. Disable Windows Defender policy. \n");
        Console.WriteLine("  2. Revert Windows Defender policy.");
        Console.Write("\n Enter your choice: ");

        string userInput = Console.ReadLine();

        if (userInput == "1")
        {
            Console.Clear();
            ShowFakeLoadingEffect();
            ModifyDefenderPolicy();
        }
        else if (userInput == "2")
        {
            Console.Clear();
            ShowFakeLoadingEffect();
            RevertDefenderPolicy();
        }
        else
        {
            Console.WriteLine("Invalid input, exiting...");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static bool IsAdministrator()
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    static void RestartAsAdmin()
    {
        try
        {
            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            ProcessStartInfo startInfo = new ProcessStartInfo(exePath)
            {
                Verb = "runas",
                UseShellExecute = true,
                CreateNoWindow = true
            };
            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to restart as administrator: {ex.Message}");
        }
    }

    static void ModifyDefenderPolicy()
    {
        try
        {
            string registryKeyPath = @"SOFTWARE\Policies\Microsoft\Windows Defender";
            using (RegistryKey key = Registry.LocalMachine.CreateSubKey(registryKeyPath))
            {
                if (key != null)
                {
                    key.SetValue("DisableAntiSpyware", 1, RegistryValueKind.DWord);
                    key.SetValue("DisableRealtimeMonitoring", 1, RegistryValueKind.DWord);
                    key.SetValue("DisableAntiVirus", 1, RegistryValueKind.DWord);
                    key.SetValue("DisableSpecialRunningModes", 1, RegistryValueKind.DWord);
                    key.SetValue("DisableRoutinelyTakingAction", 1, RegistryValueKind.DWord);
                    key.SetValue("ServiceKeepAlive", 0, RegistryValueKind.DWord);

                    string realTimeProtectionPath = registryKeyPath + @"\Real-Time Protection";
                    using (RegistryKey realTimeKey = Registry.LocalMachine.CreateSubKey(realTimeProtectionPath))
                    {
                        if (realTimeKey != null)
                        {
                            realTimeKey.SetValue("DisableBehaviorMonitoring", 1, RegistryValueKind.DWord);
                            realTimeKey.SetValue("DisableOnAccessProtection", 1, RegistryValueKind.DWord);
                            realTimeKey.SetValue("DisableRealtimeMonitoring", 1, RegistryValueKind.DWord);
                            realTimeKey.SetValue("DisableScanOnRealtimeEnable", 1, RegistryValueKind.DWord);
                        }
                    }

                    string signatureUpdatesPath = registryKeyPath + @"\Signature Updates";
                    using (RegistryKey signatureKey = Registry.LocalMachine.CreateSubKey(signatureUpdatesPath))
                    {
                        if (signatureKey != null)
                        {
                            signatureKey.SetValue("ForceUpdateFromMU", 0, RegistryValueKind.DWord);
                        }
                    }

                    string spynetPath = registryKeyPath + @"\Spynet";
                    using (RegistryKey spynetKey = Registry.LocalMachine.CreateSubKey(spynetPath))
                    {
                        if (spynetKey != null)
                        {
                            spynetKey.SetValue("DisableBlockAtFirstSeen", 1, RegistryValueKind.DWord);
                        }
                    }
                }
            }

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("\n*** Done! Please restart your system for the changes to take effect. ***");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nAn error occurred while modifying the registry: {ex.Message}");
        }
    }

    static void RevertDefenderPolicy()
    {
        try
        {
            string registryKeyPath = @"SOFTWARE\Policies\Microsoft\Windows Defender";
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKeyPath, true))
            {
                if (key != null)
                {
                    key.DeleteValue("DisableAntiSpyware", false);
                    key.DeleteValue("DisableRealtimeMonitoring", false);
                    key.DeleteValue("DisableAntiVirus", false);
                    key.DeleteValue("DisableSpecialRunningModes", false);
                    key.DeleteValue("DisableRoutinelyTakingAction", false);
                    key.DeleteValue("ServiceKeepAlive", false);

                    DeleteSubKeyTreeIfEmpty(key, "Real-Time Protection");
                    DeleteSubKeyTreeIfEmpty(key, "Signature Updates");
                    DeleteSubKeyTreeIfEmpty(key, "Spynet");

                    if (key.ValueCount == 0 && key.SubKeyCount == 0)
                    {
                        Registry.LocalMachine.DeleteSubKeyTree(registryKeyPath, false);
                    }
                }
            }

            EnableRealTimeProtection();

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("\nChanges reverted successfully. Please restart your system.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nAn error occurred while reverting the registry: {ex.Message}");
        }
    }

    static void EnableRealTimeProtection()
    {
        try
        {
            string registryKeyPath = @"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection";
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKeyPath, true))
            {
                if (key != null)
                {
                    key.DeleteValue("DisableBehaviorMonitoring", false);
                    key.DeleteValue("DisableOnAccessProtection", false);
                    key.DeleteValue("DisableRealtimeMonitoring", false);
                    key.DeleteValue("DisableScanOnRealtimeEnable", false);
                }
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = "sc",
                Arguments = "config WinDefend start= auto",
                CreateNoWindow = true,
                UseShellExecute = false
            }).WaitForExit();

            Process.Start(new ProcessStartInfo
            {
                FileName = "sc",
                Arguments = "start WinDefend",
                CreateNoWindow = true,
                UseShellExecute = false
            }).WaitForExit();

            Console.WriteLine("\nReal-time protection has been re-enabled.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nAn error occurred while enabling real-time protection: {ex.Message}");
        }
    }

    static void DeleteSubKeyTreeIfEmpty(RegistryKey parentKey, string subKeyName)
    {
        using (RegistryKey subKey = parentKey.OpenSubKey(subKeyName, true))
        {
            if (subKey != null && subKey.ValueCount == 0 && subKey.SubKeyCount == 0)
            {
                parentKey.DeleteSubKeyTree(subKeyName, false);
            }
        }
    }

    static void ShowFakeLoadingEffect()
    {
        string[] fakeMessages = {
            "Loading system registry...",
            "Checking policy states...",
            "Applying changes...",
            "Finalizing...",
            "Operation successful..."
        };

        Console.ForegroundColor = ConsoleColor.Green;
        int windowWidth = Console.WindowWidth;
        int windowHeight = Console.WindowHeight;
        int startingRow = windowHeight / 2;

        foreach (string message in fakeMessages)
        {
            int spaces = (windowWidth - message.Length) / 2;
            Console.SetCursorPosition(spaces, startingRow++);
            Console.WriteLine(message);
            Thread.Sleep(2000);
        }
        Console.ResetColor();
    }
}
