﻿using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.SceneTemplate;
#endif

/// <summary>
/// Clase principal que gestiona el menú principal de la aplicación.
/// </summary>
public class MainScript : MonoBehaviour
{
    [Header("Referencias")]
    /// <summary>
    /// Referencia al Canvas para Android.
    /// </summary>
    [SerializeField] private GameObject androidCanvas;
    /// <summary>
    /// Referencia al Canvas para PC.
    /// </summary>
    [SerializeField] private GameObject pcCanvas;
    /// <summary>
    /// Prefab del botón que se instancia para cada imagen.
    /// </summary>
    [SerializeField] private GameObject botonPrefab;
    /// <summary>
    /// Referencia al componente SwipeMenu para gestionar el carrusel.
    /// </summary>
    [SerializeField] private SwipeMenu swipeMenu;
    /// <summary>
    /// Radio de los bordes redondeados para las imágenes.
    /// </summary>
    [SerializeField] private float cornerRadius = 20f;

    [Header("Configuración")]
    /// <summary>
    /// Si es true, simula el comportamiento de Android en el editor.
    /// </summary>
    public bool simularAndroid;

    /// <summary>
    /// Carpeta donde se encuentran las imágenes a cargar.
    /// </summary>
    private string imagesFolder = "Images/Main";
    /// <summary>
    /// Referencia al contenedor de los botones generados.
    /// </summary>
    private GameObject contentView;
    /// <summary>
    /// Última orientación de pantalla detectada.
    /// </summary>
    private ScreenOrientation lastOrientation;

    /// <summary>
    /// Inicializa el menú principal, configurando la UI y cargando imágenes.
    /// </summary>
    void Awake()
    {
        // Evitar que el dispositivo se duerma
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // Configurar interfaz según plataforma
        SetupUI();

        // Cargar imágenes y crear botones
        LoadImagesAndCreateButtons();

        // Inicializar orientación
        lastOrientation = Screen.orientation;

        // Configurar para Android
        if (Application.platform == RuntimePlatform.Android)
        {
            SetupForAndroid();
        }
    }

    /// <summary>
    /// Configura la interfaz de usuario según la plataforma.
    /// </summary>
    private void SetupUI()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            androidCanvas.SetActive(true);
            pcCanvas.SetActive(false);
            contentView = androidCanvas.transform.Find("ScrollViewAndroidMain/ViewportAM/ContentAM").gameObject;
            Debug.Log("Entrando en android");
        }
        else
        {
            androidCanvas.SetActive(false);
            pcCanvas.SetActive(true);
            contentView = pcCanvas.transform.Find("Scroll View/Viewport/Content").gameObject;
            Debug.Log("Entrando en pc");
        }
    }

    /// <summary>
    /// Configura ajustes específicos para Android, como la orientación y el layout.
    /// </summary>
    private void SetupForAndroid()
    {
        OrientationAdapter orientationAdapter = FindObjectOfType<OrientationAdapter>();
        if (orientationAdapter != null)
        {
            orientationAdapter.ForceOrientationUpdate();
        }

        AdjustCanvasScalerForOrientation();
        UpdateCarouselLayout();
    }

    /// <summary>
    /// Carga imágenes y crea botones para cada una.
    /// </summary>
    private void LoadImagesAndCreateButtons()
    {
#if UNITY_EDITOR
        // Código de editor para copiar imágenes y crear escenas
        ProcessImagesInEditor();
#else
        // Código en tiempo de ejecución para cargar imágenes
        Texture2D[] textures = Resources.LoadAll<Texture2D>("CarouselImages/Main");
        foreach (var texture in textures)
        {
            string fileName = texture.name;
            CreateImageButton(fileName, texture);
        }
#endif
    }

