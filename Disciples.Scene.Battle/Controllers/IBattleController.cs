using Disciples.Engine.Base;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;
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
        /// Текущее состояние битвы.
        /// </summary>
        BattleState BattleState { get; }

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
        /// Событие возникает, когда юнит начинает действие.
        /// </summary>
        event EventHandler<UnitActionBeginEventArgs> UnitActionBegin;

        /// <summary>
        /// Событие возникает, когда следующий юнит готов к ходу.
        /// </summary>
        event EventHandler UnitActionEnded;

        /// <summary>
        /// Событие возникает, когда один из отрядов полностью уничтожен.
        /// </summary>
        event EventHandler BattleEnded;


        /// <summary>
        /// Обновить состояние объектов на сцене.
        /// </summary>
        public void UpdateSceneState(long ticksCount);


        /// <summary>
        /// Получить игровой объект юнита.
        /// </summary>
        BattleUnit GetUnitObject(Unit unit);

        /// <summary>
        /// Проверить на возможность атаки юнита.
        /// </summary>
        bool CanAttack(BattleUnit targetUnitGameObject);


        /// <summary>
        /// Атаковать выбранного юнита.
        /// </summary>
        /// <param name="targetUnitGameObject">Юнит, являющийся целью.</param>
        /// <returns>True, если атака возможна, false если невозможно атаковать.</returns>
        bool AttackUnit(BattleUnit targetUnitGameObject);

        /// <summary>
        /// Защититься на этом ходу.
        /// </summary>
        void Defend();

        /// <summary>
        /// Подождать.
        /// </summary>
        void Wait();
    }
}