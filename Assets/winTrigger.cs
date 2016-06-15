using UnityEngine;
using System.Collections;

public class winTrigger : MonoBehaviour {

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("You won!!!");
        
    }

    void OnTriggerStay2D(Collider2D other)
    {
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
