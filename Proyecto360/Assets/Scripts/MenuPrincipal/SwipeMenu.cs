﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Clase que gestiona un menú deslizante con botones, permitiendo la navegación mediante swipe o botones,
/// adaptando la escala y visibilidad de los elementos según la plataforma y la orientación de la pantalla.
/// </summary>
public class SwipeMenu : MonoBehaviour
{
    [Header("Referencias")]
    /// <summary>
    /// Referencia al objeto Scrollbar que controla el desplazamiento del menú.
    /// </summary>
    public GameObject scrollbar;

    /// <summary>
    /// Referencia al script principal que gestiona el menú principal.
    /// </summary>
    [SerializeField] private MainScript mainScript;

    [Header("Configuración de escalas")]
    /// <summary>
    /// Escala de los botones activos en PC/WebGL.
    /// </summary>
    [SerializeField] private Vector2 pcScale = new Vector2(1.0f, 1.0f);           // Reducido de 1.2f
    /// <summary>
    /// Escala de los botones inactivos en PC/WebGL.
    /// </summary>
    [SerializeField] private Vector2 pcScaleInactive = new Vector2(0.8f, 0.8f);   // Reducido de 0.9f
    /// <summary>
    /// Escala de los botones activos en Android (vertical).
    /// </summary>
    [SerializeField] private Vector2 androidScale = new Vector2(2.5f, 2.5f);      // Reducido de 3.2f
    /// <summary>
    /// Escala de los botones inactivos en Android (vertical).
    /// </summary>
    [SerializeField] private Vector2 androidScaleInactive = new Vector2(2.2f, 2.2f); // Reducido de 2.9f
    /// <summary>
    /// Escala de los botones activos en Android (horizontal).
    /// </summary>
    [SerializeField] private Vector2 androidLandscapeScale = new Vector2(1.0f, 1.0f); // Reducido de 1.3f
    /// <summary>
    /// Escala de los botones inactivos en Android (horizontal).
    /// </summary>
    [SerializeField] private Vector2 androidLandscapeScaleInactive = new Vector2(0.8f, 0.8f); // Reducido de 1f

    /// <summary>
    /// Velocidad de transición para las animaciones de escala y scroll.
    /// </summary>
    [SerializeField] private float transitionSpeed = 0.1f;

    /// <summary>
    /// Posición actual del scroll (valor entre 0 y 1).
    /// </summary>
    private float scroll_pos = 0;

    /// <summary>
    /// Array de posiciones objetivo para cada botón en el menú.
    /// </summary>
    private float[] pos;

    /// <summary>
    /// Índice del botón actualmente centrado.
    /// </summary>
    private int currentIndex = 0;

    /// <summary>
    /// Referencia a la corrutina activa de desplazamiento suave.
    /// </summary>
    private Coroutine scrollCoroutine;

    /// <summary>
    /// Indica si la orientación actual es horizontal (landscape).
    /// </summary>
    private bool isLandscapeMode = false;

    /// <summary>
    /// Actualiza el estado del menú cada frame: orientación, posiciones, entrada y efectos visuales.
    /// </summary>
    void Update()
    {
        // Verificar la orientación actual
        isLandscapeMode = IsLandscapeOrientation();

        // Calcular posiciones de botones
        UpdatePositions();

        // Manejar entrada del usuario
        HandleInput();

        // Aplicar efectos visuales
        ApplyVisualEffects();
    }

    /// <summary>
    /// Actualiza las posiciones de los botones en el menú, calculando su valor de scroll objetivo.
    /// </summary>
    void UpdatePositions()
    {
        pos = new float[transform.childCount];
        float distance = 1f / (pos.Length - 1f);

        for (int i = 0; i < pos.Length; i++)
        {
            pos[i] = distance * i;
        }
    }

    /// <summary>
    /// Maneja la entrada del usuario para el scroll: arrastre con ratón/táctil o centrado automático.
    /// </summary>
    void HandleInput()
    {
        if (Input.GetMouseButton(0))
        {
            scroll_pos = scrollbar.GetComponent<Scrollbar>().value;
        }
        else
        {
            float distance = 1f / (pos.Length - 1f);

            for (int i = 0; i < pos.Length; i++)
            {
                if (scroll_pos < pos[i] + (distance / 2) && scroll_pos > pos[i] - (distance / 2))
                {
                    scrollbar.GetComponent<Scrollbar>().value =
                        Mathf.Lerp(scrollbar.GetComponent<Scrollbar>().value, pos[i], transitionSpeed);
                    currentIndex = i;
                }
            }
        }
    }

    /// <summary>
    /// Aplica efectos visuales a los botones: escalado y visibilidad del texto según el estado y la plataforma.
    /// </summary>
    void ApplyVisualEffects()
    {
        float distance = 1f / (pos.Length - 1f);

        for (int i = 0; i < pos.Length; i++)
        {
            if (i >= transform.childCount) continue;

            RectTransform child = transform.GetChild(i) as RectTransform;
            if (child == null) continue;

            bool isCentered = scroll_pos < pos[i] + (distance / 2) && scroll_pos > pos[i] - (distance / 2);
            Vector2 targetScale = GetTargetScale(isCentered);

            child.localScale = Vector2.Lerp(child.localScale, targetScale, transitionSpeed);

            // Manejar texto
            TextMeshProUGUI text = child.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                // En landscape para Android o cuando está centrado, mostrar texto
                text.enabled = (Application.platform == RuntimePlatform.Android && isLandscapeMode) || isCentered;
                text.fontSize = isLandscapeMode ? 18 : 20; // Tamaños reducidos (originalmente 20 y 24)
            }
        }
    }

    /// <summary>
    /// Obtiene la escala objetivo para un botón según la plataforma, orientación y si está centrado.
    /// </summary>
    /// <param name="isCentered">Indica si el botón está centrado.</param>
    /// <returns>Escala objetivo como Vector2.</returns>
    Vector2 GetTargetScale(bool isCentered)
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            return isCentered ? pcScale : pcScaleInactive;
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            if (isLandscapeMode)
            {
                return isCentered ? androidLandscapeScale : androidLandscapeScaleInactive;
            }
            else
            {
                return isCentered ? androidScale : androidScaleInactive;
            }
        }
        else
        {
            return isCentered ? pcScale : pcScaleInactive;
        }
    }

    /// <summary>
    /// Mueve el scroll una posición hacia la izquierda, centrando el botón anterior (o el último si está en el primero).
    /// </summary>
    public void MoveLeft()
    {
        if (currentIndex == 0)
            currentIndex = pos.Length - 1;
        else
            currentIndex--;

        CenterButton(currentIndex);
    }

    /// <summary>
    /// Mueve el scroll una posición hacia la derecha, centrando el botón siguiente (o el primero si está en el último).
    /// </summary>
    public void MoveRight()
    {
        if (currentIndex == pos.Length - 1)
            currentIndex = 0;
        else
            currentIndex++;

        CenterButton(currentIndex);
    }

    /// <summary>
    /// Comprueba si un botón está centrado actualmente.
    /// </summary>
    /// <param name="index">Índice del botón a comprobar.</param>
    /// <returns>True si el botón está centrado, false en caso contrario.</returns>
    public bool IsButtonCentered(int index)
    {
        return index == currentIndex;
    }

    /// <summary>
    /// Centra el botón indicado por su índice, desplazando el scroll suavemente.
    /// </summary>
    /// <param name="index">Índice del botón a centrar.</param>
    public void CenterButton(int index)
    {
        if (pos == null || index < 0 || index >= pos.Length)
            return;

        if (scrollCoroutine != null)
            StopCoroutine(scrollCoroutine);

        float speed = (Application.platform == RuntimePlatform.Android && isLandscapeMode) ? 0.35f : 0.25f;
        scrollCoroutine = StartCoroutine(SmoothScrollTo(pos[index], speed));
    }

    /// <summary>
    /// Corrutina que realiza un desplazamiento suave del scroll hasta la posición objetivo.
    /// </summary>
    /// <param name="target">Valor objetivo del scroll (0-1).</param>
    /// <param name="speed">Velocidad de interpolación.</param>
    /// <returns>IEnumerator para la corrutina.</returns>
    private IEnumerator SmoothScrollTo(float target, float speed = 0.25f)
    {
        Scrollbar sb = scrollbar.GetComponent<Scrollbar>();

        while (Mathf.Abs(sb.value - target) > 0.001f)
        {
            sb.value = Mathf.Lerp(sb.value, target, speed);
            scroll_pos = sb.value;
            yield return null;
        }

        sb.value = target;
        scroll_pos = target;
        scrollCoroutine = null;
    }

    /// <summary>
    /// Actualiza el layout del menú y sus elementos según la orientación de la pantalla.
    /// </summary>
    /// <param name="isLandscape">Indica si la orientación es horizontal.</param>
    public void UpdateLayoutForOrientation(bool isLandscape)
    {
        this.isLandscapeMode = isLandscape;

        // Ajustar scrollbar (tamaños originales)
        if (scrollbar != null)
        {
            RectTransform scrollRT = scrollbar.GetComponent<RectTransform>();
            if (scrollRT != null)
            {
                scrollRT.sizeDelta = new Vector2(scrollRT.sizeDelta.x, isLandscape ? 20 : 10);
                scrollbar.GetComponent<Scrollbar>().targetGraphic.gameObject.SetActive(isLandscape);
            }
        }

        // Actualizar elementos hijos con tamaños reducidos
        UpdateChildElements(isLandscape);

        // Recentrar el elemento actual
        if (currentIndex >= 0 && currentIndex < transform.childCount)
            CenterButton(currentIndex);
    }

    /// <summary>
    /// Actualiza los elementos hijos del menú (botones) para adaptarlos a la orientación actual.
    /// </summary>
    /// <param name="isLandscape">Indica si la orientación es horizontal.</param>
    private void UpdateChildElements(bool isLandscape)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            RectTransform child = transform.GetChild(i) as RectTransform;
            if (child == null) continue;

            // Ajustar tamaño según orientación (tamaños reducidos)
            child.sizeDelta = isLandscape ?
                new Vector2(child.sizeDelta.x * 0.9f, child.sizeDelta.y * 0.9f) :
                new Vector2(350, 170); // Reducido de 450, 220

            // Texto
            TextMeshProUGUI text = child.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                bool showText = (isLandscape && Application.platform == RuntimePlatform.Android) || IsButtonCentered(i);
                text.enabled = showText;
                text.fontSize = isLandscape ? 20 : 30; // Reducido de 24 y 36
            }

            // Imágenes
            ScaleChildImages(child, isLandscape);
        }
    }

    /// <summary>
    /// Escala las imágenes hijas de un elemento del menú según la orientación.
    /// </summary>
    /// <param name="parent">RectTransform del elemento padre.</param>
    /// <param name="isLandscape">Indica si la orientación es horizontal.</param>
    private void ScaleChildImages(RectTransform parent, bool isLandscape)
    {
        Image[] images = parent.GetComponentsInChildren<Image>();
        foreach (Image img in images)
        {
            RectTransform imgRT = img.GetComponent<RectTransform>();
            if (imgRT != null)
            {
                // Reducir el tamaño de las imágenes
                imgRT.sizeDelta = new Vector2(imgRT.sizeDelta.x * 0.85f, imgRT.sizeDelta.y * 0.85f);
            }
        }
    }

    /// <summary>
    /// Comprueba si la orientación actual de la pantalla es horizontal (landscape).
    /// </summary>
    /// <returns>True si la orientación es horizontal, false si es vertical.</returns>
    private bool IsLandscapeOrientation()
    {
        return (Screen.orientation == ScreenOrientation.LandscapeLeft ||
                Screen.orientation == ScreenOrientation.LandscapeRight);
    }
}