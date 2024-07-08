using Disciples.Engine.Base;
using Disciples.Engine.Common.Components;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Common.SceneObjects;
using Disciples.Engine.Models;
using Disciples.Scene.Battle.Constants;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle.GameObjects;

/// <summary>
/// Портрет юнита, который располагается внизу экрана.
/// </summary>
internal class BottomUnitPortraitObject : GameObject
{
    private readonly ISceneObjectContainer _sceneObjectContainer;
    private readonly ITextProvider _textProvider;
    private readonly IBattleUnitResourceProvider _battleUnitResourceProvider;

    private readonly bool _isLeft;
    private readonly ImageSceneElement _portraitSceneElement;
    private readonly ImageSceneElement _leaderPanelSceneElement;
    private readonly TextBlockSceneElement _unitInfoSceneElement;

    private IImageSceneObject _portrait = null!;
    private IImageSceneObject _leaderPanel = null!;
    private ITextSceneObject _unitInfo = null!;

    private BattleUnit? _displayBattleUnit;
    private int? _displayHitPoints;

    /// <summary>
    /// Создать объект типа <see cref="BottomUnitPortraitObject" />.
    /// </summary>
    public BottomUnitPortraitObject(
        ISceneObjectContainer sceneObjectContainer,
        ITextProvider textProvider,
        IBattleUnitResourceProvider battleUnitResourceProvider,
        bool isLeft,
        SceneElement portraitSceneElement,
        SceneElement leaderPanelSceneElement,
        SceneElement unitInfoSceneElement,
        Action<BottomUnitPortraitObject> onBottomUnitPortraitMouseLeftButtonPressed
        ) : base(portraitSceneElement)
    {
        _sceneObjectContainer = sceneObjectContainer;
        _textProvider = textProvider;
        _battleUnitResourceProvider = battleUnitResourceProvider;
        _isLeft = isLeft;

        // В Interf.dlg не всегда правильно лежат плейсхолдеры (например, unitInfo там Image).
        // Поэтому просто конвертируем в нужный тип, сохраняя положение и имя.
        _portraitSceneElement = portraitSceneElement as ImageSceneElement
            ?? new ImageSceneElement { Name = portraitSceneElement.Name, Position = portraitSceneElement.Position };
        _leaderPanelSceneElement = leaderPanelSceneElement as ImageSceneElement
            ?? new ImageSceneElement() { Name = leaderPanelSceneElement.Name, Position = leaderPanelSceneElement.Position };
        _unitInfoSceneElement = unitInfoSceneElement as TextBlockSceneElement
            ?? new TextBlockSceneElement() { Name = unitInfoSceneElement.Name, Position = unitInfoSceneElement.Position };

        Components = new IComponent[]
        {
            new SelectionComponent(this, sceneObjectContainer),
            new MouseRightButtonClickComponent(this, () => onBottomUnitPortraitMouseLeftButtonPressed.Invoke(this))
        };
    }

    /// <summary>
    /// Юнит, который отображается на панели.
    /// </summary>
    public BattleUnit? BattleUnit { get; set; }

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        _portrait = _sceneObjectContainer.AddImage(_portraitSceneElement, BattleLayers.INTERFACE_LAYER + 1);
        _leaderPanel = _sceneObjectContainer.AddImage(_leaderPanelSceneElement, BattleLayers.INTERFACE_LAYER + 1);
        _unitInfo = _sceneObjectContainer.AddText(_unitInfoSceneElement, BattleLayers.INTERFACE_LAYER + 1);

        _portrait.IsReflected = !_isLeft;
        _leaderPanel.IsHidden = true;
    }

    /// <inheritdoc />
    public override void Update(long ticksCount)
    {
        base.Update(ticksCount);

        if (_displayBattleUnit != BattleUnit)
        {
            _displayBattleUnit = BattleUnit;

            if (_displayBattleUnit == null)
            {
                _portrait.Bitmap = null;
                _leaderPanel.IsHidden = true;
                // BUG: Конвертер в Avalonia не очищает объект, если задать null.
                _unitInfo.Text = new TextContainer(string.Empty);
                _displayHitPoints = null;
            }
            else
            {
                _portrait.Bitmap = _battleUnitResourceProvider.GetUnitBattleFace(_displayBattleUnit.Unit.UnitType);
                _leaderPanel.IsHidden = !_displayBattleUnit.Unit.IsLeader;
                UpdateUnitInfo();
            }
        }
        else if (_displayBattleUnit?.Unit.HitPoints != _displayHitPoints)
        {
            UpdateUnitInfo();
        }
    }

    /// <inheritdoc />
    public override void Destroy()
    {
        base.Destroy();

        _sceneObjectContainer.RemoveSceneObject(_portrait);
        _sceneObjectContainer.RemoveSceneObject(_leaderPanel);
        _sceneObjectContainer.RemoveSceneObject(_unitInfo);
    }

    /// <summary>
    /// Обновить информацию о юните.
    /// </summary>
    private void UpdateUnitInfo()
    {
        if (_displayBattleUnit == null)
            return;

        _unitInfo.Text = GetUnitInfoText(_displayBattleUnit.Unit);
        _displayHitPoints = _displayBattleUnit.Unit.HitPoints;
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