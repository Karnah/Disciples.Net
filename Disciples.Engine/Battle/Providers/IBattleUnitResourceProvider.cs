using Disciples.Engine.Base;
using Disciples.Engine.Battle.Enums;
using Disciples.Engine.Battle.Models;

namespace Disciples.Engine.Battle.Providers
{
    /// <summary>
    /// Поставщик ресурсов для юнитов на поле боя.
    /// </summary>
    public interface IBattleUnitResourceProvider : ISupportLoading
    {
        /// <summary>
        /// Получить набор анимаций юнита.
        /// </summary>
        /// <param name="unitId">Идентификатор типа юнита.</param>
        /// <param name="direction">Направление положения юнита.</param>
        BattleUnitAnimation GetBattleUnitAnimation(string unitId, BattleDirection direction);
    }
}