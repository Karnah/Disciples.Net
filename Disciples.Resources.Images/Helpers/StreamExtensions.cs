using System.Text;

namespace Disciples.Resources.Images.Helpers;

/// <summary>
/// Набор методов для взаимодействием с потоком.
/// </summary>
internal static class StreamExtensions
{
    /// <summary>
    /// Считать из потока целое 32-битное число.
    /// </summary>
    public static int ReadInt(this Stream stream)
    {
        var bytes = new byte[4];
        var readLength = stream.Read(bytes, 0, bytes.Length);
        if (readLength != bytes.Length)
            throw new ArgumentException($"Прочитано только {readLength} байт из потока. Ожидалось {bytes.Length}");

        return BitConverter.ToInt32(bytes, 0);
    }

    /// <summary>
    /// Считать из потока целое 64-битное число.
    /// </summary>
    public static long ReadLong(this Stream stream)
    {
        var bytes = new byte[8];
        var readLength = stream.Read(bytes, 0, bytes.Length);
        if (readLength != bytes.Length)
            throw new ArgumentException($"Прочитано только {readLength} байт из потока. Ожидалось {bytes.Length}");

        return BitConverter.ToInt64(bytes, 0);
    }

    /// <summary>
    /// Считать из потока строку указанного размера.
    /// </summary>
    public static string ReadString(this Stream stream, int length)
    {
        var buffer = new byte[length];
        var _ = stream.Read(buffer, 0, buffer.Length);

        return Encoding.ASCII.GetString(buffer).TrimEnd('\0');
    }

    /// <summary>
    /// Считать из потока строку произвольного размера.
    /// </summary>
    public static string ReadString(this Stream stream)
    {
        var sb = new StringBuilder();
        byte b;

        do
        {
            b = (byte)stream.ReadByte();
            sb.Append(Encoding.ASCII.GetString(new[] { b }));
        } while (b != 0);

        return sb.ToString().TrimEnd('\0');
    }
}