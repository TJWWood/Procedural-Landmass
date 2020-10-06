using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using static InfiniteTerrain;

public class MapGenerator : MonoBehaviour
{

    public Noise.NormalizeMode normalizeMode;

    public float noiseScale;
    public float lacunarity;

    public int seed = 0;
    public Vector2 offset;

    public TerrainType[] regions;
    public HeatType[] heats;

    public float meshHeightMultiplier;
    public int octaves;
    [Range(0, 6)]
    public int levelOfDetail;
    [Range(0, 1)]
    public float persistence;
    bool genDone = false;
    GameObject viewer;
    GameObject extraStore;
    Vector2 cent;
    float[,] noisey;
    ArrayList mapDatas = new ArrayList();
    Vector3 locUpdate;

    void Start()
    {
        extraStore = GameObject.Find("ExtraStore");
        viewer = GameObject.Find("Viewer");
    }
    void Update()
    {
        /*
        if (viewer.transform.hasChanged)
        {
            print("Changed");
            viewer.transform.hasChanged = false;
            removeObjectsForNewGeneration();
            locUpdate = viewer.transform.position;
                genDone = false;
                if (!genDone)
                {
                    
                float[,] noiseMap = noisey;

                    for (int y = 0; y < mapChunkSize; y++)
                    {
                        for (int x = 0; x < mapChunkSize; x++)
                        {
                            float currentHeight = noiseMap[x, y];
                            if (currentHeight >= 0.45 && currentHeight <= 0.6)
                            {
                                float highestNoiseHeight = 0f;
                                if (x != 0 && y != 0 && x < mapChunkSize - 1 && y < mapChunkSize - 1)
                                {
                                    highestNoiseHeight = Mathf.Max(currentHeight, noiseMap[x - 1, y - 1], noiseMap[x, y + 1], noiseMap[x, y - 1], noiseMap[x + 1, y + 1]);
                                }

                                if (highestNoiseHeight == currentHeight)
                                {
                                    float heightToUse = currentHeight;

                                    float xToUse = x;
                                    float yToUse = y;
                                    pos = new Vector3((xToUse * 10) - (mapChunkSize * 5), (heightToUse * 100) - 30, (yToUse * 10) - (mapChunkSize * 5));
                                    pos = Vector3.Reflect(pos, Vector3.forward);

                                    Instantiate(tree, pos, new Quaternion(), extraStore.transform);

                                }
                                else if (x >= mapChunkSize - 1 && y >= mapChunkSize - 1)
                                {
                                    break;
                                }
                            }
                            if (currentHeight >= 0.85f)
                            {
                                float highestNoiseHeight = 0f;
                                if (x != 0 && y != 0 && x < mapChunkSize - 1 && y < mapChunkSize - 1)
                                {
                                    highestNoiseHeight = Mathf.Max(currentHeight, noiseMap[x - 1, y - 1], noiseMap[x, y + 1], noiseMap[x, y - 1], noiseMap[x + 1, y + 1]);
                                }

                                if (highestNoiseHeight == currentHeight)
                                {
                                    float heightToUse = currentHeight;
                                    float xToUse = x;
                                    float yToUse = y;
                                    pos = new Vector3((xToUse * 10) - (mapChunkSize * 5), heightToUse * meshHeightMultiplier, (yToUse * 10) - (mapChunkSize * 5));
                                    pos = Vector3.Reflect(pos, Vector3.forward);
                                    Instantiate(snowParticleEffect, pos, new Quaternion(), extraStore.transform);
                                    snowParticleEffect.Play();
                                }
                                else if (x >= mapChunkSize - 1 && y >= mapChunkSize - 1)
                                {
                                    break;
                                }

                            }
                        }
                    }
                    
                    //extraStore.transform.position = viewer.transform.position;
                }
                genDone = true;
        }
        */
       
        /***
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (drawMode == DrawMode.Mesh)
            {
                drawMode = DrawMode.HeatMap;

            }
            else
            {
                drawMode = DrawMode.Mesh;
            }

        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            if (drawMode == DrawMode.Mesh)
            {

                noiseScale = UnityEngine.Random.Range(200, 450);
                lacunarity = UnityEngine.Random.Range(2, 2.5f);
                meshHeightMultiplier = UnityEngine.Random.Range(100, 200);
                octaves = UnityEngine.Random.Range(5, 6);
                persistence = UnityEngine.Random.Range(0.0f, 1.0f);
                if (transform.childCount > 0)
                {
                    removeObjectsForNewGeneration();
                }
                DrawMapInEditor();
            }
            else if (drawMode == DrawMode.HeatMap)
            {
                GenerateHeatMap();
            }

        }

        if (Input.GetKeyDown(KeyCode.S))
        {

            seed--;
            if (transform.childCount > 0)
            {
                removeObjectsForNewGeneration();
            }
            DrawMapInEditor();

        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            seed++;
            if (transform.childCount > 0)
            {
                removeObjectsForNewGeneration();
            }
            DrawMapInEditor();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            offset.x -= 0.1f;
            if (transform.childCount > 0)
            {
                removeObjectsForNewGeneration();
            }
            DrawMapInEditor();
        }
        else if (Input.GetKeyDown(KeyCode.H))
        {
            offset.x += 0.1f;
            if (transform.childCount > 0)
            {
                removeObjectsForNewGeneration();
            }
            DrawMapInEditor();
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            offset.y -= 0.1f;
            if (transform.childCount > 0)
            {
                removeObjectsForNewGeneration();
            }
            DrawMapInEditor();
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            offset.y += 0.1f;
            if (transform.childCount > 0)
            {
                removeObjectsForNewGeneration();
            }
            DrawMapInEditor();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            if (levelOfDetail == 6)
            {
                levelOfDetail = 6;
            }
            else
            {
                levelOfDetail++;
                if (transform.childCount > 0)
                {
                    removeObjectsForNewGeneration();
                }
                DrawMapInEditor();
            }
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            if (levelOfDetail == 0)
            {
                levelOfDetail = 0;
            }
            else
            {
                levelOfDetail--;
                if (transform.childCount > 0)
                {
                    removeObjectsForNewGeneration();
                }
                DrawMapInEditor();
            }
        }
        ***/

        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {

                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callBack(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callBack(threadInfo.parameter);
            }
        }
    }
    public enum DrawMode
    {
        NoiseMap,
        ColourMap,
        Mesh,
        HeatMap
    };
    public DrawMode drawMode;

