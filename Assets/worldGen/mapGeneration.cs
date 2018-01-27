﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class mapGeneration : MonoBehaviour {

    public enum Drawmode {NoiseMap, ColourMap, Mesh};
    public Drawmode drawMode;

    public const int mapChunkSize = 241;
    [Range(0,6)]
    public int levelOfDetail;
	public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    public bool autoUpdate;

    public TerrainType[] regions;

	Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
	Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

	public void DrawMapInEditor(){
		MapData mapData = generateMapData ();

		mapDisplay display = FindObjectOfType<mapDisplay> ();
		if(drawMode == Drawmode.NoiseMap)
		{
			display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightmap));

		}else if (drawMode == Drawmode.ColourMap)
		{
			display.DrawTexture(TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
		}else if (drawMode == Drawmode.Mesh)
		{
			display.drawMesh(meshGenerator.GenerateTerrainMesh(mapData.heightmap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
		}
		
	}

	public void RequestMapData(Action<MapData> callback)
	{
		ThreadStart threadStart = delegate {
			MapDataThread (callback);
		};
		new Thread (ThreadStart).Start ();
	}

	void MapDataThread(Action<MapData> callback)
	{
		MapData mapData = generateMapData ();
		lock (mapDataThreadInfoQueue) {
			mapDataThreadInfoQueue.Enqueue (new MapThreadInfo<MapData> (callback, mapData));
		}
	}

	public void requestMeshData(MapData mapData, Action<MapData> callback)
	{
		
	}
	public void meshDataThread(MapData mapData, Action<MapData> callback)
	{
		MeshData meshData = meshGenerator.GenerateTerrainMesh(mapData.heightmap, meshHeightMultiplier, meshHeightCurve, levelOfDetail );
		lock (meshDataThreadInfoQueue);
	}
	void Update(){
		if (mapDataThreadInfoQueue > 0) {
			for (int i = 0; i <mapDataThreadInfoQueue.Count; i++) {
				MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue ();
				threadInfo.callback (threadInfo.parameter);
			}
		}
	}

	MapData generateMapData(){
		
		float[,] noiseMap = noise.generateNoiseMap (mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset, regions);

        Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
        for(int y = 0; y < mapChunkSize; y++)
        {
            for(int x = 0; x < mapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];   
                for (int i = 0; i < regions.Length; i++)
                {

                if (currentHeight <= regions[i].height)
                {
                colourMap [y * mapChunkSize + x] = regions [i].colour;
                break;
                }

                }
            }
        }
		return new MapData (noiseMap, colourMap);

	}

    private void OnValidate()
    {
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }
        if (noiseScale < 0)
        {
            noiseScale = 0;
        }
    }

	struct MapThreadInfo<T>
	{
		public readonly Action<T> callback;
		public readonly T parameter;

		public MapThreadInfo (Action<T> callback, T parameter)
		{
			this.callback = callback;
			this.parameter = parameter;
		}
		
	}

}
[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color colour;
}

public struct MapData {
	public readonly float[,] heightmap;
	public readonly Color[] colourMap;

	public MapData (float[,] heightmap, Color[] colourMap)
	{
		this.heightmap = heightmap;
		this.colourMap = colourMap;
	}
	

}
