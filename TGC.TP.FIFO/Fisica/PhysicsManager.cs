﻿using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Constraints;
using BepuUtilities.Memory;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using TGC.TP.FIFO.Utilidades;

namespace TGC.TP.FIFO.Fisica
{
    public class PhysicsManager
    {
        private Simulation Simulation;
        public BufferPool BufferPool { get; private set; }
        public SimpleThreadDispatcher ThreadDispatcher { get; private set; }

        private CollidableProperty<MaterialProperties> MaterialProperties;
        public Dictionary<CollidableReference, IColisionable> CollidableReferences = new();

        public void Initialize()
        {
            BufferPool = new BufferPool();

            var targetThreadCount = Math.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
            ThreadDispatcher = new SimpleThreadDispatcher(targetThreadCount);

            MaterialProperties = new CollidableProperty<MaterialProperties>();

            Simulation = Simulation.Create(
                BufferPool,
                new NarrowPhaseCallbacks(MaterialProperties, CollidableReferences),
                new PoseIntegratorCallbacks(
                    gravity: new BepuVector3(0, -10, 0),
                    linearDamping: 0.03f, // Que tan rapido se disipa la velocidad lineal
                    angularDamping: 0.8f // Que tan rapido se disipa la velocidad angular
                 ),
                new SolveDescription(8, 4));

            MaterialProperties.Initialize(Simulation);
        }

        public void Update(float deltaTime)
        {
            float safeDeltaTime = Math.Max(deltaTime, 1f / 240f);
            Simulation.Timestep(safeDeltaTime, ThreadDispatcher);
        }

        public BodyHandle AddDynamicSphere(float radius, float mass, float friction, float dampingRatio, float springFrequency, float maximumRecoveryVelocity, XnaVector3 initialPosition, IColisionable collidableReference)
        {
            var sphereShape = new Sphere(radius);
            var shapeIndex = Simulation.Shapes.Add(sphereShape);

            var collidableDescription = new CollidableDescription(shapeIndex, ContinuousDetection.Passive);

            var bodyDescription = BodyDescription.CreateDynamic(
                initialPosition.ToBepuVector3(),
                sphereShape.ComputeInertia(mass),
                collidableDescription,
                new BodyActivityDescription(0.1f));

            var handle = Simulation.Bodies.Add(bodyDescription);

            CollidableReferences[new CollidableReference(CollidableMobility.Dynamic, handle)] = collidableReference;

            MaterialProperties.Allocate(handle) = new MaterialProperties
            {
                FrictionCoefficient = friction,
                MaximumRecoveryVelocity = maximumRecoveryVelocity,
                SpringSettings = new SpringSettings(springFrequency, dampingRatio)
            };

            return handle;
        }

        public BodyHandle AddDynamicBox(float width, float height, float length, float mass, float friction, XnaVector3 initialPosition, XnaQuaternion initialRotation, IColisionable collidableReference)
        {
            var boxShape = new Box(width, height, length);
            var shapeIndex = Simulation.Shapes.Add(boxShape);

            var collidableDescription = new CollidableDescription(shapeIndex, ContinuousDetection.Passive);

            var bodyDescription = BodyDescription.CreateDynamic(
                new RigidPose(initialPosition.ToBepuVector3(), initialRotation.ToBepuQuaternion()),
                boxShape.ComputeInertia(mass),
                collidableDescription,
                new BodyActivityDescription(0.1f));

            var handle = Simulation.Bodies.Add(bodyDescription);

            CollidableReferences[new CollidableReference(CollidableMobility.Dynamic, handle)] = collidableReference;

            MaterialProperties.Allocate(handle) = new MaterialProperties
            {
                FrictionCoefficient = friction,
                MaximumRecoveryVelocity = float.MaxValue, // Default
                SpringSettings = new SpringSettings(30, 1) // Default
            };

            return handle;
        }

