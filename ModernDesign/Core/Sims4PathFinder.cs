using System;
using System.IO;
using Microsoft.Win32;

namespace ModernDesign.Core
{
    public static class Sims4PathFinder
    {
        private static readonly string ExeRelativePath = Path.Combine("Game", "Bin", "TS4_x64.exe");

        public static bool FindSims4Path(out string result)
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && (drive.DriveType == DriveType.Fixed || drive.DriveType == DriveType.Removable))
                {
                    try
                    {
                        var directories = Directory.EnumerateDirectories(drive.RootDirectory.FullName, "*",
                            SearchOption.AllDirectories);
                        foreach (var potentialRoot in directories)
                        {
                            try
                            {
                                var exePath = Path.Combine(potentialRoot, ExeRelativePath);
                                if (File.Exists(exePath))
                                {
                                    result = potentialRoot;
                                    return true;
                                }
                            }
                            catch (UnauthorizedAccessException)
                            {
                                /* Skip inaccessible folders */
                            }
                            catch (PathTooLongException)
                            {
                                /* Skip paths that are too long */
                            }
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        /* Skip inaccessible drives */
                    }
                    catch (DirectoryNotFoundException)
                    {
                        /* Skip missing directories */
                    }
                }
            }
            
            string[] registryPaths = new[]
            {
                @"SOFTWARE\Maxis\The Sims 4",
                @"SOFTWARE\WOW6432Node\Maxis\The Sims 4"
            };
            
            foreach (var regPath in registryPaths)
            {
                try
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(regPath))
                    {
                        if (key != null)
                        {
                            var installDir = key.GetValue("Install Dir") as string;
                            if (!string.IsNullOrEmpty(installDir) && Directory.Exists(installDir))
                            {
                                result = installDir;
                                return true;
                            }
                        }
                    }
                }
                catch
                {
                }
            }
            
            result = null;
            return false;
        }
    }
}