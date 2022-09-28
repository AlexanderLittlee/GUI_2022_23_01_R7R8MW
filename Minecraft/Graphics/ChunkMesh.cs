﻿using Minecraft.Terrain;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace Minecraft.Graphics
{
    internal class ChunkMesh
    {
        private int nVAO,tVAO, nVBO,tVBO;
        private int nFaceCount = 0, tFaceCount = 0;
        public void Render(Shader shader)
        {
            shader.Use();
            shader?.SetInt("tex", AtlasTexturesData.Atlas.GetTexUnitId());

            GL.BindVertexArray(nVAO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, nFaceCount * 6);

            GL.Disable(EnableCap.CullFace);
            GL.BindVertexArray(tVAO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, tFaceCount * 6);
            GL.Enable(EnableCap.CullFace);
        }
        public static void CreateMesh(World world,Vector2 target)
        {
            var chunk = world.Chunks.GetValueOrDefault(target);

            if (chunk == null)
                return;

            var nVertices = new List<float>();
            var tVertices = new List<float>();

            for (int x = 0; x < Chunk.Size; x++)
                for (int z = 0; z < Chunk.Size; z++)
                    for (int y = 0; y <= chunk.TopBlockPositions[x, z]; y++)
                    {
                        Vector3 blockPos = new Vector3(x + Chunk.Size * target.X, y, z + Chunk.Size * target.Y);
                        var block = chunk.GetBlock(blockPos);

                        if (block == 0)
                            continue;

                        foreach (var face in FaceDirectionVectors.Vectors)
                        {
                            Vector3 neighborPos = blockPos + face.Value;

                            if (BlockIsOnBorder(target, blockPos))
                            {
                                if (chunk.IsBlockInChunk(neighborPos))
                                {
                                    var neighborBlock = chunk.GetBlock(neighborPos);

                                    if(neighborBlock == -1 || (BlockData.IsBolckTransparent(neighborBlock) && !BlockData.IsBolckTransparent(block)))
                                    {
                                        nVertices.AddRange(BlockFace.GetBlockFaceVertices(block, face.Key, blockPos));
                                        chunk.Mesh.nFaceCount++;
                                    }
                                    else if(BlockData.IsBolckTransparent(block) && neighborBlock == 0)
                                    {
                                        tVertices.AddRange(BlockFace.GetBlockFaceVertices(block, face.Key, blockPos));
                                        chunk.Mesh.tFaceCount++;
                                    }
                                }
                                else
                                {
                                    var neighborChunk = world.Chunks.GetValueOrDefault(target + face.Value.Xz);
                                    
                                    if(neighborChunk == null && !BlockData.IsBolckTransparent(block))
                                    {
                                        if(world.WorldGenerator != null)
                                        {
                                            int topBlockY = world.WorldGenerator.GetHeightAtPosition(neighborPos.Xz);

                                            if (topBlockY < neighborPos.Y)
                                            {
                                                nVertices.AddRange(BlockFace.GetBlockFaceVertices(block, face.Key, blockPos));
                                                chunk.Mesh.nFaceCount++;
                                            }
                                        }
                                    }
                                    else if(neighborChunk != null)
                                    {
                                        var neighborBlock = neighborChunk.GetBlock(neighborPos);
                                    
                                        if(BlockData.IsBolckTransparent(neighborBlock) && !BlockData.IsBolckTransparent(block))
                                        {
                                            nVertices.AddRange(BlockFace.GetBlockFaceVertices(block, face.Key, blockPos));
                                            chunk.Mesh.nFaceCount++;
                                        }
                                        else if(BlockData.IsBolckTransparent(block) && neighborBlock == 0)
                                        {
                                            tVertices.AddRange(BlockFace.GetBlockFaceVertices(block, face.Key, blockPos));
                                            chunk.Mesh.tFaceCount++;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var neighborBlock = chunk.GetBlock(neighborPos);
                                
                                if (neighborBlock == -1 || BlockData.IsBolckTransparent(neighborBlock) && !BlockData.IsBolckTransparent(block))
                                {
                                    nVertices.AddRange(BlockFace.GetBlockFaceVertices(block, face.Key, blockPos));
                                    chunk.Mesh.nFaceCount++;
                                }
                                else if (BlockData.IsBolckTransparent(block) && neighborBlock == 0)
                                {
                                    tVertices.AddRange(BlockFace.GetBlockFaceVertices(block, face.Key, blockPos));
                                    chunk.Mesh.tFaceCount++;
                                }
                            }
                        }
                    }

            chunk.Mesh.nVBO = GL.GenBuffer();
            chunk.Mesh.nVAO = GL.GenVertexArray();

            GL.BindVertexArray(chunk.Mesh.nVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, chunk.Mesh.nVBO);

            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * nVertices.Count, nVertices.ToArray(), BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 9 * sizeof(float), 6 * sizeof(float));
            GL.EnableVertexAttribArray(2);

            GL.VertexAttribPointer(3, 1, VertexAttribPointerType.Float, false, 9 * sizeof(float), 8 * sizeof(float));
            GL.EnableVertexAttribArray(3);

            chunk.Mesh.tVBO = GL.GenBuffer();
            chunk.Mesh.tVAO = GL.GenVertexArray();

            GL.BindVertexArray(chunk.Mesh.tVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, chunk.Mesh.tVBO);

            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * tVertices.Count, tVertices.ToArray(), BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 9 * sizeof(float), 6 * sizeof(float));
            GL.EnableVertexAttribArray(2);

            GL.VertexAttribPointer(3, 1, VertexAttribPointerType.Float, false, 9 * sizeof(float), 8 * sizeof(float));
            GL.EnableVertexAttribArray(3);
        }
        private static bool BlockIsOnBorder(Vector2 chunkPos,Vector3 blockPos)
        {
            float chunkLeftCorner =   chunkPos.X * Chunk.Size;
            float chunkRightCorner = (chunkPos.X + 1) * Chunk.Size - 1;

            float chunkBotCorner =  chunkPos.Y * Chunk.Size;
            float chunkTopCorner = (chunkPos.Y + 1) * Chunk.Size - 1;

            return blockPos.X == chunkLeftCorner ||
                   blockPos.X == chunkRightCorner ||
                   blockPos.Z == chunkBotCorner ||
                   blockPos.Z == chunkTopCorner;
        }
    }
}