namespace BossMod.MaskedCarnivale.Stage02.Act1.ArenaPudding
{
    public enum OID : uint
    {
        Boss = 0x25C0, //R=1.8
        Marshmallow = 0x25C2, //R1.8
        Bavarois = 0x25C4, //R1.8
    };

public enum AID : uint
{
    Fire = 14266, // 25C0->player, 1,0s cast, single-target
    Aero = 14269, // 25C2->player, 1,0s cast, single-target
    Thunder = 14268, // 25C4->player, 1,0s cast, single-target
    GoldenTongue = 14265, // 25C0/25C2/25C4->self, 5,0s cast, single-target
};

class GoldenTongue : Components.CastHint
    {
        public GoldenTongue() : base(ActionID.MakeSpell(AID.GoldenTongue), "Can be interrupted, increase its magic defenses") { }
    }
class Hints : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add("To beat this stage in a timely manner,\nyou should have at least one spell of each element.\n(Water, Fire, Ice, Lightning, Earth and Wind)");
        } 
    }
class Hints2 : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add("Pudding is weak to wind spells.\nMarshmallow is weak to ice spells.\nBavarois is weak to earth spells.");
        } 
    }    
class Stage02PuddingStates : StateMachineBuilder
    {
        public Stage02PuddingStates(BossModule module) : base(module)
        {
            TrivialPhase()
            .ActivateOnEnter<GoldenTongue>()
            .ActivateOnEnter<Hints2>()               
            .DeactivateOnEnter<Hints>();
        }
    }

public class Stage02Pudding : BossModule
    {
        public Stage02Pudding(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
        {
            ActivateComponent<Hints>();
        }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            foreach (var s in Enemies(OID.Boss))
                Arena.Actor(s, ArenaColor.Enemy, false);
            foreach (var s in Enemies(OID.Marshmallow))
                Arena.Actor(s, ArenaColor.Enemy, false);
            foreach (var s in Enemies(OID.Bavarois))
                Arena.Actor(s, ArenaColor.Enemy, false);
        }
    }
}
