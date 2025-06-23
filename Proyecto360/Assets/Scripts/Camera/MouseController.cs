using UnityEngine;

/// <summary>
/// Controlador para la entrada de mouse en vista panorámica 360 con opciones para cursor visible/invisible
/// y detección de límites de pantalla.
/// </summary>
public class MouseController : InputController
{
    /// <summary>
    /// Botón del mouse para controlar la cámara (0=izquierdo, 1=derecho, 2=medio).
    /// </summary>
    [Tooltip("Botón del mouse para controlar la cámara")]
    public int controlButton = 0;

    /// <summary>
    /// Multiplicador de sensibilidad del movimiento del mouse.
    /// </summary>
    [Tooltip("Sensibilidad del movimiento")]
    private float sensitivityMultiplier = 0.3f;

    /// <summary>
    /// Margen de pantalla para detener el movimiento (en píxeles).
    /// </summary>
    [Tooltip("Margen de pantalla para detener el movimiento (en píxeles)")]
    public float screenEdgeMargin = 10f;

    [Header("Opciones de Cursor")]
    /// <summary>
    /// Indica si se debe ocultar el cursor mientras se navega.
    /// </summary>
    [Tooltip("Ocultar el cursor mientras se navega")]
    public bool hideCursorWhileNavigating = false; // Cambiado a false para mantener el cursor visible

    /// <summary>
    /// Indica si se debe bloquear el cursor en el centro de la pantalla durante la navegación.
    /// </summary>
    [Tooltip("Bloquear el cursor en el centro de la pantalla durante la navegación")]
    public bool lockCursorToCenter = false;

    /// <summary>
    /// Última posición registrada del mouse.
    /// </summary>
    private Vector2 lastMousePosition;

    /// <summary>
    /// Indica si el usuario está arrastrando con el mouse.
    /// </summary>
    private bool isDragging = false;

    /// <summary>
    /// Evita la rotación de la cámara en el primer frame tras iniciar el arrastre.
    /// </summary>
    private bool avoidCameraRotation = false;

    /// <summary>
    /// Rectángulo que representa los límites de la pantalla considerando el margen.
    /// </summary>
    private Rect screenRect;

    /// <summary>
    /// Estado original del modo de bloqueo del cursor.
    /// </summary>
    private CursorLockMode originalLockMode;

    /// <summary>
    /// Estado original de visibilidad del cursor.
    /// </summary>
    private bool originalCursorVisibility;

    /// <summary>
    /// Indica si el cursor fue ocultado durante la navegación.
    /// </summary>
    private bool cursorWasHidden = false;

    /// <inheritdoc/>
    protected override void Awake()
    {
        base.Awake();
        UpdateScreenRect();

        // Guardar el estado inicial del cursor
        originalLockMode = Cursor.lockState;
        originalCursorVisibility = Cursor.visible;
    }

