using Disciples.Common.Models;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Components;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.GameObjects;
using Disciples.Scene.Battle.Constants;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle.GameObjects;

/// <summary>
/// Плейсхолдер для вызываемых юнитов.
/// Используется на ходу юнита-вызывателя, чтобы игрок мог указать в какое место размещать юнита.
/// </summary>
internal class SummonPlaceholder : GameObject
{
    /// <summary>
    /// Создать объект типа <see cref="SummonPlaceholder" />.
    /// </summary>
    public SummonPlaceholder(ISceneObjectContainer sceneObjectContainer,
        IBattleUnitResourceProvider battleUnitResourceProvider,
        Action<SummonPlaceholder> onPlaceholderMouseLeftButtonClicked,
        Action<SummonPlaceholder> onUnitMouseRightButtonClicked,
        BattleUnitPosition position,
        RectangleD bounds
        ) : base(bounds)
    {
        Position = position;

        AnimationComponent = new AnimationComponent(this, sceneObjectContainer, battleUnitResourceProvider.SummonPlaceholderAnimationFrames,
            BattleLayers.UNIT_SELECTION_ANIMATION_LAYER, BattleUnit.SmallBattleUnitSelectionAnimationOffset);
        Components = new IComponent[]
        {
            AnimationComponent,
            new SelectionComponent(this),
            new MouseLeftButtonClickComponent(this, Array.Empty<KeyboardButton>(), onClickedAction: () => onPlaceholderMouseLeftButtonClicked.Invoke(this)),
            new MouseRightButtonClickComponent(this, () => onUnitMouseRightButtonClicked.Invoke(this))
        };

        Height = BattleUnit.SMALL_BATTLE_UNIT_HEIGHT;
    }

    /// <summary>
    /// Компонент анимации призыва юнита.
    /// </summary>
    public AnimationComponent AnimationComponent { get; }

    /// <summary>
    /// Позиция, куда будет призван юнит.
    /// </summary>
    public BattleUnitPosition Position { get; }
}
