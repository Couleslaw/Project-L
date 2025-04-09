<link rel='stylesheet' href='../css/markdown-alert.css'/>
<link rel='stylesheet' href='../css/code-highlight.css'/>

# AI Player Guide

This guide explains how to implement your own AI player for the game.

{% include important.html content="
Before reading further, please make sure that you know the rules of the game, which can be found in the [user guide](../UserDocs/index). I also highly recommend reading the [documentation](../TechnicalDocs/core/index) for the *Project-L Core* library.
"%}

## Overview

Your AI player needs to inherit from the [AIPlayerBase](../ProjectLCoreDocs/html/T_ProjectLCore_Players_AIPlayerBase.htm) abstract class and implement the following methods:

- [GetAction](../ProjectLCoreDocs/html/M_ProjectLCore_Players_AIPlayerBase_GetAction.htm) - chooses the next action of the AI player,
- [GetReward](../ProjectLCoreDocs/html/M_ProjectLCore_Players_AIPlayerBase_GetReward.htm) - selects a reward for a completed puzzle,
- [Init](../ProjectLCoreDocs/html/M_ProjectLCore_Players_AIPlayerBase_Init.htm) - initializes the AI player.

Then export your player as a DLL and add a section containing information about it to the `aiplayers.ini` file. The entry for your AI player should look something like this:

```ini
[My_AIPlayer]
dll_path = "path/to/your/dll/AwesomeAI.dll"
name = "Awesome"
init_path = "path/to/your/init/file/or/folder" ; optional
```

The section name can be anything, its just for your reference. The properties are:

- `dll_path` is the path to the DLL file containing your AI player.
- `name` is the name of your AI player as will be displayed in game. The
- `init_path` (optional) is the path to the file or folder containing the initialization data for your AI player. This path will be passed to your player as an argument of the `Init` method.

Your AI player should now appear in the list of available player types.

TODO: specify where exactly the `aiplayers.ini` file is located.

## Step by Step Guide

This guide assumes that you are using Visual Studio to implement your AI player. If you are using a different IDE, the steps may be slightly different, but the general idea is the same.

### Create a New Project

First, you need to create a new **Class Library** project and add the _Project-L Core_ library as a reference. You can do this in two ways.

#### Downloading the DLL (recommended)

Download the _Project-L Core_ library from the [releases](https://github.com/Couleslaw/Project-L/releases/latest) page. If you are using Visual Studio, you can create a new project and add it as a dependency. To do this, follow these steps:

1. Create a new **Class Library** project in Visual Studio. You will implement your AI player here.
2. Download `ProjectLCore.dll` and `ProjectLCore.xml` from [releases](https://github.com/Couleslaw/Project-L/releases/latest), create a folder in your project called `lib` and copy the files there.
3. Right-click on the project in the Solution Explorer and select **Add** > **Project Reference** > **Browse** and find `ProjectLCore.dll`.

#### Cloning the repository

Another option is to clone the github repository, and add your AI player directly to the solution containing the _Project-L Core_ library. This way you will have access to the source code of the library, including the example projects which we will talk about layer. However, this approach is not recommended unless you really need it, as the repository is quite large and contains a lot of files you don't need.

To do this, first clone the repository:

```bash
git clone https://github.com/Couleslaw/Project-L.git
```

Then follow these steps:

1. Open the `ProjectL-CLI/ProjectLCore.sln` file in Visual Studio.
2. Add a new **Class Library** project to the solution. You will implement your AI player here.
3. Right-click on the project in the Solution Explorer and select **Add** > **Project Reference** and Select `ProjectLCore`.

### Implement Your AI Player

As mentioned in the [overview](#overview), your AI player needs to inherit from the `AIPlayerBase` class and implement the following methods:

- `GetAction` - chooses the next action of the AI player,
- `GetReward` - selects a reward for a completed puzzle,
- `Init` - initializes the AI player. If your player doesn't need any initialization, this method can have an empty body.

{% include note.html content="
If you haven't read the [documentation](../TechnicalDocs/core/index) for the _Project-L Core_ library, now is the time to do it.
"%}

The [AI Player Example](https://github.com/Couleslaw/Project-L/tree/master/ProjectL-CLI/AIPlayerExample) project showcases the implementation of a simple AI player. You can find the documentation for it [here](../AIPlayerExampleDocs/index.html).

### Test Your AI Player

Before using it in the game, you will probably want to train or at least test your AI player. You will need to write a simple program that will simulate the game loop. The [AI Player Simulation](https://github.com/Couleslaw/Project-L/tree/master/ProjectL-CLI/AIPlayerSimulation) project is a simple CLI app which simulates the game between AI players. You can use this as a starting point.

{% include tip.html content="
The project also contains the `puzzles.txt` file, which contains a list of all puzzles in the game. You can easily add more puzzles (maybe for training) if it would suit your needs. The file format is explained [here](../ProjectLCoreDocs/html/T_ProjectLCore_GameLogic_PuzzleParser.htm).
"%}

### Export Your AI Player as a DLL

Select the **Release** configuration and build your project. You will find the DLL in the `bin/Release` folder. Then add your player to the `aiplayers.ini` file as explained in the [overview](#overview).

{% include note.html content="
The game will look through the DLL for a (public) non-abstract class that inherits from the `AIPlayerBase` class. If no such class is found, the player will not appear in the list of available players. If for some weird reason there are multiple such classes, the first one found will be used.
"%}

### What Could Go Wrong?

When a game stats, the `InitAsync` method is called on all AI players in the game. This methods then calls the `Init` method on your player. If your player throws an exception during this process, the game will announce that it failed to load the player and will not start.

When your player fails to provide a valid action, or the `GetAction` method throws an exception, the game acts as if it returned a `DoNothingAction`.

When your player fails to choose a valid reward, or the `GetReward` method throws an exception, the game picks the first available reward for it.
