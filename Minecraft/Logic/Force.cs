﻿using OpenTK.Mathematics;
using System;

namespace Minecraft.Logic
{
    enum ForceType { Rise, Fall }
    internal class Force : IForce
    {
        private const float gravityStrength = 0.1f;
        private const float maxFallSpeed = 40.0f;
        private const float riseForce = 15.0f;

        private double forceGraphStep;
        private double currentStep;
        private ForceType forceType;
        public Force()
        {
            const int steps = 25;
            forceGraphStep = Math.PI / steps;
            currentStep = 0;
        }
        public void SetForceType(ForceType type)
        {
            currentStep = 0;
            forceType = type;
        }
        public void Reset()
        {
            currentStep = 0;
        }
        public void Apply(out Vector3 deltaPos, ref bool riseState)
        {
            double deltaY = 0;
            deltaPos = new Vector3();

            if (forceType == ForceType.Rise)
            {
                deltaY = Math.Sin(currentStep += forceGraphStep) * riseForce;
                if (currentStep >= Math.PI)
                {
                    SetForceType(ForceType.Fall);
                    riseState = false;
                }
            }
            else
            {
                deltaY = -Math.Min(Math.Pow(Math.E, currentStep += forceGraphStep), maxFallSpeed);
            }
            deltaPos.Y += (float)deltaY;
        }
    }
}
