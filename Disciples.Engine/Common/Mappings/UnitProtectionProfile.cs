using AutoMapper;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Common.Mappings;

/// <summary>
/// Профиль маппинга для <see cref="UnitAttackSourceProtection" /> и <see cref="UnitAttackTypeProtection" />.
/// </summary>
internal class UnitProtectionProfile : Profile
{
    /// <summary>
    /// Создать объект типа <see cref="UnitProtectionProfile" />.
    /// </summary>
    public UnitProtectionProfile()
    {
        CreateMap<Resources.Database.Models.UnitAttackSourceProtection, UnitAttackSourceProtection>();

        CreateMap<Resources.Database.Models.UnitAttackTypeProtection, UnitAttackTypeProtection>();
    }
}