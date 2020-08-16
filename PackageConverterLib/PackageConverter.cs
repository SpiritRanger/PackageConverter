using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace PackageConverterLib
{
    public static class PackageConverter
    {
        private static ZipArchive newFormatArchive;
        private static List<FileData> files;
        private static int entriesCount;
        
        public static ZipArchive Convert(MemoryStream memoryStream, ZipArchive oldFormatArchive)
        {
            newFormatArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true);
            entriesCount = oldFormatArchive.Entries.Count;
            files = new List<FileData>();
            GetAllFilesFromOldArchive();
            SetAllFilesToNewArchive();
            return newFormatArchive;
        }

        private static void GetAllFilesFromOldArchive()
        {
            
        }

        private static void SetAllFilesToNewArchive()
        {
            var demoFile = newFormatArchive.CreateEntry("/folder/foo.txt");

            using (var entryStream = demoFile.Open())
            using (var streamWriter = new StreamWriter(entryStream))
            {
                streamWriter.Write(entriesCount);
            }
        }
    }
}