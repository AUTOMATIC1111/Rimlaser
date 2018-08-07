using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Rimlaser
{
    public class LaserBeamDef : ThingDef
    {
        public float capSize = 1.0f;
        public float capOverlap = 1.1f / 64;

        public int lifetime = 30;
        public float impulse = 4.0f;

        public float beamWidth = 1.0f;

        public EffecterDef explosionEffect;
        public EffecterDef hitLivingEffect;

        public GraphicData graphicDataCap;

        public GraphicData graphicAwful;
        public GraphicData graphicAwfulCap;
        public GraphicData graphicPoor;
        public GraphicData graphicPoorCap;
        public GraphicData graphicNormal;
        public GraphicData graphicNormalCap;
        public GraphicData graphicGood;
        public GraphicData graphicGoodCap;
        public GraphicData graphicExcellent;
        public GraphicData graphicExcellentCap;
        public GraphicData graphicMasterwork;
        public GraphicData graphicMasterworkCap;
        public GraphicData graphicLegendary;
        public GraphicData graphicLegendaryCap;
    }
}
