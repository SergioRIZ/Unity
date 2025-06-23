using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Clase que gestiona las transiciones de materiales del Skybox, incluyendo efectos de fade y zoom.
/// </summary>
public class MaterialTransition : MonoBehaviour
{
    /// <summary>
    /// Indica si actualmente se está realizando una transición.
    /// </summary>
    public bool isFading = false;

    [Header("Transición")]
    /// <summary>
    /// Duración de la transición de fade en segundos.
    /// </summary>
    public float fadeDuration = 2f;

    /// <summary>
    /// Material utilizado para realizar la transición del Skybox.
    /// </summary>
    private Material blendMaterial;

    [Header("Elementos del Zoom")]
    /// <summary>
    /// Distancia de zoom en unidades.
    /// </summary>
    public float zoomDistance = 3f;

    /// <summary>
    /// Velocidad del zoom.
    /// </summary>
    public float zoomSpeed = 1f;

    /// <summary>
    /// Campo de visión (FOV) durante el zoom.
    /// </summary>
    public float zoomFOV = 30f;

    /// <summary>
    /// Duración del zoom en segundos.
    /// </summary>
    public float zoomDuration = 1f;

    /// <summary>
    /// Posición original de la cámara antes de realizar el zoom.
    /// </summary>
    private Vector3 originalPosition;

    /// <summary>
    /// Duración del fade combinado con el zoom en segundos.
    /// </summary>
    public float fadeDurationWithZoom = 1.5f;

    /// <summary>
    /// Campo de visión (FOV) original de la cámara antes de realizar el zoom.
    /// </summary>
    private float originalFOV;

    /// <summary>
    /// Bandera para habilitar o deshabilitar los botones del carrusel.
    /// </summary>
    public bool flagBtnCarousel = true;

    /// <summary>
    /// Referencia al controlador de la cámara.
    /// </summary>
    private CameraView cameraView;

    /// <summary>
    /// Referencia al contenedor de flechas (grupos de paneles).
    /// </summary>
    [SerializeField] private Transform contenedorFlechas;

    /// <summary>
    /// Inicializa el material y el shader necesarios para la transición del Skybox.
    /// </summary>
    private void Awake()
    {
        flagBtnCarousel = true;
        Shader blendShader = Shader.Find("Skybox/SkyboxBlend");
        cameraView = GameObject.Find("MainCamera").GetComponent<CameraView>();

        if (blendShader == null)
        {
            Debug.LogError("No se encontró el shader Skybox/SkyboxBlend.");
            return;
        }

        blendMaterial = new Material(blendShader);
        RenderSettings.skybox = blendMaterial;

        Debug.Log("MaterialTransition configurado correctamente.");
    }

    /// <summary>
    /// Inicia una transición de fade hacia una nueva textura de Skybox.
    /// </summary>
    /// <param name="newTexture">La nueva textura hacia la que se realizará la transición.</param>
    public void FadeTo(Texture newTexture)
    {
        if (isFading)
        {
            Debug.LogWarning("Ya se está realizando una transición.");
            return;
        }

        if (newTexture == null)
        {
            Debug.LogWarning("La textura de destino es nula.");
            return;
        }
        isFading = true;

        ApagarTodosLosPaneles();
        StartCoroutine(FadeSkyboxCoroutine(newTexture));
    }

    /// <summary>
    /// Corrutina que realiza la transición de fade entre dos texturas del Skybox.
    /// </summary>
    /// <param name="newTexture">La nueva textura hacia la que se realizará la transición.</param>
    /// <returns>IEnumerator para la corrutina.</returns>
    private IEnumerator FadeSkyboxCoroutine(Texture newTexture)
    {
        if (blendMaterial == null)
        {
            Debug.LogError("blendMaterial es NULL al iniciar la transición.");
            yield break;
        }

        Texture currentTex = blendMaterial.HasProperty("_MainTex2") ? blendMaterial.GetTexture("_MainTex2") : null;
        blendMaterial.SetTexture("_MainTex", currentTex != null ? currentTex : newTexture);
        blendMaterial.SetTexture("_MainTex2", newTexture);

        float time = 0f;
        while (time < fadeDuration)
        {
            float t = time / fadeDuration;
            blendMaterial.SetFloat("_Blend", t);
            time += Time.deltaTime;
            yield return null;
        }

        blendMaterial.SetFloat("_Blend", 1f);
        blendMaterial.SetTexture("_MainTex", newTexture);
        blendMaterial.SetTexture("_MainTex2", newTexture);
        blendMaterial.SetFloat("_Blend", 0f);

        RenderSettings.skybox = blendMaterial;
        DynamicGI.UpdateEnvironment();
        cameraView.RotateCameraToLookAt(cameraView.FindPunto());

        Debug.Log("Transición completada.");
        isFading = false;
    }

    /// <summary>
    /// Inicia una transición combinada de zoom y fade hacia una nueva textura de Skybox.
    /// </summary>
    /// <param name="skyboxTexture">La nueva textura hacia la que se realizará la transición.</param>
    public void StartTransition(Texture skyboxTexture)
    {
        ApagarTodosLosPaneles();
        StartCoroutine(CombinedTransition(skyboxTexture));
    }

