﻿using UnityEditor;
using UnityEngine;

namespace dotnow.BindingGenerator
{
    public class GenrateProxy : MonoBehaviour
    {
#if !UNITY_DISABLE
#if UNITY_EDITOR && NET_4_6
        //[MenuItem("Tools/Generate Proxy")]
        //public static void CreateProxy()
        //{
        //    BindingsGeneratorService service = new BindingsGeneratorService();

        //    service.GenerateBindingsForType(typeof(System.IO.Stream), Application.dataPath + "/TestBindings");
        //}
#endif
#endif
    }
}
