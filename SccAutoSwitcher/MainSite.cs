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
    public sealed partial class MainSite : Package
    {
        private static DTE2 _DTE2;
        private static SolutionEvents _SolutionEvents;

        private static IVsRegisterScciProvider _VsRegisterScciProvider;
        private static IVsShell _VsShell;
        private static WritableSettingsStore _SettingsStore;

        public MainSite() { }

        protected override void Initialize()
        {
            base.Initialize();
            IVsExtensibility extensibility = GetService<IVsExtensibility>();
            _DTE2 = (DTE2)extensibility.GetGlobalsObject(null).DTE;

            IVsSolution solution = GetService<SVsSolution>() as IVsSolution;
            _SolutionEvents = new SolutionEvents();
            int hr;
            uint pdwCookie;
            hr = solution.AdviseSolutionEvents(_SolutionEvents, out pdwCookie);
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
            bool enabled = false;

            switch (rcsType)
            {
                case RcsType.Subversion:
                {
                    enabled = RegisterSubversionScc(out packageGuid, out sccProviderGuid);
                    break;
                }
                case RcsType.Git:
                {
                    enabled = RegisterGitScc(out packageGuid, out sccProviderGuid);
                    break;
                }
                case RcsType.Mercurial:
                {
                    enabled = RegisterMercurialScc(out packageGuid, out sccProviderGuid);
                    break;
                }
            }

            if (!enabled)
                return;

            int installed;
            hr = _VsShell.IsPackageInstalled(ref packageGuid, out installed);
            Marshal.ThrowExceptionForHR(hr);
            if (installed == 0)
                return;

            hr = _VsRegisterScciProvider.RegisterSourceControlProvider(sccProviderGuid);
            Marshal.ThrowExceptionForHR(hr);
        }

        private static bool RegisterGitScc(out Guid packageGuid, out Guid sccProviderGuid)
        {
            GitSccProvider gitProvider = GetGitSccProvider();

            if (gitProvider == GitSccProvider.Default)
                gitProvider = GetDefaultGitSccProvider();

            if (gitProvider == GitSccProvider.Disabled)
            {
                packageGuid = new Guid();
                sccProviderGuid = new Guid();
                return false;
            }

            switch (gitProvider)
            {
                case GitSccProvider.GitSourceControlProvider:
                {
                    packageGuid = new Guid(GitScpPackagedId);
                    sccProviderGuid = new Guid(GitScpSccProviderId);
                    return true;
                }
                case GitSccProvider.VisualStudioToolsForGit:
                {
                    packageGuid = new Guid(VSToolsForGitPackagedId);
                    sccProviderGuid = new Guid(VSToolsForGitSccProviderId);
                    return true;
                }
                default:
                    throw new Exception();
            }
        }

        private static bool RegisterSubversionScc(out Guid packageGuid, out Guid sccProviderGuid)
        {
            SubversionSccProvider svnProvider = GetSubversionSccProvider();

            if (svnProvider == SubversionSccProvider.Default)
                svnProvider = GetDefaultSubversionSccProvider();

            if (svnProvider == SubversionSccProvider.Disabled)
            {
                packageGuid = new Guid();
                sccProviderGuid = new Guid();
                return false;
            }

            switch (svnProvider)
            {
                case SubversionSccProvider.AnkhSVN:
                {
                    packageGuid = new Guid(AnkhSvnPackageId);
                    sccProviderGuid = new Guid(AnkhSvnSccProviderId);
                    return true;
                }
                case SubversionSccProvider.VisualSVN:
                {
                    packageGuid = new Guid(VisualSvnPackageId);
                    sccProviderGuid = new Guid(VisualSvnSccProviderId);
                    return true;
                }
                default:
                    throw new Exception();
            }
        }


        private static bool RegisterMercurialScc(out Guid packageGuid, out Guid sccProviderGuid)
        {
            MercurialSccProvider mercurialProvider = GetMercurialSccProvider();

            if (mercurialProvider == MercurialSccProvider.Default)
                mercurialProvider = GetDefaultMercurialSccProvider();

            if (mercurialProvider == MercurialSccProvider.Disabled)
            {
                packageGuid = new Guid();
                sccProviderGuid = new Guid();
                return false;
            }

            switch (mercurialProvider)
            {
                case MercurialSccProvider.VisualHG:
                {
                    packageGuid = new Guid(VisualHGPackageId);
                    sccProviderGuid = new Guid(ViusalHGSccProviderId);
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
