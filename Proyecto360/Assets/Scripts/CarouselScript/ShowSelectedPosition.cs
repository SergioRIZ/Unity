using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Clase que gestiona la posición seleccionada en un Scrollbar y centra el Scrollbar en el botón correspondiente.
/// </summary>
public class ShowSelectedPosition : MonoBehaviour
{
    /// <summary>
    /// Componente Scrollbar que se utilizará para desplazar la vista.
    /// </summary>
    [SerializeField] private Scrollbar scrollbar;

    /// <summary>
    /// Diccionario que relaciona el texto del botón con su posición en el índice.
    /// La clave es el texto del botón y el valor es el índice del botón dentro de los hijos del objeto.
    /// </summary>
    private Dictionary<string, int> buttonNameToIndex = new Dictionary<string, int>();

    /// <summary>
    /// Corrutina actualmente activa para centrar el Scrollbar, si existe.
    /// Se utiliza para evitar que varias corrutinas se ejecuten simultáneamente.
    /// </summary>
    private Coroutine currentScrollCoroutine;

    /// <summary>
    /// Inicializa el diccionario de nombres de botones a índices al inicio.
    /// Recorre todos los hijos del objeto y almacena el texto de cada botón junto con su índice.
    /// </summary>
    void Start()
    {
        // Recorremos todos los botones para obtener el texto y almacenarlos en el diccionario.
        int index = 0;
        foreach (Transform child in transform)
        {
            TextMeshProUGUI tmp = child.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                string buttonText = tmp.text;
                Debug.Log("Nombre del texto" + tmp.text);
                // Guardamos el nombre del botón como clave y su índice como valor.
                buttonNameToIndex[buttonText] = index;
            }
            else
            {
                Debug.LogWarning($"No se encontró TextMeshProUGUI en hijo: {child.name}");
            }
            index++;
        }
    }

    /// <summary>
    /// Centra el Scrollbar en el botón especificado por su nombre.
    /// Si ya hay una corrutina de desplazamiento activa, la detiene antes de iniciar una nueva.
    /// </summary>
    /// <param name="buttonName">Nombre del botón en el que se desea centrar el Scrollbar.</param>
    public void CenterOnButton(string buttonName)
    {
        // Buscamos el índice del botón en el diccionario usando su nombre.
        if (buttonNameToIndex.TryGetValue(buttonName, out int index))
        {
            // Calculamos el valor del índice en el Scrollbar.
            float targetScrollValue = 1f - (float)index / (transform.childCount - 1f);
            // Si ya hay una corrutina corriendo, detenerla
            if (currentScrollCoroutine != null)
            {
                StopCoroutine(currentScrollCoroutine);
            }

            currentScrollCoroutine = StartCoroutine(SmoothScrollTo(targetScrollValue));
        }
    }

    /// <summary>
    /// Corrutina que mueve el Scrollbar suavemente hacia el valor objetivo.
    /// Realiza una interpolación lineal entre el valor actual y el objetivo durante un tiempo determinado.
    /// </summary>
    /// <param name="target">Valor objetivo del Scrollbar.</param>
    /// <returns>IEnumerator para la corrutina.</returns>
    private IEnumerator SmoothScrollTo(float target)
    {
        float duration = 2.5f; // Duración de la animación en segundos.
        float elapsed = 0f; // Tiempo transcurrido.
        float start = scrollbar.value; // Valor inicial del Scrollbar.

        while (elapsed < duration)
        {
            float previousValue = scrollbar.value;

            // Interpolamos
            float t = elapsed / duration;
            scrollbar.value = Mathf.Lerp(start, target, t);

            // Si ya está muy cerca del objetivo, detenemos la animación
            if (Mathf.Abs(scrollbar.value - target) < 0.01f)
            {
                scrollbar.value = target;
                break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        scrollbar.value = target;
        currentScrollCoroutine = null;
    }
}