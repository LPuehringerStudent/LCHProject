using System;
using System.IO;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        Console.Write("Game to delete: ");
        string gameName = Console.ReadLine()!;

        List<string> steamFolders = FindSteamGameLocations(gameName);

        if (steamFolders.Count > 0)
        {
            Console.WriteLine("\nSteam game folders found:");
            foreach (string folder in steamFolders)
            {
                Console.WriteLine(folder);
            }

            Console.Write("\nDo you want to delete this game? (y/n): ");
            string confirm = Console.ReadLine()!.Trim().ToLower();
            if (confirm == "y")
            {
                DeleteAllGameFolders(steamFolders);
            }
            else
            {
                Console.WriteLine("Deletion canceled.");
            }
        }
        else
        {
            Console.WriteLine("No Steam game folders found.");
        }
    }

    static List<string> FindSteamGameLocations(string game)
    {
        HashSet<string> foundPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string targetPath = Path.Combine("steamapps", "common", game);

        // Check all fixed drives
        foreach (DriveInfo drive in DriveInfo.GetDrives())
        {
            if (drive.DriveType == DriveType.Fixed) // Only check HDD/SSD
            {
                string steamDefault = Path.Combine(drive.RootDirectory.FullName, "Program Files (x86)", "Steam", targetPath);
                string steamAlt = Path.Combine(drive.RootDirectory.FullName, "SteamLibrary", targetPath);

                if (Directory.Exists(steamDefault))
                {
                    foundPaths.Add(Path.GetFullPath(steamDefault));
                }
                if (Directory.Exists(steamAlt))
                {
                    foundPaths.Add(Path.GetFullPath(steamAlt));
                }
            }
        }

        // Check Steam's libraryfolders.vdf for additional install locations
        string steamConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam", "steamapps", "libraryfolders.vdf");
        if (File.Exists(steamConfigPath))
        {
            string[] lines = File.ReadAllLines(steamConfigPath);
            foreach (string line in lines)
            {
                if (line.Contains(":\\") && line.Contains("\""))
                {
                    string path = line.Split('"')[3].Replace("\\\\", "\\").Trim();
                    string steamGamePath = Path.Combine(path, "steamapps", "common", game);
                    
                    if (Directory.Exists(steamGamePath))
                    {
                        foundPaths.Add(Path.GetFullPath(steamGamePath));
                    }
                }
            }
        }

        return new List<string>(foundPaths);
    }

    static void DeleteAllGameFolders(List<string> gameFolders)
    {
        foreach (var gameFolder in gameFolders)
        {
            try
            {
                Console.WriteLine($"\nDeleting: {gameFolder}");
                Directory.Delete(gameFolder, true); // Recursively delete everything
                Console.WriteLine("✅ Deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to delete {gameFolder}: {ex.Message}");
            }
        }
    }
}
