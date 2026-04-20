using System;
using System.Collections.Generic;
using System.Text;


namespace Renderite.Shared
{
    public class MaterialPropertyIdResult : RendererCommand
    {
        /// <summary>
        /// Uniquely identifying request ID
        /// </summary>
        
        public int requestId;

        /// <summary>
        /// The list of property ID's matching the names in the request
        /// </summary>
        
        public List<int> propertyIDs = new List<int>();

        public override void Pack(ref MemoryPacker packer)
        {
            packer.Write(requestId);
            packer.WriteValueList(propertyIDs);
        }

        public override void Unpack(ref MemoryUnpacker packer)
        {
            packer.Read(ref requestId);
            packer.ReadValueList(ref propertyIDs);
        }
    }
}
