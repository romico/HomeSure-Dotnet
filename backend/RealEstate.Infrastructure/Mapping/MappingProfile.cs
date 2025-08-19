using AutoMapper;
using RealEstate.Core.DTOs;
using RealEstate.Core.Entities;

namespace RealEstate.Infrastructure.Mapping;

/// <summary>
/// AutoMapper의 매핑 구성을 정의합니다.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>();
    }
}