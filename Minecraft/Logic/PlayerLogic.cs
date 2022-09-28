﻿using Minecraft.Controller;
using Minecraft.Game;
using Minecraft.Terrain;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using System;
using System.Collections.Generic;

namespace Minecraft.Logic
{
    internal enum Direction
    {
        Front,
        Back,
        Up,
        Down,
        Left,
        Right
    }
    internal class PlayerLogic
    {
        public bool Sprint = false;

        private World world;
        private Player player;

        private const float moveSpeed = 25.0f;
        private const float sprintSpeed = 50.0f;
        private const float mouseSpeed = 0.125f;
        private const float playerHeight = 2.0f;
        private Force force;
        public PlayerLogic(Player player,World world)
        {
            this.world = world;
            this.player = player;

            force = new Force();
            force.SetForceType(ForceType.Rise);
        }
        public void Update()
        {
            //Vector3 deltaPos = new Vector3();
            //force.Apply(ref deltaPos);
            ////Collision(ref deltaPos);
            //player.Camera.ModPosition(deltaPos);
        }
        public void Move(Direction dir,float delta)
        {
            float speed = Sprint ? sprintSpeed : moveSpeed;

            Vector3 deltaPos = new Vector3();

            switch (dir)
            {
                case Direction.Front:
                    deltaPos = player.Camera.Front * speed * delta;
                    break;
                case Direction.Back:
                    deltaPos = -player.Camera.Front * speed * delta;
                    break;
                case Direction.Up:
                    deltaPos = player.Camera.Up * speed * delta;
                    break;
                case Direction.Down:
                    deltaPos = -player.Camera.Up * speed * delta;
                    break;
                case Direction.Left:
                    deltaPos = -Vector3.Normalize(Vector3.Cross(player.Camera.Front, player.Camera.Up)) * speed * delta;
                    break;
                case Direction.Right:
                    deltaPos = Vector3.Normalize(Vector3.Cross(player.Camera.Front, player.Camera.Up)) * speed * delta;
                    break;
            }
            force.Apply(ref deltaPos);
            Collision(ref deltaPos);

            player.Camera.ModPosition(deltaPos);
        }
        public void ChangeView()
        {
            player.Camera.ChangePitch(-MathHelper.DegreesToRadians(MouseController.DeltaY) * mouseSpeed);
            player.Camera.ChangeYaw(MathHelper.DegreesToRadians(MouseController.DeltaX) * mouseSpeed);

            if (player.Camera.Pitch > MathHelper.DegreesToRadians(89.0))
                player.Camera.ResetPitch((float)MathHelper.DegreesToRadians(89.0));
            else if (player.Camera.Pitch < MathHelper.DegreesToRadians(-89.0))
                player.Camera.ResetPitch((float)MathHelper.DegreesToRadians(-89.0));

            Vector3 front = new Vector3();
            front.X = (float)(Math.Cos(player.Camera.Pitch) * Math.Cos(player.Camera.Yaw));
            front.Y = (float)Math.Sin(player.Camera.Pitch);
            front.Z = (float)(Math.Cos(player.Camera.Pitch) * Math.Sin(player.Camera.Yaw));

            player.Camera.Front = Vector3.Normalize(front);
        }
        private bool Collision(ref Vector3 deltaPos)
        {
            var pos = player.GetPosition() + deltaPos;

            Chunk? weAreIn = world.Chunks.GetValueOrDefault(new Vector2((int)pos.X / Chunk.Size, (int)pos.Z / Chunk.Size));

            if(weAreIn != null)
            {
                //x axis collision:
                
            }
            return false;
        }
    }
}