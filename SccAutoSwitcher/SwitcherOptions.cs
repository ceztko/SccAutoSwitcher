using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SccAutoSwitcher
{
    public class SwitcherOptions : DialogPage
    {
        [DisplayName("Git Provider")]
        [Category("Scc Providers")]
        public GitSccProvider GitProvider
        {
            get { return MainSite.GetGitSccProvider(); }
            set { MainSite.SetGitSccProvider(value); }
        }

        [DisplayName("Subversion Provider")]
        [Category("Scc Providers")]
        public SubversionSccProvider SubversionProvider
        {
            get { return MainSite.GetSubversionSccProvider(); }
            set { MainSite.SetSubversionSccProvider(value); }
        }

        [DisplayName("Mercurial Provider")]
        [Category("Scc Providers")]
        public MercurialSccProvider MercurialProvider
        {
            get { return MainSite.GetMercurialSccProvider(); }
            set { MainSite.SetMercurialSccProvider(value); }
        }
    }

    public enum GitSccProvider
    {
        Default = 0,

        [Description("Git Source Control Provider")]
        GitSourceControlProvider,

        [Display(Name = "Visual Studio Tools for Git")]
        VisualStudioToolsForGit,

        Disabled
    }

    public enum SubversionSccProvider
    {
        Default = 0,

        [Description("VisualSVN")]
        VisualSVN,

        [Description("AnkhSVN")]
        AnkhSVN,

        Disabled
    }

    public enum MercurialSccProvider
    {
        Default = 0,

        [Description("VisualHG")]
        VisualHG,

        Disabled
    }
}
