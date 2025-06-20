# Project L

**Project L** is a fast-paced strategic board game for up to 4 players created by [Boardcubator](https://www.boardcubator.com/games/project-l/). The main goal of the game is to solve puzzles by filling them in with colorful Tetris-like pieces. When you finish a puzzle, you get some points and also a new piece as a reward. By doing this, you increase your collection of pieces and can solve more difficult puzzles. If you don't have enough pieces to solve a puzzle, you can trade the pieces you have for new ones or take a new basic piece from the shared reserve.

![showcase](./docs/UserDocs/images/showcase.gif)

This project is a digital version of the game that can be played by both human and AI players.

## [Play Project L Online](https://couleslaw.github.io/Project-L/play/)

Experience the game in your browser ━ on your computer, tablet or phone.

Please note that AI players are not available in this version.

## [Download Project L](https://github.com/Couleslaw/Project-L/releases/latest)

Playing against AI players requires downloading the game, which is available for Windows, MacOS, and Linux.

One AI player comes pre-installed, and you can add custom AI players as well.

## How to Play the Game?

The [User Guide](https://couleslaw.github.io/Project-L/UserDocs/) explains the rules and shows how to play the game.

## Create Your Own AI Player

You can add your own AI player to the game by simply implementing the methods of the [AIPlayerBase](https://couleslaw.github.io/Project-L/ProjectLCoreDocs/html/T_ProjectLCore_Players_AIPlayerBase.htm) abstract class. Specifically:

- [Init](https://couleslaw.github.io/Project-L/ProjectLCoreDocs/html/M_ProjectLCore_Players_AIPlayerBase_Init.htm) – initializes the player
- [GetAction](https://couleslaw.github.io/Project-L/ProjectLCoreDocs/html/M_ProjectLCore_Players_AIPlayerBase_GetAction.htm) – chooses the next action to take
- [GetReward](https://couleslaw.github.io/Project-L/ProjectLCoreDocs/html/M_ProjectLCore_Players_AIPlayerBase_GetReward.htm) – selects a reward for completing a puzzle

More detailed information on creating AI players is available in the [AI Player Guide](https://couleslaw.github.io/Project-L/AIPlayerGuide/index).

## How Does It Work?

The Unity version of the game is built on top of the **ProjectLCore** library, which contains all the core game logic. **ProjectLCore** is completely independent from Unity, making it suitable for training AI players or building other interfaces. The Unity implementation simply provides a user interface and connects to this core library.

Because of this separation, the documentation is split into two parts:

- The [Library docs](https://couleslaw.github.io/Project-L/TechnicalDocs/core/) cover the inner workings of the [Project-L Core](https://couleslaw.github.io/Project-L/ProjectLCoreDocs/) library.
- The [Unity docs](https://couleslaw.github.io/Project-L/TechnicalDocs/unity/) explain how the Unity-based game client is implemented.

## Used Technologies

- **[Unity 6000.0.37f1](https://unity.com/)** – the game engine the game was created in. 
- **[C#](https://docs.microsoft.com/en-us/dotnet/csharp/)** – the programming language used to write the game code.
- **[.NET SDK 9.0](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)** – the framework used to build the ProjectLCore library.
- **[Visual Studio 2022](https://visualstudio.microsoft.com/vs/)** – the IDE used for writing and debugging code.
- **[Gemini](https://gemini.google.com/)** – the primary AI assistant used.
- **[NuGet for Unity](https://github.com/GlitchEnzo/NuGetForUnity)** – manages .NET packages inside Unity.
- **[Figma](https://www.figma.com/)** – used to design the game’s graphics and user interface.
- **[Sandcastle Help File Builder](https://github.com/EWSoftware/SHFB)** – generates documentation for the ProjectLCore library.
- **[DocFX](https://dotnet.github.io/docfx/)** – creates documentation for the Unity-based parts of the project.
- **[Ini-parser](https://www.nuget.org/packages/ini-parser-netstandard)** – parses the AI player configuration file.
- **[Unity Logger](https://github.com/herbou/Unity_Logger)** – adds an in-game logging feature for easier AI player debugging.

## Credits

This project is a digital implementation of **Project L**, a board game designed and published by **Boardcubator** in 2020. I do not claim ownership or credit for the original game design, art, or graphic design. The resources I have used (rulebook and puzzle graphics) can be publicly accessed on the [Boardcubator website](https://www.boardcubator.com/games/project-l/).
