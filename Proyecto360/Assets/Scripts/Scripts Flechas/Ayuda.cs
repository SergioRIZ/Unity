using UnityEngine;
using UnityEngine.UI;

public class Ayuda : MonoBehaviour
{
    // Referencia al panel que se va a mostrar u ocultar
    [SerializeField] private GameObject fullScreenPanel;

    // Botón que actúa como interruptor (abrir/cerrar)
    [SerializeField] private Button openPanelButton;

    // Botón adicional que cierra el panel (opcional)
    [SerializeField] private Button closeButton;

    /// <summary>
    /// Inicializa el estado del panel de ayuda y configura los eventos de los botones.
    /// </summary>
    /// <remarks>
    /// Al iniciar la escena, el panel de ayuda se oculta. El botón <c>openPanelButton</c> alterna la visibilidad del panel.
    /// Si existe <c>closeButton</c>, este cierra el panel al hacer clic.
    /// </remarks>
    void Start()
    {
        // Ocultar el panel al iniciar la escena
        fullScreenPanel.SetActive(false);

        // Toggle: si está activo, lo oculta; si está oculto, lo muestra
        openPanelButton.onClick.AddListener(() =>
        {
            fullScreenPanel.SetActive(!fullScreenPanel.activeSelf);
        });

        // Cierre directo del panel desde el botón de cerrar
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(() =>
            {
                fullScreenPanel.SetActive(false);
            });
        }
    }
}
