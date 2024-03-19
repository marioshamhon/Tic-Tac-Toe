using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    //this network variable keeps track of whose turn it is
    public NetworkVariable<int> currentTurn = new NetworkVariable<int>(0);

    //this is an instance of the GameManager script this is used to access methods or variables in the GameManager script from the BoardManager script
    public static GameManager gameManagerInstance;

    //this variable represents the tic-tac-toe board game prefab
    [SerializeField] private GameObject boardPrefab;

    //this represnts the new board to be generated 
    private GameObject newBoard;

    //this represents the panel that is displayed when someone wins/loses/ties the game
    [SerializeField] private GameObject gameEndPanel;

    //this represents the message text displayed in the panel when someone wins/loses/ties the game 
    [SerializeField] private TextMeshProUGUI messageText;

    //this represents the time displayed in the UI for the timer
    public TextMeshProUGUI timerText;

    //this represents the "Waiting for opponent" UI object
    public TextMeshProUGUI waitingForOpponent;

    //this keeps track of the actual time of the timer. Initially the timer starts at 30 seconds
    public float timerValue = 30;

    //this boolean is used to start or stop the timer in code 
    private bool timerRunning = false;

    //this is a reference to the audio source object that plays all the sounds in the game
    public AudioSource audioSource;

    //this is the sound that plays when the start host button is clicked
    public AudioClip hostButtonClickedSound;

    //this is the sound that plays when the start client button is clicked
    public AudioClip clientButtonClickedSound;

    //this is the sound that plays when the host/client makes a move during their turn
    public AudioClip validMoveSound;

    //this is the sound that plays when the host/client tries to make a move during their oppents turn
    public AudioClip invalidMoveSound;

    //this is the sound that plays when the host or client wins the game
    public AudioClip wonGameSound;

    //this is the sound that plays when the timer first starts this sounds plays for 20 seconds 
    public AudioClip startingTimerSound;

    //this is the sound that plays when the timer hits the last 10 seconds
    public AudioClip endingTimerSound;

    private void Awake()
    {
        //This checks to make sure that there is only once instance of the game manager script so that the same instance is used by all hosts/clients
        if ((gameManagerInstance != null) && (gameManagerInstance != this))
        {
            Destroy(gameObject);
        }
        else
        {
            gameManagerInstance = this; 
        }
    }

    private void Start()
    {
        //this handles when a client joins the server 
        NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
        {
            //checks if there are two players the host and the client connected
            if ((NetworkManager.Singleton.IsHost) && (NetworkManager.Singleton.ConnectedClients.Count == 2))
            {
                //hide the logo and StartButtonsPanel on the host side
                HideUIElements();

                //host tells client to hide the logo and StartButtonsPanel on the client side
                HideUIElementsClientRpc();

                //call this method to spawn the board 
                SpawnBoard();
            }
        };
    }
    private void Update()
    {
        //check if the timer is running
        if (timerRunning)
        {
            //decrement the timer value based on time passed since the last frame
            timerValue -= Time.deltaTime;

            //update the timer UI text with the new value
            UpdateTimerText();

            //this checks when there are ten seconds left on the timer to start playing a different sound
            CheckTimerTenSecondsLeft();
        }
    }

    //this method is in charge of spawning the board
    private void SpawnBoard()
    {
        //creates a new board game object using a board prefab
        newBoard = Instantiate(boardPrefab);
        
        //this finds the network object componet on the board prefab and spawns it on both the host and client side.
        newBoard.GetComponent<NetworkObject>().Spawn();
    }

    //this method makes someone the host
    public void StartHost()
    {
        //makes someone the host
        NetworkManager.Singleton.StartHost();

        //set the clip of the audiosource to the hostButtonClickedSound 
        audioSource.clip = hostButtonClickedSound;

        //play the sound when the button is clicked
        audioSource.Play();

    }

    //this method makes someone a client
    public void StartClient()
    {
        //makes someone the client
        NetworkManager.Singleton.StartClient();

        //set the clip of the audiosource to the clientButtonClickedSound 
        audioSource.clip = clientButtonClickedSound;

        //play the sound when the button is clicked
        audioSource.Play();
    }

    //this function shows the result panels and the win, lose, or tie messages on the host/client in the result of a win or tie game
   public void ShowResult(string message)
    {
        //handles when someone wins the game
        if (message.Equals("win"))
        {
            //sets the text of the message of the winner 
            messageText.text = "You Win!";

            //change the color of text to blue for the winner
            messageText.color = Color.blue;

            //sets the panel of the winner to visable 
            gameEndPanel.SetActive(true);

            //turn off waiting for opponent text
            waitingForOpponent.gameObject.SetActive(false);

            //play the sound effect when someone wins the game
            GameManager.gameManagerInstance.WonGamePlaySound();

            //calls a method to set the loser's message text
            ShowOpponentResult("You Lose");
        }

        //handles when someone ties the game
        else if (message.Equals("tie"))
        {
            //sets the message text of either the host or client
            messageText.text = "Tie Game";

            //change the color of text to red when theres a tie
            messageText.color = Color.red;

            //sets the panel of either the host or client to visable 
            gameEndPanel.SetActive(true);

            //turn off waiting for opponent text
            waitingForOpponent.gameObject.SetActive(false);

            //calls a method to set either the host or the client's message text 
            ShowOpponentResult("Tie Game");
        }
    }

    //this method is in charge of sending the appropate message to the loser
    private void ShowOpponentResult(string message)
    {
        //checks if the instance is the host 
        if (IsHost)
        {
            //host calls client so client knows to change message text and set up the results panel on their side
            ShowOpponentResultClientRpc(message);
        }

        //check if instance is the client
        else
        {
            //client calls host so host knows to change message text and set up the results panel on their side
            ShowOpponentResultServerRpc(message);
        }

    }

    //this method is called by the host it tells the client to change the message text and set up the elements in the results panel
    [ClientRpc]
    private void ShowOpponentResultClientRpc(string message)
    {
        //we need this check here because in our implementation the host is also considered a client, but we only want update clients who are not the host
        if (IsHost)
        {
            return;
        }

        //change the text of the message on the client's side
        messageText.text = message;

        //change the color of text of the loser or when there is a tie on the client's side
        messageText.color = Color.red;

        //make the panel after the game is over visable on the client's side
        gameEndPanel.SetActive(true);

        //stop the timer
        StopTimer();

        //stop playing the timer sound on the client side when the host wins or ties the game
        audioSource.Stop();
    }

    //this method is called by the client it tells the host to change the message text and set up the elements in the results panel
    [ServerRpc(RequireOwnership = false)]
    private void ShowOpponentResultServerRpc(string message)
    {
        //change the text of the message on the host's side
        messageText.text = message;

        //change the color of text of the loser or when there is a tie on the host's side
        messageText.color = Color.red;

        //make the panel after the game is over visable on the host's side
        gameEndPanel.SetActive(true);

        //stop the timer
        StopTimer();

        //stop playing the timer sound on the server side when the client wins or ties the game
        audioSource.Stop();
    }

    //this method is in charge of restarting the game
    public void RestartGame()
    {
        //this checks if the client clicked the restart button
        if (!IsHost)
        {
            //this stops the winning audio if the restart button is clicked before the winniing audio finishes playing
            audioSource.Stop();

            //make the panel invisible on the client's side
            gameEndPanel.SetActive(false);

            //client calls the host to reset the game on their side 
            RestartGameServerRpc();
        }

        //checks if host clicked the restart button
        else
        {
            //this stops the winning audio if the restart button is clicked before the winniing audio finishes playing
            audioSource.Stop();

            //destroy the current tic-tac-toe board on the host's side
            Destroy(newBoard);

            //spawn a new tic-tac-toe board on the host's side
            SpawnBoard();

            //host calls client to restart game on their side 
            RestartGameClientRpc();
        }
    }

    //this method is called by the client it tells the host to reset the elements of the game after the restart button is clicked
    [ServerRpc(RequireOwnership = false)]
    private void RestartGameServerRpc()
    {
        //destroy the board on the client's side this has to be done by the host because only the host can destroy network objects and the board prefab is a network object
        Destroy(newBoard);

        //spawn a new board on the client's side this has to be done by the host because only the host can spawn network objects and the board prefab is a network object
        SpawnBoard();

        //hide the game over panel on the host's side
        gameEndPanel.SetActive(false);
    }

    //this method is called by the host it tells the client to reset the elements of the game after the restart button is clicked
    [ClientRpc]

    private void RestartGameClientRpc()
    {
        //hide the game over panel on the client's side (and all other clients)
        gameEndPanel.SetActive(false);
    }

    //this method changes the turn 
    public void ChangeTurn()
    {
        //flip the current turn between 0 and 1
        currentTurn.Value = (currentTurn.Value == 0) ? 1 : 0;
    }

    //this method starts the timer 
    public void StartTimer()
    {
        timerRunning = true;
        
        //initialize the timer value to 30 seconds
        timerValue = 30;

        //enable the timer text
        timerText.enabled = true;

        //play the starting sound for the timer
        StartTimerPlaySound();

        //update the timer text with the initial value
        UpdateTimerText();
    }

    //this method is in charge stopping/hiding the timer
    public void StopTimer()
    {
        //disable the text property of timer UI text
        timerText.enabled = false;

        //set the timeRunning variable to false to effectively stop/disable the timer
        timerRunning = false;
    }

    //this method is in charge of updating the time in the timer UI element.
    private void UpdateTimerText()
    {
        //format the timer value as a whole number
        string updatedTime = Mathf.RoundToInt(timerValue).ToString();

        //update the timer text UI
        timerText.text = updatedTime;
    }
    
    //this method plays a sound when a valid move is made 
    public void ValidMovePlaySound()
    {
        //set the clip of the audiosource to the validMoveSound 
        audioSource.clip = validMoveSound;

        //play the sound when the button is clicked
        audioSource.Play();
    }

    //this method plays a sound when a invalid move is made 
    public void InvalidMovePlaySound()
    {
        //set the clip of the audiosource to the invalidMoveSound 
        audioSource.clip = invalidMoveSound;

        //play the sound when someone tries to make an illegal move
        audioSource.Play();
    }

    //this method plays a sound when someone wins a game
    public void WonGamePlaySound()
    {
        //set the clip of the audiosource to the wonGameSound 
        audioSource.clip = wonGameSound;

        //play the sound when the button is clicked
        audioSource.Play();
    }

    //this method plays a sound when the timer starts 
    public void StartTimerPlaySound()
    {
        //set the clip of the audiosource to the startingImerSound 
        audioSource.clip = startingTimerSound;

        //play the sound when the timer starts
        audioSource.Play();
    }

    //this method plays a sound when there are ten seconds left on the timer 
    public void CheckTimerTenSecondsLeft()
    {
        //round the timer value to an integer 
        int timeLeftOnTimer = Mathf.RoundToInt(timerValue);

        //if there are ten seconds left on the timer play a different sound 
        if (timeLeftOnTimer == 10)
        {
            //set the clip of the audiosource to the endingTimerSound 
            audioSource.clip = endingTimerSound;

            //play the sound when the timer has ten seconds left
            audioSource.Play();
        }
    }

    //this method hides the logo and StartButtonsPanel UI game objects
    private void HideUIElements()
    {
        //find the logo GameObject by name
        GameObject logo = GameObject.Find("Logo");

        //find the StartButtonsPanel GameObject by name
        GameObject startButtonsPanel = GameObject.Find("StartButtonsPanel");

        //hide the game logo 
        logo.SetActive(false);

        //hide the StartButtonsPanel 
        startButtonsPanel.SetActive(false);
    }

    //this method tells the client to hide their logo and StartButtonsPanel UI game objects on their side
    [ClientRpc]

    private void HideUIElementsClientRpc()
    {
        //we need this check here because in our implementation the host is also considered a client, but we only want clients who are not the host to hide the logo and StartButtonsPanel
        if (!IsHost)
        {
            HideUIElements();
        }
    }
}
