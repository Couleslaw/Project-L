namespace AIPlayerSimulation
{
    using ProjectLCore.GameLogic;
    using ProjectLCore.Players;
    using System.Reflection;
    using System.Runtime.Versioning;

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
    }

    internal class ParamParser
    {
        #region Fields

        private static readonly string[] _defaultPlayerNames = { "Alice", "Bob", "Charlie", "David" };

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

        /// <summary>
        /// Creates players based on user input.
        /// </summary>
        /// <param name="numPlayers">The number of players to create.</param>
        /// <returns>List of uninitialized AI players.</returns>
        public static Dictionary<AIPlayerBase, PlayerTypeInfo> GetPlayersFromStdIn(int numPlayers)
        {
            List<PlayerTypeInfo> playerTypes = PlayerTypeLoader.AvailableAIPlayerInfos.ToList();

            if (playerTypes.Count == 0) {
                throw new Exception("No AI players found in the specified ini file.");
            }

            // list available AI players
            Console.WriteLine("\nAvailable AI players:");
            for (int i = 0; i < playerTypes.Count; i++) {
                Console.WriteLine($"{i + 1}: {playerTypes[i].DisplayName}");
            }
            Console.WriteLine();

            // prompt user to pick players
            Dictionary<AIPlayerBase, PlayerTypeInfo> players = new();

            for (int i = 0; i < numPlayers; i++) {
                string name = GetStringFromStdIn(_defaultPlayerNames[i % _defaultPlayerNames.Length], $"Name of player {i + 1}");
                int playerTypeIndex = GetIntFromStdIn(1, playerTypes.Count, 1, $"Type of player {i + 1}");
                PlayerTypeInfo playerTypeInfo = playerTypes[playerTypeIndex - 1];

                // cerate player
                AIPlayerBase player = (Activator.CreateInstance(playerTypeInfo.PlayerType) as AIPlayerBase)!;
                player.Name = name;
                players.Add(player, playerTypeInfo);
            }
            return players;
        }

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
                Console.WriteLine($"Warning: TargetFrameworkAttribute not found on assembly '{assembly.FullName}'. Cannot verify target framework. Skipping...");
                return false;
            }

            const string netStandard21Tfm = ".NETStandard,Version=v2.1";
            bool result = string.Equals(targetFrameworkAttribute.FrameworkName, netStandard21Tfm, StringComparison.OrdinalIgnoreCase);

            if (!result) {
                Console.WriteLine($"Assembly '{assembly.FullName}' targets '{targetFrameworkAttribute.FrameworkName}', expected '{netStandard21Tfm}'. Skipping...");
            }

            return result;
        }

        private static string GetStringFromStdIn(string defaultVal, string valueName)
        {
            Console.Write($"{valueName} (default={defaultVal}): ");
            string? input = Console.ReadLine();
            return input is null || input == "" ? defaultVal : input;
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
