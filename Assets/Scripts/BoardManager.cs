using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class BoardManager : NetworkBehaviour
{
    Button[,] buttons = new Button[3,3];

    [SerializeField] private Sprite xSprite, oSprite;

    private void Start()
    {
        if ((NetworkManager.Singleton.IsHost) && (GameManager.gameManagerInstance.currentTurn.Value == 0))
        {
            GameManager.gameManagerInstance.StartTimer();
        }

        else if ((!NetworkManager.Singleton.IsHost) && (GameManager.gameManagerInstance.currentTurn.Value == 1))
        {
            GameManager.gameManagerInstance.StartTimer();
        }
    }

    private void Update()
    {
        if ((NetworkManager.Singleton.IsHost && GameManager.gameManagerInstance.currentTurn.Value == 0) ||
            (!NetworkManager.Singleton.IsHost && GameManager.gameManagerInstance.currentTurn.Value == 1))
        {
            if (GameManager.gameManagerInstance.timerValue <= 0)
            {
                //this handles when the host's timer is zero
                if (NetworkManager.Singleton.IsHost)
                {
                    //stop the timmer on the host side  
                    GameManager.gameManagerInstance.StopTimer();

                    //change turn to clients turn
                    GameManager.gameManagerInstance.ChangeTurn();

                    //tell the client to turn their timer on
                    WhenTimerIsZeroClientRpc();
                }
                //this handles when the client's timer is zero
                else
                {
                    //stop the timer on the client's side  
                    GameManager.gameManagerInstance.StopTimer();

                    //tell the host to turn their timer on and change to their turn
                    WhenTimerIsZeroServerRpc();
                }
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        //this gets each cell inside of the board
        var cells = GetComponentsInChildren<Button>();

        //this keeps track of the index
        int index = 0;

        for(int i = 0; i < 3; i++)
        {
            for(int j = 0; j < 3; j++)
            {
                //assign the Button component at the current index directly to the buttons array
                buttons[i, j] = cells[index];
                
                index++;

                int row = i;

                int column = j;

                //When a cell is clicked in the board we call the onClickCell function
                buttons[i, j].onClick.AddListener(delegate
                {
                    OnClickCell(row, column);
                });
            }
        }
    }

    //This function handles when cell is clicked
    private void OnClickCell(int row, int column)
    {
        //checks to see if the host or client tried click a square when it wasn't their turn
        if ((NetworkManager.Singleton.IsHost && GameManager.gameManagerInstance.currentTurn.Value == 1) ||
           (!NetworkManager.Singleton.IsHost && GameManager.gameManagerInstance.currentTurn.Value == 0))
        {
            //play the sound effect when the someone tries make a move when it is not their turn
            GameManager.gameManagerInstance.InvalidMovePlaySound();
        }

        //if a button is clicked by the host
        if ((NetworkManager.Singleton.IsHost) && (GameManager.gameManagerInstance.currentTurn.Value == 0))
        {
            //change the sprite for the host
            buttons[row, column].GetComponent<Image>().sprite = xSprite;

            //Once a cell is clicked this makes it so it can't be selected again by the host
            buttons[row, column].interactable = false;

            //stops the timer for when the host makes a move
            GameManager.gameManagerInstance.StopTimer();

            //play the sound effect when the host makes a valid move
            GameManager.gameManagerInstance.ValidMovePlaySound();

            //call this method to also change the sprite on the client side and other things so the host/client have the same state or are synchronized properly
            ChangeSpriteClientRpc(row, column);

            //check to see if someone won or tied the game
            CheckResult(row, column);

            //changing to the clients turn
            GameManager.gameManagerInstance.ChangeTurn();

        }

        //if a button is clicked by the client 
        else if ((!NetworkManager.Singleton.IsHost) && (GameManager.gameManagerInstance.currentTurn.Value == 1))
        {
            //change the sprite for the client
            buttons[row, column].GetComponent<Image>().sprite = oSprite;

            //Once a cell is clicked this makes it so it can't be selected again by the client
            buttons[row, column].interactable = false;

            //stops the timer for when the client makes a move
            GameManager.gameManagerInstance.StopTimer();

            //play the sound effect when the client makes a valid move
            GameManager.gameManagerInstance.ValidMovePlaySound();

            //call this method to also change the sprite on the host side and other things so the host/client so the two sides have the same state or are synchronized properly
            ChangeSpriteServerRpc(row, column);

            //check to see if someone won or tied the game
            CheckResult(row, column);
        }
    }

    //this method synchronizes things on the client side to make both the client and the host have the same state of the game 
    [ClientRpc]
    private void ChangeSpriteClientRpc(int row, int column)
    {
        //this tells the host that the sprite has changed
        buttons[row, column].GetComponent<Image>().sprite = xSprite;

        //this tells the host that the button has been disabled
        buttons[row, column].interactable = false;

        if (!NetworkManager.Singleton.IsHost)
        {
            GameManager.gameManagerInstance.StartTimer();
        }
    }

    //this method synchronizes things on the host side to make both the client and the host have the same state of the game 
    [ServerRpc(RequireOwnership = false)]
    private void ChangeSpriteServerRpc(int row, int column)
    {
        //this tells the client that the sprite has changed
        buttons[row, column].GetComponent<Image>().sprite = oSprite;

        //this tells the client that the button has been disabled
        buttons[row, column].interactable = false;

        //this line has be here because only the host has the ability to change this network variable
        GameManager.gameManagerInstance.ChangeTurn();

        GameManager.gameManagerInstance.StartTimer();
    }

    //switch turn on the client side when the host's timer goes down to zero
    [ClientRpc]
    private void WhenTimerIsZeroClientRpc()
    {
        //start the timer on the client's side when the timer on the host side goes down to zero 
        if (!NetworkManager.Singleton.IsHost)
        {
            GameManager.gameManagerInstance.StartTimer();
        }
    }

    //switch turn on the host side when the client's timer goes down to zero
    [ServerRpc(RequireOwnership = false)]
    private void WhenTimerIsZeroServerRpc()
    {
        //start the timer on the host's side when the timer on the client side goes down to zero 
        GameManager.gameManagerInstance.StartTimer();

        //change the turn to the host's turn when the client's timer goes down to zero.
        GameManager.gameManagerInstance.ChangeTurn();
    }

    //this method checks if there is a win or a tie after every time a move is made by hosts/clients
    public void CheckResult(int row, int column)
    {
        //if there is a win 
        if (IsWinner(row, column))
        {
            //call ShowMessage and pass "win" to display win or lose on the game over panel for both the host and client
            GameManager.gameManagerInstance.ShowMessage("win");
        }

        //if there is a tie
        else if (IsGameATie())
        {
            //call ShowMessage and pass "tie" to display tie on the game over panel for both the host and client
            GameManager.gameManagerInstance.ShowMessage("tie");
        }
    }

    //this method determines if there is a winner 
    public bool IsWinner(int row, int column)
    {
        Sprite clickedButtonSprite = buttons[row, column].GetComponent<Image>().sprite;

        //checking column
        if (buttons[0, column].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
            buttons[1, column].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
            buttons[2, column].GetComponentInChildren<Image>().sprite == clickedButtonSprite)
        {
            return true;
        }

        //checking row
        else if (buttons[row, 0].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
            buttons[row, 1].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
            buttons[row, 2].GetComponentInChildren<Image>().sprite == clickedButtonSprite)
        {
            return true;
        }

        //checking 1st diagonal
        else if (buttons[0, 0].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
            buttons[1, 1].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
            buttons[2, 2].GetComponentInChildren<Image>().sprite == clickedButtonSprite)
        {
            return true;
        }

        //checking 2nd diagonal
        else if (buttons[0, 2].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
        buttons[1, 1].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
        buttons[2, 0].GetComponentInChildren<Image>().sprite == clickedButtonSprite)
        {
            return true;
        }

        //if none of these condtions are true return false no winner yet
        return false;
    }

    //this method checks if the game is a tie.
    private bool IsGameATie()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if((buttons[i, j].GetComponent<Image>().sprite != xSprite) && (buttons[i, j].GetComponent<Image>().sprite != oSprite))
                {
                    return false;
                }
            }
        }

        return true;
    }

}
