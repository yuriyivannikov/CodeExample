using UnityEngine;
using System.Collections;
using System;

namespace GameUI
{
    public class BaseStyleObject<T> : ScriptableObject
    {
        public T value;
    }
}