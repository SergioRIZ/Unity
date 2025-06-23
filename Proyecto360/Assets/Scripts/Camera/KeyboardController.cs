using UnityEngine;

/// <summary>
/// Controlador que maneja la entrada de teclado para la navegación panorámica.
/// Permite controlar la rotación de la cámara usando teclas WASD y/o flechas.
/// </summary>
public class KeyboardController : MonoBehaviour
{
    /// <summary>
    /// Habilitar controles de teclado (WASD/Flechas).
    /// </summary>
    [Tooltip("Habilitar controles de teclado (WASD/Flechas)")]
    public bool enableKeyboardControls = true;

    /// <summary>
    /// Velocidad de rotación con teclado.
    /// </summary>
    [Tooltip("Velocidad de rotación con teclado")]
    public float keyboardRotationSpeed = 30.0f;

    /// <summary>
    /// Usar también teclas de dirección.
    /// </summary>
    [Tooltip("Usar también teclas de dirección")]
    public bool useArrowKeys = true;

    /// <summary>
    /// Referencia al controlador principal de la cámara.
    /// </summary>
    private CameraView parentController;

    /// <summary>
    /// Indica si el controlador ha sido inicializado.
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
    /// Procesa la entrada de teclado si está habilitado y no hay gestos táctiles activos.
    /// </summary>
    void Update()
    {
        if (!initialized || parentController == null)
            return;

        if (parentController.IsTransitioning || !enabled || !enableKeyboardControls)
            return;

        // Solo procesar el control de teclado si no hay gestos táctiles activos
        if (Input.touchCount == 0)
        {
            KeyboardControl();
        }
    }

    /// <summary>
    /// Procesa la entrada de teclado para la navegación panorámica.
    /// Permite rotar la cámara usando WASD y/o flechas, normalizando el movimiento diagonal.
    /// </summary>
    private void KeyboardControl()
    {
        float horizontalInput = 0f;
        float verticalInput = 0f;

        // Detectar entrada de teclado (WASD)
        if (Input.GetKey(KeyCode.W) || (useArrowKeys && Input.GetKey(KeyCode.UpArrow)))
        {
            verticalInput += 1f;
        }
        if (Input.GetKey(KeyCode.S) || (useArrowKeys && Input.GetKey(KeyCode.DownArrow)))
        {
            verticalInput -= 1f;
        }
        if (Input.GetKey(KeyCode.A) || (useArrowKeys && Input.GetKey(KeyCode.LeftArrow)))
        {
            horizontalInput -= 1f;
        }
        if (Input.GetKey(KeyCode.D) || (useArrowKeys && Input.GetKey(KeyCode.RightArrow)))
        {
            horizontalInput += 1f;
        }

        // Normalizar el vector de entrada para movimientos diagonales consistentes
        if (horizontalInput != 0f && verticalInput != 0f)
        {
            Vector2 inputVector = new Vector2(horizontalInput, verticalInput).normalized;
            horizontalInput = inputVector.x;
            verticalInput = inputVector.y;
        }

        // Solo actualizar si hay entrada detectada
        if (horizontalInput != 0f || verticalInput != 0f)
        {
            // Actualizar rotaciones acumuladas
            float yRotation = parentController.YRotation + horizontalInput * keyboardRotationSpeed * Time.deltaTime;
            float xRotation = parentController.XRotation - verticalInput * keyboardRotationSpeed * Time.deltaTime;

            // Limitar la rotación vertical para evitar volteo
            xRotation = Mathf.Clamp(xRotation, -parentController.verticalLimit, parentController.verticalLimit);

            // Actualizar los valores en el controlador principal
            parentController.XRotation = xRotation;
            parentController.YRotation = yRotation;

            // Crear la rotación objetivo
            parentController.TargetRotation = Quaternion.Euler(xRotation, yRotation, 0);
        }
    }
}