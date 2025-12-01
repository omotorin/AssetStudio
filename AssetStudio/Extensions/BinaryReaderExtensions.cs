using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetStudio
{
    public static class BinaryReaderExtensions
    {
        private const int MAX_ARRAY_LENGTH = 1000000;

        public static void AlignStream(this BinaryReader reader)
        {
            reader.AlignStream(4);
        }

        public static void AlignStream(this BinaryReader reader, int alignment)
        {
            var pos = reader.BaseStream.Position;
            var mod = pos % alignment;
            if (mod != 0)
            {
                reader.BaseStream.Position += alignment - mod;
            }
        }

        public static string ReadAlignedString(this BinaryReader reader)
        {
            try
            {
                var length = reader.ReadInt32();
                if (length > 0 && length <= reader.BaseStream.Length - reader.BaseStream.Position)
                {
                    var stringData = reader.ReadBytes(length);
                    var result = Encoding.UTF8.GetString(stringData);
                    reader.AlignStream(4);
                    return result;
                }
                return "";
            }
            catch (EndOfStreamException)
            {
                return "";
            }
        }

        public static string ReadStringToNull(this BinaryReader reader, int maxLength = 32767)
        {
            var bytes = new List<byte>();
            int count = 0;
            while (reader.BaseStream.Position != reader.BaseStream.Length && count < maxLength)
            {
                var b = reader.ReadByte();
                if (b == 0)
                {
                    break;
                }
                bytes.Add(b);
                count++;
            }
            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        public static Quaternion ReadQuaternion(this BinaryReader reader)
        {
            return new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            return new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }

        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static Vector4 ReadVector4(this BinaryReader reader)
        {
            return new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static Color ReadColor4(this BinaryReader reader)
        {
            return new Color(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static Matrix4x4 ReadMatrix(this BinaryReader reader)
        {
            return new Matrix4x4(reader.ReadSingleArray(16));
        }

        private static T[] ReadArray<T>(Func<T> del, int length, BinaryReader? reader = null)
        {
            if (length < 0 || length > MAX_ARRAY_LENGTH) // Sanity check
            {
                return Array.Empty<T>();
            }
            var array = new List<T>(length);
            try
            {
                for (int i = 0; i < length; i++)
                {
                    // Check if we can read before attempting to read
                    if (reader != null)
                    {
                        // For ObjectReader, check byteSize limit
                        if (reader is ObjectReader objectReader)
                        {
                            long currentPos = objectReader.BaseStream.Position;
                            long maxPos = objectReader.byteStart + objectReader.byteSize;
                            if (currentPos >= maxPos)
                            {
                                // Reached the end of this object's data
                                break;
                            }
                        }
                        // Also check stream length
                        if (reader.BaseStream.Position >= reader.BaseStream.Length)
                        {
                            break;
                        }
                    }
                    
                    try
                    {
                        array.Add(del());
                    }
                    catch (EndOfStreamException)
                    {
                        // Return partial array if stream ends prematurely
                        break;
                    }
                    catch (OverflowException)
                    {
                        // Return partial array if overflow occurs
                        break;
                    }
                }
            }
            catch (EndOfStreamException)
            {
                // Return partial array if stream ends prematurely
            }
            catch (OverflowException)
            {
                // Return partial array if overflow occurs
            }
            return array.ToArray();
        }

        public static bool[] ReadBooleanArray(this BinaryReader reader)
        {
            var result = ReadArray(reader.ReadBoolean, reader.ReadInt32(), reader);
            reader.AlignStream();
            return result;
        }

        public static byte[] ReadUInt8Array(this BinaryReader reader)
        {
            int length = reader.ReadInt32();
            var result = reader.ReadBytes(length);
            reader.AlignStream();
            return result;
        }

        public static ushort[] ReadUInt16Array(this BinaryReader reader)
        {
            try
            {
                // Check if we have enough bytes to read the length (4 bytes for Int32)
                if (reader.BaseStream.Position + sizeof(int) > reader.BaseStream.Length)
                {
                    return Array.Empty<ushort>();
                }
                
                // For ObjectReader, also check byteSize limit
                if (reader is ObjectReader objectReader)
                {
                    long currentPos = objectReader.BaseStream.Position;
                    long maxPos = objectReader.byteStart + objectReader.byteSize;
                    if (currentPos + sizeof(int) > maxPos)
                    {
                        return Array.Empty<ushort>();
                    }
                }
                
                var length = reader.ReadInt32();
                if (length < 0 || length > MAX_ARRAY_LENGTH) // Sanity check
                {
                    return Array.Empty<ushort>();
                }
                
                // Check if we have enough bytes for the array
                long bytesNeeded = (long)length * sizeof(ushort);
                long availableBytes = reader.BaseStream.Length - reader.BaseStream.Position;
                
                // For ObjectReader, also check byteSize limit
                if (reader is ObjectReader objectReader2)
                {
                    long currentPos = objectReader2.BaseStream.Position;
                    long maxPos = objectReader2.byteStart + objectReader2.byteSize;
                    long objectAvailableBytes = maxPos - currentPos;
                    if (objectAvailableBytes < availableBytes)
                    {
                        availableBytes = objectAvailableBytes;
                    }
                }
                
                if (bytesNeeded > availableBytes)
                {
                    // Calculate how many elements we can safely read
                    int safeLength = (int)(availableBytes / sizeof(ushort));
                    if (safeLength <= 0)
                    {
                        return Array.Empty<ushort>();
                    }
                    length = safeLength;
                }
                
                return ReadArray(reader.ReadUInt16, length, reader);
            }
            catch (EndOfStreamException)
            {
                return Array.Empty<ushort>();
            }
            catch (OverflowException)
            {
                return Array.Empty<ushort>();
            }
        }

        public static int[] ReadInt32Array(this BinaryReader reader)
        {
            int length;
            try
            {
                length = reader.ReadInt32();
            }
            catch (Exception)
            {
                return Array.Empty<int>();
            }
            if (length < 0)
            {
                Logger.Warning($"Negative numDeltas {length}");
                return Array.Empty<int>();
            }
            if (length > MAX_ARRAY_LENGTH)
            {
                Logger.Warning($"Excessive numDeltas {length}");
                return Array.Empty<int>();
            }
            return ReadArray(reader.ReadInt32, length, reader);
        }

        public static int[] ReadInt32Array(this BinaryReader reader, int length)
        {
            return ReadArray(reader.ReadInt32, length, reader);
        }

        public static uint[] ReadUInt32Array(this BinaryReader reader)
        {
            int length;
            try
            {
                length = reader.ReadInt32();
            }
            catch (Exception)
            {
                return Array.Empty<uint>();
            }
            if (length < 0)
            {
                Logger.Warning($"Negative numDeltas {length}");
                return Array.Empty<uint>();
            }
            if (length > MAX_ARRAY_LENGTH)
            {
                Logger.Warning($"Excessive numDeltas {length}");
                return Array.Empty<uint>();
            }
            return ReadArray(reader.ReadUInt32, length, reader);
        }

        public static uint[][] ReadUInt32ArrayArray(this BinaryReader reader)
        {
            int length;
            try
            {
                length = reader.ReadInt32();
            }
            catch (Exception)
            {
                return Array.Empty<uint[]>();
            }
            if (length < 0)
            {
                Logger.Warning($"Negative numDeltas {length}");
                return Array.Empty<uint[]>();
            }
            if (length > MAX_ARRAY_LENGTH)
            {
                Logger.Warning($"Excessive numDeltas {length}");
                return Array.Empty<uint[]>();
            }
            return ReadArray(reader.ReadUInt32Array, length, reader);
        }

        public static uint[] ReadUInt32Array(this BinaryReader reader, int length)
        {
            return ReadArray(reader.ReadUInt32, length, reader);
        }

        public static float[] ReadSingleArray(this BinaryReader reader)
        {
            var result = ReadArray(reader.ReadSingle, reader.ReadInt32(), reader);
            reader.AlignStream();
            return result;
        }

        public static float[] ReadSingleArray(this BinaryReader reader, int length)
        {
            return ReadArray(reader.ReadSingle, length, reader);
        }

        public static string[] ReadStringArray(this BinaryReader reader)
        {
            return ReadArray(reader.ReadAlignedString, reader.ReadInt32(), reader);
        }

        public static Vector2[] ReadVector2Array(this BinaryReader reader)
        {
            var result = ReadArray(reader.ReadVector2, reader.ReadInt32(), reader);
            reader.AlignStream();
            return result;
        }

        public static Vector4[] ReadVector4Array(this BinaryReader reader)
        {
            var result = ReadArray(reader.ReadVector4, reader.ReadInt32(), reader);
            reader.AlignStream();
            return result;
        }

        public static Matrix4x4[] ReadMatrixArray(this BinaryReader reader)
        {
            var result = ReadArray(reader.ReadMatrix, reader.ReadInt32(), reader);
            reader.AlignStream();
            return result;
        }
    }
}
