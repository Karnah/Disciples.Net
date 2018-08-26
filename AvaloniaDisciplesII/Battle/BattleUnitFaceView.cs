using System.Windows.Input;

using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Engine.Battle.Contollers;
using Engine.Battle.GameObjects;
using Engine.Battle.Providers;

namespace AvaloniaDisciplesII.Battle
{
    public class BattleUnitFaceView : TemplatedControl
    {
        public static readonly DirectProperty<BattleUnitFaceView, BattleUnit> BattleUnitProperty =
            AvaloniaProperty.RegisterDirect<BattleUnitFaceView, BattleUnit>(
                nameof(BattleUnit),
                uc => uc.BattleUnit,
                (uc, vo) => uc.BattleUnit = vo,
                defaultBindingMode: BindingMode.TwoWay);

        public static readonly DirectProperty<BattleUnitFaceView, IBattleInterfaceController> InterfaceControllerProperty =
            AvaloniaProperty.RegisterDirect<BattleUnitFaceView, IBattleInterfaceController>(
                nameof(InterfaceController),
                uc => uc.InterfaceController,
                (uc, vo) => uc.InterfaceController = vo,
                defaultBindingMode: BindingMode.TwoWay);

        public static readonly DirectProperty<BattleUnitFaceView, ICommand> BattleUnitSelectedCommandProperty =
            AvaloniaProperty.RegisterDirect<BattleUnitFaceView, ICommand>(
                nameof(BattleUnitClickedCommand),
                uc => uc.BattleUnitClickedCommand,
                (uc, vo) => uc.BattleUnitClickedCommand = vo,
                defaultBindingMode: BindingMode.TwoWay);

        public static readonly DirectProperty<BattleUnitFaceView, ICommand> BattleUnitClickedCommandProperty =
            AvaloniaProperty.RegisterDirect<BattleUnitFaceView, ICommand>(
                nameof(BattleUnitSelectedCommand),
                uc => uc.BattleUnitSelectedCommand,
                (uc, vo) => uc.BattleUnitSelectedCommand = vo,
                defaultBindingMode: BindingMode.TwoWay);


        private BattleUnit _battleUnit;
        private IBattleInterfaceController _interfaceController;
        private ICommand _battleUnitSelectedCommand;
        private ICommand _battleUnitClickedCommand;


        public BattleUnit BattleUnit {
            get => _battleUnit;
            set => SetAndRaise(BattleUnitProperty, ref _battleUnit, value);
        }

        public IBattleInterfaceController InterfaceController {
            get => _interfaceController;
            set => SetAndRaise(InterfaceControllerProperty, ref _interfaceController, value);
        }
        
        public ICommand BattleUnitSelectedCommand {
            get => _battleUnitSelectedCommand;
            set => SetAndRaise(BattleUnitSelectedCommandProperty, ref _battleUnitSelectedCommand, value);
        }

        public ICommand BattleUnitClickedCommand {
            get => _battleUnitClickedCommand;
            set => SetAndRaise(BattleUnitClickedCommandProperty, ref _battleUnitClickedCommand, value);
        }


        protected override void OnPointerEnter(PointerEventArgs e)
        {
            base.OnPointerEnter(e);

            BattleUnitSelectedCommand?.Execute(BattleUnit);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            BattleUnitClickedCommand?.Execute(BattleUnit);
        }
    }
}
