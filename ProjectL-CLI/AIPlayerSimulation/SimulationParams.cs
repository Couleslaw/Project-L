namespace AIPlayerSimulation
{
    using ProjectLCore.GameLogic;

    internal class SimulationParams
    {
        #region Properties

        public int NumPlayers { get; set; }

        public int NumInitialTetrominos { get; set; }

        public int NumWhitePuzzles { get; set; }

        public int NumBlackPuzzles { get; set; }

        public bool IsInteractive { get; set; }
        public bool ShouldClearConsole { get; set; }

        #endregion

        #region Methods

        public static SimulationParams GetSimulationParamsFromStdIn()
        {
            return new SimulationParams {
                NumPlayers = GetIntFromStdIn(1, 4, 2, "Number of players"),
                NumInitialTetrominos = GetIntFromStdIn(GameState.MinNumInitialTetrominos, 99, 15, "Number of initial tetrominos"),
                NumWhitePuzzles = GetIntFromStdIn(GameState.NumPuzzlesInRow, 100, 100, "Number of white puzzles"),
                NumBlackPuzzles = GetIntFromStdIn(GameState.NumPuzzlesInRow + 1, 100, 100, "Number of black puzzles"),
                IsInteractive = GetBoolFromStdIn(true, "Interactive mode"),
                ShouldClearConsole = GetBoolFromStdIn(true, "Clear console")
            };
        }

        private static int GetIntFromStdIn(int minVal, int maxVal, int defaultVal, string valueName)
        {
            while (true) {
                Console.Write($"{valueName} (min={minVal}, max={maxVal}, default={defaultVal}): ");
                string? input = Console.ReadLine();
                if (input is null || input == "") {
                    return defaultVal;
                }
                if (int.TryParse(input, out int result) && result >= minVal && result <= maxVal) {
                    return result;
                }
                Console.WriteLine($"Invalid input. Please enter a number between {minVal} and {maxVal}.");
            }
        }

        private static bool GetBoolFromStdIn(bool defaultVal, string valueName)
        {
            while (true) {
                string defaultStr = defaultVal ? "Yes" : "No";
                Console.Write($"{valueName} [y=Yes, n=No, default={defaultStr}]: ");
                string? input = Console.ReadLine()?.ToLower();
                if (input is null || input == "") {
                    return defaultVal;
                }
                if (input == "y" || input == "yes") {
                    return true;
                }
                if (input == "n" || input == "no") {
                    return false;
                }
                Console.WriteLine("Invalid input. Please enter 'y' or 'n'.");
            }
        }

        #endregion
    }
}
