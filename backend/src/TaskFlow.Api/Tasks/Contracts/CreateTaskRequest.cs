using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TaskFlow.Api.Tasks.Contracts;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class CreateTaskRequest
{
    [Required]
    public string? Title { get; init; }

    public string? Description { get; init; }

    public DateTimeOffset? DueDate { get; init; }
}
