using System.Text.Json.Serialization;

namespace structIQe_Application_Manager.Models
{
    public class AppDefinition
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("repo")]
        public string Repo { get; set; } = string.Empty;

        [JsonPropertyName("install_path")]
        public string InstallPath { get; set; } = string.Empty;

        public string Folder { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public List<string> Modules { get; set; } = new List<string>();
      
        [JsonPropertyName("release_notes")]
        public string ReleaseNotes { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public List<string> UninstallDisplayNames { get; set; } = new List<string>();
       
        [JsonPropertyName("hasp_installer_path")]
        public string HaspInstallerPath { get; set; }

        [JsonPropertyName("rus_path")]
        public string RusPath { get; set; }

        [JsonPropertyName("feature_range_start")]
        public int FeatureRangeStart { get; set; }

        [JsonPropertyName("feature_range_end")]
        public int FeatureRangeEnd { get; set; }

        public bool IsLicensed { get; set; }



        [JsonPropertyName("install_steps")]
        public List<InstallStep> InstallSteps { get; set; } = new List<InstallStep>();

        

        [JsonPropertyName("email_template")]
        public Dictionary<string, EmailTemplate> EmailTemplates { get; set; } = new();

    }


    public class EmailTemplate
    {
        public string Subject { get; set; }
        public string Body { get; set; }
    }

    public class InstallStep
    {
        public string Action { get; set; }
        public string From { get; set; }
        public object To { get; set; } // Can be string or array
       

        [JsonPropertyName("app_name")]  // Optional if you're using System.Text.Json and property names match
        public string AppName { get; set; }
        public string File { get; set; }
        public string Args { get; set; }
    }



}
