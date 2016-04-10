using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using System.Net;
using System.IO;
using System;

public class GameManager : MonoBehaviour {

	public static GameManager gm;
    public static string playerType;
    public string type;

    [Tooltip("If not set, the player will default to the gameObject tagged as Player.")]
	public GameObject player;

    public enum gameStates {Playing, Death, GameOver, BeatLevel};
	public gameStates gameState = gameStates.Playing;

	public int score=0;
	public bool canBeatLevel = false;
    public static int timeInit = 120;
    public int timeLeft = timeInit;
    public int timeRefresh =20;

    public GameObject mainCamera;
    public GameObject staticCanvas;
    public Text staticCanvasDisplay;
	public GameObject mainCanvas;
	public Text mainScoreDisplay;
	public GameObject gameOverCanvas;
	public Text gameOverScoreDisplay;
	[Tooltip("Only need to set if canBeatLevel is set to true.")]
	public GameObject beatLevelCanvas;

	public AudioSource backgroundMusic;
	public AudioClip gameOverSFX;
	[Tooltip("Only need to set if canBeatLevel is set to true.")]
	public AudioClip beatLevelSFX;

	public bool playerlive =true;
    private bool BLCoin = false;
    private int ct = 0;//total coins in the scene
    private int killsCn = 0;
    private int kt = 0;//total enimiesin the scene

    void Start () {
        if (gm == null) gm = gameObject.GetComponent<GameManager>();

        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
        }

        type = playerType;
        //dynamically adjust
        if (type == "Achiever")
        {
            timeInit = 100;
            timeLeft = 100;
            player.GetComponent<Controller>().moveSpeed = 30;
        }

        // make other UI inactive
        mainCanvas.SetActive(true);
        gameOverCanvas.SetActive(false);
        staticCanvas.SetActive(true);
        staticCanvasDisplay.text = Application.loadedLevelName + "|" + playerType;
        if (canBeatLevel)
            beatLevelCanvas.SetActive(false);
    }

    void Update () {
        if (ct == 0 || kt == 0)
        {
            this.ct = GameObject.FindGameObjectsWithTag("pickup").Length;
            this.kt = GameObject.FindGameObjectsWithTag("enimy").Length;
            Debug.Log("ct=" + ct + "kt=" + kt);
        }

        switch (gameState)
		{
			case gameStates.Playing:
                timeRefresh -= 1;
                if (timeRefresh == 0) {
                    timeRefresh = 20;timeLeft -= 1;
                    mainScoreDisplay.text = score.ToString() +"|"+ timeLeft.ToString();
                }
                if (playerlive == false || timeLeft==0)
				{
                    // update gameState
                    gameState = gameStates.Death;
					// set the end game score
					gameOverScoreDisplay.text = mainScoreDisplay.text;

					// switch which GUI is showing		
					mainCanvas.SetActive (false);
					gameOverCanvas.SetActive (true);
				} else if (canBeatLevel && BLCoin==true) {
					// update gameState
					gameState = gameStates.BeatLevel;

                    // hide the player so game doesn't continue playing
                    player.SetActive(false);
					// switch which GUI is showing			
					mainCanvas.SetActive (false);
					beatLevelCanvas.SetActive (true);
				}
				break;
			case gameStates.Death:
                backgroundMusic.volume -= 0.1f;
				if (backgroundMusic.volume<=0.0f) {
					AudioSource.PlayClipAtPoint (gameOverSFX,gameObject.transform.position);
                    playerType = sendStats(score,ct, killsCn,kt, (timeInit - timeLeft),timeInit);
                    gameState = gameStates.GameOver; 
                }
				break;
			case gameStates.BeatLevel:
                backgroundMusic.volume -= 0.1f;
				if (backgroundMusic.volume<=0.0f) {
					AudioSource.PlayClipAtPoint (beatLevelSFX,gameObject.transform.position);
                    playerType = sendStats(score, ct, killsCn, kt, (timeInit - timeLeft), timeInit);
                    gameState = gameStates.GameOver;
				}
				break;
			case gameStates.GameOver:
                
                break;
		}

	}

	public void Collect(int amount) {
		score += amount;
		if (canBeatLevel) {
            mainScoreDisplay.text =score.ToString() +"|"+ timeLeft.ToString();
        } else {
			mainScoreDisplay.text = score.ToString ();
		}
	}

    public void BLCollect()
    {
        if (canBeatLevel)
        {
            BLCoin = true;
        }
        
    }

    public void kills(int value)
    {
        killsCn += value;
    }


    [System.Serializable]
    public class GameMatrix
    {
        public int level;
        public int time_used;
        public int time_total;
        public int enemies_killed;
        public int enemies_total;
        public int coins_collected;
        public int coins_total;
    }
    [System.Serializable]
    public class Result
    {
        public string class_name;
        public float class_score;
    }
    public static string sendStats(int coins,int coinsT,int killsCn,int killsT,int timeUsed,int timeT)
    {
        string GameServerURL = "http://game.itomaldonado.com/api/1.0/stats";
        string user = "game";
        string pass = "matrix";
        var httpWebRequest = (HttpWebRequest)WebRequest.Create(GameServerURL);
        httpWebRequest.ContentType = "application/json";
        httpWebRequest.Method = "POST";
        string _auth = string.Format("{0}:{1}", user, pass);
        string _enc = System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(_auth));
        httpWebRequest.Headers[HttpRequestHeader.Authorization] = "Basic "+ _enc;

        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        {
            GameMatrix gma = new GameMatrix(); ;
            string a = Application.loadedLevelName; 
            gma.level = a[5];
            gma.time_used = timeUsed;
            gma.time_total = timeInit;
            gma.enemies_killed = killsCn;
            gma.enemies_total = killsT;
            gma.coins_collected = coins;
            gma.coins_total = coinsT;

            string json = JsonUtility.ToJson(gma);
            //string json = "{\"level\":"+1+", \"time\":"+(timeInit-timeLeft)+", \"enemies\":"+ killsCn+", \"collected_coins\":" + coins+ ", \"max_coins\":10}";
            streamWriter.Write(json);
        }

        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        {
            var result = streamReader.ReadToEnd();
            Result r= JsonUtility.FromJson<Result>(result);Debug.Log("result" + result);
            r.class_name= char.ToUpper(r.class_name[0]) + r.class_name.Substring(1); ;
            return r.class_name;
        }
    }
}
