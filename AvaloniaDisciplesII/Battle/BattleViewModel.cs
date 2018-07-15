using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

using Unity;
using Avalonia;
using Avalonia.Input.Raw;
using Avalonia.Media.Imaging;

using AvaloniaDisciplesII.ViewModels;
using Engine;
using Engine.Components;
using Engine.Enums;
using Engine.Interfaces;
using Engine.Models;

using Action = Engine.Enums.Action;

namespace AvaloniaDisciplesII.Battle
{
    public class BattleViewModel : PageViewModel
    {
        private readonly IUnityContainer _container;
        private readonly IGame _game;
        private readonly IAudioService _audioService;
        private IList<GameObject> _units;

        //private readonly ImagesExtractor _battleImagesExtractor;

        private int _currentUnitIndex = 0;

        public BattleViewModel(IUnityContainer container, IGame game, IMapVisual mapVisual, IAudioService audioService, Squad attackSquad, Squad defendSquad)
        {
            _container = container;
            _game = game;
            _audioService = audioService;
            MapVisual = mapVisual;

            //_battleImagesExtractor = new ImagesExtractor($"{Directory.GetCurrentDirectory()}\\Imgs\\Battle.ff");
            //_currentUnitAura = new GameObject();


            Background = GetImageBitmap("Map\\Mountains#001.png");
            BottomPanel = GetImageBitmap("Interface\\IndexMap#95.png");
            LeftPanel = GetImageBitmap("Interface\\IndexMap#107.png");
            _audioService.PlayBackground("battle");

            Application.Current.InputManager.PostProcess.OfType<RawMouseEventArgs>().Subscribe(Click);

            ArrangeUnits(attackSquad, defendSquad);
        }

        private static Bitmap GetImageBitmap(string path)
        {
            var cd = Environment.CurrentDirectory;
            using (var stream = new FileStream($"{cd}\\Images\\{path}", FileMode.Open)) {
                return new Bitmap(stream);
            }
        }

        private void Click(RawMouseEventArgs args)
        {
            if (args.Type != RawMouseEventType.LeftButtonUp)
                return;

            var mo = _units[_currentUnitIndex].Components.OfType<MapObject>().First().Action = Action.Attacking;
            ++_currentUnitIndex;
            _currentUnitIndex %= _units.Count;
        }

        private void ArrangeUnits(Squad attackSquad, Squad defendSquad)
        {
            _game.GameObjects.Clear();

            var units = new List<GameObject>();

            foreach (var attackSquadUnit in attackSquad.Units) {
                var unit = CreateUnit(
                    attackSquadUnit.UnitType.UnitTypeId,
                    attackSquadUnit.SquadLinePosition,
                    attackSquadUnit.SquadFlankPosition,
                    Direction.Southeast);

                units.Add(unit);
            }

            foreach (var defendSquadUnit in defendSquad.Units) {
                var unit = CreateUnit(
                    defendSquadUnit.UnitType.UnitTypeId,
                    ((defendSquadUnit.SquadLinePosition + 1) & 1) + 2,
                    defendSquadUnit.SquadFlankPosition,
                    Direction.Northwest);

                units.Add(unit);
            }

            foreach (var unit in units) {
                unit.OnInitialize();
                _game.GameObjects.Add(unit);
            }

            _units = units;
        }

        private GameObject CreateUnit(string id, double x, double y, Direction direction)
        {
            var bitmapResources = _container.Resolve<IBitmapResources>();
            var coor = GameInfo.OffsetCoordinates(x, y);
            var go = new GameObject();
            go.Components = new IComponent[] {
                new MapObject(go) {
                    Position = new Rect(coor.X, coor.Y, 100, 100),
                    Direction = direction,
                    Action = Action.Waiting,
                },
                new AnimationComponent(go, MapVisual, bitmapResources, id, "S1"),
                new AnimationComponent(go, MapVisual, bitmapResources, id, "A1"),
                new AnimationComponent(go, MapVisual, bitmapResources, id, "A2"),
                //new SoundsComponent(go, AudioService, attackSounds),
            };

            return go;
        }



        public IMapVisual MapVisual { get; set; }

        public Bitmap Background { get; }

        public Bitmap BottomPanel { get; }

        public Bitmap LeftPanel { get; }


        // todo Переделать в конвертеры? Или как-то поумнее раскидать
        public double LeftPanelHeight => 448 * GameInfo.Scale;

        public Thickness LeftPanelMargin => new Thickness(0, 0, 0, 140 * GameInfo.Scale);
    }
}
