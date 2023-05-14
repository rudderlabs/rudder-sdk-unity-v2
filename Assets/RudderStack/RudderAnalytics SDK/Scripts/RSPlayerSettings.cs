using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace RudderStack.Unity
{
    public class RSPlayerSettings : ScriptableObject
    {
        public int    androidCode;
        public string iosCode;
        public string packageName;
    }
}