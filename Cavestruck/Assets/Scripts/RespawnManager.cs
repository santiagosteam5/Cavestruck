using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance;

    private Vector3 currentCheckpoint;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetCheckpoint(Vector3 checkpointPosition)
    {
        currentCheckpoint = checkpointPosition;
        Debug.Log("Checkpoint actualizado a: " + currentCheckpoint);
    }

    public Vector3 GetCheckpoint()
    {
        return currentCheckpoint;
    }
}
