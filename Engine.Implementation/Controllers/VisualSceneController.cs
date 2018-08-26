using System;
using System.Collections.Generic;

using Avalonia.Media.Imaging;

using Engine.Battle.GameObjects;
using Engine.Battle.Providers;
using Engine.Common.Controllers;
using Engine.Common.Enums;
using Engine.Common.GameObjects;
using Engine.Common.Models;

namespace Engine.Implementation.Controllers
{
    public class VisualSceneController : IVisualSceneController
    {
        private readonly IGame _game;
        private readonly IMapVisual _mapVisual;
        private readonly IBattleUnitResourceProvider _battleUnitResourceProvider;

        public VisualSceneController(IGame game, IMapVisual mapVisual, IBattleUnitResourceProvider battleUnitResourceProvider)
        {
            _game = game;
            _mapVisual = mapVisual;
            _battleUnitResourceProvider = battleUnitResourceProvider;
        }


        public AnimationObject AddAnimation(IReadOnlyList<Frame> frames, double x, double y, int layer, bool repeat = true)
        {
            var animation = new AnimationObject(_mapVisual, frames, x, y, layer, repeat);
            _game.CreateObject(animation);

            return animation;
        }

        public ButtonObject AddButton(IDictionary<ButtonState, Bitmap> buttonStates, Action buttonPressedAction, double x, double y, int layer)
        {
            var button = new ButtonObject(_mapVisual, buttonStates, buttonPressedAction, x, y, layer);
            _game.CreateObject(button);

            return button;
        }

        public ToggleButtonObject AddToggleButton(IDictionary<ButtonState, Bitmap> buttonStates, Action buttonPressedAction, double x, double y, int layer)
        {
            var toggleButton = new ToggleButtonObject(_mapVisual, buttonStates, buttonPressedAction, x, y, layer);
            _game.CreateObject(toggleButton);

            return toggleButton;
        }

        public VisualObject AddVisual(Bitmap bitmap, int x, int y, int layer)
        {
            return AddVisual(bitmap, bitmap.PixelWidth, bitmap.PixelHeight, x, y, layer);
        }

        public VisualObject AddVisual(Bitmap bitmap, int width, int height, int x, int y, int layer)
        {
            var visual = new VisualObject(null, layer) {
                X = x,
                Y = y,
                Width = width,
                Height = height,
                Bitmap = bitmap
            };

            _mapVisual.AddVisual(visual);
            return visual;
        }


        public BattleUnit AddBattleUnit(Unit unit, bool isAttacker)
        {
            var battleUnit = new BattleUnit(_mapVisual, _battleUnitResourceProvider, unit, isAttacker);
            _game.CreateObject(battleUnit);

            return battleUnit;
        }
    }
}
