using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GameManagerScript : MonoBehaviour
{
    public GameObject[] TileFaces;    //Array to store TileFaces by index
    public int CurrentPlayer; // Current player (current round) - 1/2/3/4
    int readyGame = 0; //Indicator to show if game is ready to be played (1) or to end game (0)
    int turncount = 0; //The number of turns this game has currently
    //int counttime;
    public bool responded = false; //bool to check if player has responded - this is controlled by GameTileScript/OptionButtonScript
    int winnerfound = -99; //set to winner (1/2/3/4) if winner is found, else stays at -99 
    bool endofturn = true; //bool to indicate if end of turn - to run turn loop or not to run
    public int btnSelected = -99; //to indicate which button the player has selected, left to right 0 1 2 3 - this is controlled by OptionButtonScript
    public string tiletodiscard = ""; //the name of Tile GameObject to discard - this is controlled by GameTileScript
    public bool startGame = false; //indicator to start a new game (true) else, false
    bool newGame = true; // used in SetUpTiles() - if new game, create tileset and spawn tiles, else just reshuffle and rematch tile faces.
    List<GameObject> tileSetGameObject = new List<GameObject>();  //the unseeen tile set
    string freshdrawtile; //name of Tile GameObject just drawn from wall - used in DrawCardFromWall() and DrawCardFromBackWall(), also used to check for playerGang
    bool isInExposed = false; //indicator to check if playerGang tiles (the other 3 of a kind) are in the exposed tiles or players hand
    int currentWind = 1; //current wind of the game - DONG (1), NAN (2), XI (3), BEI (4)
    int currentDealer = 1; //current dealer of the round

    //TILE INDEXES
    //Bamboo - 00 to 08
    //Character - 09 to 17
    //Dots - 18 to 26
    //Dragon - 27 to 29
    //Wind - 30 to 33

    //LISTS VARIABLES
    public List<string> tileset; // used in setting up tiles

    //GAME OBJECTS VARIABLES
    public GameObject GameCanvas;
    public GameObject TilePrefab;
    public GameObject TileContainer;
    public GameObject Player1Container;
    public GameObject Player2Container;
    public GameObject Player3Container;
    public GameObject Player4Container;
    public GameObject Player1ExposedContainer;
    public GameObject Player2ExposedContainer;
    public GameObject Player3ExposedContainer;
    public GameObject Player4ExposedContainer;
    public GameObject DiscardContainer;
    public GameObject OverlayPanelPrefab;
    public GameObject OptionsButtonPrefab;
    public GameObject ChowOptionButtonPrefab;
    public GameObject PongOptionButtonPrefab;
    public GameObject GangOptionButtonPrefab;
    public GameObject MeldSetContainerPrefab;
    public GameObject GameOverPanelPrefab;
    public GameObject CurrentWindText;
    public GameObject DealerText;

    bool playerchow = false; //indicator if a player has just performed chow (this would be the next player of the player who just discarded tile)
    bool playerponggang = false; //indicator if a player has just performed pong/gang
    int pongplayer = -99; //record pongplayer if the player has just performed pong (will be next player)
    int gangplayer = -99; //record gangplayer if the player has just performed gang (will be next player)
    bool playergang = false; //indicator if a player has just performed gang

    public bool p1TileFace = false;
    bool p1_TileFace = false;
    public bool p2TileFace = false;
    bool p2_TileFace = false;
    public bool p3TileFace = false;
    bool p3_TileFace = false;
    public bool p4TileFace = false;
    bool p4_TileFace = false;

    // Start is called before the first frame update
    void Start()
    {
        //INITIALISATION
        startGame = true; //start new game indicator
    }

    // Update is called once per frame
    void Update()
    {
        if (startGame)
        {
            startGame = false; //prevent setuptiles in gameloop from running twice)
            StartCoroutine(GameLoop()); //start gameloop
        }

        if (p1TileFace != p1_TileFace) 
        {
            p1_TileFace = p1TileFace;
            for (int i = 0; i < Player1Container.transform.childCount; i++)
            {
                Player1Container.transform.GetChild(i).GetComponent<GameTileScript>().toggleTileFace();
            }
        }

        if (p2TileFace != p2_TileFace)
        {
            p2_TileFace = p2TileFace;
            for (int i = 0; i < Player2Container.transform.childCount; i++)
            {
                Player2Container.transform.GetChild(i).GetComponent<GameTileScript>().toggleTileFace();
            }
        }

        if (p3TileFace != p3_TileFace)
        {
            p3_TileFace = p3TileFace;
            for (int i = 0; i < Player3Container.transform.childCount; i++)
            {
                Player3Container.transform.GetChild(i).GetComponent<GameTileScript>().toggleTileFace();
            }
        }

        if (p4TileFace != p4_TileFace)
        {
            p4_TileFace = p4TileFace;
            for (int i = 0; i < Player4Container.transform.childCount; i++)
            {
                Player4Container.transform.GetChild(i).GetComponent<GameTileScript>().toggleTileFace();
            }
        }
    }

    //MAIN GAME LOOP HERE
    IEnumerator GameLoop() {

        //Setting up tiles
        SetupTiles();
        startGame = false;

        //this TURN loop runs while the unseen tile stack still have sufficient tiles to continue running (last tile shall be the 16th tile from the back)
        while (readyGame == 1)
        {
            //this check if the previous player has ended his turn and no winner is found yet
            if (endofturn == true && winnerfound == -99)
            {
                //set to false immediately as the next player has started his turn
                endofturn = false;
                Debug.Log("TURNCOUNT: " + turncount);
                //check that this is not the first round (as the first player would have already drawn an extra tile the first round - do not need to draw a new tile) 
                //playergang == true means that a player has just performed gang move and can now draw a tile from the back wall
                if (turncount != 1 && playergang == true)
                {
                    playergang = false;
                    playerponggang = false;
                    DrawCardFromBackWall();

                    //if player wins
                    if (CheckForHu(ComputeHand(CurrentPlayer)))
                    {
                        winnerfound = CurrentPlayer;
                        break;
                    }

                    //if not enough tiles in wall anymore
                    if (readyGame == 0)
                    {
                        break;
                    }

                    //check for gang with the new drawn tile from back of wall and ask if player want to
                    if (CheckForPlayerGang())
                    {
                        DisplayPlayerGangOptions();
                        responded = false;
                        Debug.Log("Enter WaitForResponse-PLAYERGANG");

                        //5 seconds to respond
                        for (int i = 0; i < 5; i++)
                        {
                            yield return new WaitForSeconds(1);
                            if (responded == true)
                            {
                                //break if responded
                                break;
                            }
                        }
                        //END OF WAIT FOR PONG GANG RESPONSE

                        //if responded, 
                        if (responded == true)
                        {
                            Debug.Log("WaitForResponse OPTION BUTTON - RESPONDED " + btnSelected);
                            responded = false;

                            //check if player selected an option other than "No Thanks"
                            if (btnSelected != 0)
                            {
                                //Perform gang move for player
                                ExecutePlayerGang();
                            }
                        }
                        //did not respond
                        else
                        {
                            Debug.Log("WaitForResponse No response but 5 seconds up");
                        }

                        //remove options interface and continue game
                        DestroyOptionButtons();
                        Debug.Log("WaitForResponse Completed");
                    }
                }
                //check if current round is not the first round, and player DID NOT just chow and player DID NOT just pong/gang
                else if (turncount != 1 && playerchow == false && playerponggang == false)
                {
                    DrawCardFromWall();

                    //if wall does not have sufficient tiles, end game
                    if (readyGame == 0)
                    {
                        break;
                    }

                    //Check if CURRENT player has won with the newly drawn tile
                    if (CheckForHu(ComputeHand(CurrentPlayer)))
                    {
                        winnerfound = CurrentPlayer;
                        break;
                    }

                    //check for CURRENT player is able to perform gang move with newly drawn tile and if player wants to
                    if (CheckForPlayerGang())
                    {
                        DisplayPlayerGangOptions();
                        responded = false;
                        Debug.Log("Enter WaitForResponse-PLAYERGANG");

                        //wait for player response for 5 seconds - responded will be accessed from OptionButtonScript if player responses
                        for (int i = 0; i < 5; i++)
                        {
                            yield return new WaitForSeconds(1);
                            if (responded == true)
                            {
                                break;
                            }
                        }
                        //END OF WAIT FOR PONG GANG RESPONSE
                        if (responded == true)
                        {
                            Debug.Log("WaitForResponse OPTION BUTTON - RESPONDED " + btnSelected);
                            responded = false;

                            //check if player selected an option other than "No Thanks"
                            if (btnSelected != 0)
                            {
                                //perform player gang move
                                ExecutePlayerGang();
                            }
                        }
                        else
                        {
                            Debug.Log("WaitForResponse No response but 5 seconds up");
                        }


                        //remove options display from screen
                        DestroyOptionButtons();
                        Debug.Log("WaitForResponse Completed");
                    }
                }
                //if player just performed chow/pong move, they cannot draw tile from wall this turn, hence reset indicator and move onto discard phase 
                else
                {
                    playerchow = false;
                    playerponggang = false;
                }
                //================END OF DRAW FROM WALL================



                //================WAIT FOR DISCARD================
                Debug.Log("ENTER WaitForDiscard");
                //Wait for player to discard a tile for up to 5 seconds
                for (int i = 0; i < 5; i++)
                {
                    yield return new WaitForSeconds(1);
                    //if user responses, exit this waiting loop
                    if (responded == true)
                    {
                        break;
                    }
                }
                if (responded == true)
                {
                    Debug.Log("RESPONDED");
                    //reset indicator
                    responded = false;
                    //discard the tile the player has chosen, tiletodiscard is written by GameTileScript
                    DiscardTile(tiletodiscard);
                }
                else
                {
                    Debug.Log("No response but 5 seconds up");
                    //if no response from player, discard most right tile in the player's hand
                    DiscardMostRightTile();
                }
                //================END OF WAIT FOR DISCARD================

                //================CHECK FOR WIN================
                //Loop through every player and check if any player can win (HU), priority goes to player index in the sequence: currentplayer > currentplayer+1 > currentplayer + 2 > currentplayer + 3
                for (int i = 1; i < 5; i++)
                {
                    if (i != CurrentPlayer)
                    {
                        //insert the discarded tile into player hand and check if player is able to win
                        if (CheckForHu(InsertDiscardTemp(i)))
                        {
                            winnerfound = i;
                            break;
                        }
                    }

                }
                //if a winner is found, exit this game loop
                if (winnerfound != -99)
                {
                    break;
                }
                //================END OF CHECK FOR WIN================

                //================CHECK FOR PONG================
                //this bool[] ponggangset represents if player is qualified pong (ponggang[0]) and if player is qualified to perform gang (ponggang[1]). The player is qualified to perform pong if he has 2 of the same tile (same as the discarded tile) in his hand. If the player has 3 of the same tile (same as the discarded tile) in his hand, he is qualified to perform gang.
                bool[] ponggangset = new bool[2];

                //CheckForPongGang return the bool results after running checks
                ponggangset = CheckForPongGang();

                //if the player is qualified to perform EITHER pong or gang move, display options to the qualifying player.
                if (ponggangset.Contains(true))
                {
                    DisplayPongGangOptions(ponggangset);

                    //WAIT FOR PONG GANG RESPONSE for 5 seconds
                    responded = false;
                    Debug.Log("Enter WaitForResponse-PONGGANG");
                    for (int i = 0; i < 5; i++)
                    {
                        yield return new WaitForSeconds(1);
                        //if player responds, exit waiting loop. responded is written by OptionButtonScript
                        if (responded == true)
                        {
                            break;
                        }
                    }
                    if (responded == true)
                    {
                        Debug.Log("WaitForResponse OPTION BUTTON - RESPONDED");
                        responded = false;
                        //if player chooses to perform either pong/gang move, execute the move and destroy the options display
                        if (btnSelected != 0)
                        {
                            ExecutePongGang();
                            DestroyOptionButtons();
                        }
                        //else, destroy options display and move on to check for any qualifying chow moves from the next player
                        //do take note that the pong/gang move has priority over the chow move, hence this flow.
                        else
                        {
                            DestroyOptionButtons();

                            //================CHECK FOR CHOW================
                            //there are a maximum of 3 possible ways the chow move can be performed on a qualifying discarded tile
                            // e.g., if 5 is the discarded tile, the chow move can be performed in the following sets: 3-4-5, 4-5-6, or 5-6-7
                            //hence, we prepare an array of size 3, with each elements carrying an array of size 3.
                            int[][] meldsets = new int[][] {
                                new int[3],
                                new int[3],
                                new int[3]
                            };

                            //CheckForChow will return the possible meldsets if any.
                            meldsets = CheckForChow();
                            //if the meldsets returned contains atleast 1 possible way of performing the chow move, we display the option(s) to the next player (qualifying player)
                            if (meldsets != null)
                            {
                                DisplayChowOptions(meldsets);

                                //WAIT FOR CHOW RESPONSE for up to 5 seconds
                                responded = false;
                                Debug.Log("Enter WaitForResponse-CHOW");
                                for (int i = 0; i < 5; i++)
                                {
                                    yield return new WaitForSeconds(1);
                                    //if player responses, exit waiting loop
                                    if (responded == true)
                                    {
                                        //responded is written by OptionButtonScript
                                        break;
                                    }
                                }
                                Debug.Log("EXITED WAIT FOR RESPONSE WAIT 5 SECS LOOP");

                                //if player responseded, 
                                if (responded == true)
                                {
                                    Debug.Log("WaitForResponse OPTION BUTTON - RESPONDED");
                                    responded = false;
                                    //if player did not select "No thanks" option, execute the chow move according to player's choice and destory options display
                                    if (btnSelected != 0)
                                    {
                                        ExecuteChow(meldsets);
                                    }
                                    DestroyOptionButtons();
                                }
                                //if no response from player, destroy options display, assign next player and move on to the next player (enter TURN loop again)
                                else
                                {
                                    Debug.Log("WaitForResponse No response but 5 seconds up");
                                    DestroyOptionButtons();
                                    NextPlayer();
                                }
                                //END OF WAIT FOR CHOW RESPONSE
                                Debug.Log("WaitForResponse Completed");
                                //================END CHECK FOR CHOW================
                            }
                            //if there are no possible ways to perform the chow move, assign the next player and move on to the next player (enter TURN loop again)
                            else {
                                NextPlayer();
                            }
                        }
                    }
                    //if the qualifying pong player did not respond, no pong move is executed. Hence, we check for any possible chow move for the next player
                    else
                    {
                        Debug.Log("WaitForResponse No response but 5 seconds up");
                        DestroyOptionButtons();
                        ////================CHECK FOR CHOW================
                        //this is the same code as the previous CHECK FOR CHOW SECTION ABOVE - same comments
                        int[][] meldsets = new int[][] {
                            new int[3],
                            new int[3],
                            new int[3]
                        };

                        meldsets = CheckForChow();
                        if (meldsets != null)
                        {
                            DisplayChowOptions(meldsets);

                            //WAIT FOR CHOW RESPONSE
                            responded = false;
                            Debug.Log("Enter WaitForResponse-CHOW");
                            for (int i = 0; i < 5; i++)
                            {
                                yield return new WaitForSeconds(1);
                                if (responded == true)
                                {
                                    break;
                                }
                            }
                            Debug.Log("EXITED WAIT FOR RESPONSE WAIT 5 SECS LOOP");
                            if (responded == true)
                            {
                                Debug.Log("WaitForResponse OPTION BUTTON - RESPONDED");
                                responded = false;
                                if (btnSelected != 0)
                                {
                                    ExecuteChow(meldsets);
                                }
                                DestroyOptionButtons();
                            }
                            else
                            {
                                Debug.Log("WaitForResponse No response but 5 seconds up");
                                DestroyOptionButtons();
                                NextPlayer();
                            }
                            //END OF WAIT FOR CHOW RESPONSE
                            Debug.Log("WaitForResponse Completed");
                        }
                        else
                        {
                            NextPlayer();
                        }
                        ////================END OF CHECK FOR CHOW================
                    }
                    //END OF WAIT FOR PONG GANG RESPONSE

                }
                //================END OF CHECK FOR PONG================

                //if no player qualifies for a pong/gang move, we check for any possible chow move for the next player immediately.
                else
                {
                    ////================CHECK FOR CHOW================
                    //this is the same code as the previous CHECK FOR CHOW SECTION ABOVE - same comments
                    int[][] meldsets = new int[][] {
                            new int[3],
                            new int[3],
                            new int[3]
                    };

                    meldsets = CheckForChow();
                    if (meldsets != null)
                    {
                        DisplayChowOptions(meldsets);

                        //WAIT FOR CHOW RESPONSE
                        responded = false;
                        Debug.Log("Enter WaitForResponse-CHOW");
                        for (int i = 0; i < 5; i++)
                        {
                            yield return new WaitForSeconds(1);
                            if (responded == true)
                            {
                                break;
                            }
                        }
                        Debug.Log("EXITED WAIT FOR RESPONSE WAIT 5 SECS LOOP");
                        if (responded == true)
                        {
                            Debug.Log("WaitForResponse OPTION BUTTON - RESPONDED");
                            responded = false;
                            if (btnSelected != 0)
                            {
                                ExecuteChow(meldsets);
                            }
                            else { 
                                NextPlayer();
                            }
                            DestroyOptionButtons();
                        }
                        else
                        {
                            Debug.Log("WaitForResponse No response but 5 seconds up");
                            DestroyOptionButtons();
                            NextPlayer();
                        }
                        //END OF WAIT FOR CHOW RESPONSE
                        Debug.Log("WaitForResponse Completed");
                    }
                    else {
                        NextPlayer();
                    }
                    ////================END OF CHECK FOR CHOW================
                }

                //regardless of any pong/gang/chow move or no extra moves from other players, we indicate the current turn has ended and we are ready for the next turn. repeat TURN LOOP for next player.
                endofturn = true;
            }
        }
        //END OF TURN LOOP 

        //if a winner is found, OR is readyGame = 0 (means that there are no longer sufficient tiles in the wall, we end the game by displaying game over menu), we end the game by displaying the game over menu
        GameOverMenu();

    }   

    //this function instantiates the gameoverdisplay with a play again button and the result of the game that just ended - winner shown or if drawn, "NO WINNER"
    void GameOverMenu()
    {
        GameObject gameOverPanel = Instantiate(GameOverPanelPrefab, GameCanvas.transform);
        if (winnerfound == -99)
        {
            gameOverPanel.gameObject.transform.Find("WinnerText").GetComponent<Text>().text = "NO WINNER";
        }
        else {
            gameOverPanel.gameObject.transform.Find("WinnerText").GetComponent<Text>().text = "WINNER: " + winnerfound;
        }

    }

    //this functions changes the dealer if the player decides to continue playing (play again)
    void ChangeDealer() {
        //set the current dealer label to inactive
        DealerText.transform.Find("DEALER" + currentDealer).gameObject.SetActive(false);

        //if current dealer is 4, next dealer shall be 1. this also indicates that the dealer label has gone through from player 1 to 4, and the game is ready to move on to the next wind of the game. (DONG - NAN - XI - BEI)
        if (currentDealer == 4)
        {
            currentDealer = 1;
            UpdateWind();
        }
        //if current dealer is 1, 2 or 3, the next dealer shall be 2, 3 or 4 respectively.
        else {
            currentDealer++;
        }

        //find the label that belongs to the next dealer and set it to active
        DealerText.transform.Find("DEALER" + currentDealer).gameObject.SetActive(true);
    }

    //this functions updates the wind of the game when called
    void UpdateWind() {
        string windText;
        //if the currentWind of the game is 4, and this function has been called, it is now ready to restart the game from the first wind - DONG. therefore we set it to 1.
        if (currentWind == 4)
        {
            currentWind = 1;
        }
        //else we move on to the next wind in the order 1 - 2 - 3 - 4
        else
        {
            currentWind++;
        }

        //matching the numbers to the correct string representation DONG - NAN - XI - BEI
        switch (currentWind)
        {
            case 1:
                windText = "DONG";
                break;
            case 2:
                windText = "NAN";
                break;
            case 3:
                windText = "XI";
                break;
            case 4:
                windText = "BEI";
                break;
            default:
                windText = "";
                break;

        }
        //update wind label
        CurrentWindText.GetComponent<Text>().text = "CURRENT WIND: " + windText;
    }

    //when a player chooses to play again, we have to the clear the canvas and reset the game back to original
    public void ClearCanvas() {

        //if the game that just ended, did not end with a draw or the current dealer was not the winner, we move on to the next dealer
        if ((winnerfound != -99) && (winnerfound != currentDealer))
        {
            ChangeDealer();
        }

        //resetting all the indicators back to 0 and all other counters
        playerchow = false;
        playerponggang = false;
        pongplayer = -99;
        gangplayer = -99;
        playergang = false;
        CurrentPlayer = -1;
        readyGame = 0;
        turncount = 0;
        responded = false;
        winnerfound = -99;
        endofturn = true;
        btnSelected = -99;
        tiletodiscard = "";
        startGame = false;
        if (p1TileFace != false)
        {
            p1_TileFace = false;
            p1TileFace = false;
            for (int i = 0; i < Player1Container.transform.childCount; i++)
            {
                Player1Container.transform.GetChild(i).GetComponent<GameTileScript>().toggleTileFace();
            }
        }

        if (p2TileFace != false)
        {
            p2_TileFace = false;
            p2TileFace = false;
            for (int i = 0; i < Player2Container.transform.childCount; i++)
            {
                Player2Container.transform.GetChild(i).GetComponent<GameTileScript>().toggleTileFace();
            }
        }

        if (p3TileFace != false)
        {
            p3_TileFace = false;
            p3TileFace = false;
            for (int i = 0; i < Player3Container.transform.childCount; i++)
            {
                Player3Container.transform.GetChild(i).GetComponent<GameTileScript>().toggleTileFace();
            }
        }

        if (p4TileFace != false)
        {
            p4_TileFace = false;
            p4TileFace = false;
            for (int i = 0; i < Player4Container.transform.childCount; i++)
            {
                Player4Container.transform.GetChild(i).GetComponent<GameTileScript>().toggleTileFace();
            }
        }

        //shift all the tiles back to the unseen tile container, be it in the discarded tile container, players container or players exposed containers. and set them back to facing down
        foreach (GameObject tile in tileSetGameObject) {
            if (tile.transform.parent != TileContainer.transform) { 
                tile.transform.SetParent(TileContainer.transform, false);
                tile.GetComponent<GameTileScript>().toggleTileFace();
                tile.transform.localPosition = new Vector3(0, 0, 0);
            }
        }

        //remove game over menu display
        GameObject gameOverPanel = GameObject.Find("GameOverPanel(Clone)");
        if (gameOverPanel)
        {
            Destroy(gameOverPanel);
        }

        //destroy all meldcontainers
        foreach (GameObject meldcontainer in GameObject.FindGameObjectsWithTag("MeldContainer")) {
            Destroy(meldcontainer);
        }
    }

    //this function sets up the tiles for a game
    void SetupTiles()
    {
        //if this is the first game, we generate the tileset, create tileset, set all dealer labels to inactive except for player 1, and display wind text.
        if (newGame) {         
            tileset = GenerateTileSet();
            Shuffle(tileset);
            CreateTileSet();
            newGame = false;
            DealerText.transform.Find("DEALER2").gameObject.SetActive(false);
            DealerText.transform.Find("DEALER3").gameObject.SetActive(false);
            DealerText.transform.Find("DEALER4").gameObject.SetActive(false);
            DealerText.transform.Find("DEALER1").gameObject.SetActive(true);
            CurrentWindText.GetComponent<Text>().text = "CURRENT WIND: DONG";   
        }
        //if player has just selected to play again, we shuffle the tileset and rematch the tileset to its tile faces.
        else
        {
            Shuffle(tileset);
            RematchTiles();
        }
        Debug.Log("TILESET: " + string.Join(", ", tileset));

        //deal 13 tiles to all player except the dealer. dealer gets 14.
        DealTiles();
        Debug.Log("TILESET COUNT: " + tileset.Count);
    }

    //this function generates the tileset by index in a list
    static List<string> GenerateTileSet()
    {

        List<string> newTileSet = new List<string>();

        for (int i = 0; i < 34; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (i < 10)
                {
                    newTileSet.Add("0" + i.ToString());
                }
                else
                {
                    newTileSet.Add(i.ToString());
                }
            }
        }

        return newTileSet;
    }

    //shuffle the deck created to form a deck of card w random sequeunce. 
    void Shuffle<T>(List<T> list)
    {

        System.Random random = new System.Random();

        int n = list.Count;

        while (n > 1)
        {

            int k = random.Next(n);
            n--;

            //swap positions of index k and n, k at random
            T temp = list[k];
            list[k] = list[n];
            list[n] = temp;

        }
    }

    //this function rematches the tiles in TileContainer according to the tileset sequence that has been shuffled.
    void RematchTiles() {
        //tileindex is the position the tile shall be in, in the tilecontainer.
        int tileindex = 0;
        foreach (string tile in tileset)
        {
            for (int i = 0; i > -1; i++)
            {
                if (tile == TileContainer.transform.GetChild(i).gameObject.name) {
                    TileContainer.transform.GetChild(i).gameObject.transform.SetSiblingIndex(tileindex);
                    tileindex++;
                    break;
                }
            }
        }
    }


    //Instantiate tiles according to the tileset order and match the correct tile faces to the back facving tile. this create the gametile object.
    void CreateTileSet()
    {
        foreach (string tile in tileset)
        {
            for (int i = 0; i > -1; i++)
            {
                if (tile == TileFaces[i].name)
                {
                    GameObject newTile = Instantiate(TileFaces[i], TileContainer.transform);
                    tileSetGameObject.Add(newTile);
                    newTile.name = tile;
                    break;
                }
            }
        }
    }

    //deal tiles to players starting with the dealer and ending at the dealer. so dealer gets an extra tile. every player should receive 13 tiles while the dealer receives 14.
    void DealTiles()
    {
        int playernow = currentDealer;

        for (int i = 0; i < 53; i++)
        {
            Debug.Log("DEAL" + i);
            switch (playernow)
            {
                case 1:
                    TileContainer.transform.GetChild(0).gameObject.transform.SetParent(Player1Container.transform, false);
                    Debug.Log(Player1Container.transform.GetChild(Player1Container.transform.childCount - 1).gameObject.name + " to player 1 container");
                    Player1Container.transform.GetChild(Player1Container.transform.childCount - 1).GetComponent<GameTileScript>().toggleTileFace();
                    Debug.Log(Player1Container.transform.GetChild(Player1Container.transform.childCount - 1).parent.gameObject.name);
                    break;
                case 2:
                    TileContainer.transform.GetChild(0).gameObject.transform.SetParent(Player2Container.transform, false);
                    Debug.Log(Player2Container.transform.GetChild(Player2Container.transform.childCount - 1).gameObject.name + " to player 2 container");
                    Player2Container.transform.GetChild(Player2Container.transform.childCount - 1).GetComponent<GameTileScript>().toggleTileFace();
                    break;
                case 3:
                    TileContainer.transform.GetChild(0).gameObject.transform.SetParent(Player3Container.transform, false);
                    Debug.Log(Player3Container.transform.GetChild(Player3Container.transform.childCount - 1).gameObject.name + " to player 3 container");
                    Player3Container.transform.GetChild(Player3Container.transform.childCount - 1).GetComponent<GameTileScript>().toggleTileFace();
                    break;
                case 4:
                    TileContainer.transform.GetChild(0).gameObject.transform.SetParent(Player4Container.transform, false);
                    Debug.Log(Player4Container.transform.GetChild(Player4Container.transform.childCount - 1).gameObject.name + " to player 4 container");
                    Player4Container.transform.GetChild(Player4Container.transform.childCount - 1).GetComponent<GameTileScript>().toggleTileFace();
                    break;
                default:
                    Debug.Log("Invalid player number");
                    break;
            }
            switch (playernow)
            {
                case 1:
                    playernow = 2;
                    break;
                case 2:
                    playernow = 3;
                    break;
                case 3:
                    playernow = 4;
                    break;
                case 4:
                    playernow = 1;
                    break;
                default:
                    Debug.Log("Invalid player number");
                    break;
            }
        }

        //set the dealer as the first player of this game.
        CurrentPlayer = currentDealer;

        //rearrange all tiles according to their index in all players container.
        Debug.Log("REARRANGE FOR P1");
        RearrangeCards(Player1Container);
        
        Debug.Log("REARRANGE FOR P2");
        RearrangeCards(Player2Container);

        Debug.Log("REARRANGE FOR P3");
        RearrangeCards(Player3Container);

        Debug.Log("REARRANGE FOR P4");
        RearrangeCards(Player4Container);

        //the game is ready to be played and turn count starts at 1
        readyGame = 1;
        turncount = 1;
    }

    //get parent of the tile
    public string GetParent(Component card)
    {
        return card.gameObject.transform.parent.name;
    }

    //discard the tile that the player has selected to discard
    public void DiscardTile(string tiletodiscard)
    {
        //find player's tile container (not the exposed ones)
        GameObject playerContainer;
        switch (CurrentPlayer)
        {
            case 1:
                playerContainer = Player1Container;
                if (p1_TileFace == true) {
                    playerContainer.gameObject.transform.Find(tiletodiscard).gameObject.GetComponent<GameTileScript>().toggleTileFace();
                }
                break;
            case 2:
                playerContainer = Player2Container;
                if (p2_TileFace == true)
                {
                    playerContainer.gameObject.transform.Find(tiletodiscard).gameObject.GetComponent<GameTileScript>().toggleTileFace();
                }
                break;
            case 3:
                playerContainer = Player3Container;
                if (p3_TileFace == true)
                {
                    playerContainer.gameObject.transform.Find(tiletodiscard).gameObject.GetComponent<GameTileScript>().toggleTileFace();
                }
                break;
            case 4:
                playerContainer = Player4Container;
                if (p4_TileFace == true)
                {
                    playerContainer.gameObject.transform.Find(tiletodiscard).gameObject.GetComponent<GameTileScript>().toggleTileFace();
                }
                break;
            default:
                Debug.Log("Invalid player number");
                playerContainer = null;
                break;
        }
        //find the tile in the player's container and set its parent to the discard container. this performs the discarding move
        playerContainer.gameObject.transform.Find(tiletodiscard).gameObject.transform.SetParent(DiscardContainer.transform);
    }

    //retrieve the newly discarded tile from the discard container, returns the index of the tile.
    int GetDiscardTile()
    {
        string discardTileName = DiscardContainer.transform.GetChild(DiscardContainer.transform.childCount - 1).gameObject.name;
        int discardTile;
        Debug.Log("DISCARD TILE NAME FROM GETDISCARDTILE:  " + discardTileName);
        int.TryParse(discardTileName, out discardTile);
        Debug.Log("DISCARD TILE INT FROM GETDISCARDTILE:  " + discardTile);
        return discardTile;
    }

    //temporarily inserts the newly discard tile into the given player's hand.
    int[] InsertDiscardTemp(int player) {
        int[] tilesetcount = new int[34];
        string discardTileName = DiscardContainer.transform.GetChild(DiscardContainer.transform.childCount - 1).gameObject.name;
        int discardTile;
        int.TryParse(discardTileName, out discardTile);
        tilesetcount = ComputeHand(player);
        tilesetcount[discardTile]++;
        Debug.Log("AFTER INSERT DISCARD TEMP: " + string.Join(", ", tilesetcount));
        return tilesetcount;
    }

    //compute the given player's hand into a format:
    //where 2 tiles of index 5 would make set tilesetcount[5] = 2;
    int[] ComputeHand(int player)
    {
        GameObject GivenPlayerContainer;
        int[] tilesetcount = new int[34];

        //get the given player's container (not including exposed tiles)
        switch (player)
        {
            case 1:
                GivenPlayerContainer = Player1Container;
                break;
            case 2:
                GivenPlayerContainer = Player2Container;
                break;
            case 3:
                GivenPlayerContainer = Player3Container;
                break;
            case 4:
                GivenPlayerContainer = Player4Container;
                break;
            default:
                GivenPlayerContainer = null;
                Debug.Log("Invalid player number");
                break;
        }
        //compute hand according to the format mentioned
        for (int i = 0; i < GivenPlayerContainer.transform.childCount; i++)
        {
            string tileindexstring = GivenPlayerContainer.transform.GetChild(i).gameObject.name;
            int tileindex;
            int.TryParse(tileindexstring, out tileindex);
            tilesetcount[tileindex]++;
        }
        Debug.Log("COMPUTE HAND: " + string.Join(", ", tilesetcount));

        //returns this computed hand format
        return tilesetcount;
    }

    //set the correct next player in the order 1 - 2 - 3 - 4
    public void NextPlayer()
    {
        switch (CurrentPlayer)
        {
            case 1:
                CurrentPlayer = 2;
                break;
            case 2:
                CurrentPlayer = 3;
                break;
            case 3:
                CurrentPlayer = 4;
                break;
            case 4:
                CurrentPlayer = 1;
                break;
            default:
                Debug.Log("Invalid player number");
                break;
        }
        Debug.Log("PLAYER: " + CurrentPlayer);
        turncount++;
    }

    //returns the next possible player - used for checking chow move
    int GetNextPlayer()
    {
        int temp;
        switch (CurrentPlayer)
        {
            case 1:
                temp = 2;
                Debug.Log("TempNextPlayer: " + temp);
                return temp;
            case 2:
                temp = 3;
                Debug.Log("TempNextPlayer: " + temp);
                return temp;
            case 3:
                temp = 4;
                Debug.Log("TempNextPlayer: " + temp);
                return temp;
            case 4:
                temp = 1;
                Debug.Log("TempNextPlayer: " + temp);
                return temp;
            default:
                Debug.Log("Invalid player number");
                return temp = -99;
        }
    }

    //rearrange tiles in the container according to their index (increasing order, left to right)
    public void RearrangeCards(GameObject CardContainer)
    {
        List<GameObject> tiles = new List<GameObject>();

        for (int i = 0; i < CardContainer.transform.childCount; i++)
        {
            tiles.Add(CardContainer.transform.GetChild(i).gameObject);
        }

        tiles = tiles.OrderBy(tile => tile.name).ToList();
        int index = 0;
        foreach (GameObject tile in tiles)
        {
            tile.transform.SetSiblingIndex(index);
            index++;
        }
    }


    //discards most right tile in players hand (not including tiles in exposed container)
    void DiscardMostRightTile()
    {
        switch (CurrentPlayer)
        {
            case 1:
                if (p1_TileFace == true)
                {
                    Player1Container.transform.GetChild(Player1Container.transform.childCount - 1).gameObject.GetComponent<GameTileScript>().toggleTileFace();
                }
                Player1Container.transform.GetChild(Player1Container.transform.childCount - 1).gameObject.transform.SetParent(DiscardContainer.transform);
                break;
            case 2:
                if (p2_TileFace == true)
                {
                    Player2Container.transform.GetChild(Player2Container.transform.childCount - 1).gameObject.GetComponent<GameTileScript>().toggleTileFace();
                }
                Player2Container.transform.GetChild(Player2Container.transform.childCount - 1).gameObject.transform.SetParent(DiscardContainer.transform);
                break;
            case 3:
                if (p3_TileFace == true)
                {
                    Player3Container.transform.GetChild(Player3Container.transform.childCount - 1).gameObject.GetComponent<GameTileScript>().toggleTileFace();
                }
                Player3Container.transform.GetChild(Player3Container.transform.childCount - 1).gameObject.transform.SetParent(DiscardContainer.transform);
                break;
            case 4:
                if (p4_TileFace == true)
                {
                    Player4Container.transform.GetChild(Player4Container.transform.childCount - 1).gameObject.GetComponent<GameTileScript>().toggleTileFace();
                }
                Player4Container.transform.GetChild(Player4Container.transform.childCount - 1).gameObject.transform.SetParent(DiscardContainer.transform);
                break;
            default:
                Debug.Log("Invalid player number");
                break;
        }
    }

    //draws a tile from the wall and set tile's parent to be current player's tile container
    void DrawCardFromWall()
    {
        //if the unseen tile container has less than 16 cards, end game.
        if (TileContainer.transform.childCount < 16)
        {
            readyGame = 0;
        }
        else
        {
            //draw a tile and set it to be the child of the current player's tile container
            Debug.Log("Draw From Wall");
            switch (CurrentPlayer)
            {
                case 1:
                    TileContainer.transform.GetChild(0).gameObject.transform.SetParent(Player1Container.transform, false);
                    freshdrawtile = Player1Container.transform.GetChild(Player1Container.transform.childCount - 1).gameObject.name;
                    if (p1_TileFace == false) { 
                        Player1Container.transform.GetChild(Player1Container.transform.childCount - 1).GetComponent<GameTileScript>().toggleTileFace();
                    }
                    RearrangeCards(Player1Container);
                    break;
                case 2:
                    TileContainer.transform.GetChild(0).gameObject.transform.SetParent(Player2Container.transform, false);
                    freshdrawtile = Player2Container.transform.GetChild(Player2Container.transform.childCount - 1).gameObject.name;
                    if (p2_TileFace == false)
                    {
                        Player2Container.transform.GetChild(Player2Container.transform.childCount - 1).GetComponent<GameTileScript>().toggleTileFace();
                    }
                    RearrangeCards(Player2Container);
                    break;
                case 3:
                    TileContainer.transform.GetChild(0).gameObject.transform.SetParent(Player3Container.transform, false);
                    freshdrawtile = Player3Container.transform.GetChild(Player3Container.transform.childCount - 1).gameObject.name;
                    if (p3_TileFace == false)
                    {
                        Player3Container.transform.GetChild(Player3Container.transform.childCount - 1).GetComponent<GameTileScript>().toggleTileFace();
                    }
                    RearrangeCards(Player3Container);
                    break;
                case 4:
                    TileContainer.transform.GetChild(0).gameObject.transform.SetParent(Player4Container.transform, false);
                    freshdrawtile = Player4Container.transform.GetChild(Player4Container.transform.childCount - 1).gameObject.name;
                    if (p4_TileFace == false)
                    {
                        Player4Container.transform.GetChild(Player4Container.transform.childCount - 1).GetComponent<GameTileScript>().toggleTileFace();
                    }
                    RearrangeCards(Player4Container);
                    break;
                default:
                    Debug.Log("Invalid player number");
                    break;
            }
        }

    }

    //draw tile from the back of the wall and set it to be the child of the current player's container (not the exposed container)
    void DrawCardFromBackWall()
    {
        //if there are less than 16 tiles in the unseen stack, end game.
        if (TileContainer.transform.childCount < 16)
        {
            readyGame = 0;
        }
        //else draw tile and set correct parent.
        else {        
            Debug.Log("Draw From Wall");
            switch (CurrentPlayer)
            {
                case 1:
                    TileContainer.transform.GetChild(TileContainer.transform.childCount - 1).gameObject.transform.SetParent(Player1Container.transform, false);
                    freshdrawtile = Player1Container.transform.GetChild(Player1Container.transform.childCount - 1).gameObject.name;
                    if (p1_TileFace == false) { 
                       Player1Container.transform.GetChild(Player1Container.transform.childCount - 1).GetComponent<GameTileScript>().toggleTileFace();
                    }
                    RearrangeCards(Player1Container);
                    break;
                case 2:
                    TileContainer.transform.GetChild(TileContainer.transform.childCount - 1).gameObject.transform.SetParent(Player2Container.transform, false);
                    freshdrawtile = Player2Container.transform.GetChild(Player2Container.transform.childCount - 1).gameObject.name;
                    if (p2_TileFace == false)
                    {
                        Player2Container.transform.GetChild(Player2Container.transform.childCount - 1).GetComponent<GameTileScript>().toggleTileFace();
                    }
                    RearrangeCards(Player2Container);
                    break;
                case 3:
                    TileContainer.transform.GetChild(TileContainer.transform.childCount - 1).gameObject.transform.SetParent(Player3Container.transform, false);
                    freshdrawtile = Player3Container.transform.GetChild(Player3Container.transform.childCount - 1).gameObject.name;
                    if (p3_TileFace == false)
                    {
                        Player3Container.transform.GetChild(Player3Container.transform.childCount - 1).GetComponent<GameTileScript>().toggleTileFace();
                    }
                    RearrangeCards(Player3Container);
                    break;
                case 4:
                    TileContainer.transform.GetChild(TileContainer.transform.childCount - 1).gameObject.transform.SetParent(Player4Container.transform, false);
                    freshdrawtile = Player4Container.transform.GetChild(Player4Container.transform.childCount - 1).gameObject.name;
                    if (p4_TileFace == false)
                    {
                        Player4Container.transform.GetChild(Player4Container.transform.childCount - 1).GetComponent<GameTileScript>().toggleTileFace();
                    }
                    RearrangeCards(Player4Container);
                    break;
                default:
                    Debug.Log("Invalid player number");
                    break;
            } 
        }


    }

    //displays chow option to the player that qualifies for the chow move. this functions receives the possible meld sets in the format earlier mentioned in game loop. (CHECK FOR CHOW)
    void DisplayChowOptions(int[][] meldsets)
    {
        //instantiates the display and the option button(s). OptionButton0 shall always be the no thanks option.
        GameObject overlayPrefab = Instantiate(OverlayPanelPrefab, GameCanvas.transform);
        overlayPrefab.transform.Find("PlayerLabel").GetComponent<Text>().text = "PLAYER " + GetNextPlayer().ToString();
        GameObject nothanksButton = Instantiate(OptionsButtonPrefab, overlayPrefab.transform);
        nothanksButton.name = "OptionButton" + "0";
        nothanksButton.GetComponentInChildren<Text>().text = "No thanks";

        //debugging purposes
        for (int i = 0; i < 3; i++)
        {
            if (meldsets[i] == null) { break; }

            Debug.Log(meldsets[i][0] + "-" + meldsets[i][1] + "-" + meldsets[i][2]);
        }

        //spawn option buttons in the meldsets[]
        //for (int i = 0; i < 3; i++)
        //{
        //    //if there are no longer any possible meldset[], break out of loop and stop instantiating buttons
        //    if (meldsets[i] == null) { break; }
        //    else
        //    {
        //        GameObject optionButton = Instantiate(OptionsButtonPrefab, overlayPrefab.transform);
        //        int temp = i + 1;
        //        optionButton.name = "OptionButton" + temp.ToString();
        //        optionButton.GetComponentInChildren<Text>().text = meldsets[i][0] + "-" + meldsets[i][1] + "-" + meldsets[i][2];
        //    }
        //}

        for (int i = 0; i < 3; i++)
        {
            //if there are no longer any possible meldset[], break out of loop and stop instantiating buttons
            if (meldsets[i] == null) { break; }
            else
            {
                GameObject optionButton = Instantiate(ChowOptionButtonPrefab, overlayPrefab.transform);
                int temp = i + 1;
                optionButton.name = "OptionButton" + temp.ToString();
                optionButton.gameObject.transform.Find("TileImage0").GetComponent<RawImage>().texture = Resources.Load<Texture2D>("Images/" + tileIndexToString(meldsets[i][0]));
                optionButton.gameObject.transform.Find("TileImage1").GetComponent<RawImage>().texture = Resources.Load<Texture2D>("Images/" + tileIndexToString(meldsets[i][1]));
                optionButton.gameObject.transform.Find("TileImage2").GetComponent<RawImage>().texture = Resources.Load<Texture2D>("Images/" + tileIndexToString(meldsets[i][2]));
            }
        }
    }

    string tileIndexToString(int tileindexint) {
        if (tileindexint < 10)
        {
            return "0" + tileindexint.ToString();
        }
        else {
            return tileindexint.ToString();
        }
    }

    //display the pong/gang move options to qualifying player when called
    void DisplayPongGangOptions(bool[] ponggangset) {
        GameObject overlayPrefab = Instantiate(OverlayPanelPrefab, GameCanvas.transform);
        overlayPrefab.transform.Find("PlayerLabel").GetComponent<Text>().text = "PLAYER " + pongplayer.ToString();

        //instantiate no thanks button
        GameObject nothanksButton = Instantiate(OptionsButtonPrefab, overlayPrefab.transform);
        nothanksButton.name = "OptionButton" + "0";
        nothanksButton.GetComponentInChildren<Text>().text = "No thanks";

        //instantiate the PONG option button if ponggangset[0] == true, which means player qualifies to PONG
        //if (ponggangset[0] == true) {
        //    GameObject optionButton = Instantiate(OptionsButtonPrefab, overlayPrefab.transform);
        //    optionButton.name = "OptionButton" + "1";
        //    optionButton.GetComponentInChildren<Text>().text = "PONG";
        //}

        if (ponggangset[0] == true)
        {
            GameObject optionButton = Instantiate(PongOptionButtonPrefab, overlayPrefab.transform);
            optionButton.name = "OptionButton" + "1";
            string pongtilename = DiscardContainer.transform.GetChild(DiscardContainer.transform.childCount - 1).gameObject.name;
            optionButton.GetComponentInChildren<RawImage>().texture = Resources.Load<Texture2D>("Images/" + pongtilename);
        }

        //instantiate the GANG option button if ponggangset[1] == true, which means player qualifies to PONG
        if (ponggangset[1] == true)
        {
            GameObject optionButton = Instantiate(GangOptionButtonPrefab, overlayPrefab.transform);
            optionButton.name = "OptionButton" + "2";
            string pongtilename = DiscardContainer.transform.GetChild(DiscardContainer.transform.childCount - 1).gameObject.name;
            optionButton.GetComponentInChildren<RawImage>().texture = Resources.Load<Texture2D>("Images/" + pongtilename);
        }
    }

    //display gang option to the current player (if he qualifies) if player draws a tile that qualifies player to perform gang move
    void DisplayPlayerGangOptions() {
        GameObject overlayPrefab = Instantiate(OverlayPanelPrefab, GameCanvas.transform);
        overlayPrefab.transform.Find("PlayerLabel").GetComponent<Text>().text = "PLAYER " + CurrentPlayer.ToString();

        //instantiate no thanks button
        GameObject nothanksButton = Instantiate(OptionsButtonPrefab, overlayPrefab.transform);
        nothanksButton.name = "OptionButton" + "0";
        nothanksButton.GetComponentInChildren<Text>().text = "No thanks";

        //instantiate gang option
        GameObject optionButton = Instantiate(GangOptionButtonPrefab, overlayPrefab.transform); 
        optionButton.name = "OptionButton" + "1";
        optionButton.GetComponentInChildren<RawImage>().texture = Resources.Load<Texture2D>("Images/" + freshdrawtile);
    }

    //destroys option display and the buttons
    void DestroyOptionButtons()
    {
        //for (int i = 0; i < 3; i++)
        for (int i = 0; i < 4; i++)
        {
            GameObject optionbutton = GameObject.Find("OptionButton" + i);
            //if the button exist then destroy it
            if (optionbutton)
            {
                Destroy(optionbutton);
                Debug.Log(optionbutton.name + "has been destroyed.");
            }
        }

        GameObject overlaypanel = GameObject.Find("OverlayPanel(Clone)");
        //if the button exist then destroy it
        if (overlaypanel)
        {
            Destroy(overlaypanel);
            Debug.Log(overlaypanel + "has been destroyed.");

        }
    }

    //checks for chow and returns the meldsets[][] format mentioned previously in game loop
    int[][] CheckForChow()
    {
        //retrieve the freshly discarded tile index
        int discardTile = GetDiscardTile();
        int[] playerHand = new int[34];
        //get playershand in correct format as mentioned
        playerHand = ComputeHand(GetNextPlayer());
        int meldsetindex = 0;

        int[][] possiblemeldsets = new int[3][];
        Debug.Log("CHECK FOR CHOW ENTERED");

        //check for cards that are only in the bamboo suit, dot suit and character suit
        if (discardTile < 27)
        {
            //add discard tile to hand temporarily
            playerHand[discardTile]++;

            int countShift;
            bool alrChecked;
            int suit = (int)Mathf.Floor(discardTile / 9);

            Debug.Log("Discard Tile: " + discardTile);

            Debug.Log("Suit of Discard: " + (suit));

            //CHECKING POSSIBLE MELDS
            if (discardTile % 9 == 0) //This is only for shift pattern 1,2,3. Ignore i value
            {
                //for (int i = 0 + 9 * suit; i < 1 + 9 * suit; i++)
                //{
                    int i = 0 + 9 * suit;
                    countShift = 0;
                    alrChecked = false;
                    for (int j = i; j < i + 3; j++)
                    {
                        if (playerHand[j] > 0)
                        {
                            countShift++;
                        }
                        if (countShift == 3 && alrChecked == false)
                        {
                            //activate chow button
                            alrChecked = true;
                            Debug.Log("CAN CHOW: " + i + " - " + (i + 1) + " - " + (i + 2));
                            possiblemeldsets[meldsetindex] = new int[] { i, i + 1, i + 2 };
                            meldsetindex++;
                        }
                    }
                //}
            }
            else if (discardTile % 9 == 1)
            {
                for (int i = 0 + 9 * suit; i < 2 + 9 * suit; i++)
                {
                    countShift = 0;
                    alrChecked = false;
                    for (int j = i; j < i + 3; j++)
                    {
                        if (playerHand[j] > 0)
                        {
                            countShift++;
                        }
                        if (countShift == 3 && alrChecked == false)
                        {
                            //activate chow button
                            alrChecked = true;
                            Debug.Log("CAN CHOW: " + i + " - " + (i + 1) + " - " + (i + 2));
                            possiblemeldsets[meldsetindex] = new int[] { i, i + 1, i + 2 };
                            meldsetindex++;
                        }
                    }
                }
            }
            else if (discardTile % 9 == 7)
            {
                for (int i = 5 + 9 * suit; i < 7 + 9 * suit; i++)
                {
                    countShift = 0;
                    alrChecked = false;
                    for (int j = i; j < i + 3; j++)
                    {
                        if (playerHand[j] > 0)
                        {
                            countShift++;
                        }
                        if (countShift == 3 && alrChecked == false)
                        {
                            //activate chow button
                            alrChecked = true;
                            Debug.Log("CAN CHOW: " + i + " - " + (i + 1) + " - " + (i + 2));
                            possiblemeldsets[meldsetindex] = new int[] { i, i + 1, i + 2 };
                            meldsetindex++;
                        }
                    }
                }
            }
            else if (discardTile % 9 == 8)
            {
                int i = 6 + 9 * suit;
                countShift = 0;
                alrChecked = false;
                for (int j = i; j < i + 3; j++)
                {
                    if (playerHand[j] > 0)
                    {
                        countShift++;
                    }
                    if (countShift == 3 && alrChecked == false)
                    {
                        //activate chow button
                        alrChecked = true;
                        Debug.Log("CAN CHOW: " + i + " - " + (i + 1) + " - " + (i + 2));
                        possiblemeldsets[meldsetindex] = new int[] { i, i + 1, i + 2 };
                        meldsetindex++;
                    }
                }
            }
            else if (discardTile % 9 > 1 && discardTile % 9 < 7)
            {
                for (int i = discardTile - 2; i < discardTile + 1; i++) //only need to check for wan tong suo
                {
                    countShift = 0;
                    alrChecked = false;
                    for (int j = i; j < i + 3; j++)
                    {
                        if (playerHand[j] > 0)
                        {
                            countShift++;
                        }
                        if (countShift == 3 && alrChecked == false)
                        {
                            //activate chow button
                            alrChecked = true;
                            Debug.Log("CAN CHOW: " + i + " - " + (i + 1) + " - " + (i + 2));
                            possiblemeldsets[meldsetindex] = new int[] { i, i + 1, i + 2 };
                            meldsetindex++;
                        }
                    }
                }
            }
            //END OF CHECKING POSSIBLE MELDS

            //if there are no possible meld sets
            if (possiblemeldsets[0] == null)
            {

                Debug.Log("NO MELD SETS");
                return null;

            }

            //debugging
            for (int i = 0; i < 3; i++)
            {
                if (possiblemeldsets[i] != null)
                {
                    Debug.Log(" FINAL Meld Set " + i + " :" + possiblemeldsets[i][0] + "-" + possiblemeldsets[i][1] + "-" + possiblemeldsets[i][2]);
                }
            }
            //end of debugging
            

            return possiblemeldsets;

        }

        //if tile does not belong to the bamboo, dot or character suits, return null as chow is not possible.
        return null;

    }


    //check if player is able to perform gang move with tiles in players hand (exposed or not) (not any discarded tile)
    bool CheckForPlayerGang()
    {
        Debug.Log("CHECKING FOR PLAYER: " + CurrentPlayer);
        int[] playerHand = new int[34];
        playerHand = ComputeHand(CurrentPlayer);
        //Debug.Log("PLAYER" + CurrentPlayer + " HAND" + string.Join(",", playerHand));

        //if any tile in players hand has 4 of the same kind (same index)
        for (int i = 0; i < 34; i++) {
            if (playerHand[i] == 4)
            {
                //set gangplayer as currentplayer
                gangplayer = CurrentPlayer;
                //isInExposed is an indicator to show if the other 3 tiles are in player's exposed or concealed container
                isInExposed = false;
                return true;
            }
        }

        //check for 3 of the same kind (same index) as drawn tile in player's exposed container
        GameObject PlayerExposedContainer;
        switch (CurrentPlayer)
        {
            case 1:
                PlayerExposedContainer = Player1ExposedContainer;
                break;
            case 2:
                PlayerExposedContainer = Player2ExposedContainer;
                break;
            case 3:
                PlayerExposedContainer = Player3ExposedContainer;
                break;
            case 4:
                PlayerExposedContainer = Player4ExposedContainer;
                break;
            default:
                PlayerExposedContainer = null;
                Debug.Log("Invalid player number");
                break;
        }
        Debug.Log("MELD SETS IN EXPOSED: " + PlayerExposedContainer.transform.childCount);

        if (PlayerExposedContainer.transform.childCount != 0)
        {
            foreach (Transform setcontainer in PlayerExposedContainer.transform)
            {
                if (setcontainer.GetChild(0).gameObject.name == freshdrawtile)
                {
                    if (setcontainer.GetChild(1).gameObject.name == freshdrawtile)
                    {
                        gangplayer = CurrentPlayer;
                        isInExposed = true;
                        return true;

                    }

                }
            }
        }

        //else return false
        return false;
    }

    //check for qualifying pong/gang moves
    bool[] CheckForPongGang() {

        //same as ponggangset[] previously mentioned in game loop
        bool[] ponggang = new bool[2];
        ponggang[0] = false;
        ponggang[1] = false;

        //check through all players - only possible for 1 player to perform pong/gang move if any.
        for (int i = 1; i < 5; i++)
        {
            //if current player, skip
            if (CurrentPlayer != i)
            {
                Debug.Log("CHECKING FOR PLAYER: " + i);
                int discardIndex = GetDiscardTile();
                Debug.Log("DISCARD TILE CHECKING: " + discardIndex);
                int[] playerHand = new int[34];
                //compute hand according to format previously mentioned
                playerHand = ComputeHand(i);
                //Debug.Log("PLAYER" + i + " HAND" + playerHand);
                Debug.Log("PLAYER" + i + " HAND" + string.Join(",", playerHand));

                //if there are 2 counts of the discarded tile index in the player's hand (not exposed), pong move qualifies
                if (playerHand[discardIndex] > 1)
                {
                    ponggang[0] = true;
                    pongplayer = i;
                    //if there are 3 counts, gang move qualifies
                    if (playerHand[discardIndex] == 3)
                    {
                        ponggang[1] = true;
                        gangplayer = i;
                    }
                    Debug.Log("PONG: " + ponggang[0] + " GANG: " + ponggang[1]);
                    i = 5;
                }
                Debug.Log("PONG: " + ponggang[0] + " GANG: " + ponggang[1]);
            }
        }
        return ponggang;
    }

    //this functions checks the players concealed hand with 4 methods and returns the one that gives it the best results
    public int[] checkTileSet(int[] handSet)
    {
        int[] sCount = shiftFirstCheck(handSet);
        int[] mCount = meldFirstCheck(handSet);
        int[] sbCount = shiftFirstCheckBackwards(handSet);
        int[] mbCount = meldFirstCheckBackwards(handSet);
        Debug.Log("meldFirst Count: " + mCount[0] + " meldFirst Eye: " + mCount[1]);
        Debug.Log("shiftFirst Count: " + sCount[0] + " shiftFirst Eye: " + sCount[1]);
        Debug.Log("shiftbFirst Count: " + sbCount[0] + " shiftbFirst Eye: " + sbCount[1]);
        Debug.Log("meldbFirst Count: " + mbCount[0] + " meldbFirst Eye: " + mbCount[1]);

        int[][] allCount = new int[4][];
        allCount[0] = sCount;
        allCount[1] = mCount;
        allCount[2] = sbCount;
        allCount[3] = mbCount;

        //check for eye
        for (int i = 0; i < 4; i++)
        {
            Debug.Log("EYE COUNT FOR Count Set " + i + "in checkTileSet is" + allCount[i][1]);
            if (allCount[i][1] != 1)
            {
                allCount[i] = new int[] { 0, 0 };
                Debug.Log("Count Set " + i + "in checkTileSet is OUT");
            }
        }

        //check for set count
        int[] highestCount = allCount[0];
        for (int i = 1; i < 4; i++)
        {
            if (allCount[i][0] > highestCount[0])
            {
                highestCount = allCount[i];
            }
        }
        return highestCount;
    }


    //method 1
    int[] meldFirstCheck(int[] playerHand)
    {
        int[] tempHand = new int[34];
        Array.Copy(playerHand, tempHand, 34);
        int[] totalCount = new int[2] { 0, 0 };

        for (int i = 0; i < 34; i++) //Check for pong
        {
            if (tempHand[i] == 3)
            {
                tempHand[i] -= 3;
                totalCount[0] += 1;
            }
        }
        for (int x = 0; x < 3; x++) //To keep track of the 3 suits and check for shifts
        {
            for (int i = 0 + 9 * x; i < 7 + 9 * x; i++) //only need to check for wan tong suo
            {
                int countShift = 0;
                for (int j = i; j < i + 3; j++)
                {
                    if (tempHand[j] > 0)
                    {
                        countShift++;
                        if (countShift == 3)
                        {
                            for (int k = 0; k < 3; k++)
                            {
                                tempHand[i + k] -= 1;
                            }
                            i = -1;
                            totalCount[0] += 1;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        for (int i = 0; i < 34; i++) //Check for eye
        {
            if (tempHand[i] == 2)
            {
                tempHand[i] -= 2;
                totalCount[1] += 1;
            }
        }

        return totalCount;
    }

    //method 2
    int[] meldFirstCheckBackwards(int[] playerHand)
    {
        int[] tempHand = new int[34];
        Array.Copy(playerHand, tempHand, 34);
        int[] totalCount = new int[2] { 0, 0 };

        for (int i = 0; i < 34; i++) //Check for pong
        {
            if (tempHand[i] == 3)
            {
                tempHand[i] -= 3;
                totalCount[0] += 1;
            }
        }
        for (int x = 0; x < 3; x++) //To keep track of the 3 suits and check for shifts
        {
            for (int i = 8 + 9 * x; i > 1 + (9 * x); i--) //check chow - only need to check for wan tong suo
            {
                int countShift = 0;
                for (int j = i; j > i - 3; j--)
                {
                    if (tempHand[j] > 0)
                    {
                        countShift++;
                        if (countShift == 3)
                        {
                            for (int k = 2; k > -1; k--)
                            {
                                tempHand[i - k] -= 1;
                            }
                            i += 1;
                            totalCount[0] += 1;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        for (int i = 0; i < 34; i++) //Check for eye
        {
            if (tempHand[i] == 2)
            {
                tempHand[i] -= 2;
                totalCount[1] += 1;
            }
        }

        return totalCount;
    }

    //method 3
    int[] shiftFirstCheck(int[] playerHand)
    {
        int[] tempHand = new int[34];
        Array.Copy(playerHand, tempHand, 34);
        //temp hand shall be remaining tiles in the player's container (not exposed)
        int[] totalCount = new int[2] { 0, 0 };

        for (int x = 0; x < 3; x++) //To keep track of the 3 suits and check for shifts
        {
            for (int i = 0 + 9 * x; i < 7 + 9 * x; i++) //only need to check for wan tong suo
            {
                int countShift = 0;
                for (int j = i; j < i + 3; j++)
                {
                    if (tempHand[j] > 0)
                    {
                        countShift++;
                        if (countShift == 3)
                        {
                            for (int k = 0; k < 3; k++)
                            {
                                tempHand[i + k] -= 1;
                            }
                            i = -1;
                            totalCount[0] += 1;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        for (int i = 0; i < 34; i++) //Check for pong
        {
            if (tempHand[i] == 3)
            {
                tempHand[i] -= 3;
                totalCount[0] += 1;
            }
        }
        for (int i = 0; i < 34; i++) //Check for eye
        {
            if (tempHand[i] == 2)
            {
                tempHand[i] -= 2;
                totalCount[1] += 1;
            }
        }
        return totalCount;
    }

    //method 4
    int[] shiftFirstCheckBackwards(int[] playerHand)
    {
        int[] tempHand = new int[34];
        Array.Copy(playerHand, tempHand, 34);
        //temp hand shall be remaining tiles in the player's container (not exposed)
        int[] totalCount = new int[2] { 0, 0 };

        for (int x = 0; x < 3; x++) //To keep track of the 3 suits and check for shifts
        {
            for (int i = 8 + 9 * x; i > 1 + (9 * x); i--) //check chow - only need to check for wan tong suo
            {
                int countShift = 0;
                for (int j = i; j > i - 3; j--)
                {
                    if (tempHand[j] > 0)
                    {
                        countShift++;
                        if (countShift == 3)
                        {
                            for (int k = 2; k > -1; k--)
                            {
                                tempHand[i - k] -= 1;
                            }
                            i += 1;
                            totalCount[0] += 1;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        for (int i = 0; i < 34; i++) //Check for pong
        {
            if (tempHand[i] == 3)
            {
                tempHand[i] -= 3;
                totalCount[0] += 1;
            }
        }
        for (int i = 0; i < 34; i++) //Check for eye
        {
            if (tempHand[i] == 2)
            {
                tempHand[i] -= 2;
                totalCount[1] += 1;
            }
        }
        return totalCount;
    }

    //checks if player is able to win
    bool CheckForHu(int[] playerHand)
    {
        int[] tempHand = new int[34];
        Array.Copy(playerHand, tempHand, 34);
        Debug.Log("CheckForHu tempHand = " + string.Join(", ", tempHand));
        int[] tempCount = checkTileSet(tempHand);
        int concealedtiles = GetConcealedTiles(tempHand);
        Debug.Log("concealed tiles count = " + concealedtiles);
        int requiredmelds = 0;
        
        switch (concealedtiles)
        {
            //if there are 14 remaining tiles in player's concealed hand, there needs to be 4 melds and 1 eye
            case 14:
                requiredmelds = 4;
                break;
            //if there are 11 remaining tiles in player's concealed hand, there needs to be 3 melds and 1 eye
            case 11:
                requiredmelds = 3;
                break;
            //if there are 8 remaining tiles in player's concealed hand, there needs to be 2 melds and 1 eye
            case 8:
                requiredmelds = 2;
                break;
            //if there are 5 remaining tiles in player's concealed hand, there needs to be 1 melds and 1 eye
            case 5:
                requiredmelds = 1;
                break;
            //if there are 2 remaining tiles in player's concealed hand, there needs to be 0 melds and 1 eye
            case 2:
                requiredmelds = 0;
                break;
            default:
                requiredmelds = 99;
                break;
        }
        Debug.Log("CHECK FOR HU ENTERED, CURRENT REQUIRED MELDS: " + requiredmelds);
        Debug.Log("CURRENT EYE COUNT: " + tempCount[1]);
        if (tempCount[0] == requiredmelds && tempCount[1] == 1)
        {
            //PLAYER CAN WIN
            Debug.Log("CAN HU");
            return true;
        }
        else
        {
            //PLAYER CANNOT WIN
            Debug.Log("CANNOT HU");
            return false;
        }
    }

    //retrieves number of concealed tiles in player's hand, this is for check for hu (check for win)
    int GetConcealedTiles(int[] playerHand)
    {
        Debug.Log("GetConcealedTiles received: " + string.Join(", ", playerHand));
        int totaltiles = 0;
        for (int i = 0; i < 34; i++)
        {
            totaltiles = totaltiles + playerHand[i];
        }
        Debug.Log("TOTAL CONCEALED TILES: " + totaltiles);
        return totaltiles;
    }

    //asign next player to be pong player who just executed pong move
    void AssignNextPlayer() {
        CurrentPlayer = pongplayer;
        Debug.Log("PLAYER: " + CurrentPlayer);
        turncount++;
    }

    //execute qualifying chow move's tiles to respective containers (qualifying player exposed container and create meld set container)
    void ExecuteChow(int[][] meldsets)
    {
        int discardIndex = GetDiscardTile();
        int[] chosenmeld = new int[3];
        int[] notdiscardtile = new int[2];
        bool rotate = false;

        Debug.Log("EXECUTE CHOW DISCARD INDEX: " + discardIndex);
        //identify which tile isnt discard tile
        switch (btnSelected)
        {
            case 1:
                chosenmeld = meldsets[0];
                playerchow = true;
                break;
            case 2:
                chosenmeld = meldsets[1];
                playerchow = true;
                break;
            case 3:
                chosenmeld = meldsets[2];
                playerchow = true;
                break;
            default:
                Debug.Log("Invalid Execute Chow");
                break;
        }

        NextPlayer();
        GameObject PlayerContainer;
        GameObject PlayerExposedContainer;
        switch (CurrentPlayer)
        {
            case 1:
                PlayerContainer = Player1Container;
                PlayerExposedContainer = Player1ExposedContainer;
                if (p1_TileFace == true)
                {
                    rotate = true;
                }
                break;
            case 2:
                PlayerContainer = Player2Container;
                PlayerExposedContainer = Player2ExposedContainer;
                if (p2_TileFace == true)
                {
                    rotate = true;
                }
                break;
            case 3:
                PlayerContainer = Player3Container;
                PlayerExposedContainer = Player3ExposedContainer;
                if (p3_TileFace == true)
                {
                    rotate = true;
                }
                break;
            case 4:
                PlayerContainer = Player4Container;
                PlayerExposedContainer = Player4ExposedContainer;
                if (p4_TileFace == true)
                {
                    rotate = true;
                }
                break;
            default:
                PlayerContainer = null;
                PlayerExposedContainer = null;
                Debug.Log("Invalid player number");
                break;
        }

        GameObject meldsetcontainer = Instantiate(MeldSetContainerPrefab, PlayerExposedContainer.transform);

        for (int i = 0; i < 3; i++)
        {
            string tilename;
            if (chosenmeld[i].ToString().Length == 1) { tilename = "0" + chosenmeld[i].ToString(); }
            else { tilename = chosenmeld[i].ToString(); }

            Debug.Log("Tile Name is: " + tilename);
            if (chosenmeld[i] != discardIndex)
            {
                if (rotate) {
                    PlayerContainer.transform.Find(tilename).gameObject.GetComponent<GameTileScript>().toggleTileFace();
                }
                PlayerContainer.transform.Find(tilename).gameObject.transform.SetParent(PlayerExposedContainer.transform.GetChild(PlayerExposedContainer.transform.childCount - 1).transform);
            }
            else
            {
                Debug.Log("DISCARDED TILE NAME: " + DiscardContainer.transform.GetChild(DiscardContainer.transform.childCount - 1).gameObject.name);
                DiscardContainer.transform.GetChild(DiscardContainer.transform.childCount - 1).gameObject.transform.SetParent(PlayerExposedContainer.transform.GetChild(PlayerExposedContainer.transform.childCount - 1).transform);
            }
        }

        playerchow = true;
        btnSelected = -99;
        Debug.Log("CHOSEN MELD: " + chosenmeld[0] + "-" + chosenmeld[1] + "-" + chosenmeld[2]);
    }

    //execute qualifying gang move's tiles to respective containers (qualifying player exposed container and create meld set container)
    //qualifying player also becomes the next player in turn
    void ExecutePlayerGang() {
        int gangtile = -99;
        string gangtilename;
        GameObject PlayerContainer;
        GameObject PlayerExposedContainer;
        bool rotate = false;
        switch (CurrentPlayer)
        {
            case 1:
                PlayerContainer = Player1Container;
                PlayerExposedContainer = Player1ExposedContainer;
                if (p1_TileFace == true)
                {
                    rotate = true;
                }
                break;
            case 2:
                PlayerContainer = Player2Container;
                PlayerExposedContainer = Player2ExposedContainer;
                if (p2_TileFace == true)
                {
                    rotate = true;
                }
                break;
            case 3:
                PlayerContainer = Player3Container;
                PlayerExposedContainer = Player3ExposedContainer;
                if (p3_TileFace == true)
                {
                    rotate = true;
                }
                break;
            case 4:
                PlayerContainer = Player4Container;
                PlayerExposedContainer = Player4ExposedContainer;
                if (p4_TileFace == true)
                {
                    rotate = true;
                }
                break;
            default:
                PlayerContainer = null;
                PlayerExposedContainer = null;
                Debug.Log("Invalid player number");
                break;
        }

        if (isInExposed)
        {
            isInExposed = false;
            foreach (Transform setcontainer in PlayerExposedContainer.transform)
            {
                if (setcontainer.GetChild(0).gameObject.name == freshdrawtile){
                    if (rotate)
                    {
                        PlayerContainer.transform.Find(freshdrawtile).gameObject.GetComponent<GameTileScript>().toggleTileFace();
                    }
                    PlayerContainer.transform.Find(freshdrawtile).gameObject.transform.SetParent(setcontainer);
                }
            }

        }
        else { 
            int[] playerHand = new int[34];
            playerHand = ComputeHand(CurrentPlayer);
            GameObject meldsetcontainer = Instantiate(MeldSetContainerPrefab, PlayerExposedContainer.transform);
            for (int i = 0; i < 34; i++)
            {
                if (playerHand[i] == 4)
                {
                    gangtile = i;
                    break;
                }
            }

            Debug.Log("Gang Tile Converting to string: " + gangtile.ToString());
            if (gangtile.ToString().Length == 1) { gangtilename = "0" + gangtile.ToString(); }
            else { gangtilename = gangtile.ToString(); }
            Debug.Log("Tile Name is: " + gangtilename);

            for (int i = 0; i < 4; i++) {
                if (rotate)
                {
                    PlayerContainer.transform.Find(gangtilename).gameObject.GetComponent<GameTileScript>().toggleTileFace();
                }
                PlayerContainer.transform.Find(gangtilename).gameObject.transform.SetParent(PlayerExposedContainer.transform.GetChild(PlayerExposedContainer.transform.childCount - 1).transform);
            }
        }
        DrawCardFromBackWall();
    }

    //execute qualifying pong/gang move's tiles to respective containers (qualifying player exposed container and create meld set container)
    //qualifying player also becomes the next player in turn
    void ExecutePongGang() {
        int discardIndex = GetDiscardTile();
        int times;
        AssignNextPlayer();
        GameObject PlayerContainer;
        GameObject PlayerExposedContainer;
        bool rotate = false;
        switch (CurrentPlayer)
        {
            case 1:
                PlayerContainer = Player1Container;
                PlayerExposedContainer = Player1ExposedContainer;
                if (p1_TileFace == true) {
                    rotate = true;
                }
                break;
            case 2:
                PlayerContainer = Player2Container;
                PlayerExposedContainer = Player2ExposedContainer;
                if (p2_TileFace == true)
                {
                    rotate = true;
                }
                break;
            case 3:
                PlayerContainer = Player3Container;
                PlayerExposedContainer = Player3ExposedContainer;
                if (p3_TileFace == true)
                {
                    rotate = true;
                }
                break;
            case 4:
                PlayerContainer = Player4Container;
                PlayerExposedContainer = Player4ExposedContainer;
                if (p4_TileFace == true)
                {
                    rotate = true;
                }
                break;
            default:
                PlayerContainer = null;
                PlayerExposedContainer = null;
                Debug.Log("Invalid player number");
                break;
        }
        GameObject meldsetcontainer = Instantiate(MeldSetContainerPrefab, PlayerExposedContainer.transform);

        switch (btnSelected)
        {
            case 1:
                times = 2;
                playerponggang = true;
                break;
            case 2:
                times = 3;
                playerponggang = true;
                playergang = true;
                break;
            default:
                times = 0;
                Debug.Log("Invalid Execute PongGang");
                break;
        }

        if (times == 0) { return; } 
        Debug.Log("GOING TO SHIFT TILES FOR PLAYER " + CurrentPlayer + " AND TAKE TILE " + discardIndex);
        DiscardContainer.transform.GetChild(DiscardContainer.transform.childCount - 1).gameObject.transform.SetParent(PlayerExposedContainer.transform.GetChild(PlayerExposedContainer.transform.childCount - 1).transform);

        for (int i = 0; i < times; i++)
        {
            string tilename;
            Debug.Log("Discard Tile Converting to string: " + discardIndex);
            if (discardIndex.ToString().Length == 1) { tilename = "0" + discardIndex.ToString(); }
            else { tilename = discardIndex.ToString(); }
            Debug.Log("Tile Name is: " + tilename);
            if (rotate)
            {
                PlayerContainer.transform.Find(tilename).gameObject.GetComponent<GameTileScript>().toggleTileFace();
            }
            PlayerContainer.transform.Find(tilename).gameObject.transform.SetParent(PlayerExposedContainer.transform.GetChild(PlayerExposedContainer.transform.childCount - 1).transform);
        }
    }
}

//public class Player {
//    public int seat;
//    public int[] playerHandConcealed;
//    public int[] playerHandExposed;
//    //public int bonusTai;
//    //public bool isWaiting;

//    public Player(int currentIndex) {
//        this.seat = currentIndex;
//    }

//    public int[] getPlayerHandConcealed() { 
        
//    }

//    public int[] setPlayerHandConcealed()
//    {

//    }

//    public int[] getPlayerHandExposed()
//    {

//    }

//    public int[] setPlayerHandExposed()
//    {

//    }
//}

//public class GameState {
//    public int currentWind;
//    public bool isReady;
//    public bool inGame;
//}