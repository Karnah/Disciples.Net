using AutoMapper;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Common.Mappings;

/// <summary>
/// Профиль маппинга для <see cref="UnitLevelUpgrade" />
/// </summary>
internal class UnitLevelUpgradeProfile : Profile
{
    /// <summary>
    /// Создать объект типа <see cref="UnitLevelUpgradeProfile" />.
    /// </summary>
    public UnitLevelUpgradeProfile()
    {
        CreateMap<Resources.Database.Sqlite.Models.UnitLevelUpgrade, UnitLevelUpgrade>();
    }
}