    public const int mapChunkSize = 241;

    public bool autoUpdate;

    public AnimationCurve meshHeightCurve;

    public ParticleSystem snowParticleEffect;
    public GameObject tree;
    Vector3 pos;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMap(Vector2.zero);
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.ColourMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize)); ;
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.HeatMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColourMap(GenerateHeatMap(), mapChunkSize, mapChunkSize));
        }

    }

    public void RequestMapData(Vector2 centre, Action<MapData> callBack)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(centre, callBack);
        };
        
        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 centre,Action<MapData> callBack)
    {
        cent = centre;
        MapData mapData = GenerateMap(centre);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callBack, mapData));
        }
        mapDatas.Add(mapData);
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callBack)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapData, lod, callBack);
        };
        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callBack)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod);

        lock(meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callBack, meshData));
        }
    }

    MapData GenerateMap(Vector2 center)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistence, lacunarity, center + offset, normalizeMode);
        noisey = noiseMap;
        Color[] colourMap = new Color[mapChunkSize * mapChunkSize];

        float[,] heatNoiseMap = Noise.GenerateUniformNoiseMap(mapChunkSize, mapChunkSize, 0, mapChunkSize, 0);
        float[,] heatPerlinNoiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, 0, noiseScale, octaves, persistence, lacunarity, center + offset, normalizeMode);
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];
                float currentHeat = heatNoiseMap[x, y] * heatPerlinNoiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {

                    currentHeight += meshHeightCurve.Evaluate(currentHeat) * currentHeight /2f;
                        
                    if (currentHeight > 1)
                    {
                        currentHeight = 1;
                    }
                    if (currentHeight <= regions[i].height)
                    {
                        if (currentHeat <= heats[i].height)
                        {
                            if (regions[i].name != "Water")
                            {
                                colourMap[y * mapChunkSize + x] = Color.Lerp(regions[i].colour, heats[i].colour, 0.2f);
                                break;
                            }
                            else
                            {
                                colourMap[y * mapChunkSize + x] = Color.Lerp(regions[0].colour, heats[0].colour, 0.2f);
                                break;
                            }


                        }
                    }
                    
                }
            }
        }
        return new MapData(noiseMap, colourMap);
    }
    
    public Color[] GenerateHeatMap()
    {
        Color[] heatMap = new Color[mapChunkSize * mapChunkSize];

        float[,] heatNoiseMap = Noise.GenerateUniformNoiseMap(mapChunkSize, mapChunkSize, 0, mapChunkSize, 0);
        float[,] heatPerlinNoiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, 0, noiseScale, octaves, persistence, lacunarity, offset, normalizeMode);
        float loc = 0;
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                float currentLoc = heatNoiseMap[x,y] * heatPerlinNoiseMap[x, y];
                loc = currentLoc;
                for (int i = 0; i < heats.Length; i++)
                {
                    if (currentLoc <= heats[i].height)
                    {
                        heatMap[y * mapChunkSize + x] = heats[i].colour;
                        break;
                    }
                }
            }
        }
        return heatMap;
    }

    public void removeObjectsForNewGeneration()
    {
        for(int i = 0; i < transform.childCount; i++)
        {

            GameObject.Destroy(transform.GetChild(i).transform);
        }
    }
    void OnValidate()
    {
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if(octaves < 0)
        {
            octaves = 0;
        }
    }

    struct MapThreadInfo<T>
    {
        public Action<T> callBack;
        public T parameter;

        public MapThreadInfo(Action<T> callBack, T parameter)
        {
            this.callBack = callBack;
            this.parameter = parameter;
        }

    }

    [System.Serializable]
    public struct TerrainType
    {
        public string name;
        public float height;
        public Color colour;
    }

    [System.Serializable]
    public struct HeatType
    {
        public string name;
        public float height;
        public Color colour;
    }

    public struct MapData
    {
        public readonly float[,] heightMap;
        public readonly Color[] colourMap;

        public MapData(float[,] heightMap, Color[] colourMap)
        {
            this.heightMap = heightMap;
            this.colourMap = colourMap;
        }
    }
}
