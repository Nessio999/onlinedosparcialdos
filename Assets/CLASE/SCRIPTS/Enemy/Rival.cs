using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using UnityEngine;
public class Rival : NetworkBehaviour
{
    [Networked] public NetworkScoreEntry controllingPlayer { get; set; }

    private EnemySpawner _spawner;
    private int _spawnIndex = -1;

    [SerializeField] float shootInterval = 1f;
    [SerializeField] Transform shootPoint;
    [SerializeField] GameObject bullet;

    TickTimer shootTimer;

    public void Initialize(EnemySpawner spawner, int index)
    {
        _spawner = spawner;
        _spawnIndex = index;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        Health target = GetClosestTarget();
        if (target == null) return;

        if (shootTimer.ExpiredOrNotRunning(Runner))
        {
            ShootAtObjective(target);
            shootTimer = TickTimer.CreateFromSeconds(Runner, shootInterval);
        }
    }

    Health GetClosestTarget()
    {
        Health closest = null;
        float dist = float.MaxValue;

        foreach (var obj in Health.allObjectives)
        {
            float d = Vector3.Distance(transform.position, obj.transform.position);
            if (d < dist)
            {
                dist = d;
                closest = obj;
            }
        }

        return closest;
    }

    void ShootAtObjective(Health target)
    {
        Vector3 dir = (target.transform.position - shootPoint.position).normalized;

        var bulletInstance = Runner.Spawn(
            bullet,
            shootPoint.position + dir,
            Quaternion.LookRotation(dir)
        );

        bulletInstance.GetComponent<Rigidbody>().linearVelocity = dir * 120f;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (_spawner != null && _spawnIndex != -1)
            _spawner.ReleasePoint(_spawnIndex);
    }
}
//public class Rival : NetworkBehaviour
//{
//    [Networked] public NetworkScoreEntry controllingPlayer { get; set; }

//    private EnemySpawner _spawner;
//    private int _spawnIndex = -1;

//    bool lockingObjective = false;
//    float timer = 0f;
//    [SerializeField] private float lookForObjective = 2f;

//    [SerializeField] GameObject bullet;
//    [SerializeField] float shootInterval = 1f;
//    [SerializeField] Transform shootPoint;

//    public void Initialize(EnemySpawner spawner, int index)
//    {
//        _spawner = spawner;
//        _spawnIndex = index;
//    }

//    public void SetScoreEntry(NetworkScoreEntry scoreEntry)
//    {
//        controllingPlayer = scoreEntry;
//    }

//    public override void Despawned(NetworkRunner runner, bool hasState)
//    {
//        if (_spawner != null && _spawnIndex != -1)
//        {
//            _spawner.ReleasePoint(_spawnIndex);
//        }
//    }

//    void Update()
//    {
//        if (!lockingObjective)
//        {
//            LookForObjective();
//        }
//    }

//    public void LookForObjective()
//    {
//        foreach (var rival in Health.allObjectives)
//        {
//            Health closestRival = null;
//            float closestDistance = float.MaxValue;

//            foreach (var rivalPlayer in Health.allObjectives)
//            {
//                float distance = Vector3.Distance(transform.position, rivalPlayer.transform.position);
//                if (distance < closestDistance)
//                {
//                    closestDistance = distance;
//                    closestRival = rivalPlayer;
//                }
//            }

//            if (closestRival != null)
//            {
//                lockingObjective = true;
//                StartCoroutine(LockObjective(closestRival));
//                break;
//            }
//        }
//    }

//    IEnumerator LockObjective(Health closestRival)
//    {
//        while (lockingObjective)
//        {
//            timer += Time.deltaTime;

//            if (timer >= shootInterval)
//            {
//                if (closestRival != null)
//                {
//                    ShootAtObjective(closestRival);
//                    timer = 0f;
//                }
//                else
//                {
//                    lockingObjective = false;
//                }
//            }

//            yield return null;
//        }
//    }

//    void ShootAtObjective(Health target)
//    {
//        if (!lockingObjective) return;

//        Vector3 direction = (target.transform.position - shootPoint.position).normalized;
//        NetworkObject bulletInstance = Runner.Spawn(bullet, shootPoint.position + direction, Quaternion.LookRotation(direction));
//        bulletInstance.GetComponent<Projectile>().SetProjectile(PlayerRef.FromIndex(9), 1);
//        bulletInstance.GetComponent<Rigidbody>().velocity = direction * 120f;
//        Debug.Log("Rival shooting at objective: " + target.name);
//    }
//}
