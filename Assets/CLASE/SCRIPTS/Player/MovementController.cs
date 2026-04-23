using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;
using Unity.VisualScripting;
using Fusion.Addons.KCC;

[RequireComponent(typeof(Rigidbody), typeof(GroundCheck), typeof(KCC))]
public class MovementController : NetworkBehaviour
{
    private InputManager inputManager;
    private Rigidbody rbPlayer;

    [SerializeField] private Animator _animator;
    
    private KCC kcc;

    [Networked] public NetworkScoreEntry PlayerScoreEntry { get; set; }
    
    private void Awake()
    {
      
        rbPlayer = GetComponent<Rigidbody>();
        kcc = GetComponent<KCC>();
    }

    private void Start() {
        CursorManager.Instance.CursorLock();
    }

    public void SetScoreEntry(NetworkScoreEntry entry)
    {
        PlayerScoreEntry = entry;
    }
 
    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
           if(GetInput(out NetworkInputData input))//Aqui, yo debo cersionarme de estar recibiendo el input del servidor. Me consigue el inpurt que manda el serv
           {
             Movement(input);
             UpdateAnimator(input);
           } 
        }

        
            
       

    }
    private void UpdateAnimator(NetworkInputData input)
    {
        
        _animator.SetBool("IsWalking", input.move != Vector2.zero);
        _animator.SetBool("IsRunning", input.isRunning);
        _animator.SetFloat("WalkingZ", input.move.y);
        _animator.SetFloat("WalkingX", input.move.x);
    }

    #region Movimiento

    [SerializeField] private float walkSpeed = 5.5f;
    [SerializeField] private float runSpeed = 7.7f;
    [SerializeField] private float crouchSpeed = 3.9f;

    private void Movement(NetworkInputData input)
    {


        Quaternion realRotation = Quaternion.Euler(0, input.yRotation, 0); //Creanos angulos, solo definiendo Y, que es el que nos interesa
        Vector3 worldDirection = realRotation * new Vector3(input.move.x, 0, input.move.y);
        //rbPlayer.linearVelocity = transform.localRotation * new Vector3(input.move.x, 0, input.move.y);
        
        kcc.SetKinematicVelocity(worldDirection.normalized * (Runner.DeltaTime * Speed(input)));

    }

    private float Speed(NetworkInputData input)
    {
        return input.move.y < 0 || input.move.x !=0 ? walkSpeed :
               input.isRunning ? runSpeed : walkSpeed; 
    }


    #endregion

  
}