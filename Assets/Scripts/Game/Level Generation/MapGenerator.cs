using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(MeshGenerator))]
public class MapGenerator : MonoBehaviour
{
    static readonly int MAX_GIZMO_SIZE = 4000000;
    static readonly int MAX_ROOMCONNECT_ITERATIONS = 15;
    static readonly float MAXIMUM_NOISE_VALUE = 5f;



    public enum Generation_Method { FRACTAL, RANDOM }

    [Tooltip("Method of map generation")]
    public Generation_Method methodOfGeneration = Generation_Method.FRACTAL;

    //Generic Generation Variables
    [Tooltip("Total width of generated map")]
    [SerializeField]
    int generatedMapWidth;

    [Tooltip("Total height of generated map")]
    [SerializeField]
    int generatedMapHeight;

    [Tooltip("Amount to scale the generated map after creation")]
    [SerializeField]
    [Range(1, 10)]
    int scaleFactor = 1;

    [Tooltip("Elevation of a point based on Perlin-Noise value")]
    [SerializeField]
    AnimationCurve elevationCurve = AnimationCurve.Linear(0f, 0f, 5f, 10f);

    [Tooltip("Smoothing strength of Perlin-Noise value to average of neighboring values")]
    [SerializeField]
    [Range(0f, 1f)]
    float heightSmoothing = 0f;


    [SerializeField]
    [Range(0f, 1f)]
    float meshVariationStrength = 0f;


    [Tooltip("Amount of times to Apply low-poly effect")]
    [SerializeField]
    [Range(0, 5)]
    int lowPolyLevel = 0;

    [Tooltip("Size of island that will be generated and applied to the map")]
    [SerializeField]
    DeviatingInteger islandSize = new DeviatingInteger(30, 8);

    [Tooltip("Size of map border. Values within this border will be set to 0.")]
    [SerializeField]
    int borderSize = 10;

    [SerializeField]
    [Range(0f, 100f)]
    float noiseScale = 20;

    [Tooltip("Height threshold. All Perlin-Noise values below this threshold will be set to 0.")]
    [SerializeField]
    [Range(0f, 5f)]
    float heightThreshold = .35f;

    [Tooltip("Amount of time to apply Map Smoothing effect.")]
    [SerializeField]
    [Range(0, 5)]
    int mapSmoothingLevel = 2;


    [SerializeField]
    [Range(0, 10)]
    int fractalOctaves = 4;

    [Tooltip("Chance of applying a circle mask to generated island")]
    [SerializeField]
    [Range(0f, 1f)]
    float probabilityOfCircleMask = .5f;

    //Fractal/Recursive Generation
    //EditorGUILayout.LabelField("Inspector", EditorStyles.boldLabel);

    [Tooltip("Size decrease percentage for island size with each successive fractal generation. (ie, 0.75 = 75% of size")]
    [SerializeField]
    [Range(0f, 1f)]
    float fractalSizeFallOff = 0.75f;

    [Tooltip("Chance that fractal will decrease the number of successive fractals remaining")]
    [SerializeField]
    [Range(0f, 1f)]
    float fractalDecreaseChance = 0.5f;

    [Tooltip("Increase of fractal placement along generation vector with each successive fractal generation")]
    [SerializeField]
    int fractalAngleIncrease = 5;

    [Tooltip("Inital number of fractals to place")]
    [SerializeField]
    int initialFractalNumber = 5;

    [Tooltip("Percentage of fractal placement along generation vector in terms of island size")]
    [SerializeField]
    [Range(0f, 1f)]
    float fractalPlacementPercentage = 0.5f;

    //Random Island Generation
    [Tooltip("Number of islands to generate")]
    [SerializeField]
    [Range(1, 50)]
    int islandsToGenerate = 6;

    [Tooltip("Range at which islands start grouping with one another")]
    [SerializeField]
    [Range(0f, 10f)]
    float islandGroupingRange = 7f;

    [Tooltip("Strength of island grouping")]
    [SerializeField]
    [Range(0f, 1f)]
    float islandGroupingPower = 0.15f;


    //Rooms and Regions
    [Tooltip("Should generate additional islands to connect all rooms?")]
    [SerializeField]
    bool connectAllRooms = false;

    [Tooltip("Definition of room in terms of grid size")]
    [SerializeField]
    int roomSizeThreshold = 25;


    [Tooltip("Seed value to be used to initalize RNG")]
    [SerializeField]
    string seed;

    [Tooltip("Should use a random seed value?")]
    [SerializeField]
    bool useRandomSeed = false;

    [Tooltip("Should show debug info?")]
    [SerializeField]
    bool showDebug = false;


    //public enum GizmoStyle{ NONE, HEIGHTMAP, LANDMAP, SCALED_HEIGHTMAP, SCALED_LANDMAP, ROOMS_SMALL, ROOMS_LARGE };
    public enum GizmoStyle { NONE, LANDMAP, SCALED_LANDMAP, PREROOMCONNECTION };
    public GizmoStyle gizmoToShow;

    [Tooltip("Material to be applied to the ground mesh")]
    [SerializeField]
    Material groundMaterial;
    GameObject groundObject;

   
    System.Random pseudoRandom;
    

    struct MapNode
    {
        float value;
        bool isLand;
        bool isBaseMap;
        
        public MapNode(MapNode m) :this(m.Value, m.IsLand, m.IsBaseMap) { }
        public MapNode(float _val, bool _land, bool _isBase)
        {
            value = _val;
            isLand = _land;
            isBaseMap = _isBase;
        }

        public void Reset()
        {
            value = 0;
            isLand = false;
            isBaseMap = false;
        }


        public float Value
        {
            get { return value; }
            set { this.value = value; }
        }
        public bool IsLand
        {
            get { return isLand; }
            set { isLand = value; }
        }
        public bool IsBaseMap
        {
            get { return isBaseMap; }
            set { isBaseMap = value; }
        }
    }
    MapNode[][] unscaledMap;
    MapNode[][] scaledMap;




   [HideInInspector]
    public static MapGenerator Instance { get; private set; }
    void Awake()
    {
        Instance = this;
    }





    public void EditorGenerateMap()
    {
        StopAllCoroutines();
        StartCoroutine(GenerateMap());
    }

    public IEnumerator GenerateMap()
    {
        float _time;

        //Use current seed or random seed
        if (useRandomSeed)
        {
            seed = System.DateTime.Now.Ticks.ToString();
        }

        pseudoRandom = new System.Random(seed.GetHashCode());



        //Set threshold for minimum room size
        roomSizeThreshold = islandSize.Mean - (5 * islandSize.Sigma);
        roomSizeThreshold = Mathf.Max(1, roomSizeThreshold);



        //Zero the heightMap
        if (unscaledMap == null || unscaledMap.Length != generatedMapWidth || unscaledMap[0].Length != generatedMapHeight)
        {
            unscaledMap = new MapNode[generatedMapWidth][];
            for (int i = 0; i < generatedMapWidth; i++)
            {
                unscaledMap[i] = new MapNode[generatedMapHeight];

                for (int k = 0; k < generatedMapHeight; k++)
                {
                    unscaledMap[i][k] = new MapNode();
                }
            }
        }
        else
        {
            for (int i = 0; i < unscaledMap.Length; i++)
            {
                for (int j = 0; j < unscaledMap[0].Length; j++)
                {
                    unscaledMap[i][j].Reset();
                }
            }
        }


        if (UIManager.Instance != null)
            UIManager.Instance.InflateLoadingScreen("Generating ground");


        _time = Time.realtimeSinceStartup;


        //Generate Aggregate Heightmap
        switch (methodOfGeneration)
        {
            case Generation_Method.FRACTAL:
                //Choose center island size
                Vector2 islandSize = GetGaussianIslandDimensions() * 2f;

                //Generate center island & apply
                float[][] regionMap = GenerateRegion(true, (int)islandSize.x, (int)islandSize.y, fractalOctaves);
                ApplyRegion(generatedMapWidth / 2, generatedMapHeight / 2, regionMap);

                yield return StartCoroutine(GenerateFractaledRegion(generatedMapWidth / 2, generatedMapHeight / 2, Vector2.zero, initialFractalNumber, 0));
                break;
            case Generation_Method.RANDOM:
                RandomIslandGeneration();
                break;
        }

        if (showDebug)
        {
            Debug.Log("MapGenerator --- GenerateMap -- Map generation time " + (Time.realtimeSinceStartup - _time).ToString() + " seconds");
        }


        if (UIManager.Instance != null)
            UIManager.Instance.InflateLoadingScreen("Processing ground");


        _time = Time.realtimeSinceStartup;

        //Smooth Map structure
        yield return StartCoroutine(ApplyMapSmoothing());

        if (showDebug)
        {
            Debug.Log("MapGenerator --- GenerateMap -- Map smoothing time " + (Time.realtimeSinceStartup - _time).ToString() + " seconds");
        }



        for (int i = 0; i < unscaledMap.Length; i++)
        {
            for(int k = 0; k < unscaledMap[i].Length; k++)
            {
                unscaledMap[i][k].IsLand = unscaledMap[i][k].Value >= heightThreshold;
                unscaledMap[i][k].IsBaseMap = unscaledMap[i][k].IsLand;
            }
        }



        _time = Time.realtimeSinceStartup;

        //Connect all regions of roomSizeThreshold or greater
        if (connectAllRooms)
        {
            yield return StartCoroutine(ConnectRooms());

            yield return StartCoroutine(ApplyMapSmoothing());
        }

        if (showDebug)
        {
            Debug.Log("MapGenerator --- GenerateMap -- Room connection time " + (Time.realtimeSinceStartup - _time).ToString() + " seconds");
        }



        //Create elevations for heightMap
        ElevateHeightMap();



        //Smooth heightMap elevations
        yield return StartCoroutine(SmoothHeightMap());

        

        if (UIManager.Instance != null)
            UIManager.Instance.InflateLoadingScreen("Creating ground mesh");




        bool[][] _landmap = LandMap_Unscaled;
        float[][] _heightmap = HeightMap_Unscaled;


        _time = Time.realtimeSinceStartup;

        //Create Mesh
        CreateGroundObject();
        MeshGenerator.Instance.GenerateMesh(groundObject, groundMaterial, ref _landmap, scaleFactor, meshVariationStrength, ref _heightmap, MeshGenerator.ColliderAdditionType.MeshColliderAndWall);

        if (showDebug)
        {
            Debug.Log("MapGenerator --- GenerateMap -- Mesh creation time " + (Time.realtimeSinceStartup - _time).ToString() + " seconds");
        }



        MeshFilter _filter = groundObject.GetComponent<MeshFilter>();
        for (int i = 0; i < lowPolyLevel; i++)
        {
            Utilities.MakeMeshLowPoly(_filter);
        }




        ConstructScaledMap();

        groundObject.SetActive(true);


        if (UIManager.Instance != null)
            UIManager.Instance.DeflateAll();
    }


    void CreateGroundObject()
    {

        if (groundObject != null)
        {
            DestroyImmediate(groundObject);
            groundObject = null;
        }


        groundObject = new GameObject("Ground");

        //ColorizeMesh();

        groundObject.layer = LayerMask.NameToLayer("Ground");
        groundObject.tag = "Ground";
    }







    void ElevateHeightMap()
    {
        for (int i = 0; i < unscaledMap.Length; i++)
        {
            for (int j = 0; j < unscaledMap[i].Length; j++)
            {
                unscaledMap[i][j].Value = unscaledMap[i][j].Value >= heightThreshold ? elevationCurve.Evaluate(unscaledMap[i][j].Value) : 0f;
            }
        }
    }


    void ApplyMeshVariation()
    {
        MeshFilter _filter = groundObject.GetComponent<MeshFilter>();

        if (_filter == null)
            return;

        Mesh _mesh = _filter.mesh;

        Vector3[] _vertices = _mesh.vertices;
        for (int i = 0; i < _vertices.Length; i++)
        {
            Vector2 variationVector = GetRandomInCircle() * scaleFactor * meshVariationStrength;
            _vertices[i] = transform.position + _vertices[i] + new Vector3(variationVector.x, 0, variationVector.y);
        }


        _mesh.vertices = _vertices;
        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();

        MeshCollider _coll = groundObject.GetComponent<MeshCollider>();
        _coll.sharedMesh = _mesh;
    }



    /*
    IEnumerator ApplyLowPoly()
    {
        for (int i = 0; i < lowPolyLevel; i++)
            yield return StartCoroutine(LowPolyRoutine());
    }

    IEnumerator LowPolyRoutine()
    {
        if ((heightMap.Length / 2) > 3 && (heightMap[0].Length / 2) > 3)
        {
            int width= 0;
            int height = 0;

            for(int i = 0; i < heightMap.Length; i += 2)
            {
                width++;
            }

            for (int i = 0; i < heightMap[0].Length; i += 2)
            {
                height++;
            }



            float[][] newHeightMap = new float[width][];
            int[][] newLandMap = new int[width][];
            for (int i = 0; i < heightMap.Length; i += 2)
            {
                if (i % 100 == 0)
                    yield return null;


                newHeightMap[i / 2] = new float[height];
                newLandMap[i / 2] = new int[height];
                for (int k = 0; k < heightMap[0].Length; k += 2)
                {
                    newHeightMap[i / 2][k / 2] = heightMap[i][k];
                    newLandMap[i / 2][k / 2] = landMap[i][k];
                }
            }

            /*
            for(i -=1; i < heightMap.Length; i++)
            {
                for(k = 0; )
            }
            *


            heightMap = newHeightMap;
            landMap = newLandMap;
        }
    }
*/


    IEnumerator ApplyMapSmoothing()
    {
        for (int i = 0; i < mapSmoothingLevel; i++)
        {
            yield return SmoothMapRoutine();
        }
    }

    IEnumerator SmoothMapRoutine()
    {
        for (int i = 0; i < unscaledMap.Length; i++)
        {

            if (i % 50 == 0)
                yield return null;



            for (int j = 0; j < unscaledMap[0].Length; j++)
            {
               

                List<Coord> neighboring = GetValidNeighboringTiles(ref unscaledMap, i, j, 1);

                if (neighboring.Count > 4)
                {
                    unscaledMap[i][j].IsLand = true;
                }
                else if (neighboring.Count < 4)
                {
                    unscaledMap[i][j].IsLand = false;
                }

            }
        }
    }


    IEnumerator SmoothHeightMap()
    {
        for (int k = 0; k < mapSmoothingLevel; k++)
        {
            for (int i = 0; i < unscaledMap.Length; i++)
            {

                if (i % 100 == 0)
                    yield return null;


                for (int j = 0; j < unscaledMap[0].Length; j++)
                {

                    List<Coord> neighboring = GetValidNeighboringTiles(ref unscaledMap, i, j, 1);

                    if (neighboring.Count > 4)
                    {

                        float avgVal = 0;
                        for (int a = 0; a < neighboring.Count; a++)
                        {
                            avgVal += unscaledMap[neighboring[a].tileX][neighboring[a].tileY].Value;
                        }

                        avgVal += unscaledMap[i][j].Value;
                        avgVal /= neighboring.Count + 1;

                        float newVal = unscaledMap[i][j].Value + ((avgVal - unscaledMap[i][j].Value) * heightSmoothing);

                        unscaledMap[i][j].Value = newVal; // avgVal / (neighboring.Count + 1);

                    }
                    else if (neighboring.Count < 4)
                    {
                        unscaledMap[i][j].Value = 0;
                    }
                }
            }
        }
    }







    Vector2 GetGaussianIslandDimensions()
    {
        int islandWidth = (int)GetRandomGaussian(islandSize);
        int islandHeight = (int)GetRandomGaussian(islandSize);

        //Keep island within reasonable bounds
        islandWidth = Math.Max(1, islandWidth);
        islandHeight = Math.Max(1, islandHeight);

        islandWidth = Math.Min(islandWidth, islandSize.Mean + (5 * islandSize.Sigma));
        islandHeight = Math.Min(islandHeight, islandSize.Mean + (5 * islandSize.Sigma));

        return new Vector2(islandWidth, islandHeight);
    }

    float[][] GenerateRegion(int regionWidth, int regionHeight, int numFractalOctaves, float _threshold, int smoothLevel)
    {
        return GenerateRegion(regionWidth, regionHeight, numFractalOctaves, _threshold, smoothLevel, 1);
    }
    float[][] GenerateRegion(int regionWidth, int regionHeight, int numFractalOctaves, float _threshold, int smoothLevel, int _scale)
    {
        float[][] regionMap = GeneratePerlinNoise(regionWidth, regionHeight, numFractalOctaves);
        SmoothRegion(ref regionMap, _threshold, smoothLevel);

        int _width = regionMap.Length * _scale;
        int _height = regionMap[0].Length * _scale;

        float[][] returnMap = new float[_width][];

        for (int i = 0; i < _width; i++)
        {
            returnMap[i] = new float[_height];

            for (int j = 0; j < _height; j++)
            {

                int x = (int)((i / (float)_width) * regionMap.Length);
                int y = (int)((j / (float)_height) * regionMap[0].Length);

                returnMap[i][j] = regionMap[x][y];
            }
        }

        return returnMap;
    }
    float[][] GenerateRegion(bool useCircleMask, int regionWidth, int regionHeight, int numFractalOctaves)
    {
        if (regionWidth == 0)
            regionWidth = 1;

        if (regionHeight == 0)
            regionHeight = 0;


        float[][] regionMap = GeneratePerlinNoise(regionWidth, regionHeight, numFractalOctaves);

        if (useCircleMask)
        {

            float circleRadius = (regionWidth + regionHeight) / 2;
            circleRadius *= 0.5f;

            regionMap = ApplyCircleMask(regionMap, circleRadius);
        }
        else
        {
            regionMap = ApplySquareMask(regionMap, regionWidth, regionHeight);
        }

        return regionMap;
    }

    public static void SmoothRegion(ref float[][] regionMap, float _threshold, int smoothLevel)
    {


        for (int a = 0; a < smoothLevel; a++)
        {
            for (int i = 0; i < regionMap.Length; i++)
            {
                for (int j = 0; j < regionMap[0].Length; j++)
                {

                    int numNeighboring = 0;

                    for (int p = i - 1; p <= i + 1; p++)
                    {
                        for (int q = j - 1; q <= j + 1; q++)
                        {
                            if (p >= 0 && p < regionMap.Length && q >= 0 && q < regionMap[0].Length && !(p == i && q == j))
                            {

                                if (regionMap[p][q] >= _threshold)
                                    numNeighboring++;

                            }
                        }
                    }

                    if (numNeighboring < 4)
                    {
                        regionMap[i][j] = 0;
                    }
                }
            }
        }
    }
    public static void SmoothRegion(ref int[][] regionMap, int smoothLevel)
    {
        for (int a = 0; a < smoothLevel; a++)
        {

            for (int i = 0; i < regionMap.Length; i++)
            {
                for (int j = 0; j < regionMap[0].Length; j++)
                {

                    int numNeighboring = 0;

                    for (int p = i - 1; p <= i + 1; p++)
                    {
                        for (int q = j - 1; q <= j + 1; q++)
                        {
                            if (p >= 0 && p < regionMap.Length && q >= 0 && q < regionMap[0].Length && !(p == i && q == j))
                            {

                                if (regionMap[p][q] > 0)
                                    numNeighboring++;

                            }
                        }
                    }

                    if (numNeighboring < 4)
                    {
                        //Debug.Log("Found smoothing point");
                        regionMap[i][j] = 0;
                    }
                }
            }


        }
    }
    public static void SmoothRegion(ref bool[][] regionMap, int smoothLevel)
    {
        for (int a = 0; a < smoothLevel; a++)
        {

            for (int i = 0; i < regionMap.Length; i++)
            {
                for (int j = 0; j < regionMap[0].Length; j++)
                {

                    int numNeighboring = 0;

                    for (int p = i - 1; p <= i + 1; p++)
                    {
                        for (int q = j - 1; q <= j + 1; q++)
                        {
                            if (p >= 0 && p < regionMap.Length && q >= 0 && q < regionMap[0].Length && !(p == i && q == j))
                            {

                                if (regionMap[p][q])
                                    numNeighboring++;

                            }
                        }
                    }

                    if (numNeighboring < 4)
                    {
                        regionMap[i][j] = false;
                    }
                }
            }


        }
    }

    public static void RestrictRoomSize(ref bool[][] regionMap, int minimumRoomSize)
    {
        minimumRoomSize = Mathf.Clamp(minimumRoomSize, 1, minimumRoomSize);
        List<Room> _rooms = GetRooms(ref regionMap);

        for(int i = 0; i < _rooms.Count; i++)
        {
            if(_rooms[i].RoomSize < minimumRoomSize)
            {
               // Debug.Log("REMOVING room of size: " + _rooms[i].RoomSize);
                RemoveRoom(ref regionMap, _rooms[i]);
            }
            else
            {
                //Debug.Log("KEEPING room of size: " + _rooms[i].RoomSize);
            }
        }
    }
    private static void RemoveRoom(ref bool[][] regionMap, Room _room)
    {
        List<Coord> roomCoords = _room.roomCoords;

        for (int i = 0; i < roomCoords.Count; i++)
        {
            if (roomCoords[i].tileX < 0 || roomCoords[i].tileX >= regionMap.Length)
                continue;

            if (roomCoords[i].tileY < 0 || roomCoords[i].tileY >= regionMap[0].Length)
                continue;

            regionMap[roomCoords[i].tileX][roomCoords[i].tileY] = false;
        }
    }


    void ConstructScaledMap()
    {

        if (unscaledMap == null)
            return;

        int sizeX = (int)(unscaledMap.Length * scaleFactor);
        int sizeY = (int)(unscaledMap[0].Length * scaleFactor);

        if (scaledMap == null || scaledMap.Length != sizeX || scaledMap[0].Length != sizeY)
        {
            scaledMap = new MapNode[sizeX][];
            for (int i = 0; i < sizeX; i++)
            {
                scaledMap[i] = new MapNode[sizeY];
            }
            
        }
        


        for (int x = 0; x < sizeX; x++)
        {
            
            for (int y = 0; y < sizeY; y++)
            {

                int baseX = (int)((x / (float)sizeX) * unscaledMap.Length);
                int baseY = (int)((y / (float)sizeY) * unscaledMap[0].Length);

                scaledMap[x][y] = new MapNode(unscaledMap[baseX][baseY]);
            }
        }
    }

    #region Fractal Map Generation

    IEnumerator GenerateFractaledRegion(int x, int y, Vector2 dir, int numToGenerate, int generation)
    {

        for (int i = 0; i < numToGenerate; i++)
        {
            yield return null;

            Vector2 newIslandDir = GetRandomInCircle().normalized;
            int minAngle = 45 + (fractalAngleIncrease * generation);

            if (minAngle >= 90)
                break;

            while (dir.magnitude != 0 && Mathf.Abs(Vector2.Angle(dir, newIslandDir)) <= minAngle)
            {
                newIslandDir = GetRandomInCircle().normalized;
            }


            //Choose island size
            Vector2 islandSize = GetGaussianIslandDimensions() * Mathf.Pow(fractalSizeFallOff, generation);

            if ((int)islandSize.x <= 1 || (int)islandSize.y <= 1)
                break;



            newIslandDir *= Mathf.Max(islandSize.x, islandSize.y) * (fractalPlacementPercentage * 1.5f);

            int posX = (int)(x + newIslandDir.x);
            int posY = (int)(y + newIslandDir.y);

            if (!IsInMapRange(posX, posY))
                continue;

            bool useMask = pseudoRandom.NextDouble() <= probabilityOfCircleMask ? true : false;
            float[][] regionMap = GenerateRegion(useMask, (int)islandSize.x, (int)islandSize.y, fractalOctaves);

            if (showDebug)
            {
                Vector3 posA = transform.position + new Vector3((-generatedMapWidth / 2f + x) * scaleFactor, elevationCurve.Evaluate(unscaledMap[x][y].Value) + 0.25f, (-generatedMapHeight / 2f + y) * scaleFactor); //new Vector3(x - (generatedMapWidth/2), heightMap[x][y] * elevationMultiplier, y - (generatedMapHeight/2)) * scaleFactor;
                Vector3 posB = transform.position + new Vector3((-generatedMapWidth / 2f + posX) * scaleFactor, elevationCurve.Evaluate(unscaledMap[x][y].Value) + 0.25f, (-generatedMapHeight / 2f + posY) * scaleFactor); //new Vector3(posX - (generatedMapWidth/2), heightMap[posX][posY] * elevationMultiplier, posY - (generatedMapHeight/2)) * scaleFactor;

                Debug.DrawLine(posA, posB, Color.red, 8f);
            }


            ApplyRegion(posX, posY, regionMap);

            int newNum = pseudoRandom.NextDouble() < fractalDecreaseChance ? numToGenerate - 1 : numToGenerate;
            yield return GenerateFractaledRegion(posX, posY, newIslandDir.normalized, newNum, generation + 1);
        }
    }

    #endregion


    #region Random Island Generation

    void RandomIslandGeneration()
    {
        //Generate islands
        List<Vector2> islandPoints = new List<Vector2>();

        for (int i = 0; i < islandsToGenerate; i++)
        {

            //Choose island size and position
            Vector2 generatedIslandSize = GetGaussianIslandDimensions();
            //int islandWidth = (int)GetRandomGaussian(meanIslandSize, sigmaIslandSize);
            //int islandHeight = (int)GetRandomGaussian(meanIslandSize, sigmaIslandSize);

            bool useMask = pseudoRandom.NextDouble() <= probabilityOfCircleMask ? true : false;
            float[][] regionMap = GenerateRegion(useMask, (int)generatedIslandSize.x, (int)generatedIslandSize.y, fractalOctaves);


            Vector2 randCircle = GetRandomInCircle() * (Mathf.Max(generatedMapWidth, generatedMapHeight) / (float)2);

            int posX = (int)((unscaledMap.Length * 0.5f) + randCircle.x);
            int posY = (int)((unscaledMap[0].Length * 0.5f) + randCircle.y);

            Vector2 islandPosition = new Vector2(posX, posY);

            //Group islands
            for (int z = 0; z < islandPoints.Count; z++)
            {
                Vector2 displacementVector = islandPoints[z] - islandPosition;

                if ((displacementVector.magnitude <= islandSize.Mean * islandGroupingRange) && (displacementVector.magnitude >= islandSize.Mean / 2))
                {
                    islandPosition += (displacementVector * islandGroupingPower);
                }
            }

            islandPoints.Add(islandPosition);

            for (int a = 0; a < regionMap.Length; a++)
            {
                for (int b = 0; b < regionMap[0].Length; b++)
                {
                    int x = (int)islandPosition.x - (regionMap.Length / 2) + a;
                    int y = (int)islandPosition.y - (regionMap[0].Length / 2) + b;

                    if (x >= 0 && x < unscaledMap.Length && y >= 0 && y < unscaledMap[0].Length)
                    {
                        unscaledMap[x][y].Value += regionMap[a][b];
                        unscaledMap[x][y].IsLand = unscaledMap[x][y].Value >= heightThreshold;
                    }
                }
            }
        }
    }

    #endregion


    #region Rooms & Regions

    IEnumerator ConnectRooms()
    {
        List<Room> roomRegions;// = GetRoomsOfTypeThresholded (1);
        List<Room> survivingRooms = new List<Room>();
        int counter = 0;


        while (counter < MAX_ROOMCONNECT_ITERATIONS && (roomRegions = GetRoomsOfTypeThresholded(true)).Count > 1)
        {
           // yield return null;

            survivingRooms.Clear();

            for (int i = 0; i < roomRegions.Count; i++)
            {
                if (roomRegions[i].RoomSize >= roomSizeThreshold)
                    survivingRooms.Add(roomRegions[i]);
            }

            //UnityEngine.Debug.Log("Surviving Rooms size : " + survivingRooms.Count);

            yield return StartCoroutine(ConnectClosestRooms(survivingRooms));
            
            yield return StartCoroutine(ApplyMapSmoothing());

            counter++;
        }
    }



    IEnumerator ConnectClosestRooms(List<Room> allRooms)
    {

        //UnityEngine.Debug.Log("ConnectClosestRooms --- Number of Rooms: " + allRooms.Count);
        //yield return new WaitForSeconds(0f);


        float bestDistance;
        int bestTileA = 0;
        int bestTileB = 0;
        Room bestRoomA = null;
        Room bestRoomB = null;
        bool possibleConnectionFound = false;


        for (int p = 0; p < allRooms.Count; p++)
        {

            //UnityEngine.Debug.Log("ConnectClosestRooms --- Room " + p + " size: " + allRooms[p].roomCoords.Count);
            yield return null;


            bestDistance = float.MaxValue;
            possibleConnectionFound = false;

            for (int q = p + 1; q < allRooms.Count; q++)
            {
                if (p == q)
                    continue;

                //if(allRooms[p].IsConnected(allRooms[q]))
                //break;

               // yield return null;

                for (int tileIndexA = 0; tileIndexA < allRooms[p].roomCoords.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < allRooms[q].roomCoords.Count; tileIndexB++)
                    {
                        Coord tileA = allRooms[p].roomCoords[tileIndexA];
                        Coord tileB = allRooms[q].roomCoords[tileIndexB];
                        float distanceBetweenRooms = Mathf.Sqrt((Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2)));

                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileIndexA;
                            bestTileB = tileIndexB;
                            bestRoomA = allRooms[p];
                            bestRoomB = allRooms[q];
                        }
                    }
                }
            }

            if (possibleConnectionFound && bestRoomA != null && bestRoomB != null)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }
    }

    void CreatePassage(Room roomA, Room roomB, int tileAIndex, int tileBIndex)
    {
        //Room.ConnectRooms (roomA, roomB);

        Coord tileA = roomA.roomCoords[tileAIndex];
        Coord tileB = roomB.roomCoords[tileBIndex];

        //Debug.DrawLine (CoordToWorldPoint (tileA), CoordToWorldPoint (tileB), Color.green, 100);

        Vector2 disparityVector = new Vector2(tileB.tileX - tileA.tileX, tileB.tileY - tileA.tileY);

        //Debug.Log(disparityVector.magnitude.ToString());
        Vector2 generatedIslandSize = GetGaussianIslandDimensions() * ((disparityVector.magnitude + islandSize.Sigma) / islandSize.Mean);
        float[][] regionMap = GenerateRegion(true, (int)generatedIslandSize.x, (int)generatedIslandSize.y, fractalOctaves);

        int newX = tileA.tileX + (int)disparityVector.x;
        int newY = tileA.tileY + (int)disparityVector.y;

        ApplyRegion(newX, newY, regionMap);
    }

    void RandomFill(int x, int y, int spread, float baseDelta, float fallOff)
    {
        int count = 0;
        float newDelta = baseDelta * Mathf.Pow(fallOff, count);

        //Ensure limited spread
        while (count <= spread)
        {

            newDelta = baseDelta * Mathf.Pow(fallOff, count);

            for (int p = x - count; p <= x + count; p++)
            {
                for (int q = y - count; q <= y + count; q++)
                {

                    //Stick to the edges
                    if ((p > x - count || p < x + count) && (q > y - count || q < y + count))
                        continue;

                    //Is outside of grid bounds?
                    if (IsInMapRange(p, q) && pseudoRandom.NextDouble() < newDelta)
                    {
                        unscaledMap[p][q].IsLand = true;
                    }
                }
            }

            count++;
        }
    }




    public float WorldPointToElevation(Vector3 worldPos)
    {
        Vector3 gridOrigin = transform.position + (new Vector3(-generatedMapWidth / 2, 0, -generatedMapHeight / 2) * ScaleFactor);

        float percentX = (worldPos.x - gridOrigin.x) / scaledMap.Length;
        float percentY = (worldPos.y - gridOrigin.y) / scaledMap[0].Length;

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((scaledMap.Length - 1) * percentX);
        int y = Mathf.RoundToInt((scaledMap[0].Length - 1) * percentY);


        if (IsInMapRange_Scaled(x, y))
            return scaledMap[x][y].Value;

        Debug.Log("HeightMap : " + unscaledMap.Length + "x" + unscaledMap[0].Length + " #### " + x + "-" + y);
        return 0f;
    }
    public Coord WorldPointToCoord(Vector3 worldPos)
    {

        if (scaledMap == null)
            throw new UnityException("ScaledLandMap not implemented");





        Vector3 gridOrigin = transform.position + new Vector3(-generatedMapWidth / 2, 0, -generatedMapHeight / 2);

        float percentX = (worldPos.x - gridOrigin.x) / scaledMap.Length;
        float percentY = (worldPos.y - gridOrigin.y) / scaledMap[0].Length;

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((scaledMap.Length - 1) * percentX);
        int y = Mathf.RoundToInt((scaledMap[0].Length - 1) * percentY);

        return new Coord(x, y);
    }

    public Vector3 CoordToWorldPoint(Coord tile)
    {
        return IndexToWorldPoint(tile.tileX, tile.tileY);
    }

    Vector3 IndexToWorldPoint_Unscaled(int x, int y)
    {
        float height = 0;

        if (IsInMapRange(x, y))
            height = unscaledMap[x][y].Value;

        return transform.position + new Vector3((-generatedMapWidth / 2f) + x + 0.5f, height, (-generatedMapHeight / 2f) + y + 0.5f);
    }
    Vector3 IndexToWorldPoint(int x, int y)
    {
        float height = 0;

        if (IsInMapRange_Scaled(x, y))
            height = scaledMap[x][y].Value;

        return transform.position + new Vector3((-generatedMapWidth  * scaleFactor / 2f) + x + 0.5f, height, (-generatedMapHeight * scaleFactor / 2f) + y + 0.5f);
    }

    Room GetLargestRoom()
    {
        List<Room> roomRegions = GetRoomsOfTypeThresholded(true);

        //UnityEngine.Debug.Log("Room region size -- "  + roomRegions.Count);


        if (roomRegions.Count == 0)
            return null;

        Room largestRoom = roomRegions[0];

        for (int i = 1; i < roomRegions.Count; i++)
        {
            if (roomRegions[i].RoomSize > largestRoom.RoomSize)
            {
                largestRoom = roomRegions[i];
            }
        }

        return largestRoom;
    }

    List<Room> GetRoomsOfTypeThresholded(bool isLand)
    {
        List<Room> unthresholdedRooms = GetRoomsOfType(isLand);
        List<Room> thresholdedRooms = new List<Room>();

        //UnityEngine.Debug.Log("Unthresholded room count -- " + unthresholdedRooms.Count);

        for (int i = 0; i < unthresholdedRooms.Count; i++)
        {
            if (unthresholdedRooms[i].RoomSize >= roomSizeThreshold)
                thresholdedRooms.Add(unthresholdedRooms[i]);
        }

        return thresholdedRooms;
    }

    List<Room> GetRoomsOfType(bool isLand)
    {
        List<Room> rooms = new List<Room>();
        bool[][] mapFlags = new bool[generatedMapWidth][];
        for (int i = 0; i < mapFlags.Length; i++)
        {
            mapFlags[i] = new bool[generatedMapHeight];
        }

        for (int x = 0; x < mapFlags.Length; x++)
        {
            for (int y = 0; y < mapFlags[0].Length; y++)
            {
                if (!mapFlags[x][y] && unscaledMap[x][y].IsLand == isLand)
                {
                    Room newRoom = GetRoom(x, y);
                    rooms.Add(newRoom);

                    //UnityEngine.Debug.Log("Room size: " + newRoom.roomCoords.Count);

                    List<Coord> roomCoords = newRoom.roomCoords;
                    for (int i = 0; i < roomCoords.Count; i++)
                    {
                        mapFlags[roomCoords[i].tileX][roomCoords[i].tileY] = true;
                    }
                }
            }
        }

        return rooms;
    }


    public Room GetRoom(int startX, int startY)
    {

        List<Coord> tiles = new List<Coord>();

        int[][] mapFlags = new int[generatedMapWidth][];
        for (int i = 0; i < mapFlags.Length; i++)
        {
            mapFlags[i] = new int[generatedMapHeight];
        }

        bool tileType = unscaledMap[startX][startY].IsLand;

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX][startY] = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (IsInMapRange(x, y) && mapFlags[x][y] == 0 && unscaledMap[x][y].IsLand == tileType)
                    {
                        mapFlags[x][y] = 1;
                        queue.Enqueue(new Coord(x, y));
                    }
                }
            }
        }

        //Debug.Log(tiles.Count.ToString());
        return new Room(tiles);
    }

    bool IsInMapRange(int x, int y)
    {
        if (x >= 0 && x < generatedMapWidth && y >= 0 && y < generatedMapHeight)
            return true;

        return false;
    }






    public Room GetLargestRoom_Scaled()
    {
        List<Room> roomRegions = GetRoomsOfTypeThresholded_Scaled(true);

        if (roomRegions.Count == 0)
            return null;

        Room largestRoom = roomRegions[0];

        for (int i = 1; i < roomRegions.Count; i++)
        {
            if (roomRegions[i].RoomSize > largestRoom.RoomSize)
            {
                largestRoom = roomRegions[i];
            }
        }

        return largestRoom;
    }

    List<Room> GetRoomsOfTypeThresholded_Scaled(bool tileType)
    {
        List<Room> unthresholdedRooms = GetRoomsOfType_Scaled(tileType);
        List<Room> thresholdedRooms = new List<Room>();

        for (int i = 0; i < unthresholdedRooms.Count; i++)
        {
            if (unthresholdedRooms[i].RoomSize >= roomSizeThreshold * scaleFactor)
                thresholdedRooms.Add(unthresholdedRooms[i]);
        }

        return thresholdedRooms;
    }

    List<Room> GetRoomsOfType_Scaled(bool isLand)
    {
        List<Room> rooms = new List<Room>();
        int[][] mapFlags = new int[scaledMap.Length][];
        for (int i = 0; i < mapFlags.Length; i++)
        {
            mapFlags[i] = new int[scaledMap[0].Length];
        }

        for (int x = 0; x < scaledMap.Length; x++)
        {
            for (int y = 0; y < scaledMap[0].Length; y++)
            {
                if (mapFlags[x][y] == 0 && scaledMap[x][y].IsLand == isLand)
                {
                    Room newRoom = GetRoom_Scaled(x, y);
                    rooms.Add(newRoom);

                    List<Coord> roomCoords = newRoom.roomCoords;
                    for (int i = 0; i < roomCoords.Count; i++)
                    {
                        mapFlags[roomCoords[i].tileX][roomCoords[i].tileY] = 1;
                    }
                }
            }
        }

        return rooms;
    }

    Room GetRoom_Scaled(int startX, int startY)
    {
        List<Coord> tiles = new List<Coord>();

        int[][] mapFlags = new int[scaledMap.Length][];
        for (int i = 0; i < mapFlags.Length; i++)
        {
            mapFlags[i] = new int[scaledMap[0].Length];
        }

        bool tileType = scaledMap[startX][startY].IsLand;

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX][startY] = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (IsInMapRange_Scaled(x, y) && (y == tile.tileY || x == tile.tileX))
                    {
                        if (mapFlags[x][y] == 0 && scaledMap[x][y].IsLand == tileType)
                        {
                            mapFlags[x][y] = 1;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
        }

        //Debug.Log(tiles.Count.ToString());
        return new Room(tiles);
    }

    public bool IsInMapRange_Scaled(int x, int y)
    {
        return (x >= 0 && x < scaledMap.Length && y >= 0 && y < scaledMap[0].Length);
    }







    public struct Coord
    {
        public int tileX;
        public int tileY;

        public Coord(int x, int y)
        {
            tileX = x;
            tileY = y;
        }

        public override String ToString()
        {
            return "X: " + tileX.ToString() + ".Y: " + tileY.ToString();
        }
    }


    public class Room
    {
        public List<Coord> roomCoords;
        int startColumn = int.MaxValue;
        int endColumn = 0;
        int startRow = int.MaxValue;
        int endRow = 0;


        public Room(List<Coord> roomTiles)
        {
            roomCoords = roomTiles;
            
            for(int i = 0; i < roomTiles.Count; i++)
            {
                if(roomTiles[i].tileX > endColumn)
                {
                    endColumn = roomTiles[i].tileX;
                }
                if (roomTiles[i].tileX < startColumn)
                {
                    startColumn = roomTiles[i].tileX;
                }

                if (roomTiles[i].tileY > endRow)
                {
                    endRow = roomTiles[i].tileY;
                }
                if (roomTiles[i].tileY < startRow)
                {
                    startRow = roomTiles[i].tileY;
                }
            }
        }


        public Coord GetClosestTile(Coord _coord)
        {
            float bestDist = float.MaxValue;
            Coord bestCoord = roomCoords[0];

            for (int i = 1; i < roomCoords.Count; i++)
            {
                float distX = roomCoords[i].tileX - _coord.tileX;
                float distY = roomCoords[i].tileY - _coord.tileY;
                float dist = Mathf.Sqrt(Mathf.Pow(distX, 2) + Mathf.Pow(distY, 2));

                if (dist <= bestDist)
                {
                    bestDist = dist;
                    bestCoord = roomCoords[i];
                }
            }

            return bestCoord;
        }

        public bool DoesRoomContain(Coord searchCoord)
        {
            return DoesRoomContain(searchCoord.tileX, searchCoord.tileY);
        }
        public bool DoesRoomContain(int x, int y)
        {
            for (int i = 0; i < roomCoords.Count; i++)
            {
                if (roomCoords[i].tileX == x && roomCoords[i].tileY == y)
                    return true;
            }

            return false;
        }

        public int RoomSize
        {
            get { return roomCoords.Count; }
        }
        public int StartColumn
        {
            get { return startColumn; }
        }
        public int EndColumn
        {
            get { return endColumn; }
        }
        public int StartRow
        {
            get { return startRow; }
        }
        public int EndRow
        {
            get { return endRow; }
        }

        public bool[][] GenerateRoomMap()
        {
            int minX = int.MaxValue;
            int maxX = 0;
            int minY = int.MaxValue;
            int maxY = 0;

            for (int i = 0; i < roomCoords.Count; i++)
            {
                if (roomCoords[i].tileX < minX)
                    minX = roomCoords[i].tileX;

                if (roomCoords[i].tileX > maxX)
                    maxX = roomCoords[i].tileX;


                if (roomCoords[i].tileY < minY)
                    minY = roomCoords[i].tileY;

                if (roomCoords[i].tileY > maxY)
                    maxY = roomCoords[i].tileY;
            }


            bool[][] roomMap = new bool[maxX - minX + 1][];
            for (int i = 0; i < roomMap.Length; i++)
            {
                roomMap[i] = new bool[maxY - minY + 1];
            }



            for (int i = 0; i < roomCoords.Count; i++)
            {
                roomMap[roomCoords[i].tileX - minX][roomCoords[i].tileY - minY] = true;
            }

            return roomMap;
        }
    }

    #endregion






    #region Map Utility

    public static List<Room> GetRooms(ref float[][] map, float minVal, float maxVal)
    {
        List<Room> rooms = new List<Room>();
        bool[][] mapFlags = new bool[map.Length][];
        for (int i = 0; i < mapFlags.Length; i++)
        {
            mapFlags[i] = new bool[map[0].Length];
        }

        for (int x = 0; x < mapFlags.Length; x++)
        {
            for (int y = 0; y < mapFlags[0].Length; y++)
            {
                if (!mapFlags[x][y] && map[x][y] >= minVal && map[x][y] <= maxVal)
                {
                    Room newRoom = GetRoom(ref map, x, y, minVal, maxVal);
                    rooms.Add(newRoom);


                    List<Coord> roomCoords = newRoom.roomCoords;
                    for (int i = 0; i < roomCoords.Count; i++)
                    {
                        mapFlags[roomCoords[i].tileX][roomCoords[i].tileY] = true;
                    }
                }
            }
        }

        return rooms;
    }
    public static Room GetRoom(ref float[][] map, int startX, int startY, float minVal, float maxVal)
    {

        if (map == null || map[0] == null
            || startX < 0 || startX >= map.Length || startY < 0 || startY >= map[0].Length
            || map[startX][startY] < minVal || map[startX][startY] > maxVal)
        {

            return new Room(new List<Coord>());
        }



        List<Coord> tiles = new List<Coord>();

        int[][] mapFlags = new int[map.Length][];
        for (int i = 0; i < mapFlags.Length; i++)
        {
            mapFlags[i] = new int[map[0].Length];
        }


        Queue<Coord> queue = new Queue<Coord>();

        queue.Enqueue(new MapGenerator.Coord(startX, startY));
        mapFlags[startX][startY] = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if ((x >= 0 & x < map.Length & y >= 0 & y < map[0].Length) && mapFlags[x][y] == 0 && map[x][y] >= minVal && map[x][y] <= maxVal)
                    {
                        mapFlags[x][y] = 1;
                        queue.Enqueue(new Coord(x, y));
                    }
                }
            }
        }

        return new Room(tiles);
    }







    public static List<Room> GetRooms(ref int[][] map, int roomID)
    {
        List<Room> rooms = new List<Room>();
        bool[][] mapFlags = new bool[map.Length][];
        for (int i = 0; i < mapFlags.Length; i++)
        {
            mapFlags[i] = new bool[map[0].Length];
        }

        for (int x = 0; x < mapFlags.Length; x++)
        {
            for (int y = 0; y < mapFlags[0].Length; y++)
            {
                if (!mapFlags[x][y] && map[x][y] == 1)
                {
                    Room newRoom = GetRoom(ref map, x, y, roomID);
                    rooms.Add(newRoom);


                    List<Coord> roomCoords = newRoom.roomCoords;
                    for (int i = 0; i < roomCoords.Count; i++)
                    {
                        mapFlags[roomCoords[i].tileX][roomCoords[i].tileY] = true;
                    }
                }
            }
        }

        return rooms;
    }


    public static Room GetRoom(ref int[][] map, int startX, int startY, int roomID)
    {

        if (map == null || map[0] == null
            || startX < 0 || startX >= map.Length || startY < 0 || startY >= map[0].Length
            || map[startX][startY] != roomID)
        {

            return new Room(new List<Coord>());
        }



        List<Coord> tiles = new List<Coord>();

        int[][] mapFlags = new int[map.Length][];
        for (int i = 0; i < mapFlags.Length; i++)
        {
            mapFlags[i] = new int[map[0].Length];
        }


        Queue<Coord> queue = new Queue<Coord>();

        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX][startY] = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if ((x >= 0 & x < map.Length & y >= 0 & y < map[0].Length) && mapFlags[x][y] == 0 && map[x][y] == roomID)
                    {
                        mapFlags[x][y] = 1;
                        queue.Enqueue(new Coord(x, y));
                    }
                }
            }
        }

        return new Room(tiles);
    }

    public static Room GetRoom(ref bool[][] map, int startX, int startY)
    {

        if (map == null || map[0] == null
            || startX < 0 || startX >= map.Length || startY < 0 || startY >= map[0].Length
            || !map[startX][startY])
        {

            return new Room(new List<Coord>());
        }



        List<Coord> tiles = new List<Coord>();

        int[][] mapFlags = new int[map.Length][];
        for (int i = 0; i < mapFlags.Length; i++)
        {
            mapFlags[i] = new int[map[0].Length];
        }


        Queue<Coord> queue = new Queue<Coord>();

        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX][startY] = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if ((x >= 0 & x < map.Length & y >= 0 & y < map[0].Length) && mapFlags[x][y] == 0 && map[x][y])
                    {
                        mapFlags[x][y] = 1;
                        queue.Enqueue(new Coord(x, y));
                    }
                }
            }
        }

        return new Room(tiles);
    }

    public static List<Room> GetRooms(ref bool[][] map)
    {
        List<Room> rooms = new List<Room>();
        bool[][] mapFlags = new bool[map.Length][];
        for (int i = 0; i < mapFlags.Length; i++)
        {
            mapFlags[i] = new bool[map[0].Length];
        }

        for (int x = 0; x < mapFlags.Length; x++)
        {
            for (int y = 0; y < mapFlags[0].Length; y++)
            {
                if (!mapFlags[x][y] && map[x][y])
                {
                    Room newRoom = GetRoom(ref map, x, y);
                    rooms.Add(newRoom);


                    List<MapGenerator.Coord> roomCoords = newRoom.roomCoords;
                    for (int i = 0; i < roomCoords.Count; i++)
                    {
                        mapFlags[roomCoords[i].tileX][roomCoords[i].tileY] = true;
                    }
                }
            }
        }

        return rooms;
    }
    #endregion


    #region Perlin Noise Generation

    float[][] ApplySquareMask(float[][] regionMap, int regionWidth, int regionHeight)
    {
        float max_width = ((regionWidth + regionHeight) / 2) * 0.5f;
        max_width *= .9f;

        for (int i = 0; i < regionMap.Length; i++)
        {
            for (int k = 0; k < regionMap[0].Length; k++)
            {

                float distance_x = Mathf.Abs(i - regionWidth * 0.5f);
                float distance_y = Mathf.Abs(k - regionHeight * 0.5f);
                float distance = Mathf.Max(distance_x, distance_y); // square mask

                float delta = distance / max_width;
                float gradient = delta * delta;

                regionMap[i][k] *= Mathf.Max(0.0f, 1.0f - gradient);
            }
        }

        return regionMap;
    }
    float[][] ApplyCircleMask(float[][] regionMap, float regionRadius)
    {
        float max_width = regionRadius;
        max_width *= .9f;

        for (int i = 0; i < regionMap.Length; i++)
        {
            for (int k = 0; k < regionMap[0].Length; k++)
            {

                float distance_x = Mathf.Abs(i - regionMap.Length * 0.5f);
                float distance_y = Mathf.Abs(k - regionMap[0].Length * 0.5f);
                float distance = Mathf.Sqrt(Mathf.Pow(distance_x, 2) + Mathf.Pow(distance_y, 2));

                float delta = distance / max_width;
                float gradient = delta * delta;

                regionMap[i][k] *= Mathf.Max(0.0f, 1.0f - gradient);
            }
        }

        return regionMap;
    }

    float[][] GenerateSmoothNoise(float[][] baseNoise, int octave)
    {
        int width = baseNoise.Length;
        int height = baseNoise[0].Length;

        float[][] smoothNoise = new float[width][];
        for (int i = 0; i < width; i++)
        {
            smoothNoise[i] = new float[height];
        }

        int samplePeriod = 1 << octave; // calculates 2 ^ k
        float sampleFrequency = 1.0f / samplePeriod;

        for (int i = 0; i < width; i++)
        {
            //calculate the horizontal sampling indices
            int sample_i0 = (i / samplePeriod) * samplePeriod;
            int sample_i1 = (sample_i0 + samplePeriod) % width; //wrap around
            float horizontal_blend = (i - sample_i0) * sampleFrequency;

            for (int j = 0; j < height; j++)
            {
                //calculate the vertical sampling indices
                int sample_j0 = (j / samplePeriod) * samplePeriod;
                int sample_j1 = (sample_j0 + samplePeriod) % height; //wrap around
                float vertical_blend = (j - sample_j0) * sampleFrequency;

                //blend the top two corners
                float top = Interpolate(baseNoise[sample_i0][sample_j0],
                                        baseNoise[sample_i1][sample_j0], horizontal_blend);

                //blend the bottom two corners
                float bottom = Interpolate(baseNoise[sample_i0][sample_j1],
                                           baseNoise[sample_i1][sample_j1], horizontal_blend);

                //final blend
                smoothNoise[i][j] = Interpolate(top, bottom, vertical_blend);
            }
        }

        return smoothNoise;
    }

    float[][] GeneratePerlinNoise(float[][] baseNoise, int octaveCount)
    {
        int width = baseNoise.Length;
        int height = baseNoise[0].Length;

        float[][][] smoothNoise = new float[octaveCount][][]; //an array of 2D arrays containing

        float persistance = 0.7f;

        //generate smooth noise
        for (int i = 0; i < octaveCount; i++)
        {
            smoothNoise[i] = GenerateSmoothNoise(baseNoise, i);
        }

        float[][] perlinNoise = new float[width][]; //an array of floats initialised to 0
        for (int i = 0; i < width; i++)
        {
            perlinNoise[i] = new float[height];
        }

        float amplitude = 1.0f;
        float totalAmplitude = 0.0f;

        //blend noise together
        for (int octave = octaveCount - 1; octave >= 0; octave--)
        {
            amplitude *= persistance;
            totalAmplitude += amplitude;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    perlinNoise[i][j] += smoothNoise[octave][i][j] * amplitude;
                }
            }
        }

        //normalisation
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                perlinNoise[i][j] /= totalAmplitude;
            }
        }

        return perlinNoise;
    }

    float[][] GeneratePerlinNoise(int width, int height, int octaveCount)
    {
        float[][] baseNoise = GenerateWhiteNoise(width, height);

        return GeneratePerlinNoise(baseNoise, octaveCount);
    }

    float[][] GenerateWhiteNoise(int width, int height)
    {
        float[][] noise = new float[width][];
        for (int i = 0; i < width; i++)
        {
            noise[i] = new float[height];
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (noiseScale == 0)
                {
                    noise[i][j] = (float)pseudoRandom.NextDouble() % 1;
                }
                else
                {
                    float a = ((float)pseudoRandom.NextDouble() * ((float)(i) / (float)(width))) * noiseScale;
                    float b = ((float)pseudoRandom.NextDouble() * ((float)(j) / (float)(height))) * noiseScale;
                    noise[i][j] = Mathf.PerlinNoise(a, b);
                }
            }
        }

        return noise;
    }

    float Interpolate(float x0, float x1, float alpha)
    {
        return x0 * (1 - alpha) + alpha * x1;
    }
    #endregion


    #region Utility

    float GetRandomGaussian(DeviatingInteger _deviator)
    {
        return GetRandomGaussian(_deviator.Mean, _deviator.Sigma);
    }
    float GetRandomGaussian(int mean, int stdDev)
    {
        double u1 = pseudoRandom.NextDouble();
        double u2 = pseudoRandom.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0f * Math.Log(u1)) * Math.Sin(2.0f * Math.PI * u2); //random normal(0,1)
        double randNormal = mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)

        return (float)randNormal;
    }

    Vector2 GetRandomInCircle()
    {
        float angle = 2.0f * Mathf.PI * (float)pseudoRandom.NextDouble();
        float radius = (float)pseudoRandom.NextDouble();

        float x = radius * Mathf.Cos(angle);
        float y = radius * Mathf.Sin(angle);

        return new Vector2(x, y);
    }

    void ApplyRegion(int x, int y, float[][] regionMap)
    {
        for (int a = 0; a < regionMap.Length; a++)
        {
            for (int b = 0; b < regionMap[0].Length; b++)
            {
                int newX = (int)(x - (regionMap.Length / 2) + a);
                int newY = (int)(y - (regionMap[0].Length / 2) + b);

                if (IsInMapRange(newX, newY))
                {
                    unscaledMap[newX][newY].Value += regionMap[a][b];
                    unscaledMap[newX][newY].IsLand = unscaledMap[newX][newY].Value >= heightThreshold;
                }
            }
        }
    }


    List<Coord> GetValidNeighboringTiles(ref MapNode[][] map, int x, int y, int spread)
    {
        List<Coord> neighbors = new List<Coord>();

        for (int i = x - spread; i <= x + spread; i++)
        {
            for (int j = y - spread; j <= y + spread; j++)
            {
                if (i >= 0 && i < map.Length && j >= 0 && j < map[0].Length && !(i == x && j == y))
                {

                    try
                    {
                        if (map[i][j].IsLand)
                            neighbors.Add(new Coord(i, j));
                    }
                    catch (Exception e)
                    {
                        Debug.Log(i + " -- " + j + " ##### " + map.Length + "x" + map[0].Length);
                        throw e;
                    }

                }
            }
        }

        return neighbors;
    }
    List<Coord> GetValidNeighboringTiles(ref int[][] map, int x, int y, int spread)
    {
        List<Coord> neighbors = new List<Coord>();

        for (int i = x - spread; i <= x + spread; i++)
        {
            for (int j = y - spread; j <= y + spread; j++)
            {
                if (i >= 0 && i < map.Length && j >= 0 && j < map[0].Length && !(i == x && j == y))
                {

                    try
                    {
                        if (map[i][j] >= 1)
                            neighbors.Add(new Coord(i, j));
                    }
                    catch (Exception e)
                    {
                        Debug.Log(i + " -- " + j + " ##### " + map.Length + "x" + map[0].Length);
                        throw e;
                    }

                }
            }
        }

        return neighbors;
    }

    List<Coord> GetValidNeighboringTiles(ref float[][] map, int x, int y, int spread)
    {
        List<Coord> neighbors = new List<Coord>();

        for (int i = x - spread; i <= x + spread; i++)
        {
            for (int j = y - spread; j <= y + spread; j++)
            {
                if (i >= 0 && i < map.Length && j >= 0 && j < map[0].Length && !(i == x && j == y))
                {

                    if (map[i][j] >= heightThreshold)
                        neighbors.Add(new Coord(i, j));

                }
            }
        }

        return neighbors;
    }

    public List<Coord> GetWalkableNeighbors(Coord centerCoord, int spread)
    {
        return GetWalkableNeighbors(centerCoord.tileX, centerCoord.tileY, spread);
    }
    public List<Coord> GetWalkableNeighbors(int x, int y, int spread)
    {

        List<Coord> neighbors = new List<Coord>();

        for (int i = x - spread; i <= x + spread; i++)
        {
            for (int j = y - spread; j <= y + spread; j++)
            {
                if (IsInMapRange_Scaled(i, j) && !(i == x && j == y) && scaledMap[i][j].IsLand)
                {
                    neighbors.Add(new Coord(i, j));
                }
            }
        }

        return neighbors;
    }
    #endregion


    /*
    public float GetLargestValue()
    {
        if (scaledHeightMap == null)
            return -1;

        float largestValue = 0;
        for (int i = 0; i < scaledHeightMap.Length; i++)
        {
            for (int j = 0; j < scaledHeightMap[0].Length; j++)
            {
                largestValue = Mathf.Max(largestValue, scaledHeightMap[i][j]);
            }
        }

        return largestValue;
    }
    */

        public float MapSize
    {
        get { return generatedMapWidth * generatedMapHeight * scaleFactor; }
    }
    public Vector2 MapDimensions
    {
        get { return new Vector2(generatedMapWidth, generatedMapHeight); }
    }
    public Vector2 MapDimensions_Scaled
    {
        get { return new Vector2(generatedMapWidth, generatedMapHeight) * scaleFactor; }
    }



    public bool[][] LandMap
    {
        get
        {
            bool[][] _map = new bool[scaledMap.Length][];
            for(int i = 0; i < _map.Length; i++)
            {
                _map[i] = new bool[scaledMap[i].Length];

                for(int k = 0; k < _map[i].Length; k++)
                {
                    _map[i][k] = scaledMap[i][k].IsLand;
                }
            }

            return _map;
        }
    }
    public bool[][] LandMap_Unscaled
    {
        get
        {
            bool[][] _map = new bool[unscaledMap.Length][];
            for (int i = 0; i < _map.Length; i++)
            {
                _map[i] = new bool[unscaledMap[i].Length];

                for (int k = 0; k < _map[i].Length; k++)
                {
                    _map[i][k] = unscaledMap[i][k].IsLand;
                }
            }

            return _map;
        }
    }


    public float[][] HeightMap
    {
        get
        {
            float[][] _map = new float[scaledMap.Length][];
            for (int i = 0; i < _map.Length; i++)
            {
                _map[i] = new float[scaledMap[i].Length];

                for (int k = 0; k < _map[i].Length; k++)
                {
                    _map[i][k] = scaledMap[i][k].Value;
                }
            }

            return _map;
        }
    }
    public float[][] HeightMap_Unscaled
    {
        get
        {
            float[][] _map = new float[unscaledMap.Length][];
            for (int i = 0; i < _map.Length; i++)
            {
                _map[i] = new float[unscaledMap[i].Length];

                for (int k = 0; k < _map[i].Length; k++)
                {
                    _map[i][k] = unscaledMap[i][k].Value;
                }
            }

            return _map;
        }
    }
    /*
    public float[][] ScaledHeightMap
    {
        get { return scaledHeightMap; }
    }

    public int[][] ScaledLandMap
    {
        get { return scaledLandMap; }
    }
    */

   



    public bool UseRandomSeed
    {
        get { return useRandomSeed; }
        set { useRandomSeed = value; }
    }
    public bool ConnectAllRooms
    {
        get { return connectAllRooms; }
        set { connectAllRooms = value; }
    }

    public int ScaleFactor
    {
        get { return scaleFactor; }
        set { scaleFactor = value; }
    }

    public GameObject GroundObject
    {
        get { return groundObject; }
    }
    public LayerMask GroundObjectLayer
    {
        get { return groundObject.layer; }
    }

    public string Seed
    {
        get { return seed; }
        set { seed = value; }
    }

    /*
    void DrawMap(ref float[][] mapArr)
    {
        Vector3 worldPos;
        Vector3 cubeSize = Vector3.one * 0.75f;


        for (int x = 0; x < mapArr.Length; x++)
        {
            for (int y = 0; y < mapArr[0].Length; y++)
            {
                float val = mapArr[x][y];

                if (val == 0)
                    continue;


                Gizmos.color = maxElevation == 0 ? Color.white : elevationGradient.Evaluate(val / maxElevation);

             
                worldPos = IndexToWorldPoint(x, y);
                Gizmos.DrawCube(worldPos, cubeSize);
            }
        }
    }*/
    /*
    void OnDrawGizmos()
    {
        if (gizmoToShow == GizmoStyle.NONE)
            return;


        float[][] gizmoHeightMap = null;
        int[][] gizmoLandMap = null;

        bool showRooms = false;
        bool showLargeRooms = false;

        switch (gizmoToShow)
        {
            case GizmoStyle.HEIGHTMAP:
                gizmoHeightMap = heightMap;
                break;
            case GizmoStyle.LANDMAP:
                gizmoLandMap = landMap;
                break;
            case GizmoStyle.SCALED_HEIGHTMAP:
                gizmoHeightMap = scaledHeightMap;
                break;
            case GizmoStyle.SCALED_LANDMAP:
                gizmoLandMap = scaledLandMap;
                break;
            case GizmoStyle.ROOMS_LARGE:
                showRooms = true;
                showLargeRooms = true;
                break;
            case GizmoStyle.ROOMS_SMALL:
                showRooms = true;
                showLargeRooms = false;
                break;
            default:
                break;
        }
        Vector3 cubeSize = Vector3.one * 0.75f;

        if (gizmoHeightMap != null && (gizmoHeightMap.Length * gizmoHeightMap[0].Length) <= MAX_GIZMO_SIZE)
        {
            Gizmos.color = Color.white;
            //Vector3 cubeColor = Utilities.RGBtoHSV(Color.white);
            Vector3 worldPos;

            //float maxHeight = GetLargestValue();

            for (int x = 0; x < gizmoHeightMap.Length; x++)
            {
                for (int y = 0; y < gizmoHeightMap[0].Length; y++)
                {

                    float val = gizmoHeightMap[x][y];

                    if (val == 0)
                        continue;
                    //if(val < heightThreshold)
                    //		continue;

                    /*
                    float _key = val / GRADIENT_MAX;

                    if (_key > 1)
                        _key = 1;


                    Gizmos.color = elevationGradient.Evaluate(_key);
                    */
    /*
    if (val < .75f){
        Gizmos.color = Color.black;// Color.Lerp(Color.red, Color.black, 0.5f);
    }else if (val < 1f){
        Gizmos.color = Color.red;
    }else if (val < 1.5f){
        Gizmos.color = Color.yellow; // Color.Lerp(Color.red, Color.yellow, 0.5f);
    }else if (val < 2f){
        Gizmos.color = Color.green; // Color.yellow;
    }else if (val < 3f){
        Gizmos.color = Color.blue; //Color.green;
    }else{
        Gizmos.color = Color.white; // Color.cyan;
    }
    *
    // Gizmos.color = Color.Lerp(Color.red, Color.green, val / maxHeight);
    //cubeColor.z = gizmoMap[x][y];
    //Gizmos.color = Utilities.HSVtoRGB(cubeColor);

    worldPos = IndexToWorldPoint(x, y); //transform.position + new Vector3((-gizmoMap.Length / 2) + x, (-gizmoMap[0].Length / 2) +  y, 0);
    Gizmos.DrawCube(worldPos, cubeSize);
}
}
}
else if (gizmoLandMap != null && (gizmoLandMap.Length * gizmoLandMap[0].Length) <= MAX_GIZMO_SIZE)
{
Vector3 worldPos;

for (int x = 0; x < gizmoLandMap.Length; x++)
{
for (int y = 0; y < gizmoLandMap[0].Length; y++)
{

    Gizmos.color = (gizmoLandMap[x][y] == 0) ? Color.black : Color.white;

    worldPos = IndexToWorldPoint(x, y); //transform.position + new Vector3((-gizmoIntMap.Length / 2) + x, (-gizmoIntMap[0].Length / 2) +  y, 0);
    Gizmos.DrawCube(worldPos, cubeSize);
}
}
}
if (showRooms)
{
List<Room> tempRooms = GetRoomsOfType(1);

for (int i = 0; i < tempRooms.Count; i++)
{

int diff = tempRooms[i].RoomSize - roomSizeThreshold;

if (diff < 0 && showLargeRooms)
{
    continue;
}
else if (diff >= 0 && !showLargeRooms)
{
    continue;
}

List<Coord> tempCoords = tempRooms[i].roomCoords;

for (int k = 0; k < tempCoords.Count - 1; k++)
{
    Gizmos.DrawCube(CoordToWorldPoint(tempCoords[k]), cubeSize);
    Gizmos.DrawLine(CoordToWorldPoint(tempCoords[k]), CoordToWorldPoint(tempCoords[k + 1]));
}
}
}
}
*/
    void OnDrawGizmos()
    {
        if (gizmoToShow == GizmoStyle.NONE)
            return;

      
        MapNode[][] gizmoMap = null;
        Vector3 cubeSize = Vector3.one * 0.75f;
        Vector3 worldPos = Vector3.zero;

        switch (gizmoToShow)
        {
            case GizmoStyle.LANDMAP:
                gizmoMap = unscaledMap;
                break;
            case GizmoStyle.SCALED_LANDMAP:
                gizmoMap = scaledMap;
                break;
            default:
                gizmoMap = scaledMap;
                break;
        }
       

        if (gizmoMap != null && (gizmoMap.Length * gizmoMap[0].Length) <= MAX_GIZMO_SIZE)
        {
            Gizmos.color = Color.white;
           
            


            for (int x = 0; x < gizmoMap.Length; x++)
            {
                for (int y = 0; y < gizmoMap[0].Length; y++)
                {
                    Gizmos.color = Color.white;

                    if (!gizmoMap[x][y].IsLand)
                        continue;


                    switch (gizmoToShow)
                    {
                        case GizmoStyle.LANDMAP:
                            worldPos = IndexToWorldPoint_Unscaled(x, y);
                            break;
                        case GizmoStyle.SCALED_LANDMAP:
                            worldPos = IndexToWorldPoint(x, y);
                            break;
                        case GizmoStyle.PREROOMCONNECTION:
                            if (!gizmoMap[x][y].IsBaseMap)
                                continue;

                            worldPos = IndexToWorldPoint(x, y);
                            break;
                    }
                    

                    Gizmos.DrawCube(worldPos, cubeSize);
                }
            }
        }
    }



    void OnValidate()
    {
        generatedMapWidth = Math.Max(1, generatedMapWidth);
        generatedMapHeight = Math.Max(1, generatedMapHeight);

        borderSize = Math.Max(1, borderSize);
        borderSize = Math.Min(borderSize, Math.Min(generatedMapWidth, generatedMapHeight) / 2);

        fractalAngleIncrease = Math.Max(1, fractalAngleIncrease);
        initialFractalNumber = Math.Max(1, initialFractalNumber);

        roomSizeThreshold = Math.Max(1, roomSizeThreshold);

        Utilities.ValidateCurve(elevationCurve,0f, MAXIMUM_NOISE_VALUE, 0f, Values.MAXIMUM_MAP_HEIGHT);
    }
}
