namespace Bridge.CLI
{
    /// <summary>
    /// Constant values valid throughout the application.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Directory for CLI to look for initial assemblies on an empty project folder.
        /// </summary>
        public static readonly string AssembliesFolder = "tools";

        /// <summary>
        /// Directory for CLI to look for template projects for new projects installation.
        /// </summary>
        public static readonly string TemplatesFolder = "templates";
    }
}