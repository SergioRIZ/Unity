using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Clase que gestiona la selección de botones en un carrusel de Skybox, aplicando efectos visuales y activando el canvas correspondiente.
/// </summary>
public class SkyBoxButtonSelected : MonoBehaviour
{
    /// <summary>
    /// Nombre de la textura actual del Skybox.
    /// </summary>
    private string currentTextureName;

    /// <summary>
    /// Bandera para evitar múltiples ejecuciones mientras se espera un retraso.
    /// </summary>
    private bool isWaitingForDelay = false;

    /// <summary>
    /// Contenedor de los grupos de canvas que se activan/desactivan según la textura seleccionada.
    /// </summary>
    [SerializeField] private Transform canvasGroupParent;

    /// <summary>
    /// Indica si el carrusel está listo para interactuar.
    /// </summary>
    public bool carruselListo = false;

    /// <summary>
    /// Color que se aplica al nombre del botón seleccionado.
    /// </summary>
    [SerializeField] private Color nombreDavante = Color.blue;

    /// <summary>
    /// Inicializa el componente al iniciar la escena.
    /// </summary>
    void Start()
    {

    }

    /// <summary>
    /// Devuelve una bandera que indica si el objeto está activo en la jerarquía.
    /// </summary>
    /// <returns>True si el objeto NO está activo en la jerarquía, false si está activo.</returns>
    public bool Bandera()
    {
        if (gameObject.activeInHierarchy)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// Inicializa el renderizado del carrusel y resalta el Skybox actual tras un frame.
    /// </summary>
    public void InitializeCarouselRender()
    {
        StartCoroutine(InitializeSkyboxAfterFrame());
    }

    /// <summary>
    /// Corrutina que espera un frame para asegurar que la escena esté cargada antes de inicializar el Skybox.
    /// </summary>
    /// <returns>IEnumerator para la corrutina.</returns>
    private IEnumerator InitializeSkyboxAfterFrame()
    {
        yield return null; // Espera un frame para que la escena se cargue correctamente

        // Inicializamos el nombre de la textura actual
        Material currentSkyboxMaterial = RenderSettings.skybox;
        Texture currentTex = currentSkyboxMaterial.GetTexture("_MainTex");
        currentTextureName = currentTex != null ? currentTex.name : "";

        HighlightCurrentSkyboxButton();
        Debug.Log("Se inica el hightlight inicial");
    }

    /// <summary>
    /// Resalta el botón correspondiente al Skybox actual y activa el canvas correspondiente.
    /// </summary>
    public void HighlightCurrentSkyboxButton()
    {
        ActivateMatchingCanvasOnly();

        // Si ya estamos esperando, no ejecutar de nuevo
        if (isWaitingForDelay) return;

        // Ejecutamos la corrutina si el objeto está activo
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(ApplyEffectAndWait());
        }
        else
        {
            Debug.Log("El carrusel está desactivado");
        }
    }

