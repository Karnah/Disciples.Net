using System.Windows.Input;

using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;

using Engine.Common.Models;

namespace AvaloniaDisciplesII.Battle
{
    public class VisualObjectView : TemplatedControl
    {
        public static readonly DirectProperty<VisualObjectView, VisualObject> VisualObjectProperty =
            AvaloniaProperty.RegisterDirect<VisualObjectView, VisualObject>(
                nameof(VisualObject),
                uc => uc.VisualObject,
                (uc, vo) => uc.VisualObject = vo,
                defaultBindingMode:BindingMode.TwoWay);

        public static readonly DirectProperty<VisualObjectView, ICommand> VisualSelectedCommandProperty =
            AvaloniaProperty.RegisterDirect<VisualObjectView, ICommand>(
                nameof(VisualSelectedCommand),
                uc => uc.VisualSelectedCommand,
                (uc, vo) => uc.VisualSelectedCommand = vo,
                defaultBindingMode: BindingMode.TwoWay);

        public static readonly DirectProperty<VisualObjectView, ICommand> VisualUnselectedCommandProperty =
            AvaloniaProperty.RegisterDirect<VisualObjectView, ICommand>(
                nameof(VisualUnselectedCommand),
                uc => uc.VisualUnselectedCommand,
                (uc, vo) => uc.VisualUnselectedCommand = vo,
                defaultBindingMode: BindingMode.TwoWay);

        public static readonly DirectProperty<VisualObjectView, ICommand> VisualPressedCommandProperty =
            AvaloniaProperty.RegisterDirect<VisualObjectView, ICommand>(
                nameof(VisualPressedCommand),
                uc => uc.VisualPressedCommand,
                (uc, vo) => uc.VisualPressedCommand = vo,
                defaultBindingMode: BindingMode.TwoWay);

        public static readonly DirectProperty<VisualObjectView, ICommand> VisualClickedCommandProperty =
            AvaloniaProperty.RegisterDirect<VisualObjectView, ICommand>(
                nameof(VisualClickedCommand),
                uc => uc.VisualClickedCommand,
                (uc, vo) => uc.VisualClickedCommand = vo,
                defaultBindingMode: BindingMode.TwoWay);


        private VisualObject _visualObject;
        private ICommand _visualSelectedCommand;
        private ICommand _visualUnselectedCommand;
        private ICommand _visualPressedCommand;
        private ICommand _visualClickedCommand;


        public VisualObject VisualObject {
            get => _visualObject;
            set => SetAndRaise(VisualObjectProperty, ref _visualObject, value);
        }

        public ICommand VisualSelectedCommand {
            get => _visualSelectedCommand;
            set => SetAndRaise(VisualSelectedCommandProperty, ref _visualSelectedCommand, value);
        }

        public ICommand VisualUnselectedCommand {
            get => _visualUnselectedCommand;
            set => SetAndRaise(VisualUnselectedCommandProperty, ref _visualUnselectedCommand, value);
        }

        public ICommand VisualPressedCommand {
            get => _visualPressedCommand;
            set => SetAndRaise(VisualPressedCommandProperty, ref _visualPressedCommand, value);
        }

        public ICommand VisualClickedCommand {
            get => _visualClickedCommand;
            set => SetAndRaise(VisualClickedCommandProperty, ref _visualClickedCommand, value);
        }


        protected override void OnPointerEnter(PointerEventArgs e)
        {
            base.OnPointerEnter(e);

            VisualSelectedCommand?.Execute(VisualObject.GameObject);
        }

        protected override void OnPointerLeave(PointerEventArgs e)
        {
            base.OnPointerLeave(e);

            VisualUnselectedCommand?.Execute(VisualObject.GameObject);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            VisualPressedCommand?.Execute(VisualObject.GameObject);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            VisualClickedCommand?.Execute(VisualObject.GameObject);
        }
    }
}
