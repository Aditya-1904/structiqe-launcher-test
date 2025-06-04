using System.ComponentModel;
using System.Collections.Generic;

namespace structIQe_Application_Manager.Launcher.Models
{
    public class AppStatus : INotifyPropertyChanged
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Repo { get; set; }           
        public string InstallPath { get; set; }    
        public string Folder { get; set; }
        public string LatestVersion { get; set; }
        public string ReleaseNotes { get; set; }
               public List<string> Modules { get; set; } = new List<string>();
        public List<string> UninstallDisplayNames { get; set; } = new List<string>();
        public int FeatureRangeStart { get; set; }

        public int FeatureRangeEnd { get; set; }

        public bool IsLicensed { get; set; }
        public bool CanInstallLicensed => IsLicensed && CanInstall;

        private string _installedVersion;
        public string InstalledVersion
        {
            get => _installedVersion;
            set
            {
                if (_installedVersion != value)
                {
                    _installedVersion = value;
                    OnPropertyChanged(nameof(InstalledVersion));
                    OnPropertyChanged(nameof(IsInstalled));
                    OnPropertyChanged(nameof(IsUpdateAvailable));
                    OnPropertyChanged(nameof(CanInstall));
                }
            }
        }

        public bool IsInstalled => !string.IsNullOrEmpty(InstalledVersion);
        public bool IsUpdateAvailable
        {
            get
            {
                if (!IsInstalled || string.IsNullOrWhiteSpace(InstalledVersion) || string.IsNullOrWhiteSpace(LatestVersion))
                    return false;

                try
                {
                    var installed = new Version(InstalledVersion);
                    var latest = new Version(LatestVersion);

                    return latest > installed;
                }
                catch
                {
                    // If version parsing fails, fallback to previous logic
                    return InstalledVersion != LatestVersion;
                }
            }
        }

        public bool CanInstall => !IsInstalled || IsUpdateAvailable;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
