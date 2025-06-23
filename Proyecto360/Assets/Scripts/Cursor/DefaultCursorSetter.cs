using UnityEngine;

/// <summary>
/// Establece el cursor predeterminado al iniciar la escena.
/// </summary>
public class DefaultCursorSetter : MonoBehaviour
{
    /// <summary>
    /// Textura que se usará como cursor predeterminado.
    /// </summary>
    [SerializeField] private Texture2D defaultCursor;

    /// <summary>
    /// Punto activo del cursor (hotspot).
    /// </summary>
    [SerializeField] private Vector2 hotSpot = Vector2.zero;

    /// <summary>
    /// Método llamado al iniciar el componente. Establece el cursor predeterminado.
    /// </summary>
    void Start()
    {
        Cursor.SetCursor(defaultCursor, hotSpot, CursorMode.Auto);
    }
}