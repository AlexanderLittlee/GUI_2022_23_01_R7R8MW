﻿using Minecraft.Render;
using Minecraft.Logic;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Minecraft.Terrain.Noise;

namespace Minecraft.Terrain
{
    internal class WorldGenerator
    {
        public event Action<Vector2>? OnChunkAdded;

        private FastNoise noise;

        private const int worldDepth = 32;
        private const int noiseDepth = 32;

        private World world;
        private PriorityQueue<Vector2,float> generatorQueue;
        private Queue<KeyValuePair<Vector2, Chunk>> generatedChunks;

        public WorldGenerator(World world)
        {
            generatorQueue = new PriorityQueue<Vector2, float>();
            generatedChunks = new Queue<KeyValuePair<Vector2, Chunk>>();

            this.world = world;
            this.world.WorldGenerator = this;

            noise = new FastNoise();

            noise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
            noise.SetInterp(FastNoise.Interp.Hermite);
            noise.SetFractalOctaves(4);
            noise.SetFrequency(0.005f);
            noise.SetFractalGain(0.55f);
            noise.SetCellularJitter(0.005f);

            noise.SetFractalType(FastNoise.FractalType.FBM);

        }
        public void InitWorld()
        {
            for (int x = -WorldRenderer.RenderDistance; x < WorldRenderer.RenderDistance; x++)
                for (int z = -WorldRenderer.RenderDistance; z < WorldRenderer.RenderDistance; z++)
                    AddChunk(new Vector2(x, z));
        }
        public void AddGeneratedChunksToWorld()
        {
            while(generatedChunks.Count > 0)
            {
                var chunk = generatedChunks.Dequeue();
                world.AddChunk(chunk.Key, chunk.Value);
                OnChunkAdded?.Invoke(chunk.Key);
            }
        }
        public void GenerateChunksToQueue()
        {
            if (generatorQueue.Count > 0)
            {
                for (int maxGenerationInSingleRender = 0; maxGenerationInSingleRender < 16; maxGenerationInSingleRender++)
                {
                    AddChunk(generatorQueue.Dequeue());
                    if (generatorQueue.Count == 0)
                        break;
                }
            }       
        }
        public int GetHeightAtPosition(Vector2 pos)
        {
            return (int)Math.Round((noise.GetValue(pos.X, pos.Y) + noise.GetSimplexFractal(pos.X, pos.Y)) / (1.0f / noiseDepth)) + worldDepth;
        }
        public BlockType GetBlockAtHeight(int y,int depth = 0)
        {
            if (y == 0)
                return BlockType.Bedrock;
            else if (depth > 5)
                return BlockType.Stone;
            else if (y < worldDepth - 8)
                return BlockType.Sand;
            else if (depth >= 1)
                return BlockType.Dirt;
            else
                return BlockType.Grass;
        }
        public void ExpandWorld(Direction dir,Vector2 position)
        {
            int x = (int)position.X;
            int z = (int)position.Y;

            if (dir == Direction.Left)
            {
                AddChunkRange(0, WorldRenderer.RenderDistance, -WorldRenderer.RenderDistance, WorldRenderer.RenderDistance, x, z, -1, 1);
            }
            else if (dir == Direction.Right)
            {
                AddChunkRange(0, WorldRenderer.RenderDistance, -WorldRenderer.RenderDistance, WorldRenderer.RenderDistance, x, z, 1, 1);
            }
            else if (dir == Direction.Down)
            {
                AddChunkRange(-WorldRenderer.RenderDistance, WorldRenderer.RenderDistance, 0, WorldRenderer.RenderDistance, x, z, 1, -1);

            }
            else if (dir == Direction.Up)
            {
                AddChunkRange(-WorldRenderer.RenderDistance, WorldRenderer.RenderDistance, 0, WorldRenderer.RenderDistance, x, z, 1, 1);  
            }
        }
        private void AddChunkRange(int fromXRange,int toXRgange,int fromZRange,int toZRange,int x,int z,int xSign,int zSign)
        {
            for (int xs = fromXRange; xs < toXRgange; xs++)
                for (int zs = fromZRange; zs < toZRange; zs++)
                {
                    Vector2 pos = new Vector2(x + xs * xSign, z + zs * zSign);
                    generatorQueue.Enqueue(pos,(pos - new Vector2(x,z)).Length);
                }
        }
        private void AddChunk(Vector2 position)
        {
            if (!world.Chunks.ContainsKey(position))
            {
                Chunk chunk = new Chunk(position);
                CreateChunk(chunk, position * Chunk.Size);
                generatedChunks.Enqueue(new KeyValuePair<Vector2, Chunk>(position, chunk));
            }
        }
        private void CreateChunk(Chunk chunk,Vector2 offset)
        {
            for (int x = 0; x < Chunk.Size; x++)
            {
                for (int z = 0; z < Chunk.Size; z++)
                {
                    int y = GetHeightAtPosition(new Vector2(x + offset.X, z + offset.Y));

                    int depth = 0;

                    if (y < worldDepth - 5)
                    {
                        if (y < 0)
                            y *= -1;
                        else if (y == 0)
                            y++;

                        for (int waterY = y + 1; waterY < worldDepth - 10; waterY++)
                            chunk.AddBlock(new Vector3(x, waterY, z), BlockType.Water);
                    }

                    for (; y >= 0; y--)
                    {
                        chunk.AddBlock(new Vector3(x, y, z), GetBlockAtHeight(y, depth));
                        depth++;
                    }       
                }
            }
        }
    }
}