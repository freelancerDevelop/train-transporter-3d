﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class meshGenerator{

    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve _heightCurve, int levelOfDetail)
	{
		AnimationCurve heightCurve = new AnimationCurve (_heightCurve.keys);

        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height- 1) / -2f;
		bool isFlatshaded = true;

        int meshSimpplificationIncrement = (levelOfDetail == 0)?1:levelOfDetail * 2;
        int verticesPerLine = (width - 1) / meshSimpplificationIncrement + 1;


        MeshData meshData = new MeshData(verticesPerLine,verticesPerLine);
        int vertexIndex = 0;

        for(int y = 0; y < height; y += meshSimpplificationIncrement){
            for(int x = 0; x < width; x += meshSimpplificationIncrement)
            {
                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, heightCurve.Evaluate(heightMap [x, y]) * heightMultiplier, topLeftZ -y);
                meshData.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);

                if( x < width - 1  && y < height -1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
                    meshData.AddTriangle(vertexIndex + verticesPerLine + 1 , vertexIndex, vertexIndex + 1);
                }
                vertexIndex++;
            }          
        }
		if (isFlatshaded)
		{
			RecalculateFlatShadedMeshData(meshData);
		}
        return meshData;
    }

	private static void RecalculateFlatShadedMeshData(MeshData meshData)
	{
		Vector3[] oldVerts = meshData.vertices;
		Vector2[] oldUvs = meshData.uvs;
		int[] triangles = meshData.triangles;


		Vector3[] vertices = new Vector3[triangles.Length];
		Vector2[] uvs = new Vector2[triangles.Length];

		for (int i = 0; i < triangles.Length; i++)
		{
			vertices[i] = oldVerts[triangles[i]];
			uvs[i] = oldUvs[triangles[i]];
			triangles[i] = i;
		}

		meshData.vertices = vertices;
		meshData.triangles = triangles;
		meshData.uvs = uvs;
	}
}

public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    int triangleIndex;

    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1)*(meshHeight - 1) * 6];
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles [triangleIndex] = a;
        triangles [triangleIndex + 1] = b;
        triangles [triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }
}
