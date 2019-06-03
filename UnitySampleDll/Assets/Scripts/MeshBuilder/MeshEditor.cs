using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MeshHandler))]
public class MeshEditor : Editor
{
    private static MeshHandler meshHandler;
    private Transform meshHandlerTransform;
    private Mesh mesh;

    public static EditMode editMode;
    private static Tool lastTool;
    private static int selectedVertex = -1;

    public enum EditMode
    {
        VERTEX,
        FACES,
        GAMEOBJECT
    }

    private void OnSceneGUI()
    {
        meshHandler = target as MeshHandler;
        meshHandlerTransform = meshHandler.transform;
        mesh = meshHandler.GetMesh();

        if (mesh)
        {

            switch (editMode)
            {
                case EditMode.VERTEX:
                    EditVertices();
                    break;
                case EditMode.FACES:
                    EditFaces();
                    break;
                case EditMode.GAMEOBJECT:
                    break;
                default:
                    break;
            }
        }



        //else
        //    Debug.Log("<color=yellow>No Mesh available.</color>");
    }

    public static void ChangeEditMode(EditMode _editMode)
    {
        editMode = _editMode;

        switch (editMode)
        {
            case EditMode.VERTEX:
                lastTool = Tools.current;
                Tools.current = Tool.None;
                break;
            case EditMode.GAMEOBJECT:
                Tools.current = lastTool;
                break;
            default:
                break;
        }

        SceneView.RepaintAll();
    }

    private void EditVertices()
    {
        // Cache data
        int meshVerticesLength = mesh.vertices.Length;
        Handles.color = Color.yellow;
        Vector3[] vertices = mesh.vertices;

       


        //for (int i = 0; i < meshVerticesLength; i++)
        //{
        //    Vector3 point = Handles.FreeMoveHandle(meshHandlerTransform.TransformPoint(vertices[i]), Quaternion.identity, 0.005f, Vector3.zero, Handles.DotHandleCap);


        //    //EditorGUI.BeginChangeCheck();
        //    //Handles.PositionHandle(Vector3.one, Quaternion.identity);
        //    //if (EditorGUI.EndChangeCheck())
        //    //{
        //    //}

        //    if (GUI.changed)
        //    {

               

        //        selectedVertex = i;
        //        Vector3 destination = meshHandler.transform.InverseTransformPoint(point);

                

        //        int [] sameVertices = mesh.vertices.Select((vertex, index) => vertex == mesh.vertices[i] ? index : -1).Where(j => j != -1).ToArray();

        //        for (int j = 0; j < sameVertices.Length; j++)
        //            meshHandler.MoveVertex(sameVertices[j], destination);
        //    }
        //}

        for (int i = 0; i < meshVerticesLength; i++)
        {
            if (Handles.Button(meshHandlerTransform.TransformPoint(vertices[i]), Quaternion.identity, 0.01f, HandleUtility.GetHandleSize(meshHandlerTransform.TransformPoint(vertices[i])), Handles.DotHandleCap))
            {
                // Selected
                selectedVertex = i;
            }
        }

        if (selectedVertex != -1)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.PositionHandle(meshHandlerTransform.TransformPoint(vertices[selectedVertex]), Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                // Move vertices
                int[] sameVertices = GetSameVertices(selectedVertex);
                for (int j = 0; j < sameVertices.Length; j++)
                    meshHandler.MoveVertex(sameVertices[j], meshHandler.transform.InverseTransformPoint(newPos));
            }
        }

    }

    private void EditFaces()
    {
        int meshTrianglesLength = mesh.triangles.Length;
        Handles.color = Color.yellow;
      
        for (int i = 0; i < meshTrianglesLength; i++)
        {
            // GUI
            
        }
    }

    public static void SetAsVertexColor()
    {
        MeshRenderer meshRenderer = meshHandler.GetComponent<MeshRenderer>();

        if (meshRenderer)
        {
            //for (int i = 0; i < meshRenderer.materials.Length; i++)
            //{
            //    meshRenderer.materials[i] = null;
            //}
            //mesh.colors = new Color[mesh.vertexCount];
            meshRenderer.material = (Material)Resources.Load("Materials/VertexColorShadowed");
            // meshHandler.isVertexColored = true;
        }
        else
        {
            // ERROR
        }

        SceneView.RepaintAll();

    }

    public static void ApplyVertexColor(Color color)
    {

        switch (editMode)
        {
            case EditMode.VERTEX:
                int[] sameVertices = GetSameVertices(selectedVertex);
                for (int j = 0; j < sameVertices.Length; j++)
                    meshHandler.SetVertexColor(sameVertices[j], color);
                break;
            case EditMode.FACES:
                break;
            case EditMode.GAMEOBJECT:
                int vertices = meshHandler.GetMesh().vertices.Length;
                for (int j = 0; j < vertices; j++)
                    meshHandler.SetVertexColor(j, color);
                break;
            default:
                break;
        }

      
    }

    public static int[] GetSameVertices(int givenVertex)
    {
        Mesh mesh = meshHandler.GetMesh();
        return mesh.vertices.Select((vertex, index) => vertex == mesh.vertices[givenVertex] ? index : -1).Where(j => j != -1).ToArray();
    }
}
