using AutoMapper;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Common.Mappings;

/// <summary>
/// Профиль маппинга для <see cref="UnitAttack" />
/// </summary>
internal class UnitAttackProfile : Profile
{
    /// <summary>
    /// Создать объект типа <see cref="UnitAttackProfile" />.
    /// </summary>
    public UnitAttackProfile()
    {
        CreateMap<Resources.Database.Sqlite.Models.UnitAttack, UnitAttack>()
            .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.Name.Text))
            .ForMember(dst => dst.Description, opt => opt.MapFrom(src => src.Description.Text))
            .ForMember(dst => dst.Ward1, opt => opt.Ignore())
            .ForMember(dst => dst.Ward2, opt => opt.Ignore())
            .ForMember(dst => dst.Ward3, opt => opt.Ignore())
            .ForMember(dst => dst.Ward4, opt => opt.Ignore())
            ;
    }
}