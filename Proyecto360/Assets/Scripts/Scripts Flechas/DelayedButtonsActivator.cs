using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Clase que gestiona la activación de botones con un efecto de fade-in y un retraso configurable.
/// </summary>
public class DelayedFadeInButtons : MonoBehaviour
{
    /// <summary>
    /// Tiempo de espera inicial antes de comenzar a mostrar los botones.
    /// </summary>
    [Header("Tiempo de espera antes de comenzar a mostrar los botones")]
    [SerializeField] private float initialDelay = 0.2f;

    /// <summary>
    /// Duración del efecto de fade-in para cada botón.
    /// </summary>
    [Header("Duración del fade-in de cada botón")]
    [SerializeField] private float fadeDuration = 0.5f;

    /// <summary>
    /// Retraso entre la aparición de cada botón.
    /// </summary>
    [Header("Retraso entre la aparición de cada botón")]
    [SerializeField] private float delayBetweenButtons = 0f;

    /// <summary>
    /// Lista de botones a mostrar. Si está vacía, se toman todos los hijos del objeto actual.
    /// </summary>
    [Header("Botones a mostrar (si se deja vacío, toma todos los hijos)")]
    [SerializeField] private List<GameObject> botones = new List<GameObject>();

    /// <summary>
    /// Método llamado cuando el objeto se habilita. Inicia la rutina de activación con fade.
    /// </summary>
    private void OnEnable()
    {
        StartCoroutine(ActivarConFade());
    }

    /// <summary>
    /// Corrutina que gestiona la activación de los botones con un efecto de fade-in y retrasos configurables.
    /// Si la lista de botones está vacía, busca todos los botones hijos y subhijos, incluso los desactivados.
    /// Inicialmente desactiva visualmente todos los botones y luego los activa uno a uno aplicando el efecto de fade-in.
    /// </summary>
    /// <returns>Una instancia de <see cref="IEnumerator"/> para la corrutina.</returns>
    private IEnumerator ActivarConFade()
    {
        // Si no hay botones asignados, busca todos los botones dentro de hijos y subhijos (incluso los desactivados)
        if (botones.Count == 0)
        {
            Button[] botonesEncontrados = GetComponentsInChildren<Button>(true);
            foreach (Button btn in botonesEncontrados)
            {
                botones.Add(btn.gameObject);
            }
        }

        // Preparar todos los botones: desactivar visualmente
        foreach (GameObject boton in botones)
        {
            if (boton == null) continue;

            CanvasGroup cg = boton.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = boton.AddComponent<CanvasGroup>();
            }

            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
            boton.SetActive(false);
        }

        yield return new WaitForSeconds(initialDelay);

        // Activar uno por uno con fade
        foreach (GameObject boton in botones)
        {
            if (boton == null) continue;

            boton.SetActive(true);
            CanvasGroup cg = boton.GetComponent<CanvasGroup>();
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsed / fadeDuration);
                cg.alpha = alpha;
                yield return null;
            }

            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;

            yield return new WaitForSeconds(delayBetweenButtons);
        }
    }
}
