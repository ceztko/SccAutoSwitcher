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
        public const string VSToolsForGitSccProviderId = "11b8e6d7-c08b-4385-b321-321078cdd1f8";

        public const string GitScpPackagedId = "c4128d99-2000-41d1-a6c3-704e6c1a3de2";
        public const string GitScpSccProviderId = "c4128d99-0000-41d1-a6c3-704e6c1a3de2";

        public const string HgSccPackagePackageId = "a7f26ca1-2000-4729-896e-0bbe9e380635";
        public const string HgSccPackageSccProviderId = "a7f26ca1-0000-4729-896e-0bbe9e380635";

        public const string VisualHGPackageId = "dadada00-dfd3-4e42-a61c-499121e136f3";
        public const string VisualHGSccProviderId = "dadada00-63c7-4363-b107-ad5d9d915d45";

        public const string P4VSPackageId = "8d316614-311a-48f4-85f7-df7020f62357";
        public const string P4VSPackageSccProviderId = "fda934f4-0492-4f67-a6eb-cbe0953649f0";


        public const string SccAutoSwitcherCollection = "SccAutoSwitcher";

        public const string SubversionProviderProperty = "SubversionProvider";
        public const string GitProviderProperty = "GitProvider";
        public const string MercurialProviderProperty = "MercurialProvider";
        public const string PerforceProviderProperty = "PerforceProvider";

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
                case HgSccPackageSccProviderId:
                    return SccProvider.HgSccPackage;
                case VisualHGSccProviderId:
                    return SccProvider.VisualHG;
                case P4VSPackageId:
                    return SccProvider.P4VS;
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
                case "HgSccPackage":
                    return MercurialSccProvider.HgSccPackage;
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
                case SccProvider.HgSccPackage:
                case SccProvider.VisualHG:
                    return RcsType.Mercurial;
                default:
                    return RcsType.Unknown;
            }
        }

        private static PerforceSccProvider GetPerforceSccProvider(string str)
        {
            switch (str)
            {
                case "P4VS":
                    return PerforceSccProvider.P4VS;
                case "Disabled":
                    return PerforceSccProvider.Disabled;
                default:
                    return PerforceSccProvider.Default;
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

        public static void SetPerforceSccProvider(PerforceSccProvider provider)
        {
            _SettingsStore.CreateCollection(SccAutoSwitcherCollection);
            if (provider == PerforceSccProvider.Default)
                _SettingsStore.DeleteProperty(SccAutoSwitcherCollection, PerforceProviderProperty);
            else
                _SettingsStore.SetString(SccAutoSwitcherCollection, PerforceProviderProperty, provider.ToString());

            if (provider == PerforceSccProvider.Disabled)
                return;

            if (_CurrentSolutionRcsType == RcsType.Perforce)
                RegisterPrimarySourceControlProvider(RcsType.Perforce);
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

            packageId = Guid.Parse(HgSccPackagePackageId);
            hr = _VsShell.IsPackageInstalled(ref packageId, out installed);
            Marshal.ThrowExceptionForHR(hr);
            if (installed == 1)
                return MercurialSccProvider.HgSccPackage;

            packageId = Guid.Parse(VisualHGPackageId);
            hr = _VsShell.IsPackageInstalled(ref packageId, out installed);
            Marshal.ThrowExceptionForHR(hr);
            if (installed == 1)
                return MercurialSccProvider.VisualHG;

            return MercurialSccProvider.Disabled;
        }

        public static PerforceSccProvider GetDefaultPerforceSccProvider()
        {
            var packageId = Guid.Parse(P4VSPackageId);

            int installed;
            var hr = _VsShell.IsPackageInstalled(ref packageId, out installed);
            Marshal.ThrowExceptionForHR(hr);

            return installed == 1
                ? PerforceSccProvider.P4VS
                : PerforceSccProvider.Disabled;
        }

        public static PerforceSccProvider GetPerforceSccProvider()
        {
            string providerStr = _SettingsStore.GetString(SccAutoSwitcherCollection, PerforceProviderProperty, null);
            return GetPerforceSccProvider(providerStr);
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
        HgSccPackage,
        VisualHG,
        P4VS
    }

    public enum RcsType
    {
        Unknown = 0,
        Subversion,
        Git,
        Mercurial,
        Perforce
    }
}
