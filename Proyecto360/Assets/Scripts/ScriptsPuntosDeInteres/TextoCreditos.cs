using UnityEngine;

/// <summary>
/// Controla el desplazamiento vertical del texto de créditos en pantalla.
/// </summary>
public class ScrollCreditos : MonoBehaviour
{
    /// <summary>
    /// Velocidad a la que el texto se desplaza hacia arriba (unidades por segundo).
    /// </summary>
    public float velocidad = 30f;

    /// <summary>
    /// Actualiza la posición del texto cada frame, moviéndolo hacia arriba según la velocidad establecida.
    /// </summary>
    void Update()
    {
        transform.Translate(Vector3.up * velocidad * Time.deltaTime);
    }
}
