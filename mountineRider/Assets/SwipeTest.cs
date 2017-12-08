using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeTest : MonoBehaviour {

    public GameObject player;

    public float maxTime;
    public float minSwipeDist;

    float startTime;
    float endTime;

  public  Vector2 startPos;
   public Vector2 endPos;

   public float swipeDistance;
    float swipeTime;

	// Update is called once per frame
	void Update () {
        
        if(Input.touchCount>0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                startTime = Time.time;
                startPos = touch.position;
            }
            //else if (touch.phase == TouchPhase.Ended)
            //{

            //    endTime = Time.time;
            //    endPos = touch.position;
            
            //    swipeDistance = (endPos - startPos).magnitude;
            //    swipeTime = endTime - startTime;

            //    //if (swipeTime < maxTime && swipeDistance > minSwipeDist)
            //    //{
            //    //    Swipe();
            //    //}

            //}
            if ((startPos-touch.position).magnitude >= minSwipeDist)
            {
                endTime = Time.time;
                endPos = touch.position;

                swipeDistance = (endPos - startPos).magnitude;
                swipeTime = endTime - startTime;
                Swipe();
            }

        }

	}

    void Swipe()
    {
        Vector2 distance = endPos - startPos;
        if (Mathf.Abs(distance.x) > Mathf.Abs(distance.y))
        {
           // Debug.Log("Horizontal Swipe");
            if (distance.x > 0)
            {
               // Debug.Log("Right Swipe");
            }
            if (distance.x < 0)
            {
               // Debug.Log("Left Swipe");
            }

        }
        else if (Mathf.Abs(distance.x) < Mathf.Abs(distance.y))
        {
           // Debug.Log("Vertical Swipe");
            if (distance.y > 0)
            {
                //  Debug.Log("Up Swipe");
                //player.GetComponent<NewPlayer>().Jump();
                player.GetComponent<NewPlayer>().Jump();
                //player.GetComponent<JumpPlayer>().Jump();
                //player.GetComponent<NewPlayer>().Rotation();



            }
            if (distance.y < 0)
            {
                Debug.Log("Down Swipe");
                //player.GetComponent<TestPlayer>().Rotation();
                player.GetComponent<NewPlayer>().Rotation();
                //player.GetComponent<JumpPlayer>().Rotation();
                //player.GetComponent<NewPlayer>().Jump();
            }
        }

    }
}
