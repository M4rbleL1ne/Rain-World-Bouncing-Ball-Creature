﻿using MonoMod.Cil;
using static Mono.Cecil.Cil.OpCodes;
using UnityEngine;
using RWCustom;
using BouncingBallCreature.WeakTables.Collections;
using static BouncingBallCreature.BouncingBall;

namespace BouncingBallCreature;

sealed class BouncingBall
{
    internal static readonly ConditionalWeakTable<Snail, BouncingBallFields> bouncingBallFields = new();

    internal sealed class BouncingBallFields
    {
        public float alpha = 1f;
        public float lastAlpha = 1f;
        public float consciousness;
        public float lerper;
        public bool lerpUp = true;
        public readonly int[] effectColorRND = { Random.Range(0, 2), Random.Range(0, 2) };
        public int shuffleDestination;
    }

    internal BouncingBall()
    {
        On.Snail.ctor += (orig, self, abstractCreature, world) =>
        {
            orig(self, abstractCreature, world);
            if (self.Bob())
            {
                self.bounce = 1.75f;
                bouncingBallFields.Add(self, new());
            }
        };
        IL.Snail.Click += il =>
        {
            ILCursor c = new(il);
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdcI4(60),
                x => x.MatchCallOrCallvirt<Snail>("Stun")))
            {
                c.Index--;
                c.Emit(Ldarg_0);
                c.EmitDelegate((int st, Snail self) => self.Bob() ? 0 : st);
            }
            else
                BouncingBallPlugin.logger?.LogError("Couldn't ILHook Snail.Click!");
        };
        On.Snail.Click += (orig, self) =>
        {
            if (self.Bob() && (self.dead || self.NarrowSpace()))
                return;
            orig(self);
            if (self.Bob() && self.room is not null)
            {
                var vector = self.mainBodyChunk.pos;
                self.room.AddObject(new Explosion.ExplosionLight(vector, 160f, 1f, 3, self.shellColor[1]));
                self.room.AddObject(new ExplosionSpikes(self.room, vector, 9, 4f, 5f, 5f, 90f, self.shellColor[1]));
                for (var j = 0; j < 20; j++)
                {
                    var vector2 = Custom.RNV();
                    self.room.AddObject(new Spark(vector + vector2 * Random.value * 40f, vector2 * Mathf.Lerp(4f, 30f, Random.value), self.shellColor[1], null, 4, 18));
                }
                if (self.bodyChunks is not null && self.bodyChunks.Length > 0)
                {
                    for (var i = 0; i < self.bodyChunks.Length; i++)
                        self.bodyChunks[i].vel *= 1.25f;
                }
            }
        };
        On.Snail.TerrainImpact += (orig, self, chunk, direction, speed, firstContact) =>
        {
            if (self.Bob() && !self.justClicked && speed > 3f)
                self.triggered = true;
            orig(self, chunk, direction, speed, firstContact);
            if (self.Bob() && self.bodyChunks is not null && self.bodyChunks.Length > 0)
            {
                for (var i = 0; i < self.bodyChunks.Length; i++)
                    self.bodyChunks[i].vel = new(self.bodyChunks[i].vel.x + Random.Range(.1f, -.1f) - self.bodyChunks[i].vel.x / 4f, self.bodyChunks[i].vel.y - self.bodyChunks[i].vel.y / 4f);
            }
        };
        On.Snail.Update += (orig, self, eu) =>
        {
            if (self.Bob())
            {
                self.set_consciousness(Mathf.Clamp01(self.get_consciousness() + (!self.Consious ? .02f : -.0075f)));
                self.set_lerper(Mathf.Clamp(self.get_lerper() + (self.get_lerpUp() ? .0075f : -.0075f), -.75f, 1f));
                if (self.get_lerper() == 1f)
                    self.set_lerpUp(false);
                else if (self.get_lerper() == -.75f)
                    self.set_lerpUp(true);
                self.set_lastAlpha(self.get_alpha());
                self.set_alpha(Mathf.Lerp(1f, 0f, Mathf.Max(self.get_consciousness(), self.get_lerper())));
            }
            orig(self, eu);
            if (self.Bob() && self.bodyChunks is not null && self.bodyChunks.Length > 0)
            {
                for (var i = 0; i < self.bodyChunks.Length; i++)
                {
                    if (self.bodyChunks[i].vel.x < -30f || self.bodyChunks[i].vel.x > 30f)
                        self.bodyChunks[i].vel.x = 0f;
                    if (self.bodyChunks[i].vel.y < -30f || self.bodyChunks[i].vel.y > 30f)
                        self.bodyChunks[i].vel.y = 0f;
                }
            }
        };
        On.Snail.ShortCutColor += (orig, self) => self.Bob() ? self.shellColor[0] : orig(self);
        On.Snail.Die += (orig, self) =>
        {
            if (self.Bob() && self.room is not null)
            {
                var vector = self.mainBodyChunk.pos;
                self.room.AddObject(new SootMark(self.room, vector, 50f, true));
                self.room.AddObject(new Explosion(self.room, self, vector, 5, 110f, 5f, 1.1f, 60f, .3f, self, .8f, 0f, .7f));
                for (var i = 0; i < 14; i++)
                    self.room.AddObject(new Explosion.ExplosionSmoke(vector, Custom.RNV() * 5f * Random.value, 1f));
                self.room.AddObject(new Explosion.ExplosionLight(vector, 160f, 1f, 3, self.shellColor[1]));
                self.room.AddObject(new ExplosionSpikes(self.room, vector, 9, 4f, 5f, 5f, 90f, self.shellColor[1]));
                self.room.AddObject(new ShockWave(vector, 60f, .045f, 4));
                for (var j = 0; j < 20; j++)
                {
                    var vector2 = Custom.RNV();
                    self.room.AddObject(new Spark(vector + vector2 * Random.value * 40f, vector2 * Mathf.Lerp(4f, 30f, Random.value), self.shellColor[1], null, 4, 18));
                }
                self.room.ScreenMovement(vector, default, .7f);
                self.room.PlaySound(SoundID.Bomb_Explode, vector);
            }
            orig(self);
            if (self.Bob())
                self.Destroy();
        };
    }
}

