using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;

using Avalonia;
using Avalonia.Media.Imaging;
using ReactiveUI;
using Unity;

using AvaloniaDisciplesII.ViewModels;
using Engine;
using Engine.Battle.Components;
using Engine.Battle.Enums;
using Engine.Battle.Providers;
using Engine.Components;
using Engine.Interfaces;
using Engine.Models;

namespace AvaloniaDisciplesII.Battle
{
    public class BattleViewModel : PageViewModel
    {
        private readonly IUnityContainer _container;
        private readonly IGame _game;
        private readonly IBattleResourceProvider _battleResourceProvider;
        private readonly IAudioService _audioService;

        private int _currentUnitIndex = 0;

        /// <summary>
        /// Позволяет заблокировать действия пользователя на время анимации
        /// </summary>
        private bool _isAnimating;

        /// <summary>
        /// Счётчик синхронизирует события наведения мыши.
        /// Возникают ситуации, когда событие наведения на другой объект приходит раньше,
        /// чем событие того, что курсор покинул первый объект
        /// </summary>
        private int _selectionCounter = 0;


        private Unit _currentUnit;
        private Unit _targetUnit;

        public BattleViewModel(IUnityContainer container, IGame game, IMapVisual mapVisual, IBattleResourceProvider battleResourceProvider,
            IAudioService audioService, Squad attackSquad, Squad defendSquad)
        {
            _container = container;
            _game = game;
            MapVisual = mapVisual;
            _battleResourceProvider = battleResourceProvider;
            _audioService = audioService;

            Background = GetImageBitmap("Map\\Mountains#001.png");
            BottomPanel = GetImageBitmap("Interface\\IndexMap#95.png");
            LeftPanel = GetImageBitmap("Interface\\IndexMap#107.png");
            _audioService.PlayBackground("battle");
            
            ArrangeUnits(attackSquad, defendSquad);

            GameObjectSelectedCommand = ReactiveCommand.Create<GameObject>(GameObjectSelected);
            GameObjectUnselectedCommand = ReactiveCommand.Create<GameObject>(GameObjectUnselected);
            GameObjectClickedCommand = ReactiveCommand.Create<GameObject>(GameObjectClicked);
        }

        private static Bitmap GetImageBitmap(string path)
        {
            var cd = Environment.CurrentDirectory;
            using (var stream = new FileStream($"{cd}\\Images\\{path}", FileMode.Open)) {
                return new Bitmap(stream);
            }
        }

        private void ArrangeUnits(Squad attackSquad, Squad defendSquad)
        {
            _game.ClearScene();

            var units = new List<BattleUnit>();
            var bitmapResources = _container.Resolve<IBattleUnitResourceProvider>();

            foreach (var attackSquadUnit in attackSquad.Units) {
                var unit = new BattleUnit(
                    MapVisual,
                    bitmapResources,
                    attackSquadUnit,
                    attackSquadUnit.SquadLinePosition,
                    attackSquadUnit.SquadFlankPosition,
                    BattleDirection.Attacker);

                units.Add(unit);
            }

            foreach (var defendSquadUnit in defendSquad.Units) {
                var unit = new BattleUnit(
                    MapVisual,
                    bitmapResources,
                    defendSquadUnit,
                    ((defendSquadUnit.SquadLinePosition + 1) & 1) + 2,
                    defendSquadUnit.SquadFlankPosition,
                    BattleDirection.Defender);

                units.Add(unit);
            }

            foreach (var unit in units) {
                _game.CreateObject(unit);
            }

            Units = units;
            CurrentUnit = TargetUnit = Units[_currentUnitIndex].Unit;
            AttachSelectedAnimation(Units[_currentUnitIndex]);
        }


        public IMapVisual MapVisual { get; }

        public IList<BattleUnit> Units { get; private set; }

        public Bitmap Background { get; }

        public Bitmap BottomPanel { get; }

        public Bitmap LeftPanel { get; }


        public Unit CurrentUnit {
            get => _currentUnit;
            private set => this.RaiseAndSetIfChanged(ref _currentUnit, value);
        }

        public Unit TargetUnit {
            get => _targetUnit;
            private set => this.RaiseAndSetIfChanged(ref _targetUnit, value);
        }


