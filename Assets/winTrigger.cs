using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class winTrigger : MonoBehaviour {

    List<Collider2D> particlesIn;

    static int currentLevel = 0;
    const int NR_OF_LEVELS = 3;

    void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("You won!!!");

    }

    void OnTriggerStay2D(Collider2D other)
    {

        GameObject wc = GameObject.FindGameObjectWithTag("WinCube");
        BoxCollider2D win = wc.GetComponent<BoxCollider2D>();


        if(other.gameObject.tag == "Pushable")
        {
            GameObject[] pushables = GameObject.FindGameObjectsWithTag("Pushable");
            if(pushables.Length == 1)
            {
                Debug.Log("switching from: " + currentLevel);
                currentLevel = (currentLevel + 1) % NR_OF_LEVELS;
                Debug.Log("switching to: " + currentLevel);
                SceneManager.LoadScene("Level" + currentLevel);
            } else {
                Destroy(other.gameObject);
            }
	
        }


        /*
        float rightWin = win.bounds.center.x - win.bounds.extents.x;
        float leftWin = win.bounds.center.x + win.bounds.extents.x;
        float topWin = win.bounds.center.y + win.bounds.extents.y;
        float bottomWin = win.bounds.center.y - win.bounds.extents.y;


        float rightOther = other.bounds.center.x - other.bounds.extents.x;
        float leftOther = other.bounds.center.x + other.bounds.extents.x;
        float topOther = other.bounds.center.y + other.bounds.extents.y;
        float bottompOther = other.bounds.center.y - other.bounds.extents.y;

        if ((rightWin < rightOther) &&
            (topWin > topOther) &&
            (leftWin > leftOther) &&
            (bottomWin < bottompOther))
        {
            Debug.Log("You are in!! ");

            if (particlesIn.IndexOf(other) == -1)
                particlesIn.Add(other);
        }

        GameObject[] player = GameObject.FindGameObjectsWithTag("Pushable");


        if (particlesIn.Count == player.Length) {
            currentLevel = (currentLevel + 1) % NR_OF_LEVELS;
            SceneManager.LoadScene("Level" + currentLevel);
        }
        */
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (particlesIn.IndexOf(other) > -1)
        {
            particlesIn.Remove(other);
        }

    }

    // Use this for initialization
    void Start () {
        particlesIn = new List<Collider2D>();
        currentLevel = int.Parse("" + SceneManager.GetActiveScene().name[5]); //works till Level9
        Debug.Log("starting with level: " + currentLevel);
    }

	// Update is called once per frame
	void Update () {
        // WIN CONDITION
	}
}
