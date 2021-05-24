﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Master
{
    public class Radar : Ability
    {
        public int Level { get; set; }

        public Radar(Unit owner, int level) : base(owner)
        {
            Level = level;
        }
    }
}