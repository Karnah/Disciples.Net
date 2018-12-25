using Disciples.Engine.Battle.Enums;
using Disciples.Engine.Battle.Models;

namespace Disciples.Engine.Battle.Providers
{
    public interface IBattleUnitResourceProvider
    {
        BattleUnitAnimation GetBattleUnitAnimation(string unitId, BattleDirection direction);
    }
}
