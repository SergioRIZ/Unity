using System;
using UnityEngine;

/// <summary>
/// Controlador principal de la cámara que coordina los distintos tipos de entradas
/// para la navegación panorámica 360.          
/// </summary>
public class CameraView : MonoBehaviour
{
    // Referencias a los controladores
    [SerializeField] private MouseController mouseController;
    [SerializeField] private KeyboardController keyboardController;
    [SerializeField] private MobileController touchController;
    [SerializeField] private ZoomController zoomController;
    [SerializeField] private RotationNotifier rotationNotifier;
    [SerializeField] private AutomaticMovementController automaticMovementController;

    [Header("Configuración de Rotación")]
    [Tooltip("Velocidad de rotación horizontal")]
    public float horizontalSpeed = 2.0f;

    [Tooltip("Velocidad de rotación vertical")]
    public float verticalSpeed = 2.0f;

    [Tooltip("Límite de rotación vertical en grados")]
    public float verticalLimit = 80.0f;

    [Range(0.0f, 1.0f)]
    [Tooltip("Velocidad de suavizado para la rotación")]
    public float smoothSpeed = 0.5f;

    [Header("Opciones Básicas")]
    [Tooltip("Ocultar el cursor del mouse mientras se navega")]
    public bool hideCursorWhileNavigating = true;

    [Header("Configuración de Plataforma")]
    [Tooltip("Detectar automáticamente la plataforma (PC/Móvil)")]
    public bool autoDetectPlatform = true;

    [Tooltip("Forzar modo móvil (para pruebas)")]
    public bool forceMobileMode = false;

    /// <summary>
    /// Indica si la plataforma actual es móvil.
    /// </summary>
    public bool IsMobilePlatform { get; private set; }

    // Variables para el seguimiento de la rotación
    private float xRotation = 0.0f;
    private float yRotation = 0.0f;
    private Quaternion targetRotation;
    private bool isTransitioning = false;
    /// <summary>
    /// Indica si la cámara está centrada en un objetivo.
    /// </summary>
    public bool isCentered = false;
    private Camera mainCamera;

    /// <summary>
    /// Rotación X actual de la cámara.
    /// </summary>
    public float XRotation { get => xRotation; set => xRotation = value; }
    /// <summary>
    /// Rotación Y actual de la cámara.
    /// </summary>
    public float YRotation { get => yRotation; set => yRotation = value; }
    /// <summary>
    /// Rotación objetivo de la cámara.
    /// </summary>
    public Quaternion TargetRotation { get => targetRotation; set => targetRotation = value; }
    /// <summary>
    /// Indica si la cámara está en transición de rotación.
    /// </summary>
    public bool IsTransitioning { get => isTransitioning; set => isTransitioning = value; }
    /// <summary>
    /// Referencia a la cámara principal.
    /// </summary>
    public Camera MainCamera { get => mainCamera; }
    [SerializeField] private Transform contenedorFlechas;

    /// <summary>
    /// Evento que se dispara cuando la rotación de la cámara cambia.
    /// </summary>
    public event Action<Vector3> OnRotationChanged;

    /// <summary>
    /// Inicializa la detección de plataforma, la cámara y los componentes requeridos.
    /// </summary>
    private void Awake()
    {
        DetectPlatform();
        InitializeCamera();
        InitializeRequiredComponents();
    }

    /// <summary>
    /// Detecta la plataforma actual (PC o móvil) y ajusta la propiedad IsMobilePlatform.
    /// </summary>
    private void DetectPlatform()
    {
        if (forceMobileMode)
        {
            IsMobilePlatform = true;
            return;
        }

        if (autoDetectPlatform)
        {
            // Detectar si estamos en una plataforma móvil
            IsMobilePlatform = Application.isMobilePlatform ||
                              (Application.platform == RuntimePlatform.WebGLPlayer && IsMobileWebGL());
        }
        else
        {
            IsMobilePlatform = false;
        }

        Debug.Log($"Plataforma detectada: {(IsMobilePlatform ? "Móvil" : "PC")}");
    }

