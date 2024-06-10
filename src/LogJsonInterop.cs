using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Soenneker.Blazor.LogJson.Abstract;

namespace Soenneker.Blazor.LogJson;

///<inheritdoc cref="ILogJsonInterop"/>
public class LogJsonInterop : ILogJsonInterop
{
    private readonly IJSRuntime _jsRuntime;

    public LogJsonInterop(IJSRuntime jSRuntime)
    {
        _jsRuntime = jSRuntime;
    }

    public ValueTask LogJson(string? jsonString, string group, string logLevel = "log")
    {
        return _jsRuntime.InvokeVoidAsync("logJson", jsonString, group, logLevel);
    }

    public async ValueTask LogRequest(string requestUri, HttpContent? httpContent = null, HttpMethod? httpMethod = null)
    {
        string? contentString = null;

        if (httpContent != null)
            contentString = await httpContent.ReadAsStringAsync();

        var groupString = "Request: ";

        if (httpMethod != null)
            groupString += httpMethod.ToString();

        groupString += requestUri;

        await LogJson(contentString, groupString);
    }

    public async ValueTask LogResponse(HttpResponseMessage response)
    {
        var contentString = await response.Content.ReadAsStringAsync();
        
        await LogJson(contentString, $"Response: {response.RequestMessage?.RequestUri} ({response.StatusCode})");
    }
}