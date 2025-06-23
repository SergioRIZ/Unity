using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Clase que gestiona los efectos de hover sobre un botón, incluyendo cambios de color y activación de objetos tras un retraso.
/// </summary>
public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// El objeto de texto dentro del botón.
    /// </summary>
    public GameObject textObject;

    /// <summary>
    /// El panel que actúa como fondo del texto.
    /// </summary>
    public GameObject panelObject;

    /// <summary>
    /// Tiempo en segundos para activar los objetos después de que el ratón entre en el botón.
    /// </summary>
    public float delayTime = 1f;

    /// <summary>
    /// Coroutine para manejar el retraso en la activación de los objetos.
    /// </summary>
    private Coroutine hoverCoroutine;

    /// <summary>
    /// El componente Image del botón.
    /// </summary>
    private Image buttonImage;

    /// <summary>
    /// Color original del botón.
    /// </summary>
    public Color normalColor = Color.white;

    /// <summary>
    /// Color del botón cuando el ratón está encima.
    /// </summary>
    public Color hoverColor = Color.blue;

    /// <summary>
    /// Inicializa el componente, configurando el color inicial del botón y desactivando los objetos asociados.
    /// </summary>
    private void Start()
    {
        buttonImage = GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = normalColor;
        }

        if (textObject != null)
            textObject.SetActive(false);

        if (panelObject != null)
            panelObject.SetActive(false);
    }

    /// <summary>
    /// Método llamado cuando el ratón entra en el área del botón.
    /// Cambia el color del botón y comienza una Coroutine para activar los objetos tras un retraso.
    /// </summary>
    /// <param name="eventData">Datos del evento del puntero.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buttonImage != null)
        {
            buttonImage.color = hoverColor;
        }

        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
        }

        hoverCoroutine = StartCoroutine(ActivateObjectsAfterDelay());
    }

    /// <summary>
    /// Método llamado cuando el ratón sale del área del botón.
    /// Restaura el color original del botón y desactiva los objetos inmediatamente.
    /// </summary>
    /// <param name="eventData">Datos del evento del puntero.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (buttonImage != null)
        {
            buttonImage.color = normalColor;
        }

        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
        }

        if (textObject != null)
            textObject.SetActive(false);

        if (panelObject != null)
            panelObject.SetActive(false);
    }

    /// <summary>
    /// Coroutine que activa los objetos después de un retraso especificado.
    /// </summary>
    /// <returns>IEnumerator para la Coroutine.</returns>
    private IEnumerator ActivateObjectsAfterDelay()
    {
        yield return new WaitForSeconds(delayTime);

        if (textObject != null)
            textObject.SetActive(true);

        if (panelObject != null)
            panelObject.SetActive(true);
    }
}
