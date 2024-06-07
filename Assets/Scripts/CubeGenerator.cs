using UnityEngine;
using TMPro;

public class CubeGenerator : MonoBehaviour
{
    public Vector3 initialPosition = new Vector3(0, 10, 0);
    public float fallSpeed = 5.0f;
    public GameObject cubePrefab; // Reference to the cube prefab
    public TMP_Text scoreText; // Reference to the score UI
    public float missDistanceThreshold = 0.5f; // Distance threshold to determine a miss
    public float extraFallTime = 2.0f;

    private int currentScore = 0;

    void Start()
    {
        // Initialize the score text
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString();
        }
    }

    public void GenerateCube()
    {
        GameObject cube = Instantiate(cubePrefab, initialPosition, Quaternion.identity);
        StartCoroutine(FallAndCheckScore(cube));
    }

    private System.Collections.IEnumerator FallAndCheckScore(GameObject cube)
    {
        bool scored = false;

        // Allow the cube to fall and check if it's in the scoring zone
        while (cube.transform.position.y > -missDistanceThreshold)
        {
            cube.transform.position += Vector3.down * fallSpeed * Time.deltaTime;

            // Check for input to score the cube
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (Mathf.Abs(cube.transform.position.y) <= missDistanceThreshold)
                {
                    AddScore(10);
                    Destroy(cube);
                    yield break;
                }
            }

            yield return null;
        }

        // After falling below the threshold, mark as missed and decrease score
        if (!scored)
        {
            AddScore(-5);
        }

        // Let the cube fall for an extra 2 seconds and then destroy it
        
        float fallBelowTime = 0f;
        while (fallBelowTime < extraFallTime)
        {
            if (cube == null) yield break; // Exit if the cube is already destroyed

            cube.transform.position += Vector3.down * fallSpeed * Time.deltaTime;
            fallBelowTime += Time.deltaTime;
            yield return null;
        }

        if (cube != null)
        {
            Destroy(cube);
        }
    }

    private void AddScore(int points)
    {
        currentScore += points;
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString();
        }
    }
}
