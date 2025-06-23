using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Componente que gestiona la navegación entre vistas mediante botones, mostrando tooltips y
/// adaptando el comportamiento según la plataforma (Android/WebGL/PC).
/// </summary>
[System.Serializable]
public class BotonNavegacion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tooltip")]
    [SerializeField, Tooltip("Texto TMP que se mostrará encima del botón")]
    private TMP_Text tooltipText;

    [SerializeField] private GameObject content; // El GameObject que contiene el script SkyBoxButtonSelected
    [SerializeField] private GameObject contentAndroid; // El GameObject que contiene el script SkyBoxButtonSelected
    private GameObject finalContent;
    private SceneControllers sceneControllers;
    private string nombreBoton;

    /// <summary>
    /// Inicializa el componente, asigna el listener al botón y configura el tooltip y el contenedor según la plataforma.
    /// </summary>
    private void Start()
    {
        // Buscar SceneControllers
        sceneControllers = FindObjectOfType<SceneControllers>();
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(CambiarVista);
        }
        else
        {
            Debug.LogWarning("Este objeto necesita un componente Button para usar BotonNavegacion.");
        }

        // Desactiva el tooltip al inicio
        if (tooltipText != null)
        {
            tooltipText.gameObject.SetActive(false);
        }

        if (Application.platform == RuntimePlatform.Android)
        {
            finalContent = contentAndroid;
        }
        else if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            finalContent = content;
        }
        else
        {
            finalContent = content;
        }
    }

    /// <summary>
    /// Cambia la vista, actualiza el Skybox, activa/desactiva elementos de UI y oculta el tooltip.
    /// </summary>
    private void CambiarVista()
    {
        // Llama a SkyBoxButtonSelected para subrayar el botón del carrusel
        SkyBoxButtonSelected skyboxButtonHandler = finalContent.GetComponent<SkyBoxButtonSelected>();
        if (skyboxButtonHandler.Bandera() == false)
        {
            if ((skyboxButtonHandler != null && !skyboxButtonHandler.carruselListo))
            {
                Debug.LogWarning("El carrusel aún no está listo. Ignorando clic.");
                return; // Evitamos que se ejecute el cambio si el carrusel no está listo
            }
        }
        Texture texturaSeleccionada = null;

        if (sceneControllers != null)
        {
            nombreBoton = gameObject.name;
            texturaSeleccionada = sceneControllers.views.Find(t => t.name == nombreBoton);
        }

        if (texturaSeleccionada != null)
        {
            MaterialTransition materialTransition = Camera.main.GetComponent<MaterialTransition>();
            if (materialTransition != null)
            {
                materialTransition.StartTransition(texturaSeleccionada);
                skyboxButtonHandler.OnSkyboxChanged(NormalizeTextureName(nombreBoton));
            }
        }
        else
        {
            Debug.LogWarning($"No se encontró una textura con nombre: {nombreBoton}");
        }

        if (tooltipText != null)
        {
            tooltipText.gameObject.SetActive(false);
            tooltipText.text = "";
        }
    }

    /// <summary>
    /// Muestra el nombre del material en el tooltip al pasar el ratón sobre el botón.
    /// </summary>
    /// <param name="eventData">Datos del evento del puntero.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipText != null)
        {
            string nombreNormalizado = NormalizeTextureName(gameObject.name);
            tooltipText.text = nombreNormalizado.Substring(2); // Muestra el nombre del material
            tooltipText.gameObject.SetActive(true);   // Activa el tooltip
        }
    }

    /// <summary>
    /// Oculta el tooltip al quitar el ratón del botón.
    /// </summary>
    /// <param name="eventData">Datos del evento del puntero.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipText != null)
        {
            tooltipText.text = ""; // Borra el texto
            tooltipText.gameObject.SetActive(false); // Desactiva el tooltip
        }
    }

    /// <summary>
    /// Inicializa las referencias a los contenedores de contenido para las diferentes plataformas.
    /// </summary>
    /// <param name="content">Contenedor para plataformas no Android.</param>
    /// <param name="contentAndroid">Contenedor para Android.</param>
    public void Inicializar(GameObject content, GameObject contentAndroid)
    {
        this.content = content;
        this.contentAndroid = contentAndroid;
    }

    /// <summary>
    /// Normaliza el nombre de la textura eliminando el primer carácter si hay un guion bajo en la posición 2.
    /// </summary>
    /// <param name="name">Nombre original de la textura.</param>
    /// <returns>Nombre normalizado de la textura.</returns>
    private string NormalizeTextureName(string name)
    {
        int underscoreIndex = name.IndexOf('_');
        if (underscoreIndex == 2)
        {
            // Dos letras antes del _
            return name.Substring(1); // Quita la primera letra
        }
        return name; // Lo deja igual
    }
}
