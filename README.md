# Warden Cachet Integration

![Warden](http://spetz.github.io/img/warden_logo.png)

**OPEN SOURCE & CROSS-PLATFORM TOOL FOR SIMPLIFIED MONITORING**

**[getwarden.net](http://getwarden.net)**

|Branch             |Build status                                                  
|-------------------|-----------------------------------------------------
|master             |[![master branch build status](https://api.travis-ci.org/warden-stack/Warden.Integrations.Cachet.svg?branch=master)](https://travis-ci.org/warden-stack/Warden.Integrations.Cachet)
|develop            |[![develop branch build status](https://api.travis-ci.org/warden-stack/Warden.Integrations.Cachet.svg?branch=develop)](https://travis-ci.org/warden-stack/Warden.Integrations.Cachet/branches)

**CachetIntegration** is designed to work with the **[Cachet](https://Cachet.com)** API in order to provide the beautiful [status pages](https://demo.cachethq.io) based on the data collected from the **[watchers](https://github.com/warden-stack/Warden/wiki/Watcher)**.

### Installation:

Available as a **[NuGet package](https://www.nuget.org/packages/Warden.Integrations.Cachet)**. 
```
dotnet add package Warden.Integrations.Cachet
```

### Configuration:

 - **WithTimeout()** - timeout of the HTTP request to the Cachet API.
 - **WithHeaders()** - request headers of the HTTP request.
 - **FailFast()** - flag determining whether an exception should be thrown if HTTP request returns invalid reponse (false by default).
 - **WithGroupId()** - id of the group to which will be assigned the watchers (components) - 0 by default.
 - **WithJsonSerializerSettings()** -  custom JSON serializer settings of the _Newtonsoft.Json_ library.
 - **WithDateTimeProvider()** - custom provider for the custom DateTime (UTC by default).
 - **WithCachetServiceProvider()** -  custom provider for the _ICachetService_.

**CachetIntegration** can be configured by using the **CachetIntegrationConfiguration** class or via the lambda expression passed to a specialized constructor. 

### Initialization:

In order to register and resolve **CachetIntegration** make use of the available extension methods while configuring the **Warden**:

```csharp
var wardenConfiguration = WardenConfiguration
    .Create()
    .IntegrateWithCachet("http://localhost/api/v1", "access_token_or_credentials"), cfg =>
    {
        cfg.WithTimeout(TimeSpan.FromSeconds(3))
           .WithGroupId(1);
    })
    .SetHooks((hooks, integrations) =>
    {
        hooks.OnIterationCompletedAsync(iteration => 
              OnIterationCompletedCachetAsync(iteration, integrations.Cachet()))
    })
    //Configure watchers, hooks etc..

    private static async Task OnIterationCompletedCachetAsync(IWardenIteration iteration,
        CachetIntegration cachet)
    {
        await cachet.SaveIterationAsync(iteration, notify: true);
    }
```

### Custom interfaces:
```csharp
public interface ICachetService
{
    Task<Component> GetComponentAsync(int id, int groupId);
    Task<Component> GetComponentAsync(string name);
    Task<IEnumerable<Component>> GetComponentsAsync(string name);

    Task<Component> CreateComponentAsync(string name, int status, string description = null,
        string link = null, int order = 0, int groupId = 0, bool enabled = true,
        bool createEvenIfNameIsAlreadyInUse = false);

    Task<Component> UpdateComponentAsync(int id, string name = null, int status = 1,
        string link = null, int order = 0, int groupId = 0, bool enabled = true);

    Task<bool> DeleteComponentAsync(int id)

    Task<IEnumerable<Incident>> GetIncidentsAsync(int componentId);

    Task<Incident> CreateIncidentAsync(string name, string message, int status, int visible = 1,
        int componentId = 0, int componentStatus = 1, bool notify = false,
        DateTime? createdAt = null, string template = null, params string[] vars);

    Task<Incident> UpdateIncidentAsync(int id, string name = null, string message = null,
        int status = 1, int visible = 1, int componentId = 0, int componentStatus = 1,
        bool notify = false);

    Task<bool> DeleteIncidentAsync(int id);
}

public class Component
{
    public int Id { get; protected set; }
    public string Name { get; protected set; }
    public int Status { get; protected set; }
    public string Description { get; protected set; }
    public string Link { get; protected set; }
    public int Order { get; protected set; }
    public int GroupId { get; protected set; }
    public bool Enabled { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public DateTime? DeletedAt { get; protected set; }
    public string StatusName { get; protected set; }
    public string[] Tags { get; protected set; }
}

public class Incident
{
    public int Id { get; protected set; }
    public string Name { get; protected set; }
    public string Message { get; protected set; }
    public int Status { get; protected set; }
    public int Visible { get; protected set; }
    public int ComponentId { get; protected set; }
    public int ComponentStatus { get; protected set; }
    public bool Notify { get; protected set; }
    public DateTime? CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public DateTime? DeletedAt { get; protected set; }
    public DateTime? ScheduledAt { get; protected set; }
    public string HumanStatus { get; protected set; }
    public string Template { get; protected set; }
    public string[] Vars { get; protected set; }
}
```

**ICachetService** is responsible for sending HTTP requests to the using [Cachet API](https://docs.cachethq.io). It can be configured via the *WithCachetServiceProvider()* method. By default, it is based on the HttpClient library.
