SCENES: 
The Menu scene is the main scene Project 2 should start in.
MainGame should start unloaded.

DELIVERABLES:
The files included in this submission are for Project 2. MAC and PC
executables are provided in addition to all assets and project settings
used.

ENVIRONMENT:
This game was developed on a PC in a Windows 10 environment
using a free version of the Unity engine.

ASSETS:
All models and animations are provided by the open-source resource
Mixamo. Everything else, including the enemy AI, the world generation
algorithm, and simple geometry, were developed by me.

NOTES:
There are several things about this project that are worth noting.

First, all aspects of the maze's dimensions except the height of the
walls are fully customizable. This includes how many cells long/wide
the maze is, how thick the walls are, and how big each cell is.
These dimension can be set by the player in the main menu.

The enemy's AI is also somewhat sophisticated. When the game starts, 
the enemy will slowly naviagate to a random spot, pause for a moment, then 
navigate to another spot. This is the enemy's idle state. If the path from 
the enemy to the player is short enough (5 cells away by default) but the 
enemy cannot make direct eye contact with the player, the enemy will then enter 
a hunting state. When in, this state, the enemy will navigate to the player's cell 
at a slightly faster pace. If the enemy can make direct eye contact with the player 
either from its idle state or it's hunting state, the enemy will enter a chase state,
where they will charge directly at the player as best as they can at an even faster
pace. If the player can escape the enemy's line-of-sight and move outside of 
their, the enemy will navigate to the cell where the player was last detected
before resuming it's idle behavior.

Additionally, it should be noted that the enemy's pathing is managed with a 
matrix representation of the maze and a shortest path algorithm that were coded
in MazeGenerator.cs

The player also has a defensive option. By pressing the left mouse button, the player 
can throw a rock at the enemy to stun them for a few seconds. This allows the player to 
escape situations where they may be cornered. Animations for throwing and being stunned
by the rock are present in the game.

Lastly, there is a proper UI and both start and end transitions. When the game first
opens, the player will be presented with a menu that lists the controls and gives
them the option to start or quit out of the game. Within the game itself, there is
a proper spring gauge and an indicator telling the player whether or not they have
a rock they can throw. These UI elements will update and change colors accordingly
depending on how much longer they can spring, whether or not they can sprint 
currently, and whether or not they have a rock they can throw. When the game ends,
a win or lose message will pop up and both the player character and the enemy will
play appropriate animations before being send back to the menu.
