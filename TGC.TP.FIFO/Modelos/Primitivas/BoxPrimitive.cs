﻿using Microsoft.Xna.Framework.Graphics;

namespace TGC.TP.FIFO.Modelos.Primitivas;

public class BoxPrimitive
{
    /// <summary>
    ///     Create a box with a center at the given point, with a size and a color in each vertex.
    /// </summary>
    /// <param name="graphicsDevice">Used to initialize and control the presentation of the graphics device.</param>
    /// <param name="size">Size of the box.</param>
    /// <param name="texture">The box texture.</param>
    public BoxPrimitive(GraphicsDevice graphicsDevice, XnaVector3 size)
    {
        CreateVertexBuffer(graphicsDevice, size);
        CreateIndexBuffer(graphicsDevice);
    }

    /// <summary>
    ///     Represents a list of 3D vertices to be streamed to the graphics device.
    /// </summary>
    private VertexBuffer Vertices { get; set; }

    /// <summary>
    ///     Describes the rendering order of the vertices in a vertex buffer.
    /// </summary>
    private IndexBuffer Indices { get; set; }

    /// <summary>
    ///     Built-in effect that supports optional texturing, vertex coloring, fog, and lighting.
    /// </summary>
    private BasicEffect Effect { get; }

    /// <summary>
    ///     Create a vertex buffer for the figure with the given information.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="size">Size of the box.</param>
    private void CreateVertexBuffer(GraphicsDevice graphicsDevice, XnaVector3 size)
    {
        var x = size.X / 2;
        var y = size.Y / 2;
        var z = size.Z / 2;

        var positions = new XnaVector3[]
        {
                // Back face
                new XnaVector3(x, -y, z),
                new XnaVector3(-x, -y, z),
                new XnaVector3(x, y, z),
                new XnaVector3(-x, y, z),
                
                // Front face
                new XnaVector3(x, y, -z),
                new XnaVector3(-x, y, -z),
                new XnaVector3(x, -y, -z),
                new XnaVector3(-x, -y, -z),
                
                // Top face
                new XnaVector3(x, y, z),
                new XnaVector3(-x, y, z),
                new XnaVector3(x, y, -z),
                new XnaVector3(-x, y, -z),
                
                // Bottom face
                new XnaVector3(x, -y, -z),
                new XnaVector3(x, -y, z),
                new XnaVector3(-x, -y, z),
                new XnaVector3(-x, -y, -z),
                
                // Left face
                new XnaVector3(-x, -y, z),
                new XnaVector3(-x, y, z),
                new XnaVector3(-x, y, -z),
                new XnaVector3(-x, -y, -z),
                
                // Right face
                new XnaVector3(x, -y, -z),
                new XnaVector3(x, y, -z),
                new XnaVector3(x, y, z),
                new XnaVector3(x, -y, z),
        };

        var textureCoordinates = new XnaVector2[]
        {
                // Back face
                XnaVector2.Zero,
                XnaVector2.UnitX,
                XnaVector2.UnitY,
                XnaVector2.One,
                
                // Front face
                XnaVector2.Zero,
                XnaVector2.UnitX,
                XnaVector2.UnitY,
                XnaVector2.One,
                
                // Top face
                XnaVector2.UnitX,
                XnaVector2.One,
                XnaVector2.Zero,
                XnaVector2.UnitY,
                
                // Bottom face
                XnaVector2.Zero,
                XnaVector2.UnitX,
                XnaVector2.One,
                XnaVector2.UnitY,
                
                // Left face
                XnaVector2.Zero,
                XnaVector2.UnitY,
                XnaVector2.One,
                XnaVector2.UnitX,
                
                // Right face
                XnaVector2.Zero,
                XnaVector2.UnitY,
                XnaVector2.One,
                XnaVector2.UnitX,
        };

        var normals = new XnaVector3[]
        {
                // Back face
                XnaVector3.Backward,
                XnaVector3.Backward,
                XnaVector3.Backward,
                XnaVector3.Backward,
                
                // Front face
                XnaVector3.Forward,
                XnaVector3.Forward,
                XnaVector3.Forward,
                XnaVector3.Forward,

                // Top face
                XnaVector3.Up,
                XnaVector3.Up,
                XnaVector3.Up,
                XnaVector3.Up,
                
                // Bottom face
                XnaVector3.Down,
                XnaVector3.Down,
                XnaVector3.Down,
                XnaVector3.Down,
                
                // Left face
                XnaVector3.Left,
                XnaVector3.Left,
                XnaVector3.Left,
                XnaVector3.Left,
                
                // Right face
                XnaVector3.Right,
                XnaVector3.Right,
                XnaVector3.Right,
                XnaVector3.Right,
        };

        var vertices = new VertexPositionNormalTexture[positions.Length];

        for (int index = 0; index < vertices.Length; index++)
            vertices[index] = new VertexPositionNormalTexture(positions[index], normals[index], textureCoordinates[index]);


        Vertices = new VertexBuffer(graphicsDevice, VertexPositionNormalTexture.VertexDeclaration, vertices.Length,
                BufferUsage.None);
        Vertices.SetData(vertices);
    }

    /// <summary>
    ///     Create an index buffer for the vertex buffer that the figure has.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    private void CreateIndexBuffer(GraphicsDevice graphicsDevice)
    {
        var indices = new ushort[]
        {
                
                // Back face
                1, 2, 0,
                1, 3, 2,
                
                // Front face
                5, 6, 4,
                5, 7, 6,
                
                // Top face
                9, 10, 8,
                9, 11, 10,
                
                // Bottom face
                12, 15, 13,
                13, 15, 14,
                
                // Left face
                17, 16, 19,
                17, 19, 18,
                
                // Right face
                20, 23, 21,
                21, 23, 22,
        };


        Indices = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, indices.Length,
            BufferUsage.None);
        Indices.SetData(indices);
    }

    public void Draw(Effect effect)
    {
        var graphicsDevice = effect.GraphicsDevice;

        // Set our vertex declaration, vertex buffer, and index buffer.
        graphicsDevice.SetVertexBuffer(Vertices);

        graphicsDevice.Indices = Indices;

        foreach (var effectPass in effect.CurrentTechnique.Passes)
        {
            effectPass.Apply();

            var primitiveCount = Indices.IndexCount / 3;

            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, primitiveCount);
        }
    }
}
