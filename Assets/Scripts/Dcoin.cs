using UnityEngine;
using System.Collections;

public class Dcoin : MonoBehaviour {

    private GameManager gm;

    // Use this for initialization
    void Start () {
        if (gm == null) gm = gameObject.GetComponent<GameManager>();

        if (GameManager.gm.type == "Achiever" || GameManager.gm.type == "Killer") Destroy(gameObject);

    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
