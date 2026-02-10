using AutoMapper;
using TaskManager.Api.Contracts.Tasks;
using TaskManager.Application.Dtos.Tasks;

namespace TaskManager.Api.Mappers.Tasks;

public class TaskRequestMappingProfile : Profile
{
    public TaskRequestMappingProfile()
    {
        CreateMap<CreateTaskRequest, CreateTaskDto>();
        CreateMap<UpdateTaskRequest, UpdateTaskDto>();
    }
}
