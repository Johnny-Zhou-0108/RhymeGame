using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; } // Singleton instance

    public AudioClip idleAudioClip; // Audio clip to play if no input for 10 seconds
    public float idleTimeThreshold = 10.0f; // Time in seconds to wait before playing the idle audio
    public bool isOtherClipPlaying = false; // Flag to indicate if other clips are playing
    public bool LastClip = false;
    public Button ReloadButton;

    private float timeSinceLastInput = 0.0f;
    private AudioSource audioSource;
    private bool isIdleAudioPlaying = false;
    private float idleAudioDuration = 0.0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Make the instance persistent across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        if (idleAudioClip != null)
        {
            idleAudioDuration = idleAudioClip.length;
        }

        // Ensure the ReloadButton is correctly set up
        if (ReloadButton != null)
        {
            ReloadButton.onClick.AddListener(Reload);
            ReloadButton.gameObject.SetActive(false); // Initially hide the button
        }
        else
        {
            Debug.LogError("ReloadButton is not assigned in the inspector.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandleSpaceBarPress();
            timeSinceLastInput = 0.0f; // Reset the timer on space bar press
            isIdleAudioPlaying = false; // Reset the idle audio playing flag
        }
        else
        {
            timeSinceLastInput += Time.deltaTime;
        }

        if (timeSinceLastInput >= idleTimeThreshold && !isIdleAudioPlaying && !isOtherClipPlaying)
        {
            PlayIdleAudio();
            timeSinceLastInput = -idleAudioDuration; // Subtract the audio duration to account for the clip length
        }

        if (isIdleAudioPlaying && !audioSource.isPlaying)
        {
            isIdleAudioPlaying = false; // Reset the flag when the idle audio finishes
        }

        if (LastClip)
        {
            if (ReloadButton != null)
            {
                ReloadButton.gameObject.SetActive(true);
            }
            LastClip = false;
        }
    }

    public void Reload()
    {
        SceneManager.LoadScene("Intro");
    }

    void HandleSpaceBarPress()
    {
        //Debug.Log("Space bar pressed");
        FindObjectOfType<VisualManager>().RegisterHit();
    }

    void PlayIdleAudio()
    {
        if (idleAudioClip != null && audioSource != null)
        {
            audioSource.clip = idleAudioClip;
            audioSource.Play();
            isIdleAudioPlaying = true;
            Debug.Log("Playing idle audio clip");
        }
    }
}
