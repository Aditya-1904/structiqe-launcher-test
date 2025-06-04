using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using Aladdin.HASP;
using structIQe_Application_Manager.Launcher.Models;
using structIQe_Application_Manager.Models;
using Microsoft.Win32;
using System.Reflection;
using structIQe_Application_Manager.Launcher.Views;
using System.Windows.Threading;




namespace structIQe_Application_Manager.Launcher.Services
{


    public class AppService
    {
        public static string License_Support_json = "License_support";
        public static string CAD_Assist_id = "CAD_Assist";
        public static string License_Support_Folder = "License Support";
        public static string App_Name = "structIQe";
        public static string App_Version_File = "Apps_Installed.json";
        private static string AppsJsonUrl = "https://raw.githubusercontent.com/structIQe-Technologies/Installation-Files/main/structIQe_Apps.json";
        private static string InstallRoot = Path.Combine("C:\\Program Files(x86)", App_Name);
        public static string App_Version_Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), App_Name, App_Version_File);
      
        // private readonly string _installIndexPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "installed.json")

        private readonly HttpClient Http_Definition;
        
        //public string InstallIndexPath => App_Version_Path;

        

        public AppService()
        {
            Http_Definition = new HttpClient();
            Http_Definition.DefaultRequestHeaders.UserAgent.ParseAdd("structIQe Application Manager");
        }

        // 1. Load the latest app definitions from JSON (all meta info, not install state)
        public async Task<List<AppDefinition>> LoadAppDefinitionsAsync()
        {
            using var s = await Http_Definition.GetStreamAsync(AppsJsonUrl);
            var defs = await JsonSerializer.DeserializeAsync<List<AppDefinition>>(s,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<AppDefinition>();
            return defs;
        }

        // 2. Load installed state (merge JSON + what is on disk)
        public async Task<List<AppStatus>> LoadAppsAsync()
             {
            // Ensure parent folder exists
            Directory.CreateDirectory(Path.GetDirectoryName(App_Version_Path)!);

            var defs = await LoadAppDefinitionsAsync();

            //var installedMap = File.Exists(App_Version_Path)
            //    ? JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(App_Version_Path))!
            //    : new Dictionary<string, string>();

            var installedMap = File.Exists(App_Version_Path)
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(App_Version_Path))!
                : new();

            var list = new List<AppStatus>();

            foreach (var d in defs)
            {
                installedMap.TryGetValue(d.Id, out var saved);

                var installDir = string.IsNullOrWhiteSpace(d.InstallPath) && !string.IsNullOrWhiteSpace(d.Folder)
                ? Path.Combine(InstallRoot, d.Folder)
                : d.InstallPath;


                var actual = (installDir != null && Directory.Exists(installDir)) ? saved : null;

                list.Add(new AppStatus
                {
                    Id = d.Id,
                    Name = d.Name,
                    Repo = d.Repo,
                    InstallPath = d.InstallPath,
                    Folder = d.Folder,
                    Modules = d.Modules ?? new List<string>(),
                    LatestVersion = d.Version,
                    ReleaseNotes = d.ReleaseNotes,
                    InstalledVersion = actual,
                    UninstallDisplayNames = d.UninstallDisplayNames ?? new List<string>(),
                    FeatureRangeStart = d.FeatureRangeStart,
                    FeatureRangeEnd = d.FeatureRangeEnd
                });
            }

            return list;
        }

        public void SaveInstalledState(List<AppStatus> apps)
        {
            // Load the existing saved state
            var existingMap = File.Exists(App_Version_Path)
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(App_Version_Path))!
                : new Dictionary<string, string>();

            foreach (var a in apps)
            {
                if (a.IsInstalled)
                    existingMap[a.Id] = a.InstalledVersion!;
                else
                    existingMap.Remove(a.Id);
            }

            File.WriteAllText(App_Version_Path, JsonSerializer.Serialize(existingMap, new JsonSerializerOptions { WriteIndented = true }));
        }


        // Utility: Copy all files and subfolders (used for copying from extracted ZIP)
        public static void DirectoryCopy(string sourceDir, string destDir, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDir);
            DirectoryInfo[] dirs = dir.GetDirectories();
            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);
            foreach (FileInfo file in dir.GetFiles())
                file.CopyTo(Path.Combine(destDir, file.Name), true);
            if (copySubDirs)
                foreach (DirectoryInfo subdir in dirs)
                    DirectoryCopy(subdir.FullName, Path.Combine(destDir, subdir.Name), copySubDirs);
        }

        
            public static class FileUtils
            {
                public static void RoboCopyFolder(string sourceDir, string destDir)
                {
                    if (!Directory.Exists(sourceDir))
                    {
                        MessageBox.Show($"Source folder does not exist:\n{sourceDir}", "Copy Error");
                        return;
                    }

                    // Ensure destination exists (RoboCopy will also create it)
                    Directory.CreateDirectory(destDir);

                    var psi = new ProcessStartInfo
                    {
                        FileName = "robocopy",
                        Arguments = $"\"{sourceDir}\" \"{destDir}\" /E /COPYALL /DCOPY:T /R:1 /W:1 /NFL /NDL /NJH /NJS",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    try
                    {
                        var proc = Process.Start(psi);
                        proc.WaitForExit();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"RoboCopy failed:\n{ex.Message}", "Copy Error");
                    }
                }
            }





        // Download and extract a specific app repo (per-app)
        public async Task<string> DownloadAndExtractAppRepoAsync(AppDefinition app)
        {
            string zipUrl = $"{app.Repo}/archive/refs/heads/main.zip";
            string zipPath = Path.Combine(Path.GetTempPath(), $"{app.Id}_repo.zip");
            using (var resp = await Http_Definition.GetAsync(zipUrl, HttpCompletionOption.ResponseHeadersRead))
            {
                resp.EnsureSuccessStatusCode();
                await using var fs = File.Create(zipPath);
                await resp.Content.CopyToAsync(fs);
            }

            string extractRoot = Path.Combine(Path.GetTempPath(), $"{app.Id}_extract");
            if (Directory.Exists(extractRoot))
                Directory.Delete(extractRoot, true);
            ZipFile.ExtractToDirectory(zipPath, extractRoot);

            string[] dirs = Directory.GetDirectories(extractRoot);
            if (dirs.Length == 1)
                extractRoot = dirs[0];

            return extractRoot;
        }

       
        // App-specific check for AutoCAD
      /*  private bool AutoCADInstalled_orNot()
        {
            string[] possibleYears = { "20*" };
            string basePath = @"C:\Program Files\Autodesk";
            return possibleYears.Any(year => Directory.Exists(Path.Combine(basePath, $"AutoCAD {year}")));
           // return possibleYears.Any(year => Directory.Exists(Path.Combine(basePath, $"AutoCAD 20*")));
        }
      */

        private bool AutoCADInstalled_orNot()
        {
            string basePath = @"C:\Program Files\Autodesk";

            if (!Directory.Exists(basePath))
                return false;

            return Directory.GetDirectories(basePath, "AutoCAD 20*").Any();
        }




        // Helper method to extract allow_create flag from dynamic step object
        private static bool GetAllowCreateFlag(object step)
        {
            if (step != null && step.GetType().GetProperty("AllowCreate") is { } prop)
            {
                var raw = prop.GetValue(step);
                if (raw is JsonElement j && j.ValueKind == JsonValueKind.False)
                    return false;
            }
            return true;
        }

        //private void ButtonAttach_Click(object sender, EventArgs e)
        //{
        //    /// select V2C file which should be attached
        //    if (OpenH2H.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //    {
        //        /// read data from H2H file
        //        StreamReader reader = new StreamReader(OpenH2H.FileName);
        //        String v2c = reader.ReadToEnd();
        //        reader.Close();

        //        HaspStatus status;
        //        String acknowledge = null;

        //        /// update/attach retrieved data
        //        status = Hasp.Update(v2c, ref acknowledge);
        //        if (status != HaspStatus.StatusOk)
        //            MessageBox.Show("Error while attach license (hasp_update)ErrorCode :" + status.ToString());
        //    }
        //}


        private void RegisterVstoAddIn(string appName, string addInKey, string friendlyName, string vstoPath)
        {
            try
            {
                string regPath = $@"Software\Microsoft\Office\{appName}\Addins\{addInKey}";
                using var key = Registry.CurrentUser.CreateSubKey(regPath);
                key.SetValue("Description", friendlyName);
                key.SetValue("FriendlyName", friendlyName);
                key.SetValue("Manifest", $"file:///{vstoPath}|vstolocal");
                key.SetValue("LoadBehavior", 3, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to register {appName} add-in:\n{ex.Message}", "Registry Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public void UnregisterVstoAddIn(string appName, string addInKey)
        {
            try
            {
                string regPath = $@"Software\Microsoft\Office\{appName}\Addins\{addInKey}";
                Registry.CurrentUser.DeleteSubKeyTree(regPath, false);  // false = don't throw if not found
            }
            catch (Exception ex)
            {
                MessageBox.Show($"⚠️ Failed to unregister {appName} add-in:\n{ex.Message}", "Uninstall Warning",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }



        

        //public async Task InstallOrUpdateUnifiedAsync(AppStatus app, List<AppStatus> allApps, List<AppDefinition> allDefinitions)
        //{
        //    Mouse.OverrideCursor = Cursors.Wait;
        //   try
        //   { 
        //    // Example: App-specific pre-check
        //    if (app.Id == CAD_Assist_id && !AutoCADInstalled_orNot())
        //    {
        //        MessageBox.Show(
        //            "CAD Assist requires AutoCAD to be installed.\nPlease install AutoCAD before continuing.",
        //            "Prerequisite Check", MessageBoxButton.OK, MessageBoxImage.Warning);
        //        return;
        //    }

        //    // Get the AppDefinition for this app
        //    var appDef = allDefinitions.FirstOrDefault(d => d.Id == app.Id);
        //    if (appDef == null)
        //    {
        //        MessageBox.Show($"App definition for '{app.Name}' not found!", "Install Error");
        //        return;
        //    }

        //    // Download and extract app repo
        //    string extractedRepoRoot = await DownloadAndExtractAppRepoAsync(appDef);

        //    // Clean old install, if any
        //    string targetDir = appDef.InstallPath;
        //    if (Directory.Exists(targetDir))
        //    {
        //        if (app.Id == License_Support_json)
        //        {
        //            // 🔐 Only delete known files/folders from license support — NOT the whole structIQe folder
        //            var filesToRemove = new List<string>
        //                    {
        //                        "License Support",
        //                        "User Agreement.docx",

        //                    };

        //            foreach (var item in filesToRemove)
        //            {
        //                string path = Path.Combine(targetDir, item);
        //                try
        //                {
        //                    if (Directory.Exists(path))
        //                        Directory.Delete(path, true);
        //                    else if (File.Exists(path))
        //                        File.Delete(path);
        //                }
        //                catch (Exception ex)
        //                {
        //                    MessageBox.Show($"Failed to delete {path}:\n{ex.Message}", "Cleanup Error");
        //                }
        //            }
        //        }
        //        else
        //        {
        //            // All other apps are safe to reinstall entirely
        //            Directory.Delete(targetDir, true);
        //        }
        //    }


        //    // Copy all files from extracted repo to install_path
        //    DirectoryCopy(extractedRepoRoot, targetDir, true);
              
        //    /*    if (app.Id == "STRUCT_Assist_Excel_Addin")
        //        {
        //            string vstoPath = Path.Combine(app.InstallPath, "STRUCT Assist Excel Addin.vsto");
        //            RegisterVstoExcelAddIn("STRUCT_Assist_Excel_Addin", "STRUCT Assist", vstoPath);
        //            MessageBox.Show("STRUCT Assist Excel Add-in registered successfully in Excel!", "Add-in Installed",
        //                 MessageBoxButton.OK, MessageBoxImage.Information);

        //        }
        //    */

        //        // --- Run install_steps, track if any setup was run ---
        //        bool ranSetup = false;
        //    bool setupFailed = false;
        //    if (appDef.InstallSteps != null)
        //    {
        //        foreach (var step in appDef.InstallSteps)
        //        {
        //            if (step.Action.Equals("copy", StringComparison.OrdinalIgnoreCase))
        //            {

        //                string from = Path.IsPathRooted(step.From) ? step.From : Path.Combine(targetDir, step.From);

        //                if (!Directory.Exists(from))
        //                {
        //                    MessageBox.Show($"Source folder not found: {from}", "Copy Error");
        //                    continue;
        //                }
        //                //  Mouse.OverrideCursor = Cursors.Wait;
        //                bool allowCreate = GetAllowCreateFlag(step);

        //                List<string> destinations = new();
        //                if (step.To is JsonElement elem && elem.ValueKind == JsonValueKind.Array)
        //                    destinations = elem.EnumerateArray().Select(x => x.GetString()).Where(x => !string.IsNullOrEmpty(x)).ToList();
        //                else if (step.To != null)
        //                    destinations.Add(step.To.ToString());

        //                List<string> finalDestinations = new();

        //                foreach (var dest in destinations)
        //                {
        //                    if (dest.Contains("*"))
        //                    {
        //                        string basePattern = Path.GetFileName(dest);  // e.g. "AutoCAD 20*"
        //                        string parentDir = Path.GetDirectoryName(dest); // e.g. "C:\\Program Files\\Autodesk"

        //                        if (parentDir != null && Directory.Exists(parentDir))
        //                        {
        //                            foreach (var match in Directory.GetDirectories(parentDir, basePattern))
        //                            {


        //                                finalDestinations.Add(match);
        //                            }
        //                        }
        //                    }

        //                    else
        //                    {
        //                        if (Directory.Exists(dest))
        //                            finalDestinations.Add(dest);
        //                        else if (allowCreate)
        //                        {
        //                            try
        //                            {
        //                                Mouse.OverrideCursor = Cursors.Wait;
        //                                Directory.CreateDirectory(dest);
        //                                finalDestinations.Add(dest);
        //                            }
        //                            catch (Exception ex)
        //                            {
        //                                MessageBox.Show($"Failed to create destination folder: {dest}\n{ex.Message}", "Folder Creation Error");
        //                            }
        //                            finally
        //                            {
        //                                Mouse.OverrideCursor = null;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            MessageBox.Show($"Required folder not found and creation not allowed: {dest}", "Copy Skipped");
        //                        }
        //                    }
        //                }

        //                foreach (var destPath in finalDestinations)
        //                {
        //                    try
        //                    {
        //                       DirectoryCopy(from, destPath, true);

                                    
        //                        }
        //                        catch (Exception ex)
        //                    {
        //                        MessageBox.Show($"Failed to copy to {destPath}:\n{ex.Message}", "Copy Error");
        //                    }
                                
        //                    }

                           
                           


        //                }



        //                /*     else if (step.Action.Equals("run_setup", StringComparison.OrdinalIgnoreCase))
        //                     {
        //                         ranSetup = true;
        //                         string setupPath = Path.IsPathRooted(step.File)
        //                             ? step.File
        //                             : Path.Combine(targetDir, step.File);

        //                         if (!File.Exists(setupPath))
        //                         {
        //                             MessageBox.Show($"Installer not found: {setupPath}", "Setup Error");
        //                             setupFailed = true;
        //                             continue;
        //                         }

        //                         // MessageBox.Show($"Found setup: {setupPath}\nStarting installer...");
        //                         try
        //                         {
        //                             Mouse.OverrideCursor = Cursors.Wait;
        //                             var psi = new ProcessStartInfo
        //                             {
        //                                 FileName = setupPath,
        //                                 Arguments = step.Args ?? "",
        //                                 UseShellExecute = true,
        //                                 Verb = "runas"
        //                             };
        //                             var process = Process.Start(psi);
        //                             if (process != null)
        //                                 await process.WaitForExitAsync();
        //                             await Task.Delay(4000);
        //                         }
        //                         catch (Exception ex)
        //                         {
        //                             MessageBox.Show($"Failed to run setup: {ex.Message}", "Setup Error");
        //                             setupFailed = true;
        //                         }
        //                         finally
        //                         {
        //                             Mouse.OverrideCursor = null;
        //                         }
        //                     }
        //                */
        //                if (step.Action.Equals("vsto_installer", StringComparison.OrdinalIgnoreCase))
        //                {
        //                    string vstoFile = step.GetType()
        //                        .GetProperty("file", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
        //                        ?.GetValue(step)?.ToString();

        //                    string appName = step.GetType()
        //                        .GetProperty("AppName", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
        //                        ?.GetValue(step)?.ToString();

        //                    if (string.IsNullOrWhiteSpace(vstoFile) || string.IsNullOrWhiteSpace(appName))
        //                    {
        //                        MessageBox.Show("⚠️ 'file' or 'app' missing in vsto_installer step.", "VSTO Error",
        //                                        MessageBoxButton.OK, MessageBoxImage.Warning);
        //                        return;
        //                    }

        //                    string fullVstoPath = Path.Combine(app.InstallPath, vstoFile);

        //                    if (File.Exists(fullVstoPath))
        //                    {
        //                        string fileName = Path.GetFileNameWithoutExtension(fullVstoPath);
        //                        string regKey = fileName.Replace(" ", "_");
        //                        RegisterVstoAddIn(appName, regKey, fileName, fullVstoPath);
        //                    }
        //                    else
        //                    {
        //                        MessageBox.Show($"❌ VSTO file not found:\n{fullVstoPath}", "VSTO Registration Error",
        //                                        MessageBoxButton.OK, MessageBoxImage.Warning);
        //                    }
        //                }



        //            }


        //            // 💡 Run HASP installer if app is license_support and has path defined
        //            // 💡 Only run HASP install if not already installed
        //            if (app.Id == License_Support_json)
        //        {
        //            bool hasHaspService = false;

        //            try
        //            {
        //                var scCheck = new ProcessStartInfo
        //                {
        //                    FileName = "sc",
        //                    Arguments = "query hasplms",
        //                    RedirectStandardOutput = true,
        //                    UseShellExecute = false,
        //                    CreateNoWindow = true
        //                };

        //                using (var proc = Process.Start(scCheck))
        //                {
        //                    string output = proc.StandardOutput.ReadToEnd();
        //                    proc.WaitForExit();
        //                    hasHaspService = output.Contains("STATE") && output.Contains("RUNNING");
        //                }
        //            }
        //            catch { }

        //            if (!hasHaspService)
        //            {
        //                string installerPath = Path.IsPathRooted(appDef.HaspInstallerPath)
        //                    ? appDef.HaspInstallerPath
        //                    : Path.Combine(appDef.InstallPath, appDef.HaspInstallerPath.TrimStart('.', '\\', '/'));

        //                if (File.Exists(installerPath))
        //                {
        //                    var psi = new ProcessStartInfo
        //                    {
        //                        FileName = "cmd.exe",
        //                        Arguments = $"/c \"\"{installerPath}\" -i -kp\"",
        //                        UseShellExecute = false,
        //                        CreateNoWindow = true
        //                    };

        //                    try
        //                    {
        //                        var process = Process.Start(psi);
        //                        process.WaitForExit();

        //                        MessageBox.Show("✅ HASP runtime installed successfully.", "License Support", MessageBoxButton.OK, MessageBoxImage.Information);
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        MessageBox.Show($"❌ Failed to install HASP runtime:\n{ex.Message}", "License Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //                    }
        //                }
        //                else
        //                {
        //                    MessageBox.Show("HASP installer not found at the specified path.", "License Support", MessageBoxButton.OK, MessageBoxImage.Warning);
        //                }
        //            }
        //            else
        //            {
        //                Debug.WriteLine("HASP service already running. Skipping re-installation.");
        //            }
        //        }



        //    }



        //    bool hasUninstallNames = appDef.UninstallDisplayNames != null && appDef.UninstallDisplayNames.Count > 0;

        //    if (setupFailed)
        //    {
        //        MessageBox.Show(
        //            $"{app.Name} setup did not complete successfully.\nPlease check and try again.",
        //            "Install Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //        return;
        //    }

        //    if (ranSetup && hasUninstallNames)
        //    {
        //        // Installer ran, check for uninstall entry
        //        bool uninstallEntryExists = false;
        //        var uninstallNames = app.UninstallDisplayNames;
        //        foreach (var uninstallName in uninstallNames)
        //        {
        //            if (!string.IsNullOrEmpty(MainWindow.FindUninstallString(uninstallName)))
        //            {
        //                uninstallEntryExists = true;
        //                break;
        //            }
        //        }

        //        if (uninstallEntryExists)
        //        {
        //            app.InstalledVersion = app.LatestVersion;
        //            SaveInstalledState(allApps);
        //            MessageBox.Show($"{app.Name} installed successfully!", "Install Complete");
        //        }
        //        else
        //        {
        //            MessageBox.Show(
        //                $"{app.Name} may not have installed correctly!\n\n" +
        //                $"No uninstall entry was found in the registry. Please check installation manually.",
        //                "Install Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        //            app.InstalledVersion = app.LatestVersion;
        //            SaveInstalledState(allApps);
        //        }
        //    }
        //    else
        //    {
        //        app.InstalledVersion = app.LatestVersion;
        //        SaveInstalledState(allApps);

        //        if (app.Id != AppService.License_Support_json)
        //        {
        //            MessageBox.Show(
        //                $"{app.LatestVersion} Version of {app.Name} has been installed successfully.",
        //                "Install Complete",
        //                MessageBoxButton.OK,
        //                MessageBoxImage.Information);
        //        }

        //    }
        //  }
        //    finally
        //    {
        //        Mouse.OverrideCursor = null;
        //    }
        //}
        //*/


        public static void ReverseCopyStep(AppDefinition appDef, object step, bool dryRun = false)
        {
            if (step == null || !step.GetType().GetProperty("Action")?.GetValue(step)?.ToString().Equals("copy", StringComparison.OrdinalIgnoreCase) == true)
                return;

            string from = Path.IsPathRooted(step.GetType().GetProperty("From")?.GetValue(step)?.ToString())
                ? step.GetType().GetProperty("From")?.GetValue(step)?.ToString()
                : Path.Combine(appDef.InstallPath, step.GetType().GetProperty("From")?.GetValue(step)?.ToString() ?? "");

            if (!Directory.Exists(from))
                return;

            List<string> destinations = new();
            var toProperty = step.GetType().GetProperty("To")?.GetValue(step);

            if (toProperty is JsonElement elem && elem.ValueKind == JsonValueKind.Array)
            {
                destinations = elem.EnumerateArray().Select(x => x.GetString()).Where(x => !string.IsNullOrEmpty(x)).ToList();
            }
            else if (toProperty != null)
            {
                destinations.Add(toProperty.ToString());
            }

            var sourceDirs = Directory.GetDirectories(from);
            var sourceFiles = Directory.GetFiles(from);

            foreach (var dest in destinations)
            {
                List<string> resolvedDestinations = new();

                if (dest.Contains("*"))
                {
                    string parentDir = Path.GetDirectoryName(dest);
                    string pattern = Path.GetFileName(dest);

                    if (Directory.Exists(parentDir))
                    {
                        resolvedDestinations.AddRange(Directory.GetDirectories(parentDir, pattern));
                    }
                }
                else
                {
                    resolvedDestinations.Add(dest);
                }

                foreach (var actualDest in resolvedDestinations)
                {
                    try
                    {
                        // Delete copied subfolders
                        foreach (var dir in sourceDirs)
                        {
                            string folderName = Path.GetFileName(dir);
                            string destDirPath = Path.Combine(actualDest, folderName);

                            if (Directory.Exists(destDirPath))
                            {
                                if (dryRun)
                                    Debug.WriteLine($"[DRY RUN] Would delete folder: {destDirPath}");
                                else
                                    Directory.Delete(destDirPath, true);
                            }
                        }


                        // Delete copied files
                        foreach (var file in sourceFiles)
                        {
                            string fileName = Path.GetFileName(file);
                            string destFilePath = Path.Combine(actualDest, fileName);

                            if (File.Exists(destFilePath))
                            {
                                if (dryRun)
                                    Debug.WriteLine($"[DRY RUN] Would delete file: {destFilePath}");
                                else
                                    File.Delete(destFilePath);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to remove copied items in:\n{actualDest}\n{ex.Message}", "Cleanup Error");
                    }
                }
            }
        }






















        private bool PrecheckAppRequirements(AppStatus app)
        {
            if (app.Id == CAD_Assist_id && !AutoCADInstalled_orNot())
            {
                MessageBox.Show(
                    "CAD Assist requires AutoCAD to be installed.\nPlease install AutoCAD before continuing.",
                    "Prerequisite Check", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }


        private AppDefinition? GetAppDefinition(AppStatus app, List<AppDefinition> allDefinitions)
        {
            var appDef = allDefinitions.FirstOrDefault(d => d.Id == app.Id);
            if (appDef == null)
                MessageBox.Show($"App definition for '{app.Name}' not found!", "Install Error");

            return appDef;
        }


        private void CleanOldInstall(AppStatus app, AppDefinition appDef)
        {
            string targetDir = appDef.InstallPath;

            if (!Directory.Exists(targetDir))
                return;

            if (app.Id == License_Support_json)
            {
                var filesToRemove = new List<string> { "License Support", "User Agreement.docx" };
                foreach (var item in filesToRemove)
                {
                    string path = Path.Combine(targetDir, item);
                    try
                    {
                        if (Directory.Exists(path)) Directory.Delete(path, true);
                        else if (File.Exists(path)) File.Delete(path);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to delete {path}:\n{ex.Message}", "Cleanup Error");
                    }
                }
            }
            else
            {
                Directory.Delete(targetDir, true);
            }
        }


        private async Task<bool> ExecuteInstallSteps(AppStatus app, AppDefinition appDef, string targetDir)
        {
            bool ranSetup = false;
            bool setupFailed = false;

            if (appDef.InstallSteps == null) return true;

            foreach (var step in appDef.InstallSteps)
            {
                // Handle "copy"
                if (step.Action.Equals("copy", StringComparison.OrdinalIgnoreCase))
                {
                    await HandleCopyStep(app, appDef, step, targetDir);
                }

                // Handle "vsto_installer"
                else if (step.Action.Equals("vsto_installer", StringComparison.OrdinalIgnoreCase))
                {
                    HandleVstoInstallerStep(app, step);
                }

                // You can re-enable setup step later here
            }

            return !setupFailed;
        }

        private async Task HandleCopyStep(AppStatus app, AppDefinition appDef, dynamic step, string targetDir)
        {
            string from = Path.IsPathRooted(step.From) ? step.From : Path.Combine(targetDir, step.From);
            if (!Directory.Exists(from))
            {
                MessageBox.Show($"Source folder not found: {from}", "Copy Error");
                return;
            }

            bool allowCreate = GetAllowCreateFlag(step);
            List<string> destinations = new();

            if (step.To is JsonElement elem && elem.ValueKind == JsonValueKind.Array)
                destinations = elem.EnumerateArray().Select(x => x.GetString()).Where(x => !string.IsNullOrEmpty(x)).ToList();
            else if (step.To != null)
                destinations.Add(step.To.ToString());

            List<string> finalDestinations = new();
            foreach (var dest in destinations)
            {
                if (dest.Contains("*"))
                {
                    string basePattern = Path.GetFileName(dest);
                    string parentDir = Path.GetDirectoryName(dest);

                    if (Directory.Exists(parentDir))
                        finalDestinations.AddRange(Directory.GetDirectories(parentDir, basePattern));
                }
                else
                {
                    if (Directory.Exists(dest))
                        finalDestinations.Add(dest);
                    else if (allowCreate)
                    {
                        try
                        {
                            Directory.CreateDirectory(dest);
                            finalDestinations.Add(dest);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Failed to create destination folder: {dest}\n{ex.Message}", "Folder Creation Error");
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Required folder not found and creation not allowed: {dest}", "Copy Skipped");
                    }
                }
            }

            foreach (var destPath in finalDestinations)
            {
                try
                {
                    DirectoryCopy(from, destPath, true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to copy to {destPath}:\n{ex.Message}", "Copy Error");
                }
            }
        }

        private void HandleVstoInstallerStep(AppStatus app, dynamic step)
        {
            string vstoFile = step.GetType().GetProperty("file", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                ?.GetValue(step)?.ToString();
            string appName = step.GetType().GetProperty("AppName", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                ?.GetValue(step)?.ToString();

            if (string.IsNullOrWhiteSpace(vstoFile) || string.IsNullOrWhiteSpace(appName))
            {
                MessageBox.Show("⚠️ 'file' or 'AppName' missing in vsto_installer step.", "VSTO Error");
                return;
            }

            string fullPath = Path.Combine(app.InstallPath, vstoFile);
            if (File.Exists(fullPath))
            {
                string regKey = Path.GetFileNameWithoutExtension(fullPath).Replace(" ", "_");
                RegisterVstoAddIn(appName, regKey, regKey, fullPath);
            }
            else
            {
                MessageBox.Show($"❌ VSTO file not found:\n{fullPath}", "VSTO Registration Error");
            }
        }



        public void RunHaspInstallerIfNeeded(AppStatus app, AppDefinition appDef)
        {
            if (app.Id != License_Support_json)
                return;

            bool hasHaspService = false;

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = "query hasplms",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                string output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();
                hasHaspService = output.Contains("STATE") && output.Contains("RUNNING");
            }
            catch { }

            if (!hasHaspService)
            {
                string installerPath = Path.Combine(appDef.InstallPath, appDef.HaspInstallerPath.TrimStart('.', '\\', '/'));
                if (File.Exists(installerPath))
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = $"/c \"\"{installerPath}\" -i -kp\"",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        })?.WaitForExit();
                        //MessageBox.Show("✅ HASP runtime installed successfully.", "License Support");
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show($"❌ HASP install failed:\n{ex.Message}", "License Error");
                    }
                }
            }
        }


        private void FinalizeInstallation(AppStatus app, List<AppStatus> allApps, AppDefinition appDef, bool ranSetup, bool setupFailed)
        {
            bool hasUninstallNames = appDef.UninstallDisplayNames != null && appDef.UninstallDisplayNames.Count > 0;

            if (setupFailed)
            {
                MessageBox.Show($"{app.Name} setup did not complete successfully.", "Install Error");
                return;
            }

            if (ranSetup && hasUninstallNames)
            {
                bool uninstallEntryExists = app.UninstallDisplayNames.Any(name => !string.IsNullOrEmpty(MainWindow.FindUninstallString(name)));
                if (uninstallEntryExists)
                {
                    app.InstalledVersion = app.LatestVersion;
                    SaveInstalledState(allApps);
                    MessageBox.Show($"{app.Name} installed successfully!", "Install Complete");
                }
                else
                {
                    MessageBox.Show($"{app.Name} may not have installed correctly!\nNo uninstall entry found.", "Install Warning");
                    app.InstalledVersion = app.LatestVersion;
                    SaveInstalledState(allApps);
                }
            }
            else
            {
                app.InstalledVersion = app.LatestVersion;
                SaveInstalledState(allApps);
                if (app.Id != License_Support_json && !MainWindow.Updating())
                {
                    MessageBox.Show($"{app.LatestVersion} Version of {app.Name} has been installed successfully.", "Install Complete");
                }
                else if (MainWindow.Updating())
                {
                    MessageBox.Show($"{app.LatestVersion} Version of {app.Name} has been updated successfully.", "Install Complete");
                    MainWindow.EndUpdate();
                }
            }
        }
        public async Task InstallOrUpdateUnifiedAsync(AppStatus app, List<AppStatus> allApps, List<AppDefinition> allDefinitions)
        {
            Mouse.OverrideCursor = Cursors.Wait;

            LoadingWindow loader = null;

            try
            {

                if (!PrecheckAppRequirements(app))
                    return;

                // Show the loading window on the UI thread
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    loader = new LoadingWindow
                    {
                        Owner = Application.Current.MainWindow,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                        Topmost = true
                    };

                    Application.Current.MainWindow.IsEnabled = false;
                    loader.Show();

                });

                // Do the installation work on background thread
                bool setupFailed = false;
                bool stepsSucceeded = false;

                

                var appDef = GetAppDefinition(app, allDefinitions);
                if (appDef == null)
                    return;

                CleanOldInstall(app, appDef);

                string extractedRepoRoot = await DownloadAndExtractAppRepoAsync(appDef);
                DirectoryCopy(extractedRepoRoot, appDef.InstallPath, true);

                stepsSucceeded = await ExecuteInstallSteps(app, appDef, appDef.InstallPath);

                RunHaspInstallerIfNeeded(app, appDef);
                await Task.Delay(3000);
                loader?.Close();
                Application.Current.MainWindow.IsEnabled = true;

                FinalizeInstallation(app, allApps, appDef, ranSetup: false, setupFailed: !stepsSucceeded);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Installation failed:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Close the loading window on the UI thread
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    
                    Mouse.OverrideCursor = null;
                });
            }
        }



    }
}
