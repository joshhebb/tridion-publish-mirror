using System;
using System.IO;
using System.Configuration;
using sys = System.Configuration;
using System.Reflection;
using NLog;

namespace Tridion.Events
{
    /// <summary>
    /// Responsible for reading configuration file off of the filesystem, which is in the same folder
    /// as the executing binary. 
    /// </summary>
    public static class Configuration
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static sys.Configuration _dllConfiguration;

        /// <summary>
        /// Load the DLL from the file-system hosted next to executing binary (name specified in settings.cs)
        /// </summary>
        public static sys.Configuration DllConfiguration
        {
            get
            {
                if(_dllConfiguration == null)
                {
                    try
                    {
                        ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap()
                        {
                            ExeConfigFilename = ExecutionDirectoryPathName + Settings.CONFIG_NAME
                        };

                        _dllConfiguration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
                    }
                    catch (Exception ex)
                    {
                        logger.Debug($"Exception loading config : {ex.StackTrace}");
                        throw ex;
                    }
                }

                return _dllConfiguration;
            }
        }

        /// <summary>
        /// Get the directory where the DLL is being executed in the app
        /// </summary>
        public static string ExecutionDirectoryPathName
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly()?.Location) + @"\";
            }
        }

        /// <summary>
        /// Return the configuration value as a boolean (reading true / false)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool GetBooleanAppSetting(string key)
        {
            string setting = GetAppSetting(key);
            return setting != null ? Convert.ToBoolean(setting) : false;
        }

        /// <summary>
        /// Return the configuration value corresponding to the given key (single-value)
        /// </summary>
        public static string GetAppSetting(string key)
        {
            string[] settings = GetAppSettings(key);
            return settings != null && settings.Length > 0 ? settings[0] : string.Empty;
        }

        /// <summary>
        /// Return   configuration value which is comma-separated as a string array by the input key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string[] GetAppSettings(string key)
        {
            KeyValueConfigurationElement configElement = DllConfiguration.AppSettings.Settings[key];

            if (configElement != null)
            {
                string value = configElement.Value;

                if (string.IsNullOrEmpty(value))
                {
                    return null;
                }

                if(value.Contains(","))
                {
                    return value.Split(',');
                }
                else
                {
                    return new string[] { value };
                }
            }
            return null;
        }
    }
}
