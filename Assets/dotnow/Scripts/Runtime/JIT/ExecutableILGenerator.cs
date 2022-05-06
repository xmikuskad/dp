﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Mono.Cecil.Cil;
using dotnow.Reflection;
using dotnow.Runtime.CIL;

namespace dotnow.Runtime.JIT
{
    public class ExecutableILGenerator
    {
        // Private
        private AppDomain domain = null;
        private List<CILOperation> operationsDynamic = null;
        private CILOperation[] operationsStatic = null;
        private int emitPtr = 0;
        private bool dynamic = false;

        // Constructor
        public ExecutableILGenerator(AppDomain domain)
        {
            this.domain = domain;
            this.operationsDynamic = new List<CILOperation>();
            this.dynamic = true;
        }

        public ExecutableILGenerator(AppDomain domain, int instructionSize)
        {
            this.domain = domain;
            this.operationsStatic = new CILOperation[instructionSize];
            this.dynamic = false;
        }

        // Methods
        internal CILOperation[] GetExecutableInstructions()
        {
            // Check for dynamically emitted instructions - can change at any time
            if (dynamic == true)
            {
                // Check for instructions not cahced or changed
                if (operationsStatic == null || operationsStatic.Length != operationsDynamic.Count)
                    operationsStatic = operationsDynamic.ToArray();
            }

            return operationsStatic;
        }

        #region BuildMethod
        public MethodInfo BuildExecutableMethod(Type[] localTypes, int maxStack, Type returnType, params Type[] parameterTypes)
        {
            return new CLRAnonymousMethod(domain, GetExecutableInstructions(), true, maxStack, localTypes, returnType, parameterTypes);
        }
        #endregion

        #region Emit
        public void Emit(Code opCode)
        {
            EmitOperation(new CILOperation(opCode, new StackData.Primitive { Int32 = 0 }, null));
        }

        public void Emit(Code opCode, int operand)
        {
            EmitOperation(new CILOperation(opCode, new StackData.Primitive { Int32 = operand }, null));
        }

        public void Emit(Code opCode, long operand)
        {
            EmitOperation(new CILOperation(opCode, new StackData.Primitive { Int64 = operand }, null));
        }

        public void Emit(Code opCode, float operand)
        {
            EmitOperation(new CILOperation(opCode, new StackData.Primitive { Single = operand }, null));
        }

        public void Emit(Code opCode, double operand)
        {
            EmitOperation(new CILOperation(opCode, new StackData.Primitive { Double = operand }, null));
        }

        public void Emit(Code opCode, string operand)
        {
            EmitOperation(new CILOperation(opCode, default(StackData.Primitive), operand));
        }

        public void Emit(Code opCode, int[] operand)
        {
            EmitOperation(new CILOperation(opCode, default(StackData.Primitive), operand));
        }

        public void Emit(Code opCode, Type type)
        {
            CILOperation op = new CILOperation(opCode, default(StackData.Primitive), null);
            op.typeOperand = CLRTypeInfo.GetTypeInfo(type);

            EmitOperation(op);
        }

        public void Emit(Code opCode, MethodBase method)
        {
            Emit(opCode, (MemberInfo)method);
        }

        public void Emit(Code opCode, FieldInfo field)
        {
            Emit(opCode, (MemberInfo)field);
        }

        public void Emit(Code opCode, MemberInfo member, Type specialType = null)
        {
            CILOperation op = new CILOperation(opCode, default(StackData.Primitive), member);

            // Set type value
            if(specialType != null)
                op.typeOperand = CLRTypeInfo.GetTypeInfo(specialType);

            // Check for field access
            if(member is FieldInfo)
            {
                // Setup field access
                CILFieldAccess access = new CILFieldAccess((FieldInfo)member);
                access.SetupFieldAccess(domain);

                // Create access cache
                op.objectOperand = access;
            }

            // Check for method call
            if(member is MethodBase)
            {
                // Setup method call
                CILMethodInvocation invocation = new CILMethodInvocation((MethodBase)member);
                invocation.SetupMethodCall(domain);

                // Create method cache
                op.objectOperand = invocation;
            }

            // Check for type
            if(member is Type)
            {
                op.typeOperand = CLRTypeInfo.GetTypeInfo((Type)member);
            }

            EmitOperation(op);
        }
        #endregion

#if API_NET35
        private void EmitOperation(CILOperation op)
#else
        private void EmitOperation(in CILOperation op)
#endif
        {
            if(dynamic == true)
            {
                operationsDynamic.Add(op);
            }
            else
            {
                operationsStatic[emitPtr] = op;
                emitPtr++;
            }
        }
    }
}