    /// <inheritdoc/>
    protected override void Update()
    {
        base.Update();

        if (!isInitialized || parentController == null)
            return;

        if (parentController.IsTransitioning || !enabled || parentController.isCentered)
            return;

        // Actualizar el rectángulo de la pantalla si cambia la resolución
        if (screenRect.width != Screen.width || screenRect.height != Screen.height)
        {
            UpdateScreenRect();
        }

        // Solo procesar el control de mouse si no hay gestos táctiles activos
        if (Input.touchCount == 0)
        {
            MouseControl();
        }
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
    /// Procesa la entrada del mouse para la navegación panorámica.
    /// </summary>
    private void MouseControl()
    {
        // Verificar si el cursor está dentro de los límites de la pantalla
        Vector2 mousePos = Input.mousePosition;
        bool isMouseWithinScreen = screenRect.Contains(mousePos);

        // Mostrar cursor si sale de los límites mientras estaba oculto
        if (isDragging && hideCursorWhileNavigating && !lockCursorToCenter && !isMouseWithinScreen && cursorWasHidden)
        {
            Cursor.visible = true;
            cursorWasHidden = false;
        }
        // Ocultar cursor si vuelve a entrar y está configurado para ocultarse
        else if (isDragging && hideCursorWhileNavigating && !lockCursorToCenter && isMouseWithinScreen && !cursorWasHidden)
        {
            Cursor.visible = false;
            cursorWasHidden = true;
        }

        // Manejar el inicio del control
        if (Input.GetMouseButtonDown(controlButton) && isMouseWithinScreen)
        {
            HandleMouseButtonDown();
        }

        // Manejar el control continuo solo si estamos arrastrando
        if (Input.GetMouseButton(controlButton) && isDragging)
        {
            // Si el cursor está bloqueado, siempre procesamos el movimiento
            // Si no está bloqueado, solo procesamos si está dentro de la pantalla
            if (lockCursorToCenter || isMouseWithinScreen)
            {
                HandleMouseMovement();
            }
        }

        // Si el mouse sale de la pantalla mientras se arrastra y no está bloqueado, 
        // actualizamos la última posición al borde
        if (isDragging && !isMouseWithinScreen && !lockCursorToCenter)
        {
            lastMousePosition = ClampToScreenEdge(mousePos);
        }

        // Manejar el fin del control
        if (Input.GetMouseButtonUp(controlButton))
        {
            HandleMouseButtonUp();
        }
    }

    /// <summary>
    /// Limita una posición al borde de la pantalla más cercano.
    /// </summary>
    /// <param name="position">Posición a limitar.</param>
    /// <returns>Posición ajustada dentro de los márgenes de la pantalla.</returns>
    private Vector2 ClampToScreenEdge(Vector2 position)
    {
        return new Vector2(
            Mathf.Clamp(position.x, screenEdgeMargin, Screen.width - screenEdgeMargin),
            Mathf.Clamp(position.y, screenEdgeMargin, Screen.height - screenEdgeMargin)
        );
    }

    /// <summary>
    /// Maneja el evento de presionar el botón del mouse.
    /// Inicializa el arrastre y gestiona el estado del cursor.
    /// </summary>
    private void HandleMouseButtonDown()
    {
        if (StopZoom.IsPointerOverScrollView || StopMovementPanel.IsPointerOverPanel) return;
        // Guardar la posición inicial del ratón para calcular el desplazamiento
        lastMousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        isDragging = true;
        avoidCameraRotation = true;

        // Si está configurado para ocultar/bloquear el cursor
        if (lockCursorToCenter)
        {
            // Bloquear en el centro para uso con GetAxis
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = !hideCursorWhileNavigating;
            cursorWasHidden = hideCursorWhileNavigating;
        }
        else if (hideCursorWhileNavigating)
        {
            // Solo ocultar cursor sin bloquear
            Cursor.visible = false;
            cursorWasHidden = true;
        }
    }

    /// <summary>
    /// Maneja el movimiento del mouse durante el control.
    /// Calcula el desplazamiento y actualiza la rotación de la cámara.
    /// </summary>
    private void HandleMouseMovement()
    {
        if (avoidCameraRotation)
        {
            // Ignoramos el primer movimiento de ratón para evitar saltos
            avoidCameraRotation = false;

            // Si el cursor está bloqueado, usamos GetAxis directamente
            if (lockCursorToCenter)
            {
                Input.GetAxis("Mouse X");
                Input.GetAxis("Mouse Y");
            }
            else
            {
                // Si no está bloqueado, actualizamos la última posición
                lastMousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            }
            return;
        }

        float mouseX, mouseY;

        // El método para obtener el movimiento del ratón depende del modo de bloqueo
        if (lockCursorToCenter)
        {
            // Con cursor bloqueado, usamos GetAxis directamente
            mouseX = Input.GetAxis("Mouse X") * parentController.horizontalSpeed * sensitivityMultiplier;
            mouseY = Input.GetAxis("Mouse Y") * parentController.verticalSpeed * sensitivityMultiplier;
        }
        else
        {
            // Con cursor libre, calculamos manualmente el desplazamiento
            Vector2 currentMousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            Vector2 mouseDelta = currentMousePosition - lastMousePosition;

            float scaleFactor = 0.2f * sensitivityMultiplier;
            mouseX = mouseDelta.x * scaleFactor * parentController.horizontalSpeed;
            mouseY = mouseDelta.y * scaleFactor * parentController.verticalSpeed;

            // Actualizar la posición para el siguiente frame
            lastMousePosition = currentMousePosition;
        }

        // Actualizar rotaciones acumuladas
        float yRotation = parentController.YRotation + mouseX;
        float xRotation = parentController.XRotation - mouseY;

        // Limitar la rotación vertical para evitar volteo
        xRotation = Mathf.Clamp(xRotation, -parentController.verticalLimit, parentController.verticalLimit);

        // Actualizar los valores en el controlador principal
        parentController.XRotation = xRotation;
        parentController.YRotation = yRotation;

        // Crear la rotación objetivo
        parentController.TargetRotation = Quaternion.Euler(xRotation, yRotation, 0);
    }

    /// <summary>
    /// Maneja el evento de soltar el botón del mouse.
    /// Finaliza el arrastre y restaura el estado del cursor.
    /// </summary>
    private void HandleMouseButtonUp()
    {
        isDragging = false;
        cursorWasHidden = false;

        // Restaurar el cursor
        UnlockCursor();
    }

    /// <summary>
    /// Desbloquea el cursor y restaura su visibilidad.
    /// </summary>
    private void UnlockCursor()
    {
        // Restaurar el estado del cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <inheritdoc/>
    protected override void OnDisable()
    {
        base.OnDisable();

        // Restaurar el cursor al desactivar
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isDragging = false;
        cursorWasHidden = false;
    }

    /// <inheritdoc/>
    protected override void OnEnable()
    {
        base.OnEnable();

        // Al habilitar, asegurarnos que el cursor está visible
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// Ignora el siguiente delta de movimiento del mouse para evitar saltos.
    /// </summary>
    public void IgnoreNextMouseDelta()
    {
        avoidCameraRotation = true;
    }

    /// <summary>
    /// Inicializa el controlador con una referencia al controlador principal.
    /// </summary>
    /// <param name="controller">El controlador de cámara principal.</param>
    public override void Initialize(CameraView controller)
    {
        base.Initialize(controller);

        // Mantener nuestras propias configuraciones
        hideCursorWhileNavigating = false; // Asegurarse de que el cursor siempre sea visible
        controlButton = 0; // Usar el botón izquierdo del mouse
    }
}