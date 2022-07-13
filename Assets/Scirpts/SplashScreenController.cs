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

#if UNITY_IOS
         Debug.Log("hello ios");
        Debug.LogWarning("hello ios 2");
            Application.RequestUserAuthorization(UserAuthorization.WebCam);
                StartCoroutine(GetPermissionForCameraOnIOS());           
#endif

        if (Permission.HasUserAuthorizedPermission(Permission.Camera))
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
            Application.RequestUserAuthorization(UserAuthorization.WebCam);
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
    void Update(){
        if (!MainController.Instance.isInitializing)
        {
            StartCoroutine(LoadTempleSelectionScreen());
            //TODO informative popup
        }
    }
    IEnumerator GetPermissionForCameraOnIOS()
    {

        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

    }

    public IEnumerator LoadTempleSelectionScreen()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("TempleSelectionScene");

    }
}
