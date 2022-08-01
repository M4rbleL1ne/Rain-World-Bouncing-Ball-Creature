using MonoMod.Cil;
using static Mono.Cecil.Cil.OpCodes;
using UnityEngine;
using RWCustom;

namespace BouncingBallCreature;

sealed class BouncingBallAI
{
    internal BouncingBallAI()
    {
        IL.SnailAI.TileIdleScore += il =>
        {
            ILCursor c = new(il);
            var loc = -1;
            var loc2 = -1;
            c.TryGotoNext(
                x => x.MatchLdloc(out loc),
                x => x.MatchLdcR4(1000f),
                x => x.MatchSub(),
                x => x.MatchStloc(loc));
            if (c.TryGotoNext(
                x => x.MatchLdloc(out loc2),
                x => x.MatchLdfld<Tracker.CreatureRepresentation>("representedCreature"),
                x => x.MatchLdfld<AbstractCreature>("creatureTemplate"),
                x => x.MatchLdfld<CreatureTemplate>("type"),
                x => x.MatchLdcI4(15),
                x => x.MatchBneUn(out _))
            && loc != -1 && loc2 != -1)
            {
                c.Index++;
                c.Emit(Ldarg_0);
                c.Emit(Ldarg_1);
                c.Emit(Ldloc, il.Body.Variables[loc]);
                c.EmitDelegate((Tracker.CreatureRepresentation rep, SnailAI self, WorldCoordinate pos, float num) =>
                {
                    if (rep.representedCreature is not null && rep.representedCreature.creatureTemplate.type == EnumExt_BouncingBall.BouncingBall && rep.representedCreature != self.creature && Custom.ManhattanDistance(pos, rep.BestGuessForPosition()) < 1)
                        num -= 20f;
                    return num;
                });
                c.Emit(Stloc, il.Body.Variables[loc]);
                c.Emit(Ldloc, il.Body.Variables[loc2]);
            }
            else
                BouncingBallPlugin.logger?.LogError("Couldn't ILHook SnailAI.TileIdleScore!");
        };
        On.SnailAI.TileIdleScore += (orig, self, pos) => self.snail is not null && self.snail.Bob() && self.snail.NarrowSpace() ? float.MinValue : orig(self, pos);
        On.SnailAI.Update += (orig, self) =>
        {
            orig(self);
            if (self.snail.Bob())
            {
                self.snail.set_shuffleDestination(self.snail.get_shuffleDestination() - 1);
                if (self.snail.NarrowSpace() && self.snail.get_shuffleDestination() <= 0)
                {
                    self.snail.set_shuffleDestination(200);
                    self.creature.abstractAI.SetDestination(new(self.snail.room.abstractRoom.index, Random.Range(0, self.snail.room.TileWidth), Random.Range(0, self.snail.room.TileHeight), -1));
                }
                if (self.snail.get_shuffleDestination() > 0 || self.snail.NarrowSpace())
                    self.move = true;
            }
        };
    }
}