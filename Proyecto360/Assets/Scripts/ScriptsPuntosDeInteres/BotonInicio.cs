using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Clase responsable de gestionar la navegación hacia la escena "Main360".
/// </summary>
public class VolverAMain360 : MonoBehaviour
{
    /// <summary>
    /// Carga la escena llamada "Main360".
    /// </summary>
    public void IrAMain360()
    {
        SceneManager.LoadScene("Main360");
    }
}
