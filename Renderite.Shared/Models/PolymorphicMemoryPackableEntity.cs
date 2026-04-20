using System;
using System.Collections.Generic;
using System.Text;

namespace Renderite.Shared
{
    public abstract class PolymorphicMemoryPackableEntity<T> : IMemoryPackable
        where T : PolymorphicMemoryPackableEntity<T>
    {
        static bool initialized;

        static List<Type> types;
        static Dictionary<Type, int> typeToIndex;
        static List<Func<IMemoryPackerEntityPool, T>> poolBorrowers;
        static List<Action<IMemoryPackerEntityPool, T>> poolReturners;

        static T Allocate<A>(IMemoryPackerEntityPool pool) where A : T, new() => pool.Borrow<A>();
        static void Return<A>(IMemoryPackerEntityPool pool, T instance) where A : T, new() => pool.Return<A>((A)instance);

        public static void ReturnAuto(IMemoryPackerEntityPool pool, T instance)
        {
            var index = typeToIndex[instance.GetType()];
            poolReturners[index](pool, instance);
        }

        // This MUST be called by the derived class to initialize the data
        protected static void InitTypes(List<Type> types)
        {
            PolymorphicMemoryPackableEntity<T>.types = types;

            typeToIndex = new Dictionary<Type, int>();
            poolBorrowers = new List<Func<IMemoryPackerEntityPool, T>>();
            poolReturners = new List<Action<IMemoryPackerEntityPool, T>>();

            var allocator = typeof(PolymorphicMemoryPackableEntity<T>).GetMethod(nameof(Allocate),
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            var returner = typeof(PolymorphicMemoryPackableEntity<T>).GetMethod(nameof(Return),
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            for (int i = 0; i < types.Count; i++)
            {
                typeToIndex.Add(types[i], i);

                var allocatorInstance = allocator.MakeGenericMethod(types[i]);
                var returnerInstance = returner.MakeGenericMethod(types[i]);

                var allocatorDelegate = (Func<IMemoryPackerEntityPool, T>)
                    allocatorInstance.CreateDelegate(typeof(Func<IMemoryPackerEntityPool, T>));

                var returnerDelegate = (Action<IMemoryPackerEntityPool, T>)
                    returnerInstance.CreateDelegate(typeof(Action<IMemoryPackerEntityPool, T>));

                poolBorrowers.Add(allocatorDelegate);
                poolReturners.Add(returnerDelegate);
            }

            initialized = true;
        }

        public void Encode(ref MemoryPacker packer)
        {
            var typeIndex = typeToIndex[GetType()];

            packer.Write(typeIndex);

            // We assume the derived method will then serialize next
            Pack(ref packer);
        }

        public abstract void Pack(ref MemoryPacker packer);
        public abstract void Unpack(ref MemoryUnpacker unpacker);

        public static T Decode(ref MemoryUnpacker unpacker, T existingInstance = null)
        {
            // Ensure that the static constructor was called, because in some cases it isn't
            if (!initialized)
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);

            if (poolBorrowers == null)
                throw new InvalidOperationException($"Types were not initialized for polymorphic entity: {typeof(T).FullName}");

            if (unpacker.Pool == null)
                throw new ArgumentException($"MemoryUnpacker doesn't have a Pool instance for polymorphic entity: {typeof(T).FullName}");

            var index = unpacker.Read<int>();

            T instance;

            // Check if we can use the existing instance
            if (existingInstance != null && existingInstance.GetType() == types[index])
                instance = existingInstance;
            else
            {
                // The type is mismatch, return the existing instance
                if (existingInstance != null)
                    poolReturners[index](unpacker.Pool, existingInstance);

                var borrower = poolBorrowers[index];

                if (borrower == null)
                    throw new Exception($"Borrow is null for type with index {index} for polymorphic entity: {typeof(T).FullName}");

                instance = borrower(unpacker.Pool);
            }

            if(instance == null)
                throw new Exception($"Failed to allocate instance with index {index} for polymorphic entity: {typeof(T).FullName}");

            instance.Unpack(ref unpacker);

            return instance;
        }
    }
}
