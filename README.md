# CS-596 Assignment 3: Tic-Tac-Toe
This my version of a multiplayer Tic-Tac-Toe game.

### Gameplay
This is how the gameplay works in my version of Tic-Tac-Toe:
<ul>
    <li>One player selects to be the host and the other player selects to be the client by pushing the respective buttons</li>
    <li>The host is X's and the client is O's</li>
    <li>When the game is first launched the host goes first then the client.</li>
    <li>A timer starts with 30 seconds and counts down the person whose turn it is has 30 seconds to make a move</li>
    <li>After 30 seconds the player's turn is up and the turn changes to the other player</li>
    <li>After a win or a tie game the game over panel will appear showing who won, who lost or tie and give you option to play again</li>
    <li>When the play again button is clicked:
        <ul>
            <li>If the host won the game the client starts first in the next game</li>
            <li>If the client won the game then the host starts first in the next game</li>
            <li>If the host made the final move that tied the game the client goes first in the next game</li>
            <li>If the client made the final move that tied the game the host goes first in the next game</li>
        </ul>
    </li>
</ul>

### Sounds 
The following sounds are triggred in the game when:
<ol>
    <li>"Start Host" button is clicked</li>
    <li>"Start Client" button is clicked</li>
    <li>A host or client makes a move on their turn</li>
    <li>A host or client makes a move when it isn't their turn</li>
    <li>the timer first starts up to twenty seconds</li>
    <li>the timer first reaches ten seconds</li>
    <li>A host or client wins the game</li>
</ol>

### Art
The following art is included in this game:
<ul>
    <li>A texture to start host button</li>
    <li>A texture to start client button</li>
    <li>A texture to panel that the start host and start client buttons are on </li>
    <li>A texture to the tic-tac-toe board to show the grid lines </li>
    <li>A texture to the buttons on the tic-tac-toe that represent the cells to make them look more defined </li>
    <li>An "X" sprite or an "O" sprite will be displayed in a cell when a move is made</li>
    <li>A texture to the results panel</li>
    <li>A texture to the play again button</li>
</ul>

### How to play the game
Follow these steps to set up the game: 
<ol>
    <li>Download this project from this repository, save it somewhere on your device, and unzip the files.</li>
    <li>Launch Unity and open the project in Unity</li>
    <li>In Unity in the projects panel open the "Scenes" folder and open the "MainScene"</li>
    <li>In unity in the game view tab change the Display 1 resolution to 1440 X 2560</li>
    <li>In Unity select "file" > "build and run" choose somewhere on your device to save the build</li>
    <li>Now an instance of the game will launch this will act as one instance</li>
    <li>In the Unity editor click the play button this will act as the second instance of the game</li>
    <li>In one instance click the "Start Host" button this will make this instance the host</li>
    <li>In the other instance click the "Start Client" button this will make this instance the client</li>
    <li>Start Playing the game. Have Fun!</li>
</ol>


 
