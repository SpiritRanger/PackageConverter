using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace PackageConverterLib
{
    public static class PackageConverter
    {
        private static ZipArchive newFormatArchive;
        private static List<FileData> files;

        public static ZipArchive Convert(MemoryStream memoryStream, ZipArchive oldFormatArchive)
        {
            newFormatArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true);
            files = new List<FileData>();
            GetAllFilesFromOldArchive(oldFormatArchive);
            SetAllFilesToNewArchive();
            return newFormatArchive;
        }

        private static void GetAllFilesFromOldArchive(ZipArchive oldFormatArchive)
        {
            foreach (var entry in oldFormatArchive.Entries)
            {
                var content = new byte[]{};
                using (var entryStream = entry.Open())
                    content = StreamHelper.ReadToEnd (entryStream);

                files.Add(new FileData
                {
                    OldFileName = entry.FullName,
                    NewFileName = entry.FullName,
                    Content = content
                });
            }
        }

        private static void SetAllFilesToNewArchive()
        {
            SetStatements();
            SetChecker();
            SetSolution();
            SetTests();
            CreatePackageConfig();
            CreatePackageFormat();
            foreach (var fileData in files)
            {
                if (fileData.Content.Length == 0)
                    continue;
                var demoFile = newFormatArchive.CreateEntry(fileData.NewFileName);

                using (var entryStream = demoFile.Open())
                using (var streamWriter = new StreamWriter(entryStream))
                {
                    streamWriter.BaseStream.Write(fileData.Content);
                }
            }
           
        }

        private static void SetStatements()
        {
        }
        
        private static void SetChecker()
        {
        }
        
        private static void SetSolution()
        {
        }
        
        private static void SetTests()
        {
        }
        
        private static void CreatePackageConfig()
        {
        }
        
        private static void CreatePackageFormat()
        {
        }
    }
}