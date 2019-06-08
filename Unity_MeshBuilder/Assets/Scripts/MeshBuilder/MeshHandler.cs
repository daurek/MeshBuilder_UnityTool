using UnityEngine;

[ExecuteInEditMode]
[DisallowMultipleComponent]
/// Component that allows the user to edit and export Mesh
public class MeshHandler : MonoBehaviour
{
    #region Variables

    /// <summary>
    /// Mesh filter reference
    /// </summary>
    private MeshFilter  meshFilter;

    /// <summary>
    /// Mesh cached
    /// </summary>
    private Mesh        mesh;

    /// <summary>
    /// Vertex array which we will modify and apply to Mesh
    /// </summary>
    private Vector3[]   vertices;

    /// <summary>
    /// Triangle array which we will modify and apply to Mesh
    /// </summary>
    private int[]       triangles;

    /// <summary>
    /// Color array which we will modify and apply to Mesh
    /// </summary>
    private Color[]     colors;

    /// <summary>
    /// Is the Mesh using Vertex colors
    /// </summary>
    [HideInInspector]
    public bool         isVertexColored;

    #endregion

    #region Methods
    
    private void Awake()
    {
        SetMesh();
    }

    /// <summary>
    /// Returns Mesh
    /// </summary>
    public Mesh GetMesh()
    {
        return mesh;
    }

    /// <summary>
    /// Clones mesh in order to be able to modify it
    /// </summary>
    public void SetMesh()
    {
        // Cache meshfilter
        meshFilter = GetComponent<MeshFilter>();
        // Get old mesh (we can't use sharedMesh because it will change the original Mesh)
        // For example if we use shared mesh on the cube the user will have to reinstall Unity because he modified the original Cube mesh
        Mesh oldMesh = meshFilter.sharedMesh;
        // Creates a new mesh and copy everything from the original
        mesh = new Mesh();
        mesh.vertices   = vertices = oldMesh.vertices;
        mesh.triangles  = triangles = oldMesh.triangles;
        mesh.normals    = oldMesh.normals;
        mesh.uv         = oldMesh.uv;
        // This makes us able to modify the mesh faster
        mesh.MarkDynamic();
    
        // Set Mesh colors
        colors = new Color[mesh.vertexCount];
        // Set to gray
        for (int i = 0; i < colors.Length; i++)
            colors[i] = Color.gray;

        mesh.colors = colors;
        // Sets new mesh into mesh filter
        meshFilter.mesh = mesh;
    }
    
    /// <summary>
    /// Moves given vertex index into the new given position
    /// </summary>
    public void MoveVertex(int givenIndex, Vector3 position)
    {
        // Set new position
        vertices[givenIndex] = position;
        // Apply the entire vertex array into the mesh (can't do it directly)
        mesh.vertices = vertices;
    }

    /// <summary>
    /// Sets given vertex index into the new given color
    /// </summary>
    public void SetVertexColor(int givenIndex, Color color)
    {
        // Set new color
        colors[givenIndex] = color;
        // Apply the entire color array into the mesh (can't do it directly)
        mesh.colors = colors;
    }

    /// <summary>
    /// Extrudes the selected given face 
    /// </summary>
    /// <param name="faceIndices"></param>
    public void ExtrudeFace(int[] faceIndices)
    {
        // Face selection check
        if (faceIndices == null)
        {
            Debug.Log("<color=yellow> No Face selected.</color>");
            return;
        }

        // Check if given face is a triangle
        if (faceIndices.Length != 3)
        {
            Debug.Log("<color=red> Face isn't a triangle.</color>");
            return;
        }

        // ________ Create extruded face vertices

        // Resize array to add new three vertices
        System.Array.Resize(ref vertices, vertices.Length + 3);
        // Cache vertex indices
        int firstVertexIndex = vertices.Length - 3;
        int secondVertexIndex = vertices.Length - 2;
        int thirdVertexIndex = vertices.Length - 1;

        // Get face normal (https://computergraphics.stackexchange.com/questions/4031/programmatically-generating-vertex-normals)
        Vector3 faceNormal = LibraryLoader.Normalize(LibraryLoader.Cross(vertices[faceIndices[1]], vertices[faceIndices[2]], vertices[faceIndices[0]])); 
        faceNormal *= 0.5f;

        // We use the face normal to push away the new vertices as the extrude
        vertices[firstVertexIndex] = vertices[faceIndices[0]] + faceNormal;
        vertices[secondVertexIndex] = vertices[faceIndices[1]] + faceNormal;
        vertices[thirdVertexIndex] = vertices[faceIndices[2]] + faceNormal;
        // Set final vertices
        mesh.vertices = vertices;

      
        // ________ Replace old face into the extruded face

        // Loop triangles
        for (int i = 0; i < triangles.Length; i+= 3)
        {
            // Replace old face into new extrude face
            if (triangles[i] == faceIndices[0] && triangles[i+1] == faceIndices[1] && triangles[i+2] == faceIndices[2])
            {
                triangles[i] = firstVertexIndex;
                triangles[i+1] = secondVertexIndex;
                triangles[i+2] = thirdVertexIndex;
                break;
            }
        }

        // ________ Create every new face after a triangle face extrude
        // Little drawing --> /_\ We have to create 3 Quads (6 triangles).

        // We will new 6 new triangles
        int newFaces = 6;
        // That means 3 indices per face * 6 faces = 18 new indices
        int newIndices = faceIndices.Length * newFaces;
        // Resizes triangle array to a new size
        System.Array.Resize(ref triangles, triangles.Length + newIndices);

        // Create the three needed quads
        CreateQuad(triangles.Length - newIndices,               new int[] { faceIndices[1], faceIndices[2], secondVertexIndex, thirdVertexIndex   });
        CreateQuad(triangles.Length - newIndices + newFaces,    new int[] { faceIndices[2], faceIndices[0], thirdVertexIndex, firstVertexIndex    });
        CreateQuad(triangles.Length - newFaces,                 new int[] { faceIndices[0], faceIndices[1], firstVertexIndex, secondVertexIndex   });

        // Apply triangles to mesh
        mesh.triangles = triangles;

        // Apply vertex color if it has the material
        if (isVertexColored)
        {
            // Resize colors and apply new colors
            System.Array.Resize(ref colors, colors.Length + 3);
            for (int i = 0; i < 3; i++)
                colors[colors.Length - 3 + i] = Color.gray;

            mesh.colors = colors;
        }
        else
            // Otherwise just recalculate normals to keep lighting correct
            mesh.RecalculateNormals();
    }

    /// <summary>
    /// Creates a Quad (2triangles) in the given index with the given vertex indices
    /// </summary>
    private void CreateQuad(int begin, int[] indices)
    {
        // Explanation: https://imgur.com/2FFEWcC
        // First triangle 
        LibraryLoader.CreateTriangle(begin,     triangles,  new int[] { indices[0], indices[1], indices[2] });
        // Second triangle
        LibraryLoader.CreateTriangle(begin + 3, triangles,  new int[] { indices[3], indices[2], indices[1] });
    }

    #endregion
}
