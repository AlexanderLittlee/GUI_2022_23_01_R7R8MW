﻿using Assimp;
using Minecraft.Controller;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Diagnostics;
using System.Windows;

namespace Minecraft.Render
{
    class Renderer: IDisposable
    {
        public event Action? OnRendering;

        public Scene? Scene { get; set; }
        public static Stopwatch Stopwatch = new Stopwatch();
        public void Dispose()
        {
            Scene?.Dispose();
        }
        public void SetupRenderer(int width,int height)
        {
            GL.Enable(EnableCap.Multisample);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);
            GL.FrontFace(FrontFaceDirection.Ccw);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            Scene?.OnProjectionMatrixChange((float)width / height);

            Stopwatch.Start();
        }
        public void RenderFrame()
        {
            OnRendering?.Invoke();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Scene?.Render();
        }
    }
}
