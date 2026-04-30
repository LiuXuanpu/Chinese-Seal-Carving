using UnityEngine;
[RequireComponent(typeof(MeshFilter))]
public class SubdividedCube : MonoBehaviour
{
    void Start()
    {
        Mesh mesh = new Mesh();
        // 生成精细网格（32x32个顶点）
        int res = 32;
        Vector3[] verts = new Vector3[(res + 1) * (res + 1)];
        for (int x = 0; x <= res; x++)
            for (int y = 0; y <= res; y++)
                verts[x + y * (res + 1)] = new Vector3((float)x / res - 0.5f, (float)y / res - 0.5f, 0);
        mesh.vertices = verts;
        int[] tris = new int[res * res * 6];
        for (int x = 0, ti = 0; x < res; x++)
            for (int y = 0; y < res; y++)
            {
                int a = x + y * (res + 1);
                int b = a + 1;
                int c = a + (res + 1);
                int d = c + 1;
                tris[ti++] = a; tris[ti++] = c; tris[ti++] = b;
                tris[ti++] = b; tris[ti++] = c; tris[ti++] = d;
            }
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh; // 更新碰撞体
    }
}