using Engine.Models;

namespace Engine.Interfaces
{
    public interface IUnitInfoProvider
    {
        UnitType GetUnitType(string unitTypeId);
    }
}
