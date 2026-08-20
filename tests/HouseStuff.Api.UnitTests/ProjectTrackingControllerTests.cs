using HouseStuff.Api.Controllers;
using HouseStuff.Api.ProjectTracking;
using Microsoft.AspNetCore.Mvc;

namespace HouseStuff.Api.UnitTests;

public sealed class ProjectTrackingControllerTests
{
    [Fact]
    public async Task GetAsyncReturnsCanonicalDocument()
    {
        var expected = new ProjectTrackingDocument(
            "HouseStuff",
            new DateOnly(2026, 8, 20),
            "onTrack",
            "M1",
            [new ProjectStage("M0", "Fundação", "Base executável")],
            []);
        var controller = new ProjectTrackingController(new StubReader(expected));

        var result = await controller.GetAsync(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }

    private sealed class StubReader(ProjectTrackingDocument document) : IProjectTrackingReader
    {
        public Task<ProjectTrackingDocument> ReadAsync(CancellationToken cancellationToken) =>
            Task.FromResult(document);
    }
}
