namespace Inftastructure.Components
{
    public interface IComponent
    {
        void OnInitialize();

        void OnUpdate(long tickCount);
    }
}