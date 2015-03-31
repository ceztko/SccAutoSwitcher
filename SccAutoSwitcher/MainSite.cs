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

        public const string AnkhSvnPackageId = "604ad610-5cf9-4bd5-8acc-f49810e2efd4";
        public const string AnkhSvnSccProviderId = "8770915b-b235-42ec-bbc6-8e93286e59b5";

        public const string VisualSvnPackageId = "133240d5-fafa-4868-8fd7-5190a259e676";
        public const string VisualSvnSccProviderId = "937cffd6-105a-4c00-a044-33ffb48a3b8f";

        public const string VSToolsForGitPackagedId = "7fe30a77-37f9-4cf2-83dd-96b207028e1b";
        public const string VSToolsForGitSccProviderId = "28c35eb2-67ea-4c5f-b49d-dacf73a66989";

        public const string GitScpPackagedId = "c4128d99-2000-41d1-a6c3-704e6c1a3de2";
        public const string GitScpSccProviderId = "c4128d99-0000-41d1-a6c3-704e6c1a3de2";

        public const string MercurialPackageId = "a7f26ca1-2000-4729-896e-0bbe9e380635";
        public const string MercurialSccProviderId = "a7f26ca1-0000-4729-896e-0bbe9e380635";

        private static IVsRegisterScciProvider _VsRegisterScciProvider;
        private static IVsShell _VsShell;
        private static WritableSettingsStore _SettingsStore;
        private static RcsType _CurrentRcsType;
        private static SccProvider _CurrentSccProvider;

        public MainSite() { }

        protected override void Initialize()
        {
            base.Initialize();
            IVsExtensibility extensibility = GetService<IVsExtensibility>();
            _DTE2 = (DTE2)extensibility.GetGlobalsObject(null).DTE;

            string version = _DTE2.Version;

            // TODO: Read HKEY_CURRENT_USER\Software\Microsoft\VisualStudio\12.0\CurrentSourceControlProvider
            string suffix = GetSuffix(_DTE2.CommandLineArguments);
            var key2 = @"Software\Microsoft\VisualStudio\12.0\CurrentSourceControlProvider";
            var guidString = (string)Microsoft.Win32.Registry.CurrentUser.OpenSubKey(key2).GetValue("");
            var currentGuid = Guid.Parse(guidString);


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
            SccProvider sccProvider = SccProvider.None;

            switch (rcsType)
            {
                case RcsType.Svn:
                {
                    RegisterSvnScc(out packageGuid, out sccProviderGuid, out sccProvider);
                    break;
                }
                case RcsType.Git:
                {
                    RegisterGitScc(out packageGuid, out sccProviderGuid, out sccProvider);
                    break;
                }
                case RcsType.Mercurial:
                {
                    packageGuid = new Guid(MercurialPackageId);
                    sccProviderGuid = new Guid(MercurialSccProviderId);
                    sccProvider = SccProvider.Mercurial;
                    break;
                }
            }

            int installed;
            hr = _VsShell.IsPackageInstalled(ref packageGuid, out installed);
            Marshal.ThrowExceptionForHR(hr);
            if (installed == 1)
            {
                hr = _VsRegisterScciProvider.RegisterSourceControlProvider(sccProviderGuid);
                Marshal.ThrowExceptionForHR(hr);
                _CurrentRcsType = rcsType;
                _CurrentSccProvider = sccProvider;
            }
        }

        private static void RegisterGitScc(out Guid packageGuid, out Guid sccProviderGuid, out SccProvider provider)
        {
            GitSccProvider gitProvider = GetGitSccProvider();
            switch (gitProvider)
            {
                case GitSccProvider.GitSourceControlProvider:
                {
                    packageGuid = new Guid(GitScpPackagedId);
                    sccProviderGuid = new Guid(GitScpSccProviderId);
                    provider = SccProvider.GitSourceControlProvider;
                    break;
                }
                case GitSccProvider.VisualStudioToolsForGit:
                {
                    packageGuid = new Guid(VSToolsForGitPackagedId);
                    sccProviderGuid = new Guid(VSToolsForGitSccProviderId);
                    provider = SccProvider.VisualStudioToolsForGit;
                    break;
                }
                default:
                    throw new Exception();
            }
        }

        private static void RegisterSvnScc(out Guid packageGuid, out Guid sccProviderGuid, out SccProvider provider)
        {
            SvnSccProvider svnprovider = GetSvnSccProvider();
            switch (svnprovider)
            {
                case SvnSccProvider.AnkhSVN:
                {
                    packageGuid = new Guid(AnkhSvnPackageId);
                    sccProviderGuid = new Guid(AnkhSvnSccProviderId);
                    provider = SccProvider.AnkhSvn;
                    break;
                }
                case SvnSccProvider.VisualSVN:
                {
                    packageGuid = new Guid(VisualSvnPackageId);
                    sccProviderGuid = new Guid(VisualSvnSccProviderId);
                    provider = SccProvider.VisualSVN;
                    break;
                }
                default:
                    throw new Exception();
            }
        }

        public static void SetGitSccProvider(GitSccProvider provider)
        {
            _SettingsStore.CreateCollection("SccAutoSwitcher");
            _SettingsStore.SetString("SccAutoSwitcher", "GitProvider", provider.ToString());
            if (_CurrentRcsType == RcsType.Git)
                RegisterPrimarySourceControlProvider(RcsType.Git);
        }

        public static void SetSvnSccProvider(SvnSccProvider provider)
        {
            _SettingsStore.CreateCollection("SccAutoSwitcher");
            _SettingsStore.SetString("SccAutoSwitcher", "SvnProvider", provider.ToString());
            if (_CurrentRcsType == RcsType.Svn)
                RegisterPrimarySourceControlProvider(RcsType.Svn);
        }

        private static GitSccProvider GetGitSccProvider(string provider)
        {
            switch (provider)
            {
                case "GitSourceControlProvider":
                    return GitSccProvider.GitSourceControlProvider;
                case "VisualStudioToolsForGit":
                default:
                    return GitSccProvider.VisualStudioToolsForGit;
            }
        }

        private static SvnSccProvider GetSvnSccProvider(string provider)
        {
            switch (provider)
            {
                case "VisualSVN":
                    return SvnSccProvider.VisualSVN;
                case "AnkhSVN":
                default:
                    return SvnSccProvider.AnkhSVN;
            }
        }

        public static SvnSccProvider GetSvnSccProvider()
        {
            string providerStr = _SettingsStore.GetString("SccAutoSwitcher", "SvnProvider", null);
            return GetSvnSccProvider(providerStr);
        }

        public static GitSccProvider GetGitSccProvider()
        {
            string providerStr = _SettingsStore.GetString("SccAutoSwitcher", "GitProvider", null);
            return GetGitSccProvider(providerStr);
        }

        public WritableSettingsStore GetWritableSettingsStore()
        {
            var shellSettingsManager = new ShellSettingsManager(this);
            return shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
        }

        private string GetSuffix(string args)
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

    public enum SccProvider
    {
        None = 0,
        AnkhSvn,
        VisualSVN,
        GitSourceControlProvider,
        VisualStudioToolsForGit,
        Mercurial
    }

    public enum RcsType
    {
        None = 0,
        Svn,
        Git,
        Mercurial
    }
}
