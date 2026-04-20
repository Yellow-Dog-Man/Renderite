using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;


namespace Renderite.Shared
{
    /// <summary>
    /// This describes a location in a particular shared memory buffer. This struct contains everything needed by
    /// the shared memory system to fully access the particular chunk of data. It's agnostic to what this data is,
    /// the responsibility to interpret that data is up to the system that is passing or receiving this descriptor.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    public struct SharedMemoryBufferDescriptor<T>
        where T : unmanaged
    {
        public bool IsEmpty => length == 0;

        // We put it in this way, so we can define the layout explicitly
        // We cannot define explicit layout on the struct itself
        SharedMemoryBufferDescriptor descriptor;

        public int bufferId
        {
            get => descriptor.bufferId;
            set => descriptor.bufferId = value;
        }

        public int bufferCapacity
        {
            get => descriptor.bufferCapacity;
            set => descriptor.bufferCapacity = value;
        }

        public int offset
        {
            get => descriptor.offset;
            set => descriptor.offset = value;
        }

        public int length
        {
            get => descriptor.length;
            set => descriptor.length = value;
        }

        public SharedMemoryBufferDescriptor(int bufferId, int bufferCapacity, int offset, int length)
        {
            this.bufferId = bufferId;
            this.bufferCapacity = bufferCapacity;
            this.offset = offset;
            this.length = length;
        }

        public SharedMemoryBufferDescriptor<O> As<O>()
            where O : unmanaged
        {
            return new SharedMemoryBufferDescriptor<O>(bufferId, bufferCapacity, offset, length);
        }

        public override string ToString()
        {
            return $"Shared Memory {typeof(T).Name}. BufferId: {bufferId}, Capacity: {bufferCapacity}, " +
                $"Offset: {offset}, Length: {length}";
        }
    }

    [StructLayout(LayoutKind.Explicit, Pack = 16)]
    public struct SharedMemoryBufferDescriptor
    {
        /// <summary>
        /// Unique ID of the buffer. This won't be the full ID - it requires additional keys defined by the manager
        /// but it's used to distinguish the buffers from each other within the system. This way when there are multiple
        /// buffers, it allows them to be accessed via this.
        /// </summary>
        [FieldOffset(0)]
        public int bufferId;

        /// <summary>
        /// This is the full capacity of the buffer in bytes. It's important that this is included, so when this
        /// block is first accessed, the buffer can be properly accessed
        /// </summary>
        [FieldOffset(4)]
        public int bufferCapacity;

        /// <summary>
        /// Byte offset at which data for this descriptor start in the shared memory.
        /// </summary>
        [FieldOffset(8)]
        public int offset;

        /// <summary>
        /// The length of the data in bytes.
        /// </summary>
        [FieldOffset(12)]
        public int length;
    }
}
