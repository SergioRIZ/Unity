using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controlador principal para gestionar la lógica de la escena, incluyendo la carga de imágenes,
/// creación de botones y transiciones de skybox.
/// </summary>
public class SceneControllers : MonoBehaviour
{
    // El directorio donde se encuentran las imágenes
    private string imagesFolder;

    /// <summary>
    /// Lista de texturas precargadas para las vistas del skybox.
    /// </summary>
    public List<Texture> views;

    /// <summary>
    /// Contenedor de botones para la interfaz de Android.
    /// </summary>
    [SerializeField] private GameObject androidCanvas;

    /// <summary>
    /// Contenedor de botones para la interfaz de PC.
    /// </summary>
    [SerializeField] private GameObject pcCanvas;

    /// <summary>
    /// Prefab del botón que se instanciará dinámicamente.
    /// </summary>
    [SerializeField] private GameObject botonPrefab;

    /// <summary>
    /// Contenedor donde se agregarán los botones según la plataforma.
    /// </summary>
    private GameObject contentView;

    /// <summary>
    /// Componente encargado de realizar transiciones de materiales en el skybox.
    /// </summary>
    private MaterialTransition skyboxFader;

    /// <summary>
    /// Referencia al controlador de galería para Android.
    /// </summary>
    private ShowGaleryAndroid showGaleryAndroid;

    /// <summary>
    /// Referencia al controlador de la cámara principal.
    /// </summary>
    private CameraView cameraView;

    //private int contador;

    /// <summary>
    /// Método llamado al inicializar el script. Configura la plataforma, carga imágenes y prepara el skybox.
    /// </summary>
    void Awake()
    {
        // Elegir el contenedor según la plataforma en tiempo de ejecución
        if (Application.platform == RuntimePlatform.Android)
        {
            androidCanvas.SetActive(true);
            pcCanvas.SetActive(false);
            contentView = androidCanvas.transform.Find("Scroll View Android/Viewport Android/Content Android").gameObject;
            Debug.Log("Entrando en Android");
            showGaleryAndroid = GameObject.Find("BtnAbrirGalery").GetComponent<ShowGaleryAndroid>();
        }
        else if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            androidCanvas.SetActive(false);
            pcCanvas.SetActive(true);
            contentView = pcCanvas.transform.Find("Scroll View/Viewport/Content").gameObject;
            Debug.Log("Entrando en WebGL");
        }
        else
        {
            // Si estás en otro entorno como Editor, se puede configurar de una manera predeterminada
            androidCanvas.SetActive(false);
            pcCanvas.SetActive(true);
            contentView = pcCanvas.transform.Find("Scroll View/Viewport/Content").gameObject;
            Debug.Log("Entrando en Editor o plataforma no soportada");
        }

        string sceneName = SceneManager.GetActiveScene().name;
        imagesFolder = "CarouselImages/" + sceneName;
        // Obtener el componente para transiciones de material
        skyboxFader = GameObject.Find("MainCamera").GetComponent<MaterialTransition>();
        //Obtenemos el script de cameraView
        cameraView = GameObject.Find("MainCamera").GetComponent<CameraView>();

        if (skyboxFader == null)
        {
            Debug.LogError("MaterialTransition no encontrado en la cámara principal.");
            return;
        }

        // Inicializar la lista de vistas
        views = new List<Texture>();

        // Cargar todas las texturas directamente desde Resources
        Texture2D[] textures = Resources.LoadAll<Texture2D>(imagesFolder);
        Debug.Log("texture lenght es: " + textures.Length);

        foreach (Texture2D texture in textures)
        {
            if (texture == null) continue;

            views.Add(texture);
            CreateImageButton(NormalizeTextureName(texture.name), texture);
        }

