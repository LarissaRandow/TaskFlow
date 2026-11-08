using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Tasks.Contracts;
using TaskFlow.Application.Tasks.CreateTask;
using TaskFlow.Application.Tasks.GetTasks;

namespace TaskFlow.Api.Tasks;

[ApiController]
[Authorize]
[Route("api/tasks")]
public sealed class TasksController(
    ITaskCreationService taskCreationService,
    ITaskListingService taskListingService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaskResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue("sub"), out var userId) || userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var tasks = await taskListingService.GetByUserIdAsync(userId, cancellationToken);
        return Ok(tasks.Select(TaskResponse.From).ToArray());
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TaskResponse>> Create(
        [FromBody] CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue("sub"), out var userId) || userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var result = await taskCreationService.CreateAsync(
            request.Title,
            request.Description,
            request.DueDate,
            userId,
            cancellationToken);

        if (result.Status == CreateTaskStatus.Invalid)
        {
            return BadRequest(new ValidationProblemDetails(
                result.Errors.ToDictionary(error => error.Key, error => error.Value))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred."
            });
        }

        if (result.Task is null)
        {
            throw new InvalidOperationException("The task creation service returned an inconsistent result.");
        }

        var response = TaskResponse.From(result.Task);
        return Created($"/api/tasks/{response.Id}", response);
    }
}
