using UnityEngine;

public class ObjectToggle : MonoBehaviour
{
    public GameObject object1;
    public GameObject object2;
    private bool isObject1Active = true;
    [SerializeField] private GameObject redimage;


    void Start()
    {
        if (object1 != null && object2 != null)
        {
            object1.GetComponent<PlaceOnPlane>().enabled = true;
            object2.SetActive(false);
            redimage.SetActive(true);
        }
    }

    public void ToggleObjects()
    {
        if (object1 != null && object2 != null)
        {
            isObject1Active = !isObject1Active;
            object1.GetComponent<PlaceOnPlane>().enabled = isObject1Active;
            object2.SetActive(!isObject1Active);
            redimage.SetActive(!redimage.activeSelf);
        }
    }
}
