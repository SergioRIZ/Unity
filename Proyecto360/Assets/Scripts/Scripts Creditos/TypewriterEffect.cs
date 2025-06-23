using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Efecto de máquina de escribir para mostrar texto letra por letra, con sonido y animaciones.
/// </summary>
public class TypewriterEffect : MonoBehaviour
{
    [Header("Texto")]
    /// <summary>
    /// Componente de texto en pantalla donde se mostrará el efecto de máquina de escribir.
    /// </summary>
    public TextMeshProUGUI textComponent;

    /// <summary>
    /// Texto completo que se va a escribir con el efecto.
    /// </summary>
    [TextArea(5, 20)] public string fullText;

    /// <summary>
    /// Retardo en segundos entre la aparición de cada letra.
    /// </summary>
    public float delay = 0.03f;

    /// <summary>
    /// Retardo adicional en segundos al hacer un salto de línea.
    /// </summary>
    public float lineDelay = 0.4f;

    [Header("Sonido")]
    /// <summary>
    /// Fuente de audio para reproducir sonidos de tecla.
    /// </summary>
    public AudioSource audioSource;

    /// <summary>
    /// Clip de sonido que se reproduce por cada tecla.
    /// </summary>
    public AudioClip keySound;

    /// <summary>
    /// Tiempo mínimo en segundos entre sonidos de tecla.
    /// </summary>
    public float soundCooldown = 0.05f;

    [Header("Cursor & UI")]
    /// <summary>
    /// Texto que aparece al final indicando "Presiona ENTER".
    /// </summary>
    public TextMeshProUGUI pressEnterText;

    /// <summary>
    /// Nombre de la escena que se cargará al presionar ENTER.
    /// </summary>
    public string sceneToLoad = "MainMenu";

    // Variables internas
    /// <summary>
    /// Texto que se va formando letra a letra.
    /// </summary>
    private string currentText = "";

    /// <summary>
    /// Indica si ya terminó de escribirse el texto.
    /// </summary>
    private bool typingComplete = false;

    /// <summary>
    /// Último momento en que se reprodujo un sonido de tecla.
    /// </summary>
    private float lastSoundTime = 0f;

    /// <summary>
    /// Indica si se está reproduciendo un sonido de tecla.
    /// </summary>
    private bool isPlayingSound = false;

    /// <summary>
    /// Corrutina para el parpadeo del cursor.
    /// </summary>
    private Coroutine cursorCoroutine;

    /// <summary>
    /// Corrutina para el mensaje de "Presiona ENTER".
    /// </summary>
    private Coroutine enterMessageCoroutine;

    /// <summary>
    /// Inicializa el efecto de máquina de escribir, limpiando el texto y ocultando el mensaje de ENTER.
    /// </summary>
    void Start()
    {
        // Limpiar el texto al inicio
        textComponent.text = "";

        // Ocultar el mensaje de ENTER si está asignado
        if (pressEnterText != null)
            pressEnterText.gameObject.SetActive(false);

        // Comenzar a mostrar el texto
        StartCoroutine(ShowText());
    }

    /// <summary>
    /// Detecta la pulsación de ENTER para finalizar el efecto y cargar la siguiente escena.
    /// </summary>
    void Update()
    {
        // Si ya terminó de escribir el texto y se presiona ENTER
        if (typingComplete && Input.GetKeyDown(KeyCode.Return))
        {
            // Detener corrutinas de cursor y mensaje
            if (cursorCoroutine != null)
                StopCoroutine(cursorCoroutine);
            if (enterMessageCoroutine != null)
                StopCoroutine(enterMessageCoroutine);

            // Asegurarse de mostrar todo el texto
            textComponent.text = currentText;

            // Ocultar el mensaje de ENTER
            if (pressEnterText != null)
                pressEnterText.gameObject.SetActive(false);

            // Detener el audio si estuviera sonando
            if (audioSource != null)
                audioSource.Stop();

            // Cargar la nueva escena
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    /// <summary>
    /// Corrutina principal que escribe el texto letra por letra, gestionando etiquetas y retardos.
    /// </summary>
    /// <returns>IEnumerator para la corrutina.</returns>
    IEnumerator ShowText()
    {
        int i = 0;

        // Iniciar sonido en loop si está disponible
        if (keySound != null && audioSource != null)
        {
            audioSource.clip = keySound;
            audioSource.loop = true;
            audioSource.Play();
        }

        while (i < fullText.Length)
        {
            if (fullText[i] == '<')
            {
                int end = fullText.IndexOf('>', i);
                string tag = fullText.Substring(i, end - i + 1);
                currentText += tag;
                textComponent.text = currentText;
                i = end + 1;
            }
            else
            {
                char currentChar = fullText[i];
                currentText += currentChar;
                textComponent.text = currentText;

                i++;

                if (currentChar == '\n')
                    yield return new WaitForSeconds(lineDelay);
                else
                    yield return new WaitForSeconds(delay);
            }
        }

        // Texto completado
        typingComplete = true;

        // Detener sonido de tecla continuo
        if (audioSource != null)
            audioSource.Stop();

        // Iniciar cursor parpadeante y mensaje
        cursorCoroutine = StartCoroutine(BlinkCursor());
        if (pressEnterText != null)
            enterMessageCoroutine = StartCoroutine(BlinkEnterMessage());
    }

    /// <summary>
    /// Corrutina para reproducir un sonido breve de tecla.
    /// </summary>
    /// <returns>IEnumerator para la corrutina.</returns>
    IEnumerator PlayKeySound()
    {
        isPlayingSound = true;
        audioSource.PlayOneShot(keySound);
        yield return new WaitForSeconds(keySound.length);
        isPlayingSound = false;
    }

    /// <summary>
    /// Corrutina para hacer parpadear un cursor al final del texto.
    /// Se simula el cursor más adelante con 3 espacios.
    /// </summary>
    /// <returns>IEnumerator para la corrutina.</returns>
    IEnumerator BlinkCursor()
    {
        while (true)
        {
            textComponent.text = currentText + "    <color=#FFDD00>|</color>"; // Cursor 4 espacios adelantado
            yield return new WaitForSeconds(0.5f);
            textComponent.text = currentText + "     "; // 4 espacios + 1 invisible para ocultar el cursor
            yield return new WaitForSeconds(0.5f);
        }
    }

    /// <summary>
    /// Corrutina para hacer parpadear el texto "Presiona ENTER".
    /// </summary>
    /// <returns>IEnumerator para la corrutina.</returns>
    IEnumerator BlinkEnterMessage()
    {
        pressEnterText.gameObject.SetActive(true);

        while (true)
        {
            pressEnterText.alpha = 1f;
            yield return new WaitForSeconds(0.5f);
            pressEnterText.alpha = 0f;
            yield return new WaitForSeconds(0.5f);
        }
    }
}
