namespace Disciples.Engine.Base
{
    /// <summary>
    /// Интерфейс для объектов, которым необходима установка параметров перед инициализацией.
    /// </summary>
    /// <remarks>
    /// TODO Вообще, может стоит переписать инициализацию на конструкторы.
    /// </remarks>
    public interface ISupportLoadingWithParameters<in TParameters> : ISupportLoading
    {
        /// <summary>
        /// Инициализировать параметры.
        /// </summary>
        void InitializeParameters(TParameters parameters);
    }
}
