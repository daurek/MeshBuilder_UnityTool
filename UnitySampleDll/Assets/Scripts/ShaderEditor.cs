using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Reflection;

public class ShaderEditor : EditorWindow
{

    private static Texture2D background;
    private int editorMinSize = 300;
    private bool dragBox = false;

    private List<Node> nodes = new List<Node>();
    private Node selectedNode;

    [DllImport("SampleCppDll")]
    private static extern int Multiply(int a, int b);

    [MenuItem("<ShaderEditor>/Open Shader Editor")]
    public static void OpenEditor()
    {
        ShaderEditor editor = (ShaderEditor)GetWindow(typeof(ShaderEditor));
        editor.minSize = new Vector2(800, 500);
        editor.wantsMouseMove = true;

        background = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        background.SetPixel(0, 0, new Color(0.3f, 0.3f, 0.3f));
        background.Apply();

        editor.Show();
    }

    private void Awake()
    {
        //nodes.Add(new Node("Node1", new Vector2(400, 200), new Vector2(100, 50)));
        //nodes.Add(new Node("Node2", new Vector2(530, 350), new Vector2(100, 50)));
    }

    public void CreateNode(object _node)
    {
        Node node = (Node)_node;
        nodes.Add(new Node(node.name, node.position, node.size));
    }

    private void OnGUI()
    {
        // Draw Background
        GUI.DrawTexture(new Rect(editorMinSize, 0, maxSize.x - editorMinSize, maxSize.y), background, ScaleMode.StretchToFill);
        // Draw everything else in front
        GUI.depth = 1;

        // Get Editor Event (input)
        Event currentEvent = Event.current;

        // Right Click
        if (currentEvent.type == EventType.MouseDown && currentEvent.button == 1)
        {
            // Open Dropdown
            GenericMenu rightClickDropdown = new GenericMenu();
            rightClickDropdown.AddItem(new GUIContent("Add"), false, CreateNode,        new Node("Add", currentEvent.mousePosition, new Vector2(100, 50)));
            rightClickDropdown.AddItem(new GUIContent("Multiply"), false, CreateNode,   new Node("Multiply", currentEvent.mousePosition, new Vector2(100, 50)));
            rightClickDropdown.ShowAsContext();
        }

        // Loop nodes
        foreach (Node node in nodes)
        {
            Rect rect = new Rect(node.position, node.size);

            GUI.color = Color.white;

            // Hovering Node
            if (rect.Contains(currentEvent.mousePosition))
            {
                // Select Node
                if (currentEvent.type == EventType.MouseDown)
                {
                    selectedNode = node;
                }
                // Unselect
                else if (currentEvent.type == EventType.MouseUp)
                {
                    selectedNode = null;
                }

                // Current selected node
                if (selectedNode == node)
                {
                    GUI.color = Color.green;
                    // Dragging selected node
                    if (currentEvent.type == EventType.MouseDrag)
                    {
                        node.position = rect.position = new Vector2(currentEvent.mousePosition.x - 30, currentEvent.mousePosition.y - 30);
                    }
                }
            }
            
            // Draw node
            GUI.Box(rect, node.name);

            //GUI.DrawTexture(new Rect(node.position + new Vector2(0, -20), node.size), background);
        }

        Repaint();
    }
}

public class Node
{
    public string name;
    public Vector2 position;
    public Vector2 size;

    public Node(string _name, Vector2 _position, Vector2 _size)
    {
        name = _name;
        position = _position;
        size = _size;
    }
}

