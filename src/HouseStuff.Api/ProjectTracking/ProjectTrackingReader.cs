using System.Text.Json;

namespace HouseStuff.Api.ProjectTracking;

internal sealed class ProjectTrackingReader : IProjectTrackingReader
{
    private readonly string documentPath = Path.Combine(AppContext.BaseDirectory, "Tracking", "project.json");

    public async Task<ProjectTrackingDocument> ReadAsync(CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(documentPath);
        return await JsonSerializer.DeserializeAsync(
            stream,
            ProjectTrackingJsonContext.Default.ProjectTrackingDocument,
            cancellationToken) ?? throw new InvalidDataException("O acompanhamento do projeto está vazio.");
    }
}
