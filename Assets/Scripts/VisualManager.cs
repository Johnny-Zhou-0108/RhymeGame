using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VisualManager : MonoBehaviour
{
    public GameObject cubePrefab; // Reference to the cube prefab
    public Vector3 initialPosition = new Vector3(0, 10, 0); // Initial falling position of the cubes
    public float fallSpeed = 5.0f; // Falling speed of the cubes
    public float baselineY = 0.0f; // The Y position of the baseline
    public TMP_Text scoreText; // Reference to the score UI
    public float perfectHitDistance = 0.2f; // Distance threshold for a perfect hit
    public float extraFallTime = 2.0f; // Extra fall time for missed cubes
    public int perfectHitScore = 10; // Score for a perfect hit
    public int missHitScore = -10; // Score deduction for a miss

    private int currentScore = 0;
    private List<GameObject> cubes = new List<GameObject>();
    private bool stopVisuals = false;
    private GameManager gameManager;

    void Start()
    {
        // Initialize the score text
        gameManager = FindObjectOfType<GameManager>();

        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString();
        }
    }

    public void StartVisuals(double[] scheduledTimes)
    {
        stopVisuals = false;
        StartCoroutine(GenerateAndControlCubes(scheduledTimes));
    }

    public void StopVisuals()
    {
        stopVisuals = true;
        StopAllCoroutines(); // Stop all coroutines to halt current visual processes
        // Destroy remaining cubes
        foreach (var cube in cubes)
        {
            if (cube != null)
            {
                Destroy(cube);
            }
        }
        cubes.Clear();
    }

    IEnumerator GenerateAndControlCubes(double[] scheduledTimes)
    {
        for (int i = 0; i < scheduledTimes.Length; i++)
        {
            if (stopVisuals) yield break;

            double playTime = scheduledTimes[i];
            float fallDuration = CalculateFallDuration();

            double currentTime = AudioSettings.dspTime;
            double startTime = playTime - fallDuration;
            double delay = startTime - currentTime;

            if (delay > 0)
            {
                yield return new WaitForSeconds((float)delay);
            }

            if (stopVisuals) yield break;

            GameObject cube = Instantiate(cubePrefab, initialPosition, Quaternion.identity);
            cube.name = "Cube" + i; // Name the cube with its index
            cubes.Add(cube);
            StartCoroutine(ControlFallingCube(cube));

            // Wait for the time difference between beats before generating the next cube
            if (i < scheduledTimes.Length - 1)
            {
                double nextPlayTime = scheduledTimes[i + 1];
                yield return new WaitForSeconds((float)(nextPlayTime - playTime));
            }
        }
    }

    float CalculateFallDuration()
    {
        // Calculate the fall duration based on the distance and the falling speed
        float distanceToBaseline = initialPosition.y - baselineY;
        return distanceToBaseline / fallSpeed;
    }

    IEnumerator ControlFallingCube(GameObject cube)
    {
        while (cube != null && cube.transform.position.y > baselineY - perfectHitDistance)
        {
            if (stopVisuals) yield break;

            cube.transform.position += Vector3.down * fallSpeed * Time.deltaTime;
            yield return null;
        }

        if (cube != null && !stopVisuals)
        {
            AddScore(missHitScore);
            Debug.Log($"Missed cube! Deducting score for cube: {cube.name}");

            float fallBelowTime = 0f;
            while (cube != null && fallBelowTime < extraFallTime)
            {
                if (stopVisuals) yield break;

                cube.transform.position += Vector3.down * fallSpeed * Time.deltaTime;
                fallBelowTime += Time.deltaTime;
                yield return null;
            }

            if (cube != null)
            {
                Debug.Log($"Missed cube! Destroying cube: {cube.name}");
                cubes.Remove(cube);
                Destroy(cube);
            }
        }
    }

    public void RegisterHit()
    {
        GameObject closestCube = null;
        float closestDistance = float.MaxValue;

        foreach (var cube in cubes)
        {
            if (cube == null) continue;

            float distance = Mathf.Abs(cube.transform.position.y - baselineY);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCube = cube;
            }
        }

        if (closestCube != null && closestDistance <= perfectHitDistance)
        {
            AddScore(perfectHitScore);
            Debug.Log($"Perfect hit! Destroying cube: {closestCube.name}");
            cubes.Remove(closestCube);
            Destroy(closestCube);
        }
        else
        {
            AddScore(missHitScore);
            Debug.Log($"Miss hit! Deducting score for cube: {closestCube?.name}");
        }
    }

    private void AddScore(int points)
    {
        currentScore += points;
        gameManager.AddScore(points); // Notify GameManager of the score change
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString();
        }
        Debug.Log($"Current score: {currentScore}");
    }
}
