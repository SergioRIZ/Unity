using UnityEngine;

/// <summary>
/// Controlador para manejar el zoom de la cámara mediante rueda del mouse,
/// pinch en dispositivos táctiles y teclas con movimiento fluido.
/// </summary>
public class ZoomController : MonoBehaviour
{
    [Header("Configuración de Zoom")]
    /// <summary>
    /// Permitir zoom con la rueda del mouse o pinch.
    /// </summary>
    [Tooltip("Permitir zoom con la rueda del mouse o pinch")]
    public bool allowZoom = true;

    /// <summary>
    /// Velocidad de zoom.
    /// </summary>
    [Tooltip("Velocidad de zoom")]
    [Range(1.0f, 50.0f)]
    public float zoomSpeed = 30.0f;

    /// <summary>
    /// Rango de campo de visión (FOV) mínimo.
    /// </summary>
    [Tooltip("Rango de campo de visión (FOV) mínimo")]
    [Range(1.0f, 45.0f)]
    public float minFOV = 20.0f;

    /// <summary>
    /// Rango de campo de visión (FOV) máximo.
    /// </summary>
    [Tooltip("Rango de campo de visión (FOV) máximo")]
    [Range(45.0f, 120.0f)]
    public float maxFOV = 90.0f;

    [Header("Configuración de Teclado")]
    /// <summary>
    /// Velocidad de zoom con teclado.
    /// </summary>
    [Tooltip("Velocidad de zoom con teclado")]
    [Range(1.0f, 120.0f)]
    public float keyZoomSpeed = 60.0f;

    /// <summary>
    /// Tecla para acercar.
    /// </summary>
    [Tooltip("Tecla para acercar")]
    public KeyCode zoomInKey = KeyCode.Plus; // Tecla + para acercar

    /// <summary>
    /// Tecla alternativa para acercar.
    /// </summary>
    [Tooltip("Tecla alternativa para acercar")]
    public KeyCode zoomInKeyAlt = KeyCode.KeypadPlus; // Tecla + del teclado numérico

    /// <summary>
    /// Tecla para alejar.
    /// </summary>
    [Tooltip("Tecla para alejar")]
    public KeyCode zoomOutKey = KeyCode.Minus; // Tecla - para alejar

    /// <summary>
    /// Tecla alternativa para alejar.
    /// </summary>
    [Tooltip("Tecla alternativa para alejar")]
    public KeyCode zoomOutKeyAlt = KeyCode.KeypadMinus; // Tecla - del teclado numérico

    /// <summary>
    /// Usar también las teclas PageUp/PageDown.
    /// </summary>
    [Tooltip("Usar también las teclas PageUp/PageDown")]
    public bool usePageKeys = true;

    [Header("Configuración de Suavizado")]
    /// <summary>
    /// Velocidad de interpolación para suavizado de zoom.
    /// </summary>
    [Tooltip("Velocidad de interpolación para suavizado de zoom")]
    [Range(1.0f, 20.0f)]
    public float smoothSpeed = 15.0f;

    [Header("Configuración de Límites de Pantalla")]
    /// <summary>
    /// Margen de pantalla para detener el zoom (en píxeles).
    /// </summary>
    [Tooltip("Margen de pantalla para detener el zoom (en píxeles)")]
    public float screenEdgeMargin = 10f;

    /// <summary>
    /// Referencia al controlador principal de la cámara.
    /// </summary>
    private CameraView parentController;

    /// <summary>
    /// Valor inicial del campo de visión (FOV) de la cámara.
    /// </summary>
    private float initialFOV;

    /// <summary>
    /// Última distancia registrada entre dos toques para el zoom por pinch.
    /// </summary>
    private float touchZoomLastDistance;

    /// <summary>
    /// Indica si el controlador ha sido inicializado correctamente.
    /// </summary>
    private bool initialized = false;

    /// <summary>
    /// Factor de sensibilidad para el zoom por pinch.
    /// </summary>
    private const float PINCH_SENSITIVITY_FACTOR = 0.03f;

    // Variables para el suavizado del zoom

    /// <summary>
    /// Campo de visión objetivo al que se interpola la cámara.
    /// </summary>
    private float targetFOV;

    /// <summary>
    /// Velocidad actual utilizada por SmoothDamp para el suavizado del FOV.
    /// </summary>
    private float currentVelocity;

    // Rectángulo para detección de límites de pantalla

    /// <summary>
    /// Rectángulo que representa los límites de la pantalla considerando el margen.
    /// </summary>
    private Rect screenRect;

    /// <summary>
    /// Se ejecuta al inicializar el componente. Actualiza el rectángulo de la pantalla.
    /// </summary>
    void Awake()
    {
        UpdateScreenRect();
    }

    /// <summary>
    /// Inicializa el controlador con una referencia al controlador principal.
    /// </summary>
    /// <param name="controller">El controlador de cámara principal.</param>
    public void Initialize(CameraView controller)
    {
        if (controller == null || controller.MainCamera == null)
        {
            Debug.LogError("ZoomController: Se requiere una cámara válida");
            enabled = false;
            return;
        }

        parentController = controller;
        initialFOV = parentController.MainCamera.fieldOfView;
        targetFOV = initialFOV; // Inicializamos el FOV objetivo
        initialized = true;
    }

    /// <summary>
    /// Actualiza el estado del controlador cada frame.
    /// Gestiona el zoom según la plataforma y aplica el suavizado.
    /// </summary>
    void Update()
    {
        if (!initialized || parentController == null)
            return;

        if (parentController.IsTransitioning || !enabled || !allowZoom)
            return;

        // Actualizar el rectángulo de la pantalla si cambia la resolución
        if (screenRect.width != Screen.width || screenRect.height != Screen.height)
        {
            UpdateScreenRect();
        }

        // Usar los métodos apropiados según la plataforma
        if (parentController.IsMobilePlatform)
        {
            // En dispositivos móviles, solo usamos pinch para zoom
            HandlePinchZoom();
        }
        else
        {
            // En PC, usamos rueda de mouse y teclado
            HandleMouseWheelZoom();
            HandleKeyboardZoom();
        }

        // Aplicamos el suavizado en cada frame para una transición fluida
        ApplySmoothZoom();
    }

    /// <summary>
    /// Actualiza el rectángulo que representa los límites de la pantalla.
    /// </summary>
    private void UpdateScreenRect()
    {
        // Crear un rectángulo con margen para detectar cuando se acerca a los bordes
        screenRect = new Rect(
            screenEdgeMargin,
            screenEdgeMargin,
            Screen.width - (screenEdgeMargin * 2),
            Screen.height - (screenEdgeMargin * 2)
        );
    }

    /// <summary>
    /// Verifica si el cursor está dentro de los límites de la pantalla.
    /// </summary>
    /// <returns>True si el cursor está dentro del área válida, false en caso contrario.</returns>
    private bool IsMouseWithinScreen()
    {
        return screenRect.Contains(Input.mousePosition);
    }

    /// <summary>
    /// Maneja el zoom mediante la rueda del mouse con efecto suavizado.
    /// </summary>
    private void HandleMouseWheelZoom()
    {
        // Si el mouse está sobre el ScrollView o fuera de la pantalla, no hacemos zoom
        if (StopZoom.IsPointerOverScrollView || !IsMouseWithinScreen())
            return;

        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            // Amplificamos la entrada para hacerla más sensible
            float zoomAmount = -scrollInput * zoomSpeed * 2.0f;

            // Actualizamos el FOV objetivo
            SetTargetFOV(targetFOV + zoomAmount);
        }
    }

    /// <summary>
    /// Maneja el zoom mediante pinch en dispositivos táctiles.
    /// </summary>
    private void HandlePinchZoom()
    {
        if (Input.touchCount != 2)
            return;

        Touch touchZero = Input.GetTouch(0);
        Touch touchOne = Input.GetTouch(1);

        // Verificamos que ambos toques estén dentro de la pantalla
        Vector2 positionZero = touchZero.position;
        Vector2 positionOne = touchOne.position;

        // Si alguno de los toques está fuera de los límites, no hacemos zoom
        if (!screenRect.Contains(positionZero) || !screenRect.Contains(positionOne))
            return;

        // Si es el comienzo de un pinch, guardamos la distancia inicial
        if (touchZero.phase == TouchPhase.Began || touchOne.phase == TouchPhase.Began)
        {
            touchZoomLastDistance = Vector2.Distance(positionZero, positionOne);
        }
        // De lo contrario, comparamos la distancia actual con la anterior
        else if (touchZero.phase == TouchPhase.Moved || touchOne.phase == TouchPhase.Moved)
        {
            float currentDistance = Vector2.Distance(positionZero, positionOne);
            float deltaMagnitudeDiff = touchZoomLastDistance - currentDistance;

            // Aplicar al zoom con factor de sensibilidad
            float zoomAmount = deltaMagnitudeDiff * PINCH_SENSITIVITY_FACTOR * zoomSpeed;

            // Actualizamos el FOV objetivo
            SetTargetFOV(targetFOV + zoomAmount);

            // Actualizar la última distancia
            touchZoomLastDistance = currentDistance;
        }
    }

    /// <summary>
    /// Gestiona el zoom mediante las teclas del teclado de forma suave.
    /// </summary>
    private void HandleKeyboardZoom()
    {
        // El zoom por teclado funciona siempre, independientemente de la posición del cursor
        // ya que es una entrada global y no depende de la posición del ratón

        // Calculamos la cantidad de zoom basada en tiempo para un efecto suave
        float keyZoomAmount = keyZoomSpeed * Time.deltaTime;

        // Comprobar si alguna de las teclas de zoom está presionada
        bool zoomInPressed = Input.GetKey(zoomInKey) || Input.GetKey(zoomInKeyAlt) ||
                            (usePageKeys && Input.GetKey(KeyCode.PageUp));

        bool zoomOutPressed = Input.GetKey(zoomOutKey) || Input.GetKey(zoomOutKeyAlt) ||
                             (usePageKeys && Input.GetKey(KeyCode.PageDown));

        // Aplicar zoom suave mientras se mantiene presionada la tecla
        if (zoomInPressed)
        {
            SetTargetFOV(targetFOV - keyZoomAmount); // Negativo para acercar (reducir FOV)
        }

        if (zoomOutPressed)
        {
            SetTargetFOV(targetFOV + keyZoomAmount); // Positivo para alejar (aumentar FOV)
        }
    }

    /// <summary>
    /// Establece el FOV objetivo dentro de los límites permitidos.
    /// </summary>
    /// <param name="newTargetFOV">El nuevo FOV objetivo.</param>
    private void SetTargetFOV(float newTargetFOV)
    {
        targetFOV = Mathf.Clamp(newTargetFOV, minFOV, maxFOV);
    }

    /// <summary>
    /// Aplica el zoom de forma suave usando interpolación.
    /// </summary>
    private void ApplySmoothZoom()
    {
        if (parentController == null || parentController.MainCamera == null)
            return;

        // No hacemos nada si ya estamos en el objetivo
        if (Mathf.Approximately(parentController.MainCamera.fieldOfView, targetFOV))
            return;

        // Aplicamos una interpolación suave para un movimiento fluido
        float newFOV = Mathf.SmoothDamp(
            parentController.MainCamera.fieldOfView,
            targetFOV,
            ref currentVelocity,
            1.0f / smoothSpeed
        );

        parentController.MainCamera.fieldOfView = newFOV;
    }

    /// <summary>
    /// Restablece el zoom al valor inicial de forma suave.
    /// </summary>
    public void ResetZoom()
    {
        if (parentController != null && parentController.MainCamera != null)
        {
            SetTargetFOV(initialFOV);
        }
    }
}