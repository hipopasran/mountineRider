using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.SocialPlatforms;

using UnityEngine.Advertisements;

public class NewPlayer : MonoBehaviour
{

    Rigidbody2D rd;
    public float speed = 7f;
    public int coins;
    public Text text;

    public float gameScale;
    public int pointLevel=0;

    public float gravity = -9.8f;

    public Text score_death;

    public Transform telo;

    public bool IsGround;
    Transform grounded;
    public LayerMask layerMask;

    public int points = 0;
    
    public Text pointText;

    public GameObject restart, home, ratimg, share,pause;
    Transform tr;
   
    private static int adsCount=0;

    public GameObject gameover;
    public GameObject coinsSound;
    public GameObject bonusSound;
    public GameObject theme;

    public int animType = 2;
    public Animator anim;

    public GameObject activeBoots;
    

    public GameObject Death;
    public GameObject EnemtCntrl;

    public GameObject boots;
    public GameObject BootsIcon;
    // Use this for initialization
    void Start()
    {
       // PlayGamesPlatform.Activate();
        anim = GetComponent<Animator>();

        //if (Advertisement.isSupported)
        //{
        //    Advertisement.Initialize("1566118", false);
        //}

        
       


        Social.ReportProgress("CgkIv-vamLwREAIQAw", 100.0f, (bool success) => {
            // Удачно или нет?
        });


        coins = PlayerPrefs.GetInt("Coins");
        rd = GetComponent<Rigidbody2D>();
        Time.timeScale = 1.3f;

        pointLevel = points;

        tr = GetComponent<Transform>();

        tr.Rotate(Vector3.zero);

        grounded = GameObject.Find(this.name + "/grounded").transform;

        PlayerPrefs.SetFloat("GameScale", Time.timeScale);

        Debug.Log(Time.timeScale);

    }

    // Update is called once per frame
    void Update()
    {



        IsGround = Physics2D.Linecast(transform.position, grounded.position, layerMask);

        pointText.text = (points/10).ToString();
        text.text =" "+ coins.ToString();

        //if(boots.activeInHierarchy)
        //{
        //    BootsLifetime();
        //}

        if (PlayerPrefs.GetString("AnimType") != "")
        {
            anim.Play(PlayerPrefs.GetString("AnimType"));
        }
        else
        {
            anim.Play("Knight");
        }
        //if (animType == 1)
        //{
        //    anim.Play("run");
        //}
        //else if (animType == 2)
        //{
        //    anim.Play("doctor");
        //}
        //else
        //{
        //    anim.Play("sherif");
        //}

        //if(tr.transform.position.y==jumpHight.position.y)
        //{
        //    rd.velocity = 100 * gravity * Vector2.up;
        //}

        if (rd.velocity.y<0 && IsGround == false)
        {
            //rd.velocity =  gravity * Vector2.up;
           // rd.velocity = gravity * Vector2.up;
            rd.gravityScale = 1.2f;
            //Time.timeScale = 2;
        }
        else
        {
            rd.gravityScale = 1f;
        }
        //Time.timeScale = 1f;
        if (Time.timeScale != 0)
        {
            if (points < 2500)
            {
                points = points + 3;
            }
            else
            {
                points = points + 2;
            }
        }
        if ((pointLevel + 100) <= points && Time.timeScale != 0)
        {
            pointLevel = points;
            GrowScale();

        }
        //if(Time.timeScale!=0)
        //{
        //    GrowScale();
        //}
        //StartCoroutine(FixedUpdate());
        if (points/10==100)
        {
            Social.ReportProgress("CgkIv-vamLwREAIQAg", 100.0f, (bool success) => {
                // Удачно или нет?
            });

        }
        if(points/10==50)
        {
            Social.ReportProgress("CgkIv-vamLwREAIQBA", 100.0f, (bool success) => {
                // Удачно или нет?
            });
        }

    }
    //public IEnumerator FixedUpdate()
    //{
    //    if (Time.timeScale > 0)
    //    {

    //        points = points + 1;

    //        yield return new WaitForSeconds(5f);

    //    }
    //}

    //void OnMouseDown()
    //{
    //    if (IsGround && Time.timeScale != 0)
    //    {
    //        Jump();
    //    }

