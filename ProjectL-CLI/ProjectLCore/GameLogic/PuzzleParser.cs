namespace ProjectLCore.GameLogic
{
    using ProjectLCore.GamePieces;

    /// <summary>
    /// Reads puzzles from a file.
    /// Each puzzle is encoded in the following way:
    /// <list type="bullet">
    ///     <item><c>I</c> (identifier) <c>B</c>/<c>W</c> (black/white) <c>puzzleNumber</c></item>
    ///     <item><c>R</c> (reward) <c>score</c> <c>tetromino</c> (<c>O1</c>/<c>O2</c>/<c>I2</c>/<c>I3</c>/<c>I4</c>/<c>L2</c>/<c>L3</c>/<c>Z</c>/<c>T</c>)</item>
    ///     <item>five rows starting with <c>P</c> encoding the puzzle; <c>#</c> = filled cell, <c>.</c> = empty cell</item>
    /// </list>
    /// Example:
    /// <example><code language="none">
    ///     I B 13
    ///     R 5 O1
    ///     P ##..#
    ///     P ....#
    ///     P #....
    ///     P #....
    ///     P #..##
    /// </code></example>
    /// This example encodes a black puzzle with number 13, reward of 5 points and <c>O1</c> tetromino.
    /// The puzzle color and puzzle number together uniquely identify the file in which the puzzle image is stored.
    /// </summary>
    /// <param name="path">The path to the puzzle configuration file.</param>
    public class PuzzleParser(string path) : IDisposable
    {
        #region Constants

        /// <summary>The number of lines that encodes the puzzle image.</summary>
        private const int _numPuzzleLines = 5;

        #endregion

        #region Fields

        private readonly char[] _specialChars = ['I', 'R', 'P'];

        private StreamReader _reader = new StreamReader(path);

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
        /// <exception cref="System.ArgumentException">
        /// Invalid puzzle configuration file. Line starting with special character is empty.
        /// or
        /// Duplicate identifier line.
        /// or
        /// Duplicate reward line.
        /// or
        /// Too many puzzle lines.
        /// </exception>
        public Puzzle? GetNextPuzzle()
        {
            // parsing identifiers
            bool? isBlack = null;
            int puzzleNum = 0;

            // parsing reward
            TetrominoShape? tetromino = null;
            int score = 0;

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
                if (line == null) {
                    throw new ArgumentException($"Invalid puzzle configuration file. Line {lineNum} starting with special character {firstChar} is empty.");
                }
                string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                // parse the line
                try {
                    switch (firstChar) {
                        case 'I': {
                            if (isBlack is not null) {
                                throw new ArgumentException("Duplicate identifier line.");
                            }
                            (isBlack, puzzleNum) = ParseIdentifier(parts);
                            break;
                        }
                        case 'R': {
                            if (tetromino is not null) {
                                throw new ArgumentException("Duplicate reward line.");
                            }
                            (score, tetromino) = ParseReward(parts);
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
                catch (ArgumentException e) {
                    throw new ArgumentException($"Invalid puzzle configuration file. Line {lineNum}: {e.Message}");
                }
            }

            if (!isFinished()) {
                return null;
            }

            return CreatePuzzle(isBlack!.Value, puzzleNum, score, tetromino!.Value, image!.Value);
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
        protected virtual Puzzle CreatePuzzle(bool isBlack, int puzzleNum, int score, TetrominoShape tetromino, BinaryImage image)
        {
            return new Puzzle(image, score, tetromino, isBlack);
        }

        /// <summary>
        /// Parses the identifier line. Should be in the format I B/W puzzleNumber.
        /// </summary>
        /// <param name="line">Words in the line after 'I'.</param>
        /// <returns><c>(isBlack, num)</c> where <c>isBlack</c> specifies the color of the puzzle and <c>num</c> is the file number of this puzzle.</returns>
        /// <exception cref="System.ArgumentException">Invalid line format.</exception>
        private static Tuple<bool, int> ParseIdentifier(string[] line)
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
            bool success = int.TryParse(line[1], out int puzzleNumber);
            if (!success || puzzleNumber < 0) {
                throw new ArgumentException($"Invalid puzzle number: {line[1]}");
            }

            return new Tuple<bool, int>(isBlack, puzzleNumber);
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
        /// Parses one line of the puzzle. Should be in the format P 0bxxxxx.
        /// After parsing all 5 lines, the integer encodes the image where the top left corner corresponds to the most significant bit.
        /// </summary>
        /// <param name="currentImage">The value of what has already been parsed.</param>
        /// <param name="line">Words on the line after 'P'.</param>
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
            string numericEncoding = line[0].Replace('#', '1').Replace('.', '0');
            bool success = int.TryParse(numericEncoding, out int imagePart);
            if (!success || imagePart < 0) {
                throw new ArgumentException($"Invalid image line: {line[0]}");
            }

            return currentImage << 5 | imagePart;
        }

        #endregion
    }
}
