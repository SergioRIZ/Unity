using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
using UnityEditor;
#endif

// FALTA MIRAR LA RUTA DEL VIDEO

// NECESITAS ESTO PARA UnityWebRequest
using UnityEngine.Networking;

public class PanelVideoControllerConSubtitulos : MonoBehaviour
{
    [Header("Referencias UI - Panel y Controles")]
    [SerializeField] private GameObject panelContenedor;      
    [SerializeField] private RawImage videoDisplay;           
    [SerializeField] private Button botonCerrar;
    [SerializeField] private Button botonPlay;
    [SerializeField] private Button botonPause;
    [SerializeField] private Button botonAdelantar;
    [SerializeField] private Button botonRetroceder;
    [SerializeField] private Button botonSubtitulos;     [Header("Opciones del Video")]
    [SerializeField] private float margenBotonCerrar = 20f;
    // [SerializeField] private Vector2 resolucionRenderTexture = new Vector2(1920, 1080); // Nueva variable para resolución del RenderTexture

    [Header("Opciones de Controles UI")]
    [SerializeField] private float espacioEntreControles = 10f;
    [SerializeField] private float yOffsetControlesInferiores = 30f;

    [Header("Salto de Tiempo")]
    [SerializeField] private float segundosAdelantar = 5f;
    [SerializeField] private float segundosRetroceder = 5f;

    [Header("Fade")]
    [SerializeField] private float duracionFade = 0.5f;

    [Header("Subtítulos")]
    [SerializeField] private TextMeshProUGUI subtitulosText;
    [SerializeField] private Color colorBotonSubtitulosActivo = Color.green;
    [SerializeField] private Color colorBotonSubtitulosInactivo = Color.white;

    [Header("Sound Control")]
    [SerializeField] private SoundController soundController;

    private VideoPlayer videoPlayer;
    private AudioSource audioSource;
    private bool forceClosedState = false;
    private bool materialChanged = false;
    private RenderTexture videoRenderTexture; // Nueva variable para almacenar el RenderTexture

    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;

    private bool subtitulosActivos = false;
    private List<SubtitleEntry> subtitulos = new List<SubtitleEntry>();
    private Coroutine subtitleCoroutine;

    private Material lastSkybox;

    private CameraView cameraView;
    private AutomaticMovementController autoCamera;

    private class SubtitleEntry
    {
        public int index;
        public double startTime;
        public double endTime;
        public string text;
    }

    void Awake()
    {
        // Obtener componentes básicos
        videoPlayer = GetComponent<VideoPlayer>();
        audioSource = GetComponent<AudioSource>();
        
        // Buscar referencias de cámara y movimiento automático
        cameraView = GameObject.Find("MainCamera").GetComponent<CameraView>();
        autoCamera = FindObjectOfType<AutomaticMovementController>();

        if (panelContenedor != null)
        {
            canvasGroup = panelContenedor.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = panelContenedor.AddComponent<CanvasGroup>();
            }
        }

        // Configurar listeners de botones
        if (botonCerrar != null)
        {
            botonCerrar.onClick.AddListener(CerrarPanel);
        }

        // Crear y configurar el RenderTexture para el video
        if (videoPlayer != null && videoDisplay != null)
        {
            SetupVideoRenderTexture();
        }
        else
        {
            Debug.LogError("[PanelVideoController] Awake: videoPlayer o videoDisplay no asignados!");
        }
    }

