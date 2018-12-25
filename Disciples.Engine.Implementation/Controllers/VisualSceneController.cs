using System;
using System.Collections.Generic;

using Avalonia.Media;
using Avalonia.Media.Imaging;

using Disciples.Engine.Battle.GameObjects;
using Disciples.Engine.Battle.Providers;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Common.VisualObjects;

namespace Disciples.Engine.Implementation.Controllers
{
    /// <inheritdoc />
    public class VisualSceneController : IVisualSceneController
    {
        private readonly IGame _game;
        private readonly IMapVisual _mapVisual;
        private readonly ITextProvider _textProvider;
        private readonly IBattleUnitResourceProvider _battleUnitResourceProvider;
        private readonly IBattleInterfaceProvider _battleInterfaceProvider;

        public VisualSceneController(
            IGame game,
            IMapVisual mapVisual,
            ITextProvider textProvider,
            IBattleUnitResourceProvider battleUnitResourceProvider,
            IBattleInterfaceProvider battleInterfaceProvider)
        {
            _game = game;
            _mapVisual = mapVisual;
            _textProvider = textProvider;
            _battleUnitResourceProvider = battleUnitResourceProvider;
            _battleInterfaceProvider = battleInterfaceProvider;
        }


        /// <inheritdoc />
        public AnimationObject AddAnimation(IReadOnlyList<Frame> frames, double x, double y, int layer, bool repeat = true)
        {
            var animation = new AnimationObject(_mapVisual, frames, x, y, layer, repeat);
            _game.CreateObject(animation);

            return animation;
        }

        /// <inheritdoc />
        public AnimationObject AttachAnimation(IReadOnlyList<Frame> frames, GameObject gameObject, int layerOffset = 1, bool repeat = true)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ButtonObject AddButton(IDictionary<ButtonState, Bitmap> buttonStates, Action buttonPressedAction, double x, double y, int layer, KeyboardButton? hotkey = null)
        {
            var button = new ButtonObject(_mapVisual, buttonStates, buttonPressedAction, x, y, layer, hotkey);
            _game.CreateObject(button);

            return button;
        }

        /// <inheritdoc />
        public ToggleButtonObject AddToggleButton(IDictionary<ButtonState, Bitmap> buttonStates, Action buttonPressedAction, double x, double y, int layer, KeyboardButton? hotkey = null)
        {
            var toggleButton = new ToggleButtonObject(_mapVisual, buttonStates, buttonPressedAction, x, y, layer, hotkey);
            _game.CreateObject(toggleButton);

            return toggleButton;
        }

        /// <inheritdoc />
        public ImageVisualObject AddImageVisual(Bitmap bitmap, double x, double y, int layer)
        {
            return AddImageVisual(bitmap, bitmap.PixelSize.Width, bitmap.PixelSize.Height, x, y, layer);
        }

        /// <inheritdoc />
        public ImageVisualObject AddImageVisual(Bitmap bitmap, double width, double height, double x, double y, int layer)
        {
            var imageVisual = new ImageVisualObject(layer) {
                X = x,
                Y = y,
                Width = width,
                Height = height,
                Bitmap = bitmap
            };

            _mapVisual.AddVisual(imageVisual);
            return imageVisual;
        }

        /// <inheritdoc />
        public ImageVisualObject AddColorImageVisual(GameColor color, double width, double height, double x, double y, int layer)
        {
            return AddImageVisual(_battleInterfaceProvider.GetColorBitmap(color), width, height, x, y, layer);
        }

        /// <inheritdoc />
        public TextVisualObject AddTextVisual(string text, double fontSize, double x, double y, int layer, bool isBold = false)
        {
            return AddTextVisual(text, fontSize, x, y, layer, double.NaN, TextAlignment.Left, isBold);
        }

        /// <inheritdoc />
        public TextVisualObject AddTextVisual(string text, double fontSize, double x, double y, int layer, double width,
            TextAlignment textAlignment = TextAlignment.Center, bool isBold = false, Color? foregroundColor = null)
        {
            var textVisual = new TextVisualObject(text, fontSize, layer, width, textAlignment, isBold, foregroundColor)
            {
                X = x,
                Y = y
            };

            _mapVisual.AddVisual(textVisual);
            return textVisual;
        }

        /// <inheritdoc />
        public UnitInfoTextVisualObject AddUnitInfoTextVisualObject(Func<Unit, string> textGetter, double fontSize,
            int x, int y, int layer, bool isBold = false)
        {
            var unitInfoTextVisual = new UnitInfoTextVisualObject(textGetter, fontSize, layer, isBold) {
                X = x,
                Y = y
            };

            _mapVisual.AddVisual(unitInfoTextVisual);
            return unitInfoTextVisual;
        }


        /// <inheritdoc />
        public BattleUnit AddBattleUnit(Unit unit, bool isAttacker)
        {
            var battleUnit = new BattleUnit(_mapVisual, _battleUnitResourceProvider, unit, isAttacker);
            _game.CreateObject(battleUnit);

            return battleUnit;
        }

        /// <inheritdoc />
        public UnitPortraitObject AddUnitPortrait(Unit unit, bool rightToLeft, double x, double y)
        {
            var unitPortrait = new UnitPortraitObject(_textProvider, this, _battleInterfaceProvider, unit, rightToLeft, x, y);
            _game.CreateObject(unitPortrait);

            return unitPortrait;
        }

        /// <inheritdoc />
        public DetailUnitInfoObject ShowDetailUnitInfo(Unit unit)
        {
            var detailUnitInfoObject = new DetailUnitInfoObject(this, _battleInterfaceProvider, _textProvider, unit);
            _game.CreateObject(detailUnitInfoObject);

            return detailUnitInfoObject;
        }



        /// <inheritdoc />
        public void RemoveVisualObject(VisualObject visualObject)
        {
            if (visualObject == null)
                return;

            _mapVisual.RemoveVisual(visualObject);
        }
    }
}