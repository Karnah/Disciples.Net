namespace Engine.Common.Models
{
    public class Player
    {
        public Player(int id, bool isAi)
        {
            Id = id;
            IsAI = isAi;
        }


        /// <summary>
        /// Уникальный идентификатор игрока
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Управляется ли игрок компьютером (ИИ)
        /// </summary>
        public bool IsAI { get; }
    }
}