private void SetupVideoRenderTexture()
{
    Debug.Log("[PanelVideoController] Configurando RenderTexture para el video...");
    
    // En lugar de crear un nuevo RenderTexture, usamos el que ya está asignado en el Inspector
    if (videoPlayer != null && videoDisplay != null)
    {
        // Asignar el RenderTexture del VideoPlayer al RawImage
        videoDisplay.texture = videoPlayer.targetTexture;

        // Ajustar el aspecto del RawImage al del video
        if (videoDisplay.GetComponent<AspectRatioFitter>() == null)
        {
            AspectRatioFitter aspectFitter = videoDisplay.gameObject.AddComponent<AspectRatioFitter>();
            aspectFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            // Usar la relación de aspecto del RenderTexture asignado
            if (videoPlayer.targetTexture != null)
            {
                aspectFitter.aspectRatio = (float)videoPlayer.targetTexture.width / videoPlayer.targetTexture.height;
            }
        }
    }
    else
    {
        Debug.LogError("[PanelVideoController] SetupVideoRenderTexture falló: videoPlayer o videoDisplay no asignados!");
    }
}

    void Start()
    {
        Debug.Log("[PanelVideoController] Start");

        // Configuración inicial del panel
        if (panelContenedor != null)
        {
            Debug.Log("[PanelVideoController] Configurando panelContenedor.");
            panelContenedor.SetActive(false);
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }

        // Configuración del video y audio
        if (videoPlayer != null)
        {
            videoPlayer.errorReceived += (vp, error) => Debug.LogError($"Error en Video Player: {error}");
            videoPlayer.prepareCompleted += OnVideoPlayerPrepared;
            
            if (audioSource != null)
            {
                videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
                videoPlayer.SetTargetAudioSource(0, audioSource);
                audioSource.playOnAwake = false;
            }
        }

        // Configuración inicial de subtítulos
        if (subtitulosText != null)
        {
            subtitulosText.gameObject.SetActive(false);
        }

        lastSkybox = RenderSettings.skybox;
    }

