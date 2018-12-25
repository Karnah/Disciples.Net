using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Common.Providers
{
    public interface IUnitInfoProvider
    {
        UnitType GetUnitType(string unitTypeId);
    }
}