        public BodyHandle AddDynamicCylinder(float length, float radius, float mass, float friction, XnaVector3 initialPosition, XnaQuaternion initialRotation, IColisionable collidableReference)
        {
            var cylinderShape = new Cylinder(radius, length);
            var shapeIndex = Simulation.Shapes.Add(cylinderShape);

            var rotationFix = XnaQuaternion.CreateFromAxisAngle(XnaVector3.Up, MathF.PI / 2f); // gira 90° en X

            var collidableDescription = new CollidableDescription(shapeIndex, ContinuousDetection.Passive);

            var bodyDescription = BodyDescription.CreateDynamic(
                new RigidPose(initialPosition.ToBepuVector3(), (initialRotation * rotationFix).ToBepuQuaternion()),
                cylinderShape.ComputeInertia(mass),
                collidableDescription,
                new BodyActivityDescription(0.1f));

            var handle = Simulation.Bodies.Add(bodyDescription);

            CollidableReferences[new CollidableReference(CollidableMobility.Dynamic, handle)] = collidableReference;

            MaterialProperties.Allocate(handle) = new MaterialProperties
            {
                FrictionCoefficient = friction,
                MaximumRecoveryVelocity = float.MaxValue, // Default
                SpringSettings = new SpringSettings(30, 1) // Default
            };

            return handle;
        }

        public StaticHandle AddStaticSphere(float radius, XnaVector3 initialPosition, IColisionable collidableReference)
        {
            var sphereShape = new Sphere(radius);
            var shapeIndex = Simulation.Shapes.Add(sphereShape);

            var staticDescription = new StaticDescription(
                initialPosition.ToBepuVector3(),
                BepuQuaternion.Identity,
                shapeIndex,
                continuity: ContinuousDetection.Passive);

            var handle = Simulation.Statics.Add(staticDescription);

            CollidableReferences[new CollidableReference(handle)] = collidableReference;

            MaterialProperties.Allocate(handle) = new MaterialProperties
            {
                FrictionCoefficient = 1f, // Default
                MaximumRecoveryVelocity = float.MaxValue, // Default
                SpringSettings = new SpringSettings(30, 1) // Default
            };

            return handle;
        }

        public StaticHandle AddStaticBox(float width, float height, float length, XnaVector3 initialPosition, XnaQuaternion initialRotation, IColisionable collidableReference)
        {
            var boxShape = new Box(width, height, length);
            var shapeIndex = Simulation.Shapes.Add(boxShape);

            var staticDescription = new StaticDescription(
                initialPosition.ToBepuVector3(),
                initialRotation.ToBepuQuaternion(),
                shapeIndex,
                continuity: ContinuousDetection.Passive);

            var handle = Simulation.Statics.Add(staticDescription);

            CollidableReferences[new CollidableReference(handle)] = collidableReference;

            MaterialProperties.Allocate(handle) = new MaterialProperties
            {
                FrictionCoefficient = 1f,
                MaximumRecoveryVelocity = float.MaxValue,
                SpringSettings = new SpringSettings(30, 1)
            };

            return handle;
        }

        public StaticHandle AddStaticCylinder(float length, float radius, XnaVector3 initialPosition, XnaQuaternion initialRotation, IColisionable collidableReference)
        {
            var cylinderShape = new Cylinder(radius, length);
            var shapeIndex = Simulation.Shapes.Add(cylinderShape);

            var rotationFix = XnaQuaternion.CreateFromAxisAngle(XnaVector3.Up, MathF.PI / 2f); // gira 90° en X

            var staticDescription = new StaticDescription(
                initialPosition.ToBepuVector3(),
                (initialRotation * rotationFix).ToBepuQuaternion(),
                shapeIndex,
                continuity: ContinuousDetection.Passive);

            var handle = Simulation.Statics.Add(staticDescription);

            CollidableReferences[new CollidableReference(handle)] = collidableReference;

            MaterialProperties.Allocate(handle) = new MaterialProperties
            {
                FrictionCoefficient = 1f, // Default
                MaximumRecoveryVelocity = float.MaxValue, // Default
                SpringSettings = new SpringSettings(30, 1) // Default
            };

            return handle;
        }

        public BodyHandle AddKinematicBox(float width, float height, float length, float mass, float friction, XnaVector3 initialPosition, XnaQuaternion initialRotation, IColisionable collidableReference)
        {
            var boxShape = new Box(width, height, length);
            var shapeIndex = Simulation.Shapes.Add(boxShape);

            var collidableDescription = new CollidableDescription(shapeIndex, ContinuousDetection.Passive);

            var bodyDescription = BodyDescription.CreateKinematic(
                new RigidPose(initialPosition.ToBepuVector3(), initialRotation.ToBepuQuaternion()),
                collidableDescription,
                new BodyActivityDescription(0.1f));

            var handle = Simulation.Bodies.Add(bodyDescription);

            CollidableReferences[new CollidableReference(CollidableMobility.Kinematic, handle)] = collidableReference;

            MaterialProperties.Allocate(handle) = new MaterialProperties
            {
                FrictionCoefficient = friction,
                MaximumRecoveryVelocity = 1f, // Default
                SpringSettings = new SpringSettings(30, 1) // Default
            };

            return handle;
        }

        public XnaVector3 GetPosition(BodyHandle bodyHandle)
        {
            var bodyRef = Simulation.Bodies.GetBodyReference(bodyHandle);
            return bodyRef.Pose.Position.ToXnaVector3();
        }

        public XnaVector3 GetPosition(StaticHandle staticHandle)
        {
            var bodyRef = Simulation.Statics.GetStaticReference(staticHandle);
            return bodyRef.Pose.Position.ToXnaVector3();
        }

        public XnaQuaternion GetOrientation(BodyHandle bodyHandle)
        {
            var bodyRef = Simulation.Bodies.GetBodyReference(bodyHandle);
            return bodyRef.Pose.Orientation.ToXnaQuaternion();
        }

        public XnaQuaternion GetOrientation(StaticHandle staticHandle)
        {
            var bodyRef = Simulation.Statics.GetStaticReference(staticHandle);
            return bodyRef.Pose.Orientation.ToXnaQuaternion();
        }

        public void Awake(BodyHandle bodyHandle)
        {
            var bodyRef = Simulation.Bodies.GetBodyReference(bodyHandle);
            bodyRef.Awake = true;
        }

        public XnaVector3 GetLinearVelocity(BodyHandle bodyHandle)
        {
            var bodyRef = Simulation.Bodies.GetBodyReference(bodyHandle);
            return bodyRef.Velocity.Linear.ToXnaVector3();
        }

        public XnaVector3 GetAngularVelocity(BodyHandle bodyHandle)
        {
            var bodyRef = Simulation.Bodies.GetBodyReference(bodyHandle);
            return bodyRef.Velocity.Angular.ToXnaVector3();
        }

        public void ApplyImpulse(BodyHandle bodyHandle, XnaVector3 impulseDirection, XnaVector3 impulseOffset, float impulseForce, float deltaTime)
        {
            var bodyRef = Simulation.Bodies.GetBodyReference(bodyHandle);
            var impulse = impulseDirection.ToBepuVector3() * impulseForce * deltaTime * bodyRef.LocalInertia.InverseMass;

            bodyRef.ApplyImpulse(impulse, bodyRef.Pose.Position + impulseOffset.ToBepuVector3());
        }

        public void SetPosition(BodyHandle bodyHandle, XnaVector3 newPosition)
        {
            var body = Simulation.Bodies.GetBodyReference(bodyHandle);
            body.Pose.Position = newPosition.ToBepuVector3();
            body.Velocity.Linear = BepuVector3.Zero;
        }

        public void RemoveBoundingVolume(BodyHandle bodyHandle)
        {
            Simulation.Bodies.Remove(bodyHandle);
        }

        public CollidableReference RayCast(XnaVector3 origin, XnaVector3 direction, float maxDistance)
        {
            var closestRayHitHandler = new ClosestRayHitHandler();
            Simulation.RayCast(origin.ToBepuVector3(), direction.ToBepuVector3(), maxDistance, ref closestRayHitHandler);

            return closestRayHitHandler.HitCollidable;
        }
    }
}