using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("Obvs.MessageDispatcher")]
[assembly: AssemblyDescription("A generic message dispatcher architecture based on the Obvs framework.")]

#if DEBUG
[assembly: AssemblyConfiguration("DEBUG")]
#else
[assembly: AssemblyConfiguration("RELEASE")]
#endif

[assembly: AssemblyProduct("Obvs.MessageDispatcher")]
[assembly: AssemblyCopyright("Copyright ©  2015")]

[assembly: AssemblyVersion("0.3.*")]
[assembly: AssemblyInformationalVersion("0.3.0-alpha")]

[assembly: InternalsVisibleTo("Obvs.MessageDispatcher.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]