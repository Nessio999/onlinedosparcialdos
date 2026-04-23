using System;
using Fusion;
using Fusion.Addons.KCC;
using UnityEngine;
using UnityEngine.Serialization;


public class CameraController : NetworkBehaviour
{
    [Networked] public float camY { get; set; }

    [Header("Camera Settings")] 
    [SerializeField]
    private Transform player;

    [SerializeField] private float mouseSensitivity = 1;

    [FormerlySerializedAs("smooth")] 
    [SerializeField]
    private float smoothnes;

    [SerializeField] private float maxAngleY = 80;
    [SerializeField] private float minAngleY = -80;

    private Vector2 camVelociy;
    private Vector2 smoothVelocity;

    [Header("Blob Movement")] 
    [SerializeField] private float walkingSpeed = 1f;
    
    [SerializeField, Range(0,0.1f)] private float walkingAmplitude = 0.015f; // Que tanto se mueve hacia los lados al caminar
    [SerializeField, Range(0,0.1f)] private float runningAmplitude = 0.015f; // Que tanto se mueve hacia los lados al correr
    [SerializeField, Range(0,15)] private float walkingFrequency = 10.0f; // La frecuencia con la que se mueve al caminar
    [SerializeField, Range(10,20)] private float runningFrequency = 18f; // La frecuencia con la que se mueve al correr
    [SerializeField] private float resetPosSpeed = 3.0f; // Cuando dejas de moverte que regrese al centro
    [SerializeField] private float toggleSpeed = 3.0f; // 
    
    private Vector3 startPos; // Posicion inicial de la cabeza , el centro

    [SerializeField] private bool moveHead;
    
    private Vector2 head;
    
    private InputManager inputManager;

    private KCC kcc;
    
    private void Awake()
    {
        startPos = transform.localPosition;
    }
    
    private void Start()
    {
        inputManager = InputManager.Instance;
        if (player == null)
        {
            player = FindObjectOfType<MovementController>().transform;
        }

        kcc = player.GetComponent<KCC>();
        Cursor.lockState = CursorLockMode.None;
        //Cursor.visible = false;
        
        
        
    }

    public override void Spawned()
    {
        if (player == null)
        {
            player = transform.parent;
            if (player == null) player = transform.root;
        }

        if (player != null)
        {
            kcc = player.GetComponent<KCC>();
        }

        if (kcc == null)
        {
            kcc = GetComponentInParent<KCC>();
        }

        if (!HasInputAuthority)
        {
            GetComponent<Camera>().enabled = false;
            GetComponent<AudioListener>().enabled = false;
        }
    }

    public override void FixedUpdateNetwork()
    {
       //if(HasInputAuthority)
       
           if(GetInput(out NetworkInputData input))
           {
               RotateCamera(input);
           }
       
    }
    public override void Render()
    {
        if (!HasInputAuthority && !HasStateAuthority)
        {
            transform.localRotation = Quaternion.AngleAxis(-camY, Vector3.right);
        }
        
        
    }
    private void RotateCamera(NetworkInputData input)
    {
        Vector2 rawFrameVelocity = Vector2.Scale(input.look, Vector2.one * mouseSensitivity);
        smoothVelocity = Vector2.Lerp(smoothVelocity, rawFrameVelocity, 1 / smoothnes); // Te mueve desde donde tengas el mouse, a la nueva posicion del mouse
        camVelociy += smoothVelocity;
        camVelociy.y = Mathf.Clamp(camVelociy.y, minAngleY, maxAngleY); // Limita la rotacion de la camara en Y. En base el movimiento del mouse.

        if (!HasInputAuthority)
        {
             camVelociy.y = input.xRotation;
        }

        
        //Mi calculo de rotacion se hace local, pero mi rotacion va ser por Networking
        camY = camVelociy.y;

        
        transform.localRotation = Quaternion.AngleAxis(-camVelociy.y, Vector3.right); // Rota la camara hacia arriba y abajo. La rotacion esta en X. 
        //player.localRotation = Quaternion.AngleAxis(camVelociy.x, Vector3.up);
        
        if (kcc != null)
        {
            kcc.SetLookRotation(Quaternion.AngleAxis(camVelociy.x, Vector3.up)); // Le decimos al KCC que rote el personaje en X, segun el movimiento del mouse
        }
    }
    
   
private void BlobMove()
{
    if (!inputManager.IsMoveInputPressed()) return;

    transform.localPosition += 
        inputManager.IsMovingBackwards() || inputManager.IsMovingOnXAxis()
        ? FootStepMotion()
        : inputManager.WasRunInputPressed()
            ? RunningFootStepMotion()
            : FootStepMotion();
}
  

    private void ResetPosition()
    {
        if(transform.localPosition == startPos) return; // Si la camara ya esta en la pos inicial, no hace nada
        transform.localPosition = Vector3.Lerp(transform.localPosition, startPos, resetPosSpeed * Time.deltaTime);
}

    private Vector3 FootStepMotion()
    {
        Vector3 pos = Vector3.zero;
        pos.y = Mathf.Sin(Time.time * walkingFrequency) * walkingAmplitude * walkingSpeed;
        pos.x = Mathf.Cos(Time.time * walkingFrequency / 2) * walkingAmplitude * 2 * walkingSpeed;
        return pos;
    }
    
    
    private Vector3 RunningFootStepMotion()
    {
        Vector3 pos = Vector3.zero;
        pos.y = Mathf.Sin(Time.time * runningFrequency) * runningAmplitude * walkingSpeed;
        pos.x = Mathf.Cos(Time.time * runningFrequency / 2) * runningAmplitude * 2 * walkingSpeed;
        return pos;
    }
    
}