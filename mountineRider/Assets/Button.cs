using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour {

    public bool pause = false;

	// Use this for initialization
	void Start () {
        pause = false;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Pause()
    {
        if(!pause)
        {
            pause = true;
            Time.timeScale = 0;
        }
        else
        {
            pause = false;
            Time.timeScale = PlayerPrefs.GetFloat("GameScale");
        }
    }
}
