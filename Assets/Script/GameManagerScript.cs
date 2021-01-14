﻿//MJ

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
    public GameObject TilePrefab;
    public GameObject TileContainer;
    public GameObject Player1Container;
    public GameObject Player2Container;
    public GameObject Player3Container;
    public GameObject Player4Container;
    public GameObject DiscardContainer;

    //SCRIPTS VARIABLES
    InputManagerScript inputmanager;


    // Start is called before the first frame update
    void Start()
    {
        //INITIALISATION
        inputmanager = FindObjectOfType<InputManagerScript>();

        //GAMESTART METHOD
        SetupTiles(); 
    }

    // Update is called once per frame
    void Update()
    {
        if (turncount == 84) {
            readyGame = 0;
            //end the game
        }
        if (readyGame == 1)
        {
            if (endofturn == true && !winnerfound)
            {
                endofturn = false;
                //check !first round and !just chow/pong
                //if yes, skip to StartCoroutine(WaitForDiscard());
                if (turncount != 1)
                {
                    DrawCardFromWall();
                }
                //KIV check if win and if win, zi mo, winnerfound = true
                //StartCoroutine(CheckForOwnGang());
                //`~----Yes: ExecuteGang(); ---
                //`~----No:
                StartCoroutine(WaitForDiscard());
                //StartCoroutine(CheckForPongGang()); -- check if can 3 other players can pong
                //`~---Yes: Call for function to ask player if WANT to pong/gang
                //         `~-----Yes: Call for function to ask player if WANT to pong/gang
                //                      `~---Yes: ExecutePong();
                //                      `~---No:  NextPlayer();
                CheckForChow();
                //`~---No: CheckForChow() -- check if next player can eat
                //         `~-----Yes: Call for function to ask player if WANT to chow
                //                      `~---Yes: ExecuteChow();
                //                      `~---No:  NextPlayer();
                //         `~-----No: NextPlayer();

            }
        }

    }

    void SetupTiles()
    {

        tileset = GenerateTileSet();
        Shuffle(tileset);
        CreateTileSet();
        DealTiles();
    }

    IEnumerator WaitForDiscard()
    {
        for (int i = 0; i < 5; i++) {
            yield return new WaitForSeconds(1);
            if (responded == true)
            {
                break;
            }        }
        if (responded == true)
        {
            Debug.Log("RESPONDED");
            responded = false;
        }
        else {
            Debug.Log("No response but 5 seconds up");
            DiscardMostRightTile();
        }
        NextPlayer();
        yield return new WaitForSeconds(3);

    }

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
                    print(tile);
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


        //separate to Player1Cards and Player2Cards BY NAME ONLY
        //for (int i = 0; i < 53; i++)
        //{
        //    if (playernow == 1)
        //    {
        //        player1Tiles.Add(TileContainer.transform.GetChild(i).gameObject.name);
        //        playernow = 2;
        //    }

        ////sort cards BY NAME ONLY
        //player1Tiles.Sort();

        ////transfer cards (game objects) to respective player containers
        //foreach (string tilename in player1Tiles)
        //{
        //    GameObject current = TileContainer.transform.Find(tilename).gameObject;
        //    current.transform.SetParent(Player1Container.transform, false);
        //    current.GetComponent<GameTileScript>().toggleTileFace();
        //    Debug.Log("Last Child:" + Player1Container.transform.GetChild(Player1Container.transform.childCount - 1).gameObject.name);
        //}

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

    public void DiscardTile(Component tile)
    {
        tile.gameObject.transform.SetParent(DiscardContainer.transform);
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
        turncount++;
        endofturn = true;
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
                temp = 2;
                return temp;
            case 3:
                temp = 2;
                return temp;
            case 4:
                temp = 2;
                return temp;
            default:
                Debug.Log("Invalid player number");
                return 99;
        }
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

    void DiscardMostRightTile() {
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

    void DrawCardFromWall() {
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

    void CheckForChow() {

        string discardTileName = DiscardContainer.transform.GetChild(DiscardContainer.transform.childCount - 1).gameObject.name;
        int discardTile;
        int.TryParse(discardTileName, out discardTile);
        int[] playerHand = new int[34];
        playerHand = ComputeHand();
        discardTile -= 1;
        if (discardTile < 27)
        {

            //add discard tile to hand temporarily
            playerHand[discardTile]++;

            int countShift;
            bool alrChecked;
            int multiple = (int)Mathf.Floor(discardTile / 9);

            Debug.Log("Discard Tile: " + discardTile);

            Debug.Log("Suit of Discard: " + (discardTile % 9));

            if (discardTile % 9 == 0) //This is only for shift pattern 1,2,3. Ignore i value
            {
                for (int i = 0 + 9 * multiple; i < 2 + 9 * multiple; i++)
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
                            Debug.Log("CAN CHOW: " + (1 + multiple * 9) + " - " + (2 + multiple * 9) + " - " + (3 + multiple * 9));
                        }
                    }
                }
            }
            if (discardTile % 9 == 1)
            {
                for (int i = 0 + 9 * multiple; i < 2 + 9 * multiple; i++)
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
                            Debug.Log("CAN CHOW: " + (i + 1 + multiple * 9) + " - " + (i + 2 + multiple * 9) + " - " + (i + 3 + multiple * 9));
                        }
                    }
                }
            }
            else if (discardTile % 9 > 6)
            {
                for (int i = 6 + 9 * multiple; i < 7 + 9 * multiple; i++)
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
                            Debug.Log("CAN CHOW: " + (i + 1 + multiple * 9) + " - " + (i + 2 + multiple * 9) + " - " + (i + 3 + multiple * 9));
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
                            Debug.Log("CAN CHOW: " + (i + 1 + multiple * 9) + " - " + (i + 2 + multiple * 9) + " - " + (i + 3 + multiple * 9));
                        }
                    }
                }
            }
        }
    }

    int[] ComputeHand() {
        int RightSidePlayer = GetNextPlayer();
        GameObject RightSidePlayerContainer = new GameObject();
        int[] tilesetcount = new int[34];
        switch (RightSidePlayer)
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
                Debug.Log("Invalid player number");
                break;
        }
        for (int i = 0; i < 13; i++) {
            string tileindexstring = RightSidePlayerContainer.transform.GetChild(i).gameObject.name;
            int tileindex;
            int.TryParse(tileindexstring, out tileindex);
            int temp = tileindex - 1;
            tilesetcount[temp]++;
        }
        return tilesetcount;
    }
}