using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

/// <summary>
/// Clase que controla la visibilidad de un GameObject con efectos de fade y cambia el sprite de un botón según el estado.
/// </summary>
public class BtnScrollHide : MonoBehaviour
{
    /// <summary>
    /// Duración del efecto de fade en segundos.
    /// </summary>
    public float fadeDuration = 0.5f;

    /// <summary>
    /// Componente CanvasGroup para controlar la opacidad del GameObject.
    /// </summary>
    private CanvasGroup canvasGroup;

    /// <summary>
    /// Componente Image del botón para cambiar su sprite.
    /// </summary>
    private Image buttonImage;

    /// <summary>
    /// Nombre del sprite que se usa cuando el estado es "abierto".
    /// </summary>
    private string spriteOpenName = "OpenGallery";

    /// <summary>
    /// Nombre del sprite que se usa cuando el estado es "cerrado".
    /// </summary>
    private string spriteClosedName = "CloseGallery";

    /// <summary>
    /// Diccionario que almacena los sprites cargados dinámicamente.
    /// </summary>
    private Dictionary<string, Sprite> mySpriteDict = new Dictionary<string, Sprite>();

    /// <summary>
    /// Referencia al GameObject que se controla con este script.
    /// </summary>
    [SerializeField] private GameObject content;

    /// <summary>
    /// Referencia al componente SkyBoxButtonSelected asociado al contenido.
    /// </summary>
    private SkyBoxButtonSelected skyBoxButtonSelected;

    /// <summary>
    /// Referencia al componente MaterialTransition para controlar transiciones de skybox.
    /// </summary>
    private MaterialTransition skyboxFader;

    [Header("Animación de Movimiento")]
    /// <summary>
    /// Referencia al RectTransform del panel que se moverá durante la animación.
    /// </summary>
    [SerializeField] private RectTransform panelToMove;

    /// <summary>
    /// Posición del panel cuando está oculto.
    /// </summary>
    [SerializeField] private Vector2 hiddenPosition;

    /// <summary>
    /// Posición del panel cuando está visible.
    /// </summary>
    [SerializeField] private Vector2 shownPosition;

    /// <summary>
    /// Duración de la animación de movimiento del panel.
    /// </summary>
    [SerializeField] private float moveDuration = 0.5f;

    /// <summary>
    /// Inicializa los componentes y carga los sprites necesarios.
    /// </summary>
    private void Start()
    {
        skyboxFader = GameObject.Find("MainCamera").GetComponent<MaterialTransition>();
        canvasGroup = GetComponentInChildren<CanvasGroup>();
        skyBoxButtonSelected = content.GetComponent<SkyBoxButtonSelected>();
        buttonImage = GetComponent<Image>();
        LoadPNGToSprites();

        Sprite spriteOpen = GetSpriteByName(spriteOpenName);
        Sprite spriteClosed = GetSpriteByName(spriteClosedName);

        UpdateButtonSprite(false, spriteOpen, spriteClosed);
    }

    /// <summary>
    /// Alterna la visibilidad de un GameObject con efectos de fade.
    /// </summary>
    /// <param name="go">El GameObject a alternar.</param>
    public void ToggleGameObject(GameObject go)
    {
        Sprite spriteOpen = GetSpriteByName(spriteOpenName);
        Sprite spriteClosed = GetSpriteByName(spriteClosedName);

        if (go.activeSelf)
        {
            if (skyboxFader.flagBtnCarousel == false || skyboxFader.isFading) { return; }
            // Si el objeto está activo, empieza el fade out
            StartCoroutine(FadeOut(go, spriteOpen, spriteClosed));
        }
        else
        {
            if (skyboxFader.flagBtnCarousel == true)
            {
                // Si el objeto está inactivo, empieza el fade in
                go.SetActive(true);  // Primero activa el objeto
                StartCoroutine(FadeIn(go, spriteOpen, spriteClosed)); // Luego realiza el fade in
                skyBoxButtonSelected.carruselListo = false;
                skyBoxButtonSelected.InitializeCarouselRender();
            }
        }
    }

    /// <summary>
    /// Realiza un efecto de fade out en un GameObject y lo desactiva al finalizar.
    /// </summary>
    /// <param name="go">El GameObject a desactivar.</param>
    /// <param name="spriteOpen">Sprite para el estado "abierto".</param>
    /// <param name="spriteClosed">Sprite para el estado "cerrado".</param>
    /// <returns>IEnumerator para la corrutina de fade out.</returns>
    private IEnumerator FadeOut(GameObject go, Sprite spriteOpen, Sprite spriteClosed)
    {
        UpdateButtonSprite(false, spriteOpen, spriteClosed);
        CanvasGroup cg = go.GetComponent<CanvasGroup>() ?? go.AddComponent<CanvasGroup>();

        float startAlpha = cg.alpha;
        float elapsed = 0f;
        Vector2 startPos = shownPosition;
        Vector2 endPos = hiddenPosition;

        panelToMove.anchoredPosition = startPos;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            cg.alpha = Mathf.Lerp(startAlpha, 0f, t);
            panelToMove.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            ApplyAlphaToChildren(go, cg.alpha);
            yield return null;
        }

