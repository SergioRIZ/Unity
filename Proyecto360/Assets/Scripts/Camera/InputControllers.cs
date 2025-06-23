using UnityEngine;

/// <summary>
/// Clase base abstracta para todos los controladores de entrada de cámara.
/// Proporciona lógica común de inicialización, verificación de estado y habilitación/deshabilitación.
/// Las clases derivadas deben sobrescribir los métodos relevantes para implementar el control específico.
/// </summary>
public abstract class InputController : MonoBehaviour
{   
    /// <summary>
    /// Referencia al controlador principal de la cámara.
    /// </summary>
    protected CameraView parentController;

    /// <summary>
    /// Indica si el controlador ha sido inicializado correctamente.
    /// </summary>
    protected bool isInitialized = false;

    /// <summary>
    /// Inicializa el controlador con una referencia al controlador principal.
    /// </summary>
    /// <param name="controller">El controlador de cámara principal.</param>
    public virtual void Initialize(CameraView controller)
    {
        parentController = controller;
        isInitialized = true;
        enabled = true;
    }

    /// <summary>
    /// Método de seguridad para verificar la inicialización del controlador.
    /// Si no está inicializado, intenta encontrar y asignar automáticamente un <see cref="CameraView"/> en la escena.
    /// </summary>
    /// <returns>True si la inicialización es correcta o se ha realizado correctamente; false si no se pudo inicializar.</returns>
    protected virtual bool CheckInitialization()
    {
        if (!isInitialized || parentController == null)
        {
            Debug.LogWarning($"El controlador {this.GetType().Name} no está inicializado correctamente. Buscando CameraView...");

            // Intentar encontrar el controlador en la escena
            CameraView foundController = FindObjectOfType<CameraView>();
            if (foundController != null)
            {
                Initialize(foundController);
                Debug.Log($"Se encontró y asignó automáticamente CameraView para {this.GetType().Name}");
                return true;
            }
            else
            {
                Debug.LogError($"No se encontró ningún CameraView en la escena para {this.GetType().Name}");
                enabled = false;
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Habilita o deshabilita este controlador de entrada.
    /// </summary>
    /// <param name="enabled">Si el controlador debe estar habilitado.</param>
    public virtual void SetEnabled(bool enabled)
    {
        this.enabled = enabled;
    }

    /// <summary>
    /// Método llamado al inicializar el componente.
    /// No realiza ninguna acción por defecto; la inicialización se realiza desde <see cref="CameraView"/>.
    /// </summary>
    protected virtual void Awake()
    {
        // No hacemos nada aquí - la inicialización se hará desde CameraView
    }

    /// <summary>
    /// Método llamado al iniciar el componente.
    /// Verifica la inicialización en caso de que no se haya llamado a <see cref="Initialize(CameraView)"/>.
    /// </summary>
    protected virtual void Start()
    {
        // Verificar inicialización en caso de que no se haya llamado a Initialize
        if (!isInitialized)
        {
            CheckInitialization();
        }
    }

    /// <summary>
    /// Método llamado cuando el componente se habilita.
    /// Verifica la inicialización si aún no se ha realizado.
    /// </summary>
    protected virtual void OnEnable()
    {
        // Verificar inicialización cuando se habilita
        if (!isInitialized)
        {
            CheckInitialization();
        }
    }

    /// <summary>
    /// Método de actualización llamado en cada frame.
    /// Verifica la inicialización antes de ejecutar la lógica de entrada.
    /// Las clases derivadas deben sobrescribir este método para implementar el control específico.
    /// </summary>
    protected virtual void Update()
    {
        // Verificamos la inicialización al principio de Update
        if (!CheckInitialization())
        {
            return;
        }

        // Cada clase derivada sobrescribirá este método
    }

    /// <summary>
    /// Método llamado cuando el componente se deshabilita.
    /// Proporciona un punto de extensión para limpieza en clases derivadas.
    /// </summary>
    protected virtual void OnDisable()
    {
        // Código de limpieza base - las clases derivadas pueden ampliarlo
    }
}