using System.Collections.Generic;
using Avalonia.Media.Imaging;
using Engine.Battle.GameObjects;

namespace Engine.Battle.Contollers
{
    /// <summary>
    /// Класс для взаимодействия с интерфейсом пользователя.
    /// </summary>
    public interface IBattleInterfaceController
    {
        /// <summary>
        /// Картинка заднего фона поля боя.
        /// </summary>
        Bitmap Battleground { get; }

        /// <summary>
        /// Картинка, позволяющая отобразить больших юнитов на панели (скрывает разделитель между ячейками).
        /// </summary>
        Bitmap PanelSeparator { get; }

        /// <summary>
        /// Картинка с черепом для отображения мёртвых юнитов.
        /// </summary>
        Bitmap DeathSkull { get; }

        /// <summary>
        /// Список юнитов, чьи портреты отображаются на панели справа.
        /// </summary>
        IReadOnlyCollection<BattleUnit> RightPanelUnits { get; }


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
    }
}
