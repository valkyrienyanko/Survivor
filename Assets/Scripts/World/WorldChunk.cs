using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WorldChunk : MonoBehaviour
{
    private Mesh mesh;
    private MeshRenderer meshRenderer;

    private Vector3[] vertices;
    private Vector2[] uvs;
    private int[] triangles;

    private readonly float cellSize = 0.25f;
    private readonly int chunkSize = 11;

    private GameObject prefabTree;
    private GameObject prefabRock;

    public void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        meshRenderer = GetComponent<MeshRenderer>();
        prefabTree = WorldGenerator.prefabTree;
        prefabRock = WorldGenerator.prefabRock;
    }

    public void Start()
    {
        meshRenderer.material = Resources.Load("Materials/Texture") as UnityEngine.Material;
    }

    public void Generate(int chunkX, int chunkZ)
    {
        // Discrete Procedural Grid (2 vertices are never shared)
        Vector3 renderOffset = new Vector3(chunkX * chunkSize * cellSize, 0, chunkZ * chunkSize * cellSize);
        
        vertices = new Vector3[chunkSize * chunkSize * 4];
        uvs = new Vector2[vertices.Length];
        triangles = new int[chunkSize * chunkSize * 6];

        int v = 0;
        int t = 0;

        float vertexOffset = cellSize * 0.5f; // Multiplication is not as intensive as division.

        for (int cellX = 0; cellX < chunkSize; cellX++)
        {
            for (int cellZ = 0; cellZ < chunkSize; cellZ++)
            {
                Vector3 cellOffset = new Vector3(cellX * cellSize, 0, cellZ * cellSize);

                vertices[v] = new Vector3(-vertexOffset, 0, -vertexOffset) + cellOffset + renderOffset;
                vertices[v + 1] = new Vector3(-vertexOffset, 0, vertexOffset) + cellOffset + renderOffset;
                vertices[v + 2] = new Vector3(vertexOffset, 0, -vertexOffset) + cellOffset + renderOffset;
                vertices[v + 3] = new Vector3(vertexOffset, 0, vertexOffset) + cellOffset + renderOffset;

                triangles[t] = v;
                triangles[t + 1] = triangles[t + 4] = v + 1;
                triangles[t + 2] = triangles[t + 3] = v + 2;
                triangles[t + 5] = v + 3;

                v += 4;
                t += 6;
            }
        }

        //uvs = SpriteLoader.Instance.GetTileUVs("Tex_1");

        UpdateMesh();

        InstantiateEntities(chunkX, chunkZ);
    }

    private void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        //Vector2[] texTest = SpriteLoader.Instance.GetTileUVs("Tex_1");
        Vector2[] tex1 = new Vector2[]
        {
            new Vector2(0.0f, 1.0f), //TopLeft
            new Vector2(0.0f, 0.5f), //BottomLeft
            new Vector2(0.5f, 1.0f), //TopRight
            new Vector2(0.5f, 0.5f)  //BottomRight
        };

        int k = 0;
        for (int g = 0; g < vertices.Length / 4; g++) {
            for (int i = 0; i < tex1.Length; i++)
            {
                uvs[i + k] = tex1[i];
            }
            k += 4;
        }

        mesh.uv = uvs;
        
        mesh.RecalculateNormals();
    }

    private void InstantiateEntities(int chunkX, int chunkZ)
    {
        int posX = chunkX * chunkSize;
        int posZ = chunkZ * chunkSize;

        for (int x = posX; x < posX + chunkSize; x++)
        {
            for (int z = posZ; z < posZ + chunkSize; z++)
            {
                if (Mathf.PerlinNoise(Random.Range(0f, 1f), Random.Range(0f, 1f)) < 0.3f)
                {
                    if (Random.Range(0f, 1f) < 0.5f)
                    {
                        GameObject go = Instantiate(prefabTree, new Vector3(x * cellSize, 0, z * cellSize), Quaternion.identity);
                        go.transform.parent = WorldGenerator.World.transform.Find("Trees");
                    }
                    else
                    {
                        GameObject go = Instantiate(prefabRock, new Vector3(x * cellSize, 0, z * cellSize), Quaternion.identity);
                        go.transform.parent = WorldGenerator.World.transform.Find("Rocks");
                    }
                }
            }
        }
    }
}