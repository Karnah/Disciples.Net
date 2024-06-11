using System.Buffers;
using System.Text;

namespace Disciples.Resources.Common.Extensions;

/// <summary>
/// Набор методов для взаимодействием с потоком.
/// </summary>
public static class StreamExtensions
{
    /// <summary>
    /// Пропустить указанное количество байт.
    /// </summary>
    public static void Skip(this Stream stream, int length)
    {
        stream.Seek(length, SeekOrigin.Current);
    }

    /// <summary>
    /// Считать из потока целое 16-битное число.
    /// </summary>
    public static int ReadShort(this Stream stream)
    {
        const int shortSize = sizeof(short);
        using var owner = MemoryPool<byte>.Shared.Rent(shortSize);
        var buffer = owner.Memory[..shortSize].Span;
        var readLength = stream.Read(buffer);
        if (readLength != shortSize)
            throw new ArgumentException($"Прочитано только {readLength} байт из потока. Ожидалось {shortSize}");

        return BitConverter.ToInt16(buffer);
    }

    /// <summary>
    /// Считать из потока целое 32-битное число.
    /// </summary>
    public static int ReadInt(this Stream stream)
    {
        const int intSize = sizeof(int);
        using var owner = MemoryPool<byte>.Shared.Rent(intSize);
        var buffer = owner.Memory[..intSize].Span;
        var readLength = stream.Read(buffer);
        if (readLength != intSize)
            throw new ArgumentException($"Прочитано только {readLength} байт из потока. Ожидалось {intSize}");

        return BitConverter.ToInt32(buffer);
    }

    /// <summary>
    /// Считать из потока целое 64-битное число.
    /// </summary>
    public static long ReadLong(this Stream stream)
    {
        const int longSize = sizeof(long);
        using var owner = MemoryPool<byte>.Shared.Rent(longSize);
        var buffer = owner.Memory[..longSize].Span;
        var readLength = stream.Read(buffer);
        if (readLength != longSize)
            throw new ArgumentException($"Прочитано только {readLength} байт из потока. Ожидалось {longSize}");

        return BitConverter.ToInt64(buffer);
    }

    /// <summary>
    /// Считать из потока строку указанного размера.
    /// </summary>
    public static string ReadString(this Stream stream, int length)
    {
        using var owner = MemoryPool<byte>.Shared.Rent(length);
        var buffer = owner.Memory[..length].Span;
        var _ = stream.Read(buffer);

        var stringEnd = buffer.IndexOf((byte)'\0');
        if (stringEnd == 0)
            return string.Empty;

        if (stringEnd != -1)
            buffer = buffer[..stringEnd];

        return Encoding.ASCII.GetString(buffer);
    }

    /// <summary>
    /// Считать из потока строку до нулевого символа.
    /// </summary>
    public static string ReadString(this Stream stream)
    {
        var resultString = new StringBuilder();
        while (stream.CanRead)
        {
            var ch = stream.ReadByte();
            if (ch == 0)
                return resultString.ToString();

            resultString.Append((char)ch);
        }

        return resultString.ToString();
    }
}