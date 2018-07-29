using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;

using Avalonia;
using Avalonia.Input.Raw;
using Avalonia.Media.Imaging;
using ReactiveUI;
using Unity;

using AvaloniaDisciplesII.ViewModels;
using Engine;
using Engine.Battle.Enums;
using Engine.Battle.Providers;
using Engine.Interfaces;
using Engine.Models;

namespace AvaloniaDisciplesII.Battle
{
    public class BattleViewModel : PageViewModel
    {
        private readonly IUnityContainer _container;
        private readonly IGame _game;
        private readonly IAudioService _audioService;

        //private readonly ImagesExtractor _battleImagesExtractor;

        private int _currentUnitIndex = 0;
        private bool _isAnimating;

        public BattleViewModel(IUnityContainer container, IGame game, IAudioService audioService, Squad attackSquad, Squad defendSquad)
        {
            _container = container;
            _game = game;
            _audioService = audioService;

            //_battleImagesExtractor = new ImagesExtractor($"{Directory.GetCurrentDirectory()}\\Imgs\\Battle.ff");
            //_currentUnitAura = new GameObject();


            Background = GetImageBitmap("Map\\Mountains#001.png");
            BottomPanel = GetImageBitmap("Interface\\IndexMap#95.png");
            LeftPanel = GetImageBitmap("Interface\\IndexMap#107.png");
            _audioService.PlayBackground("battle");

            //Application.Current.InputManager.PostProcess.OfType<RawMouseEventArgs>().Subscribe(Click);

            ArrangeUnits(attackSquad, defendSquad);

            SelectUnitCommand = ReactiveCommand.Create<Unit>(SelectUnit);
            AttackUnitCommand = ReactiveCommand.Create<Unit>(AttackUnit);
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

            if (_isAnimating)
                return;


            var currentUnit = Units[_currentUnitIndex];
            currentUnit.AttackUnit(Units[(_currentUnitIndex + 1) % Units.Count], OnAttackEnd);
            _isAnimating = true;
        }

        private void OnAttackEnd()
        {
            ++_currentUnitIndex;
            _currentUnitIndex %= Units.Count;
            CurrentUnit = Units[_currentUnitIndex].Unit;

            _isAnimating = false;
        }


        private void ArrangeUnits(Squad attackSquad, Squad defendSquad)
        {
            _game.GameObjects.Clear();

            var units = new List<BattleUnit>();
            var bitmapResources = _container.Resolve<IBattleUnitResourceProvider>();

            foreach (var attackSquadUnit in attackSquad.Units) {
                var unit = new BattleUnit(
                    bitmapResources,
                    attackSquadUnit,
                    attackSquadUnit.SquadLinePosition,
                    attackSquadUnit.SquadFlankPosition,
                    BattleDirection.Attacker);

                units.Add(unit);
            }

            foreach (var defendSquadUnit in defendSquad.Units) {
                var unit = new BattleUnit(
                    bitmapResources,
                    defendSquadUnit,
                    ((defendSquadUnit.SquadLinePosition + 1) & 1) + 2,
                    defendSquadUnit.SquadFlankPosition,
                    BattleDirection.Defender);

                units.Add(unit);
            }

            foreach (var unit in units) {
                unit.OnInitialize();
                _game.GameObjects.Add(unit);
            }

            Units = units;
            CurrentUnit = Units[_currentUnitIndex].Unit;
        }


        public IList<BattleUnit> Units { get; private set; }

        public Bitmap Background { get; }

        public Bitmap BottomPanel { get; }

        public Bitmap LeftPanel { get; }


        private Unit _currentUnit;
        public Unit CurrentUnit {
            get => _currentUnit;
            private set => this.RaiseAndSetIfChanged(ref _currentUnit, value);
        }

        private Unit _targetUnit;
        public Unit TargetUnit {
            get => _targetUnit;
            private set => this.RaiseAndSetIfChanged(ref _targetUnit, value);
        }


        public ICommand SelectUnitCommand { get; }

        public ICommand AttackUnitCommand { get; }


        private void SelectUnit(Unit unit)
        {
            TargetUnit = unit;
        }

        private void AttackUnit(Unit unit)
        {
            if (_isAnimating)
                return;

            if (unit == CurrentUnit)
                return;

            var currentUnit = Units[_currentUnitIndex];
            var attackedUnit = Units.FirstOrDefault(u => u.Unit == unit);
            currentUnit.AttackUnit(attackedUnit, OnAttackEnd);

            _isAnimating = true;
        }
    }
}
