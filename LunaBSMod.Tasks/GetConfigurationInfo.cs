using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace LunaBSMod.Tasks
{
    public class GetConfigurationInfo : Task
    {
        [Required]
        public string AllConfigurations { get; set; }

        [Required]
        public string Configuration { get; set; }

        [Output]
        public string Constants { get; set; } = "";

        [Output]
        public string GameVersion { get; set; } = "";

        public override bool Execute()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Configuration))
                {
                    throw new ArgumentException("Null or whitespace configuration.", nameof(Configuration));
                }

                int hyphenIndex = Configuration.IndexOf("-", StringComparison.Ordinal);
                if (hyphenIndex < 0)
                {
                    Constants = Configuration.ToUpper();
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(AllConfigurations))
                    {
                        throw new ArgumentException("Null or whitespace configurations.", nameof(AllConfigurations));
                    }

                    Version[] versions = AllConfigurations
                        .Split(new[] { ";" }, StringSplitOptions.None)
                        .Select(n => n.Substring(n.LastIndexOf("-", StringComparison.Ordinal) + 1))
                        .Distinct()
                        .Select(n => new Version(n))
                        .ToArray();
                    Version latest = versions.Max();

                    string versionString = Configuration.Substring(hyphenIndex + 1);
                    GameVersion = versionString;

                    Regex pattern = new Regex(@"\W");
                    string prettyConfig = pattern.Replace(Configuration.Substring(0, hyphenIndex), "_").ToUpper();
                    Constants += $"{prettyConfig};";

                    string prettyVersion = pattern.Replace(versionString, "_").ToUpper();
                    Constants += $"V{prettyVersion};";

                    Version curVersion = new Version(versionString);

                    foreach (Version version in versions.Where(n => n > curVersion))
                    {
                        Constants += $"PRE_V{version.ToString().Replace('.', '_').ToUpper()};";
                    }

                    if (curVersion == latest)
                    {
                        Constants += "LATEST;";
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Log.LogErrorFromException(e);
            }

            return false;
        }
    }
}
