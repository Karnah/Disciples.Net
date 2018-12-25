using System;
using Disciples.Engine.Common.Models;
using ReactiveUI;

namespace Disciples.Engine.Common.VisualObjects
{
    /// <summary>
    /// Текст на сцене, который содержит информацию о юните и обновляет её, если меняется состояние юнита / сам юнит.
    /// </summary>
    public class UnitInfoTextVisualObject : TextVisualObject
    {
        private readonly Func<Unit, string> _textGetter;

        private Unit _unit;
        private IDisposable _observer;

        public UnitInfoTextVisualObject(Func<Unit, string> textGetter, double fontSize, int layer, bool isBold = false)
            : base(string.Empty, fontSize, layer, isBold)
        {
            _textGetter = textGetter;
        }


        /// <summary>
        /// Юнит о котором выводится информация.
        /// </summary>
        public Unit Unit {
            get => _unit;
            set {
                _observer?.Dispose();
                this.RaiseAndSetIfChanged(ref _unit, value);
                _observer = _unit.Changed.Subscribe(_ => UpdateText());
                UpdateText();
            }
        }


        /// <summary>
        /// Обновить текст.
        /// </summary>
        private void UpdateText()
        {
            if (Unit == null) {
                Text = string.Empty;
                return;
            }

            Text = _textGetter.Invoke(_unit);
        }

        /// <inheritdoc />
        public override void Destroy()
        {
            _observer.Dispose();
        }
    }
}