namespace BossMod.Shadowbringers.Dungeon.D05MtGulg.D053ForgivenWhimsy;

public enum OID : uint
{
    Boss = 0x27CC, //R=20.00
    Helper = 0x2E8, //R=0.5
    Helper2 = 0x233C,
    Brightsphere = 0x27CD, //R=1.0
    Towers = 0x1EAACF, //R=0.5
}

public enum AID : uint
{
    AutoAttack = 15624, // 27CC->player, no cast, single-target
    SacramentOfPenance = 15627, // 27CC->self, 4,0s cast, single-target
    SacramentOfPenance2 = 15628, // 233C->self, no cast, range 50 circle
    Reformation = 15620, // 27CC->self, no cast, single-target, boss changes pattern
    ExegesisA = 16989, // 27CC->self, 5,0s cast, single-target
    ExegesisB = 16987, // 27CC->self, 5,0s cast, single-target
    ExegesisC = 15622, // 27CC->self, 5,0s cast, single-target
    ExegesisD = 16988, // 27CC->self, 5,0s cast, single-target
    Exegesis = 15623, // 233C->self, no cast, range 10 width 10 rect
    Catechism = 15625, // 27CC->self, 4,0s cast, single-target
    Catechism2 = 15626, // 233C->player, no cast, single-target
    JudgmentDay = 15631, // 27CC->self, 3,0s cast, single-target, tower circle 5
    Judged = 15633, // 233C->self, no cast, range 5 circle, tower success
    FoundWanting = 15632, // 233C->self, no cast, range 40 circle, tower fail
    RiteOfTheSacrament = 15629, // 27CC->self, no cast, single-target
    PerfectContrition = 15630, // 27CD->self, 6,0s cast, range 5-15 donut
}

class Catechism() : Components.SingleTargetCastDelay(ActionID.MakeSpell(AID.Catechism), ActionID.MakeSpell(AID.Catechism2), 0.5f);

class SacramentOfPenance() : Components.RaidwideCastDelay(ActionID.MakeSpell(AID.SacramentOfPenance), ActionID.MakeSpell(AID.SacramentOfPenance2), 0.5f);

class PerfectContrition() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.PerfectContrition), new AOEShapeDonut(5, 15));

public class JudgmentDay : Components.GenericTowers
{
    public override void OnActorEState(BossModule module, Actor actor, ushort state)
    {
        if (state is 0x01C or 0x02C)
        {
            if (!Towers.Any(t => t.Position.AlmostEqual(actor.Position, 1)))
                Towers.Add(new(actor.Position, 5, 1, 1));
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Judged or AID.FoundWanting && Towers.Count > 0)
            Towers.RemoveAt(0);
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Towers.Count > 0)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Towers[0].Position, 5));
        if (Towers.Count > 1)
            hints.PlannedActions.Add((ActionID.MakeSpell(WAR.AID.Sprint), actor, 1, false));
    }
}

class Exegesis : Components.GenericAOEs
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(5, 5, 5);
    private static readonly AOEShapeCross cross = new(15, 5);
    private static readonly WPos[] diagonalPositions = [new(-240, -50), new(-250, -40), new(-230, -40), new(-250, -60),  new(-230, -60)];

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;


    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        var _activation = spell.NPCFinishAt.AddSeconds(0.4f);
        switch ((AID)spell.Action.ID)
        {
            case AID.ExegesisA: //diagonal
                foreach (var p in diagonalPositions)
                    _aoes.Add(new(rect, p, activation: _activation));
                break;
            case AID.ExegesisB: //east+west
                _aoes.Add(new(rect, new(-250, -50), activation: _activation));
                _aoes.Add(new(rect, new(-230, -50), activation: _activation));
                break;
            case AID.ExegesisC: //north+south
                _aoes.Add(new(rect, new(-240, -60), activation: _activation));
                _aoes.Add(new(rect, new(-240, -40), activation: _activation));
                break;
            case AID.ExegesisD: //cross
                _aoes.Add(new(cross, new(-240, -50), activation: _activation));
                break;
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Exegesis)
            _aoes.Clear();
    }
}

class D053ForgivenWhimsyStates : StateMachineBuilder
{
    public D053ForgivenWhimsyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Catechism>()
            .ActivateOnEnter<SacramentOfPenance>()
            .ActivateOnEnter<PerfectContrition>()
            .ActivateOnEnter<JudgmentDay>()
            .ActivateOnEnter<Exegesis>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 659, NameID = 8261)]
public class D053ForgivenWhimsy : BossModule
{
    public D053ForgivenWhimsy(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(-240, -50), 15)) { }
}
