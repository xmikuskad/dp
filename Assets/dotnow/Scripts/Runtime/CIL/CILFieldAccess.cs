﻿using dotnow.Reflection;
using System.Reflection;

namespace dotnow.Runtime.CIL
{
    internal sealed class CILFieldAccess
    {
        // Internal
        internal FieldInfo targetField;
        internal AppDomain.FieldDirectAccessDelegate directReadAccessDelegate;
        internal AppDomain.FieldDirectAccessDelegate directWriteAccessDelegate;
        internal CLRTypeInfo fieldTypeInfo;
        internal bool isClrField;

        // Constructor
        public CILFieldAccess(FieldInfo targetField)
        {
            this.targetField = targetField;
            this.isClrField = targetField is CLRField;
        }

        // Methods
        public void SetupFieldAccess(AppDomain domain)
        {
            // Get type
            this.fieldTypeInfo = CLRTypeInfo.GetTypeInfo(targetField.FieldType);

            // Get direct access delegate
            this.directReadAccessDelegate = domain.GetDirectAccessDelegate(targetField, CLRFieldAccessMode.Read);
            this.directWriteAccessDelegate = domain.GetDirectAccessDelegate(targetField, CLRFieldAccessMode.Write);
        }

        public override string ToString()
        {
            return targetField.ToString();
        }
    }
}
