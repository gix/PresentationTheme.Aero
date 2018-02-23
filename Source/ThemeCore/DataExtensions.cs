namespace ThemeCore
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class DataExtensions
    {
        public static T Read<T>(
            this UnmanagedMemoryAccessor accessor, ref long position)
            where T : struct
        {
            accessor.Read(position, out T value);
            position += Marshal.SizeOf<T>();
            return value;
        }

        public static T Read<T>(this UnmanagedMemoryAccessor accessor, long position)
            where T : struct
        {
            accessor.Read(position, out T value);
            return value;
        }

        public static string ReadAlignedPascalZString(
            this BinaryReader reader, int alignment)
        {
            reader.AlignTo(alignment);
            return reader.ReadPascalZString();
        }

        public static string ReadPascalString(this BinaryReader reader)
        {
            uint size = reader.ReadUInt32();
            var buffer = new StringBuilder();
            for (uint i = 0; i < size; ++i)
                buffer.Append((char)reader.ReadUInt16());

            return buffer.ToString();
        }

        public static string ReadPascalZString(this BinaryReader reader)
        {
            uint size = reader.ReadUInt32();
            var buffer = new StringBuilder();
            for (uint i = 0; i < size - 1; ++i)
                buffer.Append((char)reader.ReadUInt16());

            if (reader.ReadUInt16() != 0)
                throw new IOException("Unterminated string.");

            return buffer.ToString();
        }

        public static string ReadAlignedZString(
            this BinaryReader reader, int alignment)
        {
            reader.AlignTo(alignment);
            return reader.ReadZString();
        }

        public static string ReadZString(
            this UnmanagedMemoryAccessor accessor, ref long position)
        {
            var buffer = new StringBuilder();
            while (true) {
                ushort chr = accessor.ReadUInt16(position);
                if (chr == 0)
                    break;

                position += 2;
                buffer.Append((char)chr);
            }

            return buffer.ToString();
        }

        public static string ReadZString(
            this UnmanagedMemoryAccessor accessor, long position)
        {
            return ReadZString(accessor, ref position);
        }

        public static T[] ReadArray<T>(
            this UnmanagedMemoryAccessor accessor, long position, int availableLength)
            where T : struct
        {
            var count = availableLength / Marshal.SizeOf<T>();
            var values = new T[count];
            if (accessor.ReadArray(position, values, 0, count) != count)
                throw new EndOfStreamException();
            return values;
        }

        public static string ReadZString(this BinaryReader reader)
        {
            var buffer = new StringBuilder();
            while (true) {
                ushort chr = reader.ReadUInt16();
                if (chr == 0)
                    break;

                buffer.Append((char)chr);
            }

            return buffer.ToString();
        }

        public static void AlignTo(ref int value, int alignment)
        {
            var remainder = value % alignment;
            if (remainder != 0)
                value += alignment - remainder;
        }

        public static void AlignTo(ref long value, long alignment)
        {
            var remainder = value % alignment;
            if (remainder != 0)
                value += alignment - remainder;
        }

        public static int AlignTo(this int value, int alignment)
        {
            var remainder = value % alignment;
            if (remainder != 0)
                value += alignment - remainder;

            return value;
        }

        public static long AlignTo(this long value, long alignment)
        {
            var remainder = value % alignment;
            if (remainder != 0)
                value += alignment - remainder;
            return value;
        }

        public static void AlignTo<T>(this BinaryReader reader) where T : struct
        {
            reader.AlignTo(Marshal.SizeOf<T>());
        }

        public static void AlignTo(this BinaryReader reader, int alignment)
        {
            var stream = reader.BaseStream;
            long pos = stream.Position;
            if (pos % alignment != 0) {
                long adjustment = alignment - (pos % alignment);
                stream.Position = Math.Min(stream.Length, pos + adjustment);
            }
        }
    }
}
