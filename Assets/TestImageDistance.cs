using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.Features2dModule;

public class TestImageDistance : MonoBehaviour
{
    Texture2D[] textList;
    string[] files;
    string pathPreFix;
    Dictionary<(int,int),double> result = new Dictionary<(int, int),double>();
    // Start is called before the first frame update
    // Start is called before the first frame update
    void Start()
    {
        string path = Application.dataPath + "/ImagesToTest/";
        pathPreFix = @"file://";
        files = System.IO.Directory.GetFiles(path, "*.png");
        Debug.Log("Images to test path:" + path);
        StartCoroutine(LoadImages());

    }
    public void testImageDistances()
    {
        int iIndex = 0;
        int jIndex = 0;
        foreach (Texture2D texture1 in textList)
        {
            foreach (Texture2D texture2 in textList)
            {
                CompareImages(texture1, texture2, iIndex, jIndex);
                jIndex++;
            }
            iIndex++;
            jIndex = 0;
        }
        var akarmi = result;
        var ize = 0;
    }
    private IEnumerator LoadImages()
    {
        //load all images in default folder as textures and apply dynamically to plane game objects.
        //6 pictures per page
        textList = new Texture2D[files.Length];
        int counter = 0;
        foreach (string tstring in files)
        {
            string pathTemp = pathPreFix + tstring;
            WWW www = new WWW(pathTemp);
            yield return www;
            Texture2D texTmp = new Texture2D(1024, 1024, TextureFormat.ARGB32, false);
            www.LoadImageIntoTexture(texTmp);
            textList[counter] = texTmp;
            counter++;
        }
        Debug.Log("Texture list length:" + textList.Length);
        testImageDistances();
    }
    void CompareImages(Texture2D img1, Texture2D img2, int img1Index, int img2Index)
    {
        OpenCVForUnity.CoreModule.Mat img1Mat = new Mat(img1.height, img1.width, CvType.CV_8UC3);
        OpenCVForUnity.CoreModule.Mat img2Mat = new Mat(img2.height, img2.width, CvType.CV_8UC3);
        Utils.texture2DToMat(img1, img1Mat);
        Utils.texture2DToMat(img2, img2Mat);
        DetectAndCalculate(img1Mat, img2Mat, img1Index, img2Index);
    }
    void DetectAndCalculate(Mat img1Mat, Mat img2Mat, int img1Index, int img2Index)
    {
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
            var bestDistanceAvarage = bestDistances.Average();
            result.Add((img1Index, img2Index), bestDistanceAvarage);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
