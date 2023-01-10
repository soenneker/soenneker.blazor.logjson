using System.Net.Http;
using System.Threading.Tasks;

namespace Soenneker.Blazor.LogJson.Abstract;

public interface ILogJsonInterop 
{
    ValueTask LogJson(string? jsonString, string group, string logLevel = "log");

    ValueTask LogRequest(string requestUri, HttpContent? httpContent = null);

    ValueTask LogResponse(HttpResponseMessage response);
}