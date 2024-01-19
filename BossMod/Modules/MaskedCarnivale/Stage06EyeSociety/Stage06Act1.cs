using System.Collections.Generic;
using System.Linq;
using BossMod.Components;

namespace BossMod.MaskedCarnivale.Stage06.Act1
{
    public enum OID : uint
    {
        Boss = 0x25CD, //R=2.53
        Mandragora = 0x2700, //R0.3
    };

    public enum AID : uint
    {
        TearyTwirl = 14693, // 2700->self, 3,0s cast, range 6+R circle
        DemonEye = 14691, // 25CD->self, 5,0s cast, range 50+R circle
        Attack = 6499, // 2700/25CD->player, no cast, single-target
        ColdStare = 14692, // 25CD->self, 2,5s cast, range 40+R 90-degree cone
    };
    public enum SID : uint
    {
        Blind = 571, // Mandragora->player, extra=0x0

    };

    class DemonEye : GenericAOEs
    {
        private bool activePillar; 
        private static readonly AOEShapeCircle circle = new(6);
        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (activePillar)
                foreach (var p in module.Enemies(OID.Boss))
                    yield return new(circle, p.Position, p.Rotation, new());
        }
    }
    class TearyTwirl : StackWithCastTargets
    {
        public TearyTwirl() : base(ActionID.MakeSpell(AID.TearyTwirl), 6) { }
    }


    class Hints : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add("Get blinded by the Teary Twirl AOE from the mandragoras.\nBlindness makes you immune to all the gaze attacks.\nThe eyes in act 2 are weak to lightning damage.");
        } 
    }   
    class Stage06Act2States : StateMachineBuilder
    {
        public Stage06Act2States(BossModule module) : base(module)
        {
            TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .ActivateOnEnter<TearyTwirl>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.Mandragora).All(e => e.IsDead);
        }
    }

    public class Stage06Act2 : BossModule
    {
        public Stage06Act2(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
        {
            ActivateComponent<Hints>();
        }
        protected override bool CheckPull() { return PrimaryActor.IsTargetable && PrimaryActor.InCombat || Enemies(OID.Mandragora).Any(e => e.InCombat); }
        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            foreach (var s in Enemies(OID.Boss))
                Arena.Actor(s, ArenaColor.Enemy, false);
            foreach (var s in Enemies(OID.Mandragora))
                Arena.Actor(s, ArenaColor.Object, false);
        }
        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
        base.CalculateAIHints(slot, actor, assignment, hints);
            foreach (var e in hints.PotentialTargets)
            {
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.Boss => 1,
                    OID.Mandragora => 0,
                    _ => 0
                };
            }
        }
    }
}
