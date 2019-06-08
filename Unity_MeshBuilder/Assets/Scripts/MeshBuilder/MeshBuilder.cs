using UnityEditor;
using UnityEngine;

/// <summary>
/// Mesh Builder window
/// </summary>
public class MeshBuilder : EditorWindow
{
    #region Variables

    /// <summary>
    /// Window reference
    /// </summary>
    private static MeshBuilder window;

    /// <summary>
    /// Default material used
    /// </summary>
    private static Material defaultMat;

    /// <summary>
    /// Vertex Color material used
    /// </summary>
    private static Material vertexColorMat;

    /// <summary>
    /// Stores edit mode strings (performance cache)
    /// </summary>
    private static string[] editModeStrings;

    /// <summary>
    /// Stores primitive type strings (performance cache)
    /// </summary>
    private static string[] primitiveTypeStrings;

    /// <summary>
    /// Stores every type of primitive available
    /// </summary>
    private static Primitive[] primitives;

    /// <summary>
    /// Initial vertex color
    /// </summary>
    private Color initialVertexColor = Color.white;

    /// <summary>
    /// Current selected edit mode
    /// </summary>
    private int editModeInt = (int)MeshEditor.EditMode.Gameobject;

    /// <summary>
    /// Current selected type
    /// </summary>
    private int primitiveTypeInt = (int)PrimitiveType.Cube;

    /// <summary>
    /// Primitive available types
    /// </summary>
    public enum PrimitiveType
    {
        Cube,
        Cone,
        Door
    }

    /// <summary>
    /// Simple Primitive container with only vertices and triangles
    /// </summary>
    public struct Primitive
    {
        public Vector3[] vertices;
        public int[] triangles;

        public Primitive(Vector3[] newVertices, int[] newTriangles)
        {
            vertices = newVertices;
            triangles = newTriangles;
        }
    }

    #endregion

    #region UnityMethods

    /// <summary>
    /// Sets current window
    /// </summary>
    private void OnEnable()
    {
        window = this;
        SceneView.RepaintAll();
    }

    /// <summary>
    /// Paints entire Editor UI
    /// </summary>
    void OnGUI()
    {
        // Initialize style
        GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
        centeredStyle.fontStyle = FontStyle.Bold;
        centeredStyle.alignment = TextAnchor.UpperCenter;
        centeredStyle.fontSize = 16;
        centeredStyle.normal.textColor = Color.yellow;

        #region PrimitiveMeshesSection
        //_ Not caching rects due to errors when changing something and going back to editor
        GUILayout.BeginArea(new Rect(10, 30, window.position.width - 20, 80));
        {
            GUILayout.Label("Primitive Meshes", centeredStyle);
            centeredStyle.normal.textColor = Color.white;
            // Grid used to select primitives
            primitiveTypeInt = GUILayout.SelectionGrid(primitiveTypeInt, primitiveTypeStrings, 3);
            GUILayout.Space(15);
            // Creates selected primitive
            if (GUILayout.Button("Create Primitive Mesh"))
                CreateMesh((PrimitiveType)primitiveTypeInt);
        }
        GUILayout.EndArea();

        #endregion 

        // Separator
        GUI.DrawTexture(new Rect(10, 119, window.position.width - 20, 2), Texture2D.whiteTexture, ScaleMode.StretchToFill);

        #region MeshEditingSection
        GUILayout.BeginArea(new Rect(10, 130, window.position.width - 20, 260));
        {
            centeredStyle.normal.textColor = Color.green;
            GUILayout.Label("Mesh Editing", centeredStyle);
            centeredStyle.normal.textColor = Color.white;

            // Add Mesh Handler Component to the selected Gameobject in order to edit it
            if (GUILayout.Button("Add MeshHandler"))
                HandleMesh();

            centeredStyle.fontSize = 12;
            // Cache selected gameobject
            GameObject selectedGameobject = Selection.activeGameObject;
            // Handle mesh only if they have the correct component
            if (selectedGameobject && selectedGameobject.GetComponent<MeshHandler>())
            {
                // Display mesh manipulation modes section
                GUILayout.Space(5);
                GUILayout.Label("Manipulation Mode", centeredStyle);
                editModeInt = GUILayout.Toolbar(editModeInt, editModeStrings);
                // Change mode
                MeshEditor.ChangeEditMode((MeshEditor.EditMode)editModeInt);
                GUILayout.Space(5);
                // Display Color editing section
                GUILayout.Label("Color", centeredStyle);
                // Set Material to VertexColor
                if (GUILayout.Button("Set Vertex Color Material"))
                    MeshEditor.SetAsVertexColor();
                // Select color to apply
                initialVertexColor = EditorGUILayout.ColorField("Vertex Color", initialVertexColor);
                // Apply color to Mesh depending on which mode is the user on
                if (GUILayout.Button("Apply Vertex Color"))
                    MeshEditor.ApplyVertexColor(initialVertexColor);
                // Separator
                GUILayout.Space(5);
                GUI.DrawTexture(new Rect(20, 190, window.position.width - 60, 1), Texture2D.whiteTexture, ScaleMode.StretchToFill);
                GUILayout.Space(20);
                // Display custom editing section depending on mode selected
                GUILayout.Label("Custom Editing", centeredStyle);
                switch (MeshEditor.editMode)
                {
                    case MeshEditor.EditMode.Vertex:
                        break;
                    case MeshEditor.EditMode.Faces:
                        // Extrudes selected face
                        if (GUILayout.Button("Extrude Face"))
                            MeshEditor.ExtrudeFace();
                        break;
                    case MeshEditor.EditMode.Gameobject:
                        // Export selected gameobject to Mesh 
                        if (GUILayout.Button("Export to Mesh"))
                            ExportMesh();
                        break;
                    default:
                        break;
                }
            }
            else
                // No Mesh Handler selected
                GUILayout.Box("Select Mesh with MeshHandler Component to edit.", GUILayout.ExpandWidth(true));
        }
        GUILayout.EndArea();
        #endregion
    }

