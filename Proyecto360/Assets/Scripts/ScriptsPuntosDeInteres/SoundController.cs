using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controlador para gestionar el sonido ambiental y de video, así como la interfaz de usuario asociada.
/// Permite silenciar/restaurar el sonido, ajustar el volumen y alternar entre fuentes de audio.
/// </summary>
public class SoundController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private AudioSource ambientSound;       // Audio principal del ambiente
    [SerializeField] private Button soundButton;             // Botón para silenciar/restaurar el sonido
    [SerializeField] private Slider volumeSlider;            // Slider para ajustar el volumen

    [Header("Sprites")]
    [SerializeField] private Sprite soundOnSprite;           // Ícono cuando el sonido está activo
    [SerializeField] private Sprite soundOffSprite;          // Ícono cuando el sonido está en mute

    private float guardarSonido = 0.2f;
    private float guardarVolumenVideo = 0.2f;
    private float previousAmbientVolume = 0.2f;              // Último volumen del ambiente antes de mutear
    private float previousVideoVolume = 0.2f;                // Último volumen del video antes de mutear
    private AudioSource currentControlledAudio;              // Audio actualmente controlado (ambiente o video)
    private bool isVideoAudioActive = false;                 // Indica si el audio actual es de video
    private const string MuteKey = "IsMuted";
    // Bandera para evitar loops de eventos del slider
    private bool isChangingFromCode = false;

    /// <summary>
    /// Inicializa el controlador de sonido, asignando eventos y configurando el estado inicial.
    /// </summary>
    void Start()
    {
        // Al iniciar, se usa el audio ambiente como fuente principal
        currentControlledAudio = ambientSound;

        if (ambientSound != null)
        {
            ambientSound.volume = previousAmbientVolume;
            volumeSlider.value = previousAmbientVolume;
        }

        // Escuchar eventos del botón y del slider
        soundButton.onClick.AddListener(ToggleMute);
        volumeSlider.onValueChanged.AddListener(ChangeVolume);

        // Configura el ícono inicial
        UpdateButtonIcon();
    }

    /// <summary>
    /// Cambia el volumen según el valor del slider.
    /// </summary>
    /// <param name="volume">Nuevo valor de volumen a establecer.</param>
    void ChangeVolume(float volume)
    {
        if (currentControlledAudio == null || isChangingFromCode) return;

        // Si el audio es de video (no ambiente)
        if (currentControlledAudio != ambientSound)
        {
#if UNITY_WEBGL
            // En WebGL, también se debe ajustar el volumen del VideoPlayer
            var videoPlayer = currentControlledAudio.GetComponent<UnityEngine.Video.VideoPlayer>();
            if (videoPlayer != null)
                videoPlayer.SetDirectAudioVolume(0, volume);
#endif
            currentControlledAudio.volume = volume;
            previousVideoVolume = volume;
        }
        else
        {
            // Audio ambiente
            currentControlledAudio.volume = volume;
            previousAmbientVolume = volume;
            Debug.Log("Este es el valor del sonido en el change: " + previousAmbientVolume);
            Debug.Log("¿Está usando ambientSound?: " + (currentControlledAudio == ambientSound));
        }

        // Actualiza el ícono según si hay sonido o no
        UpdateButtonIcon();
    }

    /// <summary>
    /// Alterna entre silenciar y restaurar el volumen del audio actualmente controlado.
    /// </summary>
    void ToggleMute()
    {
        if (currentControlledAudio == null) return;

        bool isCurrentlyMuted = currentControlledAudio.volume <= 0f;

        if (isCurrentlyMuted)
        {
            Debug.Log("Este es el valor del sonido" + previousAmbientVolume);
            // Restaurar volumen anterior
            float volumeToRestore = currentControlledAudio == ambientSound ? previousAmbientVolume : previousVideoVolume;
            volumeToRestore = Mathf.Max(volumeToRestore, 0.2f); // Evita restaurar a 0

#if UNITY_WEBGL
            if (currentControlledAudio != ambientSound)
            {
                var videoPlayer = currentControlledAudio.GetComponent<UnityEngine.Video.VideoPlayer>();
                if (videoPlayer != null)
                    videoPlayer.SetDirectAudioVolume(0, volumeToRestore);
            }
#endif
            currentControlledAudio.volume = volumeToRestore;
            // Bloquear evento del slider antes de cambiar
            isChangingFromCode = true;
            volumeSlider.value = volumeToRestore;
            isChangingFromCode = false;

        }
        else
        {
            // Guardar el volumen actual antes de silenciar
            if (currentControlledAudio == ambientSound)
            {
                previousAmbientVolume = currentControlledAudio.volume;
                Debug.Log("Guardado previousAmbientVolume: " + previousAmbientVolume);
            }
            else
            {
                previousVideoVolume = currentControlledAudio.volume;
                Debug.Log("Guardado previousVideoVolume: " + previousVideoVolume);
            }
            // Mutear (poner en silencio)
#if UNITY_WEBGL
            if (currentControlledAudio != ambientSound)
            {
                var videoPlayer = currentControlledAudio.GetComponent<UnityEngine.Video.VideoPlayer>();
                if (videoPlayer != null)
                    videoPlayer.SetDirectAudioVolume(0, 0f);
            }
#endif
            currentControlledAudio.volume = 0f;
            isChangingFromCode = true;
            volumeSlider.value = 0f;
            isChangingFromCode = false;

        }

        // Actualizar ícono
        UpdateButtonIcon();
    }

    /// <summary>
    /// Cambia el ícono del botón según si hay sonido o está en mute.
    /// </summary>
    void UpdateButtonIcon()
    {
        if (currentControlledAudio == null) return;
        bool hasSound = currentControlledAudio.volume > 0f;
        soundButton.image.sprite = hasSound ? soundOnSprite : soundOffSprite;
    }

    /// <summary>
    /// Cambia la fuente de audio controlada (ambiental o video).
    /// </summary>
    /// <param name="audioSource">La nueva fuente de audio a controlar.</param>
    public void SetControlledAudioSource(AudioSource audioSource)
    {
        if (audioSource == null) return;

        if (audioSource != ambientSound)
        {
            if (currentControlledAudio != audioSource)
            {
                guardarSonido = ambientSound.volume;
                // Cambiando al audio de video
                previousAmbientVolume = ambientSound.volume;
                ambientSound.Stop();
                ambientSound.volume = 0f;
                isVideoAudioActive = true;

#if UNITY_WEBGL
                var videoPlayer = audioSource.GetComponent<UnityEngine.Video.VideoPlayer>();
                if (videoPlayer != null)
                    videoPlayer.SetDirectAudioVolume(0, guardarVolumenVideo);
#endif
                Debug.Log("Audio del volumen" + guardarVolumenVideo);
                audioSource.volume = guardarVolumenVideo;
                volumeSlider.value = guardarVolumenVideo;
            }
            else
            {
                Debug.Log("Ya se está controlando este audio. No se reasigna el volumen.");
            }
        }
        else if (isVideoAudioActive)
        {
            if (currentControlledAudio != null)
            {
                guardarVolumenVideo = currentControlledAudio.volume;
                Debug.Log("Audio del video guardado al cerrar: " + guardarVolumenVideo);
            }
            // Volver al audio ambiente
            ambientSound.Play();
            ambientSound.volume = guardarSonido;
            volumeSlider.value = guardarSonido;
            isVideoAudioActive = false;
        }

        currentControlledAudio = audioSource;
        UpdateButtonIcon();
    }

    /// <summary>
    /// Pausa el sonido ambiental y activa el del video.
    /// </summary>
    /// <param name="videoAudioSource">Fuente de audio del video a activar.</param>
    public void PauseAmbientSound(AudioSource videoAudioSource)
    {
        if (ambientSound == null) return;

        previousAmbientVolume = ambientSound.volume;
        ambientSound.Stop();
        ambientSound.volume = 0f;

        SetControlledAudioSource(videoAudioSource);
    }

    /// <summary>
    /// Restaura el audio ambiente como principal.
    /// </summary>
    public void ResumeAmbientSound()
    {
        SetControlledAudioSource(ambientSound);
    }

    /// <summary>
    /// Carga el volumen guardado y el estado de mute al habilitar el objeto.
    /// </summary>
    void OnEnable()
    {
        if (PlayerPrefs.HasKey("SavedAmbientVolume"))
        {
            previousAmbientVolume = PlayerPrefs.GetFloat("SavedAmbientVolume");

            if (ambientSound != null)
            {
                ambientSound.volume = previousAmbientVolume;
                volumeSlider.value = previousAmbientVolume;
            }
        }

        // Recuperar estado de mute guardado
        if (PlayerPrefs.HasKey(MuteKey))
        {
            int muteValue = PlayerPrefs.GetInt(MuteKey);
            bool wasMuted = muteValue == 1;

            if (wasMuted)
            {
                // Si estaba en mute, establecer volumen en 0
#if UNITY_WEBGL
                var videoPlayer = ambientSound.GetComponent<UnityEngine.Video.VideoPlayer>();
                if (videoPlayer != null)
                    videoPlayer.SetDirectAudioVolume(0, 0f);
#endif
                ambientSound.volume = 0f;
                volumeSlider.value = 0f;
            }
        }

        UpdateButtonIcon();
    }

    /// <summary>
    /// Guarda el volumen actual y el estado de mute cuando se desactiva el objeto.
    /// </summary>
    void OnDisable()
    {
        PlayerPrefs.SetFloat("SavedAmbientVolume", previousAmbientVolume);

        bool isMuted = currentControlledAudio == null || currentControlledAudio.volume <= 0f;
        PlayerPrefs.SetInt(MuteKey, isMuted ? 1 : 0);
    }
}
