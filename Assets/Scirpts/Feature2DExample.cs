using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using OpenCVForUnity;
using System.ComponentModel;
using System.Threading;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.Features2dModule;

namespace OpenCVForUnityExample
{

    public class Feature2DExample : MonoBehaviour
    {


        [SerializeField]
        Text myMessageBox;
        Dictionary<string,Texture2D> symbolTextures;
        static WebCamTexture backCam;
        AbortableBackgroundWorker bgWoker1 ;
        bool checkImages = true;
        [SerializeField]
        RawImage panelBg;
        Texture2D test1;
        public Quaternion baseRotation;
        Texture2D test2;
        void Start ()
        {
            test1 = Resources.Load("Test/New1") as Texture2D;
            test2 = Resources.Load("Test/New2") as Texture2D;

            //Start webcam device
            WebCamDevice[] devices = WebCamTexture.devices;
            if (devices.Length == 0)
            {
                myMessageBox.text += "No camera detected";
                return;
            }
            else
            {
              //  myMessageBox.text += "Cameras detected," + devices.Length;
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
                    myMessageBox.text += "Unable to find back camera";
                    return;
                }

      
              
                 backCam.Play();
         

            }
          
            bgWoker1 = new AbortableBackgroundWorker();
           // symbolTextures = MainController.Instance.GetSymbolTextures();

            panelBg.GetComponent<RawImage>().texture = backCam;

        }


        void Update ()
        {
            //converts webcam texture to Texture2D, that can later be converted into 
            Texture2D cameraTexture = GetTexture2DFromWebcamTexture(backCam);
            if (!bgWoker1.IsBusy && checkImages)
            {
                CompareAllImages(cameraTexture);
            }
         

        }

        void CompareAllImages(Texture2D cameraTexture)
        {
            bgWoker1.DoWork += (o, a) => CompareImages(test1, "1", cameraTexture);
            // CompareImages(test1, "1", cameraTexture);
            //   CompareImages(test2, "2", cameraTexture);
            //  CompareImages(test1, "3", cameraTexture);
            //  CompareImages(test2, "4", cameraTexture);
            bgWoker1.RunWorkerAsync();

        }
       
        void CompareImages(Texture2D img1, string img1Name, Texture2D img2)
        {
            Mat img1Mat = new Mat(img1.height, img1.width, CvType.CV_8UC3);
            Utils.texture2DToMat(img1, img1Mat);

            Mat img2Mat = new Mat(img2.height, img2.width, CvType.CV_8UC3);
            Utils.texture2DToMat(img2, img2Mat);

            ORB detector = ORB.create();
            ORB extractor = ORB.create();

            MatOfKeyPoint keypoints1 = new MatOfKeyPoint();
            Mat descriptors1 = new Mat();

            detector.detect(img1Mat, keypoints1);
            extractor.compute(img1Mat, keypoints1, descriptors1);

            MatOfKeyPoint keypoints2 = new MatOfKeyPoint();
            Mat descriptors2 = new Mat();

            detector.detect(img2Mat, keypoints2);
            extractor.compute(img2Mat, keypoints2, descriptors2);


            DescriptorMatcher matcher = DescriptorMatcher.create(DescriptorMatcher.BRUTEFORCE_HAMMINGLUT);
            MatOfDMatch matches = new MatOfDMatch();

            matcher.match(descriptors1, descriptors2, matches);
           
            DMatch[] arrayDmatch = matches.toArray();
            List<double> distances = new List<double>();
            if (arrayDmatch.Length > 0)
            {
                for (int i = arrayDmatch.Length - 1; i >= 0; i--)
                {
                    distances.Add(arrayDmatch[i].distance);
                }

                distances.Sort();
                var bestDistances = distances.Take(20);
                var bestDistancesAverage = bestDistances.Average();
                myMessageBox.text = img1Name+ ": "+ bestDistancesAverage.ToString();
                

                print(img1Name+" best distance: " +bestDistancesAverage);
                if (bestDistancesAverage < 29)
                {

                    checkImages = false;
                    StartCoroutine(setResult(img1Name));
                    
                }
            }



        }

        public IEnumerator setResult(string result)
        {
            yield return new WaitForSeconds(0.5f);
            
            myMessageBox.text = "MATCH with image:" + result;
            MainController.Instance.SetDetectedSymbolName(result);
           //?? backCam.Stop();
            SceneManager.LoadScene("AudiPlayerScene");
        }

        public Texture2D GetTexture2DFromWebcamTexture(WebCamTexture webCamTexture)
        {
            
            Texture2D tx2d = new Texture2D(webCamTexture.width, webCamTexture.height);
            tx2d.SetPixels(webCamTexture.GetPixels());
          
            tx2d.Apply();
            return tx2d;
        }
        public void SearchForImage()
        {
            checkImages = true;
            if (bgWoker1 != null)
            {
              //  bgWoker1.Abort();
                bgWoker1.Dispose();
                bgWoker1 = new AbortableBackgroundWorker();
            }
         
        }

        public void CancellBgWorkers()
        {
            checkImages = false;
           // bgWoker1.Abort();
            bgWoker1.Dispose();
         
        }
      
    }

    public class AbortableBackgroundWorker : BackgroundWorker
    {

        private Thread workerThread;

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            workerThread = Thread.CurrentThread;
            try
            {
                base.OnDoWork(e);
            }
            catch (ThreadAbortException)
            {
                e.Cancel = true; //We must set Cancel property to true!
                Thread.ResetAbort(); //Prevents ThreadAbortException propagation
            }
        }


        public void Abort()
        {
            if (workerThread != null)
            {
                workerThread.Abort();
                workerThread = null;
            }
        }
    }
}