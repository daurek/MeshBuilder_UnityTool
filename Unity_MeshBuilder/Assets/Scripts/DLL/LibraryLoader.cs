using UnityEngine;
using System.Runtime.InteropServices;
using System;

/// <summary>
/// Loads C++ DLL methods
/// </summary>
public class LibraryLoader : MonoBehaviour
{
    private const string libName = "MeshBuilderLib";

    [DllImport(libName)]
    public static extern IntArray GetSameVertices (Vector3[] vertices, int length, int vertexIndex);

    [DllImport(libName)]
    public static extern IntPtr CreateTriangle (int begin, int[] triangles, int[] givenTriangle);

    [DllImport(libName)]
    public static extern Vector3 Cross(Vector3 firstVector, Vector3 secondVector, Vector3 rightHandVector);

    [DllImport(libName)]
    public static extern Vector3 Normalize(Vector3 givenVector);

    [DllImport(libName)]
    public static extern Vector3 GetMiddlePoint(Vector3[] vectors, int size);

    public struct IntArray
    {
        public IntPtr array;
        public int size;
    };
}
