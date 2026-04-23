using Fusion;
using UnityEngine;

public class WeponHandler : NetworkBehaviour
{
  [SerializeField] private Wepon actualWepon;
    
    
    

    public override void FixedUpdateNetwork()
    {
        if(GetInput(out NetworkInputData input))
        {
            if(input.shoot)
            {
                actualWepon.RigidBodyShoot();
                Debug.Log("Disparando desde WeponHandler");
            }
        }
    }
}
