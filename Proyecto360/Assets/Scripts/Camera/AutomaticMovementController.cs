using UnityEngine;
using System.Collections;

/// <summary>
/// Controlador de movimiento automático para la cámara panorámica cuando el usuario está inactivo.
/// Realiza un recorrido completo de 360° a una velocidad personalizable.
/// Se detiene inmediatamente ante cualquier interacción del usuario.
/// MODIFICADO: Inicia desactivado por defecto.
/// </summary>
public class AutomaticMovementController : MonoBehaviour
{
    [Header("Configuración de Inicio")]
    [Tooltip("Iniciar el componente desactivado por defecto")]
    public bool startDisabled = true;

    [Header("Configuración de Tiempo")]
    [Tooltip("Tiempo de inactividad antes de iniciar el movimiento automático (segundos)")]
    public float inactivityThreshold = 3.0f;

    [Header("Configuración de Movimiento")]
    [Tooltip("Velocidad de rotación automática horizontal")]
    public float autoRotationSpeed = 20.0f; // Aumentado para mayor velocidad

    [Tooltip("Tiempo para completar un recorrido completo de 360° (segundos)")]
    public float timeFor360Tour = 15.0f; // Ajústalo para controlar la velocidad del recorrido

    [Tooltip("Usar rotación vertical ondulante")]
    public bool useVerticalWave = true;

    [Tooltip("Amplitud de la onda vertical")]
    [Range(0.0f, 15.0f)]
    public float verticalWaveAmplitude = 5.0f; // Aumentado para más movimiento

    [Tooltip("Frecuencia de la onda vertical")]
    [Range(0.01f, 1.0f)]
    public float verticalWaveFrequency = 0.2f;

    [Header("Opciones Avanzadas")]
    // Eliminada la opción de desactivar durante zoom

    [Header("Interacción con Puntos de Interés")]
    [Tooltip("Desactivar movimiento automático durante puntos de interés o videos")]
    public bool disableDuringPOI = true;

    [Header("Sensibilidad de Detección")]
    [Tooltip("Sensibilidad para detectar movimiento del ratón (valores más bajos = más sensible)")]
    [Range(0.0001f, 0.01f)]
    public float mouseSensitivity = 0.001f;

    [Tooltip("Continuar desde la última posición del usuario")]
    public bool continueFromUserPosition = true;

    [Tooltip("Realizar el recorrido en sentido horario")]
    public bool clockwiseRotation = true;

    [Header("Transición Suave")]
    [Tooltip("Duración de la transición suave al iniciar el movimiento (segundos)")]
    [Range(0.5f, 5.0f)]
    public float smoothStartDuration = 2.0f;

    [Tooltip("Suavizar también la rotación vertical al inicio")]
    public bool smoothVerticalStart = true;

    [Tooltip("Modo de depuración")]
    public bool debugMode = false;

    private CameraView parentController;
    private ZoomController zoomController;
    private MouseController mouseController;
    private KeyboardController keyboardController;
    private MobileController touchController;

    private float lastInputTime;
    private bool isAutoMoving = false;
    private float verticalWaveOffset = 0.0f;
    private bool initialized = false;

    private bool isPointOfInterestActive = false;
    private float initialFOV;
    private float currentFOV;

    // Variables para controlar el tour de 360°
    private float tourStartTime;
    private float startingYRotation;

    // Variables para la transición suave
    private float transitionStartTime;
    private float initialXRotation;
    private float targetXRotation;
    private float currentSpeedFactor = 0f;

    // Variables para detectar clic o tap
    private bool wasMouseDown = false;
    private int previousTouchCount = 0;

    // Variables para rastrear la posición del ratón
    private Vector3 lastMousePosition;

    // Referencia al transform de la cámara
    private Transform cameraTransform;

    // Para almacenar el valor original de smoothSpeed
    private float originalSmoothSpeed;

    private CameraView cameraView;

