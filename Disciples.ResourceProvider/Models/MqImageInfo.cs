namespace Disciples.ResourceProvider.Models
{
    internal class MqImageInfo
    {
        public MqImageInfo(MqImage mqImage)
        {
            MqImage = mqImage;
        }


        /// <summary>
        /// Изображение
        /// </summary>
        public MqImage MqImage { get; }

        /// <summary>
        /// Является ли изображение одним из фреймов анимации
        /// </summary>
        public bool IsAnimationFrame { get; set; }
    }
}