        public ICommand GameObjectSelectedCommand { get; }

        public ICommand GameObjectUnselectedCommand { get; }

        public ICommand GameObjectClickedCommand { get; }


        private void GameObjectSelected(GameObject gameObject)
        {
            if (gameObject is BattleUnit battleUnit) {
                ++_selectionCounter;
                TargetUnit = battleUnit.Unit;

                if (_isAnimating)
                    return;

                AttachTargetAnimations(battleUnit);
            }
        }

        private void GameObjectUnselected(GameObject gameObject)
        {
            if (gameObject is BattleUnit) {
                --_selectionCounter;
                if (_selectionCounter > 0)
                    return;

                _selectionCounter = 0;

                if (_isAnimating)
                    return;

                DetachTargetAnimations();
            }
        }

        private void GameObjectClicked(GameObject gameObject)
        {
            if (_isAnimating)
                return;

            if (gameObject is BattleUnit battleUnit) {
                if (battleUnit.Unit == CurrentUnit)
                    return;

                var currentUnit = Units[_currentUnitIndex];
                currentUnit.AttackUnit(battleUnit, OnAttackEnd);

                DetachSelectedAnimation();
                DetachTargetAnimations();
                _isAnimating = true;
            }
        }

        private void OnAttackEnd()
        {
            ++_currentUnitIndex;
            _currentUnitIndex %= Units.Count;
            CurrentUnit = Units[_currentUnitIndex].Unit;

            AttachSelectedAnimation(Units[_currentUnitIndex]);
            if (_selectionCounter > 0) {
                AttachTargetAnimations(Units.First(u => u.Unit == TargetUnit));
            }


            _isAnimating = false;
        }



        private GameObject _selectedAnimation;
        private IList<GameObject> _targetAnimations;

        private void AttachSelectedAnimation(BattleUnit battleUnit)
        {
            if (_selectedAnimation != null)
                DetachSelectedAnimation();

            var frames = _battleResourceProvider.GetBattleAnimation(
                battleUnit.Unit.UnitType.SizeSmall
                    ? "MRKCURSMALLA"
                    : "MRKCURLARGEA");


            var unitPosition = battleUnit.BattleObjectComponent.Position;

            _selectedAnimation = new GameObject();
            _selectedAnimation.Components = new IComponent[] {
                new BattleObjectComponent(_selectedAnimation) {
                    // Задаём смещение 190. Возможно, стоит брать просто высоту юнита
                    Position = new Rect(unitPosition.X, unitPosition.Y + 190 * GameInfo.Scale, unitPosition.Width, unitPosition.Height)
                },
                new FrameAnimationComponent(_selectedAnimation, MapVisual, frames),
            };

            _game.CreateObject(_selectedAnimation);
        }

        private void DetachSelectedAnimation()
        {
            _game.DestroyObject(_selectedAnimation);
            _selectedAnimation = null;
        }


        private void AttachTargetAnimations(params BattleUnit[] units)
        {
            if (_targetAnimations != null)
                DetachTargetAnimations();

            _targetAnimations = new List<GameObject>(units.Length);
            foreach (var battleUnit in units) {
                var frames = _battleResourceProvider.GetBattleAnimation(
                    battleUnit.Unit.UnitType.SizeSmall
                        ? "MRKSMALLA"
                        : "MRKLARGEA");

                var unitPosition = battleUnit.BattleObjectComponent.Position;

                var targetAnimation = new GameObject();
                targetAnimation.Components = new IComponent[] {
                    new BattleObjectComponent(targetAnimation) {
                        // Задаём смещение 190. Возможно, стоит брать просто высоту юнита
                        Position = new Rect(unitPosition.X, unitPosition.Y + 190 * GameInfo.Scale, unitPosition.Width, unitPosition.Height)
                    },
                    new FrameAnimationComponent(targetAnimation, MapVisual, frames),
                };

                _targetAnimations.Add(targetAnimation);
                _game.CreateObject(targetAnimation);
            }
        }

        private void DetachTargetAnimations()
        {
            if (_targetAnimations == null)
                return;

            foreach (var targetAnimation in _targetAnimations) {
                _game.DestroyObject(targetAnimation);
            }

            _targetAnimations = null;
        }
    }
}
