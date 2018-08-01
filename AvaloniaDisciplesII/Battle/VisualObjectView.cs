using System.Windows.Input;

using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;

using Engine.Models;

namespace AvaloniaDisciplesII.Battle
{
    public class VisualObjectView : TemplatedControl
    {
        public static readonly DirectProperty<VisualObjectView, VisualObject> VisualObjectProperty =
            AvaloniaProperty.RegisterDirect<VisualObjectView, VisualObject>(
                nameof(VisualObject),
                buv => buv.VisualObject,
                (buv, vo) => buv.VisualObject = vo,
                defaultBindingMode:BindingMode.TwoWay);

        public static readonly DirectProperty<VisualObjectView, ICommand> VisualSelectedCommandProperty =
            AvaloniaProperty.RegisterDirect<VisualObjectView, ICommand>(
                nameof(VisualSelectedCommand),
                buv => buv.VisualSelectedCommand,
                (buv, vo) => buv.VisualSelectedCommand = vo,
                defaultBindingMode: BindingMode.TwoWay);

        public static readonly DirectProperty<VisualObjectView, ICommand> VisualUnselectedCommandProperty =
            AvaloniaProperty.RegisterDirect<VisualObjectView, ICommand>(
                nameof(VisualUnselectedCommand),
                buv => buv.VisualUnselectedCommand,
                (buv, vo) => buv.VisualUnselectedCommand = vo,
                defaultBindingMode: BindingMode.TwoWay);

        public static readonly DirectProperty<VisualObjectView, ICommand> VisualClickedCommandProperty =
            AvaloniaProperty.RegisterDirect<VisualObjectView, ICommand>(
                nameof(VisualClickedCommand),
                buv => buv.VisualClickedCommand,
                (buv, vo) => buv.VisualClickedCommand = vo,
                defaultBindingMode: BindingMode.TwoWay);


        private VisualObject _visualObject;
        private ICommand _visualSelectedCommand;
        private ICommand _visualUnselectedCommand;
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

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            VisualClickedCommand?.Execute(VisualObject.GameObject);
        }
    }
}
