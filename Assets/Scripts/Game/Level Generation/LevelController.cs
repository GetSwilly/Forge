using UnityEngine;
using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;


[RequireComponent(typeof(MapGenerator))]
[RequireComponent(typeof(MeshGenerator))]
[RequireComponent(typeof(A_Star_Pathfinding))]
[RequireComponent(typeof(WeatherSystem))]
public class LevelController : MonoBehaviour {
    static readonly int COORDINATE_CHECK_MAX = 1000;

    static readonly string ENVIRONMENT_OBJECT_FOLDER_PATH = "Level Objects/Environment Objects";
    static readonly string REGION_FOLDER_PATH = "Level Objects/Environment Areas";
    static readonly string CREATURE_FOLDER_PATH = "Creatures";
    static readonly string BONUS_OBJECT_PATH = "Bonus Objects";

    static readonly float DEITY_FAVOR_TEMPERATURE_MULTIPLIER = 10f;
    static readonly float DEITY_FAVOR__MOISTURE_MULTIPLIER = 10f;
    static readonly float DEITY_FAVOR_TIME_MULTIPLIER = 10f;

    static readonly float PLACEMENT_HEIGHT_OFFSET = 0.05f;
    static readonly float PROCEDURAL_REGION_OFFSET = 0.05f;
    static readonly float PLACEMENT_POPULATION_MAX_MULTIPLIER = 7f;

    static readonly int MINIMUM_REGION_SIZE = 30;
  


    [Tooltip("Current temperature value")]
    [SerializeField]
    [Range(0f, 100f)]
    float currentTemperature = 50f;

    [Tooltip("Current moisture value")]
    [SerializeField]
    [Range(0f, 100f)]
    float currentMoisture = 50f;



    //[SerializeField]
    // ColorRange climateColors = new ColorRange(10);

    [Tooltip("Color applied to the ground based on Current Temperature and Current Moisture. X-axis = Temperature. Y-Axis = Moisture.")]
    [SerializeField]
    ColorRange climateColors = new ColorRange(10);


    [Tooltip("Physical material applied to the ground based on Current Temperature and Current Moisture.")]
    [SerializeField]
    List<ClimateObjectOfPhysicMaterial> climateMaterials = new List<ClimateObjectOfPhysicMaterial>();





    //Level Variables
    [SerializeField]
    GameObject startingPadPrefab;
    [SerializeField]
    GameObject endingPadPrefab;


    Transform currentStartGoal;
    List<Transform> currentEndGoals = new List<Transform>();

    GameObject groundObject;
    GameObject genObjParent;

    [SerializeField]
    LayerMask groundMask;

    bool[][] smallLandMap;
    float[][] smallHeightMap;

    bool[][] largeLandMap;
    float[][] largeHeightMap;



    NodeType[][] regionMap;
    int[][] objectMap;



    public enum GizmoType { NONE, NODETYPE, OBJECT, LAND, LAND_SMALL }
    public GizmoType gizmoToDraw;
    List<NodeType> nodesToDraw = new List<NodeType>();

 
    
    [SerializeField]
    [Range(1,10)]
    int scaleFactor = 4;

    [Flags]
    public enum LevelOptions
    {
        PlaceRegions = 1,
        PlaceEnvironmentObjects = 1 << 1,
        PlaceBonusObjects = 1 << 2,
        PlaceCreatures = 1 << 3,
        ColorGround = 1 << 4,
        BlendColors = 1 << 5
    }

    [SerializeField]
    [EnumFlags]
    LevelOptions m_Options;
    


    [SerializeField]
    string seed = "";

    [SerializeField]
    bool useRandomSeed = true;

    [SerializeField]
    bool showDebug = false;


    WeatherSystem m_Weather;
    System.Random pseudoRandom;



    [HideInInspector]
    public static LevelController Instance { get; private set; }
    void Awake()
    {
        Instance = this;

        m_Weather = GetComponent<WeatherSystem>();
    }



    /*
    public void EditorGenerateLevel()
    {
        StartCoroutine(GenerateLevel(currentTemperature, currentMoisture));
    }
    */
    public void EditorGenerateLevel(bool isRandom)
    {
        /*
        if (seed == "" || useRandomSeed)
        {
            seed = System.DateTime.Now.Ticks.ToString();
        }

        pseudoRandom = new System.Random(seed.GetHashCode());

        if (isRandom)
        {
            currentTemperature = (float)( pseudoRandom.NextDouble() * 100);  //UnityEngine.Random.Range(0f, 100f);
            currentMoisture = (float)(pseudoRandom.NextDouble() * 100);  //UnityEngine.Random.Range(0f, 100f);
        }


        //EditorGenerateLevel();
        StartCoroutine(GenerateLevel(currentTemperature, currentMoisture));

        */


        if (isRandom)
        {
            StartCoroutine(GenerateLevel());
        }
        else
        {
            StartCoroutine(GenerateCurrentLevel());
        }
    }


    public IEnumerator GenerateLevel()
    {
        yield return StartCoroutine(LevelGeneration(true));

    }
    public IEnumerator GenerateCurrentLevel()
    {
        yield return StartCoroutine(LevelGeneration(false));

    }


    IEnumerator LevelGeneration(bool isRandomEnvironment)
    {
        //Time.timeScale = 0;

        float startTime = Time.realtimeSinceStartup;


        float _time;

        if (seed == "" || useRandomSeed)
        {
            seed = System.DateTime.Now.Ticks.ToString();
        }
        pseudoRandom = new System.Random(seed.GetHashCode());




        m_Weather.Terminate();


        if (isRandomEnvironment)
        {
            currentTemperature = (float)(pseudoRandom.NextDouble() * 100);
            currentMoisture = (float)(pseudoRandom.NextDouble() * 100);
            m_Weather.SetTimeOfDay((float)(pseudoRandom.NextDouble() * 100), true);
        }


        ApplyGodFavor();

        

        //ProceduralColorManager.Instance.SaturateGeneratedColors(1f, lightIntensity);


        //Set Map Generation variables
        MapGenerator mapGen = (MapGenerator.Instance == null ? FindObjectOfType<MapGenerator>() : MapGenerator.Instance);

        mapGen.Seed = seed;
        mapGen.UseRandomSeed = false;
        mapGen.ConnectAllRooms = true;
        mapGen.methodOfGeneration = MapGenerator.Generation_Method.FRACTAL;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.InflateLoadingScreen("Generating Level");
        }


        _time = Time.realtimeSinceStartup;


        //Generate Map
        yield return StartCoroutine(mapGen.GenerateMap());

        if (ShowDebug)
        {
            UnityEngine.Debug.Log("LevelController --- LevelGeneration -- GenerateMap() total run time " + (Time.realtimeSinceStartup - _time).ToString() + " seconds");
        }


        groundObject = mapGen.GroundObject;
        MeshCollider coll = groundObject.GetComponent<MeshCollider>();

        if (coll != null)
            Destroy(coll);

        groundObject.AddComponent<MeshCollider>();


        if (UIManager.Instance != null)
        {
            UIManager.Instance.InflateLoadingScreen("Coloring");
        }


        //Color Ground
        ColorizeGround();
        //ApplyGroundMaterial();


        _time = Time.realtimeSinceStartup;

        //Populate level with regions and objects
        yield return StartCoroutine(PopulateLevel());

        if (ShowDebug)
        {
            UnityEngine.Debug.Log("LevelController --- LevelGeneration -- PopulateLevel() total run time " + (Time.realtimeSinceStartup - _time).ToString() + " seconds");
        }



        /*
        if (UIManager.Instance != null)
            UIManager.Instance.InflateLoadingScreen("Constructing A*");

        _time = Time.realtimeSinceStartup;

        //Set up pathfinding
        yield return StartCoroutine((A_Star_Pathfinding.Instance == null ? FindObjectOfType<A_Star_Pathfinding>() : A_Star_Pathfinding.Instance).GridFromDetection(new Vector2(objectMap.Length, objectMap[0].Length)));

        if (ShowDebug)
        {
            Debug.Log("LevelController --- LevelGeneration -- A* GridFromDetection() total run time " + (Time.realtimeSinceStartup - _time).ToString() + " seconds");
        }

        */





        m_Weather.Initialize();



        if (UIManager.Instance != null)
        {
            UIManager.Instance.DeflateAll();
            UIManager.Instance.ShowTitleText("APEX", "World " + GameManager.Instance.CurrentLevel);
        }



