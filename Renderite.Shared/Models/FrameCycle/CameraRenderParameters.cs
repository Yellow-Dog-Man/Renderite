using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class CameraRenderParameters : IMemoryPackable
    {
        /// <summary>
        /// Resolution of the texture to render
        /// </summary>
        
        public RenderVector2i resolution;

        /// <summary>
        /// Format of the texture to render to
        /// </summary>
        
        public TextureFormat textureFormat = TextureFormat.ARGB32;

        /// <summary>
        /// What projetion to use
        /// </summary>
        
        public CameraProjection projection = CameraProjection.Perspective;

        /// <summary>
        /// What field of view to render with. 360 will do 360 equirectangular render
        /// </summary>
        
        public float fov = 60;

        /// <summary>
        /// In orthographic mode this controls the size
        /// </summary>
        
        public float orthographicSize = 8;

        /// <summary>
        /// How is the camera cleared
        /// </summary>
        
        public CameraClearMode clearMode = CameraClearMode.Skybox;

        /// <summary>
        /// When the clear is solid color, this indicates th color to use
        /// </summary>
        
        public RenderVector4 clearColor = default;

        /// <summary>
        /// Near clip to use
        /// </summary>
        
        public float nearClip = 0.01f;

        /// <summary>
        /// Far clip to use
        /// </summary>
        
        public float farClip = 2048f;

        /// <summary>
        /// Should private UI be rendered?
        /// </summary>
        
        public bool renderPrivateUI;

        /// <summary>
        /// Render with post processing effects
        /// </summary>
        
        public bool postProcessing = true;

        /// <summary>
        /// Should SSR be used?
        /// </summary>
        
        public bool screenSpaceReflections;

        public void Pack(ref MemoryPacker packer)
        {
            packer.Write(resolution);
            packer.Write(textureFormat);
            packer.Write(projection);
            packer.Write(fov);
            packer.Write(orthographicSize);
            packer.Write(clearMode);
            packer.Write(clearColor);
            packer.Write(nearClip);
            packer.Write(farClip);
            packer.Write(renderPrivateUI);
            packer.Write(postProcessing);
            packer.Write(screenSpaceReflections);
        }

        public void Unpack(ref MemoryUnpacker packer)
        {
            packer.Read(ref resolution);
            packer.Read(ref textureFormat);
            packer.Read(ref projection);
            packer.Read(ref fov);
            packer.Read(ref orthographicSize);
            packer.Read(ref clearMode);
            packer.Read(ref clearColor);
            packer.Read(ref nearClip);
            packer.Read(ref farClip);
            packer.Read(ref renderPrivateUI);
            packer.Read(ref postProcessing);
            packer.Read(ref screenSpaceReflections);
        }
    }
}
