using AutoMapper;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Common.Mappings;

/// <summary>
/// Профиль маппинга для <see cref="UnitType" />
/// </summary>
internal class UnitTypeProfile : Profile
{
    /// <summary>
    /// Создать объект типа <see cref="UnitTypeProfile" />.
    /// </summary>
    public UnitTypeProfile()
    {
        CreateMap<Resources.Database.Models.UnitType, UnitType>()
            .ForMember(dst => dst.PreviousUnitType, opt => opt.Ignore())
            .ForMember(dst => dst.RaceId, opt => opt.Ignore())
            .ForMember(dst => dst.RecruitBuildingId, opt => opt.Ignore())
            .ForMember(dst => dst.Name, opt => opt.Ignore())
            .ForMember(dst => dst.Description, opt => opt.Ignore())
            .ForMember(dst => dst.Ability, opt => opt.Ignore())
            .ForMember(dst => dst.MainAttack, opt => opt.Ignore())
            .ForMember(dst => dst.SecondaryAttack, opt => opt.Ignore())
            .ForMember(dst => dst.LeaderBaseUnit, opt => opt.Ignore())
            .ForMember(dst => dst.UpgradeBuildingId, opt => opt.Ignore())
            .ForMember(dst => dst.LowLevelUpgradeId, opt => opt.Ignore())
            .ForMember(dst => dst.HighLevelUpgradeId, opt => opt.Ignore())
            ;
    }
}