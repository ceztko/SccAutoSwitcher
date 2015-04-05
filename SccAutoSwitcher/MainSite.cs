// Copyright (c) 2013-2014 Francesco Pretto
// This file is subject to the MIT license

using System;
using System.Linq;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using EnvDTE80;
using Microsoft.Build.Evaluation;
using System.Reflection;
using System.IO;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;

namespace SccAutoSwitcher
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(GuidList.guidPkgString)]
    //[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)] // Load if solution exists
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string)]       // Load if no solution
    [ProvideOptionPage(typeof(SwitcherOptions), "Scc Auto Switcher", "Scc Providers", 101, 106, true)] // Options dialog page
    public sealed partial class MainSite : Package, IVsSolutionEvents3, IVsSolutionLoadEvents
    {
        private static DTE2 _DTE2;

        private static IVsRegisterScciProvider _VsRegisterScciProvider;
        private static IVsShell _VsShell;
        private static WritableSettingsStore _SettingsStore;
        private static RcsType _CurrentSolutionRcsType;

        public MainSite() { }

        protected override void Initialize()
        {
            base.Initialize();

            _CurrentSolutionRcsType = RcsType.Unknown;

            IVsExtensibility extensibility = GetService<IVsExtensibility>();
            _DTE2 = (DTE2)extensibility.GetGlobalsObject(null).DTE;

            IVsSolution solution = GetService<SVsSolution>() as IVsSolution;
            int hr;
            uint pdwCookie;
            hr = solution.AdviseSolutionEvents(this, out pdwCookie);
            Marshal.ThrowExceptionForHR(hr);

            _VsShell = GetService<SVsShell>() as IVsShell;
            _VsRegisterScciProvider = GetService<IVsRegisterScciProvider>();
            _SettingsStore = GetWritableSettingsStore();
        }

        public static void RegisterPrimarySourceControlProvider(RcsType rcsType)
        {
            int hr;
            Guid packageGuid = new Guid();
            Guid sccProviderGuid = new Guid();
            SccProvider providerToLoad = SccProvider.Unknown;
            bool enabled = false;

            switch (rcsType)
            {
                case RcsType.Subversion:
                {
                    enabled = RegisterSubversionScc(out packageGuid, out sccProviderGuid, out providerToLoad);
                    break;
                }
                case RcsType.Git:
                {
                    enabled = RegisterGitScc(out packageGuid, out sccProviderGuid, out providerToLoad);
                    break;
                }
                case RcsType.Mercurial:
                {
                    enabled = RegisterMercurialScc(out packageGuid, out sccProviderGuid, out providerToLoad);
                    break;
                }
            }

            if (!enabled)
                return;

            SccProvider currentSccProvider = GetCurrentSccProvider();
            if (providerToLoad == currentSccProvider)
                return;

            int installed;
            hr = _VsShell.IsPackageInstalled(ref packageGuid, out installed);
            Marshal.ThrowExceptionForHR(hr);
            if (installed == 0)
                return;

            hr = _VsRegisterScciProvider.RegisterSourceControlProvider(sccProviderGuid);
            Marshal.ThrowExceptionForHR(hr);
        }

        /// <returns>false if handling the scc provider is disabled for this Rcs type</returns>
        private static bool RegisterGitScc(out Guid packageGuid, out Guid sccProviderGuid, out SccProvider provider)
        {
            GitSccProvider gitProvider = GetGitSccProvider();

            if (gitProvider == GitSccProvider.Default)
                gitProvider = GetDefaultGitSccProvider();

            if (gitProvider == GitSccProvider.Disabled)
            {
                packageGuid = new Guid();
                sccProviderGuid = new Guid();
                provider = SccProvider.Unknown;
                return false;
            }

            switch (gitProvider)
            {
                case GitSccProvider.VisualStudioToolsForGit:
                {
                    packageGuid = new Guid(VSToolsForGitPackagedId);
                    sccProviderGuid = new Guid(VSToolsForGitSccProviderId);
                    provider = SccProvider.VisualStudioToolsForGit;
                    return true;
                }
                case GitSccProvider.GitSourceControlProvider:
                {
                    packageGuid = new Guid(GitScpPackagedId);
                    sccProviderGuid = new Guid(GitScpSccProviderId);
                    provider = SccProvider.GitSourceControlProvider;
                    return true;
                }
                default:
                    throw new Exception();
            }
        }

        /// <returns>false if handling the scc provider is disabled for this Rcs type</returns>
        private static bool RegisterSubversionScc(out Guid packageGuid, out Guid sccProviderGuid, out SccProvider provider)
        {
            SubversionSccProvider svnProvider = GetSubversionSccProvider();

            if (svnProvider == SubversionSccProvider.Default)
                svnProvider = GetDefaultSubversionSccProvider();

            if (svnProvider == SubversionSccProvider.Disabled)
            {
                packageGuid = new Guid();
                sccProviderGuid = new Guid();
                provider = SccProvider.Unknown;
                return false;
            }

            switch (svnProvider)
            {
                case SubversionSccProvider.AnkhSVN:
                {
                    packageGuid = new Guid(AnkhSvnPackageId);
                    sccProviderGuid = new Guid(AnkhSvnSccProviderId);
                    provider = SccProvider.AnkhSvn;
                    return true;
                }
                case SubversionSccProvider.VisualSVN:
                {
                    packageGuid = new Guid(VisualSvnPackageId);
                    sccProviderGuid = new Guid(VisualSvnSccProviderId);
                    provider = SccProvider.VisualSVN;
                    return true;
                }
                default:
                    throw new Exception();
            }
        }

        /// <returns>false if handling the scc provider is disabled for this Rcs type</returns>
        private static bool RegisterMercurialScc(out Guid packageGuid, out Guid sccProviderGuid, out SccProvider provider)
        {
            MercurialSccProvider mercurialProvider = GetMercurialSccProvider();

            if (mercurialProvider == MercurialSccProvider.Default)
                mercurialProvider = GetDefaultMercurialSccProvider();

            if (mercurialProvider == MercurialSccProvider.Disabled)
            {
                packageGuid = new Guid();
                sccProviderGuid = new Guid();
                provider = SccProvider.Unknown;
                return false;
            }

            switch (mercurialProvider)
            {
                case MercurialSccProvider.VisualHG:
                {
                    packageGuid = new Guid(VisualHGPackageId);
                    sccProviderGuid = new Guid(ViusalHGSccProviderId);
                    provider = SccProvider.VisualHG;
                    return true;
                }
                default:
                    throw new Exception();
            }
        }

        private static string GetRegUserSettingsPath()
        {
            string version = _DTE2.Version;
            string suffix = GetSuffix(_DTE2.CommandLineArguments);
            return @"Software\Microsoft\VisualStudio\" + version + suffix;
        }

        private static string GetSuffix(string args)
        {
            string[] tokens = args.Split(' ', '\t');
            int foundIndex = -1;
            int it = 0;
            foreach (string str in tokens)
            {
                if (str.Equals("/RootSuffix", StringComparison.InvariantCultureIgnoreCase))
                {
                    foundIndex = it + 1;
                    break;
                }

                it++;
            }

            if (foundIndex == -1)
                return String.Empty;

            return tokens[foundIndex];
        }

        public WritableSettingsStore GetWritableSettingsStore()
        {
            var shellSettingsManager = new ShellSettingsManager(this);
            return shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
        }

        private void GetService<T>(out T service)
        {
            service = (T)GetService(typeof(T));
        }

        private T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }

        public static DTE2 DTE2
        {
            get { return _DTE2; }
        }
    }
}
