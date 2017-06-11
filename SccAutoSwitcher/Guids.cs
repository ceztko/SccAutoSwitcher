// Copyright (c) 2013-2014 Francesco Pretto
// This file is subject to the MIT license

namespace SccAutoSwitcher
{
    static class GuidList
    {
#if VS10
        public const string guidPkgString = "e0602ccb-aef8-4c8a-a598-530ff1be851b";
#else
        public const string guidPkgString = "99316bc5-70c5-4ef1-9a29-ea7568408ed0";
#endif
    };
}