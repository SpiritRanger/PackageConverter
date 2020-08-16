using System.IO;
using System.IO.Compression;
using PackageConverterLib;

namespace PackageConverterApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var testFolderPath = Directory.GetCurrentDirectory();
            testFolderPath=  Path.GetFullPath(Path.Combine(testFolderPath, @"..\..\..\.."));
            testFolderPath=  Path.GetFullPath(Path.Combine(testFolderPath, @"test-destinations"));
            var testArchivePath = Directory.GetCurrentDirectory();
            testArchivePath=  Path.GetFullPath(Path.Combine(testArchivePath, @"..\..\..\.."));
            testArchivePath=  Path.GetFullPath(Path.Combine(testArchivePath, @"test-sources\darts-8$windows.zip"));


            var archiveCodeforces = new ZipArchive(new FileStream(testArchivePath, FileMode.Open), ZipArchiveMode.Read);


            using var memoryStream = new MemoryStream();

            var bacsArchive = PackageConverter.Convert(memoryStream, archiveCodeforces);

            using (var fileStream =
                new FileStream(Path.Combine(testFolderPath, "ResultBacsArchive.zip"), FileMode.Create))
            {
                memoryStream.Seek(0, SeekOrigin.Begin);
                memoryStream.CopyTo(fileStream);
            }
        }
    }
}