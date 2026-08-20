using System.Text.Json;
using System.Text.Json.Serialization;

namespace HouseStuff.Api.ProjectTracking;

public sealed record ProjectTrackingDocument(
    string Project,
    DateOnly UpdatedAt,
    string Status,
    string CurrentStageId,
    IReadOnlyList<ProjectStage> Stages,
    IReadOnlyList<ProjectTask> Tasks);

public sealed record ProjectStage(string Id, string Name, string Outcome);

public sealed record ProjectTask(
    string Id,
    string StageId,
    string Area,
    string Repository,
    string Type,
    string Title,
    string Status,
    string Result,
    IReadOnlyList<ProjectSubtask> Subtasks);

public sealed record ProjectSubtask(string Id, string Title, string Status);

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(ProjectTrackingDocument))]
internal sealed partial class ProjectTrackingJsonContext : JsonSerializerContext;
