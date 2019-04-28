
namespace Tridion.Events
{
    /// <summary>
    /// Publish Mirror Settings loaded from the EXE config accompanying the DLL.
    /// </summary>
    static class Settings
    {
        /// <summary>
        /// Name of configuration file (.config)
        /// </summary>
        public static string CONFIG_NAME = "Tridion.Events.config";

        /// <summary>
        /// Source publications for publish mirroring 
        /// </summary>
        public static string[] SOURCE_PUBS = Configuration.GetAppSettings("SourcePublications");

        /// <summary>
        /// Target publications for publish mirroring 
        /// </summary>
        public static string[] TARGET_PUBS = Configuration.GetAppSettings("TargetPublications");

        /// <summary>
        /// Enable or disable publish logging of all publish transactions for debugging
        /// </summary>
        public static bool PUBLISH_LOGGING_ENABLED = Configuration.GetBooleanAppSetting("PublishLoggingEnabled");

        /// <summary>
        /// Force publishing / unpublishing to child publication advanced setting set in config.
        /// </summary>
        public static bool FORCE_PUBLISH_CHILD_PUBS = Configuration.GetBooleanAppSetting("ForcePublishToChildPublications");

        /// <summary>
        /// Force publishing the workflow version advanced setting.
        /// </summary>
        public static bool FORCE_PUBLISH_WORKFLOW_VERSION = Configuration.GetBooleanAppSetting("ForcePublishWorkflowVersion");

        /// <summary>
        /// Force publishing the minor version advanced setting.
        /// </summary>
        public static bool FORCE_PUBLISH_MINOR_VERSION = Configuration.GetBooleanAppSetting("ForcePublishMinorVersion");

        internal static class Constants
        {
            public static string CM_XML_TITLE = "Title";

            public static string CM_XML_ID = "ID";
        }
    }
}
