using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using Microsoft.Win32;

class Defcon
{
    static void Main()
    {
        // Check if the application is running as administrator
        if (!IsAdministrator())
        {
            Console.WriteLine("This application needs to run as administrator.");
            Console.WriteLine("Please run this program as Administrator.");
            RestartAsAdmin();
            return; // Exit after prompting to restart
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

        // Center the "Enter 1 to disable Windows Defender policy." message
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.DarkYellow;  // Orange-like color
        string message = "Enter 1 to disable Windows Defender policy.";
        int messagePadding = (windowWidth - message.Length) / 2; // Center message
        Console.SetCursorPosition(messagePadding, Console.CursorTop);
        Console.WriteLine(message);

        // Move the cursor to just below the message
        Console.SetCursorPosition(messagePadding, Console.CursorTop + 1);

        string userInput = Console.ReadLine();

        if (userInput == "1")
        {
            Console.Clear();  // Clear the screen after pressing 1
            ModifyDefenderPolicy();
        }
        else
        {
            Console.WriteLine("Invalid input, exiting...");
        }

        // Wait for the user to acknowledge before closing
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static bool IsAdministrator()
    {
        // Check if the current user has administrator privileges
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    static void RestartAsAdmin()
    {
        try
        {
            // Get the current executable path and restart with admin privileges
            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            ProcessStartInfo startInfo = new ProcessStartInfo(exePath)
            {
                Verb = "runas", // Request elevated privileges
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
            // Base Registry Path for Windows Defender
            string registryKeyPath = @"SOFTWARE\Policies\Microsoft\Windows Defender";
            using (RegistryKey key = Registry.LocalMachine.CreateSubKey(registryKeyPath))
            {
                if (key != null)
                {
                    // Modify the registry values as per your requirements
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

                    // Simulate a loading animation with random text moving up
                    ShowLoadingAnimation();

                    // Centered Done message
                    Console.ForegroundColor = ConsoleColor.Blue;
                    int finalMessageSpaces = (Console.WindowWidth - "*** Done! Please restart your system for the changes to take effect. ***".Length) / 2;
                    Console.SetCursorPosition(finalMessageSpaces, Console.CursorTop);
                    Console.WriteLine("*** Done! Please restart your system for the changes to take effect. ***");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while modifying the registry: {ex.Message}");
        }
    }

    static void ShowLoadingAnimation()
    {
        // Set the color to green for the random text
        Console.ForegroundColor = ConsoleColor.Green;

        // List of random strings to simulate the loading effect
        string[] randomMessages = new string[] {
            "Initializing settings...",
            "Checking system configuration...",
            "Applying security changes...",
            "Loading system data...",
            "Updating policies...",
            "Verifying system integrity...",
            "Optimizing settings...",
            "Preparing to disable Defender...",
            "Finalizing changes...",
            "Please wait, completing configuration..."
        };

        Random rand = new Random();
        int consoleWidth = Console.WindowWidth;
        int consoleHeight = Console.WindowHeight;

        // Fast scrolling random text from the middle to the bottom
        int startPos = consoleHeight / 2 - 5; // Start from a little above the center
        for (int i = 0; i < 20; i++)  // Show random text for 10 seconds (about 20 iterations)
        {
            // Print a random message at a dynamic position
            string message = randomMessages[rand.Next(randomMessages.Length)];
            int spaces = (consoleWidth - message.Length) / 2;
            Console.SetCursorPosition(spaces, startPos + i); // Move down
            Console.WriteLine(message);
            Thread.Sleep(200);  // Speed up scrolling (200ms per update)
        }

        // Wait for the loading effect to be over, and show a final message
        Thread.Sleep(1000);  // Wait for a second before showing "done"
    }
}
