﻿using System.Numerics;
using System.Runtime.CompilerServices;
using HLView.Graphics.Primitives;
using Veldrid;

namespace HLView.Graphics.Pipelines
{
    public class ModelRenderPipeline : IRenderPipeline
    {
        private Pipeline _pipeline;
        private DeviceBuffer _projectionBuffer;
        private ResourceSet _projectionResourceSet;

        public string Name => "Model";
        public int Order => 10;
        
        public void CreateResources(SceneContext sc)
        {
            var transformsLayout = sc.ResourceCache.GetResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("uTransforms", ResourceKind.UniformBuffer, ShaderStages.Vertex)
                )
            );

            var vertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("vPosition", VertexElementSemantic.Position, VertexElementFormat.Float3),
                new VertexElementDescription("vNormal", VertexElementSemantic.Normal, VertexElementFormat.Float3),
                new VertexElementDescription("vTexture", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("vBone", VertexElementSemantic.Position, VertexElementFormat.UInt1)
            );

            var (vertex, fragment) = sc.ResourceCache.GetShaders("model");

            var pDesc = new GraphicsPipelineDescription
            {
                BlendState = BlendStateDescription.SingleAlphaBlend,
                DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerState = new RasterizerStateDescription(FaceCullMode.Back, PolygonFillMode.Solid, FrontFace.Clockwise, true, false),
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                ResourceLayouts = new[] { sc.ResourceCache.ProjectionLayout, sc.ResourceCache.TextureLayout, transformsLayout },
                ShaderSet = new ShaderSetDescription(new[] { vertexLayout }, new[] { vertex, fragment }),
                Outputs = new OutputDescription
                {
                    ColorAttachments = new[] { new OutputAttachmentDescription(PixelFormat.B8_G8_R8_A8_UNorm) },
                    DepthAttachment = new OutputAttachmentDescription(PixelFormat.R32_Float),
                    SampleCount = TextureSampleCount.Count1
                }
            };

            _pipeline = sc.ResourceCache.GetPipeline(ref pDesc);

            _projectionBuffer = sc.Device.ResourceFactory.CreateBuffer(
                new BufferDescription((uint)Unsafe.SizeOf<UniformProjection>(), BufferUsage.UniformBuffer)
            );

            _projectionResourceSet = sc.ResourceCache.GetResourceSet(
                new ResourceSetDescription(sc.ResourceCache.ProjectionLayout, _projectionBuffer)
            );
        }

        public void SetPipeline(CommandList cl, SceneContext sc, IRenderContext context)
        {
            sc.Device.UpdateBuffer(_projectionBuffer, 0, new UniformProjection
            {
                Model = Matrix4x4.Identity,
                View = context.Camera.View,
                Projection = context.Camera.Projection,
            });

            cl.SetPipeline(_pipeline);
            cl.SetGraphicsResourceSet(0, _projectionResourceSet);
        }

        public void DisposeResources(SceneContext sc)
        {
            _projectionBuffer.Dispose();
        }
    }
}