    /// <summary>
    /// Repaint on Update
    /// </summary>
    public void OnInspectorUpdate()
    {
        Repaint();
    }

    #endregion

    #region OtherMethods

    /// <summary>
    /// Creates window if it doesn't exist
    /// </summary>
    [MenuItem("<MeshBuilder>/Mesh Editor")]
    public static void OpenMeshEditor()
    {
        if (!window)
        {
            Initialize();
            window.minSize = new Vector2(250, 400);
            window.maxSize = new Vector2(300, 500);
            window.Show();

            // Cache data
            editModeStrings =       System.Enum.GetNames(typeof(MeshEditor.EditMode));
            primitiveTypeStrings =  System.Enum.GetNames(typeof(PrimitiveType));
            // Create primitive data
            InitializePrimitives();
            // Initial mode
            MeshEditor.ChangeEditMode(MeshEditor.EditMode.Gameobject);
        }
        else
            window.Close();
    }

    /// <summary>
    /// Initialize window
    /// </summary>
    public static void Initialize()
    {
        GetWindow(typeof(MeshBuilder));
    }

    /// <summary>
    /// Create initial data for primitives
    /// </summary>
    private static void InitializePrimitives()
    {
        // Create array
        primitives = new Primitive[primitiveTypeStrings.Length];

        #region Primitives Data
        
        // Cube data
        primitives[(int)PrimitiveType.Cube] = new Primitive( 
            new Vector3[]
            {
                new Vector3 (0, 0, 0),
                new Vector3 (1, 0, 0),
                new Vector3 (1, 1, 0),
                new Vector3 (0, 1, 0),
                new Vector3 (0, 1, 1),
                new Vector3 (1, 1, 1),
                new Vector3 (1, 0, 1),
                new Vector3 (0, 0, 1),
            },
            new int[]
            {
                0, 2, 1,
	            0, 3, 2,
                2, 3, 4, 
	            2, 4, 5,
                1, 2, 5, 
	            1, 5, 6,
                0, 7, 4, 
	            0, 4, 3,
                5, 4, 7, 
	            5, 7, 6,
                0, 6, 7, 
	            0, 1, 6
            }
        );

        // Cone data
        primitives[(int)PrimitiveType.Cone] = new Primitive(
            new Vector3[]
            {
                new Vector3 (0, 0, 0),
                new Vector3 (2, 0, 0),
                new Vector3 (2, 0, 2),
                new Vector3 (0, 0, 2),
                new Vector3 (1, 3, 1),
            },
            new int[]
            {
                0, 4, 1,
                1, 4, 2,
                2, 4, 3,
                3, 4, 0,
                0, 1, 3,
                1, 2, 3,
            }
        );

        // Door data
        primitives[(int)PrimitiveType.Door] = new Primitive(
           new Vector3[]
           {
                new Vector3 (0,     0, 0),
                new Vector3 (0.5f,  0, 0),
                new Vector3 (0,     3, 0),
                new Vector3 (0.5f,  3, 0),
                new Vector3 (0,     4, 0),
                new Vector3 (0.5f,  4, 0),
                new Vector3 (3,     0, 0),
                new Vector3 (3.5f,  0, 0),
                new Vector3 (3,     3, 0),
                new Vector3 (3.5f,  3, 0),
                new Vector3 (3,     4, 0),
                new Vector3 (3.5f,  4, 0),
           },
           new int[]
           {
                0, 2, 1,
                1, 2, 3,
                2, 4, 3,
                3, 4, 5,
                3, 5, 8,
                8, 5, 10,
                6, 8, 7,
                7, 8, 9,
                8, 10, 9,
                9, 10, 11,
           }
       );

        #endregion
    }

    /// <summary>
    /// Creates given primitive type into a Mesh Gameobject
    /// </summary>
    public void CreateMesh(PrimitiveType primitiveType)
    {
        // Create new Mesh
        Mesh mesh = new Mesh();
        // Load primitive data
        Primitive meshSimpleData = primitives[(int)primitiveType];
        // Apply it to new mesh
        mesh.vertices = meshSimpleData.vertices;
        mesh.triangles = meshSimpleData.triangles;

        // Create new Gameobject with added name
        GameObject newMeshGO = new GameObject("MeshHandler_" + primitiveType.ToString());
        // Apply new Components
        newMeshGO.AddComponent<MeshFilter>().mesh = mesh;
        newMeshGO.AddComponent<MeshRenderer>();
        newMeshGO.AddComponent<MeshHandler>();

        // Position new Mesh in front of Scene Camera (not caching camera due to reference issues)
        newMeshGO.transform.position = SceneView.lastActiveSceneView.camera.transform.TransformPoint(Vector3.forward * 5);
        // Select new Mesh
        Selection.activeGameObject = newMeshGO;

        // Load Vertex color if not loaded correctly
        if (!vertexColorMat)
            vertexColorMat = (Material)Resources.Load("Materials/VertexColorShadowed");

        // Apply it to new Mesh
        MeshEditor.SetAsVertexColor();
    }

    /// <summary>
    /// Add a MeshHandler component to the selected Mesh Gameobject allowing the user to edit it
    /// </summary>
    public void HandleMesh()
    {
        // Gets every selected gameobject
        GameObject[] selectedGameobjects = Selection.gameObjects;

        // Validity check
        if (selectedGameobjects.Length == 0)
        {
            Debug.Log("<color=yellow> No Gameobjects selected.</color>");
            return;
        }

        // Loop through every selected gameobject
        foreach (GameObject selectedGameobject in selectedGameobjects)
        {
            // Get Mesh Handler
            MeshHandler meshHandler = selectedGameobject.GetComponent<MeshHandler>();

            // Check if it has not a MeshHandler already
            if (!meshHandler)
            {
                // Get Mesh Filter
                MeshFilter meshFilter = selectedGameobject.GetComponent<MeshFilter>();

                // Check if it has a Mesh Filter 
                if (meshFilter)
                    // Add Mesh Handler
                    meshHandler = selectedGameobject.AddComponent<MeshHandler>();
                else
                    Debug.Log("<color=yellow> GameObject has no Mesh Filter.</color>");
            }
            else
                Debug.Log("<color=yellow> Already has a Mesh Handler.</color>");
        }
    }

    /// <summary>
    /// Exports Selected Mesh into the Assets folder
    /// </summary>
    public void ExportMesh()
    {
        // Get Selected gameobject
        GameObject selectedGameobject = Selection.activeGameObject;
        // If it exists
        if (selectedGameobject)
        {
            // Get Mesh Handler
            MeshHandler meshHandler = selectedGameobject.GetComponent<MeshHandler>();
            // If it exists
            if (meshHandler)
            {
                // Cache Mesh
                Mesh mesh = meshHandler.GetMesh();
                // Creates mesh to export
                Mesh finalMesh = new Mesh();
                // Copy everything
                finalMesh.vertices = mesh.vertices;
                finalMesh.triangles = mesh.triangles;
                finalMesh.normals = mesh.normals;
                finalMesh.colors = mesh.colors;
                finalMesh.uv = mesh.uv;

                // Creates Path (random number so it doesn't overlap with another mesh)
                string path = "Assets/CustomMeshes/CustomMesh" + Random.Range(0, 1000000) + ".mesh";
                // Create mesh asset into path
                AssetDatabase.CreateAsset(finalMesh, path);
                // Select it
                EditorGUIUtility.PingObject(finalMesh);
                // Message user
                Debug.Log("<color=green> Mesh has been created at: </color>" + path);
            }
            else
                Debug.Log("<color=red> Selected Gameobject has no Mesh Handler.</red>");
        }
        else
        {
            // Not necessary since UI doesnt show this button if no gameobject has been selected (same for other error)
            Debug.Log("<color=red> No Gameobject has been selected.</red>");
        }


    }

    #endregion
}
