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

Then export your player as a Class Library (DLL) targeting **.NET Standard 2.1** (see [Technical Requirements](#technical-requirements) below) and add a section containing information about it to the `aiplayers.ini` file. The entry for your AI player should look something like this:

```ini
[My_AIPlayer]
dll_path = path/to/your/dll/AwesomeAI.dll
name = Awesome
init_path = path/to/your/init/file/or/folder ; optional
```

The section name can be anything, its just for your reference. The properties are:

- `dll_path` is the path to the DLL file containing your AI player.
- `name` is the name of your AI player as will be displayed in game. The
- `init_path` (optional) is the path to the file or folder containing the initialization data for your AI player. This path will be passed to your player as an argument of the `Init` method.

Your AI player should now appear in the list of available player types.

TODO: specify where exactly the `aiplayers.ini` file is located.

## Technical Requirements

{% include important.html content="
The main game is built using the Unity engine. To ensure your AI player DLL can be loaded and run correctly by Unity, it **must** target the **.NET Standard 2.1** API specification. This ensures compatibility with Unity's .NET runtime environment. Furthermore, this generally restricts you to using **C# 8.0 features** (though C# 9 might work as well, see the [Unity docs](https://docs.unity3d.com/Manual/csharp-compiler.html); C# 8 is safer). **Do not use C# 10 or newer features** like `record struct`, `global using`, or C# 12's primary constructors, as they are incompatible and may break loading in Unity.
"%}

You will configure this when creating your project in Visual Studio, as detailed in the [Create a New Project](#create-a-new-project) section.

## Step by Step Guide

This guide assumes that you are using Visual Studio (2019 or 2022 recommended) to implement your AI player. If you are using a different IDE, the steps may be slightly different, but the general idea is the same.

### Create a New Project

First, you need to create a new **Class Library** project that targets **.NET Standard 2.1** and add the _Project-L Core_ library as a reference.

#### Setting up the Project and Target Framework

1. Create a new **Class Library** project in Visual Studio
   - **Important:** Look for a template description similar to "A project for creating a class library that targets .NET or .NET Standard". **Avoid** templates explicitly named "Class Library (.NET Framework)".
2. Name your project (e.g., `MyAwesomeAI`).
3. On the "Additional information" screen, you **must** select **.NET Standard 2.1**
   - _If **.NET Standard 2.1** is not listed:_ You may need to install the necessary .NET SDKs. Installing the [.NET Core 3.1 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/3.1) or any newer SDK (like [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)) should provide the required targeting packs. After installation, restart Visual Studio and try creating the project again.

#### Verifying/Modifying Target Framework and Settings (After Creation)

If you need to check or change the target framework after the project is created, double click the `.csproj` file in Solution Explorer to open it. You should see something like this:

```xml
<PropertyGroup>
    <OutputType>Library</OutputType>
    <TargetFramework>netstandard2.1</TargetFramework>
    <ImplicitUsings>disable</ImplicitUsings>
    <LangVersion>8.0</LangVersion>
</PropertyGroup>
```

`LangVersion` is implicitly set to `8.0` in the project template. You can set it to `9.0`, but be careful about the risks discussed in the [Technical Requirements](#technical-requirements) section. **Do not set it to `10.0` or higher**.

#### Adding the Project-L Core Reference

Now, add the _Project-L Core_ library reference using one of the following methods:

##### Downloading the DLL (recommended)

1.  Download `ProjectLCore.dll` and `ProjectLCore.xml` from the [releases](https://github.com/Couleslaw/Project-L/releases/latest) page.
2.  Create a folder in your project (e.g., named `lib`) and copy the downloaded files there.
3.  Right-click on the project in the Solution Explorer and select **Add** > **Project Reference** > **Browse** and find `ProjectLCore.dll`.

##### Cloning the repository

(Only recommended if you need the source code or examples directly in the solution).

1.  Clone the repository:
    ```bash
    git clone https://github.com/Couleslaw/Project-L.git
    ```
2.  Open the `ProjectL-CLI/ProjectLCore.sln` file in Visual Studio.
3.  Add your newly created **Class Library** project to this solution (**File** > **Add** > **Existing Project**) or create it directly within the solution.
4.  Right-click on the project in the Solution Explorer and select **Add** > **Project Reference** and Select `ProjectLCore`.

### Implement Your AI Player

As mentioned in the [overview](#overview), your AI player needs to inherit from the `AIPlayerBase` class and implement the following methods:

- `GetAction` - chooses the next action of the AI player,
- `GetReward` - selects a reward for a completed puzzle,
- `Init` - initializes the AI player. If your player doesn't need any initialization, this method can have an empty body.

{% include note.html content="
Remember the C# 8.0 language feature limitations mentioned in the [Technical Requirements](#technical-requirements). Avoid newer syntax. If you haven't read the [documentation](../TechnicalDocs/core/index) for the _Project-L Core_ library, now is the time to do it.
"%}

The [AI Player Example](https://github.com/Couleslaw/Project-L/tree/master/ProjectL-CLI/AIPlayerExample) project showcases the implementation of a simple AI player. You can find the documentation for it [here](../AIPlayerExampleDocs/index.html).

### Test Your AI Player

Before using it in the game, you will probably want to train or at least test your AI player. You will need to write a simple program that will simulate the game loop. The [AI Player Simulation](https://github.com/Couleslaw/Project-L/tree/master/ProjectL-CLI/AIPlayerSimulation) project is a simple CLI app which simulates the game between AI players. You can use this as a starting point.

{% include tip.html content="
The simulation project also contains the `puzzles.txt` file, which contains a list of all puzzles in the game. You can easily add more puzzles (maybe for training) if it would suit your needs. The file format is explained [here](../ProjectLCoreDocs/html/T_ProjectLCore_GameLogic_PuzzleParser.htm).
"%}

### Export Your AI Player as a DLL

Select the **Release** configuration and build your project. You will find the DLL in the `bin/Release` folder. Then add your player to the `aiplayers.ini` file as explained in the [overview](#overview).

{% include note.html content="
The game will look through the DLL for a (public) non-abstract class that inherits from the `AIPlayerBase` class. If no such class is found, or if the DLL cannot be loaded due to incorrect targeting (not .NET Standard 2.1) or other errors, the player will not appear in the list of available players. If for some weird reason there are multiple valid AI player classes, the first one found will be used.
"%}

Before adding your player to the game, it is a good idea to test it in the `AIPlayerSimulation` project. If your player works there, it should work in the game as well. It it doesn't, it will tell you what went wrong.

### What Could Go Wrong?

#### Failure to Load Player

The game might fail to list your player if the DLL could not be loaded. Common reasons include:

- **Incorrect Target Framework:** The DLL was not compiled targeting **.NET Standard 2.1**.
- **Missing Dependencies:** Your DLL depends on other libraries that are not available to the game.
- **Corrupted DLL:** The DLL file is incomplete or corrupted.
- **Duplicate Assembly Name:** The assembly name of your DLL conflicts with another loaded assembly. You can change the assemble name in the project properties in Visual Studio.

If your player _is_ listed but fails during startup, the `Init` method likely threw an exception. The game will announce this failure and will not start.

#### Invalid Actions

When your player fails to provide a valid action, or the `GetAction` method throws an exception, the game acts as if it returned a `DoNothingAction`.

#### Invalid Rewards

When your player fails to choose a valid reward, or the `GetReward` method throws an exception, the game picks the first available reward for it.