        cg.alpha = 0f;
        go.SetActive(false);
        panelToMove.anchoredPosition = endPos;
    }

    /// <summary>
    /// Realiza un efecto de fade in en un GameObject.
    /// </summary>
    /// <param name="go">El GameObject a activar.</param>
    /// <param name="spriteOpen">Sprite para el estado "abierto".</param>
    /// <param name="spriteClosed">Sprite para el estado "cerrado".</param>
    /// <returns>IEnumerator para la corrutina de fade in.</returns>
    private IEnumerator FadeIn(GameObject go, Sprite spriteOpen, Sprite spriteClosed)
    {
        UpdateButtonSprite(true, spriteOpen, spriteClosed);
        CanvasGroup cg = go.GetComponent<CanvasGroup>() ?? go.AddComponent<CanvasGroup>();

        float startAlpha = cg.alpha;
        float elapsed = 0f;
        Vector2 startPos = hiddenPosition;
        Vector2 endPos = shownPosition;
        panelToMove.anchoredPosition = startPos;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            cg.alpha = Mathf.Lerp(startAlpha, 1f, t);
            panelToMove.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            ApplyAlphaToChildren(go, cg.alpha);
            yield return null;
        }

        cg.alpha = 1f;
        panelToMove.anchoredPosition = endPos;
    }

    /// <summary>
    /// Aplica la opacidad a todos los hijos de un GameObject.
    /// </summary>
    /// <param name="go">El GameObject cuyos hijos se modificarán.</param>
    /// <param name="alpha">El valor de opacidad a aplicar.</param>
    private void ApplyAlphaToChildren(GameObject go, float alpha)
    {
        foreach (Transform child in go.transform)
        {
            CanvasGroup childCG = child.GetComponent<CanvasGroup>() ?? child.gameObject.AddComponent<CanvasGroup>();
            childCG.alpha = alpha;
        }
    }

    /// <summary>
    /// Obtiene un sprite del diccionario por su nombre.
    /// </summary>
    /// <param name="name">El nombre del sprite a buscar.</param>
    /// <returns>El sprite encontrado o null si no existe.</returns>
    private Sprite GetSpriteByName(string name)
    {
        if (mySpriteDict.TryGetValue(name, out var sprite))
        {
            return sprite;
        }
        else
        {
            Debug.LogWarning($"Sprite '{name}' no encontrado en el diccionario.");
            return null;
        }
    }

    /// <summary>
    /// Actualiza el sprite del botón según el estado del ScrollView.
    /// </summary>
    /// <param name="isActive">Indica si el ScrollView está activo.</param>
    /// <param name="spriteOpen">Sprite para el estado "abierto".</param>
    /// <param name="spriteClosed">Sprite para el estado "cerrado".</param>
    private void UpdateButtonSprite(bool isActive, Sprite spriteOpen, Sprite spriteClosed)
    {
        if (buttonImage != null)
        {
            buttonImage.sprite = isActive ? spriteClosed : spriteOpen;
        }
    }

    /// <summary>
    /// Carga todos los sprites PNG de la carpeta Resources/Icon y los almacena en el diccionario.
    /// </summary>
    private void LoadPNGToSprites()
    {
        // Cambiar la ruta para que apunte a la carpeta "Resources/Icon"
        string folderPath = "Icon"; // La carpeta "Icon" dentro de "Assets/Resources"

        // Cargar todos los recursos en la carpeta "Icon"
        var sprites = Resources.LoadAll<Sprite>(folderPath);

        if (sprites.Length == 0)
        {
            Debug.LogWarning("No se encontraron sprites en la carpeta 'Resources/Icon'.");
        }

        // Recorrer los sprites cargados y agregarlos al diccionario
        foreach (var sprite in sprites)
        {
            if (sprite != null)
            {
                // Guardamos el sprite en el diccionario con su nombre
                mySpriteDict[sprite.name] = sprite;
                Debug.Log("Sprite cargado dinámicamente: " + sprite.name);
            }
            else
            {
                Debug.LogWarning("Sprite es nulo, no se pudo cargar correctamente.");
            }
        }
    }
}