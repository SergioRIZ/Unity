using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneLoader : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Arrastra la escena aquí")]
    public SceneAsset sceneToLoad;
#endif

    [HideInInspector]
    public string sceneName;

    /// <summary>
    /// Se llama cuando se modifica el componente en el editor.
    /// Asigna el nombre de la escena seleccionada a la variable <c>sceneName</c>.
    /// </summary>
    private void OnValidate()
    {
#if UNITY_EDITOR
        if (sceneToLoad != null)
        {
            sceneName = sceneToLoad.name;
        }
#endif
    }

    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("No se ha asignado ninguna escena al SceneLoader.");
        }
    }
}
