using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public class ObjectReader : EndianBinaryReader
    {
        public SerializedFile assetsFile;
        public long m_PathID;
        public long byteStart;
        public uint byteSize;
        public ClassIDType type;
        public SerializedType serializedType;
        public BuildTarget platform;
        public SerializedFileFormatVersion m_Version;

        public int[] version => assetsFile.version;
        public BuildType buildType => assetsFile.buildType;

        public ObjectReader(EndianBinaryReader reader, SerializedFile assetsFile, ObjectInfo objectInfo) : base(reader.BaseStream, reader.Endian)
        {
            this.assetsFile = assetsFile;
            m_PathID = objectInfo.m_PathID;
            byteStart = objectInfo.byteStart;
            byteSize = objectInfo.byteSize;
            if (Enum.IsDefined(typeof(ClassIDType), objectInfo.classID))
            {
                type = (ClassIDType)objectInfo.classID;
            }
            else
            {
                type = ClassIDType.UnknownType;
            }
            serializedType = objectInfo.serializedType;
            platform = assetsFile.m_TargetPlatform;
            m_Version = assetsFile.header.m_Version;
        }

        public void Reset()
        {
            Position = byteStart;
        }

        /// <summary>
        /// Проверяет, является ли версия Unity 6 или выше
        /// Unity 6 имеет version[0] = 6000
        /// </summary>
        public bool IsUnity6OrGreater()
        {
            return version[0] >= 6000;
        }

        /// <summary>
        /// Проверяет, является ли версия Unity 6
        /// </summary>
        public bool IsUnity6()
        {
            return version[0] >= 6000 && version[0] < 7000;
        }

        /// <summary>
        /// Проверяет, больше или равна ли версия указанной
        /// Учитывает Unity 6 (6000.x) как более новую версию чем 2022.x
        /// </summary>
        public bool IsVersionGreaterOrEqual(int major, int minor = 0, int patch = 0)
        {
            // Unity 6 имеет version[0] = 6000, что больше чем 2022
            // Для совместимости считаем Unity 6 >= любой версии до 2023
            if (version[0] >= 6000)
            {
                if (major >= 6000)
                {
                    // Сравниваем версии Unity 6+
                    if (version[0] > major) return true;
                    if (version[0] < major) return false;
                    if (version.Length > 1 && version[1] > minor) return true;
                    if (version.Length > 1 && version[1] < minor) return false;
                    if (version.Length > 2 && version[2] >= patch) return true;
                    return version.Length <= 2;
                }
                // Unity 6+ всегда >= версий до 2023
                return major < 2023;
            }
            
            // Обычное сравнение для версий до Unity 6
            if (version[0] > major) return true;
            if (version[0] < major) return false;
            if (version.Length > 1 && version[1] > minor) return true;
            if (version.Length > 1 && version[1] < minor) return false;
            if (version.Length > 2 && version[2] >= patch) return true;
            return version.Length <= 2;
        }
    }
}
