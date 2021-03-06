﻿using System.Threading.Tasks;
using Logging.Classes;
using Storage.Classes;
using Windows.ApplicationModel;

namespace UI_Context.Classes
{
    public static class AppUpdateHelper
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--


        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--


        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--
        /// <summary>
        /// Returns the current package version.
        /// </summary>
        /// <returns>The current package version.</returns>
        private static PackageVersion GetPackageVersion()
        {
            return Package.Current.Id.Version;
        }

        /// <summary>
        /// Saves the given package version in the application storage.
        /// </summary>
        /// <param name="version">The package version, that should get saved.</param>
        private static void SetVersion(PackageVersion version)
        {
            Settings.SetSetting(SettingsConsts.VERSION_MAJOR, version.Major);
            Settings.SetSetting(SettingsConsts.VERSION_MINOR, version.Minor);
            Settings.SetSetting(SettingsConsts.VERSION_BUILD, version.Build);
            Settings.SetSetting(SettingsConsts.VERSION_REVISION, version.Revision);
        }

        /// <summary>
        /// Returns the package version from the application storage.
        /// Default: major = 0, minor = 0, build = 0, revision = 0.
        /// </summary>
        /// <returns>A not null package version.</returns>
        private static PackageVersion GetLastStartedVersion()
        {
            return new PackageVersion()
            {
                Major = Settings.GetSettingUshort(SettingsConsts.VERSION_MAJOR),
                Minor = Settings.GetSettingUshort(SettingsConsts.VERSION_MINOR),
                Build = Settings.GetSettingUshort(SettingsConsts.VERSION_BUILD),
                Revision = Settings.GetSettingUshort(SettingsConsts.VERSION_REVISION),
            };
        }

        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--
        /// <summary>
        /// Gets called on App start and performs update task e.g. migrate the DB to a new format.
        /// </summary>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public static async Task OnAppStartAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            PackageVersion versionLastStart = GetLastStartedVersion();

            // Check if version != 0.0.0.0 => first ever start of the app:
            if (!(versionLastStart.Major == 0 && versionLastStart.Major == versionLastStart.Minor && versionLastStart.Minor == versionLastStart.Revision && versionLastStart.Revision == versionLastStart.Build) || Settings.GetSettingBoolean(SettingsConsts.INITIALLY_STARTED))
            {
                if (!Compare(versionLastStart, GetPackageVersion()))
                {
                    // Total refactoring in version 2.0.0.0:
                    if (versionLastStart.Major <= 2)
                    {
                        Logger.Info("Started updating to version 2.0.0.0.");
                        Settings.SetSetting(SettingsConsts.INITIALLY_STARTED, false);
                        Logger.Info("Finished updating to version 2.0.0.0.");
                    }
                }
            }
            SetVersion(GetPackageVersion());
        }

        #endregion

        #region --Misc Methods (Private)--
        /// <summary>
        /// Checks if PackageVersion a is equal to PackageVersion b.
        /// </summary>
        /// <param name="a">The PackageVersion of the last app start.</param> 0.1.0.0
        /// <param name="b">The current PackageVersion.</param> 0.2.0.0
        /// <returns>Returns true, if the current PackageVersion equals the last app start PackageVersion.</returns>
        private static bool Compare(PackageVersion a, PackageVersion b)
        {
            return a.Major == b.Major && a.Minor == b.Minor && a.Build == b.Build && a.Revision == b.Revision;
        }

        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--


        #endregion
    }
}