    /// <summary>
    /// Corrutina que aplica efectos visuales al botón seleccionado y espera un retraso antes de permitir otra selección.
    /// </summary>
    /// <returns>IEnumerator para la corrutina.</returns>
    private IEnumerator ApplyEffectAndWait()
    {
        ShowSelectedPosition carouselScrollController = GetComponent<ShowSelectedPosition>();

        isWaitingForDelay = true; // Activamos la espera

        // Aplicar el efecto en el botón seleccionado
        foreach (Transform btn in transform)
        {
            TextMeshProUGUI tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
            CanvasTextSelected canvasTextSelected = btn.GetComponent<CanvasTextSelected>();
            Image iconImage = null;
            foreach (Image img in btn.GetComponentsInChildren<Image>(true))
            {
                if (img.gameObject.name == "Icon")
                {
                    iconImage = img;
                    break;
                }
            } // Imagen visible dentro del Icon
            RectTransform btnRectTransform = btn.GetComponent<RectTransform>(); // RectTransform del botón

            if (tmp != null && canvasTextSelected != null)
            {
                // Eliminamos subrayado temporal y establecemos el permanente
                tmp.fontStyle = tmp.fontStyle & ~FontStyles.Underline;

                // Establecemos subrayado permanente según la textura del Skybox
                if (tmp.text == currentTextureName.Substring(2))
                {
                    canvasTextSelected.SetPermanentUnderline(true);
                }
                else
                {
                    canvasTextSelected.SetPermanentUnderline(false);
                }
            }

            // Si el icono es el correcto (el seleccionado)
            if (tmp != null && tmp.text == currentTextureName.Substring(2))
            {
                // Escalar un poco la imagen del icono, pero sin distorsionarla
                RectTransform iconRectTransform = iconImage.GetComponent<RectTransform>();
                if (Application.platform == RuntimePlatform.Android)
                {
                    iconRectTransform.localScale = new Vector3(1.05f, 1.05f, 1); // Escala ligeramente la imagen
                    btnRectTransform.localScale = new Vector3(1.25f, 2.13f, 1); // Escala ligeramente el botón
                    tmp.color = nombreDavante;
                }
                else if (Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    iconRectTransform.localScale = new Vector3(1.05f, 1.05f, 1); // Escala ligeramente la imagen
                    btnRectTransform.localScale = new Vector3(0.50f, 2.27f, 1); // Escala ligeramente el botón
                    tmp.color = nombreDavante;

                    // Centrar el scroll en el botón
                    if (carouselScrollController != null && gameObject.activeSelf)
                    {
                        carouselScrollController.CenterOnButton(tmp.text);
                    }
                }
                else
                {
                    iconRectTransform.localScale = new Vector3(1.05f, 1.05f, 1); // Escala ligeramente la imagen
                    btnRectTransform.localScale = new Vector3(0.50f, 2.27f, 1); // Escala ligeramente el botón
                    tmp.color = nombreDavante;
                    // Centrar el scroll en el botón
                    if (carouselScrollController != null && gameObject.activeSelf)
                    {
                        carouselScrollController.CenterOnButton(tmp.text);
                    }
                }
            }
            else
            {
                // Restaurar tamaño de la imagen si no está seleccionada
                if (iconImage != null)
                {
                    RectTransform iconRectTransform = iconImage.GetComponent<RectTransform>();
                    if (Application.platform == RuntimePlatform.Android)
                    {
                        iconRectTransform.localScale = new Vector3(1f, 1f, 1); // Escala ligeramente la imagen
                        btnRectTransform.localScale = new Vector3(1.15f, 2.03f, 1); // Escala ligeramente el botón
                        tmp.color = nombreDavante;
                    }
                    else if (Application.platform == RuntimePlatform.WebGLPlayer)
                    {
                        iconRectTransform.localScale = new Vector3(1f, 1f, 1); // Escala normal
                        btnRectTransform.localScale = new Vector3(0.45f, 2.03f, 1); // Restaurar tamaño normal
                        tmp.color = Color.white;
                    }
                    else
                    {
                        iconRectTransform.localScale = new Vector3(1f, 1f, 1); // Escala normal
                        btnRectTransform.localScale = new Vector3(0.45f, 2.03f, 1); // Restaurar tamaño normal
                        tmp.color = Color.white;
                    }
                }
            }
        }

        // Esperamos 1.5 segundos antes de permitir otra selección de botón
        yield return new WaitForSeconds(1.5f);

        // Restablecer la flag para permitir seleccionar un nuevo botón
        isWaitingForDelay = false;
        carruselListo = true;
    }

    /// <summary>
    /// Método que se llama desde el SceneManager para actualizar el nombre de la textura actual.
    /// </summary>
    /// <param name="newTextureName">El nuevo nombre de la textura del Skybox.</param>
    public void OnSkyboxChanged(string newTextureName)
    {
        currentTextureName = newTextureName;
        Debug.Log(currentTextureName);
        HighlightCurrentSkyboxButton();
    }

    /// <summary>
    /// Activa únicamente el canvas cuyo nombre coincide con la textura actual del Skybox y gestiona la activación de los botones internos.
    /// </summary>
    public void ActivateMatchingCanvasOnly()
    {
        foreach (Transform child in canvasGroupParent)
        {
            Debug.Log(child.name);
            // Comparamos el nombre del GameObject con el nombre de la textura
            bool isMatch = child.name == currentTextureName.Substring(2);

            // Activamos el 'child' solo si hay una coincidencia
            child.gameObject.SetActive(isMatch);

            // Si el 'child' coincide, activamos todos los botones dentro de él, independientemente de la jerarquía
            if (isMatch)
            {
                // Obtiene todos los botones dentro de los sub-objetos del 'child'
                Button[] buttons = child.GetComponentsInChildren<Button>(true); // 'true' incluye botones desactivados
                foreach (Button button in buttons)
                {
                    button.interactable = true;  // Activa el botón, haciendo que sea interactuable
                    button.gameObject.SetActive(true); // Asegúrate de que el botón esté visible
                }
            }
            else
            {
                // Si el 'child' no coincide, desactivamos todos los botones dentro de él
                Button[] buttons = child.GetComponentsInChildren<Button>(true);
                foreach (Button button in buttons)
                {
                    button.interactable = false;  // Desactiva el botón
                    button.gameObject.SetActive(false); // Desactiva la visibilidad del botón
                }
            }
        }
    }
}
