#Obvs.MessageDispatcher

A message dispatching framework built upon [the Obvs framework](https://github.com/inter8ection/Obvs).

---

##Basic Configuration Example

The simplest possible configuration of the message dispatching framework on top of an Obvs service bus 
looks like this:

```
// Configure and create an Obvs service bus instance
var myObvsServiceBusConfiguration = ConfigureServiceBus();
var serviceBus = myObvsServiceBusConfiguration.CreateServiceBus();

// Create a message dispatcher for the events that are coming off the service bus
var dispatcherConfiguration = serviceBus.CreateMessageDispatcherFor(sb => sb.Events);

// Configure the dispatcher with the default message handler factory and configure that 
// with all the IMessageHandler instances in my program's assembly.
dispatcherConfiguration.WithDefaultMessageHandlerFactory()    
    .RegisterMessageHandlers(typeof(MyProgram).Assembly);

// Start the message dispatcher
dispatcherConfiguration.DispatchMessages();
```