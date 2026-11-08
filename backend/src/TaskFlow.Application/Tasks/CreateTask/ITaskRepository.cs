using TaskFlow.Domain.Tasks;

namespace TaskFlow.Application.Tasks.CreateTask;

public interface ITaskRepository
{
    Task AddAsync(TaskItem task, CancellationToken cancellationToken);
}
