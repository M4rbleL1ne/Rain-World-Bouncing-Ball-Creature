using Fisobs.Creatures;
using Fisobs.Core;
using System.Collections.Generic;
using Fisobs.Sandbox;
using static PathCost.Legality;
using UnityEngine;

namespace BouncingBallCreature;

sealed class BouncingBallCritob : Critob
{
    internal BouncingBallCritob() : base(EnumExt_BouncingBall.BouncingBall)
    {
        Icon = new SimpleIcon("icon_BouncingBall", Color.white);
        RegisterUnlock(KillScore.Configurable(2), EnumExt_BouncingBall.BouncingBallUnlock);
        new BouncingBallMisc();
        new BouncingBall();
        new BouncingBallAI();
        new BouncingBallGraphics();
    }

    public override IEnumerable<CreatureTemplate> GetTemplates()
    {
        var t = new CreatureFormula(this, "BouncingBall") {
            TileResistances = new() {
                Floor = new(1f, Allowed),
                Corridor = new(1f, Allowed),
                Climb = new(1f, Allowed),
                Wall = new(1f, Allowed),
                Ceiling = new(1f, Allowed),
                OffScreen = new(1f, Allowed)
            },
            ConnectionResistances = new() {
                Standard = new(1f, Allowed),
                OpenDiagonal = new(2f, Allowed),
                ShortCut = new(.2f, Allowed),
                NPCTransportation = new(20f, Allowed),
                Slope = new(1.6f, Allowed),
                CeilingSlope = new(1.6f, Allowed),
                DropToFloor = new(10f, Allowed),
                OffScreenMovement = new(1f, Allowed),
                BetweenRooms = new(10f, Allowed)
            },
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Uncomfortable, .5f),
            DamageResistances = new() { Base = .4f, Explosion = 102f },
            StunResistances = new() { Base = .8f, Explosion = 102f },
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.Snail)
        }.IntoTemplate();
        t.instantDeathDamageLimit = 1f;
        t.grasps = 0;
        t.dangerousToPlayer = .05f;
        t.offScreenSpeed = .05f;
        t.bodySize = .7f;
        t.shortcutSegments = 2;
        t.requireAImap = true;
        t.visualRadius = 300f;
        t.waterVision = 1f;
        t.throughSurfaceVision = .5f;
        t.countsAsAKill = 1;
        t.waterRelationship = CreatureTemplate.WaterRelationship.Amphibious;
        t.waterPathingResistance = 1f;
        t.communityInfluence = .25f;
        t.meatPoints = 1;
        yield return t;
    }

    public override void EstablishRelationships()
    {
        Relationships bob = new(EnumExt_BouncingBall.BouncingBall);
        bob.EatenBy(CreatureTemplate.Type.Leech, .5f);
        bob.IgnoredBy(CreatureTemplate.Type.Snail);
        bob.MakesUncomfortable(CreatureTemplate.Type.Scavenger, .6f);
        bob.Fears(CreatureTemplate.Type.DaddyLongLegs, 1f);
        bob.Fears(CreatureTemplate.Type.BrotherLongLegs, .8f);
        bob.Ignores(CreatureTemplate.Type.Snail);
        bob.Ignores(EnumExt_BouncingBall.BouncingBall);
    }

    public override ArtificialIntelligence GetRealizedAI(AbstractCreature acrit) => new SnailAI(acrit, acrit.world);

    public override Creature GetRealizedCreature(AbstractCreature acrit) => new Snail(acrit, acrit.world);

    public override void LoadResources(RainWorld rainWorld)
    {
        string[] sprAr = { "BoBShellA", "BoBShellB", "BoBShellC", "icon_BouncingBall" };
        foreach (var spr in sprAr) Ext.LoadAtlasFromEmbRes(GetType().Assembly, spr);
    }

    public override CreatureTemplate.Type? ArenaFallback(CreatureTemplate.Type type) => CreatureTemplate.Type.Snail;
}