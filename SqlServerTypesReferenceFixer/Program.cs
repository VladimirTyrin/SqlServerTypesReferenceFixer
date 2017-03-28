// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SqlServerTypesReferenceFixer
{
    internal static class Program
    {
        #region config
        private const string SolutionDirectory = @"C:\Users\b0-0b\Repos\Work\tmp\Digitator";
        private static readonly string SrcDirectory = Path.Combine(SolutionDirectory, @"src");

        private static readonly string[] ArchitectureList =
        {
            "x86",
            "x64"
        };
        private static readonly string[] NativeDllList =
        {
            "msvcr120.dll",
            "SqlServerSpatial140.dll"
        };
        private const string SqlServerTypesVersion = @"14.0.314.76";
        #endregion

        private static void Main(string[] args)
        {
            var localDirectories = GetLocalProjectDirectories();
            Console.WriteLine("Project directories:");
            PrintStringList(localDirectories);
            Console.WriteLine();

            var replacementDictionary = GetReplacementDictionary();

            foreach (var localDirectory in localDirectories)
            {
                var fullDirectoryPath = Path.Combine(SrcDirectory, localDirectory);
                Console.WriteLine($"Processing {fullDirectoryPath}");
                var projectFile = Path.Combine(fullDirectoryPath, $"{localDirectory}.csproj");
                if (!File.Exists(projectFile))
                {
                    Console.WriteLine($"Project file {projectFile} does not exist, skipping...");
                    continue;
                }

                var projectFileText = File.ReadAllText(projectFile, Encoding.UTF8);
                foreach (var pair in replacementDictionary)
                {
                    if (! projectFileText.Contains(pair.Key))
                        continue;

                    projectFileText = projectFileText.Replace(pair.Key, pair.Value);
                }
                    
                File.WriteAllText(projectFile, projectFileText, Encoding.UTF8);
                Console.WriteLine($"File {projectFile} fixed");
            }
        }

        private static List<string> GetLocalProjectDirectories()
            => Directory.GetDirectories(SrcDirectory).Select(GetLastPathPart).ToList();

        private static string GetLastPathPart(string path)
        {
            var parts = path.Split(Path.DirectorySeparatorChar);
            return parts[parts.Length - 1];
        }

        private static void PrintStringList(IEnumerable<string> stringList, string padding = "\t")
        {
            foreach (var str in stringList)
            {
                Console.WriteLine($"{padding}{str}");
            }
        }

        private static Dictionary<string, string> GetReplacementDictionary()
        {
            var result = new Dictionary<string, string>();

            foreach (var architecture in ArchitectureList)
            {
                foreach (var nativeDllName in NativeDllList)
                {
                    var sourceContents = $"SqlServerTypes\\{architecture}\\{nativeDllName}\">";
                    var resultContents = $"..\\..\\packages\\Microsoft.SqlServer.Types.{SqlServerTypesVersion}\\nativeBinaries\\{architecture}\\{nativeDllName}\">";
                    resultContents += $"\n        <Link>SqlServerTypes\\{architecture}\\SqlServerSpatial110.dll</Link>\n";
                    result.Add(sourceContents, resultContents);
                }
            }

            return result;
        }
    }
}
