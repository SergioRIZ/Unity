using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Asegura que el GameObject tenga RectTransform y Button.
/// Proporciona efectos visuales y de movimiento para un botón, como parpadeo, flotación y vibración.
/// </summary>
[RequireComponent(typeof(RectTransform), typeof(Button))]
public class AnimatedButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// Configuración de colores para el botón.
    /// </summary>
    [System.Serializable]
    public class ColorSettings
    {
        /// <summary>
        /// Color por defecto del botón.
        /// </summary>
        public Color normalColor = Color.white;

        /// <summary>
        /// Color al pasar el mouse sobre el botón.
        /// </summary>
        public Color hoverColor = Color.gray;
    }

    /// <summary>
    /// Configuración del parpadeo.
    /// </summary>
    [System.Serializable]
    public class BlinkSettings
    {
        /// <summary>Indica si el parpadeo está activado.</summary>
        public bool enableBlinking = false;

        /// <summary>Velocidad del parpadeo (cambio de opacidad).</summary>
        public float blinkSpeed = 1f;
    }

    /// <summary>
    /// Configuración de vibración.
    /// </summary>
    [System.Serializable]
    public class VibrationSettings
    {
        /// <summary>Indica si la vibración está activada.</summary>
        public bool enableVibration = false;

        /// <summary>Intensidad de la vibración.</summary>
        public float vibrationAmount = 2f;

        /// <summary>Velocidad de la vibración (no implementada).</summary>
        public float vibrationSpeed = 20f;
    }

    /// <summary>
    /// Configuración de flotación.
    /// </summary>
    [System.Serializable]
    public class FloatSettings
    {
        /// <summary>Indica si la flotación vertical está activada.</summary>
        public bool enableFloating = false;

        /// <summary>Altura del movimiento vertical.</summary>
        public float floatHeight = 5f;

        /// <summary>Velocidad del movimiento vertical.</summary>
        public float floatSpeed = 2f;

        /// <summary>Indica si la flotación lateral está activada.</summary>
        public bool enableLateralFloat = false;

        /// <summary>Rango del movimiento lateral.</summary>
        public float lateralFloatAmount = 3f;

        /// <summary>Velocidad del movimiento lateral.</summary>
        public float lateralFloatSpeed = 1.5f;
    }

    [Header("Configuraciones de Color")]
    public ColorSettings colorSettings; // Configuración de colores del botón

    [Header("Efecto de Encendido/Apagado")]
    public BlinkSettings blinkSettings; // Configuración del efecto de parpadeo

    [Header("Vibración")]
    public VibrationSettings vibrationSettings; // Configuración del efecto de vibración

    [Header("Flotación")]
    public FloatSettings floatSettings; // Configuración del efecto de flotación

    private Image buttonImage;               // Referencia al componente Image del botón
    private RectTransform rectTransform;     // Referencia al RectTransform del botón
    private Vector3 originalPosition;        // Posición original del botón
    private float blinkTimer = 0f;           // Temporizador para el efecto de parpadeo

    private bool isVibrating = false;        // Indica si el botón está vibrando actualmente

    /// <summary>
    /// Inicializa las referencias a los componentes y establece el color inicial del botón.
    /// </summary>
    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        buttonImage = GetComponent<Image>();
        originalPosition = rectTransform.anchoredPosition;

        if (buttonImage != null)
        {
            buttonImage.color = colorSettings.normalColor;
        }
    }

    /// <summary>
    /// Actualiza los efectos activos según las configuraciones.
    /// </summary>
    private void Update()
    {
        if (blinkSettings.enableBlinking)
        {
            BlinkEffect();
        }

        if (floatSettings.enableFloating)
        {
            FloatEffect();
        }

        if (floatSettings.enableLateralFloat)
        {
            LateralFloatEffect();
        }

        if (vibrationSettings.enableVibration && isVibrating)
        {
            Vibrate();
        }
    }

    /// <summary>
    /// Aplica el efecto de parpadeo cambiando la opacidad del botón.
    /// </summary>
    private void BlinkEffect()
    {
        blinkTimer += Time.deltaTime * blinkSettings.blinkSpeed;
        float alpha = Mathf.PingPong(blinkTimer, 1f);
        if (buttonImage != null)
        {
            Color newColor = buttonImage.color;
            newColor.a = alpha;
            buttonImage.color = newColor;
        }
    }

    /// <summary>
    /// Aplica el efecto de flotación vertical.
    /// </summary>
    private void FloatEffect()
    {
        float newY = originalPosition.y + Mathf.Sin(Time.time * floatSettings.floatSpeed) * floatSettings.floatHeight;
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, newY);
    }

    /// <summary>
    /// Aplica el efecto de flotación lateral.
    /// </summary>
    private void LateralFloatEffect()
    {
        float newX = originalPosition.x + Mathf.Sin(Time.time * floatSettings.lateralFloatSpeed) * floatSettings.lateralFloatAmount;
        rectTransform.anchoredPosition = new Vector2(newX, rectTransform.anchoredPosition.y);
    }

    /// <summary>
    /// Evento al entrar el puntero del mouse. Cambia el color y activa la vibración si está configurada.
    /// </summary>
    /// <param name="eventData">Datos del evento del puntero.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buttonImage != null)
        {
            buttonImage.color = colorSettings.hoverColor;
        }

        if (vibrationSettings.enableVibration && !isVibrating)
        {
            isVibrating = true;
        }
    }

    /// <summary>
    /// Evento al salir el puntero del mouse. Restaura el color y detiene la vibración.
    /// </summary>
    /// <param name="eventData">Datos del evento del puntero.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (buttonImage != null)
        {
            buttonImage.color = colorSettings.normalColor;
        }

        if (vibrationSettings.enableVibration)
        {
            isVibrating = false;
            rectTransform.anchoredPosition = originalPosition;
        }
    }

    /// <summary>
    /// Aplica el efecto de vibración aleatoria al botón.
    /// </summary>
    private void Vibrate()
    {
        float randomX = Random.Range(-vibrationSettings.vibrationAmount, vibrationSettings.vibrationAmount);
        float randomY = Random.Range(-vibrationSettings.vibrationAmount, vibrationSettings.vibrationAmount);
        rectTransform.anchoredPosition = originalPosition + new Vector3(randomX, randomY, 0);
    }
}
