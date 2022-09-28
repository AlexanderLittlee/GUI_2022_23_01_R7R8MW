﻿using OpenTK.Mathematics;
using OpenTK.Input;

namespace Minecraft.Graphics
{
    internal class Camera
    {
        public Vector3 Position{ get; private set; }
        public Vector3 Up { get; private set; }
        public Vector3 Front { get; set; }
        public Matrix4 View { get; private set; }
        public float Fov { get; set; }
        public float Pitch { get; private set; }
        public float Yaw { get; private set; }

        public event ShaderMat4Handler ViewMatrixChange;
        public event ShaderVec3Handler PositionChange;

        public Camera(Vector3 startPos)
        {
            Position = startPos;
            Up = Vector3.UnitY;
        }
        public void Init()
        {
            UpdateViewMatrix();
            PositionChange?.Invoke("model", Position);
        }
        public void SetPosition(Vector3 position)
        {
            Position = position;
        }
        public void ModPosition(Vector3 change)
        {
            Position += change;
            PositionChange?.Invoke("model", Position);
        }
        public void ChangeFront(Vector3 front)
        {
            Front = front;
        }
        public void ChangeYaw(float change)
        {
            Yaw += change;
        }
        public void ChangePitch(float change)
        {
            Pitch += change;
        }
        public void ResetPitch(float resetValue)
        {
            Pitch = resetValue;
        }

        public void UpdateViewMatrix()
        {
            //remove
            PositionChange?.Invoke("model", Position);
            View = Matrix4.LookAt(Position, Position + Front, Up);
            ViewMatrixChange?.Invoke("view",View);
        }
    }
}