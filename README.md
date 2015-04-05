# SccAutoSwitcher

A Visual Studio extension that auto loads Scc providers
depending on presence of reserved repository file
or directories.
Supported Scc providers are:

 * AnkhSVN *(Subversion, default)*;
 * VisualSVN *(Subversion)*;
 * Visual Studio Tools for Git *(Git, default)*;
 * Git Source Control Provider *(Git)*;
 * HgSccPackage *(mercurial)*.
 
More providers could be added, provided they are regular
Scc providers and there exists an easy way to detect
proper RCS type by checking file or directories presence
starting from solution root directory.

It supports Visual Studio 2010/2012/2013.

### Options dialog page

SccAutoSwitcher offers a very simple options dialog page
allowing to change the default Scc provider loading
priority for different RCS types. To explain allowed
values, let's take Suversion Scc providers as an example:

* **Default**: means AnkhSvn is loaded first, if it's
  found, otherwise the latter is loaded.
* **AnkhSvn**: will always try to load AnkhSvn,
  regardless of extension(s) install status;
* **VisualSvn**: will always try to load VisualSvn,
  regardless of extension(s) install status;
* **Disabled**: won't try to switch Scc provider for
  Subversion repositories, effectively disabling
  SccAutoSwitcher for this RCS.

### Notice to popular Scc provider devs (git, Subversion, ...)

No popular Scc provider seems to enforce itself using
the same mechanism SccAutoSwitcher uses to load them.
This may be a restriction of VisualStudio. enforced for
Scc provider extensions only, or a misunderstanding
on the used API[1]. Regardless of the motivation, this actually
leaves doors open for competition, as multiple Scc providers
can be installed and there's no contention during solution
loading: this is the specific moment when SccAutoSwitcher
intervenes, acting as an arbitrator for the different Scc
providers and efficiently loading the correct or favourite
provider.

[1] https://msdn.microsoft.com/en-us/library/microsoft.visualstudio.shell.interop.ivsregisterscciprovider.registersourcecontrolprovider.aspx