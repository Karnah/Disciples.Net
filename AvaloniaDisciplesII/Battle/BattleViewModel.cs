using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;

using Avalonia;
using Avalonia.Media.Imaging;
using ReactiveUI;

using AvaloniaDisciplesII.ViewModels;
using Engine;
using Engine.Battle.Components;
using Engine.Battle.Providers;
using Engine.Components;
using Engine.Enums;
using Engine.Extensions;
using Engine.Interfaces;
using Engine.Models;

namespace AvaloniaDisciplesII.Battle
{
    public class BattleViewModel : PageViewModel
    {
        private readonly IGame _game;
        private readonly IBattleResourceProvider _battleResourceProvider;
        private readonly IAudioService _audioService;
        private readonly IBattleAttackController _battleAttackController;


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

        /// <summary>
        /// Юнит, чей ход активен на данный момент
        /// </summary>
        private Unit _currentUnit;
        /// <summary>
        /// Юнит, который был выбран целью последним
        /// </summary>
        private Unit _targetUnit;

        /// <summary>
        /// Игровой объект, отрисовывающий на текущем юните аманицию выделения
        /// </summary>
        private GameObject _selectedAnimation;

        /// <summary>
        /// Игровые объекты, которые отрисовывают анимации цели на юнитах-целях
        /// </summary>
        private IList<GameObject> _targetAnimations;


        public BattleViewModel(IGame game, IMapVisual mapVisual, IBattleResourceProvider battleResourceProvider,
            IAudioService audioService, IBattleAttackController battleAttackController)
        {
            _game = game;
            MapVisual = mapVisual;
            _battleResourceProvider = battleResourceProvider;
            _audioService = audioService;
            _battleAttackController = battleAttackController;

            Background = GetImageBitmap("Map\\Mountains#001.png");
            BottomPanel = GetImageBitmap("Interface\\IndexMap#95.png");
            LeftPanel = GetImageBitmap("Interface\\IndexMap#107.png");
            _audioService.PlayBackground("battle");

            GameObjectSelectedCommand = ReactiveCommand.Create<GameObject>(GameObjectSelected);
            GameObjectUnselectedCommand = ReactiveCommand.Create<GameObject>(GameObjectUnselected);
            GameObjectClickedCommand = ReactiveCommand.Create<GameObject>(GameObjectClicked);

            CurrentUnit = TargetUnit = _battleAttackController.CurrentUnitGameObject.Unit;
            AttachSelectedAnimation(_battleAttackController.CurrentUnitGameObject);
        }

        private static Bitmap GetImageBitmap(string path)
        {
            var cd = Environment.CurrentDirectory;
            using (var stream = new FileStream($"{cd}\\Images\\{path}", FileMode.Open)) {
                return new Bitmap(stream);
            }
        }


        public IMapVisual MapVisual { get; }

        public IReadOnlyList<BattleUnit> Units => _battleAttackController.Units;

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


        /// <summary>
        /// Обработчик события, что игровой объект был выбран
        /// </summary>
        private void GameObjectSelected(GameObject gameObject)
        {
            if (gameObject is BattleUnit battleUnit) {
                TargetUnit = battleUnit.Unit;

                ++_selectionCounter;

                if (_isAnimating)
                    return;

                SelectTargetUnits();
            }
        }

        /// <summary>
        /// Обработчик события, что с игрового объекта был смещён фокус
        /// </summary>
        private void GameObjectUnselected(GameObject gameObject)
        {
            if (gameObject is BattleUnit) {

                --_selectionCounter;
                if (_selectionCounter > 0) {
                    _selectionCounter = 1;
                    return;
                }
                _selectionCounter = 0;

                DetachTargetAnimations();
            }
        }

        /// <summary>
        /// Обработчик события клика на игровом объекта
        /// </summary>
        private void GameObjectClicked(GameObject gameObject)
        {
            if (_isAnimating)
                return;

            if (gameObject is BattleUnit targetUnitGameObject) {
                if (_battleAttackController.AttackUnit(targetUnitGameObject, OnAttackEnd) == false)
                    return;

                DetachSelectedAnimation();
                DetachTargetAnimations();
                _isAnimating = true;
            }
        }

        /// <summary>
        /// Возникает, когда закончилась анимации атаки у юнита
        /// </summary>
        private void OnAttackEnd()
        {
            CurrentUnit = _battleAttackController.CurrentUnitGameObject.Unit;

            AttachSelectedAnimation(_battleAttackController.CurrentUnitGameObject);
            if (_selectionCounter > 0) {
                SelectTargetUnits();
            }


            _isAnimating = false;
        }



        #region SelectionAnimations

        /// <summary>
        /// Отобразить анимацию выделения на текущем юните
        /// </summary>
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

        /// <summary>
        /// Скрыть анимацию выделения на текущем юните
        /// </summary>
        private void DetachSelectedAnimation()
        {
            _game.DestroyObject(_selectedAnimation);
            _selectedAnimation = null;
        }


        /// <summary>
        /// Отобразить анимацию выделения цели исходя из типа атаки текущего юнита
        /// </summary>
        private void SelectTargetUnits()
        {
            // Если текущий юнит может атаковать только одну цель,
            // то всегда будет выделена только одна цель
            if (CurrentUnit.UnitType.FirstAttack.Reach != Reach.All) {
                AttachTargetAnimations(TargetUnit);
                return;
            }

            Unit[] targetUnits;

            // Если юнит применяет способность на союзников (например, лекарь), то при наведении на союзника, будут выделяться все
            // Также наооборот, если юнит применяет способность на врагов, то выделятся все враги
            // Иначе, как например, лекарь при наведении на врага будет выделять только 1 врага
            if (CurrentUnit.Player == TargetUnit.Player && CurrentUnit.HasAllyAbility() ||
                CurrentUnit.Player != TargetUnit.Player && CurrentUnit.HasEnemyAbility()) {
                targetUnits = Units
                    .Where(u => u.Unit.Player == TargetUnit.Player)
                    .Select(u => u.Unit)
                    .ToArray();
            }
            else {
                targetUnits = new[] {TargetUnit};
            }

            AttachTargetAnimations(targetUnits);
        }

        /// <summary>
        /// Отобразить анимацию выделения цели на указанных юнитах
        /// </summary>
        private void AttachTargetAnimations(params Unit[] units)
        {
            if (_targetAnimations != null)
                DetachTargetAnimations();

            _targetAnimations = new List<GameObject>(units.Length);
            foreach (var unit in units) {
                var battleUnit = Units.First(u => u.Unit == unit);

                var frames = _battleResourceProvider.GetBattleAnimation(
                    unit.UnitType.SizeSmall
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

        /// <summary>
        /// Скрыть анимации выделения цели на всех юнитах
        /// </summary>
        private void DetachTargetAnimations()
        {
            if (_targetAnimations == null)
                return;

            foreach (var targetAnimation in _targetAnimations) {
                _game.DestroyObject(targetAnimation);
            }

            _targetAnimations = null;
        }

        #endregion
    }
}
