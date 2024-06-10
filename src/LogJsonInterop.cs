using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Soenneker.Blazor.LogJson.Abstract;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;

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
            contentString = await httpContent.ReadAsStringAsync().NoSync();

        var groupStringBuilder = new StringBuilder("Request: ");

        if (httpMethod != null)
            groupStringBuilder.Append(httpMethod).Append(' ');

        groupStringBuilder.Append(requestUri);

        await LogJson(contentString, groupStringBuilder.ToString()).NoSync();
    }

    public async ValueTask LogResponse(HttpResponseMessage response)
    {
        var contentString = await response.Content.ReadAsStringAsync().NoSync();

        var groupStringBuilder = new StringBuilder("Response: ");

        if (response.RequestMessage != null)
        {
            groupStringBuilder.Append(response.RequestMessage.Method)
                .Append(' ')
                .Append(response.RequestMessage.RequestUri)
                .Append(' ');
        }

        groupStringBuilder.Append('(').Append(response.StatusCode).Append(')');

        await LogJson(contentString, groupStringBuilder.ToString()).NoSync();
    }
}