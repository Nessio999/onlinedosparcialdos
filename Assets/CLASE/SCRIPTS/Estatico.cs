using UnityEngine;

public class Estatico : MonoBehaviour
{
    public static int numero = 50;
   public static void MensajeDeError(string mensaje)
   {
       Debug.LogError(mensaje);
   }

   public static void MensajeDeAdvertencia(string mensaje)
   {
       Debug.LogWarning(mensaje);
   }

   public static int Sumar (int num1, int num2)
   {
       return num1 + num2;
   }
}
