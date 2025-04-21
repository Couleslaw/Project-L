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

#nullable enable

public struct LoadedPlayerTypeInfo
{
    #region Constructors

    public LoadedPlayerTypeInfo(Type type, string name, string? initPath)
    {
        PlayerType = type;
        DisplayName = name;
        InitPath = initPath;
    }

    #endregion

    #region Properties

    public Type PlayerType { get; }

    public string DisplayName { get; }

    public string? InitPath { get; }

    #endregion
}

#nullable enable
public static class PlayerTypeLoader
{
    private const string _iniFileName = "aiplayers.ini";
    #region Fields

    private static List<LoadedPlayerTypeInfo> _availablePlayerTypes = new();

    #endregion

    #region Constructors

    static PlayerTypeLoader()
    {
        // Load the available player types from the ini file
        string iniFilePath = GetAbsolutePath(_iniFileName);
        // Ensure the ini file exists
        if (!EnsureFileExists(iniFilePath)) {
            Debug.LogError($"Failed to ensure the ini file exists at {iniFilePath}");
            return;
        }
        Debug.Log($"Loading player types from {iniFilePath}");
        _availablePlayerTypes = GetAvailablePlayerTypes(iniFilePath);
    }

    #endregion

    #region Properties

    public static IReadOnlyList<LoadedPlayerTypeInfo> AvailableAIPlayerInfos => _availablePlayerTypes;

    #endregion

    #region Methods

    /// <summary>
    /// Checks if the given assembly targets .NET Standard 2.1.
    /// </summary>
    /// <param name="assembly">The assembly to check.</param>
    /// <returns><see langword="true"/> if the assembly targets .NET Standard 2.1, <see langword="false"/> otherwise.</returns>
    public static bool TargetsNetStandard21(Assembly assembly)
    {
        var targetFrameworkAttribute = assembly.GetCustomAttribute<TargetFrameworkAttribute>();

        // if target framework attribute not found --> can not confirm
        if (targetFrameworkAttribute == null) {
            Debug.LogWarning($"TargetFrameworkAttribute not found on assembly '{assembly.FullName}'. Cannot verify target framework. Skipping...");
            return false;
        }

        const string netStandard21Tfm = ".NETStandard,Version=v2.1";
        bool result = string.Equals(targetFrameworkAttribute.FrameworkName, netStandard21Tfm, StringComparison.OrdinalIgnoreCase);

        if (!result) {
            Debug.LogWarning($"Assembly '{assembly.FullName}' targets '{targetFrameworkAttribute.FrameworkName}', expected '{netStandard21Tfm}'. Skipping...");
        }

        return result;
    }

    /// <summary>
    /// Checks if a file exists at the specified path. If not, it attempts
    /// to create an empty file, including creating any necessary directories.
    /// </summary>
    /// <param name="filePath">The full path to the file.</param>
    /// <returns><see langword="true"/> if the file exists or was successfully created, <see langword="false"/> otherwise.</returns>
    public static bool EnsureFileExists(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) {
            Debug.LogWarning("EnsureFileExists: File path cannot be null or empty.");
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
    public static string GetAbsolutePath(string path)
    {
        // Path is already absolute
        if (Path.IsPathRooted(path)) {
            return path;
        }

        // Path is relative, assume relative to StreamingAssets
        return Path.GetFullPath(Path.Combine(Application.streamingAssetsPath, path));
    }

    /// <summary>
    /// Loads all available AI player types from the ini file. The sections of the ini file have the following format:
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
    /// <param name="iniFilePath">Path to the ini file.</param>
    /// <returns>List of all available AI player types and their names.</returns>
    private static List<LoadedPlayerTypeInfo> GetAvailablePlayerTypes(string iniFilePath)
    {
        var parser = new FileIniDataParser();
        IniData data = parser.ReadFile(iniFilePath);
        List<LoadedPlayerTypeInfo> playerTypes = new();

        // go through each section in the ini file
        foreach (SectionData section in data.Sections) {
            KeyDataCollection keyCol = section.Keys;
            string? dllPath = keyCol["dll_path"];
            string? name = keyCol["name"];
            string? initPath = keyCol["init_path"];
            if (dllPath is null || name is null) {
                Debug.LogWarning($"Invalid section in aiplayers.ini file: {section.SectionName}");
                continue;
            }

            // get the absolute paths
            dllPath = GetAbsolutePath(dllPath);
            if (initPath is not null) {
                initPath = GetAbsolutePath(initPath);
            }

            try {
                // Load the DLL
                Assembly assembly = Assembly.LoadFrom(dllPath);

                if (!TargetsNetStandard21(assembly)) {
                    continue;
                }

                // find the player type
                Type? playerType = assembly.GetTypes().FirstOrDefault(t => !t.IsAbstract && typeof(AIPlayerBase).IsAssignableFrom(t));

                // if everything went well...
                if (playerType != null) {
                    // Add the player type to the list and load the assembly
                    playerTypes.Add(new(playerType, name, initPath));
                    Debug.Log($"Successfully loaded the player from entry '{section.SectionName}' - ({playerType.Name})");
                }
                else {
                    Debug.LogWarning($"No valid AIPlayerBase class found in {dllPath}");
                }
            }
            // --- Exception Handling ---
            catch (FileNotFoundException fnfEx) {
                Debug.LogError($"Error loading assembly from '{dllPath}' (File Not Found): {fnfEx.Message}");
            }
            catch (BadImageFormatException bifEx) {
                Debug.LogError($"Error loading assembly from '{dllPath}' (Bad Image Format): {bifEx.Message}");
            }
            catch (Exception ex) {
                Debug.LogError($"Generic error loading assembly from '{dllPath}': {ex.GetType().Name} - {ex.Message}");
            }
        }

        return playerTypes;
    }

    #endregion
}
