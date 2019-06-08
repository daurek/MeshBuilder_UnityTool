using UnityEditor;
using UnityEngine;

/// <summary>
/// Paints info over the Scene and handles Mesh editing 
/// </summary>
[CustomEditor(typeof(MeshHandler))]
public class MeshEditor : Editor
{
    #region Variables

    /// <summary>
    /// Selected mesh handler reference
    /// </summary>
    private static MeshHandler meshHandler;

    /// <summary>
    /// Cached Transform
    /// </summary>
    private Transform meshHandlerTransform;

    /// <summary>
    /// Cached mesh
    /// </summary>
    private Mesh mesh;

    /// <summary>
    /// Current mode
    /// </summary>
    public static EditMode editMode;
    
    /// <summary>
    /// Current selected vertex
    /// </summary>
    private static int selectedVertex = -1;

    /// <summary>
    /// Current selected face
    /// </summary>
    private static int[] selectedFace;

    /// <summary>
    /// Available edit modes
    /// </summary>
    public enum EditMode
    {
        Vertex,
        Faces,
        Gameobject
    }

    #endregion

    #region Methods
    
    /// <summary>
    /// Displays elements on selected Mesh Handler
    /// </summary>
    private void OnSceneGUI()
    {
        // Check if target has changed
        if (!target || target != meshHandler)
        {
            // Reset
            selectedVertex = -1;
            selectedFace = null;
            ChangeEditMode(EditMode.Gameobject);
        }

        // Get Mesh
        meshHandler = target as MeshHandler;
        meshHandlerTransform = meshHandler.transform;
        mesh = meshHandler.GetMesh();

        // Check if exists
        if (mesh)
        {
            // Depending on the selected mode we display diferent elements
            switch (editMode)
            {
                case EditMode.Vertex:
                    EditVertices();
                    break;
                case EditMode.Faces:
                    EditFaces();
                    break;
                case EditMode.Gameobject:
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// Changes current edit mode with the given one
    /// </summary>
    public static void ChangeEditMode(EditMode givenEditMode)
    {
        // Return if it's already applied
        if (givenEditMode == editMode)
            return;

        editMode = givenEditMode;

        // Depending on the mode we have to deselected face/vertex or disable transform tool
        switch (editMode)
        {
            case EditMode.Vertex:
                selectedFace = null;
                Tools.current = Tool.None;
                break;
            case EditMode.Faces:
                selectedVertex = -1;
                Tools.current = Tool.None;
                break;
            case EditMode.Gameobject:
                selectedFace = null;
                selectedVertex = -1;
                Tools.current = Tool.Move;
                break;
            default:
                break;
        }

        // Repaint to apply mode
        SceneView.RepaintAll();
    }

    /// <summary>
    /// Displays vertices and handles their movement
    /// </summary>
    private void EditVertices()
    {
        // Cache data
        Vector3[] vertices = mesh.vertices;
        int meshVerticesLength = vertices.Length;
        // Vertices color
        Handles.color = Color.yellow;

        // Loops Vertices
        for (int i = 0; i < meshVerticesLength; i++)
        {
            // Displays button on vertex
            if (Handles.Button(meshHandlerTransform.TransformPoint(vertices[i]), Quaternion.identity, 0.01f, 0.01f, Handles.DotHandleCap))
                // Selected
                selectedVertex = i;
        }

        // Check if vertex has been selected
        if (selectedVertex != -1)
        {
            // If movement has been detected
            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.PositionHandle(meshHandlerTransform.TransformPoint(vertices[selectedVertex]), Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                // Get same vertices (explanation on method)
                int[] sameVertices = GetSameVertices(selectedVertex);
                // Move vertices
                for (int j = 0; j < sameVertices.Length; j++)
                    meshHandler.MoveVertex(sameVertices[j], meshHandlerTransform.InverseTransformPoint(newPos));
            }
        }
    }

    /// <summary>
    /// Displays faces and handles their movement
    /// </summary>
    private void EditFaces()
    {
        // Cache data
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        int meshTrianglesLength = triangles.Length;
        // Set paint color
        Handles.color = Color.blue;
      
        // Loop every triangle
        for (int i = 0; i < meshTrianglesLength; i+= 3)
        {
            // Draw triangle
            Handles.DrawPolyLine(meshHandlerTransform.TransformPoint(vertices[triangles[i]]), meshHandlerTransform.TransformPoint(vertices[triangles[i + 1]]), meshHandlerTransform.TransformPoint(vertices[triangles[i + 2]]), meshHandlerTransform.TransformPoint(vertices[triangles[i]]));
            
            // Get the middle point
            Vector3 middlePoint = LibraryLoader.GetMiddlePoint(new Vector3[] { vertices[triangles[i]], vertices[triangles[i + 1]], vertices[triangles[i + 2]] }, 3);
            
            // Set button to that position
            if (Handles.Button(meshHandlerTransform.TransformPoint(middlePoint), Quaternion.identity, 0.01f, 0.01f, Handles.DotHandleCap))
            {
                // Set selected face on click
                selectedFace = new int[]
                {
                    triangles[i],
                    triangles[i+1],
                    triangles[i+2]
                };
            }
        }

        // If a face has been selected
        if (selectedFace != null && selectedFace.Length > 0)
        {
            // Paint the triangle in green
            Handles.color = Color.green;
            Handles.DrawPolyLine(meshHandlerTransform.TransformPoint(vertices[selectedFace[0]]), meshHandlerTransform.TransformPoint(vertices[selectedFace[1]]), meshHandlerTransform.TransformPoint(vertices[selectedFace[2]]), meshHandlerTransform.TransformPoint(vertices[selectedFace[0]]));

            // Get middle point
            Vector3 middlePoint = LibraryLoader.GetMiddlePoint(new Vector3[] { vertices[selectedFace[0]], vertices[selectedFace[1]], vertices[selectedFace[2]] }, 3);

            // Start checking for handle movement
            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.PositionHandle(meshHandlerTransform.TransformPoint(middlePoint), Quaternion.identity);
            // If movement has been detected
            if (EditorGUI.EndChangeCheck())
            {
                // Move selected face vertices 
                for (int i = 0; i < selectedFace.Length; i++)
                {
                    // Get same vertices (explanation on method)
                    int[] sameVertices = GetSameVertices(selectedFace[i]);
                    // Move them along the way
                    for (int j = 0; j < sameVertices.Length; j++)
                        meshHandler.MoveVertex(sameVertices[j], meshHandlerTransform.InverseTransformPoint(vertices[sameVertices[j]]) + meshHandlerTransform.InverseTransformPoint(newPos) - meshHandlerTransform.InverseTransformPoint(middlePoint));
                }
            }
        }
    }

    /// <summary>
    /// Sets Vertex color material to the selected gameobject
    /// </summary>
    public static void SetAsVertexColor()
    {
        // Get selected gameobject
        GameObject selectedGameobject = Selection.activeGameObject;

        // If it exists
        if (selectedGameobject)
        {
            // Get renderer
            MeshRenderer meshRenderer = selectedGameobject.GetComponent<MeshRenderer>();

            // If it exists
            if (meshRenderer)
            {
                // Set vertex color material
                meshRenderer.material = (Material)Resources.Load("Materials/VertexColorShadowed");
                selectedGameobject.GetComponent<MeshHandler>().isVertexColored = true;

                SceneView.RepaintAll();
            }
        }
    }

    /// <summary>
    /// Applies given vertex color to the Mesh
    /// </summary>
    /// <param name="color"></param>
    public static void ApplyVertexColor(Color color)
    {
        // Check if it has a vertex color materials
        if (!meshHandler.isVertexColored)
        {
            Debug.Log("<color=red> No vertex color material.</color>");
            return;
        }

        // Depending on the edit mode we do different applications
        switch (editMode)
        {
            case EditMode.Vertex:
                // If a vertex has been selected
                if (selectedVertex != -1)
                {
                    // Get same vertices and paint them all
                    int[] sameVertices = GetSameVertices(selectedVertex);
                    for (int j = 0; j < sameVertices.Length; j++)
                        meshHandler.SetVertexColor(sameVertices[j], color);
                }
                else
                    Debug.Log("<color=red> No vertex selected.</color>");
                break;
            case EditMode.Faces:
                // If a face has been selected
                if (selectedFace != null && selectedFace.Length > 0)
                    // Loop every face vertex
                    for (int i = 0; i < selectedFace.Length; i++)
                    {
                        // Get same vertices and paint them all
                        int[] sameVertices = GetSameVertices(selectedFace[i]);
                        for (int j = 0; j < sameVertices.Length; j++)
                            meshHandler.SetVertexColor(sameVertices[j], color);
                    }
                else
                    Debug.Log("<color=red> No face selected.</color>");
                break;
            case EditMode.Gameobject:
                // Paint entire mesh
                int vertices = meshHandler.GetMesh().vertexCount;
                for (int j = 0; j < vertices; j++)
                    meshHandler.SetVertexColor(j, color);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Finds vertices with the same position
    /// Why: Most meshes (unity cube for example) do not use reuse triangle indices in order to have different normals per vertex 
    /// </summary>
    public static int[] GetSameVertices(int givenVertex)
    {
        // Get Mesh
        Mesh mesh = meshHandler.GetMesh();
        // Get vertex index pointer of the same given vertex
        LibraryLoader.IntArray verticesArrayPointer = LibraryLoader.GetSameVertices(mesh.vertices, mesh.vertexCount, givenVertex);
        // Create vertex index array
        int[] verticesArray = new int[verticesArrayPointer.size];
        // Copy pointer data to array data
        System.Runtime.InteropServices.Marshal.Copy(verticesArrayPointer.array, verticesArray, 0, verticesArrayPointer.size);
        // Returns final array data
        return verticesArray;
    }

    /// <summary>
    /// Calls for and extrude on selected face
    /// </summary>
    public static void ExtrudeFace()
    {
        // Exture current selected face
        meshHandler.ExtrudeFace(selectedFace);
        // Empty selected face
        selectedFace = null;
    }

    #endregion
}
