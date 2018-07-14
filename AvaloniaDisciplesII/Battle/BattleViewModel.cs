using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

using Avalonia;
using Avalonia.Input.Raw;
using Avalonia.Media.Imaging;

using AvaloniaDisciplesII.ViewModels;
using Engine;
using Engine.Components;
using Engine.Interfaces;
using ResourceManager;

using Action = Engine.Enums.Action;

namespace AvaloniaDisciplesII.Battle
{
    public class BattleViewModel : PageViewModel
    {
        private readonly IAudioService _audioService;
        private readonly IList<GameObject> _gameObjects;

        private readonly ImagesExtractor _battleImagesExtractor;

        private int _currentUnitIndex = 0;

        public BattleViewModel(IMapVisual mapVisual, IAudioService audioService, IList<GameObject> gameObjects)
        {
            _audioService = audioService;
            _gameObjects = gameObjects;
            MapVisual = mapVisual;

            _battleImagesExtractor = new ImagesExtractor($"{Directory.GetCurrentDirectory()}\\Imgs\\Battle.ff");
            _currentUnitAura = new GameObject();


            Background = GetImageBitmap("Map\\Ship#001.png");
            BottomPanel = GetImageBitmap("Interface\\IndexMap#95.png");
            LeftPanel = GetImageBitmap("Interface\\IndexMap#107.png");
            _audioService.PlayBackground("battle");

            Application.Current.InputManager.PostProcess.OfType<RawMouseEventArgs>().Subscribe(Click);
        }

        private static Bitmap GetImageBitmap(string path)
        {
            var cd = Environment.CurrentDirectory;
            using (var stream = new FileStream($"{cd}\\Images\\{path}", FileMode.Open))
            {
                return new Bitmap(stream);
            }
        }

        private void Click(RawMouseEventArgs args)
        {
            if (args.Type != RawMouseEventType.LeftButtonUp)
                return;

            var mo = _gameObjects[_currentUnitIndex].Components.OfType<MapObject>().First().Action = Action.Attacking;
            ++_currentUnitIndex;
            _currentUnitIndex %= _gameObjects.Count;
        }


        private GameObject _currentUnitAura;

        private void SelectUnit()
        {
            var currentUnit = _gameObjects[_currentUnitIndex];
            var cuCoor = currentUnit.Components.OfType<MapObject>().First();

            var coor = GameInfo.OffsetCoordinates(cuCoor.Position.X, cuCoor.Position.Y);

        }



        public IMapVisual MapVisual { get; set; }

        public Bitmap Background { get; }

        public Bitmap BottomPanel { get; }

        public Bitmap LeftPanel { get; }


        public double Width => 800 * GameInfo.Scale;

        public double Height => 600 * GameInfo.Scale;
    }
}
