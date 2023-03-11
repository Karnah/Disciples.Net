using AutoMapper;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Common.Mappings;

/// <summary>
/// Профиль маппинга для <see cref="ResourceSet" />
/// </summary>
internal class ResourceSetProfile : Profile
{
    /// <summary>
    /// Создать объект типа <see cref="ResourceSetProfile" />.
    /// </summary>
    public ResourceSetProfile()
    {
        CreateMap<Resources.Database.Components.ResourceSet, ResourceSet>();
    }
}