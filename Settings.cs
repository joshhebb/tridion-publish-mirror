
namespace Tridion.Events
{
    /// <summary>
    /// Publish Mirror Settings
    /// </summary>
    static class Settings
    {
        public static string CONFIG_NAME = "Tridion.Events.config";

        public static string[] SOURCE_PUBS = Configuration.GetAppSettings("SourcePublications");

        public static string[] TARGET_PUBS = Configuration.GetAppSettings("TargetPublications");

        public static bool OnlyMirrorIfPublishToChildrenSelected = Configuration.GetBooleanAppSetting("OnlyMirrorIfPublishToChildrenSelected");

        internal static class Constants
        {
            public static string CM_XML_TITLE = "Title";

            public static string CM_XML_ID = "ID";
        }
    }
}
