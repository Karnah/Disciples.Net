using System.Collections.Generic;

using Avalonia.Media.Imaging;

using Engine.Battle.Enums;
using Engine.Common.Enums;
using Engine.Common.Models;

namespace Engine.Battle.Providers
{
    /// <summary>
    /// Провайдер для основных изображений на поле битвы.
    /// </summary>
    public interface IBattleInterfaceProvider
    {
        /// <summary>
        /// Фон поля боя.
        /// </summary>
        Bitmap Battleground { get; }

        /// <summary>
        /// Картинка правой панели с юнитами.
        /// </summary>
        Bitmap RightPanel { get; }

        /// <summary>
        /// Картинка нижней панели.
        /// </summary>
        Bitmap BottomPanel { get; }

        /// <summary>
        /// Картинка-разделитель для панели юнитов.
        /// </summary>
        Bitmap PanelSeparator { get; }

        /// <summary>
        /// Иконка умершего юнита.
        /// </summary>
        Bitmap DeathSkull { get; }


        /// <summary>
        /// Иконки для эффектов, воздействующих на юнита.
        /// </summary>
        IDictionary<UnitBattleEffectType, Bitmap> UnitButtleEffectsIcon { get; }


        /// <summary>
        /// Иконки для кнопки переключения правой панели юнитов.
        /// </summary>
        IDictionary<ButtonState, Bitmap> ToggleRightButton { get; }

        /// <summary>
        /// Иконки для кнопки защиты.
        /// </summary>
        IDictionary<ButtonState, Bitmap> DefendButton { get; }

        /// <summary>
        /// Иконки для кнопки отступления.
        /// </summary>
        IDictionary<ButtonState, Bitmap> RetreatButton { get; }

        /// <summary>
        /// Иконки для кнопки ожидания.
        /// </summary>
        IDictionary<ButtonState, Bitmap> WaitButton { get; }

        /// <summary>
        /// Иконки для мгновенного завершения битвы.
        /// </summary>
        IDictionary<ButtonState, Bitmap> InstantResolveButton { get; }

        /// <summary>
        /// Иконки для автоматической битвы.
        /// </summary>
        IDictionary<ButtonState, Bitmap> AutoBattleButton { get; }


        /// <summary>
        /// Получить изображение указанного цвета.
        /// </summary>
        Bitmap GetColorBitmap(GameColor color);


        /// <summary>
        /// Получить анимацию рамки для юнита, которого можно атаковать.
        /// </summary>
        /// <param name="sizeSmall">Юнит занимает только одну клетку.</param>
        IReadOnlyList<Frame> GetUnitAttackBorder(bool sizeSmall);

        /// <summary>
        /// Получить анимацию рамки атаки для юнита, который атакует всё поле боя.
        /// </summary>
        IReadOnlyList<Frame> GetFieldAttackBorder();


        /// <summary>
        /// Получить анимацию рамки для юнита, который ходит в данный момент.
        /// </summary>
        /// <param name="sizeSmall">Юнит занимает только одну клетку.</param>
        IReadOnlyList<Frame> GetUnitSelectionBorder(bool sizeSmall);


        /// <summary>
        /// Получить анимацию исцеления для юнита, на которого можно наложить эффект.
        /// </summary>
        /// <param name="sizeSmall">Юнит занимает только одну клетку.</param>
        IReadOnlyList<Frame> GetUnitHealBorder(bool sizeSmall);

        /// <summary>
        /// Получить анимацию рамки исцеления для юнита, который накладывает эффект на всё поле боя.
        /// </summary>
        IReadOnlyList<Frame> GetFieldHealBorder();
    }
}
