using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using TMPro;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

public class BuildingBuilder : MonoBehaviour
{
    //UI stuff
    public int buildingCount;
    public int blockCount;
    public TextMeshProUGUI text;
    
    //Ray casting stuff
    [SerializeField] private Transform pointer;
    private Camera _cam;
    private Ray _ray;
    private Vector3 _snapPosition;

    //Drag start point
    private Vector3 _origin;

    //Grid parts
    private readonly List<Vector3> _gridList = new List<Vector3>();
    [SerializeField] private Transform grid;
    [SerializeField] private int gridStartSize = 3;
    [SerializeField] private float gridHeight = -0.5f;
    
    //Building parts
    private float _offset = 0.5f;
    [Header("Building 1")] [SerializeField]
    private Transform b1FirstFloor;

    [SerializeField] private Transform b1Door;
    [SerializeField] private Transform b1Window;

    [SerializeField] private Transform b1Roof;

    //Height of each column to be chosen by random
    [Range(3, 12)] [SerializeField] private int b1HeightMin, b1HeightMax;

    // I know I could've done some fancy inheritance/polymorphism but it's too late for that now

    [Header("Building 2")] [SerializeField]
    private Transform b2FirstFloor;

    [SerializeField] private Transform b2Door;
    [SerializeField] private Transform b2Window;
    [SerializeField] private Transform b2Roof;

    [Range(4, 12)]
    //Height of each column to be chosen by random
    [SerializeField]
    private int b2HeightMin, b2HeightMax;


    //Cube Vertex Thing
    private readonly Vector3[] _vertexPoints =
    {
        new (-1, +0.1f, -1),
        new (+1, +0.1f, -1),
        new (-1, +0.1f, +1),
        new (+1, +0.1f, +1),
        new (-1, +2.1f, -1),
        new (+1, +2.1f, -1),
        new (-1, +2.1f, +1),
        new (+1, +2.1f, +1)
    };

    //Mesh Thing
    private Mesh _mesh;

    //When cube generation is occurring
    private bool _cubeGen;

    private void Start()
    {
        //Set camera for ray casting
        _cam = Camera.main;
        
        //Set mesh for mesh meshing
        _mesh = GetComponent<MeshFilter>().mesh;
        
        //Grid Spawn
        if (gridStartSize % 2 == 0) //Hack to make sure grid is an odd number, aligns nicer to Unity's Grid
        {
            gridStartSize++; 
        }
        
        var gridOffset = -gridStartSize * 0.5f;
        
        for (var x = 0; x < gridStartSize; x++)
        {
            for (var z = 0; z < gridStartSize; z++)
            {
                var gridPosition = new Vector3(x + gridOffset, gridHeight, z + gridOffset);
                Instantiate(grid, gridPosition, Quaternion.identity);
                _gridList.Add(gridPosition);
            }
        }
        
    }
    
    private void Update()
    {
        _ray = _cam.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(_ray, out var hit, 100f);
        if (hit.transform.gameObject.layer != LayerMask.NameToLayer("DespawnBox"))
        {
            _snapPosition = new Vector3(Mathf.Round(hit.point.x), Mathf.Round(hit.point.y), Mathf.Round(hit.point.z));
        }

        pointer.transform.position = _snapPosition;
        
        //Check if lmb is down
        if (Input.GetMouseButton(0))
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Drag start
                // Set origin for cube to spawn
                
                _origin = pointer.transform.position;

                for (var i = 0; i < _vertexPoints.Length; i++)
                {
                    _vertexPoints[i] = new Vector3 (_origin.x, _origin.y+0.1f, _origin.z);
                }
                for (var i = 4; i < _vertexPoints.Length; i++)
                {
                    _vertexPoints[i].y = _origin.y + 1;
                }
            }

            //CreateCube();
            
            // Drag
            var position = pointer.transform.position;
            
            /*  Cube Vertex Index
             *     6---------7
             *    /|   ^ z  /|
             *   / |  /    / |
             *  4---------5  | --> x
             *  |  2------|--3
             *  | /       | /
             *  |/        |/
             *  0---------1
             * Look I know it breaks every convention under the sun but it *works!*
             */

            // Move rear right corner (+x, +z)
            if (position.x > _vertexPoints[3].x || position.x > _origin.x) // if pointer goes beyond the cube in x, move the +x vertices
                _vertexPoints[3].x = _vertexPoints[7].x = _vertexPoints[1].x = _vertexPoints[5].x = position.x;
            if (position.z > _vertexPoints[3].z || position.z > _origin.z) // if pointer goes beyond the cube in z, move the +z vertices
                _vertexPoints[3].z = _vertexPoints[7].z = _vertexPoints[2].z = _vertexPoints[6].z = position.z;

            //Move front left corner (-x, -z)
            if (position.x < _vertexPoints[0].x || position.x < _origin.x) // if pointer goes beyond the cube in -x, move the -x vertices
                _vertexPoints[0].x = _vertexPoints[4].x = _vertexPoints[2].x = _vertexPoints[6].x = position.x;
            if (position.z < _vertexPoints[4].z || position.z < _origin.z) // if pointer goes beyond the cube in -z, move the -z vertices
                _vertexPoints[0].z = _vertexPoints[4].z = _vertexPoints[1].z = _vertexPoints[5].z = position.z;

            MeshCompute(_mesh);
        }

        if (Input.GetMouseButtonUp(0))
        {
            _cubeGen = true;
        }
        while (_cubeGen)
        {
            //Build the building
            buildingCount++;
            if (blockCount < 100)
            {
                StartCoroutine(Building1(_vertexPoints[0], _vertexPoints[3]));
            }
            else
            {
                StartCoroutine(Random.Range(0, 2) == 1
                    ? Building1(_vertexPoints[0], _vertexPoints[3])
                    : Building2(_vertexPoints[0], _vertexPoints[3]));
            }

            _cubeGen = false;
            
            //Expand The Grid™
            GridGrow(_vertexPoints[0], _vertexPoints[3]);
            //reset vertex points
            for (var i = 0; i < _vertexPoints.Length; i++)
            {
                _vertexPoints[i] = _origin;
            }
            MeshCompute(_mesh);
        }
        text.text = "Building Count: " + buildingCount + "\nBlock Count: " + blockCount;
    }
    private IEnumerator Building1(Vector3 vertZero, Vector3 vertThree)
    {
        var height = Random.Range(b1HeightMin, b1HeightMax);
        for (var x = vertZero.x; x < vertThree.x; x++)
        {
            for (var z = vertZero.z; z < vertThree.z; z++)
            {
                for (var y = _origin.y; y < height; y++)
                {
                    //Ground Floor
                    if (Math.Abs(y - _origin.y) < .1f)
                    {
                        Instantiate(Random.Range(0, 2) == 1 ? b1FirstFloor : b1Door,
                            new Vector3(x + _offset, y, z + _offset), Quaternion.identity);
                    }
                    //Top Floor
                    else if (y >= height - 1)
                    {
                        Instantiate(b1Roof, new Vector3(x + _offset, y, z + _offset), Quaternion.identity);
                    }
                    //The Rest of the Building
                    else
                    {
                        Instantiate(b1Window, new Vector3(x + _offset, y, z + _offset), Quaternion.identity);
                    }
                    blockCount++;
                }
                yield return new WaitForSeconds(0.02f);
            }
        }
        yield return null;
    }

    private IEnumerator Building2(Vector3 vertZero, Vector3 vertThree)
    {
        for (var x = vertZero.x; x < vertThree.x; x++)
        {
            for (var z = vertZero.z; z < vertThree.z; z++)
            {
                var height = Random.Range(b2HeightMin, b2HeightMax);
                for (var y = _origin.y; y < height; y++)
                {
                    
                    //Ground Floor
                    if (Math.Abs(y - _origin.y) < .1f)
                    {
                        Instantiate(Random.Range(0, 2) == 1 ? b2FirstFloor : b2Door,
                            new Vector3(x + _offset, y, z + _offset), Quaternion.identity);
                    }
                    //Top Floor
                    else if (y >= height - 1)
                    {
                        Instantiate(b2Roof, new Vector3(x + _offset, y, z + _offset), Quaternion.identity);
                    }
                    //The Rest of the Building
                    else
                    {
                        Instantiate(b2Window, new Vector3(x + _offset, y, z + _offset), Quaternion.identity);
                    }
                    blockCount++;
                    yield return new WaitForSeconds(0.02f);
                }
            }
        }
        yield return null;
    }

    Vector3[] GetVertices()
    {
        Vector3[] vertices =
        {
            _vertexPoints[0], _vertexPoints[1], _vertexPoints[2], _vertexPoints[3], // 0, 1, 2, 3 Bottom
            _vertexPoints[0], _vertexPoints[2], _vertexPoints[4], _vertexPoints[6], // 4, 5, 6, 7 Left
            _vertexPoints[2], _vertexPoints[3], _vertexPoints[6], _vertexPoints[7], // 8, 9, 10, 11 Front
            _vertexPoints[0], _vertexPoints[1], _vertexPoints[4], _vertexPoints[5], // 12, 13, 14, 15 Back
            _vertexPoints[1], _vertexPoints[3], _vertexPoints[5], _vertexPoints[7], // 16, 17, 18, 19 Right
            _vertexPoints[4], _vertexPoints[5], _vertexPoints[6], _vertexPoints[7]  // 20, 21, 22, 23 Top
        };
        return vertices;
    }
    private int[] GetTriangles ()
    {
        int[] triangles =
        {
            // Bottom
            0, 3, 1,
            0, 2, 3,
            // Left
            5, 6, 4,
            5, 7, 6,
            // Front
            9, 10, 8,
            9, 11, 10,
            // Back
            12, 14, 13,
            14, 15, 13,
            // Right
            16, 18, 17,
            18, 19, 17,
            // Top
            20, 23, 21,
            20, 22, 23
        };
        return triangles;
    }
    private void MeshCompute(Mesh mesh)
    {
        mesh.vertices = GetVertices();
        mesh.triangles = GetTriangles();
        mesh.Optimize ();
        mesh.RecalculateNormals ();
        
    }

    private void GridGrow(Vector3 vertZero, Vector3 vertThree)
    {
        for (var x = vertZero.x - 2.5f; x < vertThree.x + 3.5f; x++)
        {
            for (var z = vertZero.z - 2.5f; z < vertThree.z + 3.5f; z++)
            {
                if (!_gridList.Contains(new Vector3(x, gridHeight, z)))
                {
                    var gridPosition = new Vector3(x, gridHeight, z);
                    _gridList.Add(gridPosition);
                    Instantiate(grid, gridPosition, Quaternion.identity);
                }
            }
        }
    }
}
