using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MapGenerator;

public class InfiniteTerrain : MonoBehaviour  
{

    const float scale = 2f;

    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrOfViewerThreshold = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
    public LODInfo[] detailLevels;
    public static float maxViewDistance;
    public Transform viewer;
    public static Vector2 viewerPosition;
    Vector2 viewerPositionOld;

    static MapGenerator mapGenerator;
    public Material mapMaterial;

    int chunkSize;
    int chunksVisibleInViewDistance;

    Dictionary<Vector2, TerrainChunk> chunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);
        UpdateVisibleChunks();
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / scale;
        if((viewerPositionOld-viewerPosition).sqrMagnitude > sqrOfViewerThreshold)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }   

    }

    void UpdateVisibleChunks()
    {
        for(int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for(int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                if(chunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    chunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                }
                else
                {
                    chunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
                }
            }
        }
    }

    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;
        
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;
        MapData mapData;
        bool mapDataReceived;
        int previousLODIndex = -1;
        //public ParticleSystem snowParticleEffect;
        public GameObject tree = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        public GameObject snowParticleEffect = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Vector3 pos;
        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material)
        {
            this.detailLevels = detailLevels;
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);
            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshObject.transform.position = positionV3 * scale;
            meshRenderer.material = material;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * scale;
            SetVisible(false);

            lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
            }
            mapGenerator.RequestMapData(position, OnMapDataReceived);


        }

        void OnMapDataReceived(MapData mapData)
        {
            this.mapData = mapData;
            mapDataReceived = true;
            Texture2D texture = TextureGenerator.TextureFromColourMap(mapData.colourMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
            meshRenderer.material.mainTexture = texture;
            UpdateTerrainChunk();
        }

        public void UpdateTerrainChunk()
        {
            GameObject extraStore = GameObject.Find("ExtraStore");
            if (mapDataReceived)
            {
                float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;

                if (visible)
                {
                    int lodIndex = 0;
                    for (int i = 0; i < detailLevels.Length - 1; i++)
                    {
                        if (viewerDistanceFromNearestEdge > detailLevels[i].visibleDistanceThreshold)
                        {
                            lodIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (lodIndex != previousLODIndex)
                    {
                        LODMesh lodMesh = lodMeshes[lodIndex];
                        if (lodMesh.hasMesh)
                        {
                            previousLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                        }
                        else if (!lodMesh.hasRequestedMesh)
                        {
                            lodMesh.RequestMesh(mapData);
                        }
                    }
                    terrainChunksVisibleLastUpdate.Add(this);


                   /*
                    for (int y = 0; y < mapChunkSize; y++)
                    {
                        for (int x = 0; x < mapChunkSize; x++)
                        {
                            float currentHeight = mapData.heightMap[x, y];
                        
                            if (currentHeight >= 0.6 && currentHeight <= 0.65)
                            {
                                float highestNoiseHeight = 0f;
                                if (x != 0 && y != 0 && x < mapChunkSize - 1 && y < mapChunkSize - 1)
                                {
                                    highestNoiseHeight = Mathf.Max(currentHeight, mapData.heightMap[x - 1, y - 1], mapData.heightMap[x, y + 1], mapData.heightMap[x, y - 1], mapData.heightMap[x + 1, y + 1]);
                                }

                                if (highestNoiseHeight == currentHeight)
                                {
                                    float heightToUse = currentHeight;

                                    float xToUse = x;
                                    float yToUse = y;
                                    pos = new Vector3((xToUse * 10) - (mapChunkSize * 5), (heightToUse * 100) - 30, (yToUse * 10) - (mapChunkSize * 5));
                                    pos = Vector3.Reflect(pos, Vector3.forward);

                                    Instantiate(tree, pos, new Quaternion(), meshObject.transform);

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
                                    highestNoiseHeight = Mathf.Max(currentHeight, mapData.heightMap[x - 1, y - 1], mapData.heightMap[x, y + 1], mapData.heightMap[x, y - 1], mapData.heightMap[x + 1, y + 1]);
                                }

                                if (highestNoiseHeight == currentHeight)
                                {
                                    float heightToUse = currentHeight;
                                    float xToUse = x;
                                    float yToUse = y;
                                    pos = new Vector3((xToUse * 10) - (mapChunkSize * 5), heightToUse * 100, (yToUse * 10) - (mapChunkSize * 5));
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
                
                    */

                //for (int i = 0; i < meshObject.transform.childCount; i++)
                //{
                //    GameObject.Destroy(meshObject.transform.GetChild(i).gameObject);
                //}
                SetVisible(visible);
            }
                
            }
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }

    class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;

        int lod;

        System.Action updateCallback;

        public LODMesh(int lod, System.Action updateCallback)
        {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;
            updateCallback();
        }

        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        public int lod;
        public float visibleDistanceThreshold;
    }
}
