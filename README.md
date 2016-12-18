# SimpleBoardGames
A collection of simple two-player board games. Works on both mobile and PC. Currently only local multiplayer is supported (i.e. all on one device), but online play is coming soon!

One nice feature of this project is automatic saving/loading of the game's state. Every time a player starts their turn, the current state of the game is automatically saved to a specific file. When the game opens again later, it first checks whether the save file exists, and if so, it resumes that game. This means it's effectively crash-proofed!

The underlying framework is designed to make it as easy as possible to add your own games, as long as you are proficient in C#/Unity. Towards the bottom of this readme are instructions for how to do this.


# Current Games

##Fitchneil

[A very old Celtic game](http://www.knauer.org/mike/sca/classes/fitchneal.html).

The rules are as follows:

* All pieces move like Rooks in chess (any amount of spaces orthogonally)
* First player is the attacker, second player is the defender/king
* A normal (non-king) piece is captured when the enemy places two pieces on opposite sides of him
* The king piece is captured when the enemy surrounds him on all four sides. One of those sides may be occupied by the "throne" square instead of an enemy.
* Only the king piece may land on the center "throne" square, though all pieces may move through it
* Defenders/King win by getting the King out to the edge of the board, or by killing all attackers
* Attackers win by surrounding the King on all sides

##Tron's Tour

A game invented by William Manning and Julia Wlochowski. It takes place on a rectangular board of any size you want (the project defaults to 6x9).

The rules are as follows:

* Each player gets one piece and starts on opposite sides of the board.
* The pieces move in an L-shape, like a knight in chess: two spaces along an axis, and one space along the other axis.
* Pieces can never move onto a space that has already been occupied by a piece in the past.
* The first player to not have any legal moves during his turn loses.


# How to implement a new game

All scripts are in the "Scripts" folder.
There are a large number of scripts designed to help the implementation of virtually any board game. First, there are various independent utility scripts:

* **KillAfterTime**: Destroys the target GameObject/Component after a set amount of time. If no object is set, the component's own GameObject will be the target.
* **Singleton**: Inherit from this to create a MonoBehaviour that should be unique in the game world and is easily accessible via a static property `Instance`.
To declare a singleton class, use the following syntax:
```
public class MySingleton : Singleton<MySingleton>
```
* **Vector2i**: A 2D vector of integers. Often useful for representing a coordinate on a 2D grid.
* **Vector3i**: The 3D version of Vector2i.
* **SpritePool**: A singleton component that stores sprite GameObjects for rapid creation/destruction. Use this when you need to create/delete lots of simple sprite objects very often, such as highlighting potential moves for the user to select from.
* **Stat**: A piece of data that is closely tracked (i.e. whenever it changes, an event is raised).

There are also a collection of scripts specifically for building Board Games in the *BoardGames* folder/namespace. There are two "layers" to this namespace to prevent a strong coupling between game logic and rendering/input:

* The basic gameplay code, ignoring Unity-side things like GameObjects
* The Unity-side things for input/rendering, in the sub-namespace *BoardGames.UnityLogic*

The gameplay code has no idea how it's being rendered/manipulated; it merely exposes a number of events that the *UnityLogic* code subscribes to (this is where the `Stat<>` class comes in handy).

To implement your own board game, you will generally do the following things:
1. Inherit from the classes in the *BoardGames* namespace to create your game logic.
2. Add a new Unity scene and inherit from the classes in the *BoardGames.UnityLogic* namespace for input and rendering stuff.

Use the following steps as a guideline (and check out one of the included games for a finished example):

1. Create a folder for all your game scripts.
2. Create/implement a `Piece` class that inherits from `BoardGames.Piece<>`, a `Board` class that inherits from `BoardGames.Board<>`, and any number of classes inheriting from `BoardGame.Action<>`, which represent specific moves a player can make.
  * The `LocationType` generic type used in these definitions is whatever data structure represents a position on your game board. Games that take place on a simple 2D grid should just use `Vector2i`.
3. Inherit from `BoardGames.UnityLogic.PieceRenderer<>` to create a sprite that follows the `Piece<>` stored in the `ToTrack` field, and runs coroutines to animate its actions. Note that this class inherits from `InputResponder`, which allows it to know when it's being clicked on or dragged, as long as it has a `Collider2D` component and there's an `InputManager` component somewhere in the scene.
4. Inherit from `BoardGames.UnityLogic.PieceDispatcher<>` to create a singleton that automatically creates/destroys pieces as needed during the game.
5. Inherit from `BoardGames.UnityLogic.GameMode_Offline<>` to create the local multiplayer controller that manages switching turns and saving/loading the game file. This would also be a good place to force the orientation of your game on mobile. Once multiplayer is added, there will be other GameMode types you can inherit from.
6. Add a new scene to the "Scenes" folder for your game, and add all necessary objects to that scene so that it can be played from scratch (i.e. without any other scenes running first).
7. Add a new method in "Scripts/MainMenuController.cs" that loads your scene, then add a UI button in the MainMenu scene which calls that method.
8. Add your scene to the list of scenes in the build menu.