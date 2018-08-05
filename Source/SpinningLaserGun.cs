using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Rimlaser
{
    class SpinningLaserGun : ThingWithComps
    {
        enum State
        {
            Idle = 0,
            Spinup = 1,
            Spinning = 2
        };

        int previousTick = 0;
        State state = State.Idle;

        float rotation = 0;
        float rotationSpeed = 0;
        float targetRotationSpeed;
        float rotationAcceleration = 0;
        int rotationAccelerationTicksRemaing = 0;

        new SpinningLaserGunDef def
        {
            get { return base.def as SpinningLaserGunDef; }
        }

        void ReachRotationSpeed(float target, int ticksUntil)
        {
            targetRotationSpeed = target;

            if (ticksUntil <= 0)
            {
                rotationAccelerationTicksRemaing = 0;
                rotationSpeed = target;
            }

            rotationAccelerationTicksRemaing = ticksUntil;
            rotationAcceleration = (target - rotationSpeed) / ticksUntil;
        }

        private Graphic GetGraphicForTick(int ticksPassed)
        {
            if (rotationAccelerationTicksRemaing > 0)
            {
                if (ticksPassed > rotationAccelerationTicksRemaing)
                    ticksPassed = rotationAccelerationTicksRemaing;

                rotationAccelerationTicksRemaing -= ticksPassed;
                rotationSpeed += ticksPassed * rotationAcceleration;

                if (rotationAccelerationTicksRemaing <= 0)
                {
                    rotationSpeed = targetRotationSpeed;
                }
            }

            rotation += rotationSpeed * ticksPassed;

            int frame = ((int)rotation) % def.frames.Count;
            return def.frames[frame].Graphic;
        }

        bool IsBrusting(Pawn pawn)
        {
            if (pawn.CurrentEffectiveVerb == null) return false;
            return pawn.CurrentEffectiveVerb.Bursting;
        }

        public override Graphic Graphic
        {
            get
            {
                if (def.frames.Count == 0) return DefaultGraphic;

                var holder = ParentHolder as Pawn_EquipmentTracker;
                if (holder == null) return DefaultGraphic;

                Stance stance = holder.pawn.stances.curStance;
                Stance_Warmup warmup;

                switch (state)
                {
                    case State.Idle:
                        warmup = stance as Stance_Warmup;
                        if (warmup != null)
                        {
                            state = State.Spinup;
                            ReachRotationSpeed(def.rotationSpeed, warmup.ticksLeft);
                        }
                        break;
                    case State.Spinup:
                        if (IsBrusting(holder.pawn))
                        {
                            state = State.Spinning;
                        }
                        break;
                    case State.Spinning:
                        if (! IsBrusting(holder.pawn))
                        {
                            state = State.Idle;
                            Stance_Cooldown cooldown = stance as Stance_Cooldown;
                            if (cooldown != null)
                                ReachRotationSpeed(0.0f, cooldown.ticksLeft);
                            else
                                ReachRotationSpeed(0.0f, 0);
                        }
                        break;
                }

                var tick = Find.TickManager.TicksGame;
                var res = GetGraphicForTick(tick - previousTick);
                previousTick = tick;

                return res;
            }
        }
    }
}
