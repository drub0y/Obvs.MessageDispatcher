using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Features.Scanning;

namespace Obvs.MessageDispatcher.Autofac
{
    public static class AutofacRegistrationExtensions
    {
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle> RegisterMessageHandlers(this ContainerBuilder containerBuilder, params Assembly[] messageHandlerAssemblies)
        {
            return containerBuilder.RegisterAssemblyTypes(messageHandlerAssemblies)
                .Where(t => typeof(IMessageHandler).IsAssignableFrom(t))
                .AsImplementedInterfaces();
        }
    }
}
