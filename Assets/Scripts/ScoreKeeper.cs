using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;

public class ScoreKeeper : MonoBehaviour
{
    public TMP_Text scoreText;
    public float perfectHitTimeWindow = 0.2f; // Time window for a perfect hit
    public int perfectHitScore = 10;
    public int missHitScore = -10;

    private int currentScore = 0;
    private double[] baselineTimes;
    private Dictionary<double, GameObject> cubeBaselineMap = new Dictionary<double, GameObject>();
    private HashSet<GameObject> processedCubes = new HashSet<GameObject>();

    void Start()
    {
        UpdateScoreText();
    }

    public void SetBaselineTimes(double[] times)
    {
        baselineTimes = times;
        cubeBaselineMap.Clear();
        processedCubes.Clear();
    }

    public void RegisterHitTime(double hitTime)
    {
        if (baselineTimes == null || baselineTimes.Length == 0)
        {
            Debug.LogError("Baseline times array is empty or not set.");
            return;
        }

        double closestTime;
        int index;
        GetClosestBaselineTime(hitTime, out closestTime, out index);
        double timeGap = System.Math.Abs(hitTime - closestTime);
        Debug.Log("Time gap between hit and baseline: " + timeGap);

        if (timeGap <= perfectHitTimeWindow)
        {
            currentScore += perfectHitScore;
            Debug.Log($"Perfect hit! Score: {currentScore}, Cube position in array: {index}");
            if (cubeBaselineMap.ContainsKey(closestTime))
            {
                GameObject cubeToDestroy = cubeBaselineMap[closestTime];
                if (!processedCubes.Contains(cubeToDestroy))
                {
                    processedCubes.Add(cubeToDestroy);
                    NotifyVisualManagerToDestroyCube(cubeToDestroy);
                }
            }
        }
        else
        {
            currentScore += missHitScore;
            Debug.Log($"Missed hit! Score: {currentScore}");
        }

        UpdateScoreText();
    }

    private void GetClosestBaselineTime(double hitTime, out double closestTime, out int index)
    {
        closestTime = baselineTimes[0];
        double smallestGap = System.Math.Abs(hitTime - closestTime);
        index = 0;

        for (int i = 0; i < baselineTimes.Length; i++)
        {
            double gap = System.Math.Abs(hitTime - baselineTimes[i]);
            if (gap < smallestGap)
            {
                smallestGap = gap;
                closestTime = baselineTimes[i];
                index = i;
            }
        }
    }

    public void NotifyCubeReachedBaseline(GameObject cube, double baselineTime)
    {
        if (!cubeBaselineMap.ContainsKey(baselineTime))
        {
            cubeBaselineMap[baselineTime] = cube;
            Debug.Log("Cube reached the baseline at time: " + baselineTime);
            StartCoroutine(MissAndNotify(cube, baselineTime));
        }
    }

    private IEnumerator MissAndNotify(GameObject cube, double baselineTime)
    {
        yield return new WaitForSeconds((float)perfectHitTimeWindow);

        if (cubeBaselineMap.ContainsKey(baselineTime))
        {
            currentScore += missHitScore;
            Debug.Log($"Miss hit! Score: {currentScore}");
            UpdateScoreText();

            NotifyVisualManagerToContinueFalling(cube);
        }
    }

    private void NotifyVisualManagerToDestroyCube(GameObject cube)
    {
        VisualManager visualManager = FindObjectOfType<VisualManager>();
        if (visualManager != null)
        {
            Debug.Log($"Signaling to destroy cube: {cube.name}");
            //visualManager.DestroyCube(cube);
        }
    }

    private void NotifyVisualManagerToContinueFalling(GameObject cube)
    {
        VisualManager visualManager = FindObjectOfType<VisualManager>();
        if (visualManager != null)
        {
            Debug.Log($"Signaling to continue falling for cube: {cube.name}");
            //visualManager.ContinueFalling(cube);
        }
    }

    private void UpdateScoreText()
    {
        scoreText.text = "Score: " + currentScore;
    }
}
