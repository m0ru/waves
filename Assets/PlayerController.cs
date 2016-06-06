using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

/*
public class PlayerController : MonoBehaviour, IBeginDragHandler, IDragHandler {
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag");
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("OnDrag");
        this.transform.position = eventData.position;
    }

    // Use this for initialization
    void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
*/
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{

    public int normalCollisionCount = 1;
    public float moveLimit = .5f;
    public float collisionMoveFactor = .01f;
    public float addHeightWhenClicked = 0.0f;
    public bool freezeRotationOnDrag = true;
    public Camera cam;
    private Rigidbody2D myRigidbody;
    private Transform myTransform;
    private bool canMove = false;
    private float yPos;
    private bool gravitySetting;
    private bool freezeRotationSetting;
    private float sqrMoveLimit;
    private int collisionCount = 0;
    private Transform camTransform;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myTransform = transform;
        if (!cam)
        {
            cam = Camera.main;
        }
        if (!cam)
        {
            Debug.LogError("Can't find camera tagged MainCamera");
            return;
        }
        camTransform = cam.transform;
        sqrMoveLimit = moveLimit * moveLimit;   // Since we're using sqrMagnitude, which is faster than magnitude
    }

    void OnMouseDown()
    {
        canMove = true;
        myTransform.Translate(Vector3.up * addHeightWhenClicked);
        //gravitySetting = myRigidbody.useGravity;
        freezeRotationSetting = myRigidbody.freezeRotation;
       // myRigidbody.useGravity = false;
        myRigidbody.freezeRotation = freezeRotationOnDrag;
        yPos = myTransform.position.y;
    }

    void OnMouseUp()
    {
        canMove = false;
        //myRigidbody.useGravity = gravitySetting;
        myRigidbody.freezeRotation = freezeRotationSetting;
        /*if (!myRigidbody.useGravity)
        {
            Vector3 pos = myTransform.position;
            pos.y = yPos - addHeightWhenClicked;
            myTransform.position = pos;
        }
        */
    }

    void OnCollisionEnter()
    {
        collisionCount++;
    }

    void OnCollisionExit()
    {
        collisionCount--;
    }

    void FixedUpdate()
    {
        if (!canMove)
        {
            return;
        }

        myRigidbody.velocity = Vector2.zero;
        myRigidbody.angularVelocity = 0;

        Vector2 pos = myTransform.position;
        pos.y = yPos;
        myTransform.position = pos;

        Vector2 mousePos = Input.mousePosition;
        //Vector2 move = cam.ScreenToWorldPoint(new Vector2(mousePos.x, mousePos.y, camTransform.position.y - myTransform.position.y)) - myTransform.position;
        Vector2 move = cam.ScreenToWorldPoint(new Vector2(mousePos.x, camTransform.position.y - myTransform.position.y)) - myTransform.position;

        move.y = 0.0f;
        if (collisionCount > normalCollisionCount)
        {
            move = move.normalized * collisionMoveFactor;
        }
        else if (move.sqrMagnitude > sqrMoveLimit)
        {
            move = move.normalized * moveLimit;
        }

        myRigidbody.MovePosition(myRigidbody.position + move);
    }
}