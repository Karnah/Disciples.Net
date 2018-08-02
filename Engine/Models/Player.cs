namespace Engine.Models
{
    public class Player
    {
        public Player(int id, bool isAi)
        {
            Id = id;
            IsAI = isAi;
        }


        public int Id { get; }

        public bool IsAI { get; }
    }
}
