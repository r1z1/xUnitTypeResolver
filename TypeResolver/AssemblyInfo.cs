
using System.Reflection;
using System.Runtime.InteropServices;


[assembly: AssemblyTitle( "TypeResolver" )]
[assembly: AssemblyDescription( "Used to test all concrete instances of an interface or abstract type.  Resolves types derived from a class or interface, including generic arguments.  Implements a theory data attribute for use with xUnit.net." )]
[assembly: AssemblyConfiguration( "" )]
[assembly: AssemblyCompany( "http://www.codeplex.com/TypeResolver" )]
[assembly: AssemblyProduct( "TypeResolver" )]
[assembly: AssemblyCopyright( "Copyright © 2008" )]
[assembly: AssemblyTrademark( "" )]
[assembly: AssemblyCulture( "" )]

[assembly: ComVisible( false )]

[assembly: AssemblyVersion( TypeResolver.VersionInfo.Full )]
[assembly: AssemblyFileVersion( TypeResolver.VersionInfo.Full )]

namespace TypeResolver {

    internal static class VersionInfo {
        private const string Major = "2.2.";
        private const string Framework = "45";
        private const string Build = ".102";

        public const string Full = Major + Framework + Build;
    }

}
