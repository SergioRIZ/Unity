using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Esta clase controla el efecto de aparición/desaparición suave del handle de un scrollbar
/// cuando el mouse entra o sale del área del ScrollView.
/// </summary>
public class TransitionScrollBar : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// Referencia al componente Scrollbar (se puede asignar desde el Inspector).
    /// </summary>
    public Scrollbar scrollbar;

    /// <summary>
    /// Duración de la animación de desvanecimiento (fade) en segundos.
    /// </summary>
    public float fadeDuration = 0.3f;

    /// <summary>
    /// Referencia al CanvasGroup del handle del Scrollbar, usada para controlar la opacidad.
    /// </summary>
    private CanvasGroup handleCanvasGroup;

    /// <summary>
    /// Referencia a una corrutina en ejecución, para evitar que se solapen varias fades.
    /// </summary>
    private Coroutine fadeCoroutine;

    /// <summary>
    /// Inicializa las referencias necesarias y configura el estado inicial del handle.
    /// </summary>
    private void Start()
    {
        // Si no se ha asignado el scrollbar en el inspector, intenta obtenerlo del mismo GameObject
        if (scrollbar == null)
        {
            scrollbar = GetComponent<Scrollbar>();
        }

        // Si el scrollbar existe y tiene un handle (handleRect)
        if (scrollbar != null && scrollbar.handleRect != null)
        {
            // Intenta obtener el CanvasGroup del handle
            handleCanvasGroup = scrollbar.handleRect.GetComponent<CanvasGroup>();

            // Si no tiene uno, se lo agregamos
            if (handleCanvasGroup == null)
            {
                handleCanvasGroup = scrollbar.handleRect.gameObject.AddComponent<CanvasGroup>();
            }

            // Ocultamos el handle al inicio (alpha en 0 = completamente transparente)
            handleCanvasGroup.alpha = 0f;
        }
    }

    /// <summary>
    /// Se llama automáticamente cuando el mouse entra en el área del objeto (hover).
    /// </summary>
    /// <param name="eventData">Datos del evento del puntero.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        StartFade(1f); // Inicia el fade hacia opacidad total (mostrar el handle)
    }

    /// <summary>
    /// Se llama automáticamente cuando el mouse sale del área del objeto.
    /// </summary>
    /// <param name="eventData">Datos del evento del puntero.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        StartFade(0f); // Inicia el fade hacia opacidad cero (ocultar el handle)
    }

    /// <summary>
    /// Maneja la lógica de comenzar la animación de fade.
    /// </summary>
    /// <param name="targetAlpha">Valor objetivo de opacidad (alpha).</param>
    private void StartFade(float targetAlpha)
    {
        // Si ya hay una corrutina en curso, la detenemos para no superponer animaciones
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        // Iniciamos una nueva corrutina hacia el nuevo valor de alpha
        fadeCoroutine = StartCoroutine(FadeTo(targetAlpha));
    }

    /// <summary>
    /// Corrutina que hace la transición suave de alpha.
    /// </summary>
    /// <param name="targetAlpha">Valor objetivo de opacidad (alpha).</param>
    /// <returns>Enumerador para la corrutina.</returns>
    private IEnumerator FadeTo(float targetAlpha)
    {
        // Guardamos el valor inicial de alpha
        float startAlpha = handleCanvasGroup.alpha;
        float elapsed = 0f;

        // Mientras no se haya completado la duración del fade...
        while (elapsed < fadeDuration)
        {
            // Avanza el tiempo
            elapsed += Time.deltaTime;

            // Calcula cuánto del fade se ha completado (de 0 a 1)
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            // Interpola suavemente entre alpha inicial y el objetivo
            handleCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            yield return null; // Espera un frame
        }

        // Al final, aseguramos que el alpha quede exactamente en el valor deseado
        handleCanvasGroup.alpha = targetAlpha;
    }
}
