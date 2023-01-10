[![](https://img.shields.io/nuget/v/Soenneker.Blazor.LogJson.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Blazor.LogJson/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blazor.logjson/main.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.blazor.logjson/actions/workflows/main.yml)
[![](https://img.shields.io/nuget/dt/Soenneker.Blazor.LogJson.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Blazor.LogJson/)

# Soenneker.Blazor.LogJson
### A small Blazor interop library that logs JSON (like HTTP requests/responses) within the browser

Supports log levels, grouping, and all in a nice readable format

## Installation

```
Install-Package Soenneker.Blazor.LogJson
```

## Usage

1. Insert the script in `wwwroot/index.html` at the bottom of your `<body>`

```html
<script src="_content/Soenneker.Blazor.LogJson/logjson.js"></script>
```

2. Register the interop within DI (`Program.cs`)

```csharp
public static async Task Main(string[] args)
{
    ...
    builder.Services.AddLogJson();
}
```

3. Inject `ILogJsonInterop` within pages/components where you make `HttpClient` calls


```csharp
@using Soenneker.Blazor.LogJson.Abstract
@inject ILogJsonInterop LogJsonInterop
```


### Simply logging some JSON 
```csharp
var json = "{ 'this-is', 'someJson' }"
await LogJsonInterop.LogJson(json);
```

### Logging requests

```csharp
HttpContent content = new StringContent("{ 'this-is', 'someJson' }");
await LogJsonInterop.LogRequest($"https://google.com", content);
```

### Logging responses
```csharp
HttpResponseMessage response = await client.PostAsync(requestUri, content);
await LogJsonInterop.LogResponse(response);

```