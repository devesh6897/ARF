using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PlaneTextureChanger : MonoBehaviour
{
    public ARPlaneManager planeManager;
    public Texture newTexture; // Assign in Inspector
    public Material opaqueMaterial; // Assign an opaque material in Inspector

    void Start()
    {
        // Listen for new planes being detected
        planeManager.planesChanged += OnPlanesChanged;
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        planeManager.planesChanged -= OnPlanesChanged;
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        // Apply the new texture to newly added planes
        foreach (var plane in args.added)
        {
            ChangePlaneTexture(plane);
        }
    }

    private void ChangePlaneTexture(ARPlane plane)
    {
        MeshRenderer renderer = plane.GetComponent<MeshRenderer>();

        if (renderer != null)
        {
            if (opaqueMaterial != null)
            {
                renderer.material = new Material(opaqueMaterial); // Use opaque material
            }

            if (newTexture != null)
            {
                renderer.material.mainTexture = newTexture;
                renderer.material.color = new Color(1, 1, 1, 1); // Ensure full opacity
            }
        }
    }
}
