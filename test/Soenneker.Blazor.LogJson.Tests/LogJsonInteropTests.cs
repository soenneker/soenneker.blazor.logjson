using Soenneker.Blazor.LogJson.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;

namespace Soenneker.Blazor.LogJson.Tests;

[Collection("Collection")]
public class FilePondInteropTests : FixturedUnitTest
{
    private readonly ILogJsonInterop _util;

    public FilePondInteropTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<ILogJsonInterop>(true);
    }

    [Fact]
    public void Default()
    {

    }
}