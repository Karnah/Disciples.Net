using System;
using System.Collections.Generic;

using Disciples.Engine.Battle.GameObjects;
using Disciples.Engine.Battle.Providers;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Common.SceneObjects;
using Disciples.Engine.Platform.Factories;

namespace Disciples.Engine.Implementation.Controllers
{
    /// <inheritdoc />
    public class VisualSceneController : IVisualSceneController
    {
        private readonly IGame _game;
        private readonly IScene _scene;
        private readonly ITextProvider _textProvider;
        private readonly ISceneObjectFactory _sceneObjectFactory;
        private readonly IBattleUnitResourceProvider _battleUnitResourceProvider;
        private readonly IBattleInterfaceProvider _battleInterfaceProvider;

        public VisualSceneController(
            IGame game,
            IScene scene,
            ITextProvider textProvider,
            ISceneObjectFactory sceneObjectFactory,
            IBattleUnitResourceProvider battleUnitResourceProvider,
            IBattleInterfaceProvider battleInterfaceProvider)
        {
            _game = game;
            _scene = scene;
            _textProvider = textProvider;
            _sceneObjectFactory = sceneObjectFactory;
            _battleUnitResourceProvider = battleUnitResourceProvider;
            _battleInterfaceProvider = battleInterfaceProvider;
        }


        /// <inheritdoc />
        public AnimationObject AddAnimation(IReadOnlyList<Frame> frames, double x, double y, int layer, bool repeat = true)
        {
            var animation = new AnimationObject(this, frames, x, y, layer, repeat);
            _game.CreateObject(animation);

            return animation;
        }

        /// <inheritdoc />
        public AnimationObject AttachAnimation(IReadOnlyList<Frame> frames, GameObject gameObject, int layerOffset = 1, bool repeat = true)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ButtonObject AddButton(IDictionary<SceneButtonState, IBitmap> buttonStates, Action buttonPressedAction, double x, double y, int layer, KeyboardButton? hotkey = null)
        {
            var button = new ButtonObject(this, buttonStates, buttonPressedAction, x, y, layer, hotkey);
            _game.CreateObject(button);

            return button;
        }

        /// <inheritdoc />
        public ToggleButtonObject AddToggleButton(IDictionary<SceneButtonState, IBitmap> buttonStates, Action buttonPressedAction, double x, double y, int layer, KeyboardButton? hotkey = null)
        {
            var toggleButton = new ToggleButtonObject(this, buttonStates, buttonPressedAction, x, y, layer, hotkey);
            _game.CreateObject(toggleButton);

            return toggleButton;
        }

        /// <inheritdoc />
        public IImageSceneObject AddImage(int layer)
        {
            var imageVisual = _sceneObjectFactory.CreateImageSceneObject(layer);

            _scene.AddSceneObject(imageVisual);
            return imageVisual;
        }

        /// <inheritdoc />
        public IImageSceneObject AddImage(IBitmap bitmap, double x, double y, int layer)
        {
            return AddImage(bitmap, bitmap.Width, bitmap.Height, x, y, layer);
        }

        /// <inheritdoc />
        public IImageSceneObject AddImage(IBitmap bitmap, double width, double height, double x, double y, int layer)
        {
            var imageVisual = _sceneObjectFactory.CreateImageSceneObject(layer);
            imageVisual.X = x;
            imageVisual.Y = y;
            imageVisual.Width = width;
            imageVisual.Height = height;
            imageVisual.Bitmap = bitmap;

            _scene.AddSceneObject(imageVisual);
            return imageVisual;
        }

        /// <inheritdoc />
        public IImageSceneObject AddColorImage(GameColor color, double width, double height, double x, double y, int layer)
        {
            return AddImage(_battleInterfaceProvider.GetColorBitmap(color), width, height, x, y, layer);
        }

        /// <inheritdoc />
        public ITextSceneObject AddText(string text, double fontSize, double x, double y, int layer, bool isBold = false)
        {
            return AddText(text, fontSize, x, y, layer, double.NaN, TextAlignment.Left, isBold);
        }

        public ITextSceneObject AddText(string text, double fontSize, double x, double y, int layer, double width,
            TextAlignment textAlignment = TextAlignment.Center, bool isBold = false, GameColor? foregroundColor = null)
        {
            var textVisual = _sceneObjectFactory.CreateTextSceneObject(text, fontSize, layer, width, textAlignment, isBold, foregroundColor);
            textVisual.X = x;
            textVisual.Y = y;

            _scene.AddSceneObject(textVisual);
            return textVisual;
        }



        /// <inheritdoc />
        public BattleUnit AddBattleUnit(Unit unit, bool isAttacker)
        {
            var battleUnit = new BattleUnit(this, _battleUnitResourceProvider, unit, isAttacker);
            _game.CreateObject(battleUnit);

            return battleUnit;
        }

        /// <inheritdoc />
        public BattleUnitInfoGameObject AddBattleUnitInfo(int x, int y, int layer)
        {
            var battleUnitInfoObject = new BattleUnitInfoGameObject(this, x, y, layer);
            _game.CreateObject(battleUnitInfoObject);

            return battleUnitInfoObject;
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
        public void RemoveSceneObject(ISceneObject sceneObject)
        {
            if (sceneObject == null)
                return;

            _scene.RemoveSceneObject(sceneObject);
        }
    }
}