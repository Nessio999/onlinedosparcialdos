using UnityEngine;
using Fusion;
using System.Collections.Generic;

public class EnemySpawner : NetworkBehaviour
{
    [Header("Enemy Spawner Settings")]
    [SerializeField] private NetworkObject enemyPrefab;
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] private float spawnInterval = 3f;

    private float timer = 0f;
    private List<int> availableSpawnPoints = new List<int>();

    public override void Spawned()
    {
        if (!Runner.IsServer) return;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            availableSpawnPoints.Add(i);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Runner.IsServer) return;

       timer += Runner.DeltaTime;

     if (timer >= spawnInterval)
     {
        SpawnEnemy();
        timer = 0f;
     }
    }

    private void SpawnEnemy()
    {
        if (availableSpawnPoints.Count == 0)
        {
            Debug.Log("No free spawn points available.");
            return;
        }
        if (!PhotonManager._PhotonManager.enemiesSpawned)
        {
            PhotonManager._PhotonManager.NotifyEnemiesSpawned();
        }
     //   int randIndex = Random.Range(0, availableSpawnPoints.Count);
        int randIndex = Random.Range(0, availableSpawnPoints.Count);
        int spawnPointIndex = availableSpawnPoints[randIndex];
        
        // Remove from available list (Reservation)
        availableSpawnPoints.RemoveAt(randIndex);

        Transform spawnPoint = spawnPoints[spawnPointIndex];
        NetworkObject enemyInstance = Runner.Spawn(enemyPrefab, spawnPoint.position + new Vector3(0, 1, 0), spawnPoint.rotation);
        
        if(enemyInstance.TryGetComponent(out Rival rival))
        {
            rival.Initialize(this, spawnPointIndex);
        }

        //foreach (var player in Runner.ActivePlayers)
        //{
        //    Runner.SetPlayerAlwaysInterested(player, enemyInstance, true);
        //}

        Debug.Log($"Enemy spawned at index {spawnPointIndex}. Remaining points: {availableSpawnPoints.Count}");
    }

    public void ReleasePoint(int index)
    {
        if (!availableSpawnPoints.Contains(index))
        {
            availableSpawnPoints.Add(index);
            Debug.Log($"Spawn point {index} released. Available points: {availableSpawnPoints.Count}");
        }
    }
}
