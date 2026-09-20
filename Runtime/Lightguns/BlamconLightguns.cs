namespace Blamcon.Lightguns
{
    /// <summary>Facts about the package itself.</summary>
    public static class BlamconLightguns
    {
        /// <summary>
        /// The package version, matching the <c>version</c> field in <c>package.json</c>: report it with
        /// bug reports, or show it in a settings screen.
        /// </summary>
        /// <remarks>
        /// A constant because <c>UnityEditor.PackageManager.PackageInfo</c> is Editor-only, so a player
        /// build can't read the manifest. A test keeps the two in step.
        /// </remarks>
        public const string version = "2.0.0";
    }
}
