using Disciples.Scene.Battle.GameObjects;

namespace Disciples.Scene.Battle.Models.BattleActions;

/// <summary>
/// Действие атаки одного юнита на другого.
/// </summary>
internal class MainAttackBattleAction : AnimationBattleAction
{
    /// <inheritdoc />
    public MainAttackBattleAction(BattleUnit attacker, BattleUnit target)
        : base(attacker.AnimationComponent, CalculateEndFrameIndex(attacker))
    {
        Attacker = attacker;
        Target = target;
    }

    /// <summary>
    /// Юнит, который атаковал.
    /// </summary>
    public BattleUnit Attacker { get; }

    /// <summary>
    /// Юнит, который являлся целью атаки.
    /// </summary>
    public BattleUnit Target { get; }

    /// <summary>
    /// Вычислить индекс завершения анимации.
    /// </summary>
    /// <remarks>
    /// Этот метод нужен только потому, что я не смог найти связь между анимациями и моментом нанесения удара.
    /// </remarks>
    private static int CalculateEndFrameIndex(BattleUnit unit)
    {
        // Обязательно обновляем состояние компонента, чтобы он был в актуальном состоянии.
        var animationComponent = unit.AnimationComponent;
        animationComponent.Update(0);

        var framesCount = animationComponent.FramesCount;

        return unit.Unit.UnitType.Id switch
        {
            // Ассасин Империи, 41 фрейм.
            "G000UU0154" => 12,
            // Рыцарь, 31 фрейм.
            "G000UU0002" => 12,
            // Страж Горна, 31 фрейм.
            "G000UU0162" => 26,
            // Арбалетчик, 26 фреймов.
            "G000UU0027" => 5,
            // Мастер рун, 30 фреймов.
            "G000UU0165" => 10,
            _ => framesCount - 12
        };
    }
}