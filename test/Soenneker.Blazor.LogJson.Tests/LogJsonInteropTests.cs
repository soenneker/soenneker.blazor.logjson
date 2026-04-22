using Soenneker.Blazor.LogJson.Abstract;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.Blazor.LogJson.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public class FilePondInteropTests : HostedUnitTest
{
    private readonly ILogJsonInterop _util;

    public FilePondInteropTests(Host host) : base(host)
    {
        _util = Resolve<ILogJsonInterop>(true);
    }

    [Test]
    public void Default()
    {

    }
}
