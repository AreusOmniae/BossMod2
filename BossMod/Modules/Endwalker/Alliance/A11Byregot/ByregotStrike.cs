﻿namespace BossMod.Endwalker.Alliance.A11Byregot;

class ByregotStrikeJump() : Components.LocationTargetedAOEs(ActionID.MakeSpell(AID.ByregotStrikeJump), 8);
class ByregotStrikeJumpCone() : Components.LocationTargetedAOEs(ActionID.MakeSpell(AID.ByregotStrikeJumpCone), 8);
class ByregotStrikeKnockback() : Components.KnockbackFromCastTarget(ActionID.MakeSpell(AID.ByregotStrikeKnockback), 18);

class ByregotStrikeCone : Components.GenericAOEs
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCone _shape = new(90, 22.5f.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ByregotStrikeKnockback && module.PrimaryActor.FindStatus(SID.Glow) != null)
            for (int i = 0; i < 4; ++i)
                _aoes.Add(new(_shape, caster.Position, spell.Rotation + i * 90.Degrees(), spell.NPCFinishAt));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ByregotStrikeCone)
        {
            _aoes.Clear();
            ++NumCasts;
        }
    }
}
