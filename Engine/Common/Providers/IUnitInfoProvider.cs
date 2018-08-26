using Engine.Common.Models;

namespace Engine.Common.Providers
{
    public interface IUnitInfoProvider
    {
        UnitType GetUnitType(string unitTypeId);
    }
}
