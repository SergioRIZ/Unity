using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class CursorChangerAsignerContent : EditorWindow
{
    //El contenedor de los botones
    private GameObject parentContainer;
    //El content que se le va pasar
    private GameObject contentObject;

    //Se coloca en herramientas
    [MenuItem("Herramientas/Asignar Content a CursorChanger")]
    public static void ShowWindow()
    {
        //Titulo de la ventana del editor
        GetWindow<CursorChangerAsignerContent>("Asignar Content");
    }

    private void OnGUI()
    {
        //Etiqueta debajo del titulo de la ventana
        GUILayout.Label("Asignar Content a múltiples CursorChanger", EditorStyles.boldLabel);

        //Los contenedores que se le va a pasar por el editor
        parentContainer = (GameObject)EditorGUILayout.ObjectField("Contenedor Padre", parentContainer, typeof(GameObject), true);
        contentObject = (GameObject)EditorGUILayout.ObjectField("Objeto Content", contentObject, typeof(GameObject), true);

        if (GUILayout.Button("Asignar a todos los botones"))
        {
            if (parentContainer == null || contentObject == null)
            {
                Debug.LogError("Por favor, asigna tanto el contenedor padre como el objeto content.");
                return;
            }

            AssignContentToButtons();
        }
    }

    private void AssignContentToButtons()
    {
        //Contador para saber cuantos botones se procesan
        int count = 0;
        // Busca todos los botones con componente Button dentro del contenedor, incluso si están inactivos
        Button[] buttons = parentContainer.GetComponentsInChildren<Button>(true);

        //Recorre cada botón encontrado
        foreach (var boton in buttons)
        {
            CursorChanger cursorChanger = boton.GetComponent<CursorChanger>();

            // Si no tiene el componente, lo añadimos
            if (cursorChanger == null)
            {
            Undo.RecordObject(boton.gameObject, "Agregar CursorChanger");
            cursorChanger = boton.gameObject.AddComponent<CursorChanger>();
            Debug.Log($"Se añadió CursorChanger al botón: {boton.name}");
            }
            //Permite deshacer el cambio desde el editor si haces Ctrl+Z
            Undo.RecordObject(cursorChanger, "Asignar Content a CursorChanger");
            cursorChanger.Initialize(contentObject); // Asigna por método del script CursorChanger
            //Busca el campo content aunque sea private o SerializeField y lo asigna por si no se hizo en initialize
            cursorChanger.GetType().GetField("content", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(cursorChanger, contentObject); // Forzar asignación privada
            //Marca el objeto como modificado para que Unity guarde el cambio
            EditorUtility.SetDirty(cursorChanger);
            count++;
        }

        Debug.Log($"Se asignó el objeto Content a {count} botones con CursorChanger.");
    }
}

