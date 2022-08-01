using System.Text.RegularExpressions;
using MonoMod.Cil;
using static Mono.Cecil.Cil.OpCodes;

namespace BouncingBallCreature;

sealed class BouncingBallMisc
{
    internal BouncingBallMisc()
    {
        On.WorldLoader.CreatureTypeFromString += (orig, s) => Regex.IsMatch(s, "/bouncingball/gi") || Regex.IsMatch(s, "/bob/gi") ? EnumExt_BouncingBall.BouncingBall : orig(s);
        On.DevInterface.MapPage.CreatureVis.CritCol += (orig, crit) => crit.creatureTemplate.type == EnumExt_BouncingBall.BouncingBall ? new(.4f, .8f, .6f) : orig(crit);
        On.DevInterface.MapPage.CreatureVis.CritString += (orig, crit) => crit.creatureTemplate.type == EnumExt_BouncingBall.BouncingBall ? "BoB" : orig(crit);
        On.ArenaBehaviors.SandboxEditor.CreaturePerfEstimate += delegate (On.ArenaBehaviors.SandboxEditor.orig_CreaturePerfEstimate orig, CreatureTemplate.Type critType, ref float linear, ref float exponential)
        {
            if (critType == EnumExt_BouncingBall.BouncingBall)
            {
                linear += .2f;
                exponential += .3f;
            }
            else
                orig(critType, ref linear, ref exponential);
        };
        On.MultiplayerUnlocks.UnlockedCritters += (orig, ID) =>
        {
            var list = orig(ID);
            if (ID is MultiplayerUnlocks.LevelUnlockID.Hidden)
                list.Add(EnumExt_BouncingBall.BouncingBall);
            return list;
        };
        On.RoomRealizer.RoomPerformanceEstimation += (orig, self, testRoom) =>
        {
            var res = orig(self, testRoom);
            for (var j = 0; j < testRoom.creatures.Count; j++)
            {
                // orig already added 10f for the same creature, so we just need to add 10f again to add 20f in total like Snail
                if (testRoom.creatures[j].state.alive && testRoom.creatures[j].creatureTemplate.type == EnumExt_BouncingBall.BouncingBall)
                    res += 10f;
            }
            return res;
        };
        On.AImap.IsConnectionAllowedForCreature += (orig, self, connection, crit) => (crit.type != EnumExt_BouncingBall.BouncingBall || connection.type is not MovementConnection.MovementType.DropToFloor || self.room.GetTile(connection.DestTile).DeepWater) && orig(self, connection, crit);
        IL.Leech.Swim += il =>
        {
            ILCursor c = new(il);
            var loc = -1;
            var loc2 = -1;
            if (c.TryGotoNext(
                x => x.MatchLdfld<Leech>("school"),
                x => x.MatchLdfld<Leech.LeechSchool>("prey"),
                x => x.MatchLdloc(out loc),
                x => x.MatchCallOrCallvirt(out _),
                x => x.MatchLdfld<Leech.LeechSchool.LeechPrey>("creature"),
                x => x.MatchCallOrCallvirt<Creature>("get_Template"),
                x => x.MatchLdfld<CreatureTemplate>("type"),
                x => x.MatchLdcI4(15),
                x => x.MatchBneUn(out _),
                x => x.MatchLdloc(out loc2))
            && loc != -1 && loc2 != -1)
            {
                c.Emit(Ldloc, il.Body.Variables[loc]);
                c.Emit(Ldloc, il.Body.Variables[loc2]);
                c.EmitDelegate((Leech self, int num2, float num3) =>
                {
                    if (self.school?.prey?[num2]?.creature?.Template.type == EnumExt_BouncingBall.BouncingBall)
                        num3 *= 10f;
                    return num3;
                });
                c.Emit(Stloc, il.Body.Variables[loc2]);
                c.Emit(Ldarg_0);
            }
            else
                BouncingBallPlugin.logger?.LogError("Couldn't ILHook Leech.Swim!");
        };
    }
}
