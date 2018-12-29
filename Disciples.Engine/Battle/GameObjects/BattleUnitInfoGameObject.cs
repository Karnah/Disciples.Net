using System;

using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.SceneObjects;

namespace Disciples.Engine.Battle.GameObjects
{
    /// <summary>
    /// Игровой объект, который выводит текстовую информацию о юните.
    /// </summary>
    public class BattleUnitInfoGameObject : GameObject
    {
        private readonly IVisualSceneController _visualSceneController;
        private readonly int _layer;

        private ITextSceneObject _unitInfoText;
        private Unit _lastUnit;
        private int? _lastHitpoints;

        /// <inheritdoc />
        public BattleUnitInfoGameObject(IVisualSceneController visualSceneController, double x, double y, int layer) : base(x, y)
        {
            _visualSceneController = visualSceneController;
            _layer = layer;

            Width = 120;
            Height = 40;
        }

        /// <summary>
        /// Юнит, информация о котором отображается.
        /// </summary>
        public Unit Unit { get; set; }

        /// <inheritdoc />
        public override bool IsInteractive => false;

        /// <inheritdoc />
        public override void OnUpdate(long ticksCount)
        {
            base.OnUpdate(ticksCount);

            // Удаляем текст со сцены.
            if (Unit == null) {
                if (_unitInfoText != null) {
                    _visualSceneController.RemoveSceneObject(_unitInfoText);
                    _unitInfoText = null;
                }

                return;
            }

            // Обновляем текст, если изменился юнит или его здоровье.
            if (_lastUnit != Unit || _lastHitpoints != Unit.HitPoints) {
                _visualSceneController.RemoveSceneObject(_unitInfoText);

                _unitInfoText = _visualSceneController.AddText(GetUnitNameAndHitPoints(Unit), 14, X, Y, _layer, true);
                _unitInfoText.Width = Width;
                _unitInfoText.Height = Height;

                _lastUnit = Unit;
                _lastHitpoints = Unit.HitPoints;
            }
        }

        /// <inheritdoc />
        public override void Destroy()
        {
            base.Destroy();

            _visualSceneController.RemoveSceneObject(_unitInfoText);
            _unitInfoText = null;
        }

        private static string GetUnitNameAndHitPoints(Unit unit)
        {
            return $"{unit.UnitType.Name}{Environment.NewLine}" +
                   $"ОЗ : {unit.HitPoints}/{unit.UnitType.HitPoints}";
        }
    }
}