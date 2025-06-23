using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BottonNavigationAsignerCont : EditorWindow
{
    //El contenedor de los botones
    private GameObject parentContainer;
    //El content que se le va pasar
    private GameObject contentObject;
    //El content que se le va pasar
    private GameObject contentAndr;

    //Se coloca en herramientas
    [MenuItem("Herramientas/Asignar Content a BotonNavigation")]
    public static void ShowWindow()
    {
        //Titulo de la ventana del editor
        GetWindow<BottonNavigationAsignerCont>("Asignar Content");
    }

    private void OnGUI()
    {
        //Etiqueta debajo del titulo de la ventana
        GUILayout.Label("Asignar Content a múltiples CursorChanger", EditorStyles.boldLabel);

        //Los contenedores que se le va a pasar por el editor
        parentContainer = (GameObject)EditorGUILayout.ObjectField("Contenedor Padre", parentContainer, typeof(GameObject), true);
        contentObject = (GameObject)EditorGUILayout.ObjectField("Objeto Content", contentObject, typeof(GameObject), true);
        contentAndr = (GameObject)EditorGUILayout.ObjectField("Objeto ContentAndroid", contentAndr, typeof(GameObject), true);

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
        //Busca todos los objetos hijos del contenedor padre que tengan el componente CursorChanger, incluso si están inactivos
        BotonNavegacion[] buttons = parentContainer.GetComponentsInChildren<BotonNavegacion>(true);

        //Recorre cada botón encontrado
        foreach (var button in buttons)
        {
            //Permite deshacer el cambio desde el editor si haces Ctrl+Z
            Undo.RecordObject(button, "Asignar Content a CursorChanger");
            button.Inicializar(contentObject,contentAndr); // Asigna por método del script CursorChanger
            
            //Busca el campo content aunque sea private o SerializeField y lo asigna por si no se hizo en initialize
            var type = button.GetType();
            type.GetField("content", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(button, contentObject);
            type.GetField("contentAndr", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(button, contentAndr);
            //Marca el objeto como modificado para que Unity guarde el cambio
            EditorUtility.SetDirty(button);
            count++;
        }

        Debug.Log($"Se asignó el objeto Content a {count} botones con BotonNavegacion.");
    }
}
