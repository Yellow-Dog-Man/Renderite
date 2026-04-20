using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class MaterialPropertyIdRequest : RendererCommand
    {
        /// <summary>
        /// Uniquely identifying request ID
        /// </summary>
        
        public int requestId;

        /// <summary>
        /// The list of property names that are being requested
        /// </summary>
        
        public List<string> propertyNames = new List<string>();

        public override void Pack(ref MemoryPacker packer)
        {
            packer.Write(requestId);
            packer.WriteStringList(propertyNames);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            packer.Read(ref requestId);
            packer.ReadStringList(ref propertyNames);
        }
    }
}
