using System.Collections.Generic;
using UnityEngine;

#nullable enable
public static class PuzzleSpritesLoader
{
    #region Constants

    private const string _puzzleSpritesDirectory = "PuzzleSprites";
    private const string _borderDim = "border-dim";
    private const string _borderBright = "border-bright";
    private const string _borderless = "borderless";
    private const string _hover = "hover";

    #endregion

    #region Fields

    private static readonly Dictionary<(int, bool), Sprite> _borderDimSprites = new();
    private static readonly Dictionary<(int, bool), Sprite> _borderBrightSprites = new();
    private static readonly Dictionary<(int, bool), Sprite> _borderlessSprites = new();
    private static readonly Dictionary<(int, bool), Sprite> _hoverSprites = new();

    #endregion

    #region Methods

    /// <summary>
    /// Retrieves the "border-dim" sprite for the specified puzzle number and color.
    /// </summary>
    /// <param name="puzzleNumber">The puzzle number (1-99).</param>
    /// <param name="isBlack">Indicates whether the sprite is black (<see langword="true"/>) or white (<see langword="false"/>).</param>
    /// <returns>The "border-dim" sprite, or <see langword="null"/> if it could not be loaded.</returns>
    public static Sprite? GetBorderDimSprite(int puzzleNumber, bool isBlack)
    {
        return GetSpriteFromPath(_borderDimSprites, _borderDim, puzzleNumber, isBlack);
    }

    /// <summary>
    /// Retrieves the "border-bright" sprite for the specified puzzle number and color.
    /// </summary>
    /// <param name="puzzleNumber">The puzzle number (1-99).</param>
    /// <param name="isBlack">Indicates whether the sprite is black (<see langword="true"/>) or white (<see langword="false"/>).</param>
    /// <returns>The "border-bright" sprite, or <see langword="null"/> if it could not be loaded.</returns>
    public static Sprite? GetBorderBrightSprite(int puzzleNumber, bool isBlack)
    {
        return GetSpriteFromPath(_borderBrightSprites, _borderBright, puzzleNumber, isBlack);
    }

    /// <summary>
    /// Retrieves the "borderless" sprite for the specified puzzle number and color.
    /// </summary>
    /// <param name="puzzleNumber">The puzzle number (1-99).</param>
    /// <param name="isBlack">Indicates whether the sprite is black (<see langword="true"/>) or white (<see langword="false"/>).</param>
    /// <returns>The "borderless" sprite, or <see langword="null"/> if it could not be loaded.</returns>
    public static Sprite? GetBorderlessSprite(int puzzleNumber, bool isBlack)
    {
        return GetSpriteFromPath(_borderlessSprites, _borderless, puzzleNumber, isBlack);
    }

    /// <summary>
    /// Retrieves the "hover" sprite for the specified puzzle number and color.
    /// </summary>
    /// <param name="puzzleNumber">The puzzle number (1-99).</param>
    /// <param name="isBlack">Indicates whether the sprite is black (<see langword="true"/>) or white (<see langword="false"/>).</param>
    /// <returns>The "hover" sprite, or <see langword="null"/> if it could not be loaded.</returns>
    public static Sprite? GetHoverSprite(int puzzleNumber, bool isBlack)
    {
        return GetSpriteFromPath(_hoverSprites, _hover, puzzleNumber, isBlack);
    }

    /// <summary>
    /// Generates the file name for a puzzle sprite based on its number and color.
    /// </summary>
    /// <param name="puzzleNumber">The puzzle number (1-99).</param>
    /// <param name="isBlack">Indicates whether the sprite is black (<see langword="true"/></see>) or white (<see langword="false"/>).</param>
    /// <returns>The formatted file name (e.g., "black-01.png").</returns>
    private static string GetFileName(int puzzleNumber, bool isBlack)
    {
        return $"{(isBlack ? "black" : "white")}-{puzzleNumber:D2}";
    }

    /// <summary>
    /// Retrieves a sprite specified by <paramref name="puzzleNumber"/> and <paramref name="isBlack"/>. This method caches the results.
    /// </summary>
    /// <param name="map">The dictionary to cache loaded sprites.</param>
    /// <param name="directoryName">The subdirectory name within the puzzle sprites directory.</param>
    /// <param name="puzzleNumber">The puzzle number (1-99).</param>
    /// <param name="isBlack">Indicates whether the sprite is black (<see langword="true"/>) or white (<see langword="false"/>).</param>
    /// <returns>The loaded sprite, or <see langword="null"/> if the sprite could not be loaded.</returns>
    private static Sprite? GetSpriteFromPath(Dictionary<(int, bool), Sprite> map, string directoryName, int puzzleNumber, bool isBlack)
    {
        // Validate the puzzle number
        if (puzzleNumber < 1 || puzzleNumber > 99) {
            Debug.LogError($"Invalid puzzle number: {puzzleNumber}. Must be between 1 and 99.");
            return null;
        }

        // Check if the sprite is already cached
        if (map.ContainsKey((puzzleNumber, isBlack))) {
            return map[(puzzleNumber, isBlack)];
        }

        // Load the sprite from the specified path
        string path = $"{_puzzleSpritesDirectory}/{directoryName}/{GetFileName(puzzleNumber, isBlack)}";
        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite == null) {
            Debug.LogError($"Failed to load sprite from path: {path}");
            return null;
        }

        // Cache the loaded sprite
        map[(puzzleNumber, isBlack)] = sprite;
        return sprite;
    }

    #endregion
}