        StartCoroutine(DelayedFadeToFirstView());
    }

    /// <summary>
    /// Crea un botón de imagen en la interfaz de usuario para seleccionar una vista del skybox.
    /// </summary>
    /// <param name="fileName">Nombre del archivo de la textura.</param>
    /// <param name="texture">Textura a mostrar en el botón.</param>
    void CreateImageButton(string fileName, Texture2D texture)
    {
        GameObject button = Instantiate(botonPrefab, contentView.transform);
        Button buttonComponent = button.GetComponent<Button>();
        CursorChanger cursorChanger = button.GetComponent<CursorChanger>();
        if (cursorChanger != null)
        {
            GameObject content = contentView;
            cursorChanger.Initialize(content);
        }

        Transform buttonTextTransform = button.transform.Find("Text");
        Transform iconTransform = button.transform.Find("CircularMask/Icon");

        if (buttonTextTransform != null)
        {
            TextMeshProUGUI buttonText = buttonTextTransform.GetComponent<TextMeshProUGUI>();
            if (buttonText)
            {
                buttonText.text = fileName.Substring(2);
            }

            buttonComponent.onClick.AddListener(() => OnButtonClicked(fileName));

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            Image iconImage = iconTransform.GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.sprite = sprite;
            }
        }
    }

    /// <summary>
    /// Evento que se ejecuta al hacer clic en un botón. Cambia el skybox a la textura seleccionada.
    /// </summary>
    /// <param name="fileName">Nombre del archivo de la textura seleccionada.</param>
    void OnButtonClicked(string fileName)
    {
        if ((skyboxFader.flagBtnCarousel == false && Application.platform != RuntimePlatform.Android) ||
            (skyboxFader.isFading == true && Application.platform != RuntimePlatform.Android))
        {
            Debug.LogWarning("El carrusel aún no está listo. Ignorando clic.");
            return; // Evitamos que se ejecute el cambio si el carrusel no está listo
        }
        SkyBoxButtonSelected skyboxButtonHandler = contentView.GetComponent<SkyBoxButtonSelected>();
        if (skyboxButtonHandler.Bandera() == false)
        {
            if ((skyboxButtonHandler != null && !skyboxButtonHandler.carruselListo && Application.platform != RuntimePlatform.Android))
            {
                Debug.LogWarning("El carrusel aún no está listo. Ignorando clic.");
                return; // Evitamos que se ejecute el cambio si el carrusel no está listo
            }
        }
        //Ponemos la bandera en false que centra el canvas de los hotspot
        cameraView.isCentered = false;
        // Buscar el material que corresponde al nombre del archivo
        Texture textureToApply = null;

        foreach (Texture texture in views)
        {
            if (NormalizeTextureName(texture.name) == fileName)
            {
                textureToApply = texture;
                break;
            }
        }

        if (textureToApply != null)
        {
            if (skyboxFader != null)
            {
                skyboxFader.FadeTo(textureToApply);
            }
            if (Application.platform == RuntimePlatform.Android)
            {
                showGaleryAndroid.CloseCorouselOnClick();
            }

            SkyBoxButtonSelected buttonSelected = contentView.GetComponent<SkyBoxButtonSelected>();
            if (buttonSelected != null)
            {
                buttonSelected.OnSkyboxChanged(fileName);
            }
        }
        else
        {
            Debug.LogWarning($"No se encontró un material para {fileName}");
        }
    }

    /// <summary>
    /// Corrutina que espera un frame antes de realizar la primera transición de skybox.
    /// </summary>
    /// <returns>IEnumerator para la corrutina.</returns>
    private IEnumerator DelayedFadeToFirstView()
    {
        yield return null;

        if (views.Count > 0 && skyboxFader != null)
        {
            skyboxFader.FadeTo(views[0]);
        }
        else
        {
            Debug.LogError("views vacío o skyboxFader sigue siendo null después de esperar.");
        }
    }

    /// <summary>
    /// Normaliza el nombre de la textura para su uso en la interfaz.
    /// </summary>
    /// <param name="name">Nombre original de la textura.</param>
    /// <returns>Nombre normalizado de la textura.</returns>
    string NormalizeTextureName(string name)
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
