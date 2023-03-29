using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class SimulatorManager : MonoBehaviour
{
    public GameObject target;
    public GameObject endPanel;
    public GameObject breaktime;
    public AudioClip[] musicClips;
    //private float totalDuration = 15;
    private float eachDuration = 1;
    private float lastShowTime = 0;
    private float lastHideTime = 0;
    private float nextShowTime = 0;
    private float maxDurationBetween = 3;
    private int nrOfShows = 0;
    private int maxNrOfShows = 10;
    private float curTime = 0;
    Vector2 worldBoundary ;
    private bool move = true;
    private bool hit = false;
    private int roundNumber = 1;
    private string componentName;
    private int MaxRoundNumber = 6;
    public string PID;

    public SimulatorManager _simulatorManager;

    [Serializable]
    public class KeyFrame
    {
        public float showTime;
        public float clickTime;
        public float distance;
        public bool clicked;

        public KeyFrame(float showTime, float clickTime, float distance, bool clicked)
        {
            this.showTime = showTime;
            this.clickTime = clickTime;
            this.distance = distance;
            this.clicked = clicked;
        }
    }

    private List<KeyFrame> keyFrames = new List<KeyFrame>(10);
    // Start is called before the first frame update


    void Start()
    {
        target.SetActive(false);
        worldBoundary = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        ShuffleArray(musicClips);
        GetComponent<AudioSource>().clip = musicClips[0];
        GetComponent<AudioSource>().Play();
        if (musicClips[0] != null)
        {
            componentName = musicClips[0].name;
        }
        else
        {
            componentName = "0";
        }
        Debug.Log(componentName);
    }



    void MoveTarget()
    {
        if (target.gameObject.activeSelf)
        {
            if (curTime > lastShowTime + eachDuration)
            {
                target.SetActive(false);
                lastShowTime = 0;
                lastHideTime = curTime;
                nextShowTime = UnityEngine.Random.Range(0, maxDurationBetween);
                if (hit == false)
                {
                    keyFrames.Add(new KeyFrame(lastShowTime, 0, 0, false));
                }
                hit = false;
            }
        }
        else
        {
            if (curTime - lastHideTime > nextShowTime && nrOfShows < maxNrOfShows)
            {
                Vector2 camViewPoint = new Vector2(UnityEngine.Random.Range(-485, 493), UnityEngine.Random.Range(-306, 310));
                target.transform.localPosition = camViewPoint;
                target.gameObject.SetActive(true);
                lastShowTime = curTime;
                nrOfShows++;
            }
            else if (nrOfShows == maxNrOfShows)
            {
                breaktime.SetActive(true);
                GetComponent<AudioSource>().Stop();
                Invoke("ResetNrOfShows", 15f);
            }
        }
    }
    void ResetNrOfShows()
    {

        SaveToFile();
        nrOfShows = 0;
        breaktime.SetActive(false);
        Debug.Log("roundNumber: " + (roundNumber));
        try { 
            GetComponent<AudioSource>().clip = musicClips[roundNumber];
            GetComponent<AudioSource>().Play();
            if (musicClips[roundNumber] != null)
            {
                componentName = musicClips[roundNumber].name;
            }
            else
            {
                componentName = "0";
            }
        }
        catch {
            componentName = "DontCount";
        }


        Debug.Log(componentName);
        roundNumber++;
        nrOfShows = 0;
        lastHideTime = curTime;
        nextShowTime = UnityEngine.Random.Range(0, maxDurationBetween);
        CancelInvoke("ResetNrOfShows");
    }

    // Update is called once per frame
    void Update()
    {
        curTime += Time.deltaTime;

        if (move)
        {
            MoveTarget();
        }
        if (roundNumber > MaxRoundNumber)
        {
            EndGame();
        }
    }

    private void Breaktime()
    {
        breaktime.SetActive(false);
        CancelInvoke("Breaktime");
        roundNumber++;
        nrOfShows = 0;
    }

    public void EndGame()
    {
        if (move)
        {
            //MoveTarget();
            GetComponent<AudioSource>().Stop();
            move = false;
            target.SetActive(false);
            endPanel.SetActive(true);
            SaveToFile();
        }
    }


    public void ClickOnTarget()
    {
        if (!hit)
        {
            Vector2 screenPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
            float dist = Vector3.Distance(worldPosition, target.transform.position) - 100;
            Debug.Log(dist);
            hit = true;
            keyFrames.Add(new KeyFrame(lastShowTime, curTime, dist, true));
        }
    }

    public string ToCSV()
    {
        var sb = new StringBuilder("ShowTime,ClickTime,Distance,Clicked");
        foreach (var frame in keyFrames)
        {
            sb.Append('\n')
                .Append(frame.showTime.ToString())
                .Append(',')
                .Append(frame.clickTime.ToString())
                .Append(',')
                .Append(frame.distance.ToString())
                .Append(',')
                .Append(frame.clicked.ToString());
        }

        return sb.ToString();
    }


    void ShuffleArray(AudioClip[] array)
    {
        // Fisher-Yates shuffle algorithm
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            AudioClip temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }


    public void SaveToFile()
    {
        // Use the CSV generation from before
        var content = ToCSV();

        // The target file path e.g.
    #if UNITY_EDITOR
        var folder = Application.streamingAssetsPath;

        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
#else
        var folder = Application.persistentDataPath;
#endif

        var filePath = Path.Combine(folder, PID, PID + "_" + componentName + ".csv");

        using (var writer = new StreamWriter(filePath, false))
        {
            writer.Write(content);
        }

        // Or just
        //File.WriteAllText(content);

        Debug.Log($"CSV file written to \"{filePath}\"");

    #if UNITY_EDITOR
        AssetDatabase.Refresh();
        keyFrames = new List<KeyFrame>(10);
    #endif
    }
 }