    /// <summary>
    /// Inicializa el controlador con una referencia al controlador principal.
    /// </summary>
    /// <param name="controller">El controlador de cámara principal.</param>
    public void Initialize(CameraView controller)
    {
        parentController = controller;
        // Guardar una referencia al transform de la cámara
        if (parentController != null && parentController.MainCamera != null)
            cameraTransform = parentController.MainCamera.transform;

        // Guardar el valor original de smoothSpeed
        if (parentController != null)
            originalSmoothSpeed = parentController.smoothSpeed;

        // Obtener referencias a otros controladores
        zoomController = GetComponent<ZoomController>();
        if (zoomController == null)
            zoomController = FindObjectOfType<ZoomController>();

        mouseController = GetComponent<MouseController>();
        if (mouseController == null)
            mouseController = FindObjectOfType<MouseController>();

        keyboardController = GetComponent<KeyboardController>();
        if (keyboardController == null)
            keyboardController = FindObjectOfType<KeyboardController>();

        touchController = GetComponent<MobileController>();
        if (touchController == null)
            touchController = FindObjectOfType<MobileController>();

        // Inicializar valores
        lastInputTime = Time.time;
        lastMousePosition = Input.mousePosition;
        verticalWaveOffset = Random.Range(0f, Mathf.PI * 2); // Offset aleatorio para la onda

        if (parentController.MainCamera != null)
        {
            initialFOV = parentController.MainCamera.fieldOfView;
            currentFOV = initialFOV;
        }

        // Calcular la velocidad de rotación basada en el tiempo para completar 360°
        if (timeFor360Tour > 0)
        {
            autoRotationSpeed = 360.0f / timeFor360Tour;
        }

        cameraView = GetComponent<CameraView>();

        initialized = true;

        // NUEVO: Desactivar el componente si startDisabled está activado
        if (startDisabled)
        {
            enabled = false;
            if (debugMode) Debug.Log("AutomaticMovementController iniciado como DESACTIVADO");
        }
    }

    /// <summary>
    /// Se ejecuta al inicializar el componente. Registra el detector de movimiento del ratón antes de cada frame.
    /// </summary>
    void Awake()
    {
        // Registrar el detector de movimiento del ratón antes de cada frame
        Application.onBeforeRender += CheckForMouseMovement;
    }

    /// <summary>
    /// Se ejecuta al destruir el componente. Elimina el detector de movimiento del ratón.
    /// </summary>
    void OnDestroy()
    {
        // Eliminar el detector cuando se destruya el objeto
        Application.onBeforeRender -= CheckForMouseMovement;
    }

    /// <summary>
    /// Se ejecuta al habilitar el componente. Resetea el tiempo de inactividad.
    /// </summary>
    void OnEnable()
    {
        // Resetear el tiempo de inactividad al habilitar el script
        ResetInactivityTimer();
        lastMousePosition = Input.mousePosition;

        if (debugMode) Debug.Log("AutomaticMovementController ACTIVADO");
    }

    /// <summary>
    /// Se ejecuta al deshabilitar el componente. Restaura el control al usuario si estaba en movimiento automático.
    /// </summary>
    void OnDisable()
    {
        // Asegurarse de que los controladores de usuario estén habilitados al desactivar
        if (isAutoMoving)
        {
            EnableUserControllers();
            isAutoMoving = false;
        }

        if (debugMode) Debug.Log("AutomaticMovementController DESACTIVADO");
    }

    /// <summary>
    /// Método que se ejecuta antes de cada frame para detectar movimiento del ratón.
    /// Si se detecta movimiento, detiene el movimiento automático.
    /// </summary>
    private void CheckForMouseMovement()
    {
        if (!isAutoMoving) return;

        // Detección ultra-rápida del movimiento del ratón
        Vector3 currentMousePosition = Input.mousePosition;
        if (currentMousePosition != lastMousePosition)
        {
            // Detener inmediatamente el movimiento automático
            StopAutoMovementAndRestoreControl();
            lastMousePosition = currentMousePosition;
            lastInputTime = Time.time;
        }
    }

