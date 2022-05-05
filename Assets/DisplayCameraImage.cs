using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayCameraImage : MonoBehaviour
{
 
    static WebCamTexture backCam;

    [SerializeField]
    RawImage panelBg;

    void Start()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
        
            return;
        }
        else
        {
    
            for (int i = 0; i < devices.Length; i++)
            {
                if (!devices[i].isFrontFacing)
                {
                    backCam = new WebCamTexture(devices[i].name, 500, 885);
                }
                else
                {
                    backCam = new WebCamTexture(devices[i].name, 500, 885);
                }

            }

            if (backCam == null)
            {
                return;
            }

            backCam.Play();


        }
        //Load textures to detect
        panelBg.GetComponent<RawImage>().texture = backCam;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
