using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
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
                    
                    if (visualObject is ImageSceneObject) {
                        var dv = _mapping[visualObject];
                        _mapping.Remove(visualObject);
                        _drawings.Remove(dv);
                    }
                }
                
                _removeVisualBuffer.Clear();
            }

            if (_addVisualBuffer.Any()) {
                _visuals.AddRange(_addVisualBuffer);

                foreach (var sceneObject in _addVisualBuffer) {
                    if (sceneObject is ImageSceneObject imageSceneObject) {
                        var bitmap = (ImageSource) ((WpfBitmap) imageSceneObject.Bitmap).BitmapData;
                
                        DrawingVisual dv = new DrawingVisual();
                        using (var dc = dv.RenderOpen()) {          
                            dc.DrawImage(bitmap, new Rect(sceneObject.X, sceneObject.Y, sceneObject.Width, sceneObject.Height));
                        }
                
                        _drawings.Add(dv);
                        _mapping[sceneObject] = dv;
                    }
                }
                
                _addVisualBuffer.Clear();
            }
        }

        /// <inheritdoc />
        protected override Visual GetVisualChild(int index)
        {
            return _drawings[index];
        }

        protected override int VisualChildrenCount => _drawings.Count;
    }
}