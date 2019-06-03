using UnityEditor;
using UnityEngine;

public class MeshBuilder : EditorWindow
{
    static MeshBuilder window;

    static GameObject newMeshGO;

    private static Material defaultMat;
    private static Material vertexColorMat;
    Color color = Color.white;

    private void Awake()
    {
        defaultMat = (Material)Resources.Load("Materials/Default");
        vertexColorMat = (Material)Resources.Load("Materials/VertexColorShadowed");
    }

    [MenuItem("<MeshBuilder>/Mesh Editor")]
    public static void OpenMeshEditor()
    {
        window = (MeshBuilder)GetWindow(typeof(MeshBuilder));
        window.Show();
        
        if (!defaultMat)
            defaultMat = (Material)Resources.Load("Default");

        if (newMeshGO)
            DestroyImmediate(newMeshGO);

        MeshEditor.ChangeEditMode(MeshEditor.EditMode.VERTEX);
    }

    Rect primitiveMeshesRect;
    Rect meshManipulationRect;

    void OnGUI()
    {
        // Create meshes
        primitiveMeshesRect = new Rect(10, 30, window.position.width - 20, 100);
        GUILayout.BeginArea(primitiveMeshesRect);
            GUILayout.Label("Primitive Meshes");
            if (GUILayout.Button("Create Cube"))
                CreateCube();
        GUILayout.EndArea();


        // Handle meshes
        meshManipulationRect = new Rect(10, 130, window.position.width - 20, 200);
        GUILayout.BeginArea(meshManipulationRect);
        {
            GUILayout.Label("Mesh Manipulation");
            if (GUILayout.Button("Handle Mesh"))
                HandleMesh();
            GUILayout.Label("Manipulation Mode");
            if (GUILayout.Button("Vertex"))
                MeshEditor.ChangeEditMode(MeshEditor.EditMode.VERTEX);
            if (GUILayout.Button("Faces"))
                MeshEditor.ChangeEditMode(MeshEditor.EditMode.FACES);
            if (GUILayout.Button("GameObject"))
                MeshEditor.ChangeEditMode(MeshEditor.EditMode.GAMEOBJECT);
            GUILayout.Label("Vertex Color");
            if (GUILayout.Button("Set as Vertex Color"))
                MeshEditor.SetAsVertexColor();
            color = EditorGUILayout.ColorField("Vertex Color", color);
            if (GUILayout.Button("Apply Vertex Color"))
                MeshEditor.ApplyVertexColor(color);

            
        }
        GUILayout.EndArea();
       
    }

    public void CreateCube()
    {
        Vector3[] vertices = new Vector3[]
        {
            new Vector3 (0, 0, 0),
            new Vector3 (1, 0, 0),
            new Vector3 (1, 1, 0),
            new Vector3 (0, 1, 0),
            new Vector3 (0, 1, 1),
            new Vector3 (1, 1, 1),
            new Vector3 (1, 0, 1),
            new Vector3 (0, 0, 1),
        };

        int[] triangles = new int[]
        {
            0, 2, 1, //face front
	        0, 3, 2,
            2, 3, 4, //face top
	        2, 4, 5,
            1, 2, 5, //face right
	        1, 5, 6,
            0, 7, 4, //face left
	        0, 4, 3,
            5, 4, 7, //face back
	        5, 7, 6,
            0, 6, 7, //face bottom
	        0, 1, 6
        };

        CreateMesh("MeshHandler_Cube", vertices, triangles);
    }

    public void HandleMesh()
    {
        GameObject[] selectedGameobjects = Selection.gameObjects;
        int selectedGameobjectsCount = selectedGameobjects.Length;
        for (int i = 0; i < selectedGameobjectsCount; i++)
        {
            GameObject selectedGameobject = selectedGameobjects[i];
            MeshHandler meshHandler = selectedGameobject.GetComponent<MeshHandler>();

            if (!meshHandler)
            {
                MeshFilter meshFilter = selectedGameobject.GetComponent<MeshFilter>();

                if (meshFilter)
                    meshHandler = selectedGameobject.AddComponent<MeshHandler>();
                else
                    Debug.Log("GameObject has no Mesh Filter.");
            }
            else
                Debug.Log("Already has a Mesh Handler.");
        }
    }

    public void CreateMesh(string name, Vector3[] vertices, int[] triangles)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        newMeshGO = new GameObject(name);
        newMeshGO.AddComponent<MeshFilter>().mesh  = mesh;
        newMeshGO.AddComponent<MeshRenderer>().material = vertexColorMat;
        newMeshGO.AddComponent<MeshHandler>();

        Selection.activeGameObject = newMeshGO;
    }

    
}
