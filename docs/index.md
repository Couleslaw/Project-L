<link rel='stylesheet' href='./css/markdown-alert.css'/>

# Project-L Documentation

This project is a computer version of the [Project L](https://www.boardcubator.com/games/project-l/) board game. It implements the game in Unity and provides an API for training intelligent agents to play it.

{% include note.html content="
This project implements only the **Base game**. It doesn't implement the solo variant or any of the expansions.
"%}

## User Guide

The [user guide](./UserDocs/index) explains the game rules and how to play it.

## Functional Specification

The [functional spec](./FunctionDocs/index) details all features and behavior of the game from a user's perspective.

## Technical Documentation

The Unity implementation of the game relies on a library, which is independent of the Unity engine and is suitable for training AI players. As a result, the documentation is divided into two parts:

- [Library docs](./TechnicalDocs/core/index) - describes the inner workings of the [Project-L Core](./ProjectLCoreDocs/index.html) library.

- [Unity docs](./TechnicalDocs/unity/index) - describes the Unity implementation of the game.

The [AI Player Guide](./AIPlayerGuide/index) explains how to create your own AI players for the game.