#if UNITY_EDITOR
    /// <summary>
    /// Procesa las imágenes en el editor, copiándolas a Resources y creando escenas si no existen.
    /// </summary>
    private void ProcessImagesInEditor()
    {
        string sourceFolderPath = Path.Combine(Application.dataPath, imagesFolder);
        string destinationFolderPath = Path.Combine("Assets/Resources/CarouselImages/Main");

        if (!Directory.Exists(destinationFolderPath))
        {
            Directory.CreateDirectory(destinationFolderPath);
        }

        string[] imagePaths = Directory.GetFiles(sourceFolderPath, "*.jpg");

        foreach (string imagePath in imagePaths)
        {
            string fileName = Path.GetFileNameWithoutExtension(imagePath);
            string destinationPath = Path.Combine(destinationFolderPath, fileName + ".jpg");

            if (!File.Exists(destinationPath))
            {
                File.Copy(imagePath, destinationPath);
                Debug.Log("Imagen copiada: " + destinationPath);
                UnityEditor.AssetDatabase.Refresh();
            }

            string texturePath = "Assets/Resources/CarouselImages/Main/" + fileName + ".jpg";
            Texture2D existingTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            CreateImageButton(fileName, existingTexture);

            CreateSceneIfNeeded(fileName);
        }

        UnityEditor.AssetDatabase.Refresh();
    }

    /// <summary>
    /// Crea una escena a partir de una plantilla si no existe ya una con el nombre dado.
    /// </summary>
    /// <param name="fileName">Nombre base de la escena a crear.</param>
    private void CreateSceneIfNeeded(string fileName)
    {
        string templatePath = "Assets/Scenes/Scene360.scenetemplate";
        string newScenePath = $"Assets/Scenes/{fileName}.unity";

        if (System.IO.File.Exists(newScenePath))
        {
            Debug.Log("Escena: " + fileName + " ya está creada");
            return;
        }

        var template = AssetDatabase.LoadAssetAtPath<SceneTemplateAsset>(templatePath);
        if (template == null)
        {
            Debug.LogError("No se encontró el Scene Template en: " + templatePath);
            return;
        }

        SceneTemplateService.Instantiate(template, true, newScenePath);
    }
