[![](https://img.shields.io/nuget/v/Soenneker.Blazor.LogJson.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Blazor.LogJson/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blazor.logjson/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.blazor.logjson/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/Soenneker.Blazor.LogJson.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Blazor.LogJson/)
[![](https://img.shields.io/badge/Demo-Live-blueviolet?style=for-the-badge&logo=github)](https://soenneker.github.io/soenneker.blazor.logjson/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blazor.logjson/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.blazor.logjson/actions/workflows/codeql.yml)

# Soenneker.Blazor.LogJson

A Blazor interop utility for writing structured values and bounded HTTP bodies to grouped entries in the browser developer console.

![Grouped JSON in the browser console](https://github.com/soenneker/soenneker.blazor.logjson/raw/main/READMEimg.png)

## Installation

```bash
dotnet add package Soenneker.Blazor.LogJson
```

```csharp
using Soenneker.Blazor.LogJson.Registrars;

builder.Services.AddLogJsonInteropAsScoped();
```

Inject the service into a component:

```razor
@using Soenneker.Blazor.LogJson.Abstract
@inject ILogJsonInterop JsonLog
```

## Log a value

```csharp
await JsonLog.Log(
    new
    {
        orderId = order.Id,
        status = order.Status
    },
    group: "Order updated",
    level: "info");
```

The value is transferred through Blazor's normal JavaScript serializer. A string beginning with `{` or `[` is parsed as JSON when valid; other strings are logged unchanged. `level` selects a browser `console` function such as `log`, `info`, `warn`, or `error`; an unknown or non-callable member falls back to `console.log`.

## Log an HTTP request or response

```csharp
using var request = new HttpRequestMessage(HttpMethod.Post, "api/orders")
{
    Content = JsonContent.Create(new { sku = "ABC-123", quantity = 2 })
};

await JsonLog.LogRequest(request);

using HttpResponseMessage response = await Http.SendAsync(request);
await JsonLog.LogResponse(response);
```

Request groups include the method and URI. Response groups include the status plus the originating method and URI when available. These methods log the body only; they do not log request or response headers.

Bodies are buffered up to 64 KiB. A body with a larger declared or observed size is replaced with a skipped-body message, and text over 64K characters is truncated. Logging may buffer otherwise streaming `HttpContent`, so use these helpers for diagnostics rather than high-throughput production paths.

## Sensitive data

Browser-console output is visible to anyone with access to the running browser and may be captured by browser tooling. Do not log authorization data, cookies, access tokens, personal information, payment details, or URLs/query strings containing secrets. Prefer explicitly shaped diagnostic objects over logging entire domain or transport objects.

Interop, serialization, content-read, and cancellation failures are returned to the caller. Await logging when you need to observe those failures; isolate it deliberately if diagnostic logging must never affect the application operation.