    /// <summary>
    /// Actualiza el estado del controlador cada frame.
    /// Detecta entrada del usuario, inicia o detiene el movimiento automático según corresponda.
    /// </summary>
    void Update()
    {
        if (!initialized || parentController == null || cameraView.isCentered || cameraView.IsTransitioning)
            return;

        // Detectar entrada del usuario y detener movimiento automático si es necesario
        bool userInput = DetectUserInput();

        if (userInput && isAutoMoving)
        {
            // Detener inmediatamente el movimiento automático y restaurar control al usuario
            StopAutoMovementAndRestoreControl();
            lastInputTime = Time.time;

        }
        else if (userInput)
        {
            // Solo actualizar timer si hay input pero no estamos en automático
            lastInputTime = Time.time;

        }
        else if (!isAutoMoving)
        {
            // Verificar si debemos iniciar el movimiento automático
            CheckForAutoMovement();

        }
        else if (isAutoMoving)
        {
            // Aplicar el movimiento automático si está activo
            ApplyAutomaticMovement();
        }
    }

    /// <summary>
    /// Detecta si el usuario está proporcionando alguna entrada (ratón, teclado, toque o zoom).
    /// </summary>
    /// <returns>True si se detecta entrada del usuario, false en caso contrario.</returns>
    private bool DetectUserInput()
    {
        bool hasUserInput = false;

        // Detección de movimiento de ratón más precisa
        Vector3 currentMousePosition = Input.mousePosition;
        float mouseDelta = Vector3.Distance(currentMousePosition, lastMousePosition);

        if (mouseDelta > mouseSensitivity)
        {
            hasUserInput = true;
            if (debugMode) Debug.Log($"Detectado movimiento de mouse: {mouseDelta} - Deteniendo auto-cámara");
        }

        // Actualizar la posición del ratón para la próxima comprobación
        lastMousePosition = currentMousePosition;

        // Comprobar entrada de mouse - mejorado para detectar clics
        bool isMouseDown = Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2);

        // Detectar clic (transición de no presionado a presionado)
        if (!wasMouseDown && isMouseDown)
        {
            hasUserInput = true;
            if (debugMode) Debug.Log("Detectado clic de mouse - Deteniendo auto-cámara");
        }

        wasMouseDown = isMouseDown;

        // Comprobar entrada de teclado (WASD, Flechas)
        Vector2 keyboardInput = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
        );
        if (keyboardInput.sqrMagnitude > 0.01f)
        {
            hasUserInput = true;
            if (debugMode) Debug.Log("Detectada entrada de teclado - Deteniendo auto-cámara");
        }

        // Comprobar si hay toques en pantalla
        if (Input.touchCount > 0)
        {
            // Detectar nuevo toque (comenzando)
            if (Input.touchCount > previousTouchCount)
            {
                hasUserInput = true;
                if (debugMode) Debug.Log("Detectado nuevo toque - Deteniendo auto-cámara");
            }
            // O detectar movimiento de toque existente
            else if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (Input.GetTouch(i).phase == TouchPhase.Moved ||
                        Input.GetTouch(i).phase == TouchPhase.Began)
                    {
                        hasUserInput = true;
                        break;
                    }
                }
            }
        }

        previousTouchCount = Input.touchCount;

        // Comprobar cambio en el zoom (si está disponible)
        if (parentController.MainCamera != null)
        {
            float newFOV = parentController.MainCamera.fieldOfView;
            if (Mathf.Abs(newFOV - currentFOV) > 0.1f)
            {
                hasUserInput = true;
                currentFOV = newFOV;
            }
        }

        return hasUserInput;
    }

    /// <summary>
    /// Verifica si debe iniciarse o detenerse el movimiento automático según la inactividad y puntos de interés.
    /// </summary>
    private void CheckForAutoMovement()
    {
        // Si hay un punto de interés activo y la opción de desactivar está activa, no iniciamos movimiento
        if (disableDuringPOI && isPointOfInterestActive)
            return;

        float inactiveTime = Time.time - lastInputTime;

        // Si estamos en tiempo de inactividad y no estamos en movimiento, comenzar
        if (inactiveTime >= inactivityThreshold && !isAutoMoving)
        {
            StartAutoMovement();
        }
    }

    /// <summary>
    /// Inicia el movimiento automático de la cámara con transición suave.
    /// </summary>
    private void StartAutoMovement()
    {
        // No iniciar si hay un punto de interés activo
        if (disableDuringPOI && isPointOfInterestActive)
            return;

        // Eliminada la verificación de zoom

        if (isAutoMoving)
            return;

        isAutoMoving = true;
        tourStartTime = Time.time;
        transitionStartTime = Time.time;
        startingYRotation = parentController.YRotation;

        // Guardar la rotación vertical inicial para la transición suave
        initialXRotation = parentController.XRotation;

        // Calcular un objetivo para la rotación vertical (donde debería estar cuando la onda empiece)
        // Esto asegura que no haya saltos bruscos
        float time = Time.time * verticalWaveFrequency;
        targetXRotation = Mathf.Sin(time + verticalWaveOffset) * verticalWaveAmplitude;

        // Inicializar el factor de velocidad a 0 para una aceleración suave
        currentSpeedFactor = 0f;

        if (debugMode) Debug.Log("Iniciando movimiento automático de cámara con transición suave");

        // Si no continuamos desde la posición del usuario, podríamos establecer un punto inicial
        if (!continueFromUserPosition)
        {
            // Opcionalmente, podríamos establecer un punto de inicio específico
            // parentController.XRotation = 0;
            // parentController.YRotation = 0;
        }
    }

    /// <summary>
    /// Detiene el movimiento automático y restaura el control al usuario.
    /// Aplica una detención inmediata y restaura la configuración de suavizado.
    /// </summary>
    private void StopAutoMovementAndRestoreControl()
    {
        if (!isAutoMoving)
        {
            return;
        }

        isAutoMoving = false;

        // SOLUCIÓN DEFINITIVA:
        if (parentController != null)
        {
            // 1. Desactivar cualquier transición o centrado en curso
            //parentController.IsTransitioning = false;
            //parentController.isCentered = false;

            // 2. Establecer las rotaciones actuales como objetivo, evitando cualquier interpolación
            Quaternion currentRotation = cameraTransform != null ?
                cameraTransform.localRotation :
                Quaternion.Euler(parentController.XRotation, parentController.YRotation, 0);

            parentController.TargetRotation = currentRotation;

            // 3. Aplicar directamente la rotación actual al transform (bypass del sistema de suavizado)
            if (cameraTransform != null)
            {
                cameraTransform.localRotation = currentRotation;
            }

            // 4. Temporalmente establecer smoothSpeed a un valor muy alto para una detención casi instantánea
            float originalSmooth = parentController.smoothSpeed;
            parentController.smoothSpeed = 100f; // Valor extremadamente alto para detención inmediata

            // 5. Restaurar el smoothSpeed original después de un breve tiempo
            StartCoroutine(RestoreSmoothSpeed(originalSmooth));

            if (debugMode) Debug.Log("Detención forzada aplicada - Control devuelto al usuario");
        }

        // Asegurarse de que los controladores de usuario estén habilitados
        EnableUserControllers();
    }

    /// <summary>
    /// Restaura el valor original de smoothSpeed después de un breve tiempo.
    /// </summary>
    /// <param name="originalValue">Valor original de smoothSpeed a restaurar.</param>
    /// <returns>IEnumerator para la corrutina.</returns>
    private IEnumerator RestoreSmoothSpeed(float originalValue)
    {
        // Esperar un par de frames para asegurar que la detención se haya aplicado completamente
        yield return null;
        yield return null;

        // Restaurar el valor original
        if (parentController != null)
        {
            parentController.smoothSpeed = originalValue;
            if (debugMode) Debug.Log($"Smooth speed restaurado a {originalValue}");
        }
    }

    /// <summary>
    /// Habilita todos los controladores de usuario según la plataforma (PC o móvil).
    /// </summary>
    private void EnableUserControllers()
    {
        // Configurar controladores según la plataforma
        if (parentController.IsMobilePlatform)
        {
            // Configuración para móviles
            if (touchController) touchController.enabled = true;
        }
        else
        {
            // Configuración para PC
            if (mouseController) mouseController.enabled = true;
            if (keyboardController) keyboardController.enabled = true;
        }

        // El zoom siempre está activo
        if (zoomController) zoomController.enabled = true;
    }

    /// <summary>
    /// Resetea el temporizador de inactividad y detiene el movimiento automático si está activo.
    /// </summary>
    private void ResetInactivityTimer()
    {
        lastInputTime = Time.time;

        if (isAutoMoving)
        {
            StopAutoMovementAndRestoreControl();
        }
    }

    /// <summary>
    /// Aplica el movimiento automático a la cámara para un recorrido completo de 360°.
    /// Incluye transición suave y movimiento vertical ondulante si está habilitado.
    /// </summary>
    private void ApplyAutomaticMovement()
    {
        // Si hay un punto de interés activo, detener el movimiento automático
        if (disableDuringPOI && isPointOfInterestActive)
        {
            StopAutoMovementAndRestoreControl();
            return;
        }

        // Calculamos el tiempo transcurrido desde el inicio de la transición
        float transitionElapsed = Time.time - transitionStartTime;

        // Calcular el factor de suavizado para la transición (0 a 1)
        float transitionProgress = Mathf.Clamp01(transitionElapsed / smoothStartDuration);

        // Aplicar una curva de suavizado (ease-in)
        currentSpeedFactor = Mathf.SmoothStep(0, 1, transitionProgress);

        // Calculamos el progreso del tour de 360°
        float elapsedTime = Time.time - tourStartTime;
        float rotationDirection = clockwiseRotation ? 1.0f : -1.0f;

        // Rotación horizontal con velocidad graduada
        float rotation = rotationDirection * autoRotationSpeed * Time.deltaTime * currentSpeedFactor;
        float yRotation = parentController.YRotation + rotation;

        // Rotación vertical ondulante (opcional)
        float xRotation = parentController.XRotation;

        if (useVerticalWave)
        {
            float time = Time.time * verticalWaveFrequency;
            float waveValue = Mathf.Sin(time + verticalWaveOffset) * verticalWaveAmplitude;

            if (smoothVerticalStart && transitionProgress < 1.0f)
            {
                // Hacer una transición suave desde la posición inicial hasta la onda
                xRotation = Mathf.Lerp(initialXRotation, waveValue, transitionProgress);
            }
            else
            {
                xRotation = waveValue;
            }

            // Asegurar que no excedemos los límites verticales
            xRotation = Mathf.Clamp(
                xRotation,
                -parentController.verticalLimit,
                parentController.verticalLimit
            );
        }

        // Actualizar los valores en el controlador principal
        parentController.XRotation = xRotation;
        parentController.YRotation = yRotation;

        // Crear la rotación objetivo
        parentController.TargetRotation = Quaternion.Euler(xRotation, yRotation, 0);

        // Debug info
        if (debugMode && transitionProgress < 1.0f)
        {
            Debug.Log($"Transición de cámara: {transitionProgress:F2}, Velocidad: {currentSpeedFactor:F2}");
        }
    }

    // Método IsZooming eliminado para evitar el bug

    /// <summary>
    /// Fuerza el inicio del movimiento automático (para llamadas desde otros scripts).
    /// </summary>
    public void ForceStartTour()
    {
        StartAutoMovement();
    }

    /// <summary>
    /// Fuerza la detención del movimiento automático (para llamadas desde otros scripts).
    /// </summary>
    public void ForceStopTour()
    {
        StopAutoMovementAndRestoreControl();
    }

    /// <summary>
    /// Notifica al controlador que un punto de interés o video se ha activado.
    /// Detiene el movimiento automático si está activo.
    /// </summary>
    public void OnPointOfInterestOpened()
    {
        isPointOfInterestActive = true;

        // Si el movimiento automático está activo, lo detenemos
        if (isAutoMoving)
        {
            StopAutoMovementAndRestoreControl();
        }

        if (debugMode) Debug.Log("Punto de interés abierto - Auto-cámara desactivada");
    }

    /// <summary>
    /// Notifica al controlador que un punto de interés o video se ha cerrado.
    /// Reinicia el temporizador de inactividad.
    /// </summary>
    public void OnPointOfInterestClosed()
    {
        isPointOfInterestActive = false;

        // Reiniciar el temporizador de inactividad
        ResetInactivityTimer();

        if (debugMode) Debug.Log("Punto de interés cerrado - Auto-cámara disponible nuevamente");
    }
}