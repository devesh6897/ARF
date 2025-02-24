using UnityEngine;
using UnityEngine.UI;

public class DisablePlanesButton : MonoBehaviour
{
    public Button disableButton; // Assign the UI button in the Inspector
    public float minY = 0f; // Minimum Y threshold

    void Start()
    {
        if (disableButton != null)
            disableButton.onClick.AddListener(DisableAboveMinY);
        else
            Debug.LogError("Button not assigned in the Inspector!");
    }

    void DisableAboveMinY()
    {
        GameObject[] planes = GameObject.FindGameObjectsWithTag("Plane");

        foreach (GameObject plane in planes)
        {
            if (plane.transform.position.y > minY)
            {
                plane.SetActive(false);
            }
        }

        Debug.Log("Disabled all planes above Y = " + minY);
    }
}
