using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Rimlaser
{
    public class LaserBeamDecoration
    {
        public ThingDef mote;
        public float spacing = 1.0f;
        public float initialOffset = 0;
        public float speed = 1.0f;
        public float speedJitter;
        public float speedJitterOffset;

    }

    public class LaserBeamDef : ThingDef
    {
        public float capSize = 1.0f;
        public float capOverlap = 1.1f / 64;

        public int lifetime = 30;
        public float impulse = 4.0f;

        public float beamWidth = 1.0f;
        public float shieldDamageMultiplier = 0.5f;
        public float seam = -1f;

        public List<LaserBeamDecoration> decorations;

        public EffecterDef explosionEffect;
        public EffecterDef hitLivingEffect;

        public List<string> textures;
        private List<Material> materials = new List<Material> ();

        void CreateGraphics()
        {
            for (int i = 0; i < textures.Count; i++)
            {
                materials.Add(MaterialPool.MatFrom(textures[i], ShaderDatabase.TransparentPostLight));
            }
        }

        public Material GetBeamMaterial(int index)
        {
            if (materials.Count == 0 && textures.Count != 0)
                CreateGraphics();

            if (materials.Count == 0) {
                return null;
            }

            if (index >= materials.Count || index < 0)
                index = 0;

            return materials[index];
        }
    }
}
