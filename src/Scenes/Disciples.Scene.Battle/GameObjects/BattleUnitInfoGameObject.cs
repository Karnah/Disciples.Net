using Disciples.Engine.Base;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Common.SceneObjects;
using Disciples.Engine.Models;

namespace Disciples.Scene.Battle.GameObjects;

/// <summary>
/// Игровой объект, который выводит текстовую информацию о юните.
/// </summary>
internal class BattleUnitInfoGameObject : GameObject
{
    private readonly ISceneObjectContainer _sceneObjectContainer;
    private readonly ITextProvider _textProvider;
    private readonly int _layer;

    private ITextSceneObject? _unitInfoText;
    private Unit? _lastUnit;
    private int? _lastHitpoints;

    /// <inheritdoc />
    public BattleUnitInfoGameObject(ISceneObjectContainer sceneObjectContainer, ITextProvider textProvider, double x, double y, int layer) : base(x, y)
    {
        _sceneObjectContainer = sceneObjectContainer;
        _textProvider = textProvider;
        _layer = layer;

        Width = 120;
        Height = 40;
    }

    /// <summary>
    /// Юнит, информация о котором отображается.
    /// </summary>
    public Unit? Unit { get; set; }

    /// <inheritdoc />
    public override void Update(long ticksCount)
    {
        base.Update(ticksCount);

        // Удаляем текст со сцены.
        if (Unit == null)
        {
            if (_unitInfoText != null)
            {
                _sceneObjectContainer.RemoveSceneObject(_unitInfoText);
                _unitInfoText = null;
            }

            return;
        }

        // Обновляем текст, если изменился юнит или его здоровье.
        if (_lastUnit != Unit || _lastHitpoints != Unit.HitPoints)
        {
            _sceneObjectContainer.RemoveSceneObject(_unitInfoText);

            _unitInfoText = _sceneObjectContainer.AddText(GetUnitInfoText(Unit), Width, Height, X, Y, _layer);
            _lastUnit = Unit;
            _lastHitpoints = Unit.HitPoints;
        }
    }

    /// <inheritdoc />
    public override void Destroy()
    {
        base.Destroy();

        _sceneObjectContainer.RemoveSceneObject(_unitInfoText);
        _unitInfoText = null;
    }

    /// <summary>
    /// Получить тестовое описание юнита.
    /// </summary>
    private TextContainer GetUnitInfoText(Unit unit)
    {
        return _textProvider
            .GetText("X100TA0608")
            .ReplacePlaceholders(new[]
            {
                ("%NAME%", new TextContainer(unit.Name)),
                ("%HP%", new TextContainer(unit.HitPoints.ToString())),
                ("%HPMAX%", new TextContainer(unit.MaxHitPoints.ToString())),
            });
    }
}