using AutoMapper;
using TaskManager.Application.Dtos.Tasks;

namespace TaskManager.Application.Mappers.Tasks
{
    public class TaskMappingProfile : Profile
    {
        public TaskMappingProfile()
        {
            CreateMap<TaskItem, TaskDto>();
            CreateMap<CreateTaskDto, TaskItem>();
            CreateMap<UpdateTaskDto, TaskItem>();
        }
    }
}
