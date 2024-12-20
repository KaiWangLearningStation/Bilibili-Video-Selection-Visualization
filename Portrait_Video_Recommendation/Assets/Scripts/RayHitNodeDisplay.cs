using UnityEngine;
using UnityEngine.UI;
using Oculus.Interaction;
using TMPro;
using System.Diagnostics;

public class RayHitNodeDisplay : MonoBehaviour
{
    [SerializeField]
    private RayInteractor _rayInteractor; // Reference to your RayInteractor

    [SerializeField]
    private TextMeshProUGUI _nodeNameText; // Reference to the Text component displaying node name

    [SerializeField]
    private Material defaultMaterial; // Default material when no node is hit

    [SerializeField]
    private Renderer targetRenderer; // Reference to the Renderer of the child object whose material we want to change

    private void Start()
    {
        // Set the initial material to default
        if (targetRenderer != null && defaultMaterial != null)
        {
            targetRenderer.material = defaultMaterial;
        }
    }

    private void Update()
    {
        // Check if there's a current hit
        if (_rayInteractor.CurrentHitInfo.HasValue)
        {
            // Get the node name from the current hit
            var hitInfo = _rayInteractor.CurrentHitInfo.Value;
            string nodeName = hitInfo.HitObject.name;

            // Update the UI text
            _nodeNameText.text = nodeName;

            // Try to find and apply the corresponding material dynamically
            Material material = GetMaterialForNode(nodeName);
            if (material != null)
            {
                if (targetRenderer != null)
                {
                    targetRenderer.material = material; // Update the material on the child object
                }
            }
            else
            {
                // If no match is found, set to the default material
                if (targetRenderer != null && defaultMaterial != null)
                {
                    targetRenderer.material = defaultMaterial;
                }
            }
        }
        else
        {
            // No hit, reset text and set the default material
            _nodeNameText.text = " ";

            if (targetRenderer != null && defaultMaterial != null)
            {
                targetRenderer.material = defaultMaterial;
            }
        }
    }

    // Dynamically load material based on node name
    private Material GetMaterialForNode(string nodeName)
    {
        string materialPath = "Pic/Materials/" + nodeName;
        Material material = Resources.Load<Material>(materialPath);

        if (material == null)
        {
            UnityEngine.Debug.LogWarning("Material not found for node: " + nodeName);
        }

        return material;
    }
}