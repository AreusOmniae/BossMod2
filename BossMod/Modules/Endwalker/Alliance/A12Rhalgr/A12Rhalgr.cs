﻿namespace BossMod.Endwalker.Alliance.A12Rhalgr;

class DestructiveBolt() : Components.SpreadFromCastTargets(ActionID.MakeSpell(AID.DestructiveBoltAOE), 3);
class StrikingMeteor() : Components.LocationTargetedAOEs(ActionID.MakeSpell(AID.StrikingMeteor), 6);
class BronzeLightning() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.BronzeLightning), new AOEShapeCone(50, 22.5f.Degrees()), 4);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 866, NameID = 11273, SortOrder = 3)]
public class A12Rhalgr(WorldState ws, Actor primary) : BossModule(ws, primary, new ArenaBoundsRect(new(-22.5f, 270.5f), 31.5f, 35.5f))
{
    private static readonly List<WPos> arenacoords = [new (-29.2f, 235.5f), new (-40.3f, 248f), new (-47.5f, 260f), new (-53.8f, 273.7f), new (-46.4f, 276f), new (-45.1f, 274.8f),
    new (-43.2f, 272.1f), new (-40.4f, 270.7f), new (-38.8f, 271.4f), new (-38.3f, 272.6f), new (-38.2f, 275f), new (-39.1f, 278.5f), new (-40.7f, 282.4f), new (-46.1f, 291.3f),
    new (-49.2f, 296.8f), new (-41f, 300.2f), new (-37.1f, 293.4f), new (-34.9f, 291f), new (-32.5f, 290.2f), new (-30.7f, 291.1f), new (-30.5f, 295.8f), new (-31.2f, 305f),
    new (-22.6f, 306f), new (-19.8f, 290.5f), new (-18f, 288.7f), new (-16f, 289.2f), new (-14f, 290.9f), new (-13.7f, 303.7f), new (-6.3f, 304.7f), new (-4.5f, 288.2f),
    new (-3.7f, 287f), new (-1.3f, 287.8f), new (-0.1f, 289.2f), new (3.4f, 297.15f), new (8.9f, 294f), new (6.4f, 286.6f), new (6.2f, 283.2f), new (7.3f, 276.4f), 
    new (7.7f, 267.2f), new (6.8f, 253f), new (4.5f, 242.7f), new (2.23f, 235.6f)];
    protected override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var p in arenacoords)
            Arena.PathLineTo(p);
        Arena.PathStroke(true, ArenaColor.Border);
    }
}
