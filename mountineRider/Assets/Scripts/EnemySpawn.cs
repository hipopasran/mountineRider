using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour {

    public float MinEnemyX = -3f;
    public float MaxEnemyX = +3f;
    public float MinEnemyScale = 10;
    public float MaxEnemyScale = 17;

    
    public GameObject Enemy;

    private GameObject enemyScale;

    public float num;
    public float scale;

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
            enemyScale = Enemy;
            scale = (Random.Range(MinEnemyScale, MaxEnemyScale))/100;
            Debug.Log(scale);
            enemyScale.transform.localScale = new Vector3(scale,scale,1);
            num = Random.Range(MinEnemyX, MaxEnemyX);

            Transform.Instantiate(enemyScale, new Vector3(num, gameObject.transform.position.y), transform.rotation);
            delay = timeDelay;
        }
       
    }
}
