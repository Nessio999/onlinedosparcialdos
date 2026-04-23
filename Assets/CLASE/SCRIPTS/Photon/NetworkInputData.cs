using Fusion;
using UnityEngine;

//Esta estryctyra debe cibteber TIDIS kis vakires qye se vab a nabdar
//a el servidor. Ojo unicamente los valores
//Esto deber de heredar de ItNetworkInput para  que la estrucutra se
//por el servidor como una serie de inputs a leer 
public struct NetworkInputData : INetworkInput
{
    public Vector2 move;
    public Vector2 look;
    public bool isRunning;

    public float yRotation;
    public float xRotation;

    public bool shoot;


}

