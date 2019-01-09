using Disciples.Engine.Base;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Common.Providers
{
    /// <summary>
    /// Поставщик информации о типах юнитов.
    /// </summary>
    public interface IUnitInfoProvider : ISupportLoading
    {
        /// <summary>
        /// Получить информацию о типе юнита по идентификатору.
        /// </summary>
        /// <param name="unitTypeId">Идентификатор типа юнита.</param>
        UnitType GetUnitType(string unitTypeId);
    }
}