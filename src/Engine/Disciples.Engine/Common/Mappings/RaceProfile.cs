using AutoMapper;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Common.Mappings;

/// <summary>
/// Профиль маппинга для <see cref="Race" />
/// </summary>
internal class RaceProfile : Profile
{
    /// <summary>
    /// Создать объект типа <see cref="RaceProfile" />.
    /// </summary>
    public RaceProfile()
    {
        CreateMap<Resources.Database.Sqlite.Models.Race, Race>()
            .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.Name.Text))
            ;
    }
}