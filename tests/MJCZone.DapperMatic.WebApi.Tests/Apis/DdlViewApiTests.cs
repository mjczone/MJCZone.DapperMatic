using System.Net;
using MJCZone.DapperMatic.WebApi.HandlerTypes;
using MJCZone.DapperMatic.WebApi.Options;
using Xunit.Abstractions;

namespace MJCZone.DapperMatic.WebApi.Tests.Apis;

public class DdlViewApiTests : DdlApiTestsBase
{
    public DdlViewApiTests(WebApiTestFactory factory, ITestOutputHelper output)
        : base(factory, output) { }
}
