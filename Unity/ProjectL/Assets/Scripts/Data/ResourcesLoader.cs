#nullable enable

namespace ProjectL.Data
{
    using ProjectLCore.GamePieces;
    using System.Collections.Generic;
    using UnityEngine;
    using System;

    public enum PuzzleSpriteType
    {
        BorderDim,
        BorderBright,
        Borderless,
        WithBackground
    }

    public static class ResourcesLoader
    {
        #region Constants

        private const string _puzzleSpritesDirectory = "PuzzleSprites";

        private const string _puzzleFilePath = "puzzles";

        private const string _tetrominoSpritesDirectory = "TetrominoSprites";

        #endregion

        #region Fields

        private static readonly Dictionary<PuzzleSpriteType, string> _puzzleDirectoryNames = new() {
        { PuzzleSpriteType.BorderDim, "border-dim" },
        { PuzzleSpriteType.BorderBright, "border-bright" },
        { PuzzleSpriteType.Borderless, "borderless" },
        { PuzzleSpriteType.WithBackground, "with-background" }
    };

        private static readonly Dictionary<PuzzleSpriteType, Dictionary<(uint, bool), Sprite>> _puzzleSpriteCaches = new();

        private static readonly Dictionary<TetrominoShape, Sprite> _tetrominoSpriteCaches = new();

        #endregion

        #region Methods

        /// <summary>
        /// Reads the contents of the puzzle configuration file.
        /// </summary>
        /// <param name="result">When this methods succeeds, contains the read text; or <see cref="String.Empty"/> on failure.</param>
        /// <returns>
        /// <see langword="true"/> if the file was found; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool TryReadPuzzleFile(out string result)
        {
            var textAsset = Resources.Load<TextAsset>(_puzzleFilePath);
            if (textAsset == null) {
                Debug.LogError($"Failed to load puzzle file: {_puzzleFilePath}");
                result = string.Empty;
                return false;
            }
            result = textAsset.text;
            return true;
        }

        /// <summary>
        /// Retrieves the tetromino sprite for the given shape.
        /// </summary>
        /// <param name="shape">The shape of the tetromino.</param>
        /// <param name="result">When this methods succeeds, contains the loaded sprite; or <see langword="null"/> on failure.</param>
        /// <returns>
        /// <see langword="true"/> if the sprite was found; otherwise, <see langword="false"/>.
        ///</returns>
        public static bool TryGetTetrominoSprite(TetrominoShape shape, out Sprite? result)
        {
            result = GetSprite(shape);
            if (result == null) {
                return false;
            }
            return true;

            static Sprite? GetSprite(TetrominoShape shape)
            {
                // check cache first
                if (_tetrominoSpriteCaches.ContainsKey(shape)) {
                    return _tetrominoSpriteCaches[shape];
                }
                // Load the sprite from the specified path
                string path = $"{_tetrominoSpritesDirectory}/{shape}";
                Sprite sprite = Resources.Load<Sprite>(path);
                if (sprite == null) {
                    Debug.LogError($"Failed to load sprite from path: {path}");
                    return null;
                }
                _tetrominoSpriteCaches[shape] = sprite; // cache the loaded sprite
                return sprite;
            }
        }

        /// <summary>
        /// Retrieves the puzzle sprite based on the specified puzzle and sprite type.
        /// </summary>
        /// <param name="puzzle">The <see cref="Puzzle"/> object containing the puzzle number and color information.</param>
        /// <param name="type">The type of sprite to load.</param>
        /// <param name="result">When this methods succeeds, contains the loaded sprite; or <see langword="null"/> on failure.</param>
        /// <returns>
        /// <see langword="true"/> if the sprite was found; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool TryGetPuzzleSprite(Puzzle puzzle, PuzzleSpriteType type, out Sprite? result)
        {
            result = GetSprite(type, puzzle.PuzzleNumber, puzzle.IsBlack);
            if (result == null) {
                return false;
            }
            return true;

            static Sprite? GetSprite(PuzzleSpriteType type, uint puzzleNumber, bool isBlack)
            {
                // Check cache first
                if (!_puzzleSpriteCaches.ContainsKey(type)) {
                    _puzzleSpriteCaches[type] = new Dictionary<(uint, bool), Sprite>();
                }
                var map = _puzzleSpriteCaches[type];
                if (map.ContainsKey((puzzleNumber, isBlack))) {
                    return map[(puzzleNumber, isBlack)];
                }

                // Load the sprite from the specified path
                string fileName = $"{(isBlack ? "black" : "white")}-{puzzleNumber:D2}";
                string path = $"{_puzzleSpritesDirectory}/{_puzzleDirectoryNames[type]}/{fileName}";
                Sprite sprite = Resources.Load<Sprite>(path);
                if (sprite == null) {
                    Debug.LogError($"Failed to load sprite from path: {path}");
                    return null;
                }

                // Cache the loaded sprite
                map[(puzzleNumber, isBlack)] = sprite;
                return sprite;
            }
        }

        #endregion
    }
}