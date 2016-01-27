# SimpleBoardGames
A collection of simple local multiplayer board games. The system is designed for mobile but playable on PC.


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

A game invented by William Manning and Julia Wlochowski. It takes place on a rectangular board of any size you want (the project uses 6x9).

The rules are as follows:

* Each player gets one piece and starts on opposite sides of the board.
* The pieces move like a knight in chess: two hops along one direction, then one hop along another direction.
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
* **SpritePool** A singleton component that stores sprite GameObjects for rapid creation/destruction. Use this when you need to create/delete lots of simple sprite objects very often.

There are also a collection of scripts specifically for building Board Games in the "General Gameplay" folder. They are all defined inside the "BoardGames" namespace.

Use the following steps to implement your own board game:

1. Use the same namespace and folder for all your game scripts. This keeps them separate from other games, which will likely share some class/file names.
2. Create/implement a `Board` class that inherits from `BoardGames.Board<>`, a `Movement` class that implements `BoardGame.IMovement<>`, and a `Piece` class that inherits from `BoardGame.Piece<>`. Note that `Board` is a Singleton.
  * The `LocationType` generic parameter used in these definitions is whatever data structure represents a position on your game board. Games that take place on a simple 2D grid should just use `Vector2i`.
  * If your game involves capturing pieces, you probably want to add an extra method `GetCaptures(Movement m)` to your `Board` that gets all captures that would occur by doing the given move, and add a `Captures` field to your `Movement` struct that stores every capture that would happen if that move were to be executed.
  * Note that the `Piece` class can be dragged or clicked on by the mouse. This is handled by the `InputManager` component, described below.
3. Create/implement a `StateManager` and base `State` class, inheriting from `BoardGames.StateManager<>` and `BoardGames.State<>` respectively, for your game logic state machine. Most games will have a simple `StateManager` that just sets the initial state on `Start()`.
4. Create various game states to control game flow.
  * When the game ends, you should return to the "MainMenu" scene.
5. For ease of use, open up "Scripts/Editor/SceneSetups.cs" and add a custom method that createsGameObjects with all the necessary classes for the start of the scene. Use the methods that already exist in the class as a template.
6. Create a new scene in "Assets/Scenes" for your game and call your custom method from the previous step by clicking "Board Games>[your custom method]" in the taskbar. Then set up the camera, game board, and various member fields in the controller object.
7. Add a new method in "Scripts/MainMenuController.cs" that loads your game scene, then add a button in the MainMenu scene which calls that method.
8. Add your scene to the list of scenes in the build menu.