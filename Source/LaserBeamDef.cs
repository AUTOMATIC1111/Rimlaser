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

        public List<LaserBeamDecoration> decorations;
 //       public ThingDef beamDecoration;

        public EffecterDef explosionEffect;
        public EffecterDef hitLivingEffect;

        public List<string> textureBeam;
        public List<string> textureCap;

        private List<Material> materialBeam = new List<Material> ();
        private List<Material> materialCap = new List<Material> ();

        void CreateGraphics()
        {
            for (int i = 0; i < textureBeam.Count; i++)
            {
                string beam = textureBeam[i];
                string cap = textureCap != null && i < textureCap.Count ? textureCap[i] : null;

                materialBeam.Add(MaterialPool.MatFrom(beam, ShaderDatabase.TransparentPostLight));
                materialCap.Add(cap == null ? null : MaterialPool.MatFrom(cap, ShaderDatabase.TransparentPostLight));
            }
        }

        public void GetMaterials(int index, out Material beam, out Material cap)
        {
            if (materialBeam.Count == 0 && textureBeam.Count != 0)
                CreateGraphics();

            if (materialBeam.Count == 0) {
                beam = null;
                cap = null;
                return;
            }

            if (index >= materialBeam.Count || index < 0)
                index = 0;

            beam = materialBeam[index];
            cap = materialCap[index];
        }
    }
}
