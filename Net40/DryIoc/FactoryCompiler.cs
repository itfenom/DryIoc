﻿/*
The MIT License (MIT)

Copyright (c) 2013 Maksim Volkau

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace DryIoc
{
    public static partial class FactoryCompiler
    {
        public const string DYNAMIC_ASSEMBLY_NAME = "DryIoc.DynamicAssemblyWithCompiledFactories";
        public const string DYNAMIC_ASSEMBLY_NAME_WITH_PUBLIC_KEY =
            DYNAMIC_ASSEMBLY_NAME + ",PublicKey=" + STRONG_NAME_PUBLIC_KEY;

        public const string STRONG_NAME_PUBLIC_KEY =
            "0024000004800000940000000602000000240000525341310004000001000100c3ee5dd15505ae" +
            "d491f6effe157e3ec3694e4ec3a532d3c16e497ab1b0c3ca9fb2959d870e24831b600b576e66b8" +
            "2dda14f0fd88860d8ea05547454b7fc77201d2082fb320d5e609bbaf853a16d5ac459a9585af6b" +
            "48c796b22ebb70472c5412c997f68d6e5a044de3b0de7b95d1569ee57bf72469f23c748f5879e5" +
            "0a8d50b2";

        public static byte[] StrongNameKeyPairBytes { get { return new byte[]
        {
            0x07, 0x02, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x52, 0x53, 0x41, 0x32, 0x00, 0x04, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0xC3, 0xEE, 0x5D, 0xD1, 
            0x55, 0x05, 0xAE, 0xD4, 0x91, 0xF6, 0xEF, 0xFE, 0x15, 0x7E, 0x3E, 0xC3, 0x69, 0x4E, 0x4E, 0xC3, 0xA5, 0x32, 0xD3, 0xC1, 0x6E, 0x49, 0x7A, 0xB1, 
            0xB0, 0xC3, 0xCA, 0x9F, 0xB2, 0x95, 0x9D, 0x87, 0x0E, 0x24, 0x83, 0x1B, 0x60, 0x0B, 0x57, 0x6E, 0x66, 0xB8, 0x2D, 0xDA, 0x14, 0xF0, 0xFD, 0x88, 
            0x86, 0x0D, 0x8E, 0xA0, 0x55, 0x47, 0x45, 0x4B, 0x7F, 0xC7, 0x72, 0x01, 0xD2, 0x08, 0x2F, 0xB3, 0x20, 0xD5, 0xE6, 0x09, 0xBB, 0xAF, 0x85, 0x3A, 
            0x16, 0xD5, 0xAC, 0x45, 0x9A, 0x95, 0x85, 0xAF, 0x6B, 0x48, 0xC7, 0x96, 0xB2, 0x2E, 0xBB, 0x70, 0x47, 0x2C, 0x54, 0x12, 0xC9, 0x97, 0xF6, 0x8D, 
            0x6E, 0x5A, 0x04, 0x4D, 0xE3, 0xB0, 0xDE, 0x7B, 0x95, 0xD1, 0x56, 0x9E, 0xE5, 0x7B, 0xF7, 0x24, 0x69, 0xF2, 0x3C, 0x74, 0x8F, 0x58, 0x79, 0xE5, 
            0x0A, 0x8D, 0x50, 0xB2, 0xE5, 0x99, 0xA8, 0x1A, 0x10, 0x69, 0x3F, 0x00, 0x24, 0x64, 0xBF, 0x8C, 0x38, 0x0B, 0x5E, 0xE9, 0xAC, 0x06, 0x90, 0x3E, 
            0x35, 0x1F, 0xAF, 0x82, 0x6B, 0xFA, 0x07, 0x5D, 0x18, 0xB5, 0xE4, 0x92, 0xF4, 0x2F, 0x02, 0x05, 0xFB, 0x3B, 0x5E, 0x65, 0x34, 0xDA, 0xE2, 0x5A, 
            0x88, 0x4C, 0x64, 0xF7, 0x9F, 0x3C, 0x04, 0x96, 0x57, 0x71, 0x2F, 0xAD, 0xC6, 0xD0, 0xF1, 0x60, 0x9F, 0x2D, 0xB8, 0xE8, 0x87, 0x3B, 0x39, 0xE7, 
            0x6F, 0xFF, 0x39, 0x81, 0x5F, 0x4C, 0xE1, 0xEE, 0x15, 0xC1, 0x71, 0x43, 0x0D, 0x9B, 0x35, 0xBE, 0x66, 0x94, 0xD3, 0x7D, 0xE7, 0xA3, 0x63, 0x67, 
            0xD3, 0x00, 0xFE, 0x5C, 0x54, 0xDA, 0xF5, 0x5C, 0x31, 0x7B, 0x2D, 0xE3, 0xF6, 0x8C, 0x58, 0x59, 0xA7, 0xCB, 0x27, 0x95, 0x91, 0xF4, 0x87, 0x48, 
            0xDA, 0x95, 0xA1, 0x4D, 0xE2, 0x7F, 0x09, 0xB9, 0x42, 0x18, 0x27, 0xC4, 0x0D, 0x93, 0xC1, 0xA3, 0x8F, 0xE5, 0x62, 0x2C, 0x9A, 0x3B, 0x09, 0xA4, 
            0xC9, 0x47, 0xCF, 0x11, 0xEC, 0x5E, 0x6E, 0x9F, 0x92, 0xD9, 0x08, 0x61, 0x61, 0x2F, 0x25, 0xCC, 0x60, 0x25, 0xCF, 0x3C, 0x99, 0xFB, 0xB8, 0x3C, 
            0xA0, 0x3C, 0xB4, 0x56, 0x39, 0x33, 0x78, 0xD7, 0x34, 0xB1, 0x90, 0xF9, 0xA5, 0x24, 0xB0, 0xF4, 0x93, 0xBD, 0x16, 0x38, 0xD0, 0x76, 0x2A, 0xCA, 
            0x9F, 0x4A, 0x16, 0xB4, 0xE9, 0xFB, 0x24, 0x6E, 0x23, 0x9A, 0xBA, 0xB6, 0xC5, 0x84, 0x6F, 0x65, 0x55, 0x69, 0x93, 0x4D, 0x5A, 0x9C, 0x31, 0xFC, 
            0xBA, 0x08, 0xAC, 0x20, 0x56, 0x73, 0x0D, 0x06, 0x3E, 0x05, 0x1F, 0xB9, 0xF9, 0xCE, 0x06, 0xA8, 0x9A, 0x47, 0x40, 0x78, 0x4B, 0xA9, 0x09, 0x92, 
            0x4B, 0x94, 0x05, 0x1A, 0xB7, 0xA6, 0x8F, 0xEA, 0x35, 0x91, 0x8E, 0x3B, 0x24, 0xD2, 0x29, 0x1C, 0x3E, 0x5B, 0xE8, 0x4E, 0xA7, 0xD0, 0xD2, 0x9D, 
            0x2A, 0x09, 0x0C, 0xB5, 0xAA, 0x24, 0xF6, 0xE6, 0xE2, 0x3E, 0xFB, 0x0A, 0x9D, 0xFE, 0xAA, 0xE3, 0xB0, 0xE1, 0x91, 0xB1, 0x55, 0xF8, 0x71, 0xD4, 
            0x3E, 0xC3, 0x8B, 0x60, 0x59, 0xB7, 0xD6, 0xC6, 0x14, 0x90, 0xC4, 0xCA, 0xA9, 0xD6, 0xCF, 0xBF, 0x07, 0x7C, 0x24, 0x04, 0x4B, 0xDC, 0xB0, 0xC6, 
            0x78, 0x4B, 0x72, 0x2B, 0xA3, 0x28, 0xFC, 0x41, 0xFC, 0x8D, 0x29, 0x44, 0x51, 0xC0, 0xFC, 0xF1, 0xB9, 0x86, 0x98, 0xCA, 0x42, 0x32, 0x11, 0x3C, 
            0x9C, 0xED, 0xE3, 0xCD, 0x5D, 0xC5, 0x69, 0xB4, 0x87, 0x7D, 0xE1, 0xDE, 0x55, 0x40, 0x4A, 0x7E, 0x80, 0x0B, 0x2B, 0x91, 0xBA, 0x27, 0x5E, 0x24, 
            0x93, 0x68, 0x8A, 0xA8, 0x32, 0x44, 0x33, 0x8D, 0x43, 0xF0, 0xAA, 0xEA, 0x1E, 0xDF, 0xDA, 0xDC, 0x9C, 0xD9, 0xF9, 0x34, 0x3F, 0x71, 0x5B, 0x7E, 
            0xD3, 0x0B, 0xD6, 0xF4, 0x7F, 0x31, 0xCC, 0x57, 0x53, 0x63, 0x72, 0x1B, 0xD9, 0x0A, 0x3C, 0x67, 0x25, 0xA3, 0xCE, 0x5E, 0x1A, 0xED, 0xC7, 0x2C, 
            0x24, 0xC8, 0x88, 0x17, 0x43, 0x59, 0x07, 0x7F, 0x8D, 0x7B, 0x5A, 0x5E, 0x29, 0x98, 0x26, 0xEF, 0xE2, 0xB0, 0xB9, 0xB2, 0xBA, 0x72, 0x69, 0xFB, 
            0x5C, 0xBE, 0x9B, 0xF3, 0x45, 0x3E, 0x00, 0x77, 0x64, 0x08, 0x5F, 0xAE, 0x8D, 0x96, 0xC0, 0x30, 0x1F, 0x41, 0xE6, 0x60
        }; }}

        static partial void CompileToMethod(Expression<FactoryDelegate> factoryExpression, Rules rules, ref FactoryDelegate result)
        {
            if (!rules.CompilationToDynamicAssemblyEnabled)
                return;

            result.ThrowIf(result != null);

            var typeName = "Factory" + Interlocked.Increment(ref _typeId);
            var typeBuilder = GetDynamicAssemblyModuleBuilder().DefineType(typeName, TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Abstract);

            var methodBuilder = typeBuilder.DefineMethod(
                "GetService", 
                MethodAttributes.Public | MethodAttributes.Static,
                typeof(object),
                new[] { typeof(AppendableArray<object>), typeof(ContainerWeakRef), typeof(IScope) });

            factoryExpression.CompileToMethod(methodBuilder);

            var dynamicType = typeBuilder.CreateType();
            result = (FactoryDelegate)Delegate.CreateDelegate(typeof(FactoryDelegate), dynamicType.GetMethod("GetService"));
        }

        #region Implementation

        private static int _typeId;
        private static ModuleBuilder _moduleBuilder;

        private static ModuleBuilder GetDynamicAssemblyModuleBuilder()
        {
            return _moduleBuilder ?? (_moduleBuilder = DefineDynamicAssemblyModuleBuilder());
        }

        //I me
        private static ModuleBuilder DefineDynamicAssemblyModuleBuilder()
        {
            var assemblyName = new AssemblyName(DYNAMIC_ASSEMBLY_NAME);

            // if DryIoc assembly is signed, then sign the dynamic assembly too, otherwise don't.
            if (IsDryIocSigned())
                assemblyName.KeyPair = new StrongNameKeyPair(StrongNameKeyPairBytes);

            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);
            return moduleBuilder;
        }

        private static bool IsDryIocSigned()
        {
            var publicKey = typeof(FactoryCompiler).Assembly.GetName().GetPublicKey();
            return publicKey != null && publicKey.Length != 0;
        }

        #endregion
    }

    /// <remarks>Resolution rules to enable/disable compiling to Dynamic Assembly.</remarks>
    public sealed partial class Rules
    {
        public bool CompilationToDynamicAssemblyEnabled
        {
            get { return _compilationToDynamicAssemblyEnabled; }
        }

        public Rules EnableCompilationToDynamicAssembly(bool enable)
        {
            return new Rules(this) { _compilationToDynamicAssemblyEnabled = enable };
        }
    }
}