    //}
    public void GrowScale()
    {
        //if (points < 100)
        //{
        //    Time.timeScale = Time.timeScale + 0.02f;
        //    gameScale = Time.timeScale;
        //    PlayerPrefs.SetFloat("GameScale", Time.timeScale);
        //    Debug.Log(Time.timeScale);
        //}
        //if(points>=1500)
        //{
        //    Time.timeScale = Time.timeScale + 0.01f;
        //    gameScale = Time.timeScale;
        //    PlayerPrefs.SetFloat("GameScale", Time.timeScale);
        //    Debug.Log(Time.timeScale);
        //}

        Time.timeScale = 1.2f + (Mathf.Sqrt(points/10)) / 100;
        gameScale = Time.timeScale;
        PlayerPrefs.SetFloat("GameScale", Time.timeScale);
        Debug.Log(Time.timeScale);
    }

    public void Jump()
    {
        if (IsGround && Time.timeScale != 0)
        {
            AudioSource audio = GetComponent<AudioSource>();
            audio.Play();

            rd.velocity = speed * Vector2.up;
        }
    }
    public void Rotation()
    {


        StartCoroutine(Rot());

    }



    void OnCollisionEnter2D(Collision2D coll)
    {

        if (coll.transform.tag == "enemy")
        {
            boots.SetActive(false);
            EnemtCntrl.SetActive(false);
            if (GameObject.FindGameObjectsWithTag("Audio").Length > 0)
            {
              Destroy(GameObject.Find("mainMusic"));
            }

            AudioSource themeMusic = theme.GetComponent<AudioSource>();
            themeMusic.Stop();

            AudioSource RIPaudio = gameover.GetComponent<AudioSource>();
            RIPaudio.Play();

            PlayerLose();
            score_death.text = "YOURE SCORE: " + (points/10).ToString();
            score_death.gameObject.SetActive(true);
            //Time.timeScale = 0;
            ratimg.SetActive(true);
            share.SetActive(true);
            restart.SetActive(true);
            home.SetActive(true);
            pause.SetActive(false);
            
            Death.transform.position = gameObject.transform.position;
            Destroy(gameObject);
            Death.SetActive(true);
            pointText.text = "";


            //AudioSource RIPaudio = gameover.GetComponent<AudioSource>();
            //RIPaudio.Play();

        }
        if (coll.transform.tag == "coins")
        {
            AudioSource audio = GetComponent<AudioSource>();
            audio.Stop();

            AudioSource cSound = coinsSound.GetComponent<AudioSource>();
            cSound.Play();
            Destroy(coll.gameObject);
            coins = coins + 1;
            //text.text = coins.ToString();
            PlayerPrefs.SetInt("Coins", coins);
        }

        if (coll.transform.tag=="boots")
        {
            AudioSource bSound = bonusSound.GetComponent<AudioSource>();
            bSound.Play();
            boots.SetActive(true);

            activeBoots.transform.localPosition =new Vector3(0,0,1);
            
            BootsIcon.SetActive(true);
            Destroy(coll.gameObject);
        }

    }

    void PlayerLose()
    {
        adsCount++;
        //if (Advertisement.IsReady() && adsCount % 6 == 0)
        //{
        //    Advertisement.Show();
        //}

        if (PlayerPrefs.GetInt("Points") < points/10)
        {
            Social.ReportScore(points/10, "CgkIv-vamLwREAIQAQ", (success) => {
                // Удачно или нет?
            });
            PlayerPrefs.SetInt("Points", points/10);
        }
        
        PlayerPrefs.SetInt("Coins", coins);
        
    }
    public IEnumerator Rot()
    {
        //Debug.Log(Time.time);
        if (telo.transform.localRotation.z == 0)
        {
            telo.Rotate(Vector3.forward * -90);
        }
        yield return new WaitForSeconds(1f);
        //else if (tr.rotation.z < 0)
        if (telo.transform.localRotation.z < 0)
        {
            telo.Rotate(Vector3.forward * 90);
        }


        //Debug.Log(Time.time);
    }
    public void BootsLifetime()
    {
        StartCoroutine(BootsAlive());
      
    }
    public IEnumerator BootsAlive()
    {

        yield return new WaitForSeconds(15f);
        boots.SetActive(false);
    }
}