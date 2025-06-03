using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    static List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

    private void OnEnable()
    {
        spawnPoints.Add(this);
    }

    private void OnDisable()
    {
        spawnPoints.Remove(this);
    }

    public static Vector3 GetRandomSpawnPos()
    {
        //No Spawn Points
        if (spawnPoints.Count == 0) return Vector3.zero;

        int randomSpawnPos = Random.Range(0, spawnPoints.Count + 1);
        return spawnPoints[randomSpawnPos].transform.position;
    }

    private void OnDrawGizmos()
    {
        //Draw a sphere in the editor for debugging purposes (no effect on gameplay)
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 2f);
    }
}
