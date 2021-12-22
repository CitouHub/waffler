using System;
using System.Diagnostics;

namespace Waffler.Web.Util
{
    /// <summary>
    /// Extension methods for <see cref="Type"/>
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Returns version from assembly name
        /// </summary>
        /// <param name="type">Type in an assembly to return version from</param>
        /// <returns>Four part version number as string, e.g. 1.0.0.0</returns>
        public static string GetAssemblyVersion(this Type type)
        {
            var assemblyName = type.Assembly.GetName();
            return assemblyName.Version.ToString();
        }

        /// <summary>
        /// Returns file version from assembly
        /// </summary>
        /// <param name="type">Type in an assembly to return version from</param>
        /// <returns>Four part version number as string, e.g. 1.0.0.0</returns>
        public static string GetFileVersion(this Type type)
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(type.Assembly.Location);
            return versionInfo.FileVersion;
        }

        /// <summary>
        /// Returns the informal file version, usually not in version (n.n.n.n) format.
        /// Could contain build date etc.
        /// </summary>
        /// <param name="type">Type in an assembly to return version from</param>
        /// <returns>Build information appended at compile time</returns>
        public static string GetInformalVersion(this Type type)
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(type.Assembly.Location);
            return versionInfo.ProductVersion;
        }
    }
}