using System;
using System.Collections.Generic;

using Disciples.Engine.Base;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Common.SceneObjects;
using Disciples.Engine.Platform.Factories;

namespace Disciples.Engine.Implementation.Base
{
    /// <inheritdoc cref="ISceneController" />
    public abstract class BaseSceneController<TData> : BaseSupportLoading<ISceneContainer, TData>, ISceneController<TData>
    {
        /// <inheritdoc />
        protected BaseSceneController(IGameController gameController, ISceneFactory sceneFactory, IInterfaceProvider interfaceProvider)
        {
            GameController = gameController;
            SceneFactory = sceneFactory;
            InterfaceProvider = interfaceProvider;
        }


        /// <inheritdoc />
        public override bool OneTimeLoading => false;

        /// <summary>
        /// Контроллер игры.
        /// </summary>
        protected IGameController GameController { get; }

        /// <summary>
        /// Контейнер с объектами на сцене.
        /// </summary>
        protected ISceneContainer SceneContainer { get; private set; }

        /// <summary>
        /// Фабрика для создания объектов на сцене.
        /// </summary>
        protected ISceneFactory SceneFactory { get; }

        /// <summary>
        /// Поставщик ресурсов для интерфейса.
        /// </summary>
        protected IInterfaceProvider InterfaceProvider { get; }


        /// <inheritdoc />
        protected override void LoadInternal(ISceneContainer sceneContainer, TData data)
        {
            SceneContainer = sceneContainer;

            InterfaceProvider.Load();
        }

        /// <inheritdoc />
        protected override void UnloadInternal()
        {
            if (!InterfaceProvider.OneTimeLoading)
                InterfaceProvider.Unload();
        }


        /// <inheritdoc />
        public AnimationObject AddAnimation(IReadOnlyList<Frame> frames, double x, double y, int layer, bool repeat = true)
        {
            var animation = new AnimationObject(this, frames, x, y, layer, repeat);
            GameController.CreateObject(animation);

            return animation;
        }

        /// <inheritdoc />
        public ButtonObject AddButton(IDictionary<SceneButtonState, IBitmap> buttonStates, Action buttonPressedAction, double x, double y, int layer, KeyboardButton? hotkey = null)
        {
            var button = new ButtonObject(this, buttonStates, buttonPressedAction, x, y, layer, hotkey);
            GameController.CreateObject(button);

            return button;
        }

        /// <inheritdoc />
        public ToggleButtonObject AddToggleButton(IDictionary<SceneButtonState, IBitmap> buttonStates, Action buttonPressedAction, double x, double y, int layer, KeyboardButton? hotkey = null)
        {
            var toggleButton = new ToggleButtonObject(this, buttonStates, buttonPressedAction, x, y, layer, hotkey);
            GameController.CreateObject(toggleButton);

            return toggleButton;
        }

        /// <inheritdoc />
        public IImageSceneObject AddImage(int layer)
        {
            var imageVisual = SceneFactory.CreateImageSceneObject(layer);

            SceneContainer.AddSceneObject(imageVisual);
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
            var imageVisual = SceneFactory.CreateImageSceneObject(layer);
            imageVisual.X = x;
            imageVisual.Y = y;
            imageVisual.Width = width;
            imageVisual.Height = height;
            imageVisual.Bitmap = bitmap;

            SceneContainer.AddSceneObject(imageVisual);
            return imageVisual;
        }

        /// <inheritdoc />
        public IImageSceneObject AddColorImage(GameColor color, double width, double height, double x, double y, int layer)
        {
            return AddImage(InterfaceProvider.GetColorBitmap(color), width, height, x, y, layer);
        }

        /// <inheritdoc />
        public ITextSceneObject AddText(string text, double fontSize, double x, double y, int layer, bool isBold = false)
        {
            return AddText(text, fontSize, x, y, layer, double.NaN, TextAlignment.Left, isBold);
        }

        /// <inheritdoc />
        public ITextSceneObject AddText(string text, double fontSize, double x, double y, int layer, double width,
            TextAlignment textAlignment = TextAlignment.Center, bool isBold = false, GameColor? foregroundColor = null)
        {
            var textVisual = SceneFactory.CreateTextSceneObject(text, fontSize, layer, width, textAlignment, isBold, foregroundColor);
            textVisual.X = x;
            textVisual.Y = y;

            SceneContainer.AddSceneObject(textVisual);
            return textVisual;
        }


        /// <inheritdoc />
        public void RemoveSceneObject(ISceneObject sceneObject)
        {
            if (sceneObject == null)
                return;

            SceneContainer.RemoveSceneObject(sceneObject);
        }
    }
}