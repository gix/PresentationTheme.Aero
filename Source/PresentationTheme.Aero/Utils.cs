namespace PresentationTheme.Aero
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal static class Utils
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public UIntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEM_INFO
        {
            public short wProcessorArchitecture;
            public short wReserved;
            public uint dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public UIntPtr dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public short wProcessorLevel;
            public short wProcessorRevision;
        }

        [DllImport("kernel32", ExactSpelling = true)]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32")]
        private static extern void GetNativeSystemInfo(ref SYSTEM_INFO lpSystemInfo);

        [DllImport("kernel32", ExactSpelling = true, SetLastError = true)]
        private static extern UIntPtr VirtualQuery(
            IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer,
            IntPtr dwLength);

        [DllImport("kernel32", ExactSpelling = true, SetLastError = true)]
        private static extern bool VirtualProtect(
            IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect,
            out uint lpflOldProtect);

        [DllImport("kernel32", ExactSpelling = true, SetLastError = true)]
        private static extern bool FlushInstructionCache(
            IntPtr hProcess, IntPtr lpBaseAddress, UIntPtr dwSize);

        private static UIntPtr VirtualQuery(
            IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer)
        {
            return VirtualQuery(
                lpAddress, out lpBuffer,
                (IntPtr)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION)));
        }

        private const int PAGE_EXECUTE_READWRITE = 0x40;

        private const int PROCESSOR_ARCHITECTURE_AMD64 = 9;
        private const int PROCESSOR_ARCHITECTURE_ARM = 5;
        private const int PROCESSOR_ARCHITECTURE_IA64 = 6;
        private const int PROCESSOR_ARCHITECTURE_INTEL = 0;

        private static ProcessorArchitecture GetProcessorArchitecture()
        {
            var si = new SYSTEM_INFO();
            GetNativeSystemInfo(ref si);

            switch (si.wProcessorArchitecture) {
                case PROCESSOR_ARCHITECTURE_AMD64:
                    return ProcessorArchitecture.Amd64;

                case PROCESSOR_ARCHITECTURE_IA64:
                    return ProcessorArchitecture.IA64;

                case PROCESSOR_ARCHITECTURE_INTEL:
                    return ProcessorArchitecture.X86;

                case PROCESSOR_ARCHITECTURE_ARM:
                    return ProcessorArchitecture.Arm;

                default:
                    return ProcessorArchitecture.None;
            }
        }

        public static MemoryPatch HookMethod(MethodInfo source, MethodInfo target)
        {
            if (!EqualSignatures(source, target))
                throw new ArgumentException("The method signatures are not the same.", nameof(source));

            RuntimeHelpers.PrepareMethod(source.MethodHandle);
            RuntimeHelpers.PrepareMethod(target.MethodHandle);
            IntPtr srcAddr = source.MethodHandle.GetFunctionPointer();
            IntPtr tgtAddr = target.MethodHandle.GetFunctionPointer();

            var arch = GetProcessorArchitecture();

            long offset;
            byte[] jumpInst;
            switch (arch) {
                case ProcessorArchitecture.Amd64:
                    offset = tgtAddr.ToInt64() - srcAddr.ToInt64() - 5;
                    if (offset >= Int32.MinValue && offset <= Int32.MaxValue) {
                        jumpInst = new byte[] {
                            0xE9, // JMP rel32
                            (byte)(offset & 0xFF),
                            (byte)((offset >> 8) & 0xFF),
                            (byte)((offset >> 16) & 0xFF),
                            (byte)((offset >> 24) & 0xFF)
                        };
                    } else {
                        offset = tgtAddr.ToInt64();
                        jumpInst = new byte[] {
                            0x48, 0xB8, // MOV moffs64,rax
                            (byte)(offset & 0xFF),
                            (byte)((offset >> 8) & 0xFF),
                            (byte)((offset >> 16) & 0xFF),
                            (byte)((offset >> 24) & 0xFF),
                            (byte)((offset >> 32) & 0xFF),
                            (byte)((offset >> 40) & 0xFF),
                            (byte)((offset >> 48) & 0xFF),
                            (byte)((offset >> 56) & 0xFF),
                            0xFF, 0xE0 // JMP rax
                        };
                    }
                    break;

                case ProcessorArchitecture.X86:
                    offset = tgtAddr.ToInt32() - srcAddr.ToInt32() - 5;
                    jumpInst = new byte[] {
                        0xE9, // JMP rel32
                        (byte)(offset & 0xFF),
                        (byte)((offset >> 8) & 0xFF),
                        (byte)((offset >> 16) & 0xFF),
                        (byte)((offset >> 24) & 0xFF)
                    };
                    break;

                default:
                    throw new NotSupportedException(
                        $"Processor architecture {arch} is not supported.");
            }

            return PatchMemory(srcAddr, jumpInst);
        }

        public sealed class MemoryPatch : IDisposable
        {
            private readonly IntPtr address;
            private readonly byte[] backupInstructions;

            public MemoryPatch(IntPtr address, byte[] backupInstructions)
            {
                this.address = address;
                this.backupInstructions = backupInstructions;
            }

            public void Dispose()
            {
                WriteMemory(address, backupInstructions);
            }
        }

        private static MemoryPatch PatchMemory(IntPtr address, byte[] instructions)
        {
            var backup = new byte[instructions.Length];
            Marshal.Copy(address, backup, 0, backup.Length);
            if (backup.Any(x => x == 0xCC))
                throw new InvalidOperationException(
                    "Refusing to patch memory due to breakpoints/INT3 in target memory.");

            WriteMemory(address, instructions);
            return new MemoryPatch(address, backup);
        }

        private static void WriteMemory(IntPtr address, byte[] instructions)
        {
            if (VirtualQuery(address, out MEMORY_BASIC_INFORMATION mbi) == UIntPtr.Zero)
                throw new Win32Exception();

            RuntimeHelpers.PrepareConstrainedRegions();
            try {
            } finally {
                if (!VirtualProtect(mbi.BaseAddress, mbi.RegionSize, PAGE_EXECUTE_READWRITE, out uint oldProtect))
                    throw new Win32Exception();
                Marshal.Copy(instructions, 0, address, instructions.Length);
                FlushInstructionCache(GetCurrentProcess(), address, (UIntPtr)instructions.Length);
                VirtualProtect(mbi.BaseAddress, mbi.RegionSize, oldProtect, out oldProtect);
            }
        }

        private static bool EqualSignatures(MethodInfo x, MethodInfo y)
        {
            if (x.CallingConvention != y.CallingConvention)
                return false;

            if (x.ReturnType != y.ReturnType)
                return false;

            ParameterInfo[] paramsX = x.GetParameters();
            ParameterInfo[] paramsY = y.GetParameters();
            if (paramsX.Length != paramsY.Length)
                return false;

            for (int i = 0; i < paramsX.Length; ++i) {
                if (paramsX[i].ParameterType != paramsY[i].ParameterType)
                    return false;
            }

            return true;
        }

        public static void CreateDelegate<T>(this ConstructorInfo ctor, out T lambda)
        {
            var parameters = typeof(T).GetMethod("Invoke").GetParameters().Select(
                x => Expression.Parameter(x.ParameterType)).ToArray();
            lambda = Expression.Lambda<T>(
                Expression.New(ctor, parameters), parameters).Compile();
        }

        public static void CreateDelegate<T>(this MethodInfo method, out T lambda)
        {
            var parameters = typeof(T).GetMethod("Invoke").GetParameters().Select(
                x => Expression.Parameter(x.ParameterType)).ToArray();

            if (method.IsStatic) {
                lambda = Expression.Lambda<T>(
                    Expression.Call(method, parameters), parameters).Compile();
            } else {
                Expression instance = parameters[0];
                if (instance.Type != method.DeclaringType)
                    instance = Expression.Convert(instance, method.DeclaringType);

                lambda = Expression.Lambda<T>(
                    Expression.Call(instance, method, parameters.Skip(1)), parameters).Compile();
            }
        }

        public static void CreateDelegate<T>(
            this PropertyInfo property, out T getter)
        {
            getter = CreateGetter<T>(property);
        }

        public static void CreateDelegate<T1, T2>(
            this PropertyInfo property, out T1 getter, out T2 setter)
        {
            getter = CreateGetter<T1>(property);
            setter = CreateSetter<T2>(property);
        }

        public static void CreateDelegate<T, TValue>(
            this FieldInfo field, out Func<T, TValue> getter, out Action<T, TValue> setter)
        {
            var instExpr = Expression.Parameter(typeof(T));
            var valueExpr = Expression.Parameter(field.FieldType);

            Expression fieldExpr;
            if (field.DeclaringType != typeof(T)) {
                if (typeof(T) != typeof(object))
                    throw new InvalidOperationException();

                fieldExpr = Expression.Field(
                    Expression.Convert(instExpr, field.DeclaringType), field);
            } else {
                fieldExpr = Expression.Field(instExpr, field);
            }

            getter = Expression.Lambda<Func<T, TValue>>(fieldExpr, instExpr).Compile();
            setter = Expression.Lambda<Action<T, TValue>>(
                Expression.Assign(fieldExpr, valueExpr), instExpr, valueExpr).Compile();
        }

        public static void CreateDelegate<TValue>(
            this FieldInfo field, out Func<TValue> getter, out Action<TValue> setter)
        {
            if (!field.IsStatic)
                throw new InvalidOperationException();

            var valueExpr = Expression.Parameter(field.FieldType);
            var fieldExpr = Expression.Field(null, field);

            getter = Expression.Lambda<Func<TValue>>(fieldExpr).Compile();
            setter = Expression.Lambda<Action<TValue>>(
                Expression.Assign(fieldExpr, valueExpr), valueExpr).Compile();
        }


        private static T CreateGetter<T>(this PropertyInfo property)
        {
            var getMethod = property.GetMethod;
            if (getMethod == null)
                throw new ArgumentException("Property has no getter", nameof(property));

            var lambdaInvoke = typeof(T).GetMethod("Invoke");
            var parameters = lambdaInvoke.GetParameters().Select(
                x => Expression.Parameter(x.ParameterType)).ToArray();

            var signatureMatches = lambdaInvoke.ReturnType == property.PropertyType;
            if (getMethod.IsStatic)
                signatureMatches &= parameters.Length == 1 &&
                                    property.DeclaringType == parameters[0].Type;
            else
                signatureMatches &= parameters.Length == 0;

            if (signatureMatches)
                return (T)(object)getMethod.CreateDelegate(typeof(T));

            if (getMethod.IsStatic)
                return Expression.Lambda<T>(
                    Expression.Property(null, property), parameters).Compile();

            Expression instance = parameters[0];
            if (instance.Type != property.DeclaringType)
                instance = Expression.Convert(instance, property.DeclaringType);

            return Expression.Lambda<T>(
                Expression.Property(instance, property), parameters).Compile();
        }

        private static T CreateSetter<T>(this PropertyInfo property)
        {
            var setMethod = property.SetMethod;
            if (setMethod == null)
                throw new ArgumentException("Property has no setter", nameof(property));

            var lambdaInvoke = typeof(T).GetMethod("Invoke");
            var parameters = lambdaInvoke.GetParameters().Select(
                x => Expression.Parameter(x.ParameterType)).ToArray();

            var signatureMatches = lambdaInvoke.ReturnType == property.PropertyType;
            if (property.GetMethod.IsStatic)
                signatureMatches &= parameters.Length == 1 &&
                                    property.DeclaringType == parameters[0].Type;
            else
                signatureMatches &= parameters.Length == 0;

            if (signatureMatches)
                return (T)(object)setMethod.CreateDelegate(typeof(T));

            if (setMethod.IsStatic)
                return Expression.Lambda<T>(
                    Expression.Call(setMethod, parameters), parameters).Compile();

            Expression instance = parameters[0];
            if (instance.Type != property.DeclaringType)
                instance = Expression.Convert(instance, property.DeclaringType);

            return Expression.Lambda<T>(
                Expression.Call(instance, setMethod, parameters), parameters).Compile();
        }
    }
}
