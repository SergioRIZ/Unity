using UnityEngine;

/// <summary>
/// Componente que rota el GameObject al que está adjunto alrededor de un eje especificado a una velocidad determinada.
/// </summary>
public class Rotator : MonoBehaviour
{
    /// <summary>
    /// Velocidad de rotación en grados por segundo.
    /// </summary>
    [Tooltip("Velocidad de rotación en grados por segundo.")]
    public float rotationSpeed = 10f; // Puedes ajustar la velocidad en el Inspector.

    /// <summary>
    /// Eje alrededor del cual el objeto rotará.
    /// </summary>
    [Tooltip("Eje alrededor del cual el objeto rotará.")]
    public Vector3 rotationAxis = Vector3.up; // Por defecto, rota alrededor del eje Y (hacia arriba).

    /// <summary>
    /// Llama a <see cref="Transform.Rotate(Vector3, float)"/> cada frame para rotar el objeto.
    /// </summary>
    void Update()
    {
        // Rota el objeto en cada frame.
        // Time.deltaTime asegura que la rotación sea suave y dependa del tiempo, no de los frames.
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
    }
}