void Update()
{
}



    // Método para mostrar el panel cargando un video desde una URL (usado típicamente en WebGL con StreamingAssets)
    public void MostrarPanelDesdeRuta(string videoUrl)
    {
        if (panelContenedor == null || videoPlayer == null || string.IsNullOrEmpty(videoUrl)) 
        {
            Debug.LogError("[PanelVideoController] MostrarPanelDesdeRuta falló: Referencias nulas.");
            return;
        }

        // Notificar al controlador de cámara automática
        if (autoCamera != null)
        {
            autoCamera.OnPointOfInterestOpened();
        }

        // Activar y configurar el panel
        if (!panelContenedor.activeSelf)
        {
            panelContenedor.SetActive(true);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }

        // Configurar el video
        videoPlayer.Stop();
        videoPlayer.clip = null;
        videoPlayer.url = videoUrl;
        
        // Configurar el audio
        if (soundController != null && audioSource != null)
        {
            
            soundController.SetControlledAudioSource(audioSource);
        }

        // Cargar subtítulos
        CargarSubtitulos(Path.GetFileNameWithoutExtension(videoUrl));
        
        // Preparar y reproducir el video
        videoPlayer.Prepare();
        StartCoroutine(EsperarYReproducir());

        // Ajustar controles y rotar cámara
        AjustarPosicionControles();
        if (cameraView != null)
        {
            cameraView.RotateCameraToLookAt(gameObject.transform.position);
        }

        // Iniciar fade
        StartFade(canvasGroup != null ? canvasGroup.alpha : 0, 1);
    }

    // Corrutina que espera a que el video esté preparado y luego lo reproduce y gestiona el fade-in
    private IEnumerator EsperarYReproducir()
    {
        Debug.Log("[PanelVideoController] EsperarYReproducir: Esperando preparación del video.");
        // Espera a que el videoPlayer termine de prepararse
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        Debug.Log("[PanelVideoController] EsperarYReproducir: Video preparado. Reproduciendo...");
        videoPlayer.Play(); // Inicia la reproducción del video

        // Inicia la reproducción del audioSource si existe y está configurado para el video
        if (audioSource != null && videoPlayer.audioOutputMode == VideoAudioOutputMode.AudioSource)
        {
             Debug.Log("[PanelVideoController] EsperarYReproducir: Reproduciendo audio.");
             audioSource.Play(); 
        } else if (audioSource != null && videoPlayer.audioOutputMode != VideoAudioOutputMode.AudioSource) {
             Debug.LogWarning("[PanelVideoController] EsperarYReproducir: AudioSource presente, pero VideoPlayer no configurado para usarlo.");
        } else {
             Debug.Log("[PanelVideoController] EsperarYReproducir: AudioSource no presente.");
        }


        // Iniciar la corrutina de subtítulos *solo si hay subtítulos cargados* y están activos
        // Esto maneja el caso de carga asíncrona en WebGL, donde 'subtitulos' se llena
        // DENTRO de la corrutina LoadSubtitleAssetWebGL después de la descarga.
        if (subtitulosActivos && subtitulos.Count > 0)
        {
             Debug.Log($"[PanelVideoController] EsperarYReproducir: Subtítulos activos y cargados ({subtitulos.Count} entradas). Iniciando corrutina de visualización.");
             if (subtitleCoroutine != null) StopCoroutine(subtitleCoroutine); // Detener si ya estaba corriendo
             subtitleCoroutine = StartCoroutine(MostrarSubtitulos());
             if (subtitulosText != null) subtitulosText.gameObject.SetActive(true); // Asegura que el objeto de texto esté activo
        }
        else
        {
             Debug.Log("[PanelVideoController] EsperarYReproducir: Subtítulos no activos, no cargados, o lista vacía. Ocultando texto.");
             if (subtitulosText != null)
             {
                 subtitulosText.text = ""; // Asegura que el texto esté vacío
                 subtitulosText.gameObject.SetActive(false); // Oculta el objeto de texto
             }
        }

        // *** CAMBIO CLAVE PARA WEBGL Y EDITOR ***
        // Finalmente, iniciar el fade-in del panel AHORA que el video está preparado y
        // la configuración inicial (incluyendo subtítulos asíncronos) ha comenzado.
        // Esta línea se MOVIO desde MostrarPanel y MostrarPanelDesdeRuta a aquí.
        if (canvasGroup != null)
        {
             Debug.Log("[PanelVideoController] EsperarYReproducir: Iniciando fade-in del panel.");
             StartFade(canvasGroup.alpha, 1);
        } else {
             Debug.LogError("[PanelVideoController] EsperarYReproducir: canvasGroup es nulo! No se puede iniciar el fade.");
             // Fallback: activar el panel si no podemos hacer fade
             if (panelContenedor != null) panelContenedor.SetActive(true);
        }
    }

    // Cierra el panel con un fade-out
    public void CerrarPanel()
    {
        Debug.Log("[PanelVideoController] CerrarPanel llamado.");
        // Solo procede si el panel está activo
        if (panelContenedor == null || !panelContenedor.activeSelf) 
        {
            Debug.LogWarning("[PanelVideoController] CerrarPanel llamado pero panelContenedor es nulo o inactivo.");
            return;
        }

        if (autoCamera != null)
            autoCamera.OnPointOfInterestClosed();

        if (soundController != null)
        {
            soundController.ResumeAmbientSound();
        }    

        // Inicia el fade-out a alpha 0
        if (canvasGroup != null)
        {
             Debug.Log("[PanelVideoController] Iniciando fade-out del panel.");
             StartFade(canvasGroup.alpha, 0);
        } else {
             Debug.LogError("[PanelVideoController] canvasGroup es nulo en CerrarPanel! Desactivando panel directamente.");
             if (panelContenedor != null) panelContenedor.SetActive(false);
        }

        // Detiene el video y el audio inmediatamente
        if (videoPlayer != null)
            videoPlayer.Stop();
        if (audioSource != null)
            audioSource.Stop();

       

        // AÑADIR ESTA SECCIÓN - Desactiva los subtítulos
        subtitulosActivos = false; // Fuerza el estado a desactivado
        ConfigurarBotonSubtitulos(); // Actualiza el aspecto visual del botón

        // Detiene la corrutina de subtítulos y limpia el texto
        if (subtitleCoroutine != null)
        {
            StopCoroutine(subtitleCoroutine);
            subtitleCoroutine = null;
        }
        if (subtitulosText != null)
        {
            subtitulosText.text = "";
            subtitulosText.gameObject.SetActive(false); // Asegura que el objeto de texto esté inactivo
        }

        // Opcional: Limpiar el clip o la URL del videoPlayer para liberar memoria
        if (videoPlayer != null)
        {
            videoPlayer.clip = null;
            videoPlayer.url = null;
        }

        // Limpiar la lista de subtítulos cargados
        subtitulos.Clear();

        Debug.Log("[PanelVideoController] Panel cerrado - Limpieza completada.");
    }

    // Apaga el panel
    public void ApagarPanel()
    {
        if (panelContenedor != null && panelContenedor.activeSelf && canvasGroup != null && canvasGroup.alpha > 0.1f)
        {
            if (soundController != null)
            {
                soundController.ResumeAmbientSound();
            }
            panelContenedor.SetActive(false);
            if (videoPlayer != null)
                videoPlayer.Stop();
            if (audioSource != null)
                audioSource.Stop();
            
            // Asegurar que los subtítulos también se limpien
            if (subtitulosText != null)
            {
                subtitulosText.text = "";
                subtitulosText.gameObject.SetActive(false);
            }
        }
    }

    // Avanza el video N segundos
    public void AdelantarVideo()
    {
        Debug.Log("[PanelVideoController] AdelantarVideo llamado.");
        if (videoPlayer != null && videoPlayer.isPrepared)
        {
            videoPlayer.time += segundosAdelantar;
            // Asegurarse de no ir más allá de la duración
            if (videoPlayer.time > videoPlayer.length)
                videoPlayer.time = videoPlayer.length;
            Debug.Log($"[PanelVideoController] Video adelantado a tiempo: {videoPlayer.time}");
        } else {
             Debug.LogWarning("[PanelVideoController] AdelantarVideo falló: VideoPlayer nulo o no preparado.");
        }
    }

    // Retrocede el video N segundos
    public void RetrocederVideo()
    {
        Debug.Log("[PanelVideoController] RetrocederVideo llamado.");
        if (videoPlayer != null && videoPlayer.isPrepared)
        {
            videoPlayer.time -= segundosRetroceder;
            // Asegurarse de no ir antes del inicio
            if (videoPlayer.time < 0)
                videoPlayer.time = 0;
            Debug.Log($"[PanelVideoController] Video retrocedido a tiempo: {videoPlayer.time}");
        } else {
            Debug.LogWarning("[PanelVideoController] RetrocederVideo falló: VideoPlayer nulo o no preparado.");
        }
    }

    private void StartFade(float from, float to)
    {
        if (canvasGroup == null)
        {
            if (panelContenedor != null) panelContenedor.SetActive(to > 0);
            return;
        }

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadePanel(from, to));
    }

    private IEnumerator FadePanel(float startAlpha, float endAlpha)
    {
        float t = 0f;
        float currentAlpha = canvasGroup.alpha;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        while (t < duracionFade)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / duracionFade);
            canvasGroup.alpha = Mathf.Lerp(currentAlpha, endAlpha, normalized);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;

        if (endAlpha >= 1f)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            panelContenedor.SetActive(false);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        
        fadeCoroutine = null;
    }

    // Ajusta la posición de los controles UI
    private void AjustarPosicionControles()
    {
        Debug.Log("[PanelVideoController] AjustarPosicionControles llamado.");
        // Posiciona el botón de cerrar en la esquina superior derecha
        if (botonCerrar != null)
        {
            RectTransform btnCerrarRect = botonCerrar.GetComponent<RectTransform>();
            if (btnCerrarRect != null)
            {
                btnCerrarRect.anchorMin = new Vector2(1, 1);
                btnCerrarRect.anchorMax = new Vector2(1, 1);
                btnCerrarRect.pivot = new Vector2(1, 1);
                btnCerrarRect.anchoredPosition = new Vector2(-margenBotonCerrar, -margenBotonCerrar);
                 Debug.Log("[PanelVideoController] botonCerrar posicionado.");
            } else {
                 Debug.LogWarning("[PanelVideoController] botonCerrar asignado pero no tiene RectTransform.");
            }
        } else {
            Debug.LogWarning("[PanelVideoController] botonCerrar no asignado. No se puede posicionar.");
        }

        // Posiciona los controles inferiores (play/pause, etc.)
        List<RectTransform> controlesInferiores = new List<RectTransform>();
        // Añade los controles a la lista en el orden deseado para la alineación
        if (botonRetroceder != null) controlesInferiores.Add(botonRetroceder.GetComponent<RectTransform>()); else Debug.LogWarning("[PanelVideoController] botonRetroceder no asignado para AjustarPosicionControles.");
        if (botonPlay != null) controlesInferiores.Add(botonPlay.GetComponent<RectTransform>()); else Debug.LogWarning("[PanelVideoController] botonPlay no asignado para AjustarPosicionControles.");
        if (botonPause != null) controlesInferiores.Add(botonPause.GetComponent<RectTransform>()); else Debug.LogWarning("[PanelVideoController] botonPause no asignado para AjustarPosicionControles.");
        if (botonAdelantar != null) controlesInferiores.Add(botonAdelantar.GetComponent<RectTransform>()); else Debug.LogWarning("[PanelVideoController] botonAdelantar no asignado para AjustarPosicionControles.");
        if (botonSubtitulos != null) controlesInferiores.Add(botonSubtitulos.GetComponent<RectTransform>()); else Debug.LogWarning("[PanelVideoController] botonSubtitulos no asignado para AjustarPosicionControles.");

        // Calcula el ancho total de los controles inferiores
        float anchoTotal = 0f;
        foreach (RectTransform rt in controlesInferiores)
        {
            // Ya verificamos rt == null al añadir a la lista, pero una doble verificación no hace daño
            if (rt == null) continue; 
            anchoTotal += rt.sizeDelta.x * rt.localScale.x; // Suma el ancho de cada control (considerando escala)
        }
        // Añade el espacio entre los controles
        anchoTotal += espacioEntreControles * (controlesInferiores.Count > 0 ? controlesInferiores.Count - 1 : 0);

        // Calcula la posición X inicial para centrar el bloque de controles
        float currentX = -anchoTotal / 2f;
        
        // Define el ancla y el pivote para los controles inferiores (centro inferior)
        Vector2 anchor = new Vector2(0.5f, 0);
        Vector2 pivot = new Vector2(0.5f, 0);

        // Posiciona cada control
        foreach (RectTransform rt in controlesInferiores)
        {
            if (rt == null) continue;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            
            // Calcula la posición X del centro del control
            float posX = currentX + (rt.sizeDelta.x * rt.localScale.x * rt.pivot.x);
            
            // Establece la posición anclada
            rt.anchoredPosition = new Vector2(posX, yOffsetControlesInferiores);
            
            // Avanza la posición X para el siguiente control
            currentX += (rt.sizeDelta.x * rt.localScale.x) + espacioEntreControles;
        }
        Debug.Log("[PanelVideoController] AjustarPosicionControles finalizado.");
    }

    // Configura el color del botón de subtítulos según si están activos
    private void ConfigurarBotonSubtitulos()
    {
        Debug.Log("[PanelVideoController] ConfigurarBotonSubtitulos llamado.");
        // *** VERIFICACIÓN DE NULO AÑADIDA AQUÍ ***
        if (botonSubtitulos == null)
        {
            Debug.LogWarning("[PanelVideoController] ConfigurarBotonSubtitulos falló: botonSubtitulos es nulo. No se puede configurar el color.");
            return; // Sale de la función si el botón no está asignado
        }
        // *** FIN VERIFICACIÓN ***

        Debug.Log($"[PanelVideoController] Configurando color de botonSubtitulos. subtitulosActivos: {subtitulosActivos}");
        ColorBlock colores = botonSubtitulos.colors;
        colores.fadeDuration = 0; 
        colores.normalColor = subtitulosActivos ? colorBotonSubtitulosActivo : colorBotonSubtitulosInactivo;
        colores.selectedColor = colores.normalColor; // Usa el mismo color para otros estados
        colores.pressedColor = colores.normalColor;
        colores.highlightedColor = colores.normalColor;
        colores.disabledColor = colorBotonSubtitulosInactivo; // Puede ser diferente si se desea
        botonSubtitulos.colors = colores; // Aplica los colores
         Debug.Log("[PanelVideoController] ConfigurarBotonSubtitulos finalizado.");
    }

    // Alterna el estado activo de los subtítulos
    public void ToggleSubtitulos()
    {
        subtitulosActivos = !subtitulosActivos;
        Debug.Log($"[PanelVideoController] ToggleSubtitulos llamado. Nuevo estado: {subtitulosActivos}");
        
        if (subtitulosText != null)
        {
            subtitulosText.gameObject.SetActive(subtitulosActivos);
        }
        
        ConfigurarBotonSubtitulos();

        if (subtitulosActivos)
        {
            if (subtitleCoroutine != null)
            {
                StopCoroutine(subtitleCoroutine);
            }
            subtitleCoroutine = StartCoroutine(MostrarSubtitulos());
        }
        else
        {
            if (subtitleCoroutine != null)
            {
                StopCoroutine(subtitleCoroutine);
                subtitleCoroutine = null;
            }
            if (subtitulosText != null)
            {
                subtitulosText.text = "";
            }
        }
    }

    // Corrutina para mostrar los subtítulos en el momento correcto
    private IEnumerator MostrarSubtitulos()
{
    Debug.Log("Iniciando corrutina de subtítulos...");
    
    while (subtitulosActivos)
    {
        if (videoPlayer != null && videoPlayer.isPrepared && subtitulos.Count > 0)
        {
            double tiempoActual = videoPlayer.time;  // Obtiene el tiempo actual del video
            string textoSubtitulo = "";

            // Busca el subtítulo correspondiente al tiempo actual
            foreach (var subtitulo in subtitulos)
            {
                if (tiempoActual >= subtitulo.startTime && tiempoActual <= subtitulo.endTime)
                {
                    textoSubtitulo = subtitulo.text;
                    break;
                }
            }

            // Actualiza el texto en la UI
            if (subtitulosText != null)
            {
                subtitulosText.text = textoSubtitulo;
            }
            
            Debug.Log($"Tiempo: {tiempoActual:F2} - Texto: {textoSubtitulo}");
        }
        yield return new WaitForSeconds(0.05f);  // Espera un pequeño intervalo antes de la siguiente comprobación
    }

    // Limpia los subtítulos cuando se desactivan
    if (subtitulosText != null)
    {
        subtitulosText.text = "";
        subtitulosText.gameObject.SetActive(false);
    }
    
    Debug.Log("Termina corrutina de subtítulos");
}

    // *** NUEVA CORRUTINA PARA CARGAR SUBTÍTULOS ASÍNCRONAMENTE EN WEBGL ***
    private IEnumerator LoadSubtitleAssetWebGL(string videoNameWithoutExtension)
    {
        // Construye la URL al archivo en StreamingAssets
        string subtitleUrl = Path.Combine(Application.streamingAssetsPath, "Subtitulos", videoNameWithoutExtension + ".txt");

        Debug.Log($"[PanelVideoController][WebGL] Intentando cargar subtítulos desde URL: {subtitleUrl}");

        // Usa UnityWebRequest para descargar el archivo de texto
        using (UnityWebRequest www = UnityWebRequest.Get(subtitleUrl))
        {
            // Envía la solicitud y espera a que se complete
            yield return www.SendWebRequest();

            // Verifica si hubo errores
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"[PanelVideoController][WebGL] Error al cargar subtítulos desde {subtitleUrl}: {www.error}");
                // Si falla, asegúrate de que no haya subtítulos cargados
                subtitulos.Clear();
            }
            else
            {
                Debug.Log($"[PanelVideoController][WebGL] Subtítulos cargados exitosamente desde {subtitleUrl}");
                // Obtiene el texto descargado
                string content = www.downloadHandler.text;

                if (!string.IsNullOrEmpty(content))
                {
                     // Parsea el contenido descargado y llena la lista 'subtitulos'
                     ParsearArchivoSRT(content); 
                     Debug.Log($"[PanelVideoController][WebGL] Subtítulos parseados. {subtitulos.Count} entradas encontradas.");
                }
                else
                {
                     Debug.LogWarning($"[PanelVideoController][WebGL] El archivo de subtítulos {subtitleUrl} está vacío o no contiene texto parseable.");
                     subtitulos.Clear(); // Si el archivo está vacío o no se parseó nada, limpia la lista
                }
            }
        }

        // IMPORTANTE: Una vez que la carga asíncrona ha terminado (éxito o fallo),
        // si el video ya está preparado y reproduciendo, y los subtítulos están activos,
        // podemos iniciar (o reiniciar) la corrutina que MUESTRA los subtítulos.
        // La corrutina EsperarYReproducir también hace esto, pero esto puede ser útil
        // si la carga de subtítulos tarda más que la preparación del video.
        if (subtitulosActivos && videoPlayer != null && videoPlayer.isPrepared && videoPlayer.isPlaying)
        {
             Debug.Log("[PanelVideoController][WebGL] Carga asíncrona de subs terminada, intentando iniciar/reiniciar corrutina de visualización.");
             if (subtitleCoroutine != null) StopCoroutine(subtitleCoroutine);
             if (subtitulos.Count > 0) 
             {
                subtitleCoroutine = StartCoroutine(MostrarSubtitulos());
                if (subtitulosText != null) subtitulosText.gameObject.SetActive(true);
             } else if (subtitulosText != null) // Si no hay subs cargados, asegurar texto limpio
             {
                 subtitulosText.text = "";
                 subtitulosText.gameObject.SetActive(false);
             }
        } else {
            Debug.Log("[PanelVideoController][WebGL] Carga asíncrona de subs terminada, pero condiciones no cumplen para iniciar visualización (activo="+subtitulosActivos+", preparado="+(videoPlayer?.isPrepared ?? false)+", reproduciendo="+(videoPlayer?.isPlaying ?? false)+", subs.Count="+subtitulos.Count+").");
             if (subtitulosText != null) // Asegurar texto limpio si no se inicia la visualización
             {
                 subtitulosText.text = "";
                 subtitulosText.gameObject.SetActive(false);
             }
        }
         Debug.Log("[PanelVideoController][WebGL] Corrutina LoadSubtitleAssetWebGL finalizada.");
    }


    // Intenta cargar el asset de subtítulos automáticamente usando el nombre del video.
    // Esta función decide si usar Resources.Load (Editor/Standalone) o UnityWebRequest (WebGL).
    private void CargarSubtitulos(string nombreVideo)
    {
        Debug.Log($"[PanelVideoController] CargarSubtitulos llamado para video: '{nombreVideo}'.");

        // Siempre limpia subtítulos anteriores y detiene la corrutina de visualización
        // antes de intentar cargar nuevos.
        if (subtitleCoroutine != null)
        {
            Debug.Log("[PanelVideoController] Deteniendo corrutina de subtítulos existente.");
            StopCoroutine(subtitleCoroutine); 
            subtitleCoroutine = null;
        }
        subtitulos.Clear(); 
        if (subtitulosText != null)
            subtitulosText.text = ""; 

        // Construye la ruta al archivo de subtítulos en StreamingAssets
        string nombreSinExt = Path.GetFileNameWithoutExtension(nombreVideo);
        string subtitlePath = Path.Combine(Application.streamingAssetsPath, "Subtitulos", nombreSinExt + ".srt");

        #if UNITY_WEBGL
            // En WebGL, la carga de StreamingAssets es asíncrona
            Debug.Log($"[PanelVideoController][WebGL] Iniciando carga asíncrona de subtítulo desde StreamingAssets para: {nombreSinExt}");
            StartCoroutine(LoadSubtitleAssetWebGL(nombreSinExt));
        #else
            // En otras plataformas, carga el archivo directamente
            if (File.Exists(subtitlePath))
            {
                Debug.Log($"[PanelVideoController] Subtítulo encontrado en StreamingAssets: {subtitlePath}");
                string content = File.ReadAllText(subtitlePath);
                ParsearArchivoSRT(content);
            }
            else
            {
                Debug.LogWarning($"[PanelVideoController] No se encontró el subtítulo: {subtitlePath}");
            }
        #endif
         Debug.Log("[PanelVideoController] CargarSubtitulos finalizado (inicio síncrono o asíncrono).");
    }

    // Parsea el contenido de un archivo SRT
    private void ParsearArchivoSRT(string contenido)
    {
        Debug.Log("[PanelVideoController] ParsearArchivoSRT llamado.");
        subtitulos.Clear(); // Limpia cualquier subtítulo anterior antes de parsear
        if (string.IsNullOrEmpty(contenido))
        {
            Debug.LogWarning("[PanelVideoController] ParsearArchivoSRT: Contenido vacío.");
            return;
        }
        
        // Divide el contenido en bloques separados por líneas en blanco
        string[] bloques = contenido.Split(new[] { "\r\n\r\n", "\n\n", "\r\r" }, System.StringSplitOptions.RemoveEmptyEntries);

        // Procesar cada bloque como una entrada de subtítulo
        foreach (string bloque in bloques)
        {
             string[] lineas = bloque.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None);

             // Verificar que tenemos suficientes líneas
             if (lineas.Length < 3)
             {
                 Debug.LogWarning($"[PanelVideoController] Bloque inválido: {bloque}");
                 continue; // Salta este bloque si no tiene al menos 3 líneas (índice, tiempo, texto)
             }

             SubtitleEntry entrada = new SubtitleEntry();
             int lineaActual = 0;

            // 1. Parsear el índice
            if (!int.TryParse(lineas[lineaActual++], out entrada.index))
            {
                 Debug.LogWarning($"[PanelVideoController] Error al parsear índice: {lineas[0]}");
                 continue; // Salta este bloque
            }

            // 2. Parsear los tiempos
            string[] partesTiempo = lineas[lineaActual++].Split(new string[] { " --> " }, System.StringSplitOptions.None);
            if (partesTiempo.Length != 2)
            {
                Debug.LogWarning($"[PanelVideoController] Formato de tiempo inválido: {lineas[lineaActual-1]}");
                continue;
            }
            
            entrada.startTime = TimeToSeconds(partesTiempo[0]);
            entrada.endTime = TimeToSeconds(partesTiempo[1]);

            // 3. Leer el texto (resto de líneas del bloque actual)
            StringBuilder sb = new StringBuilder();
            for (int i = lineaActual; i < lineas.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(lineas[i]))
                {
                    if (sb.Length > 0) sb.Append(" ");
                    sb.Append(lineas[i].Trim());
                }
            }
            entrada.text = sb.ToString();

            subtitulos.Add(entrada);
            Debug.Log($"Subtítulo parseado - Index: {entrada.index}, Time: {entrada.startTime}-{entrada.endTime}, Text: {entrada.text}");
        }
         Debug.Log($"[PanelVideoController] Parseo completado. Total de entradas de subtítulos cargadas: {subtitulos.Count}");
         foreach (var sub in subtitulos)
{
    Debug.Log($"Subtítulo cargado: {sub.index}, Tiempo: {sub.startTime}-{sub.endTime}, Texto: {sub.text}");
}
    }

    // Convierte una cadena de tiempo SRT (HH:MM:SS,ms) a segundos (double)
private double TimeToSeconds(string timeString)
{
    try
    {
        // Formato esperado: 00:00:00,000
        string[] parts = timeString.Split(':');
        if (parts.Length != 3) return 0;

        int hours = int.Parse(parts[0]);
        int minutes = int.Parse(parts[1]);
        
        string[] secondParts = parts[2].Split(',');
        int seconds = int.Parse(secondParts[0]);
        int milliseconds = int.Parse(secondParts[1]);

        return hours * 3600 + minutes * 60 + seconds + (milliseconds / 1000.0);
    }
    catch
    {
        Debug.LogWarning($"Error parsing time string: {timeString}");
        return 0;
    }
}
    // Método para debug cuando el video está preparado
    private void OnVideoPlayerPrepared(VideoPlayer source)
    {
        Debug.Log("[PanelVideoController] Video preparado. Duración: " + source.length);
        Debug.Log($"[PanelVideoController] Subtítulos cargados: {subtitulos.Count}");
    }
}
