using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    public WebCamTexture backCam;
    public Quaternion baseRotation;
    void Start()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        for (int i = 0; i < devices.Length; i++)
        {
            if (!devices[i].isFrontFacing)
            {
                backCam = new WebCamTexture(devices[i].name, 720, 1280);
            }
            else
            {
                backCam = new WebCamTexture(devices[i].name, 720, 1280);
            }

        }

        backCam.Play();
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.mainTexture = backCam;
        baseRotation = transform.rotation;
       
    }

    void Update()
    {
        transform.rotation = baseRotation * Quaternion.AngleAxis(backCam.videoRotationAngle, Vector3.up);
    }
}
