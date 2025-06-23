using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class MostrarSliderSonido : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public AudioSource audioAmbiente;
    public Slider sliderVolumen;
    public GameObject iconoSonido;
    public Sprite iconoConSonido;
    public Sprite iconoSinSonido;
    private Coroutine ocultarSliderCoroutine;

    private bool musicaPausada = false;  // Variable para llevar el seguimiento del estado de la música

    /// <summary>
    /// Inicializa el volumen global y del AudioSource, configura el slider y actualiza el icono de sonido.
    /// </summary>
    void Start()
    {
        AudioListener.volume = 0.3f;
        if (audioAmbiente != null)
        {
            audioAmbiente.volume = 1.0f;
            audioAmbiente.Play();
            audioAmbiente.loop = true;
        }

        sliderVolumen.value = AudioListener.volume;
        sliderVolumen.onValueChanged.AddListener(CambiarVolumen);
        sliderVolumen.gameObject.SetActive(false);

        ActualizarIcono(AudioListener.volume);
    }

    /// <summary>
    /// Muestra el slider de volumen cuando el puntero entra en el área del icono.
    /// </summary>
    /// <param name="eventData">Datos del evento de puntero.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ocultarSliderCoroutine != null)
            StopCoroutine(ocultarSliderCoroutine);

        sliderVolumen.gameObject.SetActive(true);
    }

    /// <summary>
    /// Inicia la corrutina para ocultar el slider de volumen cuando el puntero sale del área del icono.
    /// </summary>
    /// <param name="eventData">Datos del evento de puntero.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (ocultarSliderCoroutine != null)
            StopCoroutine(ocultarSliderCoroutine);

        ocultarSliderCoroutine = StartCoroutine(EsperarYOcultarSlider());
    }

    /// <summary>
    /// Corrutina que espera un tiempo antes de ocultar el slider de volumen.
    /// </summary>
    /// <returns>IEnumerator para la corrutina.</returns>
    IEnumerator EsperarYOcultarSlider()
    {
        yield return new WaitForSeconds(2f);
        sliderVolumen.gameObject.SetActive(false);
    }

    void CambiarVolumen(float valor)
    {
        // Si el volumen es mayor que cero y la música está en mute, la reanudamos
        if (valor > 0.01f && AudioListener.pause)
        {
            AudioListener.pause = false;  // Reanudamos la música globalmente
            if (audioAmbiente != null && !audioAmbiente.isPlaying)  // Solo reproducir si no está ya en reproducción
            {
                audioAmbiente.Play();  // Reproducimos la música
            }
            musicaPausada = false;
        }

        // Cambiamos el volumen global y el volumen del AudioSource
        AudioListener.volume = valor;
        audioAmbiente.volume = valor;
        ActualizarIcono(valor);
    }

    void ActualizarIcono(float volumen)
    {
        Image img = iconoSonido.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = (volumen <= 0.01f) ? iconoSinSonido : iconoConSonido;
        }
    }

    public void SilenciarAmbiente(bool silenciar)
    {
        AudioListener.pause = silenciar;
        if (silenciar)
        {
            audioAmbiente.Pause();  // Si silenciamos, también pausamos la fuente de audio
        }
        else
        {
            if (!audioAmbiente.isPlaying)  // Si no se está reproduciendo, la reanudamos
            {
                audioAmbiente.Play();
            }
        }
    }

    // Nueva función para manejar la pausa y reanudación de la música
    public void AlternarMusica()
    {
        if (musicaPausada)
        {
            // Reanuda la música si estaba pausada
            audioAmbiente.Play();
            musicaPausada = false;
            ActualizarIcono(AudioListener.volume); // Actualiza el icono para reflejar que la música está activa
        }
        else
        {
            // Pausa la música
            audioAmbiente.Pause();
            musicaPausada = true;
            ActualizarIcono(0); // Actualiza el icono para reflejar que la música está pausada
        }
    }
}
