using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Componente que permite abrir enlaces incrustados en un <see cref="TextMeshProUGUI"/> al hacer clic sobre ellos.
/// </summary>
public class LinkOpener : MonoBehaviour, IPointerClickHandler
{
    /// <summary>
    /// Referencia al componente <see cref="TextMeshProUGUI"/> que contiene los enlaces.
    /// </summary>
    public TextMeshProUGUI textMeshPro;

    /// <summary>
    /// Maneja el evento de clic del puntero sobre el objeto.
    /// Si el clic ocurre sobre un enlace, abre la URL asociada en el navegador predeterminado.
    /// </summary>
    /// <param name="eventData">Datos del evento de clic del puntero.</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textMeshPro, Input.mousePosition, null);
        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = textMeshPro.textInfo.linkInfo[linkIndex];
            string url = linkInfo.GetLinkID();
            Application.OpenURL(url);
        }
    }
}
