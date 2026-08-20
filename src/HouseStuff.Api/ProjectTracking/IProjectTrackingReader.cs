namespace HouseStuff.Api.ProjectTracking;

public interface IProjectTrackingReader
{
    Task<ProjectTrackingDocument> ReadAsync(CancellationToken cancellationToken);
}
