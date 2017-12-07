using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour {

    public GameObject groundEnemy;
    public GameObject flyEnemy;
    public GameObject StartCoin;
    public GameObject coins;
    public GameObject StartBonus;
    public GameObject BootsBonus;
    public int enemyType;
    public float delay;
    public float timeDelay;

    public GameObject boots;
    //public NewPlayer player;
    public int prep;
    public int num;

    public GameObject[] arr;

    public int NumberOfEnemy=0;
    public int NumberForBonus=0;

    // Use this for initialization
    void Start () {
        delay = 0;
        groundEnemy.SetActive(false);
        flyEnemy.SetActive(false);



    }

    // Update is called once per frame
    void Update () {

        
        
        delay = delay - Time.deltaTime;
        if (delay <= 0)
        {
            enemyType = Random.Range(1, 10);
            if (enemyType < 3)
            {
                groundEnemy.SetActive(false);
                flyEnemy.SetActive(true);
                
                num = Random.Range(0, 1);

                
                Transform.Instantiate(arr[3], flyEnemy.transform.position, transform.rotation);
                NumberOfEnemy = NumberOfEnemy + 1;
                if (!BootsBonus.activeInHierarchy)
                {
                    NumberForBonus = NumberForBonus + 1;
                }
            }
            if (enemyType >= 3)
            {
                groundEnemy.SetActive(true);

                //int num = Random.Range(0, 3);
                //Debug.Log(num);
                prep = Random.Range(1, 4);
                if(prep==1)
                {
                    int type = Random.Range(0, 2);
                    Debug.Log(type);
                    Transform.Instantiate(arr[type], groundEnemy.transform.position, transform.rotation);
                }

                if (prep == 2)
                {
                    num = Random.Range(0,2);
                    if(num==0)
                    {
                        for (int t = 0; t < 2; t++)
                        {
                            int type = Random.Range(0, 2);
                            Transform.Instantiate(arr[type], groundEnemy.transform.position, transform.rotation);
                        }
                    }
                    if(num==1)
                    {
                        Transform.Instantiate(arr[2], groundEnemy.transform.position, transform.rotation);
                    }
                }
                if(prep==3)
                {
                    num = Random.Range(0, 3);
                    if(num==0)
                    {
                        for (int t = 0; t < 1; t++)
                        {
                            int type = Random.Range(0, 2);
                            Transform.Instantiate(arr[type], groundEnemy.transform.position, transform.rotation);
                        }
                        Transform.Instantiate(arr[2], groundEnemy.transform.position, transform.rotation);
                    }
                    if(num==1)
                    {
                        for (int t = 0; t < 3; t++)
                        {
                            int type = Random.Range(0, 2);
                            
                            Transform.Instantiate(arr[type], groundEnemy.transform.position, transform.rotation);
                        }
                    }
                    if(num==2)
                    {
                        Transform.Instantiate(arr[2], groundEnemy.transform.position, transform.rotation);
                        for (int t = 0; t < 1; t++)
                        {
                            int type = Random.Range(0, 2);
                            Transform.Instantiate(arr[type], groundEnemy.transform.position, transform.rotation);
                        }
                    }
                }
                NumberOfEnemy = NumberOfEnemy + 1;
            }

            timeDelay = Random.Range(2, 3);
            //timeDelay = Random.Range(1.2f, 1.5f);

            delay = timeDelay;
           
        }

        if(NumberForBonus >=4 && (flyEnemy.activeInHierarchy && !boots.activeInHierarchy))
        {
            Transform.Instantiate(BootsBonus, StartBonus.transform.position, transform.rotation);
            flyEnemy.SetActive(false);
            NumberForBonus = 0;
        }
       

        if(NumberOfEnemy >=4 && groundEnemy.activeInHierarchy)
        {
            Transform.Instantiate(coins, StartCoin.transform.position, transform.rotation);
            groundEnemy.SetActive(false);
            //StartCoin.SetActive(true);
            NumberOfEnemy = 0;
        }
        else if(NumberOfEnemy < 4)
        {
            StartCoin.SetActive(false);
        }
        if (gameObject.activeInHierarchy == false)
        {
            delay = 0;
        }

    }
}
