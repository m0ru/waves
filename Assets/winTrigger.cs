using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class winTrigger : MonoBehaviour {

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("You won!!!");

    }

    void OnTriggerStay2D(Collider2D other)
    {

        GameObject wc = GameObject.FindGameObjectWithTag("WinCube");
        BoxCollider2D win = wc.GetComponent<BoxCollider2D>();

        float rightWin = win.bounds.center.x - win.bounds.extents.x;
        float rightOther = other.bounds.center.x - other.bounds.extents.x;
        float leftWin = win.bounds.center.x + win.bounds.extents.x;
        float leftOther = other.bounds.center.x + other.bounds.extents.x;

        float topOther = other.bounds.center.y + other.bounds.extents.y;
        float topWin = win.bounds.center.y + win.bounds.extents.y;
        float bottompOther = other.bounds.center.y - other.bounds.extents.y;
        float bottomWin = win.bounds.center.y - win.bounds.extents.y;
        
        if ((rightWin < rightOther) &&
            (topWin > topOther) &&
            (leftWin > leftOther) &&
            (bottomWin < bottompOther)) {

            Debug.Log("You are in!!");


            SceneManager.LoadScene("Level2");

        } 
    }

    void OnTriggerExit2D(Collider2D other)
    {
    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
