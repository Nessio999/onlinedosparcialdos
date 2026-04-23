using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.UI;



public class Health : NetworkBehaviour
{
    public static List<Health> allObjectives = new List<Health>();
    [SerializeField] private int health;
    [Networked] public int _healt { get; set; } 
    
    public override void Spawned()
    {
        _healt = health; 
        allObjectives.Add(this);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_TakeDamage(int damage, PlayerRef shooter)
    {
        if (!Object.HasStateAuthority) return;
        _healt -= damage;
            Debug.Log($"[Health] Took damage. Current Health: {_healt}");
        
            if (_healt <= 0)
            {
                Debug.Log($"[Health] Object died. Shooter: {shooter}. Checking for score entry...");
                if (NetworkScoreEntry.AllScores.TryGetValue(shooter, out NetworkScoreEntry scoreEntry))
                {
                    Debug.Log($"[Health] Found score entry for {shooter}. Current Score: {scoreEntry.Score}. Adding 1.");
                    scoreEntry.Score += 1;
                }
                else
                {
                    Debug.LogError($"[Health] No se encontró la entrada de puntuación para el jugador {shooter}. Available Keys: {string.Join(", ", NetworkScoreEntry.AllScores.Keys)}");
                }
           OnDeath();
        }
        
    }
    private void OnDeath()
    {
       Runner.Despawn(Object);
       Debug.Log($"{name} ha muerto.");
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (allObjectives.Contains(this))
        {
            allObjectives.Remove(this);
        }
    }

}
