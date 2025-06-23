// Importamos las librerías necesarias
using TMPro; // Para trabajar con TextMeshPro
using UnityEngine;
using UnityEngine.EventSystems; // Para detectar clics en la interfaz

// El script implementa la interfaz IPointerClickHandler para detectar clics en UI
public class TMP_LinkOpener : MonoBehaviour, IPointerClickHandler
{
    // Variable para guardar la referencia al componente TMP_Text (el texto con el link)
    private TMP_Text tmpText;

    // Awake se llama cuando el objeto se activa por primera vez
    void Awake()
    {
        // Obtenemos el componente TextMeshPro del mismo GameObject
        tmpText = GetComponent<TMP_Text>();
    }

    /// <summary>
    /// Método llamado automáticamente cuando se hace clic sobre el objeto UI asociado.
    /// Detecta si el clic fue sobre un enlace en el texto y, si es así, abre la URL correspondiente en el navegador predeterminado.
    /// </summary>
    /// <param name="eventData">Datos del evento de clic, incluyendo la posición y la cámara del evento.</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        // Busca si el clic fue sobre un enlace en el texto
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(tmpText, eventData.position, eventData.enterEventCamera);

        // Si linkIndex es distinto de -1, significa que hizo clic sobre un link válido
        if (linkIndex != -1)
        {
            // Obtenemos la información del enlace
            TMP_LinkInfo linkInfo = tmpText.textInfo.linkInfo[linkIndex];

            // Extraemos la URL del enlace (del atributo link="...")
            string url = linkInfo.GetLinkID();

            // Abrimos la URL en el navegador predeterminado del sistema
            Application.OpenURL(url);
        }
    }
}

