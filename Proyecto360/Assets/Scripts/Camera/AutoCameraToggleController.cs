using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Controlador para un toggle de UI que activa/desactiva el movimiento automático de la cámara.
/// Proporciona opciones visuales y funcionales para controlar el script AutomaticMovementController.
/// MODIFICADO: Corregido para manejar el estado inicial desactivado.
/// </summary>
public class AutoCameraToggleController : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Referencia al controlador de movimiento automático")]
    public AutomaticMovementController automaticMovement;

    [Tooltip("Referencia al Toggle UI")]
    public Toggle toggleControl;

    [Tooltip("Texto para mostrar el estado (opcional)")]
    public TextMeshProUGUI statusText;

    [Header("Configuración")]
    [Tooltip("Texto cuando está activado")]
    public string enabledText = "Auto-cámara: ACTIVADA";

    [Tooltip("Texto cuando está desactivado")]
    public string disabledText = "Auto-cámara: DESACTIVADA";

    [Tooltip("Color cuando está activado")]
    public Color enabledColor = new Color(0.2f, 0.8f, 0.2f);

    [Tooltip("Color cuando está desactivado")]
    public Color disabledColor = new Color(0.8f, 0.2f, 0.2f);

    [Tooltip("Buscar automáticamente referencias en Start() si no están asignadas")]
    public bool autoFindReferences = true;

    [Tooltip("Forzar el toggle a estar desactivado al inicio")]
    public bool forceToggleOffAtStart = true;

    /// <summary>
    /// Inicializa referencias y configura el estado inicial del toggle antes de que se renderice.
    /// </summary>
    private void Awake()
    {
        // Buscar referencias si es necesario
        if (autoFindReferences)
        {
            if (automaticMovement == null)
                automaticMovement = FindObjectOfType<AutomaticMovementController>();

            if (toggleControl == null)
                toggleControl = GetComponent<Toggle>();
        }

        // Configurar el toggle como desactivado ANTES de que se renderice
        if (toggleControl != null && forceToggleOffAtStart)
        {
            toggleControl.isOn = false;
        }
    }

    /// <summary>
    /// Verifica referencias y comienza la inicialización del estado del toggle.
    /// </summary>
    private void Start()
    {
        // Verificar que tenemos las referencias necesarias
        if (automaticMovement == null)
        {
            Debug.LogError("AutoCameraToggleController: No se encontró el componente AutomaticMovementController");
            enabled = false;
            return;
        }

        if (toggleControl == null)
        {
            Debug.LogError("AutoCameraToggleController: No se encontró el componente Toggle");
            enabled = false;
            return;
        }

        // Usar una corrutina para asegurar que el AutomaticMovementController se haya inicializado completamente
        StartCoroutine(InitializeToggleState());
    }

    /// <summary>
    /// Corrutina que espera un frame para asegurar que todos los componentes estén inicializados.
    /// </summary>
    private IEnumerator InitializeToggleState()
    {
        // Esperar un frame para que el AutomaticMovementController termine su inicialización
        yield return null;

        // Asegurar que el AutomaticMovementController esté desactivado si forzamos el toggle off
        if (forceToggleOffAtStart)
        {
            automaticMovement.enabled = false;
        }

        // El toggle ya debería estar configurado como false desde Awake()
        // Solo necesitamos añadir el listener y actualizar el texto

        // Añadir listener para el evento de cambio del toggle
        toggleControl.onValueChanged.AddListener(OnToggleValueChanged);

        // Actualizar texto inicial
        UpdateStatusText(toggleControl.isOn);

        Debug.Log($"AutoCameraToggleController inicializado - Toggle: {toggleControl.isOn}, AutoMovement.enabled: {automaticMovement.enabled}");
    }

    /// <summary>
    /// Manejador del evento onValueChanged del Toggle.
    /// Activa o desactiva el movimiento automático y actualiza el estado visual.
    /// </summary>
    /// <param name="isOn">Estado actual del toggle</param>
    private void OnToggleValueChanged(bool isOn)
    {
        Debug.Log($"Toggle cambiado a: {isOn}");

        // Activar o desactivar el controlador de movimiento automático
        automaticMovement.enabled = isOn;

        // Si está activado, reiniciar inactividad para iniciar el tour
        if (isOn)
        {
            // Opcional: iniciar inmediatamente el tour automático
            // automaticMovement.ForceStartTour();
        }
        else
        {
            // Si está desactivado, asegurarse de que el movimiento automático se detiene
            automaticMovement.ForceStopTour();
        }

        // Actualizar texto de estado
        UpdateStatusText(isOn);
    }

    /// <summary>
    /// Actualiza el texto de estado y el color según el valor del toggle.
    /// </summary>
    /// <param name="isOn">Estado actual del toggle</param>
    private void UpdateStatusText(bool isOn)
    {
        if (statusText != null)
        {
            statusText.text = isOn ? enabledText : disabledText;
            statusText.color = isOn ? enabledColor : disabledColor;
        }
    }

    /// <summary>
    /// Activa programáticamente el movimiento automático.
    /// </summary>
    public void EnableAutomaticMovement()
    {
        SetToggleValue(true);
    }

    /// <summary>
    /// Desactiva programáticamente el movimiento automático.
    /// </summary>
    public void DisableAutomaticMovement()
    {
        SetToggleValue(false);
    }

    /// <summary>
    /// Cambia el valor del toggle programáticamente.
    /// </summary>
    /// <param name="value">Nuevo valor para el toggle</param>
    private void SetToggleValue(bool value)
    {
        if (toggleControl != null && toggleControl.isOn != value)
        {
            toggleControl.isOn = value;
            // No necesitamos llamar a OnToggleValueChanged porque el listener lo hará automáticamente
        }
    }

    /// <summary>
    /// Verifica si el movimiento automático de la cámara está activado.
    /// </summary>
    /// <returns>True si el movimiento automático está habilitado, false en caso contrario.</returns>
    public bool IsAutoCameraEnabled()
    {
        return automaticMovement != null && automaticMovement.enabled;
    }

    /// <summary>
    /// Sincroniza manualmente el estado del toggle con el estado del AutomaticMovementController.
    /// </summary>
    public void SyncToggleWithAutoMovement()
    {
        if (automaticMovement != null && toggleControl != null)
        {
            bool currentState = automaticMovement.enabled;
            if (toggleControl.isOn != currentState)
            {
                toggleControl.SetIsOnWithoutNotify(currentState);
                UpdateStatusText(currentState);
            }
        }
    }
}