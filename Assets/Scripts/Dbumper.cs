using UnityEngine;
using System.Collections;

public class Dbumper : MonoBehaviour {

    private GameManager gm;

	// Use this for initialization
	void Start () {
        if (gm == null) gm = gameObject.GetComponent<GameManager>();
	}
	
	// Update is called once per frame
	void Update () {
        if (GameManager.gm.type == "Achiever" || GameManager.gm.type == "Killer")
        {
            Destroy(gameObject);
        }
	}
}
