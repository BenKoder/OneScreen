using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public GameObject cube;

    public int mapWidth, mapHeight;

    public float xOrg, yOrg;

    public float xOffset, yOffset;

    public float xScale = 1f, yScale = 1f, zScale = 1f;
    
    
    // Start is called before the first frame update
    void Start()
    {
        GenerateTerrain();
    }

    private void GenerateTerrain()
    {
        float y = 0.0f;

        while (y < mapHeight)
        {
            float x = 0.0f;
            while (x < mapWidth)
            {
                float xCoord = xOrg + x / mapWidth * xScale;
                float yCoord = yOrg + y / mapHeight * yScale;
                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                Instantiate(cube, new Vector3(x + xOffset, (int)Mathf.Round(sample * zScale), y + yOffset),Quaternion.identity);
                x++;
            }
            y++;
        }
    }
}
