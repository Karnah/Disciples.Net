using Disciples.Engine.Battle.GameObjects;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Battle.Contollers
{
    /// <summary>
    /// Класс для взаимодействия с интерфейсом пользователя.
    /// </summary>
    public interface IBattleInterfaceController
    {
        /// <summary>
        /// Расположить все объекты интерфейса на сцене.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Обновить цель.
        /// </summary>
        /// <param name="targetUnitObject">Юнит, на которого навели курсором.</param>
        /// <param name="animateTarget">Необходимо ли выделить юнита с помощью анимации (красный крутящийся круг).</param>
        void UpdateTargetUnit(BattleUnit targetUnitObject, bool animateTarget = true);

        /// <summary>
        /// Отобразить детальную информацию по указанному юниту.
        /// </summary>
        /// <param name="unit">Юнит, информацию о котором необходимо отобразить.</param>
        void ShowDetailUnitInfo(Unit unit);

        /// <summary>
        /// Прекратить отображение детальной информации по юниту.
        /// </summary>
        void StopShowDetailUnitInfo();
    }
}