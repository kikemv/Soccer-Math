using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class Cinematics : MonoBehaviour
{

    public VideoPlayer videoPlayer;

    // Start is called before the first frame update
    void Start()
    {
        videoPlayer.loopPointReached += Disablevideo;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShootVideo()
    {
        gameObject.SetActive(true);
        videoPlayer.Play();
        
    }

    private void Disablevideo(VideoPlayer vp)
    {
        gameObject.SetActive(false);
    }
}

