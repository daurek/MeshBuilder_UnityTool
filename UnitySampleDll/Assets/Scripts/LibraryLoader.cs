using UnityEngine;
using System.Runtime.InteropServices;

public class LibraryLoader : MonoBehaviour
{

    [DllImport("SampleCppDll")]
    private static extern Vector3 Add(Vector3 a, Vector3 b);
    [DllImport("SampleCppDll")]
    private static extern int Multiply(int a, int b);

    void Awake ()
    {
        print(Add(Vector3.one, Vector3.zero));
    }

}
