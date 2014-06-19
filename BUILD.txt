BUILD
======

Install Visual Studio 2010 SP1 (or 2013) SDK[1][2]
Just hit F6. Debug with F5.

Under the hood (for VS2013 replace 10.0 with 12.0):
Build script installs the VSPackage in %LOCALAPPDATA%\Microsoft\VisualStudio\10.0Exp\Extensions
"Exp" means a VS local storage for experimental purposes.

It is run by launching the DevEnv with:
c:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\devenv.exe /rootsuffix Exp

[1] http://www.microsoft.com/en-us/download/details.aspx?id=21835
[2] http://www.microsoft.com/en-us/download/details.aspx?id=40758
