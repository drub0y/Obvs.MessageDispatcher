using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("Obvs.MessageDispatcher.Autofac")]
[assembly: AssemblyDescription("Autofac integration support for the Obvs.MessageDispatcher framework.")]

#if DEBUG
[assembly: AssemblyConfiguration("DEBUG")]
#else
[assembly: AssemblyConfiguration("RELEASE")]
#endif

[assembly: AssemblyProduct("Obvs.MessageDispatcher")]
[assembly: AssemblyCopyright("Copyright ©  2015")]

[assembly: AssemblyVersion("0.1.*")]
[assembly: AssemblyInformationalVersion("0.1.0-alpha")]

[assembly: InternalsVisibleTo("Obvs.MessageDispatcher.Autofac.Tests")]