using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Clase que controla la interacción entre un botón y un panel de texto.
/// Permite mostrar y ocultar un panel de texto al interactuar con el botón.
/// </summary>
public class MostrarTextoDesdeBoton : MonoBehaviour
{
    /// <summary>
    /// Referencia al controlador del panel de texto.
    /// Este campo debe ser asignado en el Inspector.
    /// </summary>
    [SerializeField] private PanelTextoController panelControllerTexto;

    /// <summary>
    /// Referencia al componente Button asociado a este GameObject.
    /// </summary>
    private Button thisButton;

    /// <summary>
    /// Método llamado al inicializar el script.
    /// Obtiene el componente Button de este GameObject.
    /// </summary>
    void Awake()
    {
        // Obtiene el componente Button de este GameObject.
        thisButton = GetComponent<Button>();
    }

    /// <summary>
    /// Método llamado al inicio del script.
    /// Configura el evento onClick del botón y asegura que el panel esté oculto al inicio.
    /// </summary>
    void Start()
    {
        // Asegúrate de que tanto el botón como el controlador del panel estén asignados en el Inspector.
        if (thisButton != null && panelControllerTexto != null)
        {
            // Añade un listener al evento onClick del botón.
            // Cuando se haga clic en el botón, se llamará al método MostrarPanel() del panelControllerTexto.
            thisButton.onClick.AddListener(panelControllerTexto.MostrarPanel);

            // Opcional pero útil: Asegurarse de que el panel esté desactivado al inicio del juego.
            // Esto previene que el panel aparezca inmediatamente al cargar la escena si no es deseado.
            if (panelControllerTexto.gameObject.activeSelf)
            {
                panelControllerTexto.CerrarPanel();
            }
        }
    }

    /// <summary>
    /// Método público para ocultar el panel de texto.
    /// Puede ser llamado por otros scripts o eventos (como un botón "Cerrar" en la UI).
    /// </summary>
    public void OcultarPanelTexto()
    {
        if (panelControllerTexto != null)
        {
            panelControllerTexto.CerrarPanel();
        }
    }
}
