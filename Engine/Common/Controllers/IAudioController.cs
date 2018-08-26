namespace Engine.Common.Controllers
{
    public interface IAudioController
    {
        /// <summary>
        /// Играть зацикленную фоновую музыку
        /// </summary>
        void PlayBackground(string name);

        /// <summary>
        /// Проиграть звук
        /// </summary>
        void PlaySound(string name);
    }
}
