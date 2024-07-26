// Created by Victor Engstr�m
// Copyright 2024 Sonigon AB
// http://www.sonity.org/

using UnityEngine;
using System;

namespace Sonity.Internal {

    [Serializable]
    public abstract class SoundMixBase : ScriptableObject {

        public SoundMixInternals internals = new SoundMixInternals();
    }
}