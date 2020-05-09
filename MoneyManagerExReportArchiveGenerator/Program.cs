using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace MoneyManagerExReportArchiveGenerator
{
    internal static class Program
    {
        private static readonly string ArchiveExtension = "grm"; // zip or grm
        private static readonly List<string> ReportFiles = new List<string>()
        {
            "description.txt",
            "luacontent.lua",
            "sqlcontent.sql",
            "template.htt"
        };

        private static void Main(string[] args)
        {
            // Folder to begin searching for Money Manager Ex report folders in
            var rootFolder = @"D:\Downloads\general-reports-master\general-reports-master\packages";
            
            // Start working
            DescendDirectory(rootFolder, rootFolder);
            Console.WriteLine($"All folders finished");
        }

        private static void DescendDirectory(string entryDirectory, string rootFolder, string parentDirectoryName = "")
        {
            var directories = new List<string>(Directory.EnumerateDirectories(entryDirectory));
            foreach (var directory in directories)
            {
                var dir = new DirectoryInfo(directory).Name;
                Console.WriteLine($"Entering directory {dir}");

                var directoryName = string.IsNullOrWhiteSpace(parentDirectoryName)
                    ? dir
                    : $"{parentDirectoryName}_{dir}";

                var containsSubOrFiles = new List<string>(Directory.EnumerateFileSystemEntries($@"{directory}"));

                if (containsSubOrFiles.Count > 0)
                {
                    DescendDirectory(directory, rootFolder, directoryName);
                }

                Console.WriteLine($"Directory {dir} finished");
            }

            var files = new List<string>(Directory.EnumerateFiles($@"{entryDirectory}"));
            if (files.Count > 0 && ReportFiles.Any(rf =>
            {
                foreach (var file in files)
                {
                    if (rf.ToLowerInvariant() == Path.GetFileName(file.Split(@"\").Last()))
                    {
                        return true;
                    }
                }
                return false;
            }))
            {
                var dir = new DirectoryInfo(entryDirectory).Name;
                var directoryName = string.IsNullOrWhiteSpace(parentDirectoryName)
                    ? "Report"
                    : $"{parentDirectoryName}";
                GenerateArchiveFromDirectoryContents(entryDirectory, rootFolder, $"{directoryName}.{ArchiveExtension}");
            }
        }

        private static void GenerateArchiveFromDirectoryContents(string directory, string rootFolder, string archiveName)
        {
            var streamFileName = @$"{rootFolder}\{archiveName}";

            if (File.Exists(streamFileName))
            {
                Console.WriteLine($"Archive {archiveName} exists, deleting");
                File.Delete(streamFileName);
            }

            using (var fileStream = new FileStream(streamFileName, FileMode.CreateNew))
            {
                Console.WriteLine($"Creating archive {archiveName}");
                using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
                {
                    Console.WriteLine($"Populating archive");
                    var files = new List<string>(Directory.EnumerateFiles($@"{directory}"));

                    foreach (var file in files)
                    {
                        var fileName = Path.GetFileName(file.Split(@"\").Last());
                        if (ReportFiles.Contains(fileName?.ToLowerInvariant()))
                        {
                            Console.WriteLine($"Adding {fileName}");
                            archive.CreateEntryFromFile($@"{file}", fileName);
                        }
                        else
                        {
                            Console.WriteLine($"Skipping {fileName}, not needed for report archive import");
                        }
                    }
                }

                Console.WriteLine($"Archive {archiveName} finished");
            }
        }
    }
}
