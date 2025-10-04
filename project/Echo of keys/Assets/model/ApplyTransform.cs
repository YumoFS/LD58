using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyTransform : MonoBehaviour
{
    [ContextMenu("Apply Transform To Mesh")]
    void ApplyTransformToMesh()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null) return;

        Mesh mesh = Instantiate(mf.sharedMesh); // 拷贝一份
        Vector3[] verts = mesh.vertices;

        Matrix4x4 localToWorld = transform.localToWorldMatrix;
        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] = localToWorld.MultiplyPoint3x4(verts[i]);
        }
        mesh.vertices = verts;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        mf.sharedMesh = mesh;

        // Reset transform
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }
}
