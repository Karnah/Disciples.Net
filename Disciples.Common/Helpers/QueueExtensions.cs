using System.Collections.Generic;
using System.Linq;

namespace Disciples.Common.Helpers
{
    /// <summary>
    /// Набор методов для работы с очередью.
    /// </summary>
    public static class QueueExtensions
    {
        /// <summary>
        /// Извлечь элемент из очереди, если она не пустая.
        /// </summary>
        public static bool TryDequeue<TItem>(this Queue<TItem> queue, out TItem? value)
        {
            value = default(TItem);

            if (!queue.Any())
                return false;

            value = queue.Dequeue();
            return true;
        }
    }
}