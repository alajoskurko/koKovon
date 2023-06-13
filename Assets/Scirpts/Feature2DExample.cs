using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
//using OpenCVForUnity;
using System.ComponentModel;
using System.Threading;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.Features2dModule;
using UnityEngine.Scripting;
using static SwipeDetector;

namespace OpenCVForUnityExample
{

    public class Feature2DExample : MonoBehaviour
    {

        Dictionary<string,Texture2D> symbolTextures;
        static WebCamTexture backCam;
        //BackgroundWorker bgWoker1,bgWoker2,bgWoker3,bgWoker4,bgWoker5;
        List<MyBacgkroundWorked> backgroundWorkers = new List<MyBacgkroundWorked>();

        [SerializeField]
        RawImage panelBg;
        public Quaternion baseRotation;
        Dictionary<string, Mat> scannableImagesDic = new Dictionary<string, Mat>(); 
        double bestDistanceAvarage;
        string compareFinhisString;

        public bool isComparingFinished = false;
        bool scanIsOver = false;
        string scannedSymbolName = null;

        ProgressController progressController;
        public SuccessfulScan successfulScanController;

        TempleData.AudioData[] symbolAudios;
        Texture2D cameraTexture;

        [SerializeField]
        RawImage referencePanel;

        [SerializeField]
        Canvas mainCanvas;

        int counter = 0;

        public float deltaTime;
        public float maxFps = 0;
        void Start ()
        {
            successfulScanController = this.gameObject.GetComponent<SuccessfulScan>();
            StartWebcamDevice();
            ProcessSymbolImages();
            
            backCam.Play();
            /// set the workers for the separated threads
            /// set the workers for the separated threads
            SetBackgroundWorkers();
            panelBg.GetComponent<RawImage>().texture = backCam;
            panelBg.rectTransform.sizeDelta = new Vector2(referencePanel.rectTransform.rect.height, referencePanel.rectTransform.rect.width);
            //panelBg.rectTransform.sizeDelta = new Vector2(backCam.width, backCam.height);
            //Debug.LogWarning(panelBg.rectTransform.sizeDelta.x / panelBg.rectTransform.sizeDelta.y  + "elotte");
            ////Debug.LogWarning(referencePanel.GetComponent<RawImage>().rectTransform.rect.height + " height");
            ////Debug.LogWarning(panelBg.rectTransform.GetComponent<RawImage>().rectTransform.rect.height + " height");
            ////Debug.LogWarning(mainCanvas.GetComponent<RectTransform>().rect.height + " height");
            ////Debug.LogWarning(panelBg.rectTransform.rect.width + " height");

            float heightDifference = 100 - (panelBg.rectTransform.rect.width * 100 / mainCanvas.GetComponent<RectTransform>().rect.height);
            float diffInScale = 1 + (heightDifference / 100);
            panelBg.rectTransform.localScale = new Vector3(diffInScale, diffInScale, diffInScale);
            //Debug.LogWarning(panelBg.rectTransform.sizeDelta.x / panelBg.rectTransform.sizeDelta.y + "utana");
            //Debug.LogWarning(diffInScale + " diffInScale");

            //Debug.LogWarning(diffInScale + " diffInScale");


#if UNITY_IPHONE
            panelBg.transform.localScale = new Vector3(1, -1, 1);
#endif
            progressController = MainController.Instance.progressController;
            SwipeDetector.OnSwipe += SwipeDetector_OnSwipe;
            
        }
        public void ProcessSymbolImages()
        {
            foreach (var symbol in MainController.Instance.chosenSymbol.Value.symbols)
            {
                Texture2D imageTexture = new Texture2D(512, 512, TextureFormat.PVRTC_RGB4, false);
                byte[] resultBytes = MainController.Instance.GetImageLocaly(MainController.Instance.getCurrentTempleData().name, symbol.symbol_name, ".jpg");
                imageTexture.LoadImage(resultBytes);
                
                OpenCVForUnity.CoreModule.Mat mat = new Mat(imageTexture.height, imageTexture.width, CvType.CV_8UC3);
                Utils.texture2DToMat(imageTexture,mat);
                scannableImagesDic.Add(symbol.symbol_name, mat);
            }
        }

        #region InitialSetups
        void StartWebcamDevice()
        {
            //Start webcam device
            WebCamDevice[] devices = WebCamTexture.devices;
            if (devices.Length == 0)
            {
                return;
            }
            else
            {
                //  myMessageBox.text += "Cameras detected," + devices.Length;
                for (int i = 0; i < devices.Length; i++)
                {
                    //if (!devices[i].isFrontFacing)
                    //{
                    //    backCam = new WebCamTexture(devices[i].name, 500, 885);
                    //}
                    // else
                    // {
                    //    backCam = new WebCamTexture(devices[i].name, 500, 885);
                    // }

                    //decrease webcamtexture size for a better fps performance
                    //if there is something wrong with the symbol scan, put back the 500, 885
                    backCam = new WebCamTexture(devices[0].name, 500, 885);
                    //System.GC.Collect();
                    //Debug.Log(GarbageCollector.GCMode + " gb gcmode");

                }

                if (backCam == null)
                {
                    return;
                }

                


            }
        }

        void SetBackgroundWorkers()
        {
            for (int i = 0; i < scannableImagesDic.Count; i++)
            {
                var bgWorker = new MyBacgkroundWorked();
                backgroundWorkers.Add(bgWorker);
            }
        }

        #endregion


        
        void Update()
        {
                counter = 0;
                Destroy(cameraTexture);
                if (!scanIsOver)
                {
                    if (isComparingFinished)
                    {

                        scanIsOver = true;
                        Debug.LogWarning("compare finish feherhiba " + scannedSymbolName);
                        GetAudioForSymbol();
                        progressController.UpdateProgressInJson(scannedSymbolName, MainController.Instance.getCurrentTempleData().name);
                        successfulScanController.SuccessfulScanHappened(scannedSymbolName);
                        backCam.Stop();
                    }
                    else
                    {
                        //converts webcam texture to Texture2D, that can later be converted into 
                        cameraTexture = GetTexture2DFromWebcamTexture(backCam);
                        CompareAllImages(cameraTexture);
                    }

                }
        }

        void GetAudioForSymbol()
        {
            foreach (var symbol in MainController.Instance.chosenSymbol.Value.symbols)
            {
                if(symbol.symbol_name == scannedSymbolName)
                {
                    symbolAudios = symbol.audios;
                }
            }
        }

        void CompareAllImages(Texture2D cameraTexture)
        {
            Mat cameraImageMat = new Mat(cameraTexture.height, cameraTexture.width, CvType.CV_8UC3);
            Utils.texture2DToMat(cameraTexture, cameraImageMat);
            
            for (int i = 0; i < scannableImagesDic.Count; i++)
            {
                CompareImages(backgroundWorkers[i], scannableImagesDic.ElementAt(i).Value, scannableImagesDic.ElementAt(i).Key, cameraImageMat);
            }
        }


        void CompareImages(MyBacgkroundWorked bgWoker,Mat img1Mat, string img1Name, Mat cameraImageMat)
        {

            

            if (!bgWoker.IsBusy)
            {
                bgWoker.DoWork += (o, a) => DetectAndCalculate(img1Mat, cameraImageMat, img1Name);
                bgWoker.RunWorkerAsync();
                //bgWoker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
            }
        }
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            backgroundWorkers[1].Dispose();

        }
        private void SwipeDetector_OnSwipe(SwipeData data)
        {
            if (data.Direction == SwipeDirection.Right)
            {
                GobackToTempleSelection();
            }
        }
        void DetectAndCalculate(Mat img1Mat, Mat img2Mat,string img1Name){
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
                bestDistanceAvarage = bestDistances.Average();

                if (bestDistanceAvarage < 29 && !isComparingFinished)
                {
                    compareFinhisString = img1Name + "image name" + " bestdistance" + bestDistanceAvarage;
                    if (scannedSymbolName == null)
                    {
                        scannedSymbolName = img1Name;
                    }
                    isComparingFinished = true;
                }

            }
        }


        public IEnumerator setResult(string result)
        {
            yield return new WaitForSeconds(0.5f);
            
            MainController.Instance.SetDetectedSymbolName(result);
           //?? backCam.Stop();
            SceneManager.LoadScene("AudiPlayerScene");
        }
        public void GobackToTempleSelection()
        {
            Resources.UnloadUnusedAssets();
            backCam.Stop();
            Destroy(backCam);
            backgroundWorkers = null;
            SceneManager.LoadScene("SpecificTempleScene");
        }

        public Texture2D GetTexture2DFromWebcamTexture(WebCamTexture webCamTexture)
        {
            if (webCamTexture)
            {
                Texture2D tx2d = new Texture2D(webCamTexture.width, webCamTexture.height);
                tx2d.SetPixels(webCamTexture.GetPixels());

                tx2d.Apply();
                return tx2d;
            }
            else
            {
                return new Texture2D(100, 100);
            }   
           
        }

        // public void SearchForImage()
        // {
        //     checkImages = true;
        //     if (bgWoker1 != null)
        //     {
        //       //  bgWoker1.Abort();
        //         bgWoker1.Dispose();
        //         bgWoker1 = new AbortableBackgroundWorker();
        //     }
         
        // }

        // public void CancellBgWorkers()
        // {
        //     checkImages = false;
        //    // bgWoker1.Abort();
        //     bgWoker1.Dispose();
         
        // }
      
    }


    #region AbortableBackground

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
    #endregion
}

public class MyBacgkroundWorked : BackgroundWorker {
    public event RunWorkerCompletedEventHandler MyRunWorkerCompleted;
    public delegate void RunWorkerCompletedEventHandler(object sender, RunWorkerCompletedEventArgs e);
}
