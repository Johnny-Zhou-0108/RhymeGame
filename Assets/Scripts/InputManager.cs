using UnityEngine;

public class InputManager : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandleSpaceBarPress();
        }
    }

    void HandleSpaceBarPress()
    {
        Debug.Log("Space bar pressed");
        FindObjectOfType<VisualManager>().RegisterHit();
    }
}
