using UnityEngine;


/// <summary>
/// Clase que permite mostrar una imagen específica en un panel al hacer clic en un botón.
/// </summary>
public class MostrarImagenDesdeBoton : MonoBehaviour
{
    /// <summary>
    /// Referencia al controlador del panel de imagen para mostrar la imagen.
    /// Debe ser asignado desde el Inspector.
    /// </summary>
    [SerializeField] private PanelImagenController panelController;

    /// <summary>
    /// La imagen específica que será mostrada por este botón al hacer clic.
    /// Debe ser asignada desde el Inspector.
    /// </summary>
    [SerializeField] private Sprite imagen;

    /// <summary>
    /// Método público llamado por el evento OnClick del botón.
    /// Muestra la imagen en el panel asociado si las referencias están correctamente asignadas.
    /// </summary>
    public void MostrarImagen()
    {
        // Verifica que las referencias necesarias estén asignadas.
        if (panelController != null && imagen != null)
        {
            // Llama al método MostrarPanel del controlador, pasando la imagen.
            panelController.MostrarPanel(imagen);
        }
    }
}
