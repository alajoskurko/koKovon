using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class SplashScreenController : MonoBehaviour
{
    private void Start()
    {
        // Will attach a VideoPlayer to the main camera.
        GameObject camera = GameObject.Find("Main Camera");

        // VideoPlayer automatically targets the camera backplane when it is added
        // to a camera object, no need to change videoPlayer.targetCamera.
        var videoPlayer = camera.AddComponent<UnityEngine.Video.VideoPlayer>();
    

        // By default, VideoPlayers added to a camera will use the far plane.
        // Let's target the near plane instead.
        videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.CameraNearPlane;


        VideoClip clip = Resources.Load<VideoClip>("IntroClip") as VideoClip;
        videoPlayer.clip = clip;

        // Restart from beginning when done.
        videoPlayer.isLooping = true;

        videoPlayer.Play();
    }
    public void LoadTempleSelectionScreen()
    {
        if (!MainController.Instance.isInitializing)
        {
            SceneManager.LoadScene("TempleSelectionScene");
            //TODO informative popup
        }
        else
        {
            // TODO
            print("Wait for init to end");
        }
       
       
    }
}
