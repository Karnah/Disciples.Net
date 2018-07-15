using System;
using System.IO;
using System.Text;

namespace ResourceProvider.Helpers
{
    internal static class StreamExtensions
    {
        public static int ReadInt(this Stream stream)
        {
            var bytes = new byte[4];
            stream.Read(bytes, 0, bytes.Length);

            return BitConverter.ToInt32(bytes, 0);
        }

        public static long ReadLong(this Stream stream)
        {
            var bytes = new byte[8];
            stream.Read(bytes, 0, bytes.Length);

            return BitConverter.ToInt64(bytes, 0);
        }

        public static string ReadString(this Stream stream, int length)
        {
            var buffer = new byte[length];
            stream.Read(buffer, 0, buffer.Length);

            return Encoding.ASCII.GetString(buffer).TrimEnd('\0');
        }

        public static string ReadString(this Stream stream)
        {
            var sb = new StringBuilder();
            byte b;

            do {
                b = (byte) stream.ReadByte();
                sb.Append(Encoding.ASCII.GetString(new[] {b}));
            } while (b != 0);

            return sb.ToString().TrimEnd('\0');
        }
    }
}