    /// <summary>
    /// Detecta si la plataforma WebGL es móvil usando JavaScript (solo fuera del editor).
    /// </summary>
    /// <returns>True si es WebGL móvil, false en caso contrario.</returns>
    private bool IsMobileWebGL()
    {
        // Detectar si estamos en WebGL en un dispositivo móvil
        bool isMobile = false;

#if !UNITY_EDITOR && UNITY_WEBGL
        // Usar JavaScript para detectar si es móvil
        isMobile = IsMobileUserAgent();
#endif

        return isMobile;
    }

#if !UNITY_EDITOR && UNITY_WEBGL
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern bool IsMobileUserAgent();
#else
    /// <summary>
    /// Fallback para detectar si el user agent es móvil (siempre false fuera de WebGL).
    /// </summary>
    /// <returns>False en editor y otras plataformas.</returns>
    private bool IsMobileUserAgent()
    {
        return false; // Fallback para editor y otras plataformas
    }
#endif

    /// <summary>
    /// Inicializa la referencia a la cámara principal.
    /// </summary>
    private void InitializeCamera()
    {
        // Obtener la cámara
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            Debug.LogWarning("No se encontró componente Camera en este objeto, usando Camera.main.");
        }
    }

    /// <summary>
    /// Inicializa los componentes requeridos y configura los controladores según la plataforma.
    /// </summary>
    private void InitializeRequiredComponents()
    {
        // Inicializar componentes si no están asignados en el inspector
        mouseController = GetOrAddComponent<MouseController>(mouseController);
        keyboardController = GetOrAddComponent<KeyboardController>(keyboardController);
        touchController = GetOrAddComponent<MobileController>(touchController);
        zoomController = GetOrAddComponent<ZoomController>(zoomController);
        rotationNotifier = GetOrAddComponent<RotationNotifier>(rotationNotifier);
        automaticMovementController = GetOrAddComponent<AutomaticMovementController>(automaticMovementController); // Nueva línea

        // Configurar controladores según la plataforma
        ConfigureControllersByPlatform();
    }

    /// <summary>
    /// Habilita o deshabilita los controladores de entrada según la plataforma detectada.
    /// </summary>
    private void ConfigureControllersByPlatform()
    {
        if (IsMobilePlatform)
        {
            // Configuración para móviles
            if (mouseController) mouseController.enabled = false;
            if (keyboardController) keyboardController.enabled = false;
            if (touchController) touchController.enabled = true;
        }
        else
        {
            // Configuración para PC
            if (mouseController) mouseController.enabled = true;
            if (keyboardController) keyboardController.enabled = true;
            if (touchController) touchController.enabled = false;
        }

        // El zoom y el rotationNotifier siempre están activos
        if (zoomController) zoomController.enabled = true;
        if (rotationNotifier) rotationNotifier.enabled = true;
    }

    /// <summary>
    /// Obtiene el componente especificado o lo añade si no existe.
    /// </summary>
    /// <typeparam name="T">Tipo de componente.</typeparam>
    /// <param name="component">Componente existente.</param>
    /// <returns>Componente existente o recién añadido.</returns>
    private T GetOrAddComponent<T>(T component) where T : Component
    {
        if (component == null)
            return gameObject.AddComponent<T>();
        return component;
    }

    /// <summary>
    /// Inicializa la orientación de la cámara y los controladores.
    /// </summary>
    void Start()
    {
        InitializeOrientation();
        InitializeControllers();
    }

    /// <summary>
    /// Inicializa la rotación de la cámara a partir de su rotación local actual.
    /// </summary>
    private void InitializeOrientation()
    {
        // Inicializar la rotación
        Vector3 eulerAngles = transform.localEulerAngles;
        xRotation = eulerAngles.x;
        yRotation = eulerAngles.y;
        targetRotation = transform.localRotation;
    }

    /// <summary>
    /// Inicializa los controladores de entrada y suscribe los eventos necesarios.
    /// </summary>
    private void InitializeControllers()
    {
        // Espera a que todos los componentes existan antes de inicializarlos
        if (mouseController != null) mouseController.Initialize(this);
        if (keyboardController != null) keyboardController.Initialize(this);
        if (touchController != null) touchController.Initialize(this);
        if (zoomController != null) zoomController.Initialize(this);
        if (rotationNotifier != null) rotationNotifier.Initialize(this);
        if (automaticMovementController != null) automaticMovementController.Initialize(this); // Nueva línea

        // Suscribir al evento de notificación
        if (rotationNotifier != null)
            rotationNotifier.OnRotationChanged += HandleRotationChanged;
    }

    /// <summary>
    /// Maneja el evento de cambio de rotación y lo propaga a los suscriptores.
    /// </summary>
    /// <param name="rotation">Nueva rotación.</param>
    private void HandleRotationChanged(Vector3 rotation)
    {
        OnRotationChanged?.Invoke(rotation);
    }

    /// <summary>
    /// Método de actualización por frame (no utilizado actualmente).
    /// </summary>
    void Update()
    {

    }

    /// <summary>
    /// Actualiza la rotación de la cámara de forma suave o durante transiciones.
    /// </summary>
    void FixedUpdate()
    {
        if (!isTransitioning && !isCentered)
        {
            ApplySmoothRotation();
        }
        else if (isTransitioning && !isCentered)
        {
            smoothSpeed = 2f * Time.deltaTime; // Haces la transición lenta
            ApplySmoothRotation();

            // Cuando ya casi llegamos a destino, detenemos el reset
            if (Quaternion.Angle(transform.localRotation, targetRotation) < 0.1f)
            {
                isTransitioning = false;
                smoothSpeed = 0.5f; // Restauramos el valor original o el que quieras para rotación normal
                // Actualizar las rotaciones internas del controlador
                Vector3 euler = transform.localRotation.eulerAngles;
                //Tenemos que corregir el euler.x para que la camara no pase de 180º
                float x = euler.x > 180 ? euler.x - 360 : euler.x;
                float y = euler.y;

                XRotation = x;
                YRotation = y;
            }
        }
        else if (isCentered && !isTransitioning)
        {
            smoothSpeed = 2f * Time.deltaTime; // Haces la transición lenta
            ApplySmoothRotation();
            // Stop rotating when close enough
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                isCentered = false;
                smoothSpeed = 0.5f; // Restauramos el valor original o el que quieras para rotación normal
                // Actualizar las rotaciones internas del controlador
                Vector3 euler = transform.localRotation.eulerAngles;
                //Tenemos que corregir el euler.x para que la camara no pase de 180º
                float x = euler.x > 180 ? euler.x - 360 : euler.x;
                float y = euler.y;

                XRotation = x;
                YRotation = y;
            }
        }
    }

    /// <summary>
    /// Aplica la rotación suave a la cámara usando interpolación esférica.
    /// </summary>
    private void ApplySmoothRotation()
    {
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, smoothSpeed);
    }

    /// <summary>
    /// Resetea la rotación de la cámara a su estado inicial y reinicia el zoom.
    /// </summary>
    public void ResetView()
    {
        xRotation = 0;
        yRotation = 0;
        //flag para el resetView
        isTransitioning = true;
        //Meve la camara al origen
        targetRotation = Quaternion.identity;

        if (zoomController != null)
        {
            zoomController.ResetZoom();
        }
    }

    /// <summary>
    /// Restaura la configuración del cursor cuando el script se desactiva.
    /// </summary>
    private void OnDisable()
    {
        // Restaurar configuración de cursor si se desactiva el script
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// Rota la cámara suavemente para mirar hacia una posición objetivo.
    /// </summary>
    /// <param name="targetPosition">Posición a la que mirar.</param>
    public void RotateCameraToLookAt(Vector3 targetPosition)
    {
        if (Camera.main == null)
        {
            Debug.LogError("No se encuentra la cámara principal.");
            return;
        }

        Vector3 direction = targetPosition - transform.position;
        targetRotation = Quaternion.LookRotation(direction);

        // NO actualizar manualmente Camera.main.transform.rotation

        isCentered = true; // Esto activa la rotación suave en FixedUpdate

        mouseController?.IgnoreNextMouseDelta();
    }

    /// <summary>
    /// Busca y devuelve la posición del objeto "Punto" dentro del grupo de flechas activo.
    /// </summary>
    /// <returns>Posición del objeto "Punto" en el grupo activo.</returns>
    public Vector3 FindPunto()
    {
        Transform grupoActivo = null;
        foreach (Transform grupo in contenedorFlechas)
        {
            if (grupo.gameObject.activeInHierarchy)
            {
                grupoActivo = grupo;
                Debug.Log("El nombre del grupo activo es: " + grupoActivo.name);
                break;
            }
        }
        Vector3 direction = grupoActivo.Find("Punto").position;
        return direction;
    }
}