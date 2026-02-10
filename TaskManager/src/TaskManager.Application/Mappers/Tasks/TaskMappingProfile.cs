using AutoMapper;
using TaskManager.Application.Dtos.Tasks;
using TaskManager.Domain.Models.Tasks;

namespace TaskManager.Application.Mappers.Tasks;

public class TaskMappingProfile : Profile
{
    public TaskMappingProfile()
    {
        CreateMap<TaskItem, TaskDto>();
        CreateMap<CreateTaskDto, TaskItem>();
        CreateMap<UpdateTaskDto, TaskItem>();
    }
}
