#Obvs.MessageDispatcher

A message dispatching framework built upon [the Obvs framework](https://github.com/inter8ection/Obvs).

---

This framework is designed to sit atop the Obvs framework's core library and provide a   

The simplest possible configuration of the message dispatching framework on top of an Obvs service bus 
looks like this:

```
// Configure and create an Obvs service bus instance
var myObvsServiceBusConfiguration = ConfigureServiceBus();
var serviceBus = myServiceBusConfiguration.CreateServiceBus();

// Create a message dispatcher for the events that are coming off the service bus
var dispatcherConfiguration = serviceBus.CreateMessageDispatcherFor(sb => sb.Events);

// Configure the dispatcher with a "simple" message handler factory and configure that 
// with all the IMessageHandler instances in my program's assembly.
dispatcherConfiguration.WithSimpleMessageHandlerFactory()    
    .RegisterMessageHandlers(typeof(MyProgram).Assembly);

// Start the message dispatcher
dispatcherConfiguration.DispatchMessages();
```


---

#Obvs.MessageDispatcher.Autofac

Provides integration support between the core `Obvs.MessageDispatcher` framework and the Autofac dependency injection framework in the form 
of an `IMessageHandlerSelector` which resolves instances of `IMessageHandler<TMessage>` from a specified Autofac `Container`. 