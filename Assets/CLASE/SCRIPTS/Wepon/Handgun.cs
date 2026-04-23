using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using UnityEngine;

public class Handgun : Wepon
{

    //LayerMask
    //Range
    //Un RPC es un protocolo para mandar allamar uin metodo en diferentes clientes
    //RpcSoces es quien leo manda a llmar 
    //RpcTargetes es quien lo ejecuta
    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public override void RpcRaycastShoot(RpcInfo info = default)
    {
      if(Physics.Raycast(playerCam.transform.position, playerCam.transform.forward,out RaycastHit hit, range,layerMask))
        {
            Debug.Log(hit.collider.name);
            if (hit.collider.TryGetComponent(out Health health))
            {
                health.Rpc_TakeDamage((int)damage, info.Source);
            }
            else
            {
                //Hacer aparecer un agujero de bala
            }
        }
    }

    public override void RigidBodyShoot()
    {
        if (Object.HasStateAuthority)
        {
            Debug.Log($"[Handgun] Server Spawning Bullet. InputAuthority: {Object.InputAuthority}");
            NetworkObject bulletInstance = Runner.Spawn(bullet, shootPoint.position, shootPoint.rotation, Object.InputAuthority);
            bulletInstance.GetComponent<Projectile>().SetProjectile(Object.InputAuthority, damage);
            
            
            Collider playerCollider = GetComponentInParent<Collider>();
            if (playerCollider != null && bulletInstance.TryGetComponent(out Collider bulletCollider))
            {
                Physics.IgnoreCollision(playerCollider, bulletCollider);
            }
        }
    }
    
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(playerCam.transform.position, playerCam.transform.forward * range);
    }
}
