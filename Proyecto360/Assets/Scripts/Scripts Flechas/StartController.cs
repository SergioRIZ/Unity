using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlador para gestionar el comportamiento inicial de los elementos del juego.
/// </summary>
public class StartController : MonoBehaviour
{
    //public int numberSpheres;

    /// <summary>
    /// Arreglo de objetos Canvas que serán desactivados al inicio.
    /// </summary>
    [SerializeField] GameObject[] objetoActivo;
    [SerializeField] GameObject[] arrayCanvas;

    /// <summary>
    /// Método llamado automáticamente por Unity al iniciar el ciclo de vida del script.
    /// Activa todos los objetos especificados en <see cref="objetoActivo"/> y desactiva todos los objetos en <see cref="arrayCanvas"/>.
    /// </summary>
    void Start()
    {
        //Nos aseguramos de activar el primer objeto(contiene este script)
        foreach (GameObject objActivar in objetoActivo)
        {
            objActivar.SetActive(true);
        }

        //Se desactivan todos los objetos menos el primero
        foreach (GameObject objDesactivar in arrayCanvas)
        {
            objDesactivar.SetActive(false);
        }
    }

}
