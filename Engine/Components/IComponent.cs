namespace Engine.Components
{
    public interface IComponent
    {
        void OnInitialize();

        void OnUpdate(long tickCount);

        void Destroy();
    }
}