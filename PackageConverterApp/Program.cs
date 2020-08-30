using System;
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
            
            Console.WriteLine("Введите полный путь к архиву, который необходимо конвертировать");
            
            var testArchivePath = Console.ReadLine();


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