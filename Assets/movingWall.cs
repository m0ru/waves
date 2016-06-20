using UnityEngine;
using System.Collections;

public class movingWall : MonoBehaviour {

    Vector3 startPosition;
    Vector3 endPosition;
    Vector3 speed;

    public float horizontalSpeed = 0.05f;
    public float verticalSpeed = 0f;


    // Use this for initialization
    void Start () {
        startPosition = this.transform.position;
        endPosition = startPosition + new Vector3(6f, 0f, 0f);
        speed = new Vector3(0.05f, 0f, 0f);
	}

    // Update is called once per frame
    void Update() {

        this.transform.position += speed;

        //this one goes in x direction
        if ((endPosition.x < this.transform.position.x) || (startPosition.x > this.transform.position.x))
        {
            speed = -speed;
        }

	}
}
