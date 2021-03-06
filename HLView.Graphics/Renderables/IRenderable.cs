﻿using System.Numerics;
using Veldrid;

namespace HLView.Graphics.Renderables
{
    public interface IRenderable : IUpdateable
    {
        int Order { get; }
        string Pipeline { get; }
        float DistanceFrom(Vector3 location);
        void CreateResources(SceneContext sc);
        void Render(SceneContext sc, CommandList cl, IRenderContext rc);
        void RenderAlpha(SceneContext sc, CommandList cl, IRenderContext rc, Vector3 cameraLocation);
        void DisposeResources(SceneContext sc);
    }
}