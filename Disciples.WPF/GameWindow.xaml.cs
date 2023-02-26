using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Disciples.Engine;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.SceneObjects;
using Disciples.WPF.Models;
using Disciples.WPF.SceneObjects;
using ReactiveUI.Legacy;

namespace Disciples.WPF
{
    /// <summary>
    /// Окно игры.
    /// </summary>
    public partial class GameWindow : Window, ISceneContainer
    {
        private readonly ReactiveList<ISceneObject> _visuals;
        private readonly IList<ISceneObject> _addVisualBuffer;
        private readonly IList<ISceneObject> _removeVisualBuffer;
        private readonly Dictionary<ISceneObject, DrawingVisual> _mapping;
        private readonly List<DrawingVisual> _drawings;

        public GameWindow(GameWindowViewModel viewModel)
        {
            _visuals = new ReactiveList<ISceneObject>();
            _addVisualBuffer = new List<ISceneObject>();
            _removeVisualBuffer = new List<ISceneObject>();
            _mapping = new Dictionary<ISceneObject, DrawingVisual>();
            _drawings = new List<DrawingVisual>();

            this.DataContext = viewModel;

            InitializeComponent();
        }

        /// <inheritdoc />
        public IReadOnlyList<ISceneObject> SceneObjects => _visuals;

        /// <inheritdoc />
        public void AddSceneObject(ISceneObject sceneObject)
        {
            _addVisualBuffer.Add(sceneObject);
        }

        /// <inheritdoc />
        public void RemoveSceneObject(ISceneObject sceneObject)
        {
            // Обрабатываем ситуацию, когда объект был добавлен и тут же удалён.
            if (_addVisualBuffer.Contains(sceneObject)) {
                _addVisualBuffer.Remove(sceneObject);
                return;
            }

            _removeVisualBuffer.Add(sceneObject);
        }

        /// <inheritdoc />
        public void UpdateContainer()
        {
            if (_removeVisualBuffer.Any()) {
                _visuals.RemoveAll(_removeVisualBuffer);

                foreach (var visualObject in _removeVisualBuffer) {
                    visualObject.Destroy();

                    if (visualObject is ImageSceneObject imageSceneObject)
                        RemoveImageSceneObject(imageSceneObject);
                }

                _removeVisualBuffer.Clear();
            }

            _visuals.AddRange(_addVisualBuffer);
            _addVisualBuffer.Clear();

            _visuals.Sort((o, sceneObject) => o.Layer.CompareTo(sceneObject.Layer));

            foreach (var sceneObject in _visuals) {
                if (sceneObject is ImageSceneObject imageSceneObject) {
                    RedrawImageSceneObject(imageSceneObject);
                }
            }

            InvalidateVisual();

            //if (_addVisualBuffer.Any()) {
            //    _visuals.AddRange(_addVisualBuffer);

            //    foreach (var sceneObject in _addVisualBuffer) {
            //        if (sceneObject is ImageSceneObject imageSceneObject)
            //            AddImageSceneObject(imageSceneObject);
            //    }

            //    _addVisualBuffer.Clear();
            //}
        }

        /// <inheritdoc />
        protected override Visual GetVisualChild(int index)
        {
            return _drawings[index];
        }

        /// <inheritdoc />
        protected override int VisualChildrenCount => _drawings.Count;

        private void RedrawImageSceneObject(ImageSceneObject sceneObject)
        {
            if (_mapping.TryGetValue(sceneObject, out var oldDrawingVisual)) {
                if (sceneObject.Bitmap != null) {
                    using (var dc = oldDrawingVisual.RenderOpen()) {
                        var bitmap = (ImageSource)((WpfBitmap)sceneObject.Bitmap).BitmapData;
                        dc.DrawImage(bitmap,
                            new Rect(
                                sceneObject.X * GameInfo.Scale + GameInfo.OffsetX,
                                sceneObject.Y * GameInfo.Scale + GameInfo.OffsetY,
                                sceneObject.Width * GameInfo.Scale,
                                sceneObject.Height * GameInfo.Scale));
                    }
                }

                //RemoveVisualChild(oldDrawingVisual);
                //RemoveLogicalChild(oldDrawingVisual);
                return;
            }

            AddImageSceneObject(sceneObject);
        }

        private void AddImageSceneObject(ImageSceneObject sceneObject)
        {
            if (sceneObject.Bitmap == null)
                return;

            var bitmap = (ImageSource)((WpfBitmap)sceneObject.Bitmap).BitmapData;

            var drawingVisual = new DrawingVisual();
            using (var dc = drawingVisual.RenderOpen()) {
                dc.DrawImage(bitmap,
                    new Rect(
                        sceneObject.X * GameInfo.Scale + GameInfo.OffsetX,
                        sceneObject.Y * GameInfo.Scale + GameInfo.OffsetY,
                        sceneObject.Width * GameInfo.Scale,
                        sceneObject.Height * GameInfo.Scale));
            }

            _drawings.Add(drawingVisual);
            _mapping[sceneObject] = drawingVisual;

            AddVisualChild(drawingVisual);
            AddLogicalChild(drawingVisual);
        }

        private void RemoveImageSceneObject(ImageSceneObject sceneObject)
        {
            var dv = _mapping[sceneObject];
            _mapping.Remove(sceneObject);
            _drawings.Remove(dv);

            RemoveVisualChild(dv);
            RemoveLogicalChild(dv);
        }
    }
}