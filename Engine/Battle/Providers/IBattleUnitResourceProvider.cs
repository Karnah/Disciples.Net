using Engine.Battle.Enums;
using Engine.Battle.Models;

namespace Engine.Battle.Providers
{
    public interface IBattleUnitResourceProvider
    {
        BattleUnitAnimation GetBattleUnitAnimation(string unitId, BattleDirection direction);
    }
}