    /// <summary>
    /// Corrutina que realiza una transición combinada de zoom y fade.
    /// </summary>
    /// <param name="newSkybox">La nueva textura hacia la que se realizará la transición.</param>
    /// <returns>IEnumerator para la corrutina.</returns>
    private IEnumerator CombinedTransition(Texture newSkybox)
    {
        flagBtnCarousel = false;
        // Guardamos estado inicial
        originalPosition = transform.position;
        originalFOV = Camera.main.fieldOfView;
        Coroutine zoomIn = StartCoroutine(ZoomInDirection(false));

        yield return new WaitForSeconds(zoomDuration * 0.5f);
        Coroutine fade = StartCoroutine(FadeAndZoomSkybox(newSkybox));
        yield return new WaitForSeconds(fadeDuration * 0.5f);
        Coroutine zoomOut = StartCoroutine(ZoomInDirection(true));
        cameraView.RotateCameraToLookAt(cameraView.FindPunto());

        yield return fade;
        yield return zoomOut;
        flagBtnCarousel = true;
    }

    /// <summary>
    /// Corrutina que realiza una transición de fade mientras se aplica un zoom.
    /// </summary>
    /// <param name="newTexture">La nueva textura hacia la que se realizará la transición.</param>
    /// <returns>IEnumerator para la corrutina.</returns>
    private IEnumerator FadeAndZoomSkybox(Texture newTexture)
    {
        if (blendMaterial == null)
        {
            Debug.LogError("blendMaterial es NULL al iniciar la transición.");
            yield break;
        }

        Texture currentTex = blendMaterial.HasProperty("_MainTex2") ? blendMaterial.GetTexture("_MainTex2") : null;
        blendMaterial.SetTexture("_MainTex", currentTex != null ? currentTex : newTexture);
        blendMaterial.SetTexture("_MainTex2", newTexture);

        float time = 0f;
        while (time < fadeDurationWithZoom)
        {
            float t = time / fadeDurationWithZoom;
            blendMaterial.SetFloat("_Blend", t);
            time += Time.deltaTime;
            yield return null;
        }

        blendMaterial.SetFloat("_Blend", 1f);
        blendMaterial.SetTexture("_MainTex", newTexture);
        blendMaterial.SetTexture("_MainTex2", newTexture);
        blendMaterial.SetFloat("_Blend", 0f);

        RenderSettings.skybox = blendMaterial;
        DynamicGI.UpdateEnvironment();

        Debug.Log("Transición completada.");
        isFading = false;
    }

    /// <summary>
    /// Corrutina que realiza un zoom hacia adelante o hacia atrás.
    /// </summary>
    /// <param name="reverse">Indica si el zoom debe ser hacia atrás (true) o hacia adelante (false).</param>
    /// <returns>IEnumerator para la corrutina.</returns>
    private IEnumerator ZoomInDirection(bool reverse = false)
    {
        Vector3 startPos = reverse ? transform.position : originalPosition;
        Vector3 endPos = reverse ? originalPosition : transform.position + transform.forward.normalized * zoomDistance;

        float startFOV = reverse ? Camera.main.fieldOfView : originalFOV;
        float endFOV = reverse ? originalFOV : zoomFOV;

        Quaternion targetRotation = transform.rotation;

        float elapsedTime = 0f;
        float currentDuration = zoomDuration;

        if (reverse)
        {
            currentDuration *= 0.3f;
        }

        while (elapsedTime < currentDuration)
        {
            float t = elapsedTime / currentDuration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            transform.rotation = targetRotation;
            Camera.main.fieldOfView = Mathf.Lerp(startFOV, endFOV, t);

            elapsedTime += Time.deltaTime * zoomSpeed;
            yield return null;
        }

        transform.position = endPos;
        transform.rotation = targetRotation;
        Camera.main.fieldOfView = endFOV;
    }

    /// <summary>
    /// Busca todas las referencias a scripts PanelVideoControllerConSubtitulos en los grupos activos.
    /// </summary>
    /// <returns>Lista de referencias a PanelVideoControllerConSubtitulos.</returns>
    public List<PanelVideoControllerConSubtitulos> FindAllVideoPanels()
    {
        List<PanelVideoControllerConSubtitulos> paneles = new List<PanelVideoControllerConSubtitulos>();

        foreach (Transform grupo in contenedorFlechas)
        {
            if (grupo.gameObject.activeInHierarchy)
            {
                PanelVideoControllerConSubtitulos[] encontrados = grupo.GetComponentsInChildren<PanelVideoControllerConSubtitulos>(true);
                paneles.AddRange(encontrados);
            }
        }

        return paneles;
    }

    /// <summary>
    /// Busca todas las referencias a scripts PanelImagenController en los grupos activos.
    /// </summary>
    /// <returns>Lista de referencias a PanelImagenController.</returns>
    public List<PanelImagenController> FindAllImagePanels()
    {
        List<PanelImagenController> paneles = new List<PanelImagenController>();

        foreach (Transform grupo in contenedorFlechas)
        {
            if (grupo.gameObject.activeInHierarchy)
            {
                PanelImagenController[] encontrados = grupo.GetComponentsInChildren<PanelImagenController>(true);
                paneles.AddRange(encontrados);
            }
        }

        return paneles;
    }

    /// <summary>
    /// Apaga todos los paneles de video e imagen antes de realizar la transición.
    /// </summary>
    public void ApagarTodosLosPaneles()
    {
        // Foreach para apagar todos los paneles video
        foreach (var panel in FindAllVideoPanels())
        {
            panel.ApagarPanel();
        }

        // Foreach para apagar todos los paneles imagenes
        foreach (var panel in FindAllImagePanels())
        {
            panel.ApagarImagen();
        }
    }
}
