namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarSkatene;

public enum OID : uint
{
    Boss = 0x2535, //R=4.48
    BossAdd = 0x255E, //R=0.9
    BossHelper = 0x233C,
    BonusAddAltarMatanga = 0x2545, // R3.420
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // BonusAddAltarMatanga->player, no cast, single-target
    AutoAttack3 = 6499, // BossAdd->player, no cast, single-target
    RecklessAbandon = 13311, // Boss->player, 3.0s cast, single-target
    Tornado = 13309, // Boss->location, 3.0s cast, range 6 circle
    VoidCall = 13312, // Boss->self, 3.5s cast, single-target
    Chirp = 13310, // Boss->self, 3.5s cast, range 8+R circle

    unknown = 9636, // BonusAddAltarMatanga->self, no cast, single-target
    Spin = 8599, // BonusAddAltarMatanga->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // BonusAddAltarMatanga->self, 2.5s cast, range 5+R 120-degree cone
    Hurl = 5352, // BonusAddAltarMatanga->location, 3.0s cast, range 6 circle
    Telega = 9630, // BonusAdds->self, no cast, single-target, bonus adds disappear
}

class Chirp(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Chirp), new AOEShapeCircle(12.48f));
class Tornado(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Tornado), 6);
class VoidCall(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.VoidCall), "Calls adds");
class RecklessAbandon(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.RecklessAbandon));

class Hurl(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Hurl), 6);
class Spin(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.Spin), new AOEShapeCone(9.42f, 60.Degrees()), (uint)OID.BonusAddAltarMatanga);
class RaucousScritch(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RaucousScritch), new AOEShapeCone(8.42f, 30.Degrees()));

class SkateneStates : StateMachineBuilder
{
    public SkateneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Chirp>()
            .ActivateOnEnter<Tornado>()
            .ActivateOnEnter<VoidCall>()
            .ActivateOnEnter<RecklessAbandon>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<RaucousScritch>()
            .ActivateOnEnter<Spin>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead) && module.Enemies(OID.BonusAddAltarMatanga).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7587)]
public class Skatene(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(19))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.BossAdd))
            Arena.Actor(s, ArenaColor.Object);
        foreach (var s in Enemies(OID.BonusAddAltarMatanga))
            Arena.Actor(s, ArenaColor.Vulnerable);
    }

    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.BonusAddAltarMatanga => 3,
                OID.BossAdd => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
