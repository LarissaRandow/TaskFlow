using TaskFlow.Domain.Tasks;

namespace TaskFlow.Application.Tasks.GetTasks;

public interface ITaskListingService
{
    Task<IReadOnlyList<TaskItem>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken);
}
