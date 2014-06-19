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

namespace ScpSwitcher
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(GuidList.guidPkgString)]
    //[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)] // Load if solution exists
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string)]       // Load if no solution
    public sealed partial class MainSite : Package
    {
        private static DTE2 _DTE2;
        private SolutionEvents _SolutionEvents;

        public const string AnkhSvnPackageId = "604ad610-5cf9-4bd5-8acc-f49810e2efd4";
        public const string AnkhSvnSccProviderId = "8770915b-b235-42ec-bbc6-8e93286e59b5";

        public const string GitScpPackagedId = "c4128d99-2000-41d1-a6c3-704e6c1a3de2";
        public const string GitScpSccId = "c4128d99-0000-41d1-a6c3-704e6c1a3de2";

        private static IVsRegisterScciProvider _VsRegisterScciProvider;
        private static IVsShell _VsShell;

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
        }

        public static void RegisterPrimarySourceControlProvider(SccProvider provider)
        {
            int hr;
            Guid packageGuid = new Guid();
            Guid sccProviderGuid = new Guid();
            switch (provider)
            {
                case SccProvider.AnkhSvn:
                {
                    packageGuid = new Guid(AnkhSvnPackageId);
                    sccProviderGuid = new Guid(AnkhSvnSccProviderId);
                    break;
                }
                case SccProvider.GitSourceControlProvider:
                {
                    packageGuid = new Guid(GitScpPackagedId);
                    sccProviderGuid = new Guid(GitScpSccId);
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
            }
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
        AnkhSvn,
        GitSourceControlProvider
    }
}
