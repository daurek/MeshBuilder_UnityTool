using UnityEngine;

[ExecuteInEditMode]
public class MeshHandler : MonoBehaviour
{
    private MeshFilter meshFilter;
    private Mesh mesh;
    private Mesh oldMesh;
    private Vector3[] vertices;
    private Color[] colors;
    public bool isVertexColored { get; set; }

    private void Awake()
    {
        SetMesh();
    }

    public Mesh GetMesh()
    {
        return mesh;
    }

    public void SetMesh()
    {
        meshFilter = GetComponent<MeshFilter>();
        oldMesh = meshFilter.sharedMesh;
        mesh = new Mesh();
        mesh.vertices   = vertices = oldMesh.vertices;
        mesh.triangles  = oldMesh.triangles;
        mesh.normals    = oldMesh.normals;
        mesh.colors     = oldMesh.colors;
        mesh.uv         = oldMesh.uv;
        mesh.MarkDynamic();
    
        meshFilter.mesh = mesh;
        colors = new Color[mesh.vertexCount];
    }

    public void MoveVertex(int i, Vector3 position)
    {
        vertices[i] = position;
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }

    public void SetVertexColor(int i, Color color)
    {
        colors[i] = color;
        mesh.colors = colors;
    }
}
