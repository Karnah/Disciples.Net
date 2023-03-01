using Disciples.Engine.Base;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;

namespace Disciples.Scene.Battle.Controllers
{
    /// <summary>
    /// Класс для мониторинга и управлением состояния битвы.
    /// </summary>
    public interface IBattleController : ISupportLoadingWithParameters<BattleSquadsData>
    {
        /// <summary>
        /// Юнит, который сейчас ходит.
        /// </summary>
        BattleUnit CurrentUnitObject { get; }

        /// <summary>
        /// Признак того, что юнит атакует второй раз за текущий ход.
        /// </summary>
        /// <remarks>Актуально только для юнитов, которые бьют дважды за ход.</remarks>
        bool IsSecondAttack { get; }

        /// <summary>
        /// Все юниты.
        /// </summary>
        IReadOnlyList<BattleUnit> Units { get; }

        /// <summary>
        /// Обновить состояние объектов на сцене.
        /// </summary>
        public void BeforeSceneUpdate(BattleUpdateContext context);

        /// <summary>
        /// Обновить состояние объектов на сцене.
        /// </summary>
        public void AfterSceneUpdate(BattleUpdateContext context);


        /// <summary>
        /// Получить игровой объект юнита.
        /// </summary>
        BattleUnit GetUnitObject(Unit unit);

        /// <summary>
        /// Проверить на возможность атаки юнита.
        /// </summary>
        bool CanAttack(BattleUnit targetUnitGameObject);
    }
}