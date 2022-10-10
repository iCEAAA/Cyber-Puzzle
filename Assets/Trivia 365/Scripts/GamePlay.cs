using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class GamePlay : MonoBehaviour
{
    internal static GamePlay instance;

    public GameObject WinPanel;
    public Text WinText;
    public Text StepText;
    public Text LevelName;
    public GameObject MapBg;
    //public Text APText;
    public GameObject GUIplayer;

    public struct pos
    {
        public int i;
        public int j;
    };

    // logical
    public static pos startPos;
    public static pos goalPos;

    public class Player
    {
        public pos position;
        public float offset;
        public int mapSize;

        public Player(int i = 2, int j = 0, int MapSize = 3, float moffset = 300.0f)
        {
            initPlayer(i, j);
            offset = moffset;
            mapSize = MapSize;
        }

        RectTransform GUIplayer = GameObject.Find("Player").transform.GetComponent<RectTransform>();
        GameObject MapBg = GameObject.Find("MapBg");

        public void initPlayer(int i, int j, int MapSize = 3, float moffset = 300.0f)
        {
            //GUIplayer = GameObject.Find("Player").transform.GetComponent<RectTransform>();
            position.i = i;
            position.j = j;
            offset = moffset;
            mapSize = MapSize;

            float posx=0.0f, posy=0.0f;
            switch(mapSize)
            {
                case 3:
                    posx = -300.0f;
                    posy = -300.0f;                      
                    break;
                case 4:
                    posx = -337.5f;
                    posy = -337.5f;
                    break;
                case 5:
                    posx = -360.0f;
                    posy = -360.0f;
                    break;
                default:
                    Debug.Log("unknown map size");
                    break;
            }
            //Debug.Log(GUIplayer.anchoredPosition);
            GUIplayer.anchoredPosition = new Vector2(posx, posy);
        }

        public void movePlayer(int i, int j)
        {
            //3*3 300
            //4*4 225
            //5*5 180
            position.i += i;
            position.j += j;

            if (checkOverflow(mapSize)) // make sure move to valid place
            {
                // GUI move
                float posx = GUIplayer.anchoredPosition.x;
                float posy = GUIplayer.anchoredPosition.y;
                posx += offset * j;
                posy += -offset * i;
                GUIplayer.anchoredPosition = new Vector2(posx, posy);
                // need animation
            } 
            else
            {
                position.i -= i;
                position.j -= j;
            }

        }

        public bool checkBlock()
        {
            int idx = position.i * mapSize + position.j;
            if (MapBg.transform.GetChild(idx).GetComponent<Image>().color == Color.grey) return true;
            return false;
        }

        public bool checkOverflow(int mapSize)
        {
            if (position.i > mapSize - 1 || position.i < 0 || position.j > mapSize - 1 || position.j < 0) return false;
            if (checkBlock()) return false;
            return true;
        }

    };

    public static Player player;

    public static int AP = 0;
    public static Text APtext;

    public static void updateAP(int APdelta)
    {
        GamePlay.AP += APdelta;
        //GamePlay.AP = GamePlay.AP < 0 ? 0 : GamePlay.AP; // never less than 0;
        GamePlay.AP = GamePlay.AP > 15 ? 15 : GamePlay.AP;

        
        APtext.text = string.Concat("  Active Point: ", GamePlay.AP.ToString(),
            GamePlay.AP >= 15 ? " (MAX)" : "");
        
    }

    public static int Step = 0;
    

    

    // type 0 CW 顺, type 1 CCW 逆, rotate center node
    public void rotate(int CWtype, int nodei, int nodej, int mapSize=3)
    {
        bool type = CWtype == 1;

        // rotate obstacle
        int idx = nodei * mapSize + nodej;
        Transform b1 = MapBg.transform.GetChild(idx);
        Transform b2 = MapBg.transform.GetChild(idx + 1);
        Transform b3 = MapBg.transform.GetChild(idx + mapSize);
        Transform b4 = MapBg.transform.GetChild(idx + mapSize + 1);

        Color temp = b1.GetComponent<Image>().color;
        if (type)
        {
            b1.GetComponent<Image>().color = b2.GetComponent<Image>().color;
            b2.GetComponent<Image>().color = b4.GetComponent<Image>().color;
            b4.GetComponent<Image>().color = b3.GetComponent<Image>().color;
            b3.GetComponent<Image>().color = temp;
        } else
        {
            b1.GetComponent<Image>().color = b3.GetComponent<Image>().color;
            b3.GetComponent<Image>().color = b4.GetComponent<Image>().color;
            b4.GetComponent<Image>().color = b2.GetComponent<Image>().color;
            b2.GetComponent<Image>().color = temp;
        }

        // rotate player
        if (nodei == player.position.i)
        {
            if(nodej == player.position.j)
            {
                // 左上
                if (type) player.movePlayer(1, 0); // 下
                else player.movePlayer(0, 1); // 右
            }
            else if(nodej + 1 == player.position.j)
            {
                // 右上
                if (type) player.movePlayer(0, -1); // 左
                else player.movePlayer(1, 0); // 下
            }
        } 
        else if (nodei + 1 == player.position.i)
        {
            if (nodej == player.position.j)
            {
                // 左下
                if (type) player.movePlayer(0, 1); // 右
                else player.movePlayer(-1, 0); // 上
            }
            else if(nodej + 1 == player.position.j)
            {
                // 右下
                if (type) player.movePlayer(-1, 0); // 上
                else player.movePlayer(0, -1); // 左
            }
        }
    }

    public void checkWin()
    {
        // check win
        if (player.position.i == goalPos.i && player.position.j == goalPos.j)
        {
            WinPanel.SetActive(true);
            int finalScore = Controller.puzzleScore + 100 * AP - 100 * Step;
            WinText.text = string.Concat(
                "Complete!",
                "\nYour Step: ", Step, " x(-100.0)",
                "\nYour Remain AP: ", AP, " x(100.0)",
                "\nYour QuizScore: ", Controller.puzzleScore,
                "\n-------------------------------",
                "\nYour Final Score: ", finalScore);
        }
  
    }

    public void invokeCardRotate(GameObject card)
    {
        int cost = int.Parse(card.transform.Find("Cost").GetComponent<Text>().text);
        string level = LevelName.text;
        if (AP >= cost)
        {
            updateAP(cost*-1);

            int CWtype = card.name == "CWCard" ? 0 : 1; // 0顺 1逆
            string nodeText = card.transform.Find("Node").GetComponent<Text>().text;

            if(level == "Level 1")
            {
                if(nodeText == "A") rotate(CWtype, 0, 0,3);
                else rotate(CWtype, 0, 1,3);
            } 
            else if( level == "Level 2")
            {
                if(nodeText == "A") rotate(CWtype, 0, 1,4);
                else if(nodeText=="B") rotate(CWtype, 1, 0,4);
                else if (nodeText == "C") rotate(CWtype, 1, 2,4);
                else rotate(CWtype, 2, 1,4);
            }
            else if (level == "Level 3")
            {
                if (nodeText == "A") rotate(CWtype, 1, 1,5);
                else if (nodeText == "B") rotate(CWtype, 1, 3,5);
                else if (nodeText == "C") rotate(CWtype, 2, 0,5);
                else rotate(CWtype, 2, 2,5);
            }
            else
            {
                Debug.Log("undefined node");
            }


            // update step
            Step += 1;
            StepText.text = string.Concat("Step: ", Step);

        }
        //else
        //{
        //    // not enough AP for card, lead to minus AP.
        //    updateAP(cost * -1);
        //}

        checkWin();
    }

    public void invokeCard(GameObject card)
    {
        int cost = int.Parse(card.transform.Find("Cost").GetComponent<Text>().text);
        if(AP >= cost)
        {
            updateAP(cost * -1);
            if (card.name == "UpCard")
            {
                player.movePlayer(-1, 0);
            }
            else if (card.name == "DownCard")
            {
                player.movePlayer(1, 0);
            }
            else if (card.name == "LeftCard")
            {
                player.movePlayer(0, -1);
            }
            else if (card.name == "RightCard")
            {
                player.movePlayer(0, 1);
            }

            // update step
            Step += 1;
            StepText.text = string.Concat("Step: ", Step);

        }
        //else
        //{
        //    // not enough AP for card, lead to minus AP.
        //    updateAP(cost * -1);
        //}

        checkWin();
    }

    //public static void init(int mapSize)
    //{
    //    float[] offsets = { 300, 225, 180 };

    //}

    // Start is called before the first frame update
    public void Awake()
    {
        
        // AP init
        AP = 0;
        APtext = GameObject.Find("Active Text").GetComponent<Text>();

        // Step init
        Step = 0;

        // position init
        startPos.i = 2;
        startPos.j = 0;
        goalPos.i = 0;
        goalPos.j = 2;
        player = new Player(2, 0, 3, 300.0f);
        player.initPlayer(startPos.i, startPos.j);

        // win panel init
        WinPanel.SetActive(false);

        //// read from Card Scroll game object
        //actionQueue = new Queue();
        //actionQueue.Enqueue(CARD.RIGHT);
        //actionQueue.Enqueue(CARD.RIGHT);
        //actionQueue.Enqueue(CARD.UP);
        //actionQueue.Enqueue(CARD.UP);

        //string currentCard = (string)actionQueue.Dequeue();
        //Debug.Log(currentCard);
        //Debug.Log(actionQueue.Count);
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void Reset()
    {
        // Reset all state
    }






    //public static Queue actionQueue;

    //public static int[,] gameMap;

    //public void InitMap()
    //{
    //    gameMap = new int[3, 3];
    //    for(int i=0; i<3; ++i)
    //    {
    //        for(int j=0; j<3; ++j)
    //        {
    //            gameMap[i,j] = 0;
    //        }
    //    }

    //    // set start
    //    gameMap[2, 0] = 1;

    //    // set goal
    //    gameMap[0, 2] = 2;
    //}

    //enum CARD
    //{
    //    UP, DOWN, LEFT, RIGHT, CW, CCW
    //};

    //public void TestDebug()
    //{
    //    Debug.Log("123132");
    //}

    //public bool isMoving = false;

    //IEnumerator imoveplayer(int sec, int i, int j)
    //{
    //    yield return new WaitForSeconds(sec);
    //    player.movePlayer(i, j);
    //}

    //public void ActionRun()
    //{
        
    //    // read queue and move player
    //    Debug.Log("read queue and move player");

    //    // use clone to save state
    //    Queue tempQueue = (Queue)actionQueue.Clone();

    //    int count = 0; // step
    //    while (tempQueue.Count!=0)
    //    {
    //        // dequeue
    //        GamePlay.CARD currentCard = (GamePlay.CARD)tempQueue.Dequeue();
    //        //Invoke("TestDebug", 2);

    //        // different cases
    //        switch (currentCard)
    //        {
    //            case CARD.UP:
    //                StartCoroutine(imoveplayer(count,-1,0));

    //                break;
    //            case CARD.DOWN:
    //                StartCoroutine(imoveplayer(count,1,0));

    //                break;
    //            case CARD.LEFT:
    //                StartCoroutine(imoveplayer(count,0,-1));

    //                break;
    //            case CARD.RIGHT:
    //                StartCoroutine(imoveplayer(count,0,1));

    //                break;
    //            case CARD.CW:
    //                Debug.Log("ClockWise card");
    //                break;

    //            case CARD.CCW:
    //                Debug.Log("CounterClockWise card");
    //                break;

    //            default:
    //                Debug.Log("unknown card");
    //                break;
    //        }
    //        count += 1;

    //        // do more, add animation, check condition

    //    }


    //    // if not win, reset state

    //}

    //public void buyCard(GameObject go)
    //{
    //    Debug.Log(go.name);
    //    // buy card when card in ShopBG, add to queue.
    //    // Instantiate card prefab
    //    string path = "Prefabs/Cards/" + go.name;
    //    GameObject prefab = (GameObject)Resources.Load(path);
    //    prefab = Instantiate(prefab);
    //    GameObject cbg = GameObject.Find("CardBg");
    //    prefab.transform.SetParent(cbg.transform, false);

    //    // resize CardBg width
    //    RectTransform rt = cbg.transform.GetComponent<RectTransform>();
    //    int childCounts = gameObject.transform.childCount;
    //    rt.sizeDelta = new Vector2(20 + childCounts * 220, 300); // width, height

    //    // add to queue
    //    //(CARD)Enum.Parse(typeof(CARD), go.name);
    //}
}
