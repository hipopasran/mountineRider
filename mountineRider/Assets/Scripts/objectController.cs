using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class objectController : MonoBehaviour {


    Rigidbody2D enemy;
    public float speed = 3f;
    public Transform EndLevel;
    

	// Use this for initialization
	void Start () {
        enemy = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update () {
        Vector2 vel = enemy.velocity;
        vel.y = 1 * speed;
        enemy.velocity = vel;
        
       
		
	}
    void OnCollisionEnter2D(Collision2D coll)
    {

        if (coll.transform.tag == "endplat")
        {

            Destroy(gameObject);
        }



    }
}
