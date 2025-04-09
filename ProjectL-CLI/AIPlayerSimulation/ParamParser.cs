namespace AIPlayerSimulation
{
    using IniParser;
    using IniParser.Model;
    using ProjectLCore.GameLogic;
    using ProjectLCore.Players;
    using System.Reflection;

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
        /// <param name="iniFilePath">Path to the AI player ini file.</param>
        /// <returns>List of uninitialized AI players.</returns>
        public static List<Player> GetPlayersFromStdIn(int numPlayers, string iniFilePath)
        {
            List<Tuple<Type, string>> playerTypes = GetAvailablePlayerTypes(iniFilePath);

            if (playerTypes.Count == 0) {
                throw new Exception("No AI players found in the specified ini file.");
            }

            // list available AI players
            Console.WriteLine("Available AI players:");
            for (int i = 0; i < playerTypes.Count; i++) {
                Console.WriteLine($"{i + 1}: {playerTypes[i].Item2}");
            }
            Console.WriteLine();

            // prompt user to pick players
            List<Player> players = new();

            for (int i = 0; i < numPlayers; i++) {
                string name = GetStringFromStdIn(_defaultPlayerNames[i % _defaultPlayerNames.Length], $"Name of player {i + 1}");
                int playerTypeIndex = GetIntFromStdIn(1, playerTypes.Count, 1, $"Type of player {i + 1}");

                // cerate player
                Player player = (Activator.CreateInstance(playerTypes[playerTypeIndex - 1].Item1) as Player)!;
                player.Name = name;
                players.Add(player);
            }
            return players;
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
        private static List<Tuple<Type, string>> GetAvailablePlayerTypes(string iniFilePath)
        {
            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile(iniFilePath);
            List<Tuple<Type, string>> playerTypes = new();

            // go through each section in the ini file
            foreach (SectionData section in data.Sections) {
                KeyDataCollection keyCol = section.Keys;
                string? dllPath = keyCol["dll_path"];
                string? name = keyCol["name"];
                string? initPath = keyCol["init_path"];
                if (dllPath is null || name is null) {
                    Console.WriteLine($"Invalid section in ini file: {section.SectionName}");
                    continue;
                }

                try {
                    // Load the DLL and find the type
                    Assembly assembly = Assembly.LoadFrom(dllPath);
                    Type? playerType = assembly.GetTypes().FirstOrDefault(t => !t.IsAbstract && typeof(AIPlayerBase).IsAssignableFrom(t));

                    if (playerType != null) {
                        // Add the player type to the list
                        playerTypes.Add(new(playerType, name));
                        Console.WriteLine($"Successfully loaded the player from entry '{section.SectionName}' - ({playerType.Name})");
                    }
                    else {
                        Console.WriteLine($"No valid AIPlayerBase class found in {dllPath}");
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine($"Error loading assembly {dllPath}: {ex.Message}");
                    continue;
                }
            }

            return playerTypes;
        }

        #endregion
    }
}
