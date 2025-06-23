#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GeneradorDeBotonesDesdeImg : MonoBehaviour
{
    //Creamos nuevo apartado en el menu superior
   [MenuItem("Herramientas/Generar Prefabs desde Imágenes")]
    public static void GenerarPrefabs()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        // Carpeta de Resources
        string carpetaImagenes = "CarouselImages/"+sceneName; // Ajustado para que coja la carpeta de la escena
        Texture2D[] imagenes = Resources.LoadAll<Texture2D>(carpetaImagenes);

        // Encuentra o crea un contenedor padre en la jerarquía
        GameObject contenedor = GameObject.Find("ContenedorBotones");
        if (contenedor == null)
        {
            contenedor = new GameObject("ContenedorBotones");
        }

        // Cargar el prefab (ajusta el path real del prefab)
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/HabitacionContenedora.prefab");

        if (prefab == null)
        {
            Debug.LogError("No se encontró el prefab. Verifica la ruta en AssetDatabase.LoadAssetAtPath.");
            return;
        }
        //Busca los dos objetos content en la jerarquia
        GameObject content = GameObject.Find("Content");         
        GameObject contentAndroid = GameObject.Find("Content Android"); 

        //Recorre todas las imagenes
        foreach (Texture2D img in imagenes)
        {
            //Coge el nombre de la imagen
            string nombreImagen = img.name;

            //Instancia el prefab habitación contenedora, le da el nombre de la imagen y lo mete en el contenedor
            GameObject instancia = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instancia.name = nombreImagen.Substring(2);
            instancia.transform.SetParent(contenedor.transform, false); // lo mantiene sin alterar el scale

            // Buscar el botón dentro del prefab
            Button boton = instancia.GetComponentInChildren<Button>(true);
            if (boton != null)
            {
                boton.name = nombreImagen;

                // Asignar los content al script del botón
                BotonNavegacion script = boton.GetComponent<BotonNavegacion>();
                if (script != null)
                {
                    script.Inicializar(content, contentAndroid);
                }
                else
                {
                    Debug.LogWarning($"No se encontró el script BotonNavegacion en el botón: {nombreImagen}");
                }
            }
            else
            {
                Debug.LogWarning($"No se encontró botón dentro del prefab: {nombreImagen}");
            }
        }

        Debug.Log($"Se generaron {imagenes.Length} objetos en la jerarquía.");
    }
}
#endif
