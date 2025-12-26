using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.IO.Compression;

namespace ModernDesign.MVVM.View
{
    public static class UnlockerService
    {
        // Shared HttpClient for downloads
        private static readonly HttpClient _httpClient = new HttpClient();

        // Unlocker working folder (NOT in TEMP, setup.bat rejects TEMP paths)
        private static readonly string _unlockerFolder;

        // Unlocker package URL (.zip with setup + config + g_The Sims 4.ini)
        private const string UnlockerPackageUrl = "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Unlocker.zip";

        // AppData folder structure
        private const string CommonDir = @"anadius\EA DLC Unlocker v2";

        // Static constructor: runs once
        static UnlockerService()
        {
            // Use AppData\Local instead of TEMP (setup.bat rejects TEMP paths)
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _unlockerFolder = Path.Combine(localAppData, "LeuansSims4Toolkit", "DLCUnlocker");

            if (!Directory.Exists(_unlockerFolder))
            {
                Directory.CreateDirectory(_unlockerFolder);
            }
        }

        // ===================== PUBLIC API =====================

        private static void DeleteFileSafe(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch
            {
                // ignore
            }
        }

        private static void DeleteDirectorySafe(string path)
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, recursive: true);
            }
            catch
            {
                // ignore
            }
        }

        /// <summary>
        /// Returns true if the unlocker is installed for EA app / Origin.
        /// </summary>
        public static bool IsUnlockerInstalled(out string clientName)
        {
            clientName = null;

            if (!TryGetClientPath(out var clientPath, out var clientId, out var friendlyName))
                return false;

            clientName = friendlyName;

            var dstDll = Path.Combine(clientPath, "version.dll");
            var appDataRoot = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appDataDir = Path.Combine(appDataRoot, CommonDir);
            var dstConfig = Path.Combine(appDataDir, "config.ini");

            return File.Exists(dstDll) && File.Exists(dstConfig);
        }

        /// <summary>
        /// Downloads the unlocker package and runs setup.bat auto
        /// </summary>
        public static async Task InstallUnlockerAsync()
        {
            await DownloadUnlockerPackageAsync();
            RunUnlockerScriptAuto();

            // Wait for setup to complete (it runs with admin elevation internally)
            await Task.Delay(10000);

            // Clean up after installation
            CleanupUnlockerFiles();
        }

        // ===================== INTERNAL HELPERS =====================

        /// <summary>
        /// Downloads and extracts the unlocker package to LocalAppData (NOT TEMP)
        /// </summary>
        private static async Task DownloadUnlockerPackageAsync()
        {
            var tempZip = Path.Combine(Path.GetTempPath(), "LeuansSims4_Unlocker.zip");

            await DownloadWithResumeAsync(UnlockerPackageUrl, tempZip);

            try
            {
                await Task.Run(() =>
                {
                    // Clean old files first
                    DeleteFileSafe(Path.Combine(_unlockerFolder, "setup.bat"));
                    DeleteFileSafe(Path.Combine(_unlockerFolder, "setup.exe"));
                    DeleteFileSafe(Path.Combine(_unlockerFolder, "setup_linux.sh"));
                    DeleteFileSafe(Path.Combine(_unlockerFolder, "setup_macos.sh"));
                    DeleteFileSafe(Path.Combine(_unlockerFolder, "g_The Sims 4.ini"));
                    DeleteFileSafe(Path.Combine(_unlockerFolder, "config.ini"));
                    DeleteDirectorySafe(Path.Combine(_unlockerFolder, "ea_app"));
                    DeleteDirectorySafe(Path.Combine(_unlockerFolder, "origin"));

                    // Extract to LocalAppData folder (NOT TEMP)
                    ZipFile.ExtractToDirectory(tempZip, _unlockerFolder);
                });
            }
            finally
            {
                DeleteFileSafe(tempZip);
            }

            var iniPath = Path.Combine(_unlockerFolder, "g_The Sims 4.ini");
            if (!File.Exists(iniPath))
            {
                throw new FileNotFoundException(
                    "The unlocker package was extracted, but 'g_The Sims 4.ini' was not found.",
                    iniPath);
            }
        }

        /// <summary>
        /// Runs setup.bat with "auto" argument
        /// The script will request admin rights internally via Start-Process -Verb RunAs
        /// </summary>
        private static void RunUnlockerScriptAuto()
        {
            var batPath = Path.Combine(_unlockerFolder, "setup.bat");

            if (!File.Exists(batPath))
            {
                throw new FileNotFoundException("setup.bat was not found in the unlocker folder.", batPath);
            }

            var psi = new ProcessStartInfo
            {
                FileName = batPath,
                Arguments = "auto",
                WorkingDirectory = _unlockerFolder,
                UseShellExecute = true, // Required for .bat files to run properly
                CreateNoWindow = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process.Start(psi);
        }

        /// <summary>
        /// Cleans up unlocker files after installation
        /// </summary>
        private static void CleanupUnlockerFiles()
        {
            try
            {
                DeleteFileSafe(Path.Combine(_unlockerFolder, "setup.bat"));
                DeleteFileSafe(Path.Combine(_unlockerFolder, "setup.exe"));
                DeleteFileSafe(Path.Combine(_unlockerFolder, "setup_linux.sh"));
                DeleteFileSafe(Path.Combine(_unlockerFolder, "setup_macos.sh"));
                DeleteFileSafe(Path.Combine(_unlockerFolder, "g_The Sims 4.ini"));
                DeleteFileSafe(Path.Combine(_unlockerFolder, "config.ini"));

                DeleteDirectorySafe(Path.Combine(_unlockerFolder, "origin"));
                DeleteDirectorySafe(Path.Combine(_unlockerFolder, "ea_app"));

                // Try to delete the folder itself if empty
                try
                {
                    if (Directory.Exists(_unlockerFolder) && Directory.GetFileSystemEntries(_unlockerFolder).Length == 0)
                    {
                        Directory.Delete(_unlockerFolder);
                    }
                }
                catch { }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        // ===================== CLIENT DETECTION (EA app / Origin) =====================

        private static bool TryGetClientPath(out string clientPath, out string clientId, out string clientFriendlyName)
        {
            clientPath = null;
            clientId = null;
            clientFriendlyName = null;

            // EA Desktop (EA app) first
            if (TryGetClientPathFromRegistry(@"Electronic Arts\EA Desktop", out var eaClientPath))
            {
                clientPath = eaClientPath;
                clientId = "ea_app";
                clientFriendlyName = "EA app";
                return true;
            }

            // Origin 32-bit key
            if (TryGetClientPathFromRegistry(@"WOW6432Node\Origin", out var origin32Path))
            {
                clientPath = origin32Path;
                clientId = "origin";
                clientFriendlyName = "Origin";
                return true;
            }

            // Origin normal key
            if (TryGetClientPathFromRegistry(@"Origin", out var originPath))
            {
                clientPath = originPath;
                clientId = "origin";
                clientFriendlyName = "Origin";
                return true;
            }

            return false;
        }

        private static bool TryGetClientPathFromRegistry(string subKey, out string clientPath)
        {
            clientPath = null;

            try
            {
                // Try 64-bit view first
                using (var key64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                                              .OpenSubKey(@"SOFTWARE\" + subKey))
                {
                    if (key64 != null)
                    {
                        var cp = key64.GetValue("ClientPath") as string;
                        if (!string.IsNullOrEmpty(cp) && File.Exists(cp))
                        {
                            clientPath = Directory.GetParent(cp).FullName;
                            return true;
                        }
                    }
                }

                // Try 32-bit view
                using (var key32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
                                              .OpenSubKey(@"SOFTWARE\" + subKey))
                {
                    if (key32 != null)
                    {
                        var cp = key32.GetValue("ClientPath") as string;
                        if (!string.IsNullOrEmpty(cp) && File.Exists(cp))
                        {
                            clientPath = Directory.GetParent(cp).FullName;
                            return true;
                        }
                    }
                }
            }
            catch
            {
                // ignore registry errors
            }

            return false;
        }

        // ===================== DOWNLOAD + RESUME =====================

        private static async Task DownloadWithResumeAsync(string url, string tempFilePath)
        {
            long existingLength = 0;

            if (File.Exists(tempFilePath))
            {
                var info = new FileInfo(tempFilePath);
                existingLength = info.Length;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            if (existingLength > 0)
            {
                request.Headers.Range = new RangeHeaderValue(existingLength, null);
            }

            using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            {
                if (response.StatusCode == HttpStatusCode.OK && existingLength > 0)
                {
                    using (var fs = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(fs);
                    }
                }
                else if (response.StatusCode == HttpStatusCode.PartialContent || existingLength == 0)
                {
                    using (var fs = new FileStream(
                               tempFilePath,
                               existingLength > 0 ? FileMode.Append : FileMode.Create,
                               FileAccess.Write,
                               FileShare.None))
                    {
                        await response.Content.CopyToAsync(fs);
                    }
                }
                else
                {
                    throw new Exception($"Unexpected HTTP response: {(int)response.StatusCode} {response.ReasonPhrase}");
                }
            }
        }

        // ===================== UNINSTALL =====================

        public static async Task UninstallUnlockerAsync()
        {
            await Task.Run(() =>
            {
                if (TryGetClientPath(out var clientPath, out var clientId, out var friendlyName))
                {
                    var dstDll = Path.Combine(clientPath, "version.dll");
                    DeleteFileSafe(dstDll);

                    if (clientId == "ea_app")
                    {
                        try
                        {
                            var parentOfClient = Directory.GetParent(clientPath)?.FullName;
                            if (!string.IsNullOrEmpty(parentOfClient))
                            {
                                var stagedDir = Path.Combine(parentOfClient, @"StagedEADesktop\EA Desktop");
                                var stagedDll = Path.Combine(stagedDir, "version.dll");
                                DeleteFileSafe(stagedDll);
                            }

                            var psi = new ProcessStartInfo
                            {
                                FileName = "schtasks",
                                Arguments = "/Delete /TN copy_dlc_unlocker /F",
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                WindowStyle = ProcessWindowStyle.Hidden
                            };
                            using (var proc = Process.Start(psi))
                            {
                                proc?.WaitForExit(5000);
                            }
                        }
                        catch { }
                    }

                    var appDataRoot = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    var appDataDir = Path.Combine(appDataRoot, CommonDir);

                    var localAppDataRoot = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    var localAppDataDir = Path.Combine(localAppDataRoot, CommonDir);

                    DeleteFolderRecursively(appDataDir);
                    DeleteFolderIfEmptyParent(appDataDir);

                    DeleteFolderRecursively(localAppDataDir);
                    DeleteFolderIfEmptyParent(localAppDataDir);
                }

                CleanupUnlockerFiles();
            });
        }

        private static void DeleteFolderRecursively(string directory)
        {
            try
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, recursive: true);
                }
            }
            catch { }
        }

        private static void DeleteFolderIfEmptyParent(string directory)
        {
            try
            {
                var parent = Directory.GetParent(directory);
                if (parent != null && Directory.Exists(parent.FullName))
                {
                    if (Directory.GetFileSystemEntries(parent.FullName).Length == 0)
                    {
                        Directory.Delete(parent.FullName);
                    }
                }
            }
            catch { }
        }

        public static void CleanLocalUnlockerFiles()
        {
            CleanupUnlockerFiles();
        }
    }
}