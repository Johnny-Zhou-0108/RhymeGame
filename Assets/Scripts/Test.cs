using UnityEngine;

public class Test : MonoBehaviour
{
    // not using now
    public AudioManager audioManager;
    public VisualManager visualManager;
    public GameManager gameManager; // Reference to the GameManager to reset the score

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // Generate and load the audio manager and then start visuals
            audioManager.LoadAndStartRemix();
            visualManager.StartVisuals(audioManager.GetPlayScheduledTimes());
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            // Only generate and load the audio manager
            audioManager.LoadAndStartRemix();
            // Optionally, you can debug the array
            Debug.Log("PlayScheduledTimes: [" + string.Join(", ", audioManager.GetPlayScheduledTimes()) + "]");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            // Stop the current remix and visuals and zero out the score
            audioManager.StopRemix();
            visualManager.StopVisuals();
            //gameManager.ResetScore();
        }
    }
}