#endif

    /// <summary>
    /// Actualiza cada frame para detectar cambios de orientación en Android.
    /// </summary>
    void Update()
    {
        // Detectar cambios de orientación
        if (Application.platform == RuntimePlatform.Android && lastOrientation != Screen.orientation)
        {
            lastOrientation = Screen.orientation;
            HandleOrientationChange();
        }
    }

    /// <summary>
    /// Maneja el cambio de orientación de pantalla, ajustando la UI y el layout.
    /// </summary>
    private void HandleOrientationChange()
    {
        OrientationAdapter orientationAdapter = FindObjectOfType<OrientationAdapter>();
        if (orientationAdapter != null)
        {
            orientationAdapter.ForceOrientationUpdate();
        }

        UpdateCarouselLayout();
        AdjustCanvasScalerForOrientation();
    }

    /// <summary>
    /// Ajusta el CanvasScaler de todos los Canvas según la orientación de la pantalla.
    /// </summary>
    void AdjustCanvasScalerForOrientation()
    {
        bool isLandscape = IsLandscapeOrientation();
        CanvasScaler[] scalers = FindObjectsOfType<CanvasScaler>();

        foreach (CanvasScaler scaler in scalers)
        {
            scaler.referenceResolution = isLandscape ?
                new Vector2(1920, 1080) : new Vector2(1080, 1920);

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.matchWidthOrHeight = isLandscape ? 0.5f : 1f;
        }
    }

    /// <summary>
    /// Actualiza el layout del carrusel de botones según la orientación.
    /// </summary>
    void UpdateCarouselLayout()
    {
        bool isLandscape = IsLandscapeOrientation();

        // Si no hay contentView, salir
        if (contentView == null) return;

        // Ajustar GridLayoutGroup
        GridLayoutGroup grid = contentView.GetComponent<GridLayoutGroup>();
        if (grid != null)
        {
            UpdateGridLayout(grid, isLandscape);
        }

        // Ajustar tamaño del contenedor
        RectTransform rt = contentView.GetComponent<RectTransform>();
        if (rt != null)
        {
            float widthMultiplier = isLandscape ? 1.3f : 1.0f; // Reducido de 1.4f y 1.1f
            rt.sizeDelta = new Vector2(rt.sizeDelta.x * widthMultiplier, rt.sizeDelta.y);
        }

        // Actualizar SwipeMenu
        if (swipeMenu != null)
        {
            swipeMenu.UpdateLayoutForOrientation(isLandscape);
        }
    }

    /// <summary>
    /// Actualiza la configuración del GridLayoutGroup según la orientación.
    /// </summary>
    /// <param name="grid">GridLayoutGroup a actualizar.</param>
    /// <param name="isLandscape">Indica si la orientación es horizontal.</param>
    private void UpdateGridLayout(GridLayoutGroup grid, bool isLandscape)
    {
        if (isLandscape)
        {
            grid.constraintCount = 3;
            grid.spacing = new Vector2(50, 15);

            // Ajustar tamaño de celdas (reducido)
            RectTransform contentRect = contentView.GetComponent<RectTransform>();
            float cellWidth = (contentRect.rect.width - (grid.spacing.x * 2)) / 3 * 0.9f; // Reducido de 1.2f
            float cellHeight = grid.cellSize.y * 0.9f; // Reducido
            grid.cellSize = new Vector2(cellWidth, cellHeight);
        }
        else
        {
            grid.constraintCount = 1;
            grid.spacing = new Vector2(25, 25);
            // Tamaño reducido
            grid.cellSize = new Vector2(350, 170); // Reducido de 450, 220
        }
    }

    /// <summary>
    /// Crea un botón con una imagen y lo añade al contenedor.
    /// </summary>
    /// <param name="fileName">Nombre de la imagen y del botón.</param>
    /// <param name="texture">Textura de la imagen a mostrar.</param>
    void CreateImageButton(string fileName, Texture2D texture)
    {
        GameObject button = Instantiate(botonPrefab, contentView.transform);
        Button buttonComponent = button.GetComponent<Button>();

        Transform buttonTextTransform = button.transform.Find("Text");
        Transform maskTransform = button.transform.Find("Mask");
        Transform iconTransform = maskTransform.transform.Find("Icon");

        if (buttonTextTransform == null || maskTransform == null || iconTransform == null)
        {
            Debug.LogError("Estructura de prefab de botón incorrecta");
            return;
        }

        // Configurar texto
        TextMeshProUGUI buttonText = buttonTextTransform.GetComponent<TextMeshProUGUI>();
        if (buttonText)
        {
            buttonText.text = fileName;
            // Reducir tamaño de fuente
            buttonText.fontSize = buttonText.fontSize * 0.9f;
        }

        // Asignar acción de clic
        buttonComponent.onClick.AddListener(() => OnButtonClicked(fileName));

        // Reducir tamaño de la máscara para imagen más pequeña
        RectTransform maskRect = maskTransform.GetComponent<RectTransform>();
        if (maskRect != null)
        {
            // Reducir el tamaño de la máscara en un 15%
            maskRect.sizeDelta = new Vector2(maskRect.sizeDelta.x * 0.85f, maskRect.sizeDelta.y * 0.85f);
        }

        // Aplicar imagen con bordes redondeados
        if (texture != null)
        {
            Texture2D roundedTexture = CreateRoundedTexture(texture, cornerRadius);
            Sprite sprite = CreateSpriteFromTexture(roundedTexture);

            Image iconImage = iconTransform.GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.sprite = sprite;
                iconImage.preserveAspect = true;
                iconImage.raycastTarget = false;

                // Reducir el tamaño de la imagen
                RectTransform iconRT = iconImage.GetComponent<RectTransform>();
                if (iconRT != null)
                {
                    iconRT.sizeDelta = new Vector2(iconRT.sizeDelta.x * 0.85f, iconRT.sizeDelta.y * 0.85f);
                }
            }
        }
    }

    /// <summary>
    /// Crea un sprite a partir de una textura dada.
    /// </summary>
    /// <param name="texture">Textura de origen.</param>
    /// <returns>Sprite generado a partir de la textura.</returns>
    private Sprite CreateSpriteFromTexture(Texture2D texture)
    {
        return Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100.0f,
            0,
            SpriteMeshType.FullRect
        );
    }

    /// <summary>
    /// Crea una textura con bordes redondeados a partir de la original.
    /// </summary>
    /// <param name="originalTexture">Textura original.</param>
    /// <param name="radius">Radio de los bordes redondeados.</param>
    /// <returns>Textura con esquinas redondeadas.</returns>
    Texture2D CreateRoundedTexture(Texture2D originalTexture, float radius)
    {
        Texture2D readableTexture = MakeTextureReadable(originalTexture);
        int width = readableTexture.width;
        int height = readableTexture.height;

        Texture2D roundedTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color[] originalPixels = readableTexture.GetPixels();
        Color[] newPixels = new Color[width * height];
        float radiusSq = radius * radius;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                newPixels[index] = originalPixels[index];

                // Comprobar esquinas
                bool isCorner = false;
                float distSq = 0f;

                // Esquina superior izquierda
                if (x < radius && y < radius)
                {
                    distSq = (radius - x) * (radius - x) + (radius - y) * (radius - y);
                    isCorner = true;
                }
                // Esquina superior derecha
                else if (x > width - radius && y < radius)
                {
                    distSq = (x - (width - radius)) * (x - (width - radius)) + (radius - y) * (radius - y);
                    isCorner = true;
                }
                // Esquina inferior izquierda
                else if (x < radius && y > height - radius)
                {
                    distSq = (radius - x) * (radius - x) + (y - (height - radius)) * (y - (height - radius));
                    isCorner = true;
                }
                // Esquina inferior derecha
                else if (x > width - radius && y > height - radius)
                {
                    distSq = (x - (width - radius)) * (x - (width - radius)) + (y - (height - radius)) * (y - (height - radius));
                    isCorner = true;
                }

                // Si estamos en una esquina y fuera del radio, hacer el píxel transparente
                if (isCorner && distSq > radiusSq)
                {
                    Color pixel = originalPixels[index];
                    newPixels[index] = new Color(pixel.r, pixel.g, pixel.b, 0);
                }
            }
        }

        roundedTexture.SetPixels(newPixels);
        roundedTexture.Apply();
        return roundedTexture;
    }

    /// <summary>
    /// Convierte una textura de solo lectura en una textura legible por CPU.
    /// </summary>
    /// <param name="texture">Textura de origen.</param>
    /// <returns>Textura legible.</returns>
    Texture2D MakeTextureReadable(Texture2D texture)
    {
        RenderTexture tmp = RenderTexture.GetTemporary(
            texture.width, texture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);

        Graphics.Blit(texture, tmp);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = tmp;

        Texture2D readableTexture = new Texture2D(texture.width, texture.height);
        readableTexture.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
        readableTexture.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(tmp);

        return readableTexture;
    }

    /// <summary>
    /// Método que se ejecuta al hacer clic en un botón de imagen.
    /// </summary>
    /// <param name="fileName">Nombre del botón/escena asociado.</param>
    void OnButtonClicked(string fileName)
    {
        Debug.Log("OnButtonClicked: " + fileName);

        // Para Android, cargar directamente la escena
        if (Application.platform == RuntimePlatform.Android)
        {
            SceneManager.LoadScene(fileName);
            return;
        }

        // Para otras plataformas, usar comportamiento de centrado
        int index = FindButtonIndex(fileName);
        if (index == -1)
        {
            Debug.LogWarning("No se encontró el índice del botón con nombre: " + fileName);
            return;
        }

        if (swipeMenu.IsButtonCentered(index))
        {
            // Si está centrado, ejecuta la acción
            SceneManager.LoadScene(fileName);
        }
        else
        {
            // Si no, solo lo centra
            swipeMenu.CenterButton(index);
        }
    }

    /// <summary>
    /// Encuentra el índice de un botón por su nombre.
    /// </summary>
    /// <param name="buttonName">Nombre del botón a buscar.</param>
    /// <returns>Índice del botón, o -1 si no se encuentra.</returns>
    private int FindButtonIndex(string buttonName)
    {
        for (int i = 0; i < contentView.transform.childCount; i++)
        {
            var text = contentView.transform.GetChild(i).GetComponentInChildren<TextMeshProUGUI>();
            if (text != null && text.text == buttonName)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Comprueba si la orientación actual de la pantalla es horizontal.
    /// </summary>
    /// <returns>True si la orientación es horizontal, false si es vertical.</returns>
    private bool IsLandscapeOrientation()
    {
        return (Screen.orientation == ScreenOrientation.LandscapeLeft ||
                Screen.orientation == ScreenOrientation.LandscapeRight);
    }
}