using System.Windows;
// using System.Windows.Forms;
using System.Windows.Controls;
using structIQe_Application_Manager.Launcher.Models;
using structIQe_Application_Manager.Launcher.Services;
using System.Diagnostics;
using Microsoft.Win32;
using System.IO;
using System.Security.Policy;
using System.Threading.Tasks;
using Aladdin.HASP;
using System.ComponentModel;
using System.Windows.Input;
using structIQe_Application_Manager.Models;
using System.Text.Json;
using System.Text;
using System.Runtime.InteropServices;

using System.DirectoryServices.ActiveDirectory;
using System.Windows.Interop;

//using Aladdin.HASP;


namespace structIQe_Application_Manager.Launcher
{

    public partial class MainWindow : Window
    {

        private string Company_Name = "structIQe";
        private string App_Name = "Application Manager";
        private string Vendor_Email_id = "info@structiqe.com";
        private string Temp_Location => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Company_Name);
        private string fresh_license_c2v = "Computer Fingerprint for Fresh License.c2v";
        private string existing_license_c2v = "Existing License.c2v";
        public void ShowLoading()
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            Mouse.OverrideCursor = Cursors.Wait;
        }

        public void HideLoading()
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
            Mouse.OverrideCursor = null;
        }


        private bool AnyAppInstalled => Apps.Any(a => a.IsInstalled);
        private readonly AppService NewAppService = new();
        private List<AppStatus> Apps = new();
        private List<AppDefinition> Definitions = new();

        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            AppsListView.SizeChanged += AppsListView_SizeChanged;
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoading();

                // Load ALL app definitions
                Definitions = await NewAppService.LoadAppDefinitionsAsync();
                Apps = await NewAppService.LoadAppsAsync();
                // --- LICENSE SUPPORT CHECK/UPDATE STARTS HERE ---

                // Find License Support definition
                var licenseSupportDef = Definitions.FirstOrDefault(a => a.Id == AppService.License_Support_json);
                var licenseApp = Apps.FirstOrDefault(a => a.Id == AppService.License_Support_json);

                // Combine with install_path if the field doesn't start with a drive letter (C: or \\)
                string licenseSupportBase = licenseSupportDef.InstallPath;

                string haspInstaller = Path.IsPathRooted(licenseSupportDef.HaspInstallerPath)
                    ? licenseSupportDef.HaspInstallerPath
                    : Path.Combine(licenseSupportBase, licenseSupportDef.HaspInstallerPath.TrimStart('.', '\\', '/'));

                string rusExe = Path.IsPathRooted(licenseSupportDef.RusPath)
                    ? licenseSupportDef.RusPath
                    : Path.Combine(licenseSupportBase, licenseSupportDef.RusPath.TrimStart('.', '\\', '/'));



                if (licenseSupportDef != null)
                {
                    string licenseSupportPath = licenseSupportDef.InstallPath;
                    string installedVersion = null;

                    // Load version from installed.json
                    var installedMap = File.Exists(AppService.App_Version_Path)
                        ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(
                            File.ReadAllText(AppService.App_Version_Path))
                        : new Dictionary<string, string>();

                    installedMap.TryGetValue(AppService.License_Support_json, out installedVersion);




                    // If missing or outdated, update License Support
                    /* bool needsUpdate = !Directory.Exists(Path.Combine(licenseSupportPath, AppService.License_Support_Folder))
                      || installedVersion != licenseSupportDef.Version;


                     if (needsUpdate)
                     {
                         var confirm = MessageBox.Show($"A new version of License Support files is available (v{licenseSupportDef.Version}).\nWould you like to update now?\nThis will also reinstall the Sentinel License Runtime.",
                             "License Support Update",
                             MessageBoxButton.YesNo,
                             MessageBoxImage.Question);

                         if (confirm == MessageBoxResult.Yes)
                         {


                             ShowLoading();
                             PurgeLicense(); // Purge license before updating
                             NewAppService.RunHaspInstallerIfNeeded(licenseApp, licenseSupportDef);


                             var licenseSupportStatus = licenseApp ?? new AppStatus
                             {
                                 Id = licenseSupportDef.Id,
                                 Name = licenseSupportDef.Name,
                                 Repo = licenseSupportDef.Repo,
                                 InstallPath = licenseSupportDef.InstallPath,
                                 LatestVersion = licenseSupportDef.Version,
                                 ReleaseNotes = licenseSupportDef.ReleaseNotes,
                                 InstalledVersion = installedVersion
                             };

                             // Update the matching app in _apps to preserve existing install states
                             var existingIndex = Apps.FindIndex(a => a.Id == AppService.License_Support_json);
                             if (existingIndex >= 0)
                                 Apps[existingIndex] = licenseSupportStatus;
                             else
                                 Apps.Add(licenseSupportStatus);

                             // Use the full list to avoid wiping installed.json
                             await NewAppService.InstallOrUpdateUnifiedAsync(licenseSupportStatus, Apps, Definitions);



                             MessageBox.Show(
                                 $"License Support has been updated to v{licenseSupportDef.Version}.",
                                 "License Support Updated",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information
                             );

                             HideLoading();
                         }
                         else
                         {
                             RefreshLicenseStatus();
                             UninstallAllButton.IsEnabled = AnyAppInstalled;

                         }

                     }  */

                    string licenseSupportDir = Path.Combine(licenseSupportPath, AppService.License_Support_Folder);
                    bool folderExists = Directory.Exists(licenseSupportDir);
                    bool shouldUpdate = false;

                    if (!string.IsNullOrWhiteSpace(installedVersion) && !string.IsNullOrWhiteSpace(licenseSupportDef.Version))
                    {
                        try
                        {
                            var installedVer = new Version(installedVersion);
                            var latestVer = new Version(licenseSupportDef.Version);
                            shouldUpdate = latestVer > installedVer;
                        }
                        catch
                        {
                            shouldUpdate = installedVersion != licenseSupportDef.Version;
                        }
                    }


                    var licenseSupportStatus = licenseApp ?? new AppStatus
                    {
                        Id = licenseSupportDef.Id,
                        Name = licenseSupportDef.Name,
                        Repo = licenseSupportDef.Repo,
                        InstallPath = licenseSupportDef.InstallPath,
                        LatestVersion = licenseSupportDef.Version,
                        ReleaseNotes = licenseSupportDef.ReleaseNotes,
                        InstalledVersion = installedVersion
                    };

                    // Update the matching app in _apps to preserve install state
                    var existingIndex = Apps.FindIndex(a => a.Id == AppService.License_Support_json);
                    if (existingIndex >= 0)
                        Apps[existingIndex] = licenseSupportStatus;
                    else
                        Apps.Add(licenseSupportStatus);

                    if (!folderExists)
                    {
                        
                        await NewAppService.InstallOrUpdateUnifiedAsync(licenseSupportStatus, Apps, Definitions);
                       
                    }
                    else if (shouldUpdate)
                    {
                        var confirm = MessageBox.Show(
                            $"A new version of License Support files is available (v{licenseSupportDef.Version}).\nWould you like to update now?\nThis will also reinstall the Sentinel License Runtime.",
                            "License Support Update",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (confirm == MessageBoxResult.Yes)
                        {
                            ShowLoading();
                            PurgeLicense();  // ✅ Purge only on update
                            await NewAppService.InstallOrUpdateUnifiedAsync(licenseSupportStatus, Apps, Definitions);
                            HideLoading();
                        }
                    }


                    // After loading _apps from JSON or disk
                    RefreshLicenseStatus();

                    

                }

                var licenseApp2 = Apps.FirstOrDefault(a => a.Id == AppService.License_Support_json);
                var licenseDef2 = Definitions.FirstOrDefault(d => d.Id == AppService.License_Support_json);

                if (licenseApp != null && licenseDef2 != null)
                {
                    NewAppService.RunHaspInstallerIfNeeded(licenseApp2, licenseDef2);
                }
                // --- LICENSE SUPPORT CHECK/UPDATE ENDS HERE ---

                // Only show other apps in UI
                var visibleDefinitions = Definitions.Where(a => a.Id != AppService.License_Support_json).ToList();

                Apps = (await NewAppService.LoadAppsAsync())
                    .Where(a => a.Id != AppService.License_Support_json).ToList();

                AppsListView.ItemsSource = Apps.Where(a => a.Id != AppService.License_Support_json).ToList();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error loading apps: {ex.Message}", "Launcher Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                HideLoading();
            }

            RefreshLicenseStatus();
            UninstallAllButton.IsEnabled = AnyAppInstalled;

        }








        private async void OnAppActionClick(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement fe || fe.Tag is not string id) return;
            var app = Apps.Find(a => a.Id == id);
            if (app == null) return;

            try
            {
                if (app.IsUpdateAvailable)
                {

                    var msg = $"A new version of {app.Name} is available.\n\nDo you want to update it?";
                    var confirm = MessageBox.Show(
                        msg,
                        "Confirm Update",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (confirm != MessageBoxResult.Yes)
                        return; // 🚫 Exit early if user says no

                    StartUpdate();
                    // Call your uninstall code directly here—no confirmation needed!
                    OnUninstallClick(sender, e);  // This works, but...

                    // Wait a little to ensure the uninstall is done (or call your uninstall code directly as a method for more control)
                    // System.Threading.Thread.Sleep(2000); // Not best practice, but works for simple scenarios
                }

                await NewAppService.InstallOrUpdateUnifiedAsync(app, Apps, Definitions);


            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Install/Update failed: {ex.Message}", "Launcher Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                AppsListView.Items.Refresh();
                UninstallAllButton.IsEnabled = AnyAppInstalled;


            }

            UninstallAllButton.IsEnabled = AnyAppInstalled;

        }



        private static bool IsUpdating = false;
        public static void StartUpdate()
        {
            IsUpdating = true;
        }
        public static void EndUpdate()
        {
            IsUpdating = false;
        }
        public static bool Updating()
        {
            return IsUpdating;
        }


        private void OnUninstallClick(object sender, RoutedEventArgs e)
        {
            

            if (sender is not FrameworkElement fe || fe.Tag is not string id) return;
            var app = Apps.Find(a => a.Id == id);
            if (app == null || !app.IsInstalled) return;

            // Gather uninstall names (for suites/modules)
            /*   var uninstallNames = app.UninstallDisplayNames != null && app.UninstallDisplayNames.Any()
                   ? app.UninstallDisplayNames
                   : new List<string> { app.Name };
            */

            // Confirmation message (shows all modules if >1)


            if (app.IsUpdateAvailable)
            {
               

                
            }
            else
            {
                var msg = $"Are you sure you want to uninstall {app.Name}?";
                if (!IsUninstallAllMode)
                {
                    var confirm = MessageBox.Show(
                                    msg,
                                    "Confirm Uninstall",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Warning);
                    if (confirm != MessageBoxResult.Yes)
                        return;
                }
            }
            

            /* if (uninstallNames.Count > 1)
                 msg += "\n\nThis will remove:\n- " + string.Join("\n- ", uninstallNames);
            */







            try
            {
                // Find install folder for potential cleanup
                string installRoot = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                    "structIQe");
                string folderToDelete = null;

                foreach (var dir in Directory.GetDirectories(installRoot))
                {
                    var dirName = Path.GetFileName(dir).Replace(" ", "").ToLowerInvariant();
                    if (dirName == app.Id.Replace(" ", "").ToLowerInvariant() ||
                        dirName == app.Name.Replace(" ", "").ToLowerInvariant())
                    {
                        folderToDelete = dir;
                        break;
                    }
                }






                /*

                                var unfoundModules = new List<string>();
                                bool anyUninstallAction = false;

                                // Loop through all uninstall names
                                foreach (var uninstallName in uninstallNames)
                                {
                                    string uninstallStr = FindUninstallString(uninstallName);
                                    if (!string.IsNullOrEmpty(uninstallStr))
                                    {
                                        anyUninstallAction = true;
                                        // Handle VSTO uninstall
                                        if (uninstallStr.Contains("VSTOInstaller", StringComparison.OrdinalIgnoreCase))
                                        {
                                            var (installer, vstoFile) = ParseUninstallString(uninstallStr);
                                            if (!string.IsNullOrEmpty(installer) && !string.IsNullOrEmpty(vstoFile))
                                            {
                                                UninstallVSTOAddin(installer, vstoFile);
                                            }
                                        }
                                        else
                                        {
                                            // For .exe or .msi uninstallers
                                            var psi = new ProcessStartInfo
                                            {
                                                FileName = uninstallStr,
                                                UseShellExecute = true,
                                                Verb = "runas"
                                            };
                                            var process = Process.Start(psi);
                                            process.WaitForExit();
                                        }
                                    }
                                    else
                                    {
                                        unfoundModules.Add(uninstallName);
                                    }
                                }

                                 // Warn if some modules/add-ins could not be found for uninstall
                                bool hadSetupRun = Definitions.FirstOrDefault(d => d.Id == app.Id)?.InstallSteps?.Any(
                                    step => step.Action?.Equals("run_setup", StringComparison.OrdinalIgnoreCase) == true
                                ) ?? false;

                                if (unfoundModules.Count > 0)
                                {
                                    if (hadSetupRun)
                                    {
                                        MessageBox.Show(
                                            "Some modules/add-ins could not be found for uninstall (perhaps already removed or the name is wrong):\n\n" +
                                            string.Join("\n", unfoundModules),
                                            "Uninstall Warning",
                                            MessageBoxButton.OK, MessageBoxImage.Warning);
                                    }
                                    else
                                    {


                                        if (!IsUninstallAllMode &&  app.Id != AppService.License_Support_json)
                                        {
                                            MessageBox.Show(
                                            $"{app.Name} was uninstalled successfully.",
                                            "Uninstall Complete",
                                            MessageBoxButton.OK, MessageBoxImage.Information);
                                        }

                                    }
                                }
                                else
                                {

                                    MessageBox.Show(
                                        $"{app.Name} was uninstalled successfully.",
                                        "Uninstall Complete",
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                               */



                // ➕ Reverse all copy actions before folder delete
                var appDef = Definitions.FirstOrDefault(d => d.Id == app.Id);
                if (appDef?.InstallSteps != null)
                {
                    foreach (var step in appDef.InstallSteps)
                    {

                       

                        if (app.Id == AppService.CAD_Assist_id || IsUninstallAllMode)
                        {
                            AppService.ReverseCopyStep(appDef, step, dryRun: false);
                        }







                        if (step.Action.Equals("vsto_installer", StringComparison.OrdinalIgnoreCase))
                        {
                            string vstoFile = step.File;
                            string appName = step.AppName ?? "Excel";

                            if (!string.IsNullOrWhiteSpace(vstoFile))
                            {
                                string addInName = Path.GetFileNameWithoutExtension(vstoFile);
                                string registryKey = addInName.Replace(" ", "_");

                                NewAppService.UnregisterVstoAddIn(appName, registryKey); // If _appService is shared
                            }
                        }
                    }
                }

                // After all uninstall attempts, always try to clean up folder
                if (folderToDelete != null && Directory.Exists(folderToDelete))
                {
                    // Try uninstall.exe first (optional, but harmless)
                    string uninstallExePath = Path.Combine(folderToDelete, "uninstall.exe");
                    if (File.Exists(uninstallExePath))
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = uninstallExePath,
                            UseShellExecute = true,
                            Verb = "runas"
                        };
                        var process = Process.Start(psi);
                        process.WaitForExit();
                    }

                    // Then delete folder (if still present)


                    if (Directory.Exists(folderToDelete))
                    {
                        try
                        {
                            Directory.Delete(folderToDelete, true);

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Failed to delete app folder: {ex.Message}");
                        }
                    }
                }


                app.InstalledVersion = null;
                NewAppService.SaveInstalledState(Apps);
                AppsListView.Items.Refresh();

                if (Updating())
                {
                   
                }
                else
                {
                    MessageBox.Show(
                        $"{app.Name} has been uninstalled successfully.",
                        "Uninstall Complete",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
                   
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Uninstall failed: {ex.Message}", "Uninstall Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            UninstallAllButton.IsEnabled = AnyAppInstalled;

        }


        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            //RefreshLicenseStatus();
        }




        private void OnLicenseSupportClick(object sender, RoutedEventArgs e)
        {
            // Get the license_support definition from your list
            var licenseSupportDef = Definitions.FirstOrDefault(a => a.Id == AppService.License_Support_json);
            if (licenseSupportDef == null)
            {
                MessageBox.Show("License Support is not configured in the launcher.", AppService.License_Support_Folder);
                return;
            }

            string licenseSupportBase = licenseSupportDef.InstallPath ?? "";

            // Build full path for RUS.exe (handles both absolute and relative paths)
            string rusExe = Path.IsPathRooted(licenseSupportDef.RusPath)
                ? licenseSupportDef.RusPath
                : Path.Combine(licenseSupportBase, licenseSupportDef.RusPath.TrimStart('.', '\\', '/'));

            // (Optional) Check for HASP driver and run installer if needed
            string haspInstaller = Path.IsPathRooted(licenseSupportDef.HaspInstallerPath)
                ? licenseSupportDef.HaspInstallerPath
                : Path.Combine(licenseSupportBase, licenseSupportDef.HaspInstallerPath.TrimStart('.', '\\', '/'));

            // Example: Ensure HASP driver is present (first time only)
            if (File.Exists(haspInstaller))
            {
                // You could check driver status here and run install if not present
                // For demo: just print or log the path, or add driver check logic here
            }
            else
            {
                MessageBox.Show("HASP installer not found! License Support may not function correctly.", AppService.License_Support_Folder);
                // Continue (RUS might still work for license operations)
            }

            // Run RUS.exe
            if (File.Exists(rusExe))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = rusExe,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to open RUS: {ex.Message}", AppService.License_Support_Folder);
                }
            }
            else
            {
                MessageBox.Show("RUS application not found! Please reinstall or update License Support.", AppService.License_Support_Folder);
            }
        }






        private async void OnRequestLicenseClick(object sender, RoutedEventArgs e)
        {
            /* // 1. Get app/module info from JSON
             var apps = await _appService.LoadAppsAsync();
             var appNames = apps.Select(a => a.Name).ToList();

             // 2. Choose software (simple input box or custom dialog, here’s a basic example)
             string selectedApp = ShowSelectionDialog("Select Software", appNames);
             if (string.IsNullOrEmpty(selectedApp)) return;

             var app = apps.First(a => a.Name == selectedApp);
             var modules = app.Modules ?? new List<string> { "Full" }; // fallback

             string selectedModule = ShowSelectionDialog("Select Module", modules);
             if (string.IsNullOrEmpty(selectedModule)) return;

             string yearsInput = Microsoft.VisualBasic.Interaction.InputBox(
                 "Enter number of years for license:", "License Duration", "1");
             if (!int.TryParse(yearsInput, out int years) || years <= 0)
             {
                 MessageBox.Show("Invalid years entered.", "Error");
                 return;
             }

             // 3. Compose Email (mail client open)
             string subject = $"License Request: {selectedApp} - {selectedModule}";
             string body = $"Software: {selectedApp}\nModule: {selectedModule}\nYears: {years}\n";
             body += $"User: {Environment.UserName}\nMachine: {Environment.MachineName}";

             string mailto = $"mailto:license@structiqe.com?subject={Uri.EscapeDataString(subject)}&body={Uri.EscapeDataString(body)}";

             System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(mailto) { UseShellExecute = true });   */

            string softwareName = "STRUCT Assist";   // Set this based on user selection
            string moduleName = "Excel Add-in";      // Optional: from user or app JSON
            string years = "1";                      // Get from user (textbox or dropdown)

            string subject = Uri.EscapeDataString($"License Request: {softwareName}");
            string body = Uri.EscapeDataString(
                $"I would like to request a license for:\n\n" +
                $"- Software: {softwareName}\n" +
                $"- Module: {moduleName}\n" +
                $"- Duration: {years} year(s)\n\n" +
                $"Please provide further instructions."
            );

            string mailto = $"mailto:info@structiqe.com?subject={subject}&body={body}";
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(mailto) { UseShellExecute = true });

        }


        public static bool IsUninstallAllMode = false;

        private void OnUninstallAllClick(object sender, RoutedEventArgs e)
        {

            var confirm = MessageBox.Show("Are you sure you want to uninstall all structIQe applications?",
            "Confirm Uninstall All",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);




            //  MessageBox.Show($"Loaded {Definitions.Count} app definitions.\nFirst app: {Definitions[0].Name} Range: {Definitions[0].FeatureRangeStart}-{Definitions[0].FeatureRangeEnd}");
            // RefreshLicenseStatus();

            if (confirm != MessageBoxResult.Yes)
                return;

            try
            {
                ShowLoading();
                PurgeLicense();
                

                IsUninstallAllMode = true;

                foreach (var app in Apps.ToList()) // Clone to avoid modifying during loop
                {
                    if (app.IsInstalled)
                    {
                        var fakeSender = new Button { Tag = app.Id };
                        OnUninstallClick(fakeSender, new RoutedEventArgs());
                        // await Task.Delay(1000); // Stagger for smoother uninstall
                    }
                }

                IsUninstallAllMode = false;

                MessageBox.Show("All apps have been uninstalled.",
                                "Uninstall Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Uninstall All failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                HideLoading();
                UninstallAllButton.IsEnabled = Apps.Any(a => a.IsInstalled); // 🔄 Update button status
            }

            UninstallAllButton.IsEnabled = AnyAppInstalled;
        }


        private void PurgeLicense()
        {
            try
            {
                string licenseSupportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "structIQe", "License Support");
                string haspInstaller = Path.Combine(licenseSupportPath, "haspdinst_106406.exe");

                if (!File.Exists(haspInstaller))
                {
                    MessageBox.Show("HASP uninstaller not found. Cannot purge license.", "License Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"\"{haspInstaller}\" -purge\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = Process.Start(psi);
                process.WaitForExit();

               // MessageBox.Show("✅ License runtime purged successfully.", "License Removed", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Failed to purge license: {ex.Message}", "License Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }







        public static string FindUninstallString(string appName)
        {
            string[] registryRoots = {
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
        @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
    };

            foreach (var root in registryRoots)
            {
                using (var key = Registry.CurrentUser.OpenSubKey(root))
                {
                    if (key == null) continue;
                    foreach (var subKeyName in key.GetSubKeyNames())
                    {
                        using (var subKey = key.OpenSubKey(subKeyName))
                        {
                            var displayName = subKey?.GetValue("DisplayName") as string;
                            var uninstallString = subKey?.GetValue("UninstallString") as string;
                            if (!string.IsNullOrEmpty(displayName) &&
                                displayName.Contains(appName, StringComparison.OrdinalIgnoreCase) &&
                                !string.IsNullOrEmpty(uninstallString))
                            {
                                return uninstallString;
                            }
                        }
                    }
                }
            }
            return null;
        }


        public static (string installer, string vstoFile) ParseUninstallString(string uninstallString)
        {
            if (string.IsNullOrEmpty(uninstallString)) return (null, null);

            int uninstallIdx = uninstallString.IndexOf(" /Uninstall");
            string installer = uninstallIdx >= 0
                ? uninstallString.Substring(0, uninstallIdx).Trim('\"')
                : uninstallString.Trim('\"');

            int fileIdx = uninstallString.IndexOf("file://");
            string vstoFile = fileIdx >= 0
                ? uninstallString.Substring(fileIdx).Trim('\"')
                : "";

            return (installer, vstoFile);
        }



        public static bool UninstallVSTOAddin(string installer, string vstoFile)
        {
            var psi = new ProcessStartInfo
            {
                FileName = installer,
                Arguments = $"/Uninstall \"{vstoFile}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            using (var proc = Process.Start(psi))
            {
                proc.WaitForExit();
                string output = proc.StandardOutput.ReadToEnd();
                string errors = proc.StandardError.ReadToEnd();
                if (!string.IsNullOrEmpty(errors))
                {
                    MessageBox.Show("Error Uninstalling: " + errors, "Uninstall Error");
                    return false;
                }
                return true;
            }
        }









        private void AppsListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (AppsListView.View is GridView gridView)
            {
                double totalWidth = AppsListView.ActualWidth - SystemParameters.VerticalScrollBarWidth - 10;
                if (totalWidth <= 0) return;

                double nameCol = totalWidth * 0.35;
                double versionCol = totalWidth * 0.15;
                double otherCols = (totalWidth - nameCol - versionCol) / 2;

                double minColWidth = 150;
                gridView.Columns[0].Width = Math.Max(nameCol, minColWidth);
                gridView.Columns[1].Width = versionCol;
                gridView.Columns[2].Width = otherCols;
                gridView.Columns[3].Width = otherCols;
            }
        }








        private static bool check_license(int License_ID)

        {
            HaspFeature feature = HaspFeature.FromFeature(License_ID);

            string vendorCode =
            "ZpxswDkoWmhewQLAgZUTlLGoEG+kMFHYTilP8EHzqJQQRQ8nzGFSAtGnkh2oj5L7rLVp0zcnRQFT4+Vt" +
            "T4xp4aT8knCxaRUGvEvt/RbzAUc3st+PVyqRvi0PvfxrIsin2Ejz8qoWzTvvFQvnU4iS+aJBqvatJSaY" +
            "6H4USuR/p/GGaDqBwf0MuNUvWLU+l86SZPvyvpecCk0Hsd3n3ORklsAa5tc9kDYU/w+1iv0S+RhWyhaF" +
            "T19ChE+vTBltMj3l8K/hxvCa7Pyjw0eOaMtA8GhnI10PljMRh66v9QDatHhMoJrY9uGmVFqBbJkGYxtr" +
            "e51nu+TuKzPUu7chV5si8S+wIbRDqdl3ZMlmA2CfVbXV7GkpeE3kD1mWrEBpXh+Ijd6NLaJPmGWFkSD7" +
            "FRqVhoETqhzqnU0FxKfwUXPtq334MlO86mUnb5yR47wakrwr1Rc/Yt3yWFDye1DWFuYh41KS2ozHiJLX" +
            "D0EWOGSJOYKGzbuSEga8sos2eAtW8/A6feEHlAnJB3GOFwYOneIBIdhnSuZN+296w5jST5Osb5g+zGm7" +
            "rvfWuWYvHeDnKQBI2S86txGd6vqu+dR7h+fzIvtZjX7brVu2uPSGCfn6OqZhXK0eTDJbQrVAgpzU7l10" +
            "hWe/VTGCwg2G0oeSgh9ayFjRmyTK3DmlCkUedz5HVoyhNTIPhHpMTVmEFgI5KXkgWea6+9CxoG1V3Xkt" +
            "jiSLNKqglRovXdbHTND7oGIMs5pCJ01yl8PZTaxLZmZED6vn2UhRc1gx4+4gg/4y00Y96Ogreuli+ptg" +
            "6FlmZ/lOV7FVhtcXiOyAZwh9BRYpdUZ4hMJW2SRhLzEY/1EKJ5OHEB/kYxpzCW54kf6wzVh9+Rdev0Fu" +
            "HnnMzPhKM1d8P6bBmuu6NmiQQgL6xrE2QYWD9MXiJeshRrtcRj/dkRNb/26ahbmzY0GWNPT0LTEB7bPD" +
            "/v+wmJ4bY6y3jmxoKOoEww==";

            Hasp hasp = new Hasp(feature);
            HaspStatus status = hasp.Login(vendorCode);

            return status == HaspStatus.StatusOk;
        }


        private static bool CheckLicenseInRange(int start, int end)
        {
            if (start <= 0 || end <= 0 || end < start)
                return false;

            for (int i = start; i <= end; i++)
            {
                if (check_license(i))
                    return true;
            }
            return false;
        }






        private void RefreshLicenseStatus()
        {


            foreach (var app in Apps)
            {

                if (app.Id == AppService.License_Support_json)
                    continue;


                if (app.FeatureRangeStart <= 0 || app.FeatureRangeEnd <= 0 || app.FeatureRangeEnd < app.FeatureRangeStart)
                {
                    app.IsLicensed = false;
                    continue;
                }


                app.IsLicensed = CheckLicenseInRange(app.FeatureRangeStart, app.FeatureRangeEnd);

            }


            AppsListView.Items.Refresh();
        }




        private void Refresh_Button(object sender, RoutedEventArgs e)
        {
            RefreshLicenseStatus();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();   // Close the application
        }
















        private void ReqUpdateLicense(object sender, RoutedEventArgs e)
        {
            CollectC2VForLicenseUpdate();
        }



        private void ApplyLicense(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select License Update File",
                Filter = "Vendor To Customer Files (*.v2c)|*.v2c",
                DefaultExt = "v2c"
            };

            if (openFileDialog.ShowDialog() != true)
                return;

            string v2cData = File.ReadAllText(openFileDialog.FileName);
            string acknowledge = null;

            HaspStatus status = Hasp.Update(v2cData, ref acknowledge);

            if (status == HaspStatus.StatusOk)
            {
                MessageBox.Show("License updated successfully.", App_Name, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Error applying license: " + status, App_Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        private void CollectC2VForLicenseUpdate()
        {
            string scope = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
            <haspscope>
                <license_manager hostname=""localhost"" />
            </haspscope>";

            string format = @"<haspformat format=""updateinfo""/>";

            string vendorCode =
                "ZpxswDkoWmhewQLAgZUTlLGoEG+kMFHYTilP8EHzqJQQRQ8nzGFSAtGnkh2oj5L7rLVp0zcnRQFT4+Vt" +
                "T4xp4aT8knCxaRUGvEvt/RbzAUc3st+PVyqRvi0PvfxrIsin2Ejz8qoWzTvvFQvnU4iS+aJBqvatJSaY" +
                "6H4USuR/p/GGaDqBwf0MuNUvWLU+l86SZPvyvpecCk0Hsd3n3ORklsAa5tc9kDYU/w+1iv0S+RhWyhaF" +
                "T19ChE+vTBltMj3l8K/hxvCa7Pyjw0eOaMtA8GhnI10PljMRh66v9QDatHhMoJrY9uGmVFqBbJkGYxtr" +
                "e51nu+TuKzPUu7chV5si8S+wIbRDqdl3ZMlmA2CfVbXV7GkpeE3kD1mWrEBpXh+Ijd6NLaJPmGWFkSD7" +
                "FRqVhoETqhzqnU0FxKfwUXPtq334MlO86mUnb5yR47wakrwr1Rc/Yt3yWFDye1DWFuYh41KS2ozHiJLX" +
                "D0EWOGSJOYKGzbuSEga8sos2eAtW8/A6feEHlAnJB3GOFwYOneIBIdhnSuZN+296w5jST5Osb5g+zGm7" +
                "rvfWuWYvHeDnKQBI2S86txGd6vqu+dR7h+fzIvtZjX7brVu2uPSGCfn6OqZhXK0eTDJbQrVAgpzU7l10" +
                "hWe/VTGCwg2G0oeSgh9ayFjRmyTK3DmlCkUedz5HVoyhNTIPhHpMTVmEFgI5KXkgWea6+9CxoG1V3Xkt" +
                "jiSLNKqglRovXdbHTND7oGIMs5pCJ01yl8PZTaxLZmZED6vn2UhRc1gx4+4gg/4y00Y96Ogreuli+ptg" +
                "6FlmZ/lOV7FVhtcXiOyAZwh9BRYpdUZ4hMJW2SRhLzEY/1EKJ5OHEB/kYxpzCW54kf6wzVh9+Rdev0Fu" +
                "HnnMzPhKM1d8P6bBmuu6NmiQQgL6xrE2QYWD9MXiJeshRrtcRj/dkRNb/26ahbmzY0GWNPT0LTEB7bPD" +
                "/v+wmJ4bY6y3jmxoKOoEww==";

            string info= null;
            byte[] vendorCodeBytes = Encoding.ASCII.GetBytes(vendorCode);

            HaspStatus status = Hasp.GetInfo(scope, format, vendorCodeBytes, ref info);

            if (status == HaspStatus.StatusOk)
            {
                string tempFolder = Temp_Location; // e.g. @"C:\Users\...\Temp"
                string fileName = Path.Combine(tempFolder, existing_license_c2v); // your filename constant

                try
                {
                    if (!Directory.Exists(tempFolder))
                        Directory.CreateDirectory(tempFolder);

                    File.WriteAllText(fileName, info);

                    Mail_update_Existing_license(fileName); // 🔁 implement this method as needed
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to save C2V file: " + ex.Message, App_Name);
                }
            }
            else
            {
                var result = MessageBox.Show(
                    "Sentinel Protection key not found!\n\nWould you like to request a fresh license?",
                    App_Name, MessageBoxButton.YesNo, MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    CollectC2VForFreshLicense(); // 🔁 your other function
                }
                else
                {
                    // MessageBox.Show("Please contact support for assistance.", App_Name + " : Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    
                }
            }
        }

        private void CollectC2VForFreshLicense()
        {
            string fileName = Path.Combine(Temp_Location, fresh_license_c2v);

            string scope = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
                            <haspscope>
                                <license_manager hostname=""localhost"" />
                            </haspscope>";

            string format = @"<haspformat format=""host_fingerprint""/>";

            string vendorCode =
                "ZpxswDkoWmhewQLAgZUTlLGoEG+kMFHYTilP8EHzqJQQRQ8nzGFSAtGnkh2oj5L7rLVp0zcnRQFT4+Vt" +
                "T4xp4aT8knCxaRUGvEvt/RbzAUc3st+PVyqRvi0PvfxrIsin2Ejz8qoWzTvvFQvnU4iS+aJBqvatJSaY" +
                "6H4USuR/p/GGaDqBwf0MuNUvWLU+l86SZPvyvpecCk0Hsd3n3ORklsAa5tc9kDYU/w+1iv0S+RhWyhaF" +
                "T19ChE+vTBltMj3l8K/hxvCa7Pyjw0eOaMtA8GhnI10PljMRh66v9QDatHhMoJrY9uGmVFqBbJkGYxtr" +
                "e51nu+TuKzPUu7chV5si8S+wIbRDqdl3ZMlmA2CfVbXV7GkpeE3kD1mWrEBpXh+Ijd6NLaJPmGWFkSD7" +
                "FRqVhoETqhzqnU0FxKfwUXPtq334MlO86mUnb5yR47wakrwr1Rc/Yt3yWFDye1DWFuYh41KS2ozHiJLX" +
                "D0EWOGSJOYKGzbuSEga8sos2eAtW8/A6feEHlAnJB3GOFwYOneIBIdhnSuZN+296w5jST5Osb5g+zGm7" +
                "rvfWuWYvHeDnKQBI2S86txGd6vqu+dR7h+fzIvtZjX7brVu2uPSGCfn6OqZhXK0eTDJbQrVAgpzU7l10" +
                "hWe/VTGCwg2G0oeSgh9ayFjRmyTK3DmlCkUedz5HVoyhNTIPhHpMTVmEFgI5KXkgWea6+9CxoG1V3Xkt" +
                "jiSLNKqglRovXdbHTND7oGIMs5pCJ01yl8PZTaxLZmZED6vn2UhRc1gx4+4gg/4y00Y96Ogreuli+ptg" +
                "6FlmZ/lOV7FVhtcXiOyAZwh9BRYpdUZ4hMJW2SRhLzEY/1EKJ5OHEB/kYxpzCW54kf6wzVh9+Rdev0Fu" +
                "HnnMzPhKM1d8P6bBmuu6NmiQQgL6xrE2QYWD9MXiJeshRrtcRj/dkRNb/26ahbmzY0GWNPT0LTEB7bPD" +
                "/v+wmJ4bY6y3jmxoKOoEww==";

            string info = null;
            byte[] vendorCodeBytes = Encoding.ASCII.GetBytes(vendorCode);
            HaspStatus status = Hasp.GetInfo(scope, format, vendorCodeBytes, ref info);

            if (status == HaspStatus.StatusOk)
            {
                try
                {
                    if (!Directory.Exists(Temp_Location))
                        Directory.CreateDirectory(Temp_Location);

                    File.WriteAllText(fileName, info);

                    Mail_fresh_license(fileName); // ✅ implement this as needed
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to save C2V file:\n" + ex.Message, App_Name, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Error: " + status.ToString() + "\nPlease contact support for assistance.",
                                App_Name + " : Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Mail_fresh_license(string attachment_file)
        {
            try
            {

                var licenseSupportDef = Definitions.FirstOrDefault(d => d.Id == AppService.License_Support_json);
               

                if (licenseSupportDef == null || !licenseSupportDef.EmailTemplates.ContainsKey("fresh"))
                {
                    MessageBox.Show("Email template for 'fresh' license not found.", App_Name);
                    return;
                }

                var template = licenseSupportDef.EmailTemplates["fresh"];
                string subject = template.Subject;
                string body = template.Body;


                /*
                var outlookApp = new Microsoft.Office.Interop.Outlook.Application();
                var mail = (Microsoft.Office.Interop.Outlook.MailItem)outlookApp.CreateItem(
                    Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);

                mail.To = Vendor_Email_id;
                mail.Subject = subject;
                mail.Attachments.Add(attachment_file);
                mail.Display();

                string existingBody = mail.HTMLBody;
                string customBodyHtml = "<p style='margin:0;'>" + body.Trim().Replace("\r\n", "<br>").Replace("\n", "<br>") + "</p>";
                mail.HTMLBody = customBodyHtml + existingBody;

                


            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating Outlook email: " + ex.Message, App_Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }

                */


                Microsoft.Office.Interop.Outlook.Application outlookApp;
                try
                {
                    outlookApp = (Microsoft.Office.Interop.Outlook.Application)Activator.CreateInstance(
                        Type.GetTypeFromProgID("Outlook.Application"));
                }
                catch
                {
                    outlookApp = new Microsoft.Office.Interop.Outlook.Application();
                }

                var mail = (Microsoft.Office.Interop.Outlook.MailItem)outlookApp.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);

                mail.To = Vendor_Email_id;
                mail.Subject = subject;
                mail.Attachments.Add(attachment_file);

                // Append the body safely after Display()
                mail.Display();

                string customBodyHtml = "<p style='margin:0;'>" + body.Trim().Replace("\r\n", "<br>").Replace("\n", "<br>") + "</p>";
                mail.HTMLBody = customBodyHtml + mail.HTMLBody;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating Outlook email. If Outlook is not installed, please send the file manually.\n\n" + ex.Message,
                                App_Name, MessageBoxButton.OK, MessageBoxImage.Warning);

                // Optional: fallback using default mail client
                string fallbackSubject = Uri.EscapeDataString("License Request: STRUCT Assist");
                string fallbackBody = Uri.EscapeDataString("Please find attached the license request file.");
                System.Diagnostics.Process.Start(new ProcessStartInfo($"mailto:{Vendor_Email_id}?subject={fallbackSubject}&body={fallbackBody}")
                {
                    UseShellExecute = true
                });
            }



        }
        /*
        private void Mail_update_Existing_license(string attachment_file)
        {
            try
            {
                var licenseSupportDef = Definitions.FirstOrDefault(d => d.Id == AppService.License_Support_json);
                if (licenseSupportDef == null || !licenseSupportDef.EmailTemplates.ContainsKey("upgrade"))
                {
                    MessageBox.Show("Email template for 'upgrade' license not found.", App_Name);
                    return;
                }

                var template = licenseSupportDef.EmailTemplates["upgrade"];
                string subject = template.Subject;
                string body = template.Body;

                var outlookApp = new Microsoft.Office.Interop.Outlook.Application();
                var mail = (Microsoft.Office.Interop.Outlook.MailItem)outlookApp.CreateItem(
                    Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);

                mail.To = Vendor_Email_id;
                mail.Subject = subject;
                mail.Attachments.Add(attachment_file);
                mail.Display();

                string existingBody = mail.HTMLBody;
                string customBodyHtml = "<p style='margin:0;'>" + body.Trim().Replace("\r\n", "<br>").Replace("\n", "<br>") + "</p>";
                mail.HTMLBody = customBodyHtml + existingBody;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating Outlook email: " + ex.Message, App_Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        */
        private void Mail_update_Existing_license(string attachment_file)
        {
            try
            {
                var licenseSupportDef = Definitions.FirstOrDefault(d => d.Id == AppService.License_Support_json);
                if (licenseSupportDef == null || !licenseSupportDef.EmailTemplates.ContainsKey("upgrade"))
                {
                    MessageBox.Show("Email template for 'upgrade' license not found.", App_Name);
                    return;
                }

                var template = licenseSupportDef.EmailTemplates["upgrade"];
                string subject = template.Subject;
                string body = template.Body;

                Microsoft.Office.Interop.Outlook.Application outlookApp;
                try
                {
                    outlookApp = (Microsoft.Office.Interop.Outlook.Application)Activator.CreateInstance(
                        Type.GetTypeFromProgID("Outlook.Application"));
                }
                catch
                {
                    outlookApp = new Microsoft.Office.Interop.Outlook.Application();
                }

                var mail = (Microsoft.Office.Interop.Outlook.MailItem)outlookApp.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);
                mail.To = Vendor_Email_id;
                mail.Subject = subject;
                mail.Attachments.Add(attachment_file);
                mail.Display();

                string customBodyHtml = "<p style='margin:0;'>" + body.Trim().Replace("\r\n", "<br>").Replace("\n", "<br>") + "</p>";
                mail.HTMLBody = customBodyHtml + mail.HTMLBody;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating Outlook email. If Outlook is not installed, please send the file manually.\n\n" + ex.Message,
                                App_Name, MessageBoxButton.OK, MessageBoxImage.Warning);

                string fallbackSubject = Uri.EscapeDataString("License Update: STRUCT Assist");
                string fallbackBody = Uri.EscapeDataString("Please find attached the updated license file.");
                System.Diagnostics.Process.Start(new ProcessStartInfo($"mailto:{Vendor_Email_id}?subject={fallbackSubject}&body={fallbackBody}")
                {
                    UseShellExecute = true
                });
            }
        }



        private void TextBlock_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
