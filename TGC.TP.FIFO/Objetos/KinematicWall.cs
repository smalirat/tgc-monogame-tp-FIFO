﻿using BepuPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TGC.TP.FIFO.Cameras;
using TGC.TP.FIFO.Efectos;
using TGC.TP.FIFO.Fisica;
using TGC.TP.FIFO.Modelos;
using TGC.TP.FIFO.Modelos.Primitivas;
using TGC.TP.FIFO.Texturas;

namespace TGC.TP.FIFO.Objetos;

public class KinematicWall : IColisionable
{
    private readonly ModelManager modelManager;
    private readonly EffectManager effectManager;
    private readonly PhysicsManager physicsManager;
    private readonly TextureManager textureManager;

    private readonly BodyHandle boundingVolume;
    private readonly BoxPrimitive model;

    private XnaMatrix world;

    public BodyType BodyType => BodyType.FloorRamp;

    private float tiempo;

    public bool CanPlayerBallJumpOnIt { get; private set; }

    private const float Height = 1.25f;

    private float movementMultiplier;

    public KinematicWall(ModelManager modelManager,
        EffectManager effectManager,
        PhysicsManager physicsManager,
        TextureManager textureManager,
        GraphicsDevice graphicsDevice,
        XnaVector3 position,
        float width,
        float length,
        float mass,
        float friction,
        bool canPlayerBallJumpOnIt,
        float movementMultiplier)
    {
        this.modelManager = modelManager;
        this.effectManager = effectManager;
        this.physicsManager = physicsManager;
        this.textureManager = textureManager;

        CanPlayerBallJumpOnIt = canPlayerBallJumpOnIt;
        this.movementMultiplier = movementMultiplier;

        var rotation = XnaQuaternion.CreateFromAxisAngle(XnaVector3.Right, MathF.PI / 2f);

        model = this.modelManager.CreateBox(graphicsDevice, Height, width, length);
        boundingVolume = this.physicsManager.AddKinematicBox(width, Height, length, mass, friction, position, rotation, this);

        world = XnaMatrix.CreateTranslation(position) * XnaMatrix.CreateFromQuaternion(rotation);
        this.movementMultiplier = movementMultiplier;
    }

    public void Draw(XnaMatrix view, XnaMatrix projection)
    {
        var effect = effectManager.BasicTextureShader;

        effect.Parameters["View"]?.SetValue(view);
        effect.Parameters["Projection"]?.SetValue(projection);
        effect.Parameters["World"]?.SetValue(world);
        effect.Parameters["DiffuseColor"]?.SetValue(Color.Black.ToVector3());
        effect.Parameters["ModelTexture"].SetValue(textureManager.WoodBox2Texture);
        effect.Parameters["UVScale"].SetValue(1f);

        model.Draw(effect);
    }

    public void Update(float deltaTime, TargetCamera camera)
    {
        physicsManager.Awake(boundingVolume);

        tiempo += deltaTime;
        float x = MathF.Sin(tiempo) * movementMultiplier;

        var previousPosition = physicsManager.GetPosition(boundingVolume);

        physicsManager.SetPosition(boundingVolume, new BepuVector3(x, previousPosition.Y, previousPosition.Z));

        // Actualizo matriz mundo
        world = XnaMatrix.CreateFromQuaternion(physicsManager.GetOrientation(boundingVolume)) *
                XnaMatrix.CreateTranslation(physicsManager.GetPosition(boundingVolume));
    }

    public void NotifyCollition(IColisionable with)
    {
    }
}