        if (ShowDebug)
        {
            UnityEngine.Debug.Log(string.Format("LevelController --- LevelGeneration total run time: {0} seconds", Time.realtimeSinceStartup - startTime));
        }
    }


    //Add objects to level
    IEnumerator PopulateLevel()
    {
        Vector2 _mapSize = MapGenerator.Instance.MapDimensions_Scaled;
        A_Star_Pathfinding.Instance.WorldSize = new Vector3(_mapSize.x, 0, _mapSize.y);


        if (UIManager.Instance != null)
            UIManager.Instance.InflateLoadingScreen("Detecting Ground");

        yield return StartCoroutine(A_Star_Pathfinding.Instance.GroundCheck());



        largeLandMap = A_Star_Pathfinding.Instance.LandMap;
        largeHeightMap = A_Star_Pathfinding.Instance.HeightMap;

        
        int lengthA = (int)(largeLandMap.Length / (float)scaleFactor);
        int lengthB = (int)(largeLandMap[0].Length / (float)scaleFactor);

        //Debug.Log(string.Format("Original : {0} x {1}. New: {2} x {3}", largeLandMap.Length, largeLandMap[0].Length, lengthA, lengthB));

        smallLandMap = new bool[lengthA][];
        smallHeightMap = new float[lengthA][];


        for (int i = 0; i < lengthA; i++)
        {
            smallLandMap[i] = new bool[lengthB];
            smallHeightMap[i] = new float[lengthB];

            for (int k = 0; k < lengthB; k++)
            {
                int x = (int)((i / (float)lengthA) * largeLandMap.Length);
                int y = (int)((k / (float)lengthB) * largeLandMap[0].Length);

                if (x >= largeLandMap.Length)
                    x = largeLandMap.Length - 1;

                if (y >= largeLandMap[0].Length)
                    y = largeLandMap[0].Length - 1;


                smallLandMap[i][k] = largeLandMap[x][y];
                smallHeightMap[i][k] = largeHeightMap[x][y];
            }
        }
        

        float _time;


        //Destroy previously generated objects
        if (genObjParent != null || (genObjParent = GameObject.Find("Generated Objects")) != null)
        {
            Destroy(genObjParent);
        }

        genObjParent = new GameObject("Generated Objects");





        //Region and Object Map

        regionMap = new NodeType[largeLandMap.Length][];
        for (int i = 0; i < largeLandMap.Length; i++)
        {
            regionMap[i] = new NodeType[largeLandMap[i].Length];

            for (int k = 0; k < largeLandMap[i].Length; k++)
            {
                regionMap[i][k] = largeLandMap[i][k] ? NodeType.BasicGround : NodeType.Empty;
            }
        }
        /*
        regionMap = new NodeType[smallLandMap.Length][];
        for (int i = 0; i < smallLandMap.Length; i++)
        {
            regionMap[i] = new NodeType[smallLandMap[i].Length];

            for (int k = 0; k < smallLandMap[i].Length; k++)
            {
                regionMap[i][k] = smallLandMap[i][k] ? NodeType.BasicGround : NodeType.Empty;
            }
        }*/

        objectMap = new int[largeLandMap.Length][];
        for (int i = 0; i < largeLandMap.Length; i++)
        {
            objectMap[i] = new int[largeLandMap[i].Length];
        }





        _time = Time.realtimeSinceStartup;

        yield return StartCoroutine(PlaceStartAndFinish());

        if (ShowDebug)
        {
            UnityEngine.Debug.Log("LevelController --- PopulateLevel -- PlaceStartAndFinish() total run time " + (Time.realtimeSinceStartup - _time).ToString() + " seconds");
        }



        if (ShowDebug)
            UnityEngine.Debug.Log("Start and Finish placed");





        //Place Biome RegionsunscaledLandMap
        if (Utilities.HasFlag(m_Options, LevelOptions.PlaceRegions))
        {
            _time = Time.realtimeSinceStartup;

            yield return StartCoroutine(PlaceEnvironmentRegions());

            if (ShowDebug)
            {
                UnityEngine.Debug.Log("LevelController --- PopulateLevel -- PlaceEnvironmentRegions() total run time " + (Time.realtimeSinceStartup - _time).ToString() + " seconds");
            }


            if (ShowDebug)
                UnityEngine.Debug.Log("Regions placed");
        }

        if (UIManager.Instance != null)
            UIManager.Instance.InflateLoadingScreen("Detecting Environment Areas");


        yield return StartCoroutine(A_Star_Pathfinding.Instance.EnvironmentAreaCheck());
        regionMap = A_Star_Pathfinding.Instance.RegionMap;


        //Place Environment Objects
        if (Utilities.HasFlag(m_Options, LevelOptions.PlaceEnvironmentObjects))
        {
            _time = Time.realtimeSinceStartup;

            yield return StartCoroutine(PlaceEnvironmentObjects());

            if (ShowDebug)
            {
                UnityEngine.Debug.Log("LevelController --- PopulateLevel -- PlaceEnvironmentObjects() total run time " + (Time.realtimeSinceStartup - _time).ToString() + " seconds");
            }


            if (ShowDebug)
                UnityEngine.Debug.Log("Environment Objects placed");
        }

        //Place Bonus Objects
        if (Utilities.HasFlag(m_Options, LevelOptions.PlaceBonusObjects))
        {
            _time = Time.realtimeSinceStartup;

            yield return StartCoroutine(PlaceBonusObjects());

            if (ShowDebug)
            {
                UnityEngine.Debug.Log("LevelController --- PopulateLevel -- PlaceBonusObjects() total run time " + (Time.realtimeSinceStartup - _time).ToString() + " seconds");
            }

            if (ShowDebug)
                UnityEngine.Debug.Log("Bonus Objects placed");
        }

        //Place Creatures
        if (Utilities.HasFlag(m_Options, LevelOptions.PlaceCreatures))
        {
            _time = Time.realtimeSinceStartup;

            yield return StartCoroutine(PlaceCreatures());

            if (ShowDebug)
            {
                UnityEngine.Debug.Log("LevelController --- PopulateLevel -- PlaceCreatures() total run time " + (Time.realtimeSinceStartup - _time).ToString() + " seconds");
            }

            if (ShowDebug)
                UnityEngine.Debug.Log("Creatures placed");
        }


        if (UIManager.Instance != null)
            UIManager.Instance.InflateLoadingScreen("Detecting Obstacles");

        yield return StartCoroutine(A_Star_Pathfinding.Instance.EnvironmentCheck());
    }




    void ApplyGodFavor()
    {
        CurrentTemperature = Temperature_Favored;
        CurrentMoisture = Moisture_Favored;
        m_Weather.SetTimeOfDay(Time_Favored, false);
    }



    /*
    void ApplyGodFavor()
    {

        float _favor = (GameManager.Instance == null ? FindObjectOfType<GameManager>() : GameManager.Instance).GodFavor;
        float temperatureFavor = _favor * GOD_FAVOR_TEMPERATURE_MULTIPLIER;
        float moistureFavor = _favor * GOD_FAVOR__MOISTURE_MULTIPLIER;


        //In Good Favor?
        if (_favor > 0)
        {

            //Move temperature closer towards 50
            float tempDiff = 50f - currentTemperature;

            if (Mathf.Abs(tempDiff) < Mathf.Abs(temperatureFavor))
            {
                currentTemperature = 50f;
            }
            else if (tempDiff != 0)
            {
                currentTemperature = Mathf.Clamp(currentTemperature + (temperatureFavor * (tempDiff / tempDiff)), 0f, 100f);
            }



            //Move moisture closer towards 50
            float moistureDiff = 50f - currentMoisture;

            if (Mathf.Abs(moistureDiff) < Mathf.Abs(moistureFavor))
            {
                currentMoisture = 50f;
            }
            else if (moistureDiff != 0)
            {
                currentMoisture = Mathf.Clamp(currentMoisture + (moistureFavor * (moistureDiff / moistureDiff)), 0f, 100f);
            }

        }
        //In bad favor?
        else
        {
            //Move temperature closer to extremes
            currentTemperature += moistureFavor * (currentTemperature < 50 ? -1f : 1f);
            currentTemperature = Mathf.Clamp(currentTemperature, 0f, 100f);

            //Move moisture closer to extremes
            currentMoisture += moistureFavor * (currentMoisture < 50 ? -1f : 1f);
            currentMoisture = Mathf.Clamp(currentMoisture, 0f, 100f);

        }

        currentTime += _favor * GOD_FAVOR_TIME_MULTIPLIER;
        currentTime = Mathf.Clamp(currentTime, 0f, 100f);
    }
    */




    void ColorizeGround()
    {
        if (groundObject == null || !Utilities.HasFlag(m_Options, LevelOptions.ColorGround))
            return;


        MeshRenderer _renderer = groundObject.GetComponent<MeshRenderer>();


        if (_renderer != null)
        {
            Material _material = _renderer.material;

            Color _color = GetColor();
            //_color = Utilities.GetSimilarColor(_color, HUE_VARIATION, SATURATION_VARIATION, VALUE_VARIATION);

            _material.color = _color;
        }

    }

    void ApplyGroundMaterial()
    {

        if (groundObject == null)
            return;


        List<PhysicMaterial> viableMaterials = new List<PhysicMaterial>();

        for (int i = 0; i < climateMaterials.Count; i++)
        {
            if (climateMaterials[i].IsInRange(currentTemperature, currentMoisture))
                viableMaterials.Add(climateMaterials[i].GenericObject);
        }

        PhysicMaterial _material = null;

        if (viableMaterials.Count > 0)
        {
            _material = viableMaterials[pseudoRandom.Next(viableMaterials.Count)];
        }

        Collider _collider = groundObject.GetComponent<Collider>();
        _collider.material = _material;
    }

    Color GetColor()
    {
        float temperaturePercentage = currentTemperature / 100f;
        float moisturePercentage = currentMoisture / 100f;


        float actualX = temperaturePercentage * (climateColors.colorFields.Length - 1);
        int minX = Mathf.FloorToInt(actualX);
        int maxX = Mathf.CeilToInt(actualX);


        float actualY = moisturePercentage * (climateColors.colorFields[0].colorRow.Length - 1);
        int minY = Mathf.FloorToInt(actualY);
        int maxY = Mathf.CeilToInt(actualY);

        Color? defaultColor = climateColors.GetColor((int)Mathf.Clamp(actualX, 0, climateColors.colorFields.Length - 1), (int)Mathf.Clamp(actualY, 0, climateColors.colorFields[0].colorRow.Length - 1));
        defaultColor = defaultColor == null ? Color.red : defaultColor;

        if (!Utilities.HasFlag(m_Options, LevelOptions.BlendColors))
            return (Color)defaultColor;

        Color? colorA = climateColors.GetColor(minX, maxY);
        colorA = colorA == null ? defaultColor : colorA;
        float distA = Vector2.Distance(new Vector2(actualX, actualY), new Vector2(minX, maxY));

        Color? colorB = climateColors.GetColor(maxX, maxY);
        colorB = colorB == null ? defaultColor : colorB;
        float distB = Vector2.Distance(new Vector2(actualX, actualY), new Vector2(maxX, maxY));

        Color? colorC = climateColors.GetColor(minX, minY);
        colorC = colorC == null ? defaultColor : colorC;
        float distC = Vector2.Distance(new Vector2(actualX, actualY), new Vector2(minX, minY));

        Color? colorD = climateColors.GetColor(maxX, minY);
        colorD = colorD == null ? defaultColor : colorD;
        float distD = Vector2.Distance(new Vector2(actualX, actualY), new Vector2(maxX, minY));

        /*
        if (ShowDebug)
        {
            Debug.Log(this.name + " ##### GetColor()");
            Debug.Log("Actual : " + actualX + "," + actualY);
            Debug.Log("X : " + minX + " - " + maxX);
            Debug.Log("Y : " + minY + " - " + maxY);
            Debug.Log("A -- " + colorA);
            Debug.Log("B -- " + colorB);
            Debug.Log("C -- " + colorC);
            Debug.Log("D -- " + colorD);
        }
        */

        float totalDist = distA + distB + distC + distD;

        if (totalDist == 0)
            return (Color)defaultColor;

        Vector4 blendedColor = Vector4.zero;
        blendedColor += (Vector4)((Color)colorA * (1 - (distA / totalDist)));
        blendedColor += (Vector4)((Color)colorB * (1 - (distB / totalDist)));
        blendedColor += (Vector4)((Color)colorC * (1 - (distC / totalDist)));
        blendedColor += (Vector4)((Color)colorD * (1 - (distD / totalDist)));

        blendedColor /= 4f;
        // blendedColor /= 255f;
        //blendedColor.w = 255f;


        Color _retColor = blendedColor;
        _retColor.a = 255f;

        return _retColor;
    }






    #region Object Placement
    
    IEnumerator PlaceStartAndFinish()
    {

        if (startingPadPrefab != null && endingPadPrefab != null)
        {
            Stopwatch watch = new Stopwatch();



            watch.Start();
            //Get largest connected room
            MapGenerator.Room largestRoom = GetLargestRoom();
            watch.Stop();

            if (ShowDebug)
            {
                UnityEngine.Debug.Log("LargestRoom size: " + largestRoom.RoomSize);
                UnityEngine.Debug.Log("GetLargestRoom() time: " + watch.Elapsed);
            }


            List<MapGenerator.Coord> roomCoords = largestRoom.roomCoords;


            if (UIManager.Instance != null)
                UIManager.Instance.InflateLoadingScreen("Placing Start and Finish");




            //TODO -- Improve retrieval of coordinates with largest distance between them
            //Find first used row/column and last used row/column.
            //Randomly check coordinates with within that range, using increasing search region.
            //Keep coordinates with the largest distance
            /*
            bool[][] roomMap = new bool[largeLandMap.Length][];
            for (int i = 0; i < roomMap.Length; i++)
            {
                roomMap[i] = new bool[largeLandMap[0].Length];
            }


            watch.Reset();
            watch.Start();
            //Debug.Log(string.Format("{0}. Marking roomMap", Time.realtimeSinceStartup));
            for (int i = 0; i < roomCoords.Count; i++)
            {
                roomMap[roomCoords[i].tileX][roomCoords[i].tileY] = true;
            }

            watch.Stop();
            UnityEngine.Debug.Log("RoomMap tagging time: " + watch.Elapsed);
            */




            int startRow = int.MaxValue;
            int startCol = int.MaxValue;
            int endRow = 0;
            int endCol = 0;

            //Debug.Log(string.Format("{0}. Marking mapFlags", Time.realtimeSinceStartup));

            //Flag coordinates in largest room
            HashSet<IndexHolder> mapFlags = new HashSet<IndexHolder>();

  
            watch.Reset();
            watch.Start();
            
            for (int i = 0; i < largestRoom.roomCoords.Count; i++)
            {
                //if (i % 750 == 0)
                //{
                //    UIManager.Instance.InflateLoadingScreen((float)i / largestRoom.roomCoords.Count);
                //    yield return null;
                //}
                if (FrameRateTracker.Instance != null && FrameRateTracker.Instance.IsFrameDue())
                {
                    UIManager.Instance.InflateLoadingScreen((float)i / largestRoom.roomCoords.Count);
                    FrameRateTracker.Instance.Reset();
                    yield return null;
                }

                if (largestRoom.roomCoords[i].tileX < startCol)
                {
                    startCol = largestRoom.roomCoords[i].tileX;
                }
                if (largestRoom.roomCoords[i].tileX > endCol)
                {
                    endCol = largestRoom.roomCoords[i].tileX;
                }



                if (largestRoom.roomCoords[i].tileY < startRow)
                {
                    startRow = largestRoom.roomCoords[i].tileY;
                }
                if (largestRoom.roomCoords[i].tileY > endRow)
                {
                    endRow = largestRoom.roomCoords[i].tileY;
                }




                mapFlags.Add(new IndexHolder(largestRoom.roomCoords[i].tileX, largestRoom.roomCoords[i].tileY));
            }

            watch.Stop();

            if (ShowDebug)
                UnityEngine.Debug.Log("MapFlags tagging time: " + watch.Elapsed);




            int maxRows = endRow - startRow;
            int maxCols = endCol - startCol;


            float currentCheckPercentage = 0.01f;
            float percentageDelta = (.5f - currentCheckPercentage) / COORDINATE_CHECK_MAX;

          


            MapGenerator.Coord cA = roomCoords[pseudoRandom.Next(0, roomCoords.Count)];
            MapGenerator.Coord cB = roomCoords[pseudoRandom.Next(0, roomCoords.Count)];


            IndexHolder indexA = new IndexHolder(cA.tileX, cA.tileY);
            IndexHolder indexB = new IndexHolder(cB.tileX, cB.tileY);
            float largestDistance = Mathf.Sqrt(Mathf.Pow(indexB.Y - indexA.Y, 2) + Mathf.Pow(indexB.X - indexA.X, 2));


            Vector3 worldPos;



            /*
            Vector3 worldPos = transform.position - (Vector3.right * objectMap.Length / 2) - (Vector3.forward * objectMap[0].Length / 2);
            worldPos += Vector3.right * indexA.X;
            worldPos.x += 0.5f;
            worldPos += Vector3.forward * indexA.Y;
            worldPos.z += 0.5f;

            UnityEngine.Debug.DrawRay(worldPos + (Vector3.up * 50), Vector3.down * 50f, Color.magenta, 15f);


            worldPos = transform.position - (Vector3.right * objectMap.Length / 2) - (Vector3.forward * objectMap[0].Length / 2);
            worldPos += Vector3.right * indexB.X;
            worldPos.x += 0.5f;
            worldPos += Vector3.forward * indexB.Y;
            worldPos.z += 0.5f;

            UnityEngine.Debug.DrawRay(worldPos + (Vector3.up * 50), Vector3.down *  50f, Color.magenta, 15f);
            */


            //UnityEngine.Debug.Log(string.Format("Start: {0}, {1}. End: {2}, {3}.", startCol, startRow, endCol, endRow));
           // UnityEngine.Debug.Log(string.Format("IndexA: {0}, {1}. IndexB: {2}, {3}. Distance: {4}", indexA.X, indexA.Y, indexB.X, indexB.Y, largestDistance));

            //  Debug.Log(string.Format("{0}. Choosing furthest coordinates", Time.realtimeSinceStartup));


            watch.Reset();
            watch.Start();


            for (int i = 0; i < COORDINATE_CHECK_MAX; i++)
            {

                //if (i % 250 == 0)
                //{
                //    yield return null;
                //}
                if (FrameRateTracker.Instance != null && FrameRateTracker.Instance.IsFrameDue())
                {
                    FrameRateTracker.Instance.Reset();
                    yield return null;
                }

                int rowDifferential = (int)Math.Round(pseudoRandom.NextDouble() * currentCheckPercentage * maxRows);
                int colDifferential = (int)Math.Round(pseudoRandom.NextDouble() * currentCheckPercentage * maxCols);

                int r = pseudoRandom.NextDouble() >= 0.5f ? startRow + rowDifferential : endRow - rowDifferential;
                int c = pseudoRandom.NextDouble() >= 0.5f ? startCol + colDifferential : endCol - colDifferential;

                IndexHolder newIndex = new IndexHolder(c,r);

                /*
                worldPos = transform.position - (Vector3.right * objectMap.Length / 2) - (Vector3.forward * objectMap[0].Length / 2);
                worldPos += Vector3.right * c;
                worldPos.x += 0.5f;
                worldPos += Vector3.forward * r;
                worldPos.z += 0.5f;

                UnityEngine.Debug.DrawRay(worldPos + (Vector3.up * 50), Vector3.down, Color.magenta, 15f);
                */



                if (mapFlags.Contains(newIndex))
                {
                    float dist;
                    bool drawDebug = false;

                    if ((dist = Mathf.Sqrt(Mathf.Pow(r - indexA.Y, 2) + Mathf.Pow(c - indexA.X, 2))) > largestDistance)
                    {
                       // UnityEngine.Debug.Log(string.Format("Old distance: {0}. New distance: {1}.", largestDistance, dist));

                        largestDistance = dist;
                        indexB = newIndex;

                        drawDebug = true;
                    }
                    else if ((dist = Mathf.Sqrt(Mathf.Pow(r - indexB.Y, 2) + Mathf.Pow(c - indexB.X, 2))) > largestDistance)
                    {
                       // UnityEngine.Debug.Log(string.Format("Old distance: {0}. New distance: {1}.", largestDistance, dist));


                        largestDistance = dist;
                        indexA = newIndex;

                        drawDebug = true;
                    }


                    //if (ShowDebug && drawDebug)
                    //{
                    //    Vector3 worldPosA = transform.position - (Vector3.right * objectMap.Length / 2) - (Vector3.forward * objectMap[0].Length / 2);
                    //    worldPosA += Vector3.right * indexA.X;
                    //    worldPosA.x += 0.5f;
                    //    worldPosA += Vector3.forward * indexA.Y;
                    //    worldPosA.z += 0.5f;
                    //    worldPosA.y += 25;

                    //    Vector3 worldPosB = transform.position - (Vector3.right * objectMap.Length / 2) - (Vector3.forward * objectMap[0].Length / 2);
                    //    worldPosB += Vector3.right * indexB.X;
                    //    worldPosB.x += 0.5f;
                    //    worldPosB += Vector3.forward * indexB.Y;
                    //    worldPosB.z += 0.5f;
                    //    worldPosB.y += 25;

                    //    UnityEngine.Debug.DrawLine(worldPosA, worldPosB, Color.blue, 15f);
                    //}
                }

                currentCheckPercentage += percentageDelta;
            }


            watch.Stop();

            if (ShowDebug)
                UnityEngine.Debug.Log("Getting largest coordinates time: " + watch.Elapsed);

            //UnityEngine.Debug.Log(string.Format("IndexA: {0}, {1}. IndexB: {2}, {3}. Distance: {4}", indexA.X, indexA.Y, indexB.X, indexB.Y, largestDistance));


            /*
            worldPos = transform.position - (Vector3.right * objectMap.Length / 2) - (Vector3.forward * objectMap[0].Length / 2);
            worldPos += Vector3.right * indexA.X;
            worldPos.x += 0.5f;
            worldPos += Vector3.forward * indexA.Y;
            worldPos.z += 0.5f;

            UnityEngine.Debug.DrawRay(worldPos + (Vector3.up * 50), Vector3.down * 50f, Color.magenta, 30f);


            worldPos = transform.position - (Vector3.right * objectMap.Length / 2) - (Vector3.forward * objectMap[0].Length / 2);
            worldPos += Vector3.right * indexB.X;
            worldPos.x += 0.5f;
            worldPos += Vector3.forward * indexB.Y;
            worldPos.z += 0.5f;

            UnityEngine.Debug.DrawRay(worldPos + (Vector3.up * 50), Vector3.down * 50f, Color.magenta, 30f);
            */




            //  Debug.Log("Map flag count: " + mapFlags.Count);

            yield return null;

            //Find area to search for potential placement coordinates
            int neighborSpread = ((largeLandMap.Length + largeLandMap[0].Length) / 2) / 25;

            //Get potential candidates for both coordinates

            watch.Reset();
            watch.Start();

            List<MapGenerator.Coord> coordANeighbors = GetWalkableNeighbors(ref mapFlags, indexA, neighborSpread, true);
            List<MapGenerator.Coord> coordBNeighbors = GetWalkableNeighbors(ref mapFlags, indexB, neighborSpread,true);


            watch.Stop();


            if (ShowDebug)
            {
                UnityEngine.Debug.Log("GetWalkableNeighbors() time: " + watch.Elapsed);
                UnityEngine.Debug.Log("Neighbor count. A: " + coordANeighbors.Count + ". B: " + coordBNeighbors.Count);


                for (int q = 0; q < coordANeighbors.Count - 1; q++)
                {
                    worldPos = transform.position - (Vector3.right * objectMap.Length / 2) - (Vector3.forward * objectMap[0].Length / 2);
                    worldPos += Vector3.right * coordANeighbors[q].tileX;
                    worldPos.x += 0.5f;
                    worldPos += Vector3.forward * coordANeighbors[q].tileY;
                    worldPos.z += 0.5f;

                    UnityEngine.Debug.DrawRay(worldPos + (Vector3.up * 50), Vector3.down * 50f, Color.yellow, 30f);
                }
                for (int q = 0; q < coordBNeighbors.Count - 1; q++)
                {
                    worldPos = transform.position - (Vector3.right * objectMap.Length / 2) - (Vector3.forward * objectMap[0].Length / 2);
                    worldPos += Vector3.right * coordBNeighbors[q].tileX;
                    worldPos.x += 0.5f;
                    worldPos += Vector3.forward * coordBNeighbors[q].tileY;
                    worldPos.z += 0.5f;

                    UnityEngine.Debug.DrawRay(worldPos + (Vector3.up * 50), Vector3.down * 50f, Color.yellow, 30f);
                }
            }





            MapGenerator.Coord coordA = roomCoords[pseudoRandom.Next(0, roomCoords.Count)];
            MapGenerator.Coord coordB = roomCoords[pseudoRandom.Next(0, roomCoords.Count)];

            


            //Get extents of object
            Vector3 objBounds = Utilities.CalculateObjectBounds(startingPadPrefab, true);
            objBounds /= A_Star_Pathfinding.Instance.NodeDiameter;


            //Get random neighbor
            bool validPoint = false;
            int count = 0;


            watch.Reset();
            watch.Start();

            while(!validPoint && coordANeighbors.Count > 0)
            {

                //if (count % 1000 == 0)
                //    yield return null;

                if (FrameRateTracker.Instance != null && FrameRateTracker.Instance.IsFrameDue())
                {
                    FrameRateTracker.Instance.Reset();
                    yield return null;
                }


                validPoint = true;
                coordA = coordANeighbors[pseudoRandom.Next(coordANeighbors.Count)];


                for (int a = coordA.tileX - (int)objBounds.x; a <= coordA.tileX + (int)objBounds.x; a++)
                {
                    for (int b = coordA.tileY - (int)objBounds.z; b <= coordA.tileY + (int)objBounds.z; b++)
                    {
                        if (!(a >= 0 && a < largeLandMap.Length && b >= 0 && b < largeLandMap[0].Length) || !largeLandMap[a][b])
                        {
                            validPoint = false;
                            break;
                        }
                    }
                }


                if (!validPoint && coordANeighbors.Contains(coordA))
                {
                    coordANeighbors.Remove(coordA);
                }

                count++;
            }

            watch.Stop();

            if (ShowDebug)
                UnityEngine.Debug.Log("Getting coordA time: " + watch.Elapsed);


            if (!validPoint)
            {
                UnityEngine.Debug.Log("ERROR -- Could not find coordA");

                coordA = roomCoords[pseudoRandom.Next(0, roomCoords.Count)];
            }




            objBounds = Utilities.CalculateObjectBounds(endingPadPrefab, true);
            objBounds /= A_Star_Pathfinding.Instance.NodeDiameter;
            validPoint = false;
            count = 0;

            
            watch.Reset();
            watch.Start();


            while (!validPoint && coordBNeighbors.Count > 0)
            {

                //if (count % 1000 == 0)
                //    yield return null;

                if (FrameRateTracker.Instance != null && FrameRateTracker.Instance.IsFrameDue())
                {
                    FrameRateTracker.Instance.Reset();
                    yield return null;
                }


                validPoint = true;
                coordB = coordBNeighbors[pseudoRandom.Next(coordBNeighbors.Count)];


                for (int a = coordB.tileX - (int)objBounds.x; a <= coordB.tileX + (int)objBounds.x; a++)
                {
                    for (int b = coordB.tileY - (int)objBounds.z; b <= coordB.tileY + (int)objBounds.z; b++)
                    {
                        if (!(a >= 0 && a < largeLandMap.Length && b >= 0 && b < largeLandMap[0].Length) || !largeLandMap[a][b])
                        {
                            validPoint = false;
                            break;
                        }
                    }
                }


                if (!validPoint && coordBNeighbors.Contains(coordB))
                {
                    coordBNeighbors.Remove(coordB);
                }

                count++;
            }

            watch.Stop();

            if (ShowDebug)
                UnityEngine.Debug.Log("Getting coordB time: " + watch.Elapsed);



            if (!validPoint)
            {
                UnityEngine.Debug.Log("ERROR -- Could not find coordB");
                coordB = roomCoords[pseudoRandom.Next(0, roomCoords.Count)];
            }

            //Debug.Log(string.Format("CoordA: {0}. CoordB: {1}", coordA, coordB));



            //Potentially swap start and end point
            MapGenerator.Coord startCoord, endCoord;
            if (pseudoRandom.NextDouble() <= 0.5f)
            {
                startCoord = coordA;
                endCoord = coordB;
            }
            else
            {
                endCoord = coordA;
                startCoord = coordB;
            }
            

            //Place objects
            Vector2 errorVector = new Vector2((float)(pseudoRandom.NextDouble() * 2) - 1, (float)(pseudoRandom.NextDouble() * 2) - 1) * 0.5f; 
            
            Vector3 startGoalPosition = transform.position - (Vector3.right * objectMap.Length / 2) - (Vector3.forward * objectMap[0].Length / 2);
            startGoalPosition += Vector3.right * startCoord.tileX;
            startGoalPosition.x += 0.5f;
            startGoalPosition += Vector3.forward * startCoord.tileY;
            startGoalPosition.z += 0.5f;
            startGoalPosition += new Vector3(errorVector.x, 0, errorVector.y);
            startGoalPosition.y += objBounds.y / 2;
            startGoalPosition.y += HeightFromWorldPosition(startGoalPosition) + PLACEMENT_HEIGHT_OFFSET;

            errorVector = new Vector2((float)(pseudoRandom.NextDouble() * 2) - 1, (float)(pseudoRandom.NextDouble() * 2) - 1) * 0.5f; 

            Vector3 endGoalPosition = transform.position - (Vector3.right * objectMap.Length / 2) - (Vector3.forward * objectMap[0].Length / 2);
            endGoalPosition += Vector3.right * endCoord.tileX;
            endGoalPosition.x += 0.5f;
            endGoalPosition += Vector3.forward * endCoord.tileY;
            endGoalPosition.z += 0.5f;
            endGoalPosition += new Vector3(errorVector.x, 0, errorVector.y);
            endGoalPosition.y += objBounds.y / 2;
            endGoalPosition.y += HeightFromWorldPosition(endGoalPosition) + PLACEMENT_HEIGHT_OFFSET;

            Quaternion placementQuaternion = Quaternion.AngleAxis((float)(pseudoRandom.NextDouble() * 360) - 180f, Vector3.up); //Quaternion.AngleAxis(UnityEngine.Random.Range(-180f, 180f), Vector3.up);
            GameObject _startGoal = Instantiate(startingPadPrefab, startGoalPosition, placementQuaternion) as GameObject;
            placementQuaternion = Quaternion.AngleAxis((float)(pseudoRandom.NextDouble() * 360) - 180f, Vector3.up);
            GameObject _endGoal = Instantiate(endingPadPrefab, endGoalPosition, placementQuaternion) as GameObject;

            _startGoal.transform.SetParent(genObjParent.transform);
            _endGoal.transform.SetParent(genObjParent.transform);

            objBounds = Utilities.CalculateObjectBounds(startingPadPrefab, true);
            for (int p = startCoord.tileX - ((int)objBounds.x + 0); p <= startCoord.tileX + ((int)objBounds.x + 0); p++)
            {
                for (int q = startCoord.tileY - ((int)objBounds.z + 0); q <= startCoord.tileY + ((int)objBounds.z + 0); q++)
                {

                    if (p >= 0 && p < objectMap.Length && q >= 0 && q < objectMap[0].Length)
                        objectMap[p][q] = -1;
                }
            }



            objBounds = Utilities.CalculateObjectBounds(endingPadPrefab, true);
            for (int p = endCoord.tileX - ((int)objBounds.x + 5); p <= endCoord.tileX + ((int)objBounds.x + 5); p++)
            {
                for (int q = endCoord.tileY - ((int)objBounds.z + 5); q <= endCoord.tileY + ((int)objBounds.z + 5); q++)
                {

                    if (p >= 0 && p < objectMap.Length && q >= 0 && q < objectMap[0].Length)
                        objectMap[p][q] = -1;
                }
            }


            _startGoal.SetActive(true);
            _endGoal.SetActive(true);

            StartGoalTransform = _startGoal.transform;
            EndGoalTransform = _endGoal.transform;
        }

    }

    IEnumerator PlaceEnvironmentRegions()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.InflateLoadingScreen("Placing Regions");

        yield return null;

        string _name;
        GameObject _parent;

        UnityEngine.Object[] _objects = Resources.LoadAll(REGION_FOLDER_PATH, typeof(ProceduralRegion));
        ProceduralRegion[] environmentRegions = new ProceduralRegion[_objects.Length];

        for (int i = 0; i < _objects.Length; i++)
        {
            environmentRegions[i] = (ProceduralRegion)_objects[i];
        }

        Array.Sort(environmentRegions, delegate (ProceduralRegion r1, ProceduralRegion r2)
        {
            return r1.Compare(r2);
        });


        for (int i = 0; i < environmentRegions.Length; i++)
        {
            yield return null;

            Stopwatch timer = new Stopwatch();





            _name = environmentRegions[i].gameObject.name;
            _parent = new GameObject(_name + " Parent");
            _parent.transform.SetParent(genObjParent.transform);



            List<ProceduralRegion.NodeTypeProbability> companionNodes = environmentRegions[i].CompanionNodes;
            float climateMultiplier = environmentRegions[i].GetClimateMultiplier(currentTemperature, currentMoisture);
            climateMultiplier = Mathf.Clamp01(climateMultiplier);



            if (ShowDebug)
            {
                UnityEngine.Debug.Log("Placing region " + environmentRegions[i].gameObject.name + ". Climate multiplier: " + climateMultiplier);
            }


            bool[][] regionPlacementMap = new bool[largeLandMap.Length][];
            for (int p = 0; p < regionPlacementMap.Length; p++)
            {
                regionPlacementMap[p] = new bool[largeLandMap[0].Length];
            }


            timer.Start();
            //Check generated maps for viable region points
            for (int p = 0; p < regionPlacementMap.Length; p++)
            {
                for (int q = 0; q < regionPlacementMap[p].Length; q++)
                {
                    if (FrameRateTracker.Instance != null && FrameRateTracker.Instance.IsFrameDue())
                    {
                        FrameRateTracker.Instance.Reset();
                        yield return null;
                    }




                    if (environmentRegions[i].NeedsLand && !largeLandMap[p][q])
                    {
                        continue;
                    }

                    //Should region be placed on this node?
                    bool validPoint = true;

                    //Check if current region can mix with established region
                    if (regionMap != null)
                    {
                        for (int k = 0; k < companionNodes.Count; k++)
                        {
                            if (regionMap[p][q] == companionNodes[k].Type)
                            {
                                validPoint &= (float)pseudoRandom.NextDouble() <= (companionNodes[k].GetProbability(largeHeightMap[p][q]) * climateMultiplier);
                            }
                            else
                            {
                                validPoint = false;
                            }
                        }
                    }


                    if (validPoint)
                    {
                        regionPlacementMap[p][q] = true;
                    }

                }
            }

            timer.Stop();

            if (ShowDebug)
            {
                UnityEngine.Debug.Log("Creating placement map time: " + timer.Elapsed);
            }

            timer.Reset();
            timer.Start();

            //Smooth Region
            MapGenerator.SmoothRegion(ref regionPlacementMap, environmentRegions[i].SmoothingLevel);
            MapGenerator.RestrictRoomSize(ref regionPlacementMap, MINIMUM_REGION_SIZE);

            timer.Stop();

            if (ShowDebug)
            {
                UnityEngine.Debug.Log("SmoothRegion() time: " + timer.Elapsed);
            }



            timer.Reset();
            timer.Start();

            //Sign RegionMap
            if (environmentRegions[i].ShouldSignMap)
            {
                for (int p = 0; p < regionPlacementMap.Length; p++)
                {
                    for (int q = 0; q < regionPlacementMap[0].Length; q++)
                    {
                        if (FrameRateTracker.Instance != null && FrameRateTracker.Instance.IsFrameDue())
                        {
                            FrameRateTracker.Instance.Reset();
                            yield return null;
                        }


                        if (regionPlacementMap[p][q])
                        {
                            regionMap[p][q] |= environmentRegions[i].EnvironmentType;
                        }

                    }
                }
            }


            timer.Stop();

            if (ShowDebug)
            {
                UnityEngine.Debug.Log("Signing region map time: " + timer.Elapsed);
            }

            int lengthA = (int)(largeLandMap.Length / (float)environmentRegions[i].ScaleFactor);
            int lengthB = (int)(largeLandMap[0].Length / (float)environmentRegions[i].ScaleFactor);



            timer.Reset();
            timer.Start();

            //Scale to smaller grid
            bool[][] smallRegionPlacementMap = new bool[lengthA][];
            for (int q = 0; q < smallRegionPlacementMap.Length; q++)
            {
                smallRegionPlacementMap[q] = new bool[lengthB];

                for (int k = 0; k < smallRegionPlacementMap[q].Length; k++)
                {
                    if (FrameRateTracker.Instance != null && FrameRateTracker.Instance.IsFrameDue())
                    {
                        FrameRateTracker.Instance.Reset();
                        yield return null;
                    }



                    int x = (int)((q / (float)smallRegionPlacementMap.Length) * regionPlacementMap.Length);
                    int y = (int)((k / (float)smallRegionPlacementMap[q].Length) * regionPlacementMap[0].Length);

                    if (x >= regionPlacementMap.Length)
                        x = regionPlacementMap.Length - 1;

                    if (y >= regionPlacementMap[0].Length)
                        y = regionPlacementMap[0].Length - 1;


                    smallRegionPlacementMap[q][k] = regionPlacementMap[x][y];
                }
            }

            timer.Stop();

            if (ShowDebug)
            {
                UnityEngine.Debug.Log("Scaling region placement map time: " + timer.Elapsed);
            }

            timer.Reset();
            timer.Start();

            //Double check to insure proper placement
            for (int q = 0; q < regionPlacementMap.Length; q++)
            {
                for (int k = 0; k < regionPlacementMap[q].Length; k++)
                {
                    if (FrameRateTracker.Instance != null && FrameRateTracker.Instance.IsFrameDue())
                    {
                        FrameRateTracker.Instance.Reset();
                        yield return null;
                    }


                    if (regionPlacementMap[q][k])
                        continue;



                    int x = (int)((q / (float)regionPlacementMap.Length) * smallRegionPlacementMap.Length);
                    int y = (int)((k / (float)regionPlacementMap[q].Length) * smallRegionPlacementMap[0].Length);

                    if (x >= smallRegionPlacementMap.Length)
                        x = smallRegionPlacementMap.Length - 1;

                    if (y >= smallRegionPlacementMap[0].Length)
                        y = smallRegionPlacementMap[0].Length - 1;


                    smallRegionPlacementMap[x][y] = false;
                }
            }

            timer.Stop();

            if (ShowDebug)
            {
                UnityEngine.Debug.Log("Double checking placement time: " + timer.Elapsed);
            }

            timer.Reset();
            timer.Start();

            //Generate individual region "rooms"
            List<MapGenerator.Room> _rooms = MapGenerator.GetRooms(ref smallRegionPlacementMap);

            timer.Stop();

            if (ShowDebug)
            {
                UnityEngine.Debug.Log("Getting room time: " + timer.Elapsed);
            }


            timer.Reset();
            timer.Start();
            
            TimeSpan getObjectTime = new TimeSpan();
     

            bool[][][] roomMaps = new bool[_rooms.Count][][];
            for (int q = 0; q < _rooms.Count; q++)
            {
                yield return null;
                
                roomMaps[q] = new bool[smallRegionPlacementMap.Length][];

                for (int k = 0; k < roomMaps[q].Length; k++)
                {
                    roomMaps[q][k] = new bool[smallRegionPlacementMap[0].Length];
                }


                for (int k = 0; k < _rooms[q].roomCoords.Count; k++)
                {
                    roomMaps[q][_rooms[q].roomCoords[k].tileX][_rooms[q].roomCoords[k].tileY] = true;
                }



                Stopwatch sw = new Stopwatch();
                sw.Start();

                //Generate Object based on regionPlacementMap
                GameObject _obj = environmentRegions[i].GetObject(roomMaps[q], environmentRegions[i].ScaleFactor);
                sw.Stop();
                getObjectTime += sw.Elapsed;

                ElevateRegion(_obj);


                /*
                Vector3[] cornerPositions = new Vector3[4];

                //Start, Start
                cornerPositions[0] = (Vector3.right * A_Star_Pathfinding.Instance.NodeDiameter * (objectMap.Length / 2f)) - (Vector3.forward * A_Star_Pathfinding.Instance.NodeDiameter * (objectMap[0].Length / 2f));
                cornerPositions[0] += Vector3.right * A_Star_Pathfinding.Instance.NodeDiameter * _rooms[q].StartColumn;
                cornerPositions[0].x += A_Star_Pathfinding.Instance.NodeRadius;
                cornerPositions[0] += Vector3.forward * A_Star_Pathfinding.Instance.NodeDiameter * _rooms[q].StartRow;
                cornerPositions[0].z += A_Star_Pathfinding.Instance.NodeRadius;

                //Start, End
                cornerPositions[1] = (Vector3.right * A_Star_Pathfinding.Instance.NodeDiameter * (objectMap.Length / 2f)) - (Vector3.forward * A_Star_Pathfinding.Instance.NodeDiameter * (objectMap[0].Length / 2f));
                cornerPositions[1] += Vector3.right * A_Star_Pathfinding.Instance.NodeDiameter * _rooms[q].StartColumn;
                cornerPositions[1].x += A_Star_Pathfinding.Instance.NodeRadius;
                cornerPositions[1] += Vector3.forward * A_Star_Pathfinding.Instance.NodeDiameter * _rooms[q].EndRow;
                cornerPositions[1].z += A_Star_Pathfinding.Instance.NodeRadius;

                //End, Start
                cornerPositions[2] = (Vector3.right * A_Star_Pathfinding.Instance.NodeDiameter * (objectMap.Length / 2f)) - (Vector3.forward * A_Star_Pathfinding.Instance.NodeDiameter * (objectMap[0].Length / 2f));
                cornerPositions[2] += Vector3.right * A_Star_Pathfinding.Instance.NodeDiameter * _rooms[q].EndColumn;
                cornerPositions[2].x += A_Star_Pathfinding.Instance.NodeRadius;
                cornerPositions[2] += Vector3.forward * A_Star_Pathfinding.Instance.NodeDiameter * _rooms[q].StartRow;
                cornerPositions[2].z += A_Star_Pathfinding.Instance.NodeRadius;

                //End, End
                cornerPositions[3] = (Vector3.right * A_Star_Pathfinding.Instance.NodeDiameter * (objectMap.Length / 2f)) - (Vector3.forward * A_Star_Pathfinding.Instance.NodeDiameter * (objectMap[0].Length / 2f));
                cornerPositions[3] += Vector3.right * A_Star_Pathfinding.Instance.NodeDiameter * _rooms[q].EndColumn;
                cornerPositions[3].x += A_Star_Pathfinding.Instance.NodeRadius;
                cornerPositions[3] += Vector3.forward * A_Star_Pathfinding.Instance.NodeDiameter * _rooms[q].EndRow;
                cornerPositions[3].z += A_Star_Pathfinding.Instance.NodeRadius;


                Vector3 pos = cornerPositions[0] + cornerPositions[1] + cornerPositions[2] + cornerPositions[3];
                pos /= 4f;

                _obj.transform.position = transform.TransformPoint(pos);
                */

                _obj.transform.position = transform.position;
                _obj.transform.SetParent(_parent.transform,true);
                _obj.SetActive(true);
            }

            timer.Stop();

            if (ShowDebug)
            {
                if(_rooms.Count > 0)
                {
                UnityEngine.Debug.Log("Constructing individual rooms (" + _rooms.Count + ") time: " + timer.Elapsed + ". Average GetObject time: " + new TimeSpan(getObjectTime.Ticks /  _rooms.Count));
                }
                else
                {
                    UnityEngine.Debug.Log("Constructing individual rooms (" + _rooms.Count + ") time: " + timer.Elapsed);
                }
            }


            if (_parent.transform.childCount == 0)
                Destroy(_parent);


            UIManager.Instance.InflateLoadingScreen((i + 1) / (float)environmentRegions.Length);
        }
    }

    IEnumerator PlaceEnvironmentObjects()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.InflateLoadingScreen("Placing Environment Objects");

        yield return null;



        UnityEngine.Object[] _objects = Resources.LoadAll(ENVIRONMENT_OBJECT_FOLDER_PATH, typeof(ProceduralObject));
        ProceduralObject[] environmentObjects = new ProceduralObject[_objects.Length];

        for (int i = 0; i < _objects.Length; i++)
        {
            environmentObjects[i] = _objects[i] as ProceduralObject;
        }


        Array.Sort(environmentObjects, delegate (ProceduralObject o1, ProceduralObject o2) {
            return o1.Compare(o2);
        });

        yield return StartCoroutine(PlaceObjects(environmentObjects));
    }

    IEnumerator PlaceBonusObjects()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.InflateLoadingScreen("Placing Bonus Objects");

        yield return null;



        UnityEngine.Object[] _objects = Resources.LoadAll(BONUS_OBJECT_PATH, typeof(ProceduralObject));
        ProceduralObject[] bonusObjects = new ProceduralObject[_objects.Length];

        for (int i = 0; i < _objects.Length; i++)
        {
            bonusObjects[i] = _objects[i] as ProceduralObject;
        }


        Array.Sort(bonusObjects, delegate (ProceduralObject o1, ProceduralObject o2) {
            return o1.Compare(o2);
        });

        yield return StartCoroutine(PlaceObjects(bonusObjects));
    }

    IEnumerator PlaceCreatures()
    {

        if (UIManager.Instance != null)
            UIManager.Instance.InflateLoadingScreen("Placing Creatures");

        yield return null;



        UnityEngine.Object[] _objects = Resources.LoadAll(CREATURE_FOLDER_PATH, typeof(ProceduralObject));
        ProceduralObject[] environmentCreatures = new ProceduralObject[_objects.Length];

        for (int i = 0; i < _objects.Length; i++)
        {
            environmentCreatures[i] = _objects[i] as ProceduralObject;
        }


        Array.Sort(environmentCreatures, delegate (ProceduralObject o1, ProceduralObject o2) {
            return o1.Compare(o2);
        });

        yield return StartCoroutine(PlaceObjects(environmentCreatures));
    }

    //Place Objects into game world
    IEnumerator PlaceObjects(ProceduralObject[] _objects)
    {
        if (ShowDebug)
        {
            UnityEngine.Debug.Log(string.Format("{0} objects to place.", _objects.Length));
        }


        string _name;
        GameObject _parent;
        Vector2 mapDimensions = MapGenerator.Instance.MapDimensions_Scaled;



        for (int i = 0; i < _objects.Length; i++)
        {
            if (_objects[i] == null)
                continue;

            //if (i % 50 == 0)
            //    yield return null;
            if (FrameRateTracker.Instance != null && FrameRateTracker.Instance.IsFrameDue())
            {
                FrameRateTracker.Instance.Reset();
                yield return null;
            }


            //Get maximum number of objects to place
            int maxToPlace = (int)(_objects[i].GetPopulationPercentage(currentTemperature, currentMoisture) * mapDimensions.x * mapDimensions.y);

            if (maxToPlace > _objects[i].PopulationMax)
            {
                maxToPlace = _objects[i].PopulationMax;
            }

            if (maxToPlace == 0)
                continue;




            int maxRandomChecks = (int)(maxToPlace * PLACEMENT_POPULATION_MAX_MULTIPLIER);
            int numPlaced = 0;


           

            if (ShowDebug)
            {
                UnityEngine.Debug.Log(string.Format("Placing Object #{0} --- {1} --- Climate Rating: {2}. Max to place: {3}. Max Checks: {4}.", i, _objects[i].gameObject.name, _objects[i].GetClimateRating(currentTemperature, currentMoisture), maxToPlace, maxRandomChecks));
            }



            GameObject objectPrefab = _objects[i].gameObject;



            _name = objectPrefab.name;
            _parent = new GameObject(_name + " Parent");
            _parent.transform.SetParent(genObjParent.transform);


            NodeType placementNodes = _objects[i].ViablePlacementNodes;


            List<int> companionIDS = _objects[i].CompanionIDs;



            //Instantiate object and set parent
            GameObject holderObj = null;
            PlacementGrid _grid = new PlacementGrid();
            bool[][] _gridMask = null;

            

            int k = 0;


            //Place max # of objects or until count is reached
            for (k = 0; k < maxRandomChecks && numPlaced < maxToPlace; k++)
            {
                if (FrameRateTracker.Instance != null && FrameRateTracker.Instance.IsFrameDue())
                {
                    FrameRateTracker.Instance.Reset();
                    yield return null;
                }



                if (holderObj == null)
                {
                    holderObj = _objects[i].GetObject(currentTemperature, currentMoisture);

                    Vector3 euler = holderObj.transform.eulerAngles;
                    euler.y = (float)(pseudoRandom.NextDouble() * 360f);
                    holderObj.transform.eulerAngles = euler;
                    


                    _grid = CalculatePlacementGrid(holderObj);
                    _gridMask = _grid.occupyMask;

                    int _width = (int)(_gridMask.Length / 2f);

                    if (_width <= 0)
                        _width = 1;

                    int _height = (int)(_gridMask[0].Length / 2f);

                    if (_height <= 0)
                        _height = 1;


                    bool[][] _newMask = new bool[_width][];
                    for (int a = 0; a < _newMask.Length; a++)
                    {
                        _newMask[a] = new bool[_height];
                        for (int b = 0; b < _newMask[a].Length; b++)
                        {
                            int _tempX = (int)(((float)a / _width) * _gridMask.Length);
                            int _tempY = (int)(((float)b / _height) * _gridMask[0].Length);

                            _newMask[a][b] = _gridMask[_tempX][_tempY];
                        }
                    }

                    _gridMask = _newMask;


                    // Vector3 _bounds = Utilities.CalculateObjectBounds(holderObj, 1, false);
                    //Debug.Log(holderObj.name + " ." + string.Format("Object bounds: {0} x {1} x {2}. GridMask Dimensions: {3} x {4}", _bounds.x, _bounds.y, _bounds.z, _gridMask.Length, _gridMask[0].Length));

                }


                //Choose random point
                int randX = pseudoRandom.Next(largeHeightMap.Length);
                int randY = pseudoRandom.Next(largeHeightMap[0].Length);


                //Should object be placed at this height?
                if (((float)pseudoRandom.NextDouble() >= _objects[i].GetProbability(largeHeightMap[randX][randY])) || _gridMask == null)
                {
                    continue;
                }


                bool validPoint = true;

                int xDisparity = Mathf.RoundToInt(_gridMask.Length / 2f);
                int yDisparity = Mathf.RoundToInt(_gridMask[0].Length / 2f);




                for (int a = 0; a < _gridMask.Length; a++)
                {

                    if (!validPoint)
                        break;

                    for (int b = 0; b < _gridMask[a].Length; b++)
                    {

                        if (!validPoint)
                            break;


                        if (!_gridMask[a][b])
                            continue;



                        int tempX_Object = randX - xDisparity + a;
                        int tempY_Object = randY - yDisparity + b;


                        //Convert (a,b) to regionMap coordinates
                        int tempX_Region = tempX_Object;// (int)(((float)tempX_Object / objectMap.Length) * regionMap.Length);
                        int tempY_Region = tempY_Object;// (int)(((float)tempY_Object / objectMap[0].Length) * regionMap[0].Length);



                        if (tempX_Object < 0 || tempY_Object < 0 || tempX_Object >= objectMap.Length || tempY_Object >= objectMap[0].Length)
                        {
                            validPoint = false;
                        }
                        if (tempX_Region < 0 || tempY_Region < 0 || tempX_Region >= regionMap.Length || tempY_Region >= regionMap[0].Length)
                        {
                            validPoint = false;
                        }



                        if (!validPoint)
                            break;



                        //Is at proper height?
                        validPoint = _objects[i].IsValidHeight(largeHeightMap[tempX_Object][tempY_Object]);


                        if (!validPoint)
                            break;


                        //Is occupied by a region?
                        validPoint = Utilities.HasFlag(placementNodes, regionMap[tempX_Region][tempY_Region]);


                        if (!validPoint)
                            break;


                        //Debug.Log(string.Format("Valid Point. {0} ---- Placement Nodes: {1}. Region Map: {2}", _name, placementNodes.ToString(), regionMap[tempX_Region][tempY_Region].ToString()));




                        //Should check for other objects in area?
                        if (_objects[i].ShouldCheckArea && objectMap[tempX_Object][tempY_Object] != 0)
                        {
                            validPoint = false;

                            //Check if other object IDs are companions
                            for (int z = 0; z < companionIDS.Count; z++)
                            {
                                if (objectMap[a][b] == companionIDS[z])
                                {
                                    validPoint = true;
                                }
                            }

                        }

                    }
                }



                if (!validPoint)
                    continue;


                //Sign ObjectMap
                if (_objects[i].ShouldLeaveMark)
                {
                    for (int a = 0; a < _gridMask.Length; a++)
                    {
                        for (int b = 0; b < _gridMask[a].Length; b++)
                        {
                            if (!_gridMask[a][b])
                                continue;


                            int tempX_Object = randX - xDisparity + a;
                            int tempY_Object = randY - yDisparity + b;

                            objectMap[tempX_Object][tempY_Object] = -1;// _objects[i].ObjectID;
                        }
                    }
                }




                //Place object randomly within chosen square and with random rotation
                //TODO --- fix to rely on A_STAR.NodeRadius rather than 0.5f
                Vector2 errorVector = new Vector2((float)(pseudoRandom.NextDouble() * 2) - 1, (float)(pseudoRandom.NextDouble() * 2) - 1) * 0.5f;    //UnityEngine.Random.insideUnitCircle * 0.5f;
                Vector3 worldPos = transform.position - (Vector3.right * ((objectMap.Length / 2f) + 0.5f)) - (Vector3.forward * ((objectMap[0].Length / 2f) + 0.5f));
                worldPos.x += randX + 0.5f;
                worldPos.z += randY + 0.5f;
                worldPos += new Vector3(errorVector.x, 0, errorVector.y);
                worldPos.y = HeightFromWorldPosition(worldPos) + PLACEMENT_HEIGHT_OFFSET;

                if (_objects[i].ChanceAlignWithGround > 0f && pseudoRandom.NextDouble() <= _objects[i].ChanceAlignWithGround)
                {
                   
                    RaycastHit hit;
                    Ray checkRay = new Ray(worldPos + Vector3.up, Vector3.down);
                    
                    if (Physics.Raycast(checkRay, out hit, 2f, groundMask))
                    {
                        holderObj.transform.rotation = Quaternion.LookRotation(holderObj.transform.forward, hit.normal);
                    }
                }


                holderObj.transform.position = worldPos;
                holderObj.transform.SetParent(_parent.transform,true);
                holderObj.SetActive(true);
                


                holderObj = null;

                numPlaced++;

            }



            if (holderObj != null)
                Destroy(holderObj);

            if (_parent.transform.childCount == 0)
            {
                Destroy(_parent);
            }

            if (_parent.transform.childCount > 0 && ShowDebug)
            {
                UnityEngine.Debug.Log(_name + " count: " + _parent.transform.childCount);
            }

            UIManager.Instance.InflateLoadingScreen((i+1) / (float)_objects.Length);
        }
    }



    void PlaceGodFavors()
    {
        if (GameManager.Instance == null)
            return;


        float _favor = GameManager.Instance.DeityFavor;

        if (ShowDebug)
            UnityEngine.Debug.Log("God Favor: " + _favor);


    }

    #endregion




    void ElevateRegion(GameObject obj)
    {
        MeshFilter _filter = obj.GetComponent<MeshFilter>();
        MeshCollider _coll = obj.GetComponent<MeshCollider>();

        if (_filter == null)
            return;


        Mesh _mesh = _filter.mesh;

        Vector3[] _vertices = _mesh.vertices;

        for (int i = 0; i < _vertices.Length; i++)
        {
            _vertices[i].y = HeightFromWorldPosition(_vertices[i]) + PROCEDURAL_REGION_OFFSET;
        }

        _mesh.vertices = _vertices;

        if (_coll != null)
            _coll.sharedMesh = _mesh;
    }


    #region Land Detection

    /*
    IEnumerator DetectLandMap()
    {

        yield return null;

        Vector3 origin = transform.position - (Vector3.right * (scaledLandMap.Length / 2)) - (Vector3.forward * (scaledLandMap[0].Length / 2));
        int landCount = 0;
       
        bool[][] newLandMap = new bool[scaledLandMap.Length][];


        LayerMask groundMask = LayerMask.GetMask("Ground");
        Vector3 worldPos;
        RaycastHit hit;

        for (int x = 0; x < scaledLandMap.Length; x++)
        {

            newLandMap[x] = new bool[scaledLandMap[0].Length];


            for (int y = 0; y < scaledLandMap[0].Length; y++)
            {
               


                worldPos = origin + (Vector3.right * (x + 0.5f)) + (Vector3.forward * (y + 0.5f));
                worldPos.y = 1000;
                Physics.SphereCast(worldPos, 0.5f, Vector3.down, out hit, 1001, groundMask);

                newLandMap[x][y] = hit.collider != null ;

                if (newLandMap[x][y])
                    landCount++;
            }

            if(x % 100 == 0)
            {
                yield return null;
                
            }
        }
        

        scaledLandMap = newLandMap;
    }
    */


    MapGenerator.Room GetLargestRoom()
    {
        List<MapGenerator.Room> roomRegions = GetRoomsOfType(1);


        if (roomRegions.Count == 0)
            return null;

        MapGenerator.Room largestRoom = roomRegions[0];

        for (int i = 1; i < roomRegions.Count; i++)
        {
            if (roomRegions[i].RoomSize > largestRoom.RoomSize)
            {
                largestRoom = roomRegions[i];
            }
        }

        return largestRoom;
    }

    List<MapGenerator.Room> GetRoomsOfType(int tileType)
    {
        List<MapGenerator.Room> rooms = new List<MapGenerator.Room>();
        bool[][] mapFlags = new bool[largeLandMap.Length][];
        for (int i = 0; i < mapFlags.Length; i++)
        {
            mapFlags[i] = new bool[largeLandMap[0].Length];
        }

        for (int x = 0; x < mapFlags.Length; x++)
        {
            for (int y = 0; y < mapFlags[0].Length; y++)
            {
                if (!mapFlags[x][y] && largeLandMap[x][y])
                {


                    MapGenerator.Room newRoom = GetRoom(x, y);
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

    MapGenerator.Room GetRoom(int startX, int startY)
    {

        List<MapGenerator.Coord> tiles = new List<MapGenerator.Coord>();

        int[][] mapFlags = new int[largeLandMap.Length][];
        for (int i = 0; i < mapFlags.Length; i++)
        {
            mapFlags[i] = new int[largeLandMap[0].Length];
        }

        bool tileType = largeLandMap[startX][startY];

        Queue<MapGenerator.Coord> queue = new Queue<MapGenerator.Coord>();
        queue.Enqueue(new MapGenerator.Coord(startX, startY));
        mapFlags[startX][startY] = 1;

        while (queue.Count > 0)
        {
            MapGenerator.Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (x >= 0 && x < largeLandMap.Length && y >= 0 && y < largeLandMap[0].Length)
                    {
                        if (mapFlags[x][y] == 0 && largeLandMap[x][y] == tileType)
                        {
                            mapFlags[x][y] = 1;
                            queue.Enqueue(new MapGenerator.Coord(x, y));
                        }
                    }
                }
            }
        }
        

        return new MapGenerator.Room(tiles);
    }

    
    List<MapGenerator.Coord> GetWalkableNeighbors(ref HashSet<IndexHolder> roomMap, IndexHolder centerIndex, int spread, bool includeCenter)
    {
        List<MapGenerator.Coord> coords = GetWalkableNeighbors(ref roomMap, centerIndex, spread);// centerIndex.X, centerIndex.Y, spread);

        if (includeCenter && roomMap.Contains(centerIndex))
        {
            coords.Add(new MapGenerator.Coord(centerIndex.X, centerIndex.Y));
        }

        return coords;
    }
    
    /*
    List<MapGenerator.Coord> GetWalkableNeighbors(MapGenerator.Coord centerCoord, int spread)
    {
        return GetWalkableNeighbors(centerCoord.tileX, centerCoord.tileY, spread);
    }
    */
    List<MapGenerator.Coord> GetWalkableNeighbors(ref HashSet<IndexHolder> roomMap, IndexHolder centerIndex, int spread)
    {
        List<MapGenerator.Coord> neighbors = new List<MapGenerator.Coord>();

        for (int i = centerIndex.X - spread; i <= centerIndex.X + spread; i++)
        {
            for (int j = centerIndex.Y - spread; j <= centerIndex.Y + spread; j++)
            {
                if (i >= 0 && i < largeLandMap.Length && j >= 0 && j < largeLandMap[0].Length && !(i == centerIndex.X && j == centerIndex.Y) && roomMap.Contains(new IndexHolder(i,j)))// && largeLandMap[i][j])
                {
                    neighbors.Add(new MapGenerator.Coord(i, j));
                }
            }
        }

        return neighbors;
    }

    #endregion


    float HeightFromWorldPosition(Vector3 worldPos)
    {
        RaycastHit hit;
        float _height = 0f;

        worldPos.y = 100;


        Collider coll = groundObject.GetComponent<Collider>();
        if (coll.Raycast(new Ray(worldPos, Vector3.down), out hit, worldPos.y * 1.2f))
        {
            _height = hit.point.y;
        }

        // float _height = MapGenerator.Instance.WorldPointToElevation(worldPos);
        //Debug.DrawLine(worldPos, worldPos + Vector3.up * _height, Color.yellow, 20f);

        return _height;
    }





    public struct PlacementGrid
    {
        public Vector3 bounds;
        public bool[][] occupyMask;
    }


    public PlacementGrid CalculatePlacementGrid(GameObject obj)
    {
        if (obj == null)
            return new PlacementGrid();


        obj.SetActive(true);

        Vector3 _bounds = Utilities.CalculateObjectBounds(obj, 1, false);
        List<Collider> _colliders = new List<Collider>();
        _colliders.AddRange(obj.GetComponents<Collider>());
        _colliders.AddRange(obj.GetComponentsInChildren<Collider>());

        int maskWidth = Mathf.CeilToInt(_bounds.x / (A_Star_Pathfinding.Instance.NodeRadius));
        maskWidth = maskWidth == 0 ? 1 : maskWidth;

        int maskHeight = Mathf.CeilToInt(_bounds.z / (A_Star_Pathfinding.Instance.NodeRadius));
        maskHeight = maskHeight == 0 ? 1 : maskHeight;



        PlacementGrid _grid;
        bool[][] _mask = new bool[maskWidth][];

        for (int i = 0; i < _mask.Length; i++)
        {
            _mask[i] = new bool[maskHeight];
            for (int k = 0; k < _mask[i].Length; k++)
            {

                Vector3 horizontalOffset = Vector3.right * ((-_mask.Length * A_Star_Pathfinding.Instance.NodeRadius * .5f) + (i * A_Star_Pathfinding.Instance.NodeRadius));//(-_bounds.x + (i * A_Star_Pathfinding.Instance.NodeRadius) + (A_Star_Pathfinding.Instance.NodeRadius / 2f));
                Vector3 forwardOffset = Vector3.forward * ((-_mask[i].Length * A_Star_Pathfinding.Instance.NodeRadius * .5f) + (k * A_Star_Pathfinding.Instance.NodeRadius));// (-_bounds.z + (k * A_Star_Pathfinding.Instance.NodeRadius) + (A_Star_Pathfinding.Instance.NodeRadius / 2f));
                Vector3 checkOrigin = obj.transform.position + horizontalOffset + forwardOffset + new Vector3(0, _bounds.y * 2.5f, 0);

                Ray _ray = new Ray(checkOrigin, Vector3.down);

                //Debug.DrawLine(checkOrigin, checkOrigin + (Vector3.down * ((checkOrigin.y - obj.transform.position.y) * 2.5f)), Color.red, 10f);


                RaycastHit[] _hits = Physics.SphereCastAll(_ray, A_Star_Pathfinding.Instance.NodeRadius, (checkOrigin.y - obj.transform.position.y) * 2.5f);


                bool didHit = false;
                for (int a = 0; a < _hits.Length; a++)
                {
                    for (int b = 0; b < _colliders.Count; b++)
                    {
                        if (_hits[a].collider == _colliders[b])
                        {
                            didHit = true;
                            break;
                        }
                    }
                }


                _mask[i][k] = didHit;
            }
        }


        obj.SetActive(true);
        _grid.bounds = _bounds;
        _grid.occupyMask = _mask;
        return _grid;
    }


    public void DestroyAllGenerated()
    {
        if (genObjParent != null)
            DestroyImmediate(genObjParent);

        GameObject groundobj = (MapGenerator.Instance == null ? FindObjectOfType<MapGenerator>() : MapGenerator.Instance).GroundObject;

        if (groundobj != null)
            Destroy(groundobj);


        m_Weather.Terminate();
    }











    #region Accessors

    public bool ShowDebug
    {
        get { return showDebug; }
    }

    public float CurrentTemperature
    {
        get { return currentTemperature; }
        set { currentTemperature = Mathf.Clamp(value, 0f, 100f); }
    }
    public float CurrentMoisture
    {
        get { return currentMoisture; }
        set { currentMoisture = Mathf.Clamp(value, 0f, 100f); }
    }
    public float CurrentTime
    {
        get { return m_Weather.CurrentTime; }
    }

    public float Temperature_Favored
    {
        get
        {
            float _favor = GameManager.Instance.DeityFavor;
            float temperatureFavor = _favor * DEITY_FAVOR_TEMPERATURE_MULTIPLIER;


            //In Good Favor?
            if (_favor > 0)
            {

                //Move temperature closer towards 50
                float tempDiff = 50f - currentTemperature;

                if (Mathf.Abs(tempDiff) < Mathf.Abs(temperatureFavor))
                {
                    return 50f;
                }
                else if (tempDiff != 0)
                {
                    return currentTemperature + (temperatureFavor * (tempDiff / tempDiff));
                }
            }
            //In bad favor?
            else
            {
                //Move temperature closer to extremes
                return currentTemperature + temperatureFavor * (currentTemperature < 50 ? -1f : 1f);
            }

            return CurrentTemperature;
        }
    }
    public float Moisture_Favored
    {
        get
        {
            float _favor = GameManager.Instance.DeityFavor;
            float moistureFavor = _favor * DEITY_FAVOR__MOISTURE_MULTIPLIER;


            //In Good Favor?
            if (_favor > 0)
            {

                //Move moisture closer towards 50
                float moistureDiff = 50f - CurrentMoisture;

                if (Mathf.Abs(moistureDiff) < Mathf.Abs(moistureFavor))
                {
                    return 50f;
                }
                else if (moistureDiff != 0)
                {
                    return CurrentMoisture + (moistureFavor * (moistureDiff / moistureDiff));
                }
            }
            //In bad favor?
            else
            {
                //Move moisture closer to extremes
                return CurrentMoisture + moistureFavor * (CurrentMoisture < 50 ? -1f : 1f);
            }

            return CurrentMoisture;
        }
    }
    public float Time_Favored
    {
        get
        {
            float _favor = GameManager.Instance.DeityFavor;
            float timeFavor = _favor * DEITY_FAVOR_TIME_MULTIPLIER;


            //In Good Favor?
            if (_favor > 0)
            {

                //Move time closer towards 50
                float timeDiff = 50f - CurrentTime;

                if (Mathf.Abs(timeDiff) < Mathf.Abs(timeFavor))
                {
                    return 50f;
                }
                else if (timeDiff != 0)
                {
                    return CurrentTime + (timeFavor * (timeDiff / timeDiff));
                }
            }
            //In bad favor?
            else
            {
                //Move time closer to extremes
                return CurrentTime + timeFavor * (CurrentTime < 50 ? -1f : 1f);
            }

            return CurrentTime;
        }
    }


    public Vector3 Wind
    {
        get { return m_Weather.Wind; }
    }

    public Transform StartGoalTransform
    {
        get { return currentStartGoal; }
        private set { currentStartGoal = value; }
    }
    public Transform EndGoalTransform
    {
        get { return currentEndGoals.Count == 0 ? null : currentEndGoals[0]; }
        private set
        {
            currentEndGoals.Clear();
            currentEndGoals.Add(value);
        }
    }


    public Vector3 StartGoalPosition
    {
        get { return StartGoalTransform == null ? Vector3.zero : StartGoalTransform.position; }
    }
    public Vector3 EndGoalPosition
    {
        get { return EndGoalTransform == null ? Vector3.zero : EndGoalTransform.position; }
    }

    #endregion


  

    void OnDrawGizmos()
    {

        if (gizmoToDraw == GizmoType.NONE || regionMap == null || objectMap == null)
            return;
        
        Vector3 basicColor = new Vector3(0f, 1f, 1f);
        float radius = A_Star_Pathfinding.Instance.NodeRadius; 


        if (gizmoToDraw == GizmoType.LAND)
        {
           
            for (int i = 0; i < largeLandMap.Length; i++)
            {
                for (int j = 0; j < largeLandMap[0].Length; j++)
                {

                    if (!largeLandMap[i][j])
                        continue;

                    Vector3 worldPos = transform.position - (Vector3.right * largeLandMap.Length / 2) - (Vector3.forward * largeLandMap[0].Length / 2);
                    worldPos += Vector3.right * i * (radius * 2f);
                    worldPos.x += radius;

                    worldPos += Vector3.forward * j * (radius * 2f);
                    worldPos.z += radius;

                    Gizmos.DrawCube(worldPos, Vector3.one * (radius * 2f) * 0.75f);
                }
            }

            return;
        }


        if (gizmoToDraw == GizmoType.LAND_SMALL)
        {
            radius /= scaleFactor;


            for (int i = 0; i < smallLandMap.Length; i++)
            {
                for (int j = 0; j < smallLandMap[0].Length; j++)
                {

                    if (!smallLandMap[i][j])
                        continue;

                    Vector3 worldPos = transform.position - (Vector3.right * smallLandMap.Length / 2) - (Vector3.forward * smallLandMap[0].Length / 2);
                    worldPos += Vector3.right * i * (radius * 2f);
                    worldPos.x += radius;

                    worldPos += Vector3.forward * j * (radius * 2f);
                    worldPos.z += radius;

                    Gizmos.DrawCube(worldPos, Vector3.one * (radius * 2f) * 0.75f);
                }
            }
            
            return;
        }

        for (int i = 0; i < objectMap.Length; i++)
        {
            for (int j = 0; j < objectMap[0].Length; j++)
            {

                bool shouldDraw = false;
                if (gizmoToDraw == GizmoType.OBJECT)
                {
                    // /if (objectMap[i][j] == 0)
                    //  continue;


                    Gizmos.color = objectMap[i][j] == 0 ? Color.white : Color.red;
                    shouldDraw = true;
                }
                else
                {

                    for (int k = 0; k < nodesToDraw.Count; k++)
                    {

                        int a = (int)(((float)i / objectMap.Length) * regionMap.Length);
                        int b = (int)(((float)j / objectMap[0].Length) * regionMap[0].Length);


                        if (!Utilities.HasFlag(regionMap[a][b], nodesToDraw[k]))
                            continue;

                        shouldDraw = true;
                        if (nodesToDraw[k] == NodeType.Empty)
                        {
                            Gizmos.color = Color.black;
                        }
                        else if (nodesToDraw[k] == NodeType.BasicGround)
                        {
                            Gizmos.color = Color.white;
                        }
                        else
                        {
                            basicColor.x = ((int)k - 2) * 30f;

                            Gizmos.color = Utilities.HSVtoRGB(basicColor);
                        }

                    }
                }

                if (!shouldDraw)
                    continue;

                Vector3 worldPos = transform.position - (Vector3.right * objectMap.Length / 2) - (Vector3.forward * objectMap[0].Length / 2);
                worldPos += Vector3.right * i * (radius * 2f);
                worldPos.x += radius;

                worldPos += Vector3.forward * j * (radius * 2f);
                worldPos.z += radius;

                Gizmos.DrawCube(worldPos, Vector3.one * (radius * 2f) * 0.75f);
            }
        }
    }
}
