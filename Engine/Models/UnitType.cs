using Avalonia.Media.Imaging;

namespace Engine.Models
{
    public class UnitType
    {
        public UnitType(string unitTypeId, string name, Bitmap face)
        {
            UnitTypeId = unitTypeId;
            Name = name;
            Face = face;
        }


        public string UnitTypeId { get; }

        public string Name { get; }

        public Bitmap Face { get; }
    }
}
