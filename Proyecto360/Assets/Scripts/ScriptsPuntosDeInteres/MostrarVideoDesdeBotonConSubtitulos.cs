using UnityEngine;
using UnityEngine.Video;
using System.IO;

public class MostrarVideoDesdeBotonConSubtitulos : MonoBehaviour
{
    [SerializeField] private PanelVideoControllerConSubtitulos panelVideoController;
    [SerializeField] private string videoFileName;

    /// <summary>
    /// Muestra el video especificado en el panel de video con subtítulos.
    /// Construye la ruta del video desde la carpeta StreamingAssets/Videos y solicita al panel que lo muestre.
    /// Si faltan referencias necesarias, muestra una advertencia en la consola.
    /// </summary>
    public void MostrarVideo()
    {
        if (panelVideoController != null && !string.IsNullOrEmpty(videoFileName))
        {
            // Construir la ruta del video desde StreamingAssets
            string videoPath = Path.Combine(Application.streamingAssetsPath, "Videos", videoFileName);
            Debug.Log($"Cargando video desde: {videoPath}");
            panelVideoController.MostrarPanelDesdeRuta(videoPath);
        }
        else
        {
            Debug.LogWarning("Faltan referencias: panelVideoController o videoFileName no asignado.");
        }
    }
}