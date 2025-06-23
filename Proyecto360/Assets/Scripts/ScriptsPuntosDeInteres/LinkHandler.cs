using UnityEngine;

/// <summary>
/// Proporciona funcionalidad para abrir URLs desde la aplicación.
/// </summary>
public class LinkHandler : MonoBehaviour
{
    /// <summary>
    /// Abre la URL especificada en el navegador predeterminado del sistema.
    /// </summary>
    /// <param name="url">La URL que se desea abrir.</param>
    public void OpenURL(string url)
    {
        Application.OpenURL(url);
    }
}