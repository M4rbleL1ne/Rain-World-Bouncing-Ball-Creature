using MonoMod.Cil;
using static Mono.Cecil.Cil.OpCodes;
using UnityEngine;

namespace BouncingBallCreature;

sealed class BouncingBallAI
{
    internal BouncingBallAI()
    {
        IL.SnailAI.TileIdleScore += il =>
        {
            ILCursor c = new(il);
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdloc(out _),
                x => x.MatchLdfld<Tracker.CreatureRepresentation>("representedCreature"),
                x => x.MatchLdfld<AbstractCreature>("creatureTemplate"),
                x => x.MatchLdfld<CreatureTemplate>("type"),
                x => x.MatchLdcI4(15),
                x => x.MatchBneUn(out _)))
            {
                c.Prev.Previous.OpCode = Call;
                c.Prev.Previous.Operand = typeof(BouncingBallExtenstions).GetMethod("SnailOrBob");
                c.Prev.OpCode = Brfalse;
            }
            else BouncingBallPlugin.logger?.LogError("Couldn't ILHook SnailAI.TileIdleScore!");
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
                if (self.snail.get_shuffleDestination() > 0 || self.snail.NarrowSpace()) self.move = true;
            }
        };
    }
}