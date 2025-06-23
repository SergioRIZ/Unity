using UnityEngine;

/// <summary>
/// Controlador que maneja la entrada táctil para dispositivos móviles.
/// Permite rotar la cámara mediante gestos de swipe.
/// </summary>
public class MobileController : MonoBehaviour
{
    /// <summary>
    /// Velocidad de swipe en dispositivos táctiles.
    /// </summary>
    [Tooltip("Velocidad de swipe en dispositivos táctiles")]
    public float touchSensitivity = 1.0f;

    /// <summary>
    /// Usar controles táctiles en todos los dispositivos, no solo en móviles.
    /// </summary>
    [Tooltip("Usar controles táctiles en todos los dispositivos")]
    public bool forceEnableTouchControls = false;

    /// <summary>
    /// Referencia al controlador principal de la cámara.
    /// </summary>
    private CameraView parentController;

    /// <summary>
    /// Última posición registrada del toque.
    /// </summary>
    private Vector2 touchLastPos;

    /// <summary>
    /// Indica si actualmente hay un toque activo.
    /// </summary>
    private bool isTouching = false;

    /// <summary>
    /// Indica si el controlador ha sido inicializado correctamente.
    /// </summary>
    private bool initialized = false;

    /// <summary>
    /// Inicializa el controlador con una referencia al controlador principal.
    /// </summary>
    /// <param name="controller">El controlador de cámara principal.</param>
    public void Initialize(CameraView controller)
    {
        parentController = controller;
        initialized = true;
    }

    /// <summary>
    /// Actualiza el estado del controlador cada frame.
    /// Procesa la entrada táctil si está inicializado y habilitado.
    /// </summary>
    void Update()
    {
        if (!initialized || parentController == null)
            return;

        if (parentController.IsTransitioning || !enabled)
            return;

        // Solo procesar el control táctil si hay toques activos
        if (Input.touchCount > 0)
        {
            TouchControl();
        }
        else
        {
            // Resetear estado de toque si no hay toques
            isTouching = false;
        }
    }

    /// <summary>
    /// Procesa la entrada táctil para la navegación.
    /// Solo maneja un toque (swipe) para rotar la cámara.
    /// </summary>
    private void TouchControl()
    {
        // Si no hay toques o más de un toque (podría ser zoom), salir
        if (Input.touchCount == 0 || Input.touchCount > 1)
        {
            isTouching = false;
            return;
        }

        // Manejar rotación con un solo toque (swipe)
        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                HandleTouchBegan(touch);
                break;

            case TouchPhase.Moved:
                HandleTouchMoved(touch);
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                isTouching = false;
                break;
        }
    }

    /// <summary>
    /// Maneja el inicio del toque.
    /// Guarda la posición inicial del toque y activa el estado de toque.
    /// </summary>
    /// <param name="touch">Estructura Touch con la información del toque.</param>
    private void HandleTouchBegan(Touch touch)
    {
        touchLastPos = touch.position;
        isTouching = true;
    }

    /// <summary>
    /// Maneja el movimiento del toque.
    /// Calcula la diferencia de posición y actualiza la rotación de la cámara.
    /// </summary>
    /// <param name="touch">Estructura Touch con la información del toque.</param>
    private void HandleTouchMoved(Touch touch)
    {
        if (!isTouching)
            return;

        Vector2 touchDelta = touch.position - touchLastPos;

        // Multiplicamos por la sensibilidad y normalizamos por el tamaño de pantalla
        float touchX = touchDelta.x * touchSensitivity / Screen.width;
        float touchY = touchDelta.y * touchSensitivity / Screen.height;

        // Factor de ajuste para que se sienta bien
        const float adjustmentFactor = 100f;

        // Actualizar rotaciones acumuladas
        float yRotation = parentController.YRotation + touchX * parentController.horizontalSpeed * adjustmentFactor;
        float xRotation = parentController.XRotation - touchY * parentController.verticalSpeed * adjustmentFactor;

        // Limitar la rotación vertical para evitar volteo
        xRotation = Mathf.Clamp(xRotation, -parentController.verticalLimit, parentController.verticalLimit);

        // Actualizar los valores en el controlador principal
        parentController.XRotation = xRotation;
        parentController.YRotation = yRotation;

        // Crear la rotación objetivo
        parentController.TargetRotation = Quaternion.Euler(xRotation, yRotation, 0);

        // Actualizar la última posición
        touchLastPos = touch.position;
    }
}