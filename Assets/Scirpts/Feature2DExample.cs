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
using static SwipeDetector;

namespace OpenCVForUnityExample
{

    public class Feature2DExample : MonoBehaviour
    {


        [SerializeField]
        Text myMessageBox;
        Dictionary<string,Texture2D> symbolTextures;
        static WebCamTexture backCam;
        BackgroundWorker bgWoker1,bgWoker2,bgWoker3,bgWoker4,bgWoker5;
        List<BackgroundWorker> backgroundWorkers = new List<BackgroundWorker>();
        bool checkImages = true;
        [SerializeField]
        RawImage panelBg;
        Texture2D test1;
        public Quaternion baseRotation;
        Dictionary<string, Texture2D> scannableImagesDic = new Dictionary<string, Texture2D>(); 
        double bestDistanceAvarage;
        string compareFinhisString;

        public bool isComparingFinished = false;
        bool scanIsOver = false;
        string scannedSymbolName = "";

        ProgressController progressController;
        public SuccessfulScan successfulScanController;

        TempleData.AudioData[] symbolAudios;


        void Start ()
        {
            successfulScanController = this.gameObject.GetComponent<SuccessfulScan>();
            ProcessSymbolImages();
            bgWoker1 = new BackgroundWorker();
            ///get comparable images
            GetImages();
            StartWebcamDevice();
            /// set the workers for the separated threads
            SetBackgroundWorkers();
            panelBg.GetComponent<RawImage>().texture = backCam;
            progressController = MainController.Instance.progressController;
            SwipeDetector.OnSwipe += SwipeDetector_OnSwipe;
        }
        public void ProcessSymbolImages()
        {
            foreach (var symbol in MainController.Instance.chosenSymbol.Value.symbols)
            {
                Texture2D imageTexture = new Texture2D(512, 512, TextureFormat.PVRTC_RGBA4, false);
                byte[] resultBytes = MainController.Instance.GetImageLocaly(MainController.Instance.getCurrentTempleData().name, symbol.symbol_name, ".jpg");
                imageTexture.LoadImage(resultBytes);
                var akarmi = imageTexture;
                scannableImagesDic.Add(symbol.symbol_name, imageTexture);
                //symbolImage.texture = imageTexture;
                //Color currColor = symbolImage.color;
                //currColor.a = 1;
                //symbolImage.color = currColor;
            }
        }

        #region InitialSetups
        void GetImages()
        {
            //imagesList.Add(test1);
            //imagesList.Add(test2);
            //imagesList.Add(test1);
            //imagesList.Add(test2);
            //imagesList.Add(test1);

        }
        void StartWebcamDevice()
        {
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
                    //if (!devices[i].isFrontFacing)
                    //{
                    //    backCam = new WebCamTexture(devices[i].name, 500, 885);
                    //}
                    // else
                    // {
                    //    backCam = new WebCamTexture(devices[i].name, 500, 885);
                    // }
                    backCam = new WebCamTexture(devices[0].name, 500, 885);

                }

                if (backCam == null)
                {
                    myMessageBox.text += "Unable to find back camera";
                    return;
                }

                backCam.Play();


            }
        }

        void SetBackgroundWorkers()
        {
            for (int i = 0; i < scannableImagesDic.Count; i++)
            {
                var bgWorker = new BackgroundWorker();
                backgroundWorkers.Add(bgWorker);
            }
        }
        #endregion

        void Update()
        {
            if (!scanIsOver)
            {
                //converts webcam texture to Texture2D, that can later be converted into 
                Texture2D cameraTexture = GetTexture2DFromWebcamTexture(backCam);
                CompareAllImages(cameraTexture);
            }

        }

        private void FixedUpdate()
        {
            if (!scanIsOver) { 
                if (isComparingFinished)
                {
                    backCam.Stop();
                    myMessageBox.text = compareFinhisString;
                    
                    scanIsOver = true;
                    Debug.Log("compare finish");
                    GetAudioForSymbol();
                    progressController.UpdateProgressInJson(scannedSymbolName, MainController.Instance.getCurrentTempleData().name);
                    successfulScanController.SuccessfulScanHappened(scannedSymbolName);
                }
                else
                {
                    myMessageBox.text = bestDistanceAvarage.ToString();
                }
            }
            else
            {
                
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
            
            for (int i = 0; i < scannableImagesDic.Count; i++)
            {
                var akarmi = scannableImagesDic.ElementAt(i).Value;
                var barmi = scannableImagesDic.ElementAt(i).Key;
                var ize = 0;
                CompareImages(backgroundWorkers[i], scannableImagesDic.ElementAt(i).Value, scannableImagesDic.ElementAt(i).Key, cameraTexture);
            }
        }


        void CompareImages(BackgroundWorker bgWoker,Texture2D img1, string img1Name, Texture2D img2)
        {

            OpenCVForUnity.CoreModule.Mat img1Mat = new Mat(img1.height, img1.width, CvType.CV_8UC3);
            OpenCVForUnity.CoreModule.Mat img2Mat = new Mat(img2.height, img2.width, CvType.CV_8UC3);

            Utils.texture2DToMat(img1, img1Mat);
            Utils.texture2DToMat(img2, img2Mat);
            
            if( !bgWoker.IsBusy ){
                bgWoker.DoWork += (o, a) => DetectAndCalculate(img1Mat,img2Mat,img1Name);
                //DetectAndCalculate(detector,img2Mat,keypoints2,extractor,descriptors1,descriptors2,img1Name);
                bgWoker.RunWorkerAsync();
            }
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
            //    myMessageBox.text = img1Name+ ": "+ bestDistancesAverage.ToString();
            if(!isComparingFinished){
  print(img1Name+" best distance: " +bestDistanceAvarage);
            }
              
               if (bestDistanceAvarage < 29 && !isComparingFinished)
               {
     compareFinhisString = img1Name + "image name" + " bestdistance" + bestDistanceAvarage;
                    scannedSymbolName = img1Name;
                   checkImages = false;
       
                    isComparingFinished = true;
                    
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
        public void GobackToTempleSelection()
        {
            backCam.Stop();
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