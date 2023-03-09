using AutoMapper;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Common.Mappings;

/// <summary>
/// Профиль маппинга для <see cref="ResourceCost" />
/// </summary>
internal class ResourceCostProfile : Profile
{
    /// <summary>
    /// Создать объект типа <see cref="ResourceCostProfile" />.
    /// </summary>
    public ResourceCostProfile()
    {
        CreateMap<Resources.Database.Components.ResourceCost, ResourceCost>();
    }
}