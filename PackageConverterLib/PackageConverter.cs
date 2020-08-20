using System.Text;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace PackageConverterLib
{
    public static class PackageConverter
    {
        private static ZipArchive newFormatArchive;
        private static ZipArchive oldFormatArchive;
        private static List<FileData> files;
        private static int memoryLimit;
        private static int timeLimit;
        private static string problemName;

        public static ZipArchive Convert(MemoryStream memoryStream, ZipArchive oldFormatArchive, bool copyPDFStatements = false)
        {
            PackageConverter.oldFormatArchive = oldFormatArchive;
            newFormatArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true);
            files = new List<FileData>();
            GetAllFilesFromOldArchive();
            SetAllFilesToNewArchive(copyPDFStatements);
            return newFormatArchive;
        }

        private static void GetAllFilesFromOldArchive()
        {
            GetStatements();
            GetChecker();
            GetSolution();
            GetTests();
            GetOtherFiles();
            GetMemoryAndTimeLimits();
        }

        private static void GetMemoryAndTimeLimits()
        {
            var file = oldFormatArchive.Entries
                .Select(entry => entry)
                .Single(entry => entry.FullName.Equals("statements/russian/problem.tex"));
            using (var entryStream = file.Open())
            {
                var content = StreamHelper.ReadToEnd(entryStream);
                var words = Encoding.UTF8.GetString(content, 0, content.Length).Split('/', '{', '}', ' ', '\n');
                timeLimit = System.Convert.ToInt32(words[11]);
                problemName = words[3];
                memoryLimit = System.Convert.ToInt32(words[14]);
            }
        }


        private static void GetOtherFiles()
        {
            var testFiles = GetFilesWhichStartsWith("files/")
                .Select(entry => entry)
                .Where(entry => !entry.FullName.EndsWith(".exe"))
                .ToList();
            foreach (var file in testFiles)
            {

                byte[] content;
                using (var entryStream = file.Open())
                    content = StreamHelper.ReadToEnd (entryStream);

                files.Add(new FileData
                {
                    OldFileName = file.FullName,
                    NewFileName = $"misc/{file.FullName}",
                    Content = content
                });
            }
        }

        private static void GetTests()
        {
            var testFiles = GetFilesWhichStartsWith("tests/").ToList();
            foreach (var file in testFiles)
            {

                byte[] content;
                using (var entryStream = file.Open())
                    content = StreamHelper.ReadToEnd (entryStream);

                files.Add(new FileData
                {
                    OldFileName = file.FullName,
                    NewFileName = file.FullName.Contains(".a") ? 
                        file.FullName.Replace(".a", ".out") : 
                        $"{file.FullName}.in",
                    Content = content
                });
            }
        }

        private static void GetSolution()
        {
            var testFiles = GetFilesWhichStartsWith("solutions/").ToList();
            foreach (var file in testFiles.Where(file => !file.FullName.EndsWith(".exe")))
            {
                byte[] content;
                using (var entryStream = file.Open())
                    content = StreamHelper.ReadToEnd (entryStream);

                files.Add(new FileData
                {
                    OldFileName = file.FullName,
                    NewFileName = $"misc/{file.FullName}",
                    Content = content
                });
            }
        }

        private static void GetChecker()
        {
            var testFiles = oldFormatArchive.Entries.Select(entry => entry).Where(entry => entry.FullName == "check.cpp").ToList();
            foreach (var file in testFiles)
            {
                byte[] content;
                using (var entryStream = file.Open())
                    content = StreamHelper.ReadToEnd (entryStream);

                files.Add(new FileData
                {
                    OldFileName = file.FullName,
                    NewFileName = "checker/checker.cpp",
                    Content = content
                });
            }
        }

        private static void GetStatements()
        {
            var statementFiles = GetFilesWhichStartsWith("statements/.html/russian").ToList();
            statementFiles = statementFiles.Concat(GetFilesWhichStartsWith("statements/.pdf/russian"))
                .ToList();
            foreach (var file in statementFiles)
            {

                byte[] content;
                using (var entryStream = file.Open())
                    content = StreamHelper.ReadToEnd (entryStream);

                files.Add(new FileData
                {
                    OldFileName = file.FullName,
                    NewFileName = file.FullName.Contains("/.html/") ? 
                        file.FullName.Replace("statements/.html/russian/", "statement/") :
                        file.FullName.Replace("statements/.pdf/russian/", "statement/"),
                    Content = content
                });
            }
        }

        private static IEnumerable<ZipArchiveEntry> GetFilesWhichStartsWith(string substring)
        {
            return oldFormatArchive.Entries.Select(entry => entry).Where(entry => entry.FullName.StartsWith(substring));
        }

        private static void SetAllFilesToNewArchive(bool setPdfStatements)
        {
            SetStatements(setPdfStatements ? "pdf" : "html");
            SetStatementsToMisc(setPdfStatements ? "html" : "pdf");
            SetChecker();
            SetSolution();
            SetOtherFiles();
            SetTests();
            CreatePackageConfig();
            CreatePackageFormat();
        }

        private static void SetOtherFiles()
        {
            var statementFiles = files.Select(data => data)
                .Where(data => data.NewFileName.StartsWith(@"misc/files/")).ToList();
            
            foreach (var file in statementFiles.Where(fileData => fileData.Content.Length != 0))
            {
                SetOneFileToNewArchive(file);
            }
        }

        private static void SetStatements(string type)
        {
            var statementFiles = files.Select(data => data)
                .Where(data => data.NewFileName.Contains($@"statement/") && data.OldFileName.Contains(type)).ToList();
            
            foreach (var file in statementFiles.Where(fileData => fileData.Content.Length != 0))
            {
                SetOneFileToNewArchive(file);
            }
            
            var configContent = $"[info]\nlanguage = C\n\n[build]\nbuilder = copy\nsource = problem.{type}";
            
            var configFile = new FileData
            {
                OldFileName = "",
                NewFileName = $"statement/{type}.ini",
                Content = configContent.ToCharArray().Select(System.Convert.ToByte).ToArray()
            };
            SetOneFileToNewArchive(configFile);
        }
        
        private static void SetStatementsToMisc(string type)
        {
            var statementFiles = files.Select(data => data)
                .Where(data => data.NewFileName.Contains($"statement/") && data.OldFileName.Contains(type)).ToList();
            
            foreach (var file in statementFiles.Where(fileData => fileData.Content.Length != 0))
            {
                file.NewFileName = file.NewFileName.Replace("statement/", "misc/");
                SetOneFileToNewArchive(file);
            }
        }

        private static void SetChecker()
        {
            var testFiles = files.Select(data => data)
                .Where(data => data.OldFileName.Equals(@"check.cpp")).ToList();
            
            foreach (var file in testFiles.Where(fileData => fileData.Content.Length != 0))
            {
                SetOneFileToNewArchive(file);
            }
            
            var configContent = "[build]\nbuilder = single\nsource = checker.cpp\nlibs = testlib.googlecode.com-0.9.12\n\n[utility]\ncall = in_out_hint\nreturn = testlib";
            
            var configFile = new FileData
            {
                OldFileName = "",
                NewFileName = $"checker/config.ini",
                Content = configContent.ToCharArray().Select(System.Convert.ToByte).ToArray()
            };
            SetOneFileToNewArchive(configFile);
        }
        
        private static void SetSolution()
        {
            var testFiles = files.Select(data => data)
                .Where(data => data.OldFileName.Contains($@"solution")).ToList();
            
            foreach (var file in testFiles.Where(fileData => fileData.Content.Length != 0))
            {
                SetOneFileToNewArchive(file);
            }
        }
        
        private static void SetTests()    
        {
            var testFiles = files.Select(data => data)
                .Where(data => data.NewFileName.Contains($@"tests/")).ToList();
            
            foreach (var file in testFiles.Where(fileData => fileData.Content.Length != 0))
            {
                SetOneFileToNewArchive(file);
            }
        }
        
        private static void CreatePackageConfig()
        {
            var configContent =
                $"[info]\nname = {problemName}\nmaintainers = anonimus\n\n[resource_limits]\ntime = {timeLimit}s\nmemory = {memoryLimit}MiB";
            
            var file = new FileData
            {
                OldFileName = "",
                NewFileName = "config.ini",
                Content = Encoding.UTF8.GetBytes(configContent)
            };
            SetOneFileToNewArchive(file);
        }
        
        private static void CreatePackageFormat()
        {
            var formatContent = "bacs/problem/single#simple0";
            var file = new FileData
            {
                OldFileName = "",
                NewFileName = "format",
                Content = formatContent.ToCharArray().Select(System.Convert.ToByte).ToArray()
            };
            SetOneFileToNewArchive(file);
        }
        
        private static void SetOneFileToNewArchive(FileData file)
        {
            var demoFile = newFormatArchive.CreateEntry(file.NewFileName);

            using (var entryStream = demoFile.Open())
            using (var streamWriter = new StreamWriter(entryStream))
            {
                streamWriter.BaseStream.Write(file.Content, 0, file.Content.Length);
            }
        }
    }
}