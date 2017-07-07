using UnityEngine;
using System.Collections;

public class MeshUtils
{
    public static Mesh MakeQuad(float originX, float originZ, float sizeX, float sizeZ)
    {
        Mesh newMesh = new Mesh();

        Vector3[] vertices = new Vector3[]
        {
            new Vector3( originX,         0, originZ ),
            new Vector3( originX + sizeX, 0, originZ ),
            new Vector3( originX + sizeX, 0, originZ + sizeZ ),
            new Vector3( originX,         0, originZ + sizeZ )
        };

        int[] triangles = new int[]
        {
            2, 1, 0,
            0, 3, 2
        };

        Vector2[] uvs = new Vector2[]
        {
            new Vector2( 0, 0 ),
            new Vector2( 1, 0 ),
            new Vector2( 1, 1 ),
            new Vector2( 0, 1 )
        };

        newMesh.vertices = vertices;
        newMesh.triangles = triangles;
        newMesh.uv = uvs;

        return (newMesh);
    }
}