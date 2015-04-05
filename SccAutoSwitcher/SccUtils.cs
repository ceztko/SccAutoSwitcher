// Copyright (c) 2013-2014 Francesco Pretto
// This file is subject to the MIT license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace SccAutoSwitcher
{
    public partial class MainSite
    {
        public const string AnkhSvnPackageId = "604ad610-5cf9-4bd5-8acc-f49810e2efd4";
        public const string AnkhSvnSccProviderId = "8770915b-b235-42ec-bbc6-8e93286e59b5";

        public const string VisualSvnPackageId = "133240d5-fafa-4868-8fd7-5190a259e676";
        public const string VisualSvnSccProviderId = "937cffd6-105a-4c00-a044-33ffb48a3b8f";

        public const string VSToolsForGitPackagedId = "7fe30a77-37f9-4cf2-83dd-96b207028e1b";
        public const string VSToolsForGitSccProviderId = "28c35eb2-67ea-4c5f-b49d-dacf73a66989";

        public const string GitScpPackagedId = "c4128d99-2000-41d1-a6c3-704e6c1a3de2";
        public const string GitScpSccProviderId = "c4128d99-0000-41d1-a6c3-704e6c1a3de2";

        public const string VisualHGPackageId = "a7f26ca1-2000-4729-896e-0bbe9e380635";
        public const string ViusalHGSccProviderId = "a7f26ca1-0000-4729-896e-0bbe9e380635";

        public const string SccAutoSwitcherCollection = "SccAutoSwitcher";

        public const string SubversionProviderProperty = "SubversionProvider";
        public const string GitProviderProperty = "GitProvider";
        public const string MercurialProviderProperty = "MercurialProvider";

        private static SccProvider GetCurrentSccProvider()
        {
            return GetSccProviderFromGuid(GetCurrentSccProviderGuid());
        }

        private static SccProvider GetSccProviderFromGuid(string guid)
        {
            switch (guid)
            {
                case AnkhSvnSccProviderId:
                    return SccProvider.AnkhSvn;
                case VisualSvnSccProviderId:
                    return SccProvider.VisualSVN;
                case VSToolsForGitSccProviderId:
                    return SccProvider.VisualStudioToolsForGit;
                case GitScpSccProviderId:
                    return SccProvider.GitSourceControlProvider;
                case ViusalHGSccProviderId:
                    return SccProvider.VisualHG;
                default:
                    return SccProvider.Unknown;
            }
        }

        private static GitSccProvider GetGitSccProvider(string str)
        {
            switch (str)
            {
                case "GitSourceControlProvider":
                    return GitSccProvider.GitSourceControlProvider;
                case "VisualStudioToolsForGit":
                    return GitSccProvider.VisualStudioToolsForGit;
                case "Disabled":
                    return GitSccProvider.Disabled;
                default:
                    return GitSccProvider.Default;
            }
        }

        private static SubversionSccProvider GetSubversionSccProvider(string str)
        {
            switch (str)
            {
                case "VisualSVN":
                    return SubversionSccProvider.VisualSVN;
                case "AnkhSVN":
                    return SubversionSccProvider.AnkhSVN;
                case "Disabled":
                    return SubversionSccProvider.Disabled;
                default:
                    return SubversionSccProvider.Default;
            }
        }

        private static MercurialSccProvider GetMercurialSccProvider(string str)
        {
            switch (str)
            {
                case "VisualHG":
                    return MercurialSccProvider.VisualHG;
                case "Disabled":
                    return MercurialSccProvider.Disabled;
                default:
                    return MercurialSccProvider.Default;
            }
        }

        private static RcsType GetRcsTypeFromSccProvider(SccProvider provider)
        {
            switch (provider)
            {
                case SccProvider.AnkhSvn:
                case SccProvider.VisualSVN:
                    return RcsType.Subversion;
                case SccProvider.VisualStudioToolsForGit:
                case SccProvider.GitSourceControlProvider:
                    return RcsType.Git;
                case SccProvider.VisualHG:
                    return RcsType.Mercurial;
                default:
                    return RcsType.Unknown;
            }
        }

        public static void SetGitSccProvider(GitSccProvider provider)
        {
            _SettingsStore.CreateCollection(SccAutoSwitcherCollection);
            if (provider == GitSccProvider.Default)
                _SettingsStore.DeleteProperty(SccAutoSwitcherCollection, GitProviderProperty);
            else
                _SettingsStore.SetString(SccAutoSwitcherCollection, GitProviderProperty, provider.ToString());

            if (provider == GitSccProvider.Disabled)
                return;

            if (_CurrentSolutionRcsType == RcsType.Git)
                RegisterPrimarySourceControlProvider(RcsType.Git);
        }

        public static void SetSubversionSccProvider(SubversionSccProvider provider)
        {
            _SettingsStore.CreateCollection(SccAutoSwitcherCollection);
            if (provider == SubversionSccProvider.Default)
                _SettingsStore.DeleteProperty(SccAutoSwitcherCollection, SubversionProviderProperty);
            else
                _SettingsStore.SetString(SccAutoSwitcherCollection, SubversionProviderProperty, provider.ToString());

            if (provider == SubversionSccProvider.Disabled)
                return;

            if (_CurrentSolutionRcsType == RcsType.Subversion)
                RegisterPrimarySourceControlProvider(RcsType.Subversion);
        }

        public static void SetMercurialSccProvider(MercurialSccProvider provider)
        {
            _SettingsStore.CreateCollection(SccAutoSwitcherCollection);
            if (provider == MercurialSccProvider.Default)
                _SettingsStore.DeleteProperty(SccAutoSwitcherCollection, MercurialProviderProperty);
            else
                _SettingsStore.SetString(SccAutoSwitcherCollection, MercurialProviderProperty, provider.ToString());

            if (provider == MercurialSccProvider.Disabled)
                return;

            if (_CurrentSolutionRcsType == RcsType.Mercurial)
                RegisterPrimarySourceControlProvider(RcsType.Mercurial);
        }


        public static SubversionSccProvider GetSubversionSccProvider()
        {
            string providerStr = _SettingsStore.GetString(SccAutoSwitcherCollection, SubversionProviderProperty, null);
            return GetSubversionSccProvider(providerStr);
        }

        public static MercurialSccProvider GetMercurialSccProvider()
        {
            string providerStr = _SettingsStore.GetString(SccAutoSwitcherCollection, MercurialProviderProperty, null);
            return GetMercurialSccProvider(providerStr);
        }

        public static GitSccProvider GetGitSccProvider()
        {
            string providerStr = _SettingsStore.GetString(SccAutoSwitcherCollection, GitProviderProperty, null);
            return GetGitSccProvider(providerStr);
        }

        public static GitSccProvider GetDefaultGitSccProvider()
        {
            int installed;
            int hr;
            Guid packageId;

            packageId = Guid.Parse(VSToolsForGitPackagedId);
            hr = _VsShell.IsPackageInstalled(ref packageId, out installed);
            Marshal.ThrowExceptionForHR(hr);
            if (installed == 1)
                return GitSccProvider.VisualStudioToolsForGit;

            packageId = Guid.Parse(GitScpPackagedId);
            hr = _VsShell.IsPackageInstalled(ref packageId, out installed);
            Marshal.ThrowExceptionForHR(hr);
            if (installed == 1)
                return GitSccProvider.GitSourceControlProvider;

            return GitSccProvider.Disabled;
        }

        public static SubversionSccProvider GetDefaultSubversionSccProvider()
        {
            int installed;
            int hr;
            Guid packageId;

            packageId = Guid.Parse(AnkhSvnPackageId);
            hr = _VsShell.IsPackageInstalled(ref packageId, out installed);
            Marshal.ThrowExceptionForHR(hr);
            if (installed == 1)
                return SubversionSccProvider.AnkhSVN;

            packageId = Guid.Parse(VisualSvnPackageId);
            hr = _VsShell.IsPackageInstalled(ref packageId, out installed);
            Marshal.ThrowExceptionForHR(hr);
            if (installed == 1)
                return SubversionSccProvider.VisualSVN;

            return SubversionSccProvider.Disabled;
        }

        public static MercurialSccProvider GetDefaultMercurialSccProvider()
        {
            int installed;
            int hr;
            Guid packageId;

            packageId = Guid.Parse(VisualHGPackageId);
            hr = _VsShell.IsPackageInstalled(ref packageId, out installed);
            Marshal.ThrowExceptionForHR(hr);
            if (installed == 1)
                return MercurialSccProvider.VisualHG;

            return MercurialSccProvider.Disabled;
        }

        public static RcsType GetLoadedRcsType()
        {
            SccProvider provider = GetCurrentSccProvider();
            return GetRcsTypeFromSccProvider(provider);
        }

        private static string GetCurrentSccProviderGuid()
        {
            var key = GetRegUserSettingsPath() + @"\CurrentSourceControlProvider";
            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(key);
            if (regKey == null)
                return null;

            return (string)regKey.GetValue("");
        }
    }

    public enum SccProvider
    {
        Unknown = 0,
        AnkhSvn,
        VisualSVN,
        GitSourceControlProvider,
        VisualStudioToolsForGit,
        VisualHG
    }

    public enum RcsType
    {
        Unknown = 0,
        Subversion,
        Git,
        Mercurial
    }
}
