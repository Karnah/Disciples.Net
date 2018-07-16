using System;
using System.Data;

namespace AvaloniaDisciplesII.Helpers
{
    public static class DataRowExtensions
    {
        public static T GetClass<T>(this DataRow dataRow, string columnName)
            where T : class
        {
            var value = dataRow[columnName];
            if (value == DBNull.Value)
                return null;

            return (T) value;
        }

        public static T? GetStruct<T>(this DataRow dataRow, string columnName)
            where T : struct
        {
            var value = dataRow[columnName];
            if (value == DBNull.Value)
                return null;

            return (T)value;
        }
    }
}
