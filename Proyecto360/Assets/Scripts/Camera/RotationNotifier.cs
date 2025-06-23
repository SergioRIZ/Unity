using System;
using UnityEngine;

/// <summary>
/// Componente que notifica los cambios de rotación a otros sistemas.
/// Permite suscribirse a eventos de cambio de rotación, aplicando un umbral de cambio significativo
/// y un intervalo mínimo entre notificaciones para evitar sobrecarga.
/// </summary>
public class RotationNotifier : MonoBehaviour
{
    /// <summary>
    /// Intervalo mínimo entre notificaciones de cambio de rotación, en segundos.
    /// </summary>
    [Tooltip("Intervalo mínimo entre notificaciones (segundos)")]
    public float notificationThrottleTime = 0.05f; // 50ms por defecto

    /// <summary>
    /// Distancia mínima (en grados) para considerar un cambio de rotación como significativo.
    /// </summary>
    [Tooltip("Distancia mínima para considerar un cambio significativo")]
    public float significantChangeThreshold = 0.1f;

    /// <summary>
    /// Referencia al controlador principal de la cámara.
    /// </summary>
    private CameraView parentController;

    /// <summary>
    /// Última rotación reportada a los suscriptores.
    /// </summary>
    private Vector3 lastReportedRotation;

    /// <summary>
    /// Marca de tiempo del último evento de notificación.
    /// </summary>
    private float lastEventTimestamp = 0f;

    /// <summary>
    /// Indica si el notificador ha sido inicializado correctamente.
    /// </summary>
    private bool initialized = false;

    /// <summary>
    /// Evento que notifica a los suscriptores cuando la rotación cambia significativamente.
    /// El evento se dispara con throttling según <see cref="notificationThrottleTime"/>.
    /// </summary>
    public event Action<Vector3> OnRotationChanged;

    /// <summary>
    /// Inicializa el notificador con una referencia al controlador principal.
    /// </summary>
    /// <param name="controller">El controlador de cámara principal.</param>
    public void Initialize(CameraView controller)
    {
        parentController = controller;
        lastReportedRotation = transform.eulerAngles;
        initialized = true;
    }

    /// <summary>
    /// Llamado una vez por frame. Verifica si debe notificar cambios de rotación.
    /// </summary>
    void Update()
    {
        if (!initialized || !enabled)
            return;

        NotifyRotationChanges();
    }

    /// <summary>
    /// Verifica los cambios de rotación y notifica a los suscriptores si el cambio es significativo
    /// y ha pasado el tiempo mínimo de espera entre notificaciones.
    /// </summary>
    private void NotifyRotationChanges()
    {
        // Si no hay suscriptores, no hay necesidad de procesar
        if (OnRotationChanged == null)
            return;

        // Throttling del evento para evitar sobrecarga
        if (Time.time - lastEventTimestamp < notificationThrottleTime)
            return;

        Vector3 currentEuler = transform.eulerAngles;

        // Calcular la diferencia entre rotaciones
        float rotationDifference = Vector3.Distance(lastReportedRotation, currentEuler);

        // Solo disparar el evento si la rotación cambió significativamente
        if (rotationDifference > significantChangeThreshold)
        {
            lastReportedRotation = currentEuler;
            OnRotationChanged.Invoke(currentEuler);
            lastEventTimestamp = Time.time;
        }
    }
}