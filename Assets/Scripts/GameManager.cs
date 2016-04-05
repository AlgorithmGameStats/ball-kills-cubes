using UnityEngine;
using System.Collections;
using UnityEngine.UI;

using System.Net;
using System.IO;

public class GameManager : MonoBehaviour {

	public static GameManager gm;

	[Tooltip("If not set, the player will default to the gameObject tagged as Player.")]
	public GameObject player;

	public enum gameStates {Playing, Death, GameOver, BeatLevel};
	public gameStates gameState = gameStates.Playing;

	public int score=0;
	public bool canBeatLevel = false;
	public int beatLevelScore=0;
    public int timeLeft = 120;
    public int timeRefresh =20;

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

	private Health playerHealth;
    private bool BLCoin = false;
    private int killsCn = 0;

	void Start () {
		if (gm == null) 
			gm = gameObject.GetComponent<GameManager>();

		if (player == null) {
			player = GameObject.FindWithTag("Player");
		}

		playerHealth = player.GetComponent<Health>();

		// setup score display
		Collect (0);

        BLCoin = false;
        killsCn = 0;

		// make other UI inactive
		gameOverCanvas.SetActive (false);
		if (canBeatLevel)
			beatLevelCanvas.SetActive (false);
	}

	void Update () {
        switch (gameState)
		{
			case gameStates.Playing:
                timeRefresh -= 1;
                if (timeRefresh == 0) {
                    timeRefresh = 20;timeLeft -= 1;
                    mainScoreDisplay.text = score.ToString() +"|"+ timeLeft.ToString();
                }
                if (playerHealth.isAlive == false || timeLeft==0)
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
				backgroundMusic.volume -= 0.01f;
				if (backgroundMusic.volume<=0.0f) {
					AudioSource.PlayClipAtPoint (gameOverSFX,gameObject.transform.position);

					gameState = gameStates.GameOver;
				}
				break;
			case gameStates.BeatLevel:
				backgroundMusic.volume -= 0.01f;
				if (backgroundMusic.volume<=0.0f) {
					AudioSource.PlayClipAtPoint (beatLevelSFX,gameObject.transform.position);
					
					gameState = gameStates.GameOver;
				}
				break;
			case gameStates.GameOver:
                // nothing
                //sendStats(score);
                Debug.Log("Coin collected=" + score);
                Debug.Log("enimy killed=" + killsCn);
                Debug.Log("time left="+timeLeft);
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

    public static void sendStats(int coins)
    {
        string GameServerURL = "http://172.31.231.177:5000/api/1.0/stats";
        string user = "game";
        string pass = "matrix";
        var httpWebRequest = (HttpWebRequest)WebRequest.Create(GameServerURL);
        httpWebRequest.ContentType = "application/json";
        httpWebRequest.Method = "POST";
        string _auth = string.Format("{0}:{1}", user, pass);
        string _enc = System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(_auth));
        httpWebRequest.Headers[HttpRequestHeader.Authorization] = "Basic Z2FtZTptYXRyaXg=";

        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        {
            string json = "{\"level\":1, \"time\":13.5, \"enemies\":3, \"collected_coins\":" + coins+ ", \"max_coins\":10}";
            streamWriter.Write(json);
        }

        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        {
            var result = streamReader.ReadToEnd();
        }
    }
}
