using AutoMapper;
using FluentValidation;
using TaskManager.Application.Dtos.Tasks;
using TaskManager.Application.Validation.Facades.Interfaces;
using TaskManager.Domain.Models.Tasks;
using TaskManager.Domain.Repositories;

namespace TaskManager.Application.Services.Implementations;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IMapper _mapper;
    private readonly IValidationService _validationService;

    public TaskService(
        ITaskRepository taskRepository,
        IMapper mapper,
        IValidationService validationService)
    {
        _taskRepository = taskRepository;
        _mapper = mapper;
        _validationService = validationService;
    }

    public async Task<IEnumerable<TaskDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tasks = await _taskRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<TaskDto>>(tasks);
    }

    public async Task<TaskDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, cancellationToken);
        return task == null ? null : _mapper.Map<TaskDto>(task);
    }

    public async Task<TaskDto> CreateAsync(CreateTaskDto dto, CancellationToken cancellationToken = default)
    {
        await _validationService.ValidateAsync(dto, cancellationToken);

        var task = _mapper.Map<TaskItem>(dto);
        var created = await _taskRepository.CreateAsync(task, cancellationToken);
        return _mapper.Map<TaskDto>(created);
    }

    public async Task<TaskDto?> UpdateAsync(string id, UpdateTaskDto dto, CancellationToken cancellationToken = default)
    {
        await _validationService.ValidateAsync(dto, cancellationToken);

        var existing = await _taskRepository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return null;

        _mapper.Map(dto, existing);

        var updated = await _taskRepository.UpdateAsync(existing, cancellationToken);
        return _mapper.Map<TaskDto>(updated);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _taskRepository.DeleteAsync(id, cancellationToken);
    }
}
