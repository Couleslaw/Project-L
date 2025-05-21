<link rel='stylesheet' href='./css/markdown-alert.css'/>

# Project-L Documentation

This is the documentation for the [Project L](https://github.com/Couleslaw/Project-L) digital board game, which is a computer version of the board game created by [Boardcubator](https://www.boardcubator.com/games/project-l/).

{% include note.html content="
This project implements only the **Base game**. It doesn't implement the solo variant or any of the expansions.
"%}

## User Guide

The [User Guide](./UserDocs/index) explains the rules and how to play the game.

## Functional Specification

The [Functional Specification](./FunctionDocs/index) details all features and behavior of the game from a user's perspective.

## Technical Documentation

The Unity version of the game is built on top of the **ProjectLCore** library, which contains all the core game logic. **ProjectLCore** is completely independent from Unity, making it suitable for training AI players or building other interfaces. The Unity implementation simply provides a user interface and connects to this core library.

Because of this separation, the documentation is split into two parts:

- The [Library docs](./TechnicalDocs/core/index) cover the inner workings of the [Project-L Core](./ProjectLCoreDocs/index.html) library.
- The [Unity docs](./TechnicalDocs/unity/index) explain how the Unity-site client is implemented.

The [AI Player Guide](./AIPlayerGuide/index) explains how to create your own AI players for the game.
