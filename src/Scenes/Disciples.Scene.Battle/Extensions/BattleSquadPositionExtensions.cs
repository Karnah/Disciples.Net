using Disciples.Scene.Battle.Enums;

namespace Disciples.Scene.Battle.Extensions;

/// <summary>
/// Расширения для <see cref="BattleSquadPosition" />.
/// </summary>
internal static class BattleSquadPositionExtensions
{
    /// <summary>
    /// Получить направление, обратное указанному.
    /// </summary>
    /// <param name="squadPosition">Направление.</param>
    /// <returns>Обратное направление.</returns>
    public static BattleSquadPosition GetOpposite(this BattleSquadPosition squadPosition)
    {
        return squadPosition == BattleSquadPosition.Attacker
            ? BattleSquadPosition.Defender
            : BattleSquadPosition.Attacker;
    }
}