using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Soenneker.Blazor.LogJson.Abstract;
using Soenneker.Blazor.Utils.ModuleImport.Abstract;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.Utils.AsyncSingleton;

namespace Soenneker.Blazor.LogJson;

///<inheritdoc cref="ILogJsonInterop"/>
public class LogJsonInterop : ILogJsonInterop
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IModuleImportUtil _moduleImportUtil;
    private readonly AsyncSingleton<object> _scriptInitializer;

    public LogJsonInterop(IJSRuntime jSRuntime, IModuleImportUtil moduleImportUtil)
    {
        _jsRuntime = jSRuntime;
        _moduleImportUtil = moduleImportUtil;

        _scriptInitializer = new AsyncSingleton<object>(async () => {
            await _moduleImportUtil.Import("Soenneker.Blazor.LogJson/logjsoninterop.js");
            await _moduleImportUtil.WaitUntilLoadedAndAvailable("Soenneker.Blazor.LogJson/logjsoninterop.js", "JsonLogger");

            return new object();
        });
    }

    public async ValueTask LogJson(string? jsonString, string group, string logLevel = "log", CancellationToken cancellationToken = default)
    {
        await _scriptInitializer.Get().NoSync();

        await _jsRuntime.InvokeVoidAsync("JsonLogger.logJson", cancellationToken, jsonString, group, logLevel);
    }

    public async ValueTask LogRequest(string requestUri, HttpContent? httpContent = null, HttpMethod? httpMethod = null, CancellationToken cancellationToken = default)
    {
        string? contentString = null;

        if (httpContent != null)
            contentString = await httpContent.ReadAsStringAsync(cancellationToken).NoSync();

        var groupStringBuilder = new StringBuilder("Request: ");

        if (httpMethod != null)
            groupStringBuilder.Append(httpMethod).Append(' ');

        groupStringBuilder.Append(requestUri);

        await LogJson(contentString, groupStringBuilder.ToString(), cancellationToken: cancellationToken).NoSync();
    }

    public async ValueTask LogResponse(HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        var contentString = await response.Content.ReadAsStringAsync(cancellationToken).NoSync();

        var groupStringBuilder = new StringBuilder("Response: ");

        if (response.RequestMessage != null)
        {
            groupStringBuilder.Append(response.RequestMessage.Method)
                .Append(' ')
                .Append(response.RequestMessage.RequestUri)
                .Append(' ');
        }

        groupStringBuilder.Append('(').Append(response.StatusCode).Append(')');

        await LogJson(contentString, groupStringBuilder.ToString(), cancellationToken: cancellationToken).NoSync();
    }

    public ValueTask DisposeAsync()
    {
        return _moduleImportUtil.DisposeModule("Soenneker.Blazor.LogJson/logjsoninterop.js");
    }
}