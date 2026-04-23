using Fusion;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Wepon : NetworkBehaviour
{
    [SerializeField] protected ShootType type;
    [SerializeField] protected Transform shootPoint;
    [SerializeField] protected NetworkPrefabRef bullet;
    [SerializeField] protected Camera playerCam;
    [SerializeField] protected LayerMask layerMask;

    [SerializeField] protected int damage;
    [SerializeField] protected float range;
    [SerializeField] protected int actualAmmo;
    public abstract void RigidBodyShoot();
    public abstract void RpcRaycastShoot(RpcInfo info = default);
}
public enum ShootType
{
    RigidBody,Raycast
    
}
