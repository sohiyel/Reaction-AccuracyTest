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
    public float totalDuration = 5;
    public float eachDuration = 1;
    public float lastShowTime = 0;
    public float lastHideTime = 0;
    public float nextShowTime = 0;
    public float maxDurationBetween = 3;
    public int nrOfShows = 0;
    public int maxNrOfShows = 10;
    public float curTime = 0;
    Vector2 worldBoundary ;
    public bool move = true;
    public bool hit = false;

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
            if (curTime - lastHideTime > nextShowTime)
            {
                Vector2 camViewPoint = new Vector2(UnityEngine.Random.Range(-485, 493), UnityEngine.Random.Range(-306, 310));
                target.transform.localPosition = camViewPoint;
                target.gameObject.SetActive(true);
                lastShowTime = curTime;
                nrOfShows++;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        curTime += Time.deltaTime;
        if (curTime > totalDuration || maxNrOfShows < nrOfShows)
        {
            if (move)
            {
                move = false;
                target.SetActive(false);
                endPanel.SetActive(true);
                SaveToFile();
            }
        }
        if (move)
        {
            MoveTarget();
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

        var filePath = Path.Combine(folder, "export.csv");

        using (var writer = new StreamWriter(filePath, false))
        {
            writer.Write(content);
        }

        // Or just
        //File.WriteAllText(content);

        Debug.Log($"CSV file written to \"{filePath}\"");

    #if UNITY_EDITOR
        AssetDatabase.Refresh();
    #endif
    }
 }
