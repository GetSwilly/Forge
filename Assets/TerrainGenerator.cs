using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class TerrainGenerator : MonoBehaviour {

    [SerializeField]
    int depth;

    [SerializeField]
    int width;

    [SerializeField]
    int height;

    [SerializeField]
    int octaves;

    [SerializeField]
    [Range(0f, 100f)]
    float noiseScale = 20;


    float[,] heightMap;
    Terrain m_Terrain;

    System.Random pseudoRandom;

    [HideInInspector]
    public static TerrainGenerator Instance { get; private set; }
    void Awake()
    {
        Instance = this;

        m_Terrain = GetComponent<Terrain>();
    }
   
    public void GenerateTerrain()
    {
        pseudoRandom = new System.Random();

        if (m_Terrain == null)
            m_Terrain = GetComponent<Terrain>();

        m_Terrain.terrainData = GenerateTerrain(m_Terrain.terrainData);
    }

    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width + 1;

        float[][] generatedRegion = GenerateRegion(pseudoRandom.Next() < 0.5f, width, height, octaves);
        heightMap = new float[generatedRegion.Length,generatedRegion[0].Length];

        for(int i = 0; i < generatedRegion.Length; i++)
        {
            for(int k = 0; k < generatedRegion[i].Length; k++)
            {
                heightMap[i, k] = generatedRegion[i][k];
            }
        }

        terrainData.size = new Vector3(width, depth, height);
        terrainData.SetHeights(0, 0, heightMap);

        return terrainData;
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


}
