<link rel='stylesheet' href='../css/markdown-alert.css'/>
<link rel='stylesheet' href='../css/code-highlight.css'/>

# AI Player Guide

This guide explains how to implement your own AI player for the game.

{% include important.html content="
Before reading further, make sure that you know the rules of the game and ideally have read the [documentation](../TechnicalDocs/core/index) for the *Project-L Core* library. You can find the rules of the game in the [user guide](../UserDocs/index).
"%}

## Overview

Your AI player needs to inherit from the [AIPlayerBase](../ProjectLCoreDocs/html/T_ProjectLCore_Players_AIPlayerBase.htm) abstract class and implement the following methods:

- [GetAction](../ProjectLCoreDocs/html/M_ProjectLCore_Players_AIPlayerBase_GetAction.htm) - chooses the next action of the AI player,
- [GetReward](../ProjectLCoreDocs/html/M_ProjectLCore_Players_AIPlayerBase_GetReward.htm) - selects a reward for a completing a puzzle,
- [Init](../ProjectLCoreDocs/html/M_ProjectLCore_Players_AIPlayerBase_Init.htm) - initialized the AI player.

Then export the DLL and add info about it to the `aiplayers.ini` file. The entry for your AI player should look like this:

```ini
[My_AIPlayer]
dll_path = "absolute/path/to/your/dll/AwesomeAI.dll"
name = "Awesome"
init_path = "absolute/path/to/your/init/file/or/directory" ; optional
```

The section name can be anything, its just for your reference. The properties are:

- `dll_path` is the absolute path to the DLL file containing your AI player.
- `name` is the name of your AI player as will be displayed in game. The
- `init_path` is the absolute path to the file or directory containing the initialization data for your AI player. This property is optional.

TODO: specify where exactly the `aiplayers.ini` file is located.

## Step by Step Guide

### Creating a project

First you need to get your hands on the _Project-L Core_ library. You can do this in two ways.

TODO.

The [AI Player Example](https://github.com/Couleslaw/Project-L/tree/master/ProjectL-CLI/AIPlayerExample) project showcases the implementation of a simple AI player. You can find the documentation for it [here](../AIPlayerExampleDocs/index.html).

The [AI Player Simulation](https://github.com/Couleslaw/Project-L/tree/master/ProjectL-CLI/AIPlayerSimulation) project is a simple CLI app you can use as a starting point for testing your AI player.
