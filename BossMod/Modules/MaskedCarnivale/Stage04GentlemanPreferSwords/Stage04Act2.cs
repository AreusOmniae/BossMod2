using BossMod.Components;

namespace BossMod.MaskedCarnivale.Stage04.Act2
{
    public enum OID : uint
    {
        Boss = 0x25D5, //R=2.5
        Beetle = 0x25D6, //R=0.6
    };

public enum AID : uint
{
    AutoAttack = 6497, // 25D5->player, no cast, single-target
    GrandStrike = 14366, // 25D5->self, 1,5s cast, range 75+R width 2 rect
    Attack2 = 6499, // 25D6->player, no cast, single-target
    MagitekField = 14369, // 25D5->self, 5,0s cast, single-target
    Spoil = 14362, // 25D6->self, no cast, range 6+R circle
    MagitekRay = 14368, // 25D5->location, 3,0s cast, range 6 circle
};


    class GrandStrike : SelfTargetedAOEs
    {
        public GrandStrike() : base(ActionID.MakeSpell(AID.GrandStrike), new AOEShapeRect(75,2)) { } 
    }

    class MagitekRay: LocationTargetedAOEs
    {
        public MagitekRay() : base(ActionID.MakeSpell(AID.MagitekRay), 6) { }
    }
    class MagitekField : CastHint
    {
        public MagitekField() : base(ActionID.MakeSpell(AID.MagitekField), "Interruptible, increases its defenses") { }
    }
class Hints : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add("Kreios is weak to Lightning.\nDuring the fight he will spawn 6 beetles.\nIf available use the Ultravibration combo.");
        } 
    }
class Hints2 : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add("Kreios is weak against lightning spells and can be frozen.");
        } 
    }    

    class Stage03States : StateMachineBuilder
    {
        public Stage03States(BossModule module) : base(module)
        {
            TrivialPhase()
            .ActivateOnEnter<MagitekField>()
            .ActivateOnEnter<MagitekRay>()
            .ActivateOnEnter<GrandStrike>()
            .ActivateOnEnter<Hints2>()
            .DeactivateOnEnter<Hints>();
        }
    }

    public class Stage03 : BossModule
    {
        public Stage03(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
        {
            ActivateComponent<Hints>();
        }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy, true);
            foreach (var s in Enemies(OID.Beetle))
                Arena.Actor(s, ArenaColor.Object, false);
        }
    }
}
