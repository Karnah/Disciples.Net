using System.Windows.Input;

using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;

using Engine;

namespace AvaloniaDisciplesII.Battle
{
    public class BattleUnitView : TemplatedControl
    {
        public static readonly DirectProperty<BattleUnitView, BattleUnit> BattleUnitProperty =
            AvaloniaProperty.RegisterDirect<BattleUnitView, BattleUnit>(
                nameof(BattleUnit),
                buv => buv.BattleUnit,
                (buv, vo) => buv.BattleUnit = vo,
                defaultBindingMode:BindingMode.TwoWay);

        public static readonly DirectProperty<BattleUnitView, ICommand> SelectUnitCommandProperty =
            AvaloniaProperty.RegisterDirect<BattleUnitView, ICommand>(
                nameof(SelectUnitCommand),
                buv => buv.SelectUnitCommand,
                (buv, vo) => buv.SelectUnitCommand = vo,
                defaultBindingMode: BindingMode.TwoWay);

        public static readonly DirectProperty<BattleUnitView, ICommand> AttackUnitCommandProperty =
            AvaloniaProperty.RegisterDirect<BattleUnitView, ICommand>(
                nameof(AttackUnitCommand),
                buv => buv.AttackUnitCommand,
                (buv, vo) => buv.AttackUnitCommand = vo,
                defaultBindingMode: BindingMode.TwoWay);



        protected override void OnPointerEnter(PointerEventArgs e)
        {
            base.OnPointerEnter(e);

            SelectUnitCommand?.Execute(BattleUnit.Unit);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            AttackUnitCommand?.Execute(BattleUnit.Unit);
        }


        private BattleUnit _battleUnit;
        private ICommand _selectUnitCommand;
        private ICommand _attackUnitCommand;


        public BattleUnit BattleUnit {
            get => _battleUnit;
            set => SetAndRaise(BattleUnitProperty, ref _battleUnit, value);
        }

        public ICommand SelectUnitCommand {
            get => _selectUnitCommand;
            set => SetAndRaise(SelectUnitCommandProperty, ref _selectUnitCommand, value);
        }

        public ICommand AttackUnitCommand
        {
            get => _attackUnitCommand;
            set => SetAndRaise(AttackUnitCommandProperty, ref _attackUnitCommand, value);
        }
    }
}
