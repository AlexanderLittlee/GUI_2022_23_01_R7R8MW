﻿using Minecraft.Graphics;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace Minecraft.Terrain
{
    internal class World
    {
        public Dictionary<Vector2, Chunk> Chunks { get; set; }
        public WorldGenerator? WorldGenerator { get; set; }

        private Dictionary<Vector2, List<Block>> blockQueue;

        public World()
        {
            Chunks = new Dictionary<Vector2, Chunk>();
            blockQueue = new Dictionary<Vector2, List<Block>>();
        }
        public Chunk? GetChunk(Vector3 pos,out Vector2 chunkPos)
        {
            int chunkPosX = (int)(pos.X / Chunk.Size);
            int chunkPosZ = (int)(pos.Z / Chunk.Size);

            if (pos.X < 0 && (int)pos.X % Chunk.Size != 0)
                chunkPosX--;
            if (pos.Z < 0 && (int)pos.Z % Chunk.Size != 0)
                chunkPosZ--;

            chunkPos = new Vector2(chunkPosX, chunkPosZ);

            return Chunks.GetValueOrDefault(new Vector2(chunkPosX, chunkPosZ));
        }
        public BlockType? GetBlock(Vector3 pos)
        {
            var chunk = GetChunk(pos,out Vector2 chunkPos);

            if(chunk != null)
            {
                return chunk.GetBlock(pos);
            }

            return null;
        }
        public void AddChunk(Vector2 pos,Chunk chunk)
        {  
            if(Chunks.TryAdd(pos, chunk))
            {
                if (blockQueue.ContainsKey(pos))
                {
                    foreach (var block in blockQueue[pos])
                    {
                        chunk.AddBlock(block.Position, block.Type, false);
                    }
                    blockQueue.Remove(pos);
                }
            }
        }
        public void RemoveBlock(Vector3 pos)
        {
            var chunk = GetChunk(pos, out Vector2 chunkPos);

            if (chunk != null)
            {
                chunk.RemoveBlock(pos);
                ChunkMesh.CreateMesh(this, chunk.Position);

                var left =  new Vector3(-1,0, 0);
                var right = new Vector3( 1,0, 0);
                var back =  new Vector3( 0,0,-1);
                var front = new Vector3( 0,0, 1);

                GetChunk(pos + left, out Vector2 chunkPosLeft);
                GetChunk(pos + right, out Vector2 chunkPosRight);
                GetChunk(pos + back, out Vector2 chunkPosBack);
                GetChunk(pos + front, out Vector2 chunkPosFront);

                if (chunkPosLeft != chunkPos)
                    ChunkMesh.CreateMesh(this, chunkPosLeft);

                if (chunkPosRight != chunkPos)
                    ChunkMesh.CreateMesh(this, chunkPosRight);

                if (chunkPosBack != chunkPos)
                    ChunkMesh.CreateMesh(this, chunkPosBack);

                if (chunkPosFront != chunkPos)
                    ChunkMesh.CreateMesh(this, chunkPosFront);
            }
        }
        public void AddBlock(Vector3 pos,BlockType block)
        {
            var chunk = GetChunk(pos,out Vector2 chunkPos);

            if (chunk != null)
            {
                chunk.AddBlock(pos, block, true);
                ChunkMesh.CreateMesh(this, chunk.Position);
            }
        }
        public void AddEntity(Vector3 position,EntityType entityType, Chunk chunk)
        {
            var entity = EntityData.Entities[(int)entityType];

            foreach(var block in entity.Blocks)
            {
                Vector3 blockPos = block.Position + position;

                if (chunk.IsBlockPosInChunk(block.Position + position))
                    chunk.AddBlock(blockPos, block.Type, false);
                else
                {

                    GetChunk(blockPos, out Vector2 whereShouldBlockBe);

                    if (Chunks.ContainsKey(whereShouldBlockBe))
                    {
                        Chunks[whereShouldBlockBe].AddBlock(block.Position + position, block.Type, true);
                    }
                    else
                    {
                        if (blockQueue.ContainsKey(whereShouldBlockBe))
                        {
                            blockQueue[whereShouldBlockBe].Add(new Block(blockPos, block.Type));
                        }
                        else
                        {
                            blockQueue.Add(whereShouldBlockBe, new List<Block>());
                            blockQueue[whereShouldBlockBe].Add(new Block(blockPos, block.Type));
                        }
                    }
                }
            }
        }
    }
}
