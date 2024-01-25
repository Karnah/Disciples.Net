using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using UnitModifierItemType = Disciples.Resources.Database.Sqlite.Enums.UnitModifierItemType;

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
            .ForMember(dst => dst.AttackTypeProtections, opt => opt.MapFrom(src =>
                GetProtections(src)
                    .Where(p => p.ModifierItemType == UnitModifierItemType.AttackTypeProtection)
                    .Select(p => new UnitAttackTypeProtection((UnitAttackType) p.ProtectionType!.Value, (ProtectionCategory)p.ProtectionCategory!.Value))))
            .ForMember(dst => dst.AttackSourceProtections, opt => opt.MapFrom(src =>
                GetProtections(src)
                    .Where(p => p.ModifierItemType == UnitModifierItemType.AttackSourceProtection)
                    .Select(p => new UnitAttackSourceProtection((UnitAttackSource) p.ProtectionType!.Value, (ProtectionCategory)p.ProtectionCategory!.Value))))
            ;
    }

    /// <summary>
    /// Получить список защит.
    /// </summary>
    private static IEnumerable<Resources.Database.Sqlite.Models.UnitModifierItem> GetProtections(Resources.Database.Sqlite.Models.UnitAttack unitAttack)
    {
        return Array.Empty<Resources.Database.Sqlite.Models.UnitModifierItem>()
            // Не учитываем WardsCount, предполагаем что ресурсы заполнены корректно.
            .Concat(unitAttack.Ward1?.ModifierItems ?? Array.Empty<Resources.Database.Sqlite.Models.UnitModifierItem>())
            .Concat(unitAttack.Ward2?.ModifierItems ?? Array.Empty<Resources.Database.Sqlite.Models.UnitModifierItem>())
            .Concat(unitAttack.Ward3?.ModifierItems ?? Array.Empty<Resources.Database.Sqlite.Models.UnitModifierItem>())
            .Concat(unitAttack.Ward4?.ModifierItems ?? Array.Empty<Resources.Database.Sqlite.Models.UnitModifierItem>());
    }
}