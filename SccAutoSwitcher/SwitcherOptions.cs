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

        [DisplayName("Svn Provider")]
        [Category("Scc Providers")]
        public SvnSccProvider SvnProvider
        {
            get { return MainSite.GetSvnSccProvider(); }
            set { MainSite.SetSvnSccProvider(value); }
        }
    }

    public enum GitSccProvider
    {
        [Description("Git Source Control Provider")]
        GitSourceControlProvider,

        [Display(Name = "Visual Studio Tools for Git")]
        VisualStudioToolsForGit
    }

    public enum SvnSccProvider
    {
        [Description("VisualSVN")]
        VisualSVN,

        [Description("AnkhSVN")]
        AnkhSVN 
    }
}
