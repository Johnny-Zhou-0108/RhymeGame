using UnityEngine;

public class Test : MonoBehaviour
{
    public AudioManager audioManager;
    public VisualManager visualManager;

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
    }
}
