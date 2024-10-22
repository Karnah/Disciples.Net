﻿using AutoMapper;
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
        CreateMap<Resources.Database.Sqlite.Models.UnitType, UnitType>()
            .ForMember(dst => dst.PreviousUnitType, opt => opt.Ignore())
            .ForMember(dst => dst.RaceType, opt => opt.MapFrom(src => src.Race.RaceType))
            .ForMember(dst => dst.RecruitBuildingId, opt => opt.Ignore())
            .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.Name.Text))
            .ForMember(dst => dst.Description, opt => opt.MapFrom(src => src.Description.Text))
            .ForMember(dst => dst.Ability, opt => opt.MapFrom(src => src.AbilityDescription.Text))
            .ForMember(dst => dst.UpgradeBuildingId, opt => opt.Ignore())
            .ForMember(dst => dst.LeaderBaseUnitType, opt => opt.Ignore())
            ;
    }
}