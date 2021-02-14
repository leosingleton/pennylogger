# PennyLogger

Log analysis tools such as Splunk, Sumo Logic, or Logstash are critical for monitoring modern cloud applications, but
the costs of processing millions and billions of log events can become prohibitively expensive. PennyLogger is a .NET
Standard library that offloads the first level of event aggregation and filtering to the application itself, enabling
events to be logged at a fraction of the usual costs.


## Getting Started

PennyLogger supports the ASP.NET Core model for dependency injection and logs its output to the standard `ILogger`
interface. To get started, add the PennyLogger service to your application's service dependencies in `Startup.cs`:

```
    public void ConfigureServices(IServiceCollection services)
    {
        // ...
        services.AddPennyLogger();
        // ...
    }
```

Then, in your controller class, add `IPennyLogger` to the constructor's parameters:
```
public class SampleController : ControllerBase
{
    public SampleController(IPennyLogger logger)
    {
        Logger = logger;
    }

    private readonly IPennyLogger Logger;
```

Then, log events to PennyLogger using the `IPennyLogger.Event()` method:
```
        Logger.Event(new
        {
            Event = "MyEvent",
            Value = value
        });
```

By default, events are aggregated and written to the output log every 5 minutes. For instance, if in a 5-minute window,
three events are written to PennyLogger with the values 1, 5, and 10, the following aggregate log entry will be written
to the output `ILogger` interface (formatted for clarity):

```
{
    "Event": "MyEvent$",
    "Count": 3,
    "Value": {
        "Min": 1,
        "Max": 10,
        "Sum": 16
    }
}
```

Note the `$` appended to the event ID to distinguish aggregate events in the final output log.


## License
Copyright (c) 2017-2021 [Leo C. Singleton IV](https://www.leosingleton.com/).
This software is licensed under the MIT License.
