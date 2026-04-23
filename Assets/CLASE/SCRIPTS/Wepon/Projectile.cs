using UnityEngine;
using Fusion;
using System.Threading.Tasks;

public class Projectile : NetworkBehaviour
{

    [SerializeField] private float speed;
    [Networked] public int damage { get; set; }
    [SerializeField] private int lifeTime;
    

    [Networked] PlayerRef shooter { get; set; }

    Rigidbody rb;
    
    public override void Spawned()
    {
        rb = GetComponent<Rigidbody>();

        rb.linearVelocity = transform.forward * speed;

        DespawnedAfterTime();
    }

    private async void DespawnedAfterTime()
    {
        await Task.Delay(lifeTime * 1000);
        if (Object != null && Runner != null)
        {
            Runner.Despawn(Object);
        }
    }

    public void SetProjectile(PlayerRef shooter, int damage, bool customDirection = false)
    {
       this.shooter = shooter;
       this.damage = damage;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (Object == null || !Object.HasStateAuthority) return;
        
        if (collision.collider.TryGetComponent(out Health health))
        {
            Debug.Log("Se intenta aplicar : " + damage + " de daño a " + collision.collider.name);
            health.Rpc_TakeDamage(damage, shooter);
        }
        else
        {
            Debug.Log("No tiene componente de vida");
        }
        
        if (Runner != null)
        {
            Runner.Despawn(Object);
        }
    }
}
