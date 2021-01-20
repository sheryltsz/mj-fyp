 using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManagerScript : MonoBehaviour
{

    //STATIC VARIABLES    
    //public Sprite[] TileFaces;
    public GameObject[] TileFaces;
    public int CurrentPlayer;
    int readyGame = 0;
    int turncount = 0;
    int counttime;
    public bool responded = false;
    bool winnerfound = false;
    bool endofturn = true;
    public int btnSelected = -99;
    public string tiletodiscard = "";

    //Bamboo - 1 to 9
    //Character - 10 to 18
    //Dots - 19 to 27
    //Dragon - 28 to 30
    //Wind - 31 to 34

    //LISTS VARIABLES
    public List<string> tileset;

    public List<string> Tiles;
    public List<string> player1Tiles;
    public List<string> player2Tiles;
    public List<string> player3Tiles;
    public List<string> player4Tiles;

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
    public GameObject MeldSetContainerPrefab;

    //SCRIPTS VARIABLES
    InputManagerScript inputmanager;

    bool playerchow = false;
    bool playerponggang = false;
    int pongplayer = -99;

    // Start is called before the first frame update
    void Start()
    {
        //INITIALISATION
        inputmanager = FindObjectOfType<InputManagerScript>();

        //GAMESTART METHOD
        SetupTiles();
        //OptionsContainer.SetActive(false);

        //GAMELOOP
        StartCoroutine(GameLoop());
        //StartCoroutine(TestLoop());


    }

    // Update is called once per frame
    void Update()
    {
        if (turncount == 84)
        {
            readyGame = 0;
            //end the game
        }

    }

    IEnumerator GameLoop() {
        while (readyGame == 1)
        {
            if (endofturn == true && !winnerfound)
            {
                endofturn = false;
                Debug.Log("TURNCOUNT: " + turncount);

                //================DRAW FROM WALL================
                //check !first round and !just chow/pong
                if (turncount != 1 && playerchow == false && playerponggang == false)
                {
                    DrawCardFromWall();
                }
                else {
                    playerchow = false;
                    playerponggang = false;
                }
                //================END OF DRAW FROM WALL================

                //================WAIT FOR DISCARD================
                Debug.Log("ENTER WaitForDiscard");
                for (int i = 0; i < 5; i++)
                {
                    yield return new WaitForSeconds(1);
                    if (responded == true)
                    {
                        break;
                    }
                }
                if (responded == true)
                {
                    Debug.Log("RESPONDED");
                    responded = false;
                    DiscardTile(tiletodiscard);
                }
                else
                {
                    Debug.Log("No response but 5 seconds up");
                    DiscardMostRightTile();
                }
                //================END OF WAIT FOR DISCARD================

                //================CHECK FOR PONG================
                bool[] ponggangset = new bool[2];
                ponggangset = CheckForPongGang();
                if (ponggangset.Contains(true))
                {
                    DisplayPongGangOptions(ponggangset);

                    //WAIT FOR PONG GANG RESPONSE
                    responded = false;
                    Debug.Log("Enter WaitForResponse-PONGGANG");
                    for (int i = 0; i < 5; i++)
                    {
                        yield return new WaitForSeconds(1);
                        if (responded == true)
                        {
                            break;
                        }
                    }
                    if (responded == true)
                    {
                        Debug.Log("WaitForResponse OPTION BUTTON - RESPONDED");
                        responded = false;
                        ExecutePongGang();
                    }
                    else
                    {
                        Debug.Log("WaitForResponse No response but 5 seconds up");
                        //check chow
                        NextPlayer();
                    }
                    //END OF WAIT FOR PONG GANG RESPONSE
                    DestroyOptionButtons();
                    Time.timeScale = 1f;
                    Debug.Log("WaitForResponse Completed");

                }
                else {
                    NextPlayer();
                }
                //================END OF CHECK FOR PONG================



                ////================CHECK FOR CHOW================
                //int[][] meldsets = new int[][] {
                //    new int[3],
                //    new int[3],
                //    new int[3]
                //};

                //meldsets = CheckForChow();
                //if (meldsets != null)
                //{
                //    DisplayChowOptions(meldsets);

                //    //WAIT FOR CHOW RESPONSE
                //    responded = false;
                //    Debug.Log("Enter WaitForResponse-CHOW");
                //    for (int i = 0; i < 5; i++)
                //    {
                //        yield return new WaitForSeconds(1);
                //        if (responded == true)
                //        {
                //            break;
                //        }
                //    }
                //    Debug.Log("EXITED WAIT FOR RESPONSE WAIT 5 SECS LOOP");
                //    if (responded == true)
                //    {
                //        Debug.Log("WaitForResponse OPTION BUTTON - RESPONDED");
                //        responded = false;
                //        ExecuteChow(meldsets);
                //    }
                //    else
                //    {
                //        Debug.Log("WaitForResponse No response but 5 seconds up");
                //        NextPlayer();
                //    }
                //    //END OF WAIT FOR CHOW RESPONSE

                //    DestroyOptionButtons();
                //    Time.timeScale = 1f;
                //    Debug.Log("WaitForResponse Completed");
                //}
                //else if (meldsets == null)
                //{
                //    NextPlayer();
                //}
                ////================END OF CHECK FOR CHOW================
                
                endofturn = true;
            }
        }
    }
    //IEnumerator WaitForNext()
    //{
    //    //KIV check if win and if win, zi mo, winnerfound = true
    //    //StartCoroutine(CheckForOwnGang());
    //    //`~----Yes: ExecuteGang(); ---
    //    //`~----No: Wait for Discard

    //    //Wait for Discard
    //    Debug.Log("ENTER WaitForNext");
    //    for (int i = 0; i < 5; i++)
    //    {
    //        yield return new WaitForSeconds(1);
    //        if (responded == true)
    //        {
    //            break;
    //        }
    //    }
    //    if (responded == true)
    //    {
    //        Debug.Log("RESPONDED");
    //        responded = false;
    //    }
    //    else
    //    {
    //        Debug.Log("No response but 5 seconds up");
    //        DiscardMostRightTile();
    //    }
    //    //end of Wait for Discard

    //    //CheckForPongGang(); -- check if can 3 other players can pong
    //    //`~---Yes: Call for function to ask player if WANT to pong/gang
    //    //         `~-----Yes: Call for function to ask player if WANT to pong/gang
    //    //                      `~---Yes: AssignAsNextPlayer(); ExecutePong(); 
    //    //                      `~---No: CheckForChow();
    //    //`~---No: CheckForChow() -- check if next player can eat
    //    //         `~-----Yes: Call for function to ask player if WANT to chow
    //    //                      `~---Yes: ExecuteChow(); NextPlayer();
    //    //                      `~---No:  NextPlayer();
    //    //         `~-----No: NextPlayer();
    //    //WaitForPongGang();

    //    //if (CheckForPongGang())
    //    //{
    //    //    if (ToExecutePongGang())
    //    //    {
    //    //        ExecutePongGang();
    //    //        AssignAsNextPlayer();
    //    //    }
    //    //    else
    //    //    {
    //    //        if (CheckForChow())
    //    //        {
    //    //            if (ToExecuteChow())
    //    //            {
    //    //                ExecuteChow();
    //    //            }
    //    //            else
    //    //            {
    //    //                NextPlayer();
    //    //            }
    //    //        }
    //    //        else
    //    //        {
    //    //            NextPlayer();
    //    //        }
    //    //    }
    //    //}
    //    //else
    //    //{
    //    //    if (!CheckForChow) { 

    //    //    }
    //    //    if (CheckForChow())
    //    //    {
    //    //        if (ToExecuteChow())
    //    //        {
    //    //            ExecuteChow();
    //    //        }
    //    //        else
    //    //        {
    //    //            NextPlayer();
    //    //        }
    //    //    }
    //    //    else
    //    //    {
    //    //        NextPlayer();
    //    //    }
    //    //}
    //    CheckForChow();
    //    NextPlayer();

    //    endofturn = true;
    //}


    void SetupTiles()
    {

        tileset = GenerateTileSet();
        Shuffle(tileset);
        CreateTileSet();
        DealTiles();
    }

    //IEnumerator WaitForDiscard()
    //{
    //    Debug.Log("ENTER WAITFORDISCARD");
    //    for (int i = 0; i < 5; i++)
    //    {
    //        yield return new WaitForSeconds(1);
    //        if (responded == true)
    //        {
    //            break;
    //        }
    //    }
    //    if (responded == true)
    //    {
    //        Debug.Log("RESPONDED");
    //        responded = false;
    //    }
    //    else
    //    {
    //        Debug.Log("No response but 5 seconds up");
    //        DiscardMostRightTile();
    //    }
    //    //NextPlayer();
    //    Debug.Log("waiting");
    //}

    static List<string> GenerateTileSet()
    {

        List<string> newTileSet = new List<string>();

        //this nested loop creates a card deck of 52 cards 
        for (int i = 1; i < 35; i++)
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

    //spawn deck cards
    void CreateTileSet()
    {

        foreach (string tile in tileset)
        {
            for (int i = 0; i > -1; i++)
            {
                if (tile == TileFaces[i].name)
                {
                    GameObject newTile = Instantiate(TileFaces[i], TileContainer.transform);
                    newTile.name = tile;
                    //print(tile);
                    //print("A tile face match was found with Tile: " + this.name + " and TileFace: " + gm.TileFaces[i].name);
                    break;
                }
            }
        }
    }

    //deal cards to player 1 and player 2 hands
    void DealTiles()
    {
        int playernow = 1;

        for (int i = 0; i < 53; i++)
        {
            //for (int j = 0; j < 4; j++)
            //{
            switch (playernow)
            {
                case 1:
                    //player1Tiles.Add(TileContainer.transform.GetChild(0).gameObject.name);
                    TileContainer.transform.GetChild(0).gameObject.transform.SetParent(Player1Container.transform, false);
                    Player1Container.transform.GetChild(Player1Container.transform.childCount - 1).GetComponent<GameTileScript>().toggleTileFace();
                    break;
                case 2:
                    TileContainer.transform.GetChild(0).gameObject.transform.SetParent(Player2Container.transform, false);
                    Player2Container.transform.GetChild(Player2Container.transform.childCount - 1).GetComponent<GameTileScript>().toggleTileFace();
                    break;
                case 3:
                    TileContainer.transform.GetChild(0).gameObject.transform.SetParent(Player3Container.transform, false);
                    Player3Container.transform.GetChild(Player3Container.transform.childCount - 1).GetComponent<GameTileScript>().toggleTileFace();
                    break;
                case 4:
                    TileContainer.transform.GetChild(0).gameObject.transform.SetParent(Player4Container.transform, false);
                    Player4Container.transform.GetChild(Player4Container.transform.childCount - 1).GetComponent<GameTileScript>().toggleTileFace();
                    break;
                default:
                    Debug.Log("Invalid player number");
                    break;
            }

            //}
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

        CurrentPlayer = 1;
        RearrangeCards(Player1Container);
        RearrangeCards(Player2Container);
        RearrangeCards(Player3Container);
        RearrangeCards(Player4Container);
        readyGame = 1;
        turncount = 1;
    }

    public string GetParent(Component card)
    {
        return card.gameObject.transform.parent.name;
    }

    public void DiscardTile(string tiletodiscard)
    {
        GameObject playerContainer;
        switch (CurrentPlayer)
        {
            case 1:
                playerContainer = Player1Container;
                break;
            case 2:
                playerContainer = Player2Container;
                break;
            case 3:
                playerContainer = Player3Container;
                break;
            case 4:
                playerContainer = Player4Container;
                break;
            default:
                Debug.Log("Invalid player number");
                playerContainer = null;
                break;
        }
        playerContainer.gameObject.transform.Find(tiletodiscard).gameObject.transform.SetParent(DiscardContainer.transform);
    }

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

    void DiscardMostRightTile()
    {
        switch (CurrentPlayer)
        {
            case 1:
                Player1Container.transform.GetChild(Player1Container.transform.childCount - 1).gameObject.transform.SetParent(DiscardContainer.transform);
                break;
            case 2:
                Player2Container.transform.GetChild(Player2Container.transform.childCount - 1).gameObject.transform.SetParent(DiscardContainer.transform);
                break;
            case 3:
                Player3Container.transform.GetChild(Player3Container.transform.childCount - 1).gameObject.transform.SetParent(DiscardContainer.transform);
                break;
            case 4:
                Player4Container.transform.GetChild(Player4Container.transform.childCount - 1).gameObject.transform.SetParent(DiscardContainer.transform);
                break;
            default:
                Debug.Log("Invalid player number");
                break;
        }
    }

    void DrawCardFromWall()
    {
        Debug.Log("Draw From Wall");
        switch (CurrentPlayer)
        {
            case 1:
                TileContainer.transform.GetChild(0).gameObject.transform.SetParent(Player1Container.transform, false);
                Player1Container.transform.GetChild(Player1Container.transform.childCount - 1).GetComponent<GameTileScript>().toggleTileFace();
                RearrangeCards(Player1Container);
                break;
            case 2:
                TileContainer.transform.GetChild(0).gameObject.transform.SetParent(Player2Container.transform, false);
                Player2Container.transform.GetChild(Player2Container.transform.childCount - 1).GetComponent<GameTileScript>().toggleTileFace();
                RearrangeCards(Player2Container);
                break;
            case 3:
                TileContainer.transform.GetChild(0).gameObject.transform.SetParent(Player3Container.transform, false);
                Player3Container.transform.GetChild(Player3Container.transform.childCount - 1).GetComponent<GameTileScript>().toggleTileFace();
                RearrangeCards(Player3Container);
                break;
            case 4:
                TileContainer.transform.GetChild(0).gameObject.transform.SetParent(Player4Container.transform, false);
                Player4Container.transform.GetChild(Player4Container.transform.childCount - 1).GetComponent<GameTileScript>().toggleTileFace();
                RearrangeCards(Player4Container);
                break;
            default:
                Debug.Log("Invalid player number");
                break;
        }

    }

    int GetNextPlayer()
    {
        int temp;
        switch (CurrentPlayer)
        {
            case 1:
                temp = 2;
                return temp;
            case 2:
                temp = 3;
                return temp;
            case 3:
                temp = 4;
                return temp;
            case 4:
                temp = 1;
                return temp;
            default:
                Debug.Log("Invalid player number");
                return temp = -99;
        }
    }

    void ExecuteChow(int[][] meldsets)
    {
        int discardIndex = GetDiscardTile();
        int[] chosenmeld = new int[3];
        int[] notdiscardtile = new int[2];

        Debug.Log("EXECUTE CHOW DISCARD INDEX: " + discardIndex);
        //identify which tile isnt discard tile
        switch (btnSelected)
        {
            case 0:
                chosenmeld = meldsets[0];
                playerchow = true;
                break;
            case 1:
                chosenmeld = meldsets[1];
                playerchow = true;
                break;
            case 2:
                chosenmeld = meldsets[2];
                playerchow = true;
                break;
            default:
                Debug.Log("Invalid Execute Chow");
                break;
        }
        //extract other 2 tiles from playercontainer and bring to P_TilesExposed***

        //int notdiscardtileindex = 0;
        //for (int i = 0; i < 3; i++)
        //{
        //    if (chosenmeld[i] != discardIndex)
        //    {
        //        notdiscardtile[notdiscardtileindex] = chosenmeld[i];
        //        notdiscardtileindex++;
        //    }
        //}

        //for (int i = 0; i < 3; i++) {
        //    chosenmeld[i] = chosenmeld[i];
        //}
        NextPlayer();
        GameObject PlayerContainer;
        GameObject PlayerExposedContainer;
        switch (CurrentPlayer)
        {
            case 1:
                PlayerContainer = Player1Container;
                PlayerExposedContainer = Player1ExposedContainer;
                break;
            case 2:
                PlayerContainer = Player2Container; 
                PlayerExposedContainer = Player2ExposedContainer;
                break;
            case 3:
                PlayerContainer = Player3Container;
                PlayerExposedContainer = Player3ExposedContainer;
                break;
            case 4:
                PlayerContainer = Player4Container;
                PlayerExposedContainer = Player4ExposedContainer;
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

                PlayerContainer.transform.Find(tilename).gameObject.transform.SetParent(PlayerExposedContainer.transform.GetChild(PlayerExposedContainer.transform.childCount - 1).transform);
            }
            else {
                Debug.Log("DISCARDED TILE NAME: " + DiscardContainer.transform.GetChild(DiscardContainer.transform.childCount - 1).gameObject.name);
                DiscardContainer.transform.GetChild(DiscardContainer.transform.childCount - 1).gameObject.transform.SetParent(PlayerExposedContainer.transform.GetChild(PlayerExposedContainer.transform.childCount - 1).transform);
            }
        }

        playerchow = true;
        btnSelected = -99;
        Debug.Log("CHOSEN MELD: " + chosenmeld[0] + "-" + chosenmeld[1] + "-" + chosenmeld[2]);
    }

    void DisplayPongGangOptions(bool[] ponggangset) {
        GameObject overlayPrefab = Instantiate(OverlayPanelPrefab, GameCanvas.transform);
        if (ponggangset[0] == true) {
            GameObject optionButton = Instantiate(OptionsButtonPrefab, overlayPrefab.transform);
            optionButton.name = "OptionButton" + "0";
            optionButton.GetComponentInChildren<Text>().text = "PONG";
        }
        if (ponggangset[1] == true)
        {
            GameObject optionButton = Instantiate(OptionsButtonPrefab, overlayPrefab.transform);
            optionButton.name = "OptionButton" + "1";
            optionButton.GetComponentInChildren<Text>().text = "GANG";
        }
    }

    bool[] CheckForPongGang() {
        bool[] ponggang = new bool[2];
        ponggang[0] = false;
        ponggang[1] = false;
        for (int i = 1; i < 5; i++)
        {
            if (CurrentPlayer != i)
            {
                Debug.Log("CHECKING FOR PLAYER: " + i);
                int discardIndex = GetDiscardTile() - 1;
                Debug.Log("DISCARD TILE CHECKING: " + discardIndex);
                int[] playerHand = new int[34];
                playerHand = ComputeHand(i);
                //Debug.Log("PLAYER" + i + " HAND" + playerHand);
                Debug.Log("PLAYER" + i + " HAND" + string.Join(",", playerHand));

                if (playerHand[discardIndex] > 1)
                {
                    ponggang[0] = true;
                    pongplayer = i;
                    if (playerHand[discardIndex] == 3)
                    {
                        ponggang[1] = true;
                    }
                    Debug.Log("PONG: " + ponggang[0] + " GANG: " + ponggang[1]);
                    i = 5;
                }
                Debug.Log("PONG: " + ponggang[0] + " GANG: " + ponggang[1]);
            }
        }
        return ponggang;
    }

    void AssignNextPlayer() {
        CurrentPlayer = pongplayer;
        Debug.Log("PLAYER: " + CurrentPlayer);
        turncount++;
    }
    void ExecutePongGang() {
        int discardIndex = GetDiscardTile();
        int times;
        AssignNextPlayer();
        GameObject PlayerContainer;
        GameObject PlayerExposedContainer;
        switch (CurrentPlayer)
        {
            case 1:
                PlayerContainer = Player1Container;
                PlayerExposedContainer = Player1ExposedContainer;
                break;
            case 2:
                PlayerContainer = Player2Container;
                PlayerExposedContainer = Player2ExposedContainer;
                break;
            case 3:
                PlayerContainer = Player3Container;
                PlayerExposedContainer = Player3ExposedContainer;
                break;
            case 4:
                PlayerContainer = Player4Container;
                PlayerExposedContainer = Player4ExposedContainer;
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
            case 0:
                times = 2;
                playerponggang = true;
                break;
            case 1:
                times = 3;
                playerponggang = true;
                break;
            default:
                times = 0;
                Debug.Log("Invalid Execute PongGang");
                break;
        }

        Debug.Log("GOING TO SHIFT TILES FOR PLAYER " + CurrentPlayer + " AND TAKE TILE " + discardIndex);
        DiscardContainer.transform.GetChild(DiscardContainer.transform.childCount - 1).gameObject.transform.SetParent(PlayerExposedContainer.transform.GetChild(PlayerExposedContainer.transform.childCount - 1).transform);

        for (int i = 0; i < times; i++)
        {
            string tilename;
            Debug.Log("Discard Tile Converting to string: " + discardIndex);
            if (discardIndex.ToString().Length == 1) { tilename = "0" + discardIndex.ToString(); }
            else { tilename = discardIndex.ToString(); }
            Debug.Log("Tile Name is: " + tilename);
            PlayerContainer.transform.Find(tilename).gameObject.transform.SetParent(PlayerExposedContainer.transform.GetChild(PlayerExposedContainer.transform.childCount - 1).transform);
        }
    }

    void DestroyOptionButtons()
    {
        for (int i = 0; i < 3; i++)
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

    int GetDiscardTile()
    {
        string discardTileName = DiscardContainer.transform.GetChild(DiscardContainer.transform.childCount - 1).gameObject.name;
        int discardTile;
        Debug.Log("DISCARD TILE NAME FROM GETDISCARDTILE:  " + discardTileName);
        int.TryParse(discardTileName, out discardTile);
        Debug.Log("DISCARD TILE INT FROM GETDISCARDTILE:  " + discardTile);
        return discardTile;
    }

    void DisplayChowOptions(int[][] meldsets)
    {
        GameObject overlayPrefab = Instantiate(OverlayPanelPrefab, GameCanvas.transform);


        for (int i = 0; i < 3; i++)
        {
            if (meldsets[i] == null) { break; }

            Debug.Log(meldsets[i][0] + "-" + meldsets[i][1] + "-" + meldsets[i][2]);
        }
        //spawn option buttons
        for (int i = 0; i < 3; i++)
        {
            if (meldsets[i] == null) { break; }
            else
            {
                GameObject optionButton = Instantiate(OptionsButtonPrefab, overlayPrefab.transform);
                optionButton.name = "OptionButton" + i.ToString();
                optionButton.GetComponentInChildren<Text>().text = meldsets[i][0] + "-" + meldsets[i][1] + "-" + meldsets[i][2];
            }
        }
    }

    int[][] CheckForChow()
    {
        int discardTile = GetDiscardTile();
        int[] playerHand = new int[34];
        playerHand = ComputeHand(GetNextPlayer());
        discardTile -= 1;
        int meldsetindex = 0;
        //int[][] possiblemeldsets = new int[][] {
        //     new int[3],
        //     new int[3],
        //     new int[3]
        // };
        int[][] possiblemeldsets = new int[3][];
        Debug.Log("CHECK FOR CHOW ENTERED");

        if (discardTile < 27)
        {
            //add discard tile to hand temporarily
            playerHand[discardTile]++;

            int countShift;
            bool alrChecked;
            int suit = (int)Mathf.Floor(discardTile / 9);

            Debug.Log("Discard Tile: " + discardTile);

            Debug.Log("Suit of Discard: " + (suit));

            if (discardTile % 9 == 0) //This is only for shift pattern 1,2,3. Ignore i value
            {
                for (int i = 0 + 9 * suit; i < 1 + 9 * suit; i++)
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
                            Debug.Log("CAN CHOW: " + (i + 1) + " - " + (i + 2) + " - " + (i + 3));
                            possiblemeldsets[meldsetindex] = new int[] { i + 1, i + 2, i + 3 };
                            meldsetindex++;
                        }
                    }
                }
            }
            if (discardTile % 9 == 1)
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
                            Debug.Log("CAN CHOW: " + (i + 1) + " - " + (i + 2) + " - " + (i + 3));
                            possiblemeldsets[meldsetindex] = new int[] { i + 1, i + 2, i + 3 };
                            meldsetindex++;
                        }
                    }
                }
            }
            else if (discardTile % 9 > 6)
            {
                for (int i = 4 + 9 * suit; i < 7 + 9 * suit; i++)
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
                            Debug.Log("CAN CHOW: " + (i + 1) + " - " + (i + 2) + " - " + (i + 3));
                            possiblemeldsets[meldsetindex] = new int[] { i + 1, i + 2, i + 3 };
                            meldsetindex++;
                        }
                    }
                }
            }
            else if (discardTile % 9 > 1 && discardTile % 9 < 8)
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
                            Debug.Log("CAN CHOW: " + (i + 1) + " - " + (i + 2) + " - " + (i + 3));
                            possiblemeldsets[meldsetindex] = new int[] { i + 1, i + 2, i + 3 };
                            meldsetindex++;
                        }
                    }
                }
            }

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

        return null;

    }

    int[] ComputeHand(int player)
    {
        GameObject RightSidePlayerContainer;
        int[] tilesetcount = new int[34];
        switch (player)
        {
            case 1:
                RightSidePlayerContainer = Player1Container;
                break;
            case 2:
                RightSidePlayerContainer = Player2Container;
                break;
            case 3:
                RightSidePlayerContainer = Player3Container;
                break;
            case 4:
                RightSidePlayerContainer = Player4Container;
                break;
            default:
                RightSidePlayerContainer = null;
                Debug.Log("Invalid player number");
                break;
        }
        for (int i = 0; i < RightSidePlayerContainer.transform.childCount; i++)
        {
            string tileindexstring = RightSidePlayerContainer.transform.GetChild(i).gameObject.name;
            int tileindex;
            int.TryParse(tileindexstring, out tileindex);
            int temp = tileindex - 1;
            tilesetcount[temp]++;
        }
        return tilesetcount;
    }
}