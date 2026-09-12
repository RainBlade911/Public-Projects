using UnityEngine;

public class RightCameraAutoFollow : MonoBehaviour
{
    private Transform target;

    void Start()
    {
        // Automatically find the character in the scene
        GameObject character = GameObject.Find("RightCharacter");

        if (character != null)
        {
            target = character.transform;
            Debug.Log("Target found: RightCharacter");
        }
        else
        {
            Debug.LogError("RightCharacter not found in scene!");
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Always look at the character
        transform.LookAt(target.position + Vector3.up * 1.2f);
    }
}
