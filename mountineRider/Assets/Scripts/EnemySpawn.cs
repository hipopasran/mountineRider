using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour {

    public float MinEnemyX = -3f;
    public float MaxEnemyX = +3f;
    public GameObject Enemy;

    public float num;

    public float delay;
    public float timeDelay = 1f;

    // Use this for initialization
    void Start () {
        delay = 0;
    }
    void FixedUpdate()
    {
      
    }
	
	// Update is called once per frame
	void Update () {
        delay = delay - Time.deltaTime;
        if (delay <= 0)
        {
            num = Random.Range(MinEnemyX, MaxEnemyX);

            Transform.Instantiate(Enemy, new Vector3(num, gameObject.transform.position.y), transform.rotation);
            delay = timeDelay;
        }
       
    }
}
