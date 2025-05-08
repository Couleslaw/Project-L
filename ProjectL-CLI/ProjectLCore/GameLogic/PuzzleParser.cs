namespace ProjectLCore.GameLogic
{
    using ProjectLCore.GamePieces;
    using System.IO;
    using System;
    using System.Linq;

    /// <summary>
    /// Reads puzzles from a file.
    /// Each puzzle is encoded in the following way:
    /// <list type="bullet">
    ///     <item><c>I</c> (identifier) <c>B</c>/<c>W</c> (black/white) <c>puzzleNumber</c></item>
    ///     <item><c>R</c> (reward) <c>score</c> <c>tetromino</c> (<c>O1</c>/<c>O2</c>/<c>I2</c>/<c>I3</c>/<c>I4</c>/<c>L2</c>/<c>L3</c>/<c>Z</c>/<c>T</c>)</item>
    ///     <item>five rows starting with <c>P</c> encoding the puzzle; <c>#</c> = filled cell, <c>.</c> = empty cell</item>
    /// </list>
    /// The puzzle color and puzzle number together uniquely identify the file in which the puzzle image is stored.
    /// </summary>
    /// <typeparam name="T"> The puzzle type to parse. Must be a subclass of <see cref="Puzzle"/> and have a constructor with the signature <see cref="Puzzle(BinaryImage, int, TetrominoShape, bool, uint)"/></typeparam>
    /// <remarks>
    /// The order of the lines doesn't matter and there can be an arbitrary number of lines not starting with a special character scattered throughout the puzzle definition.
    /// This however isn't recommended as it makes the file harder to read for humans.
    /// </remarks>
    /// <example><code language="none">
    ///     I B 13
    ///     R 5 O1
    ///     P ##..#
    ///     P ....#
    ///     P #....
    ///     P #....
    ///     P #..##
    /// </code>
    /// This example encodes a black puzzle with number 13, reward of 5 points and <c>O1</c> tetromino.
    /// </example>
    public class PuzzleParser<T> : IDisposable where T : Puzzle
    {
        #region Constants

        /// <summary>The number of lines that encodes the puzzle image.</summary>
        private const int _numPuzzleLines = 5;

        #endregion

        #region Fields

        private readonly char[] _specialChars = { 'I', 'R', 'P' };

        private readonly StreamReader _reader;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PuzzleParser{T}"/> class.
        /// </summary>
        /// <param name="path">The path to the puzzle configuration file.</param>
        /// <exception cref="ArgumentException">The path cannot be null or empty.</exception>"
        public PuzzleParser(string path)
        {
            if (string.IsNullOrEmpty(path)) {
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));
            }
            _reader = new StreamReader(path);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PuzzleParser"/> class.
        /// </summary>
        /// <param name="stream">The stream to parse the puzzles from.</param>
        public PuzzleParser(Stream stream)
        {
            _reader = new StreamReader(stream);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Disposes the <see cref="StreamReader"/> object used to read the file.
        /// </summary>
        /// <seealso cref="TextReader.Dispose()"/>
        /// <seealso cref="IDisposable"/>
        public void Dispose()
        {
            _reader.Dispose();
        }

        /// <summary>
        /// Parses the next puzzle from the file.
        /// </summary>
        /// <returns>The decoded puzzle or <see langword="null"/> if reached end of file.</returns>
        /// <exception cref="InvalidPuzzleException">
        /// Invalid puzzle configuration file. Line starting with special character is empty.
        /// or
        /// Duplicate identifier line.
        /// or
        /// Duplicate reward line.
        /// or
        /// Too many puzzle lines.
        /// </exception>
        public T? GetNextPuzzle()
        {
            // parsing identifiers
            bool? isBlack = null;
            uint? puzzleNum = null;

            // parsing reward
            TetrominoShape? tetromino = null;
            int? score = null;

            // parsing image
            int currentImage = 0;
            BinaryImage? image = null;
            int numPuzzleLinesRead = 0;

            // checks if all parts of the puzzle have been read
            bool isFinished() => tetromino != null && isBlack != null && image != null;

            // remember line number for error messages
            int lineNum = 0;
            while (!isFinished() && !_reader.EndOfStream) {
                lineNum++;
                char firstChar = (char)_reader.Read();
                if (!_specialChars.Contains(firstChar)) {
                    // if we haven't read newline --> readline to get to next line
                    if (firstChar != '\n') {
                        _reader.ReadLine();
                    }
                    continue;
                }

                // read the rest of the line
                string? line = _reader.ReadLine();

                // parse the line
                try {
                    ParseLine(line, firstChar);
                }
                catch (Exception e) {
                    throw new InvalidPuzzleException(
                        $"Invalid puzzle configuration file on line {lineNum}: {e.Message}",
                        isBlack, puzzleNum, score, tetromino, currentImage, numPuzzleLinesRead
                    );
                }
            }

            if (!isFinished()) {
                return null;
            }

            return CreatePuzzle(isBlack!.Value, puzzleNum!.Value, score!.Value, tetromino!.Value, image!.Value);

            void ParseLine(string? line, char firstChar)
            {
                if (line == null || line == "") {
                    throw new ArgumentException($"Line {lineNum} starting with special character {firstChar} is empty.");
                }

                string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                switch (firstChar) {
                    case 'I': {
                        if (isBlack is not null) {
                            throw new ArgumentException("Duplicate identifier line.");
                        }
                        (isBlack!, puzzleNum!) = ParseIdentifier(parts);
                        break;
                    }
                    case 'R': {
                        if (tetromino is not null) {
                            throw new ArgumentException("Duplicate reward line.");
                        }
                        (score!, tetromino!) = ParseReward(parts);
                        break;
                    }
                    case 'P': {
                        if (image is not null) {
                            throw new ArgumentException("Too many puzzle lines.");
                        }
                        currentImage = ParseImageLine(currentImage, parts);

                        // if we have read all lines, create the image
                        if (++numPuzzleLinesRead == _numPuzzleLines) {
                            // the image needs to be rotated 180 degrees
                            // we parse it so that the most significant bit is the top left corner
                            // but the image is stored so that the least significant bit is the top left corner
                            image = new BinaryImage(currentImage).RotateLeft().RotateLeft();
                        }
                        break;
                    }
                    default:
                        // should never happen
                        break;
                }
            }
        }

        /// <summary>
        /// Creates a <seealso cref="Puzzle"/> object from the parsed data.
        /// </summary>
        /// <param name="isBlack">Specifies the color of the puzzle.</param>
        /// <param name="puzzleNum">The puzzle number.</param>
        /// <param name="score">The puzzle reward score.</param>
        /// <param name="tetromino">The puzzle reward tetromino.</param>
        /// <param name="image">The puzzle image encoded with a <see cref="BinaryImage"/>.</param>
        /// <returns>The puzzle initialized from the given parameters.</returns>
        protected virtual T CreatePuzzle(bool isBlack, uint puzzleNum, int score, TetrominoShape tetromino, BinaryImage image)
        {
            // Use reflection to create an instance of T, assuming T has a constructor matching the parameters.
            return (T)Activator.CreateInstance(typeof(T), image, score, tetromino, isBlack, puzzleNum)!;
        }

        /// <summary>
        /// Parses the identifier line. Should be in the format I B/W puzzleNumber.
        /// </summary>
        /// <param name="line">Words in the line after 'I'.</param>
        /// <returns><c>(isBlack, num)</c> where <c>isBlack</c> specifies the color of the puzzle and <c>num</c> is the file number of this puzzle.</returns>
        /// <exception cref="System.ArgumentException">Invalid line format.</exception>
        private static Tuple<bool, uint> ParseIdentifier(string[] line)
        {
            if (line.Length != 2) {
                throw new ArgumentException("Invalid number of arguments.");
            }
            // parse puzzle color
            if (line[0] != "B" && line[0] != "W") {
                throw new ArgumentException($"Invalid puzzle color: {line[0]}");
            }
            bool isBlack = line[0] == "B";

            // parse puzzle number
            bool success = uint.TryParse(line[1], out uint puzzleNumber);
            if (!success || puzzleNumber < 0) {
                throw new ArgumentException($"Invalid puzzle number: {line[1]}");
            }

            return new Tuple<bool, uint>(isBlack, puzzleNumber);
        }

        /// <summary>
        /// Parses the reward line. Should be in the format R score tetromino.
        /// </summary>
        /// <param name="line">Words on the line after 'R'.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Invalid line format.</exception>"
        private static Tuple<int, TetrominoShape> ParseReward(string[] line)
        {
            if (line.Length != 2) {
                throw new ArgumentException("Invalid number of arguments.");
            }
            // parse score 
            bool success = int.TryParse(line[0], out int score);
            if (!success || score < 0) {
                throw new ArgumentException($"Invalid score: {line[0]}");
            }

            // parse tetromino
            string tetrominoString = line[1].ToUpper();
            var tetromino = tetrominoString switch {
                "O1" => TetrominoShape.O1,
                "O2" => TetrominoShape.O2,
                "I2" => TetrominoShape.I2,
                "I3" => TetrominoShape.I3,
                "I4" => TetrominoShape.I4,
                "L2" => TetrominoShape.L2,
                "L3" => TetrominoShape.L3,
                "Z" => TetrominoShape.Z,
                "T" => TetrominoShape.T,
                _ => throw new ArgumentException($"Invalid tetromino shape {line[1]}"),
            };
            return new Tuple<int, TetrominoShape>(score, tetromino);
        }

        /// <summary>
        /// Parses one line of the puzzle. Should be in the format <c>P</c> 0bxxxxx.
        /// After parsing all 5 lines, the integer encodes the image where the top left corner corresponds to the most significant bit.
        /// </summary>
        /// <param name="currentImage">The value of what has already been parsed.</param>
        /// <param name="line">Words on the line after <c>P</c>'.</param>
        /// <returns>Image representation extended by characters on the current line.</returns>
        /// <exception cref="System.ArgumentException">Invalid line format.</exception>"
        private static int ParseImageLine(int currentImage, string[] line)
        {
            // check if the line is valid
            if (line.Length != 1) {
                throw new ArgumentException("Invalid number of arguments.");
            }
            if (line[0].Length != 5) {
                throw new ArgumentException($"Invalid image line: {line[0]}");
            }

            // parse the line
            int imagePart = 0;
            foreach (char c in line[0]) {
                imagePart <<= 1;
                if (c == '#') {
                    imagePart |= 1;
                }
                if (c != '#' && c != '.') {
                    throw new ArgumentException($"Invalid character in image line: {c}");
                }
            }

            return currentImage << 5 | imagePart;
        }

        #endregion
    }

    /// <summary>
    /// Represents an error that occurred while parsing a puzzle.
    /// </summary>
    /// <seealso cref="System.Exception"/>
    internal class InvalidPuzzleException : Exception
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidPuzzleException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="isBlack">Specifies the color of the puzzle.</param>
        /// <param name="puzzleNumber">The puzzle number.</param>
        /// <param name="score">The puzzle reward score.</param>
        /// <param name="tetromino">The puzzle reward tetromino.</param>
        /// <param name="currentImage">The image part that was parsed so far.</param>
        /// <param name="numPuzzleLinesRead">The number of image coding lines (starting with <c>P</c>) read so far.</param>
        public InvalidPuzzleException(string message, bool? isBlack, uint? puzzleNumber, int? score, TetrominoShape? tetromino, int currentImage, int numPuzzleLinesRead) : base(message)
        {
            IsBlack = isBlack;
            PuzzleNumber = puzzleNumber;
            Score = score;
            Tetromino = tetromino;
            CurrentImage = currentImage;
            NumPuzzleLinesRead = numPuzzleLinesRead;
        }

        #endregion

        #region Properties

        /// <summary>
        /// <see langword="null"/> if the puzzle color was not parsed. Otherwise <see langword="true"/> if the puzzle is black, <see langword="false"/> if it is white.
        /// </summary>
        public bool? IsBlack { get; }

        /// <summary>
        /// <see langword="null"/> if the puzzle number was not parsed. Otherwise the puzzle number.
        /// </summary>
        public uint? PuzzleNumber { get; }

        /// <summary>
        /// <see langword="null"/> if the score reward was not parsed. Otherwise the score reward.
        /// </summary>
        public int? Score { get; }

        /// <summary>
        /// <see langword="null"/> if the tetromino reward was not parsed. Otherwise the tetromino reward.
        /// </summary>
        public TetrominoShape? Tetromino { get; }

        /// <summary>
        /// The image part that was parsed so far.
        /// </summary>
        public int CurrentImage { get; }

        /// <summary>
        /// The number of image coding lines (starting with <c>P</c>) read so far.
        /// </summary>
        public int NumPuzzleLinesRead { get; }

        #endregion
    }
}
