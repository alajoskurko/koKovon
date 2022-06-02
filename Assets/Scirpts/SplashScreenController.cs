using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.Android;
//using UnityEngine.iOS;

public class SplashScreenController : MonoBehaviour
{
    private void Start()
    {

        if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {

        }
        else
        {

            // We do not have permission to use the microphone.
            // Ask for permission or proceed without the functionality enabled.
            #if UNITY_ANDROID
            Permission.RequestUserPermission(Permission.Camera);
            #endif
            #if UNITY_IOS
                    StartCoroutine(GetPermissionForCameraOnIOS());           
            #endif

        }

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

    IEnumerator GetPermissionForCameraOnIOS()
    {
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.Log("webcam found");
        }
        else
        {
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        }

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