public static class BouncingBallExtenstions
{
    public static bool Bob(this Snail self) => self.Template.type == EnumExt_BouncingBall.BouncingBall;

#pragma warning disable IDE1006
    public static float get_alpha(this Snail self) => bouncingBallFields.TryGetValue(self, out var f) ? f.alpha : 0f;

    public static void set_alpha(this Snail self, float value)
    {
        if (bouncingBallFields.TryGetValue(self, out var f))
            f.alpha = value;
    }

    public static float get_lastAlpha(this Snail self) => bouncingBallFields.TryGetValue(self, out var f) ? f.lastAlpha : 0f;

    public static void set_lastAlpha(this Snail self, float value)
    {
        if (bouncingBallFields.TryGetValue(self, out var f))
            f.lastAlpha = value;
    }

    public static float get_lerper(this Snail self) => bouncingBallFields.TryGetValue(self, out var f) ? f.lerper : 0f;

    public static void set_lerper(this Snail self, float value)
    {
        if (bouncingBallFields.TryGetValue(self, out var f))
            f.lerper = value;
    }

    public static bool get_lerpUp(this Snail self) => bouncingBallFields.TryGetValue(self, out var f) && f.lerpUp;

    public static void set_lerpUp(this Snail self, bool value)
    {
        if (bouncingBallFields.TryGetValue(self, out var f))
            f.lerpUp = value;
    }

    public static float get_consciousness(this Snail self) => bouncingBallFields.TryGetValue(self, out var f) ? f.consciousness : 0f;

    public static void set_consciousness(this Snail self, float value)
    {
        if (bouncingBallFields.TryGetValue(self, out var f))
            f.consciousness = value;
    }

    public static int get_shuffleDestination(this Snail self) => bouncingBallFields.TryGetValue(self, out var f) ? f.shuffleDestination : 0;

    public static void set_shuffleDestination(this Snail self, int value)
    {
        if (bouncingBallFields.TryGetValue(self, out var f))
            f.shuffleDestination = value;
    }

    public static int get_effectColorRND(this Snail self, int index) => bouncingBallFields.TryGetValue(self, out var f) ? f.effectColorRND[index] : 0;

    public static bool NarrowSpace(this Snail self)
    {
        if (self.room is not null)
        {
            if (self.room.GetTile(new IntVector2(self.abstractCreature.pos.x, self.abstractCreature.pos.y + 1)).Solid && self.room.GetTile(new IntVector2(self.abstractCreature.pos.x, self.abstractCreature.pos.y - 1)).Solid)
                return true;
            if (self.room.GetTile(new IntVector2(self.abstractCreature.pos.x + 1, self.abstractCreature.pos.y)).Solid && self.room.GetTile(new IntVector2(self.abstractCreature.pos.x - 1, self.abstractCreature.pos.y)).Solid)
                return true;
            if (self.room.GetTile(new IntVector2(self.abstractCreature.pos.x, self.abstractCreature.pos.y)).shortCut != 0)
                return true;
        }
        return false;
    }
#pragma warning restore IDE1006
}