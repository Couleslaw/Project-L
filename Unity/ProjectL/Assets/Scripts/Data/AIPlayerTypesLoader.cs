#nullable enable


namespace ProjectL.Data
{
    using IniParser;
    using IniParser.Model;
    using ProjectLCore.Players;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Versioning;
    using UnityEngine;


    /// <summary>
    /// Provides functionality to load player types from an INI file.
    /// </summary>
    public static class AIPlayerTypesLoader
    {
        /// <summary>
        /// The name of the INI file containing player type information.
        /// </summary>
        private const string _iniFileName = "aiplayers.ini";

        /// <summary>
        /// A list of available player types loaded from the INI file.
        /// </summary>
        private static readonly List<PlayerTypeInfo> _availablePlayerTypes = new();

        /// <summary>
        /// Gets a read-only list of available AI player information. This value is initialized only once when the class is loaded.
        /// </summary>
        public static IReadOnlyList<PlayerTypeInfo> AvailableAIPlayerTypes => _availablePlayerTypes;

#if !UNITY_WEBGL

        /// <summary>
        /// Class constructor to load player types from the INI file and prepare <see cref="AvailableAIPlayerTypes"/>.
        /// </summary>
        static AIPlayerTypesLoader()
        {
            // Load the available player types from the INI file
            string iniFilePath = GetAbsolutePath(_iniFileName);
            // Ensure the INI file exists
            if (!EnsureFileExists(iniFilePath)) {
                Debug.LogError($"Failed to ensure the INI file exists at {iniFilePath}");
                return;
            }
            Debug.Log($"Loading player types from {iniFilePath}");
            _availablePlayerTypes = GetCustomAIPlayerTypes(iniFilePath);
        }

        /// <summary>
        /// Checks if the given assembly targets .NET Standard 2.0 or 2.1.
        /// </summary>
        /// <param name="assembly">The assembly to check.</param>
        /// <returns><see langword="true"/> if the assembly targets .NET Standard 2.0 or 2.1; otherwise, <see langword="false"/>.</returns>
        private static bool TargetsNetStandard2(Assembly assembly)
        {
            var targetFrameworkAttribute = assembly.GetCustomAttribute<TargetFrameworkAttribute>();

            // If target framework attribute not found --> cannot confirm
            if (targetFrameworkAttribute == null) {
                Debug.LogWarning($"TargetFrameworkAttribute not found on assembly '{assembly.FullName}'. Cannot verify target framework. Skipping...");
                return false;
            }

            const string netStandard21Tfm = ".NETStandard,Version=v2.1";
            const string netStandard20Tfm = ".NETStandard,Version=v2.0";
            bool result = string.Equals(targetFrameworkAttribute.FrameworkName, netStandard20Tfm, StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(targetFrameworkAttribute.FrameworkName, netStandard21Tfm, StringComparison.OrdinalIgnoreCase);
            if (!result) {
                Debug.LogWarning($"Assembly '{assembly.FullName}' targets '{targetFrameworkAttribute.FrameworkName}', expected '{netStandard20Tfm}' or {netStandard21Tfm}. Skipping...");
            }

            return result;
        }

        /// <summary>
        /// Checks if a file exists at the specified path. If not, it attempts to create an empty file, including creating any necessary directories.
        /// </summary>
        /// <param name="filePath">The full path to the file.</param>
        /// <returns><see langword="true"/> if the file exists or was successfully created; otherwise, <see langword="false"/>.</returns>
        private static bool EnsureFileExists(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) {
                Debug.LogWarning("File path cannot be null or empty.");
                return false;
            }

            try {
                // Check if the file already exists
                if (File.Exists(filePath)) {
                    Debug.Log($"File found: {filePath}");
                    return true;
                }

                Debug.LogWarning($"File not found: {filePath}");

                // File doesn't exist, ensure the directory exists
                string directoryPath = Path.GetDirectoryName(filePath);

                if (!string.IsNullOrEmpty(directoryPath)) {
                    if (!Directory.Exists(directoryPath)) {
                        // Create the directory (and any parent directories)
                        Directory.CreateDirectory(directoryPath);
                        Debug.Log($"Created directory: {directoryPath}");
                    }
                }

                // Create the empty file
                using FileStream fs = File.Create(filePath);
                Debug.Log($"Created empty file at: {filePath}");
                return true;

            }
            catch (IOException ioEx) {
                Debug.LogError($"IO Error ensuring file exists at '{filePath}': {ioEx.Message}");
                return false;
            }
            catch (Exception ex) {
                Debug.LogError($"Error ensuring file exists at '{filePath}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Creates the absolute path from the given path. Assumes that the given path is either absolute or relative to the StreamingAssets folder.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The absolute version of the given path.</returns>
        private static string GetAbsolutePath(string path)
        {
            // Path is already absolute
            if (Path.IsPathRooted(path)) {
                return path;
            }

            // Path is relative, assume relative to StreamingAssets
            return Path.GetFullPath(Path.Combine(Application.streamingAssetsPath, path));
        }

        /// <summary>
        /// Loads all available AI player types from the INI file. The sections of the INI file have the following format:
        /// <code language="none">
        /// [My_AIPlayer]
        /// dll_path = "path/to/your/dll/AwesomeAI.dll"
        /// name = "Awesome"
        /// init_path = "path/to/your/init/file/or/folder" ; optional
        /// </code>
        /// This method looks through each DLL for a non-abstract class that inherits from the AIPlayerBase class.
        /// <remark>
        /// If for some weird reason there are multiple such classes in a single DLL, the first one found will be used.
        /// </remark>
        /// </summary>
        /// <param name="iniFilePath">Path to the INI file.</param>
        /// <returns>List of all available AI player types and their names.</returns>
        private static List<PlayerTypeInfo> GetCustomAIPlayerTypes(string iniFilePath)
        {
            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile(iniFilePath);
            List<PlayerTypeInfo> playerTypes = new();

            // Go through each section in the INI file
            foreach (SectionData section in data.Sections) {
                KeyDataCollection keyCol = section.Keys;
                string? dllPath = keyCol["dll_path"];
                string? name = keyCol["name"];
                string? initPath = keyCol["init_path"];

                // If the section is invalid
                if (dllPath is null || name is null) {
                    Debug.LogWarning($"Invalid section in aiplayers.ini file: {section.SectionName}");
                    continue;
                }

                // Get absolute paths to the dll and initialization file
                dllPath = GetAbsolutePath(dllPath);
                if (initPath is not null) {
                    initPath = GetAbsolutePath(initPath);
                }

                // Load the DLL and check if it contains an AI player
                try {
                    Assembly assembly = Assembly.LoadFrom(dllPath);

                    // Ensure that the DLL targets .NET Standard 2.1 - needed to work properly inside of Unity
                    if (!TargetsNetStandard2(assembly)) {
                        continue;
                    }

                    // Try to find the player type
                    Type? playerType = assembly.GetTypes().FirstOrDefault(t => !t.IsAbstract && typeof(AIPlayerBase).IsAssignableFrom(t));

                    // If the assembly donesn't contain a valid player type
                    if (playerType == null) {
                        Debug.LogWarning($"No valid AIPlayerBase class found in {dllPath}");
                        continue;
                    }

                    // Add info about the player to the list of available AI players
                    playerTypes.Add(new(playerType, name, initPath));
                    Debug.Log($"Successfully loaded the player from entry '{section.SectionName}' - ({playerType.Name})");

                }
                // --- Exception Handling ---
                catch (FileNotFoundException fnfEx) {
                    Debug.LogWarning($"Error loading assembly from '{dllPath}' (File Not Found): {fnfEx.Message}");
                }
                catch (BadImageFormatException bifEx) {
                    Debug.LogWarning($"Error loading assembly from '{dllPath}' (Bad Image Format): {bifEx.Message}");
                }
                catch (Exception ex) {
                    Debug.LogWarning($"Generic error loading assembly from '{dllPath}': {ex.GetType().Name} - {ex.Message}");
                }
            }

            return playerTypes;
        }
#endif
    }
}