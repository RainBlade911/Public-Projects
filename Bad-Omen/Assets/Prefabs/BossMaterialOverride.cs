using UnityEngine;

public class BossMaterialOverride : MonoBehaviour
{
    [SerializeField] private Material bossMaterial;

    void Start()
    {
        var renderers = GetComponentsInChildren<Renderer>();

        foreach (var r in renderers)
        {
            r.material = bossMaterial;
        }
    }
}