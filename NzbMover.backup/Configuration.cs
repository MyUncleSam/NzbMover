using System;
using System.Collections.Generic;
//using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IniParser;
using IniParser.Model;
using Alphaleonis.Win32.Filesystem;

namespace NzbMover
{
    public class Configuration
    {
        public ConfigGeneral General { get; private set; }
        public ConfigSettings Settings { get; private set; }

        public Configuration(IniData conf)
        {
            General = new ConfigGeneral()
            {
                debug = Convert.ToBoolean(conf["GENERAL"]["debug"])
            };

            Settings = new ConfigSettings()
            {
                target = new DirectoryInfo(conf["SETTINGS"]["target"]),
                action = (ConfigSettings.FileAction)Enum.Parse(typeof(ConfigSettings.FileAction), conf["SETTINGS"]["action"]),
                allow_duplicates = Convert.ToBoolean(conf["SETTINGS"]["allow_duplicates"]),
                close_success = Convert.ToInt32(conf["SETTINGS"]["close_success"]),
                move_switch = Convert.ToBoolean(conf["SETTINGS"]["close_error"]),
                ask_for_password = Convert.ToBoolean(conf["SETTINGS"]["ask_for_password"]),
                password_extraction_method = (ConfigSettings.PasswordExtractionMethod)Enum.Parse(typeof(ConfigSettings.PasswordExtractionMethod), conf["SETTINGS"]["password_extraction_method"])
            };
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine();
            sb.AppendLine(Output.GetSeperator());
            sb.AppendLine("Configuration:");

            sb.AppendLine(Output.GetSeperator());

            sb.AppendLine("[GENERAL]");
            sb.AppendLine(string.Format("debug = {0}", General.debug));

            sb.AppendLine(Output.GetSeperator());

            sb.AppendLine("[SETTINGS]");
            sb.AppendLine(string.Format("target = {0} (exists: {1})", Settings.target.FullName, Settings.target.Exists));
            sb.AppendLine(string.Format("action = {0}", Settings.action.ToString()));
            sb.AppendLine(string.Format("allow_duplicates = {0}", Settings.allow_duplicates));
            sb.AppendLine(string.Format("close_success = {0}", Settings.close_success));
            sb.AppendLine(string.Format("move_switch = {0}", Convert.ToBoolean(Settings.move_switch)));
            sb.AppendLine(string.Format("ask_for_password = {0}", Convert.ToBoolean(Settings.ask_for_password)));
            sb.AppendLine(string.Format("password_extraction_method = {0}", Settings.password_extraction_method.ToString()));

            sb.AppendLine(Output.GetSeperator());
            sb.AppendLine();

            return sb.ToString();
        }

        public class ConfigGeneral
        {
            public bool debug { get; set; }
        }

        public class ConfigSettings
        {
            public DirectoryInfo target { get; set; }
            public FileAction action { get; set; }
            public bool allow_duplicates { get; set; }
            public int close_success { get; set; }
            public bool move_switch { get; set; }
            public bool ask_for_password { get; set; }
            public PasswordExtractionMethod password_extraction_method { get; set; }

            public enum FileAction
            {
                MOVE,
                COPY
            }

            public enum PasswordExtractionMethod
            {
                None,
                OnlyName,
                OnlyNzbMetadata,
                FilenameOverNzbMetadata,
                NzbMetadataOverFilename
            }
        }
    }
}
