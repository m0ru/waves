using UnityEngine;
using System.Collections;

public class movingWall : MonoBehaviour {

    Vector3 startPosition;
    Vector3 endPosition;
    Vector3 speed;

    //public float horizontalSpeed = 0.05f;
    //public float verticalSpeed = 0f;

    public float endOffsetX = 4; //the delta to move in x direction
    public float endOffsetY = 0; // the delta to move in y direction
    public float oneWayDuration = 2; // in seconds


    // Use this for initialization
    void Start () {
        startPosition = this.transform.position;
        endPosition = startPosition + new Vector3(endOffsetX, endOffsetY, 0f);
        speed = new Vector3(endOffsetX / oneWayDuration, endOffsetY / oneWayDuration, 0f);
	}

    // Update is called once per frame
    void Update() {

        this.transform.position += speed * Time.deltaTime;

        //this one goes in x direction
        if ((endPosition.x < this.transform.position.x) || (startPosition.x > this.transform.position.x))
        {
            speed = -speed;
        }

	}
}
