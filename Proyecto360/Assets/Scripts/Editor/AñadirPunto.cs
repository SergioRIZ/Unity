using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AñadirPunto : EditorWindow
{
    private GameObject parentContainer;

    [MenuItem("Herramientas/Añadir Punto a Hijos Directos")]
    public static void ShowWindow()
    {
        GetWindow<AñadirPunto>("Añadir Punto");
    }

    private void OnGUI()
    {
        GUILayout.Label("Añadir objeto 'Punto' a cada hijo directo", EditorStyles.boldLabel);

        parentContainer = (GameObject)EditorGUILayout.ObjectField("Contenedor Padre", parentContainer, typeof(GameObject), true);

        if (GUILayout.Button("Añadir 'Punto' a hijos"))
        {
            if (parentContainer == null)
            {
                Debug.LogError("Por favor, asigna un contenedor padre.");
                return;
            }

            AddPuntosToChildren();
        }
    }

    private void AddPuntosToChildren()
    {
        int count = 0;

        foreach (Transform child in parentContainer.transform)
        {
            // Evita duplicar si ya existe un hijo llamado "Punto"
            if (child.Find("Punto") != null)
            {
                Debug.Log($"'{child.name}' ya tiene un hijo llamado 'Punto'. Se omite.");
                continue;
            }

            GameObject punto = new GameObject("Punto");
            Undo.RegisterCreatedObjectUndo(punto, "Crear Punto");
            punto.transform.SetParent(child, false); // Lo añade sin modificar posición local
            punto.transform.localPosition = Vector3.zero;

            EditorUtility.SetDirty(child);
            count++;
        }

        Debug.Log($"Se añadieron {count} objetos 'Punto' a los hijos de {parentContainer.name}.");
    }
}
