﻿namespace BossMod.Endwalker.Alliance.A13Azeyma;

class SolarFans() : Components.ChargeAOEs(ActionID.MakeSpell(AID.SolarFansAOE), 5);

class RadiantRhythm : Components.GenericAOEs
{
    private static readonly AOEShapeDonutSector _shape = new(20, 30, 45.Degrees());
    private readonly List<AOEInstance> _aoes = [];
    private readonly List<AOEInstance> _aoes2 = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            yield return new(_aoes[0].Shape, _aoes[0].Origin, _aoes[0].Rotation, _aoes[0].Activation, ArenaColor.Danger);
            yield return new(_aoes2[0].Shape, _aoes2[0].Origin, _aoes2[0].Rotation, _aoes2[0].Activation, ArenaColor.Danger);
        }
        if (_aoes.Count > 1)
        {
            yield return new(_aoes[1].Shape, _aoes[1].Origin, _aoes[1].Rotation, _aoes[1].Activation);
            yield return new(_aoes2[1].Shape, _aoes2[1].Origin, _aoes2[1].Rotation, _aoes2[1].Activation);
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        var _activation = spell.NPCFinishAt.AddSeconds(7.7f);
        if ((AID)spell.Action.ID == AID.SolarFansCharge) //since it seems impossible to determine early enough if 5 or 6 casts happen, we draw one extra one just incase
        {
            if (spell.LocXZ.AlmostEqual(new(-775, -750), 10))
                for (int i = 1; i < 6; ++i)
                {
                    _aoes.Add(new(_shape, module.Bounds.Center, (225 + i * 90).Degrees(), _activation.AddSeconds(1.3f * (i - 1))));
                    _aoes2.Add(new(_shape, module.Bounds.Center, (45 + i * 90).Degrees(), _activation.AddSeconds(1.3f * (i - 1))));
                }
            if (spell.LocXZ.AlmostEqual(new(-750, -775), 1))
                for (int i = 1; i < 6; ++i)
                {
                    _aoes.Add(new(_shape, module.Bounds.Center, (135 + i * 90).Degrees(), _activation.AddSeconds(1.3f * (i - 1))));
                    _aoes2.Add(new(_shape, module.Bounds.Center, (-45 + i * 90).Degrees(), _activation.AddSeconds(1.3f * (i - 1))));
                }
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.RadiantFinish)
        {
            _aoes.Clear();
            _aoes2.Clear();
        }
        if (_aoes.Count > 0 && (AID)spell.Action.ID == AID.RadiantFlight && ++NumCasts % 2 == 0)
        {
            _aoes.RemoveAt(0);
            _aoes2.RemoveAt(0);
        }
    }
}

class RadiantFlourish : Components.GenericAOEs
{
    private int teleportcounter;
    private static readonly AOEShapeCircle circle = new(25);
    private readonly List<AOEInstance> _aoes = [];
    private const float RadianConversion = MathF.PI / 180;

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SolarFansAOE)
            _aoes.Add(new(circle, spell.LocXZ, activation: module.WorldState.CurrentTime.AddSeconds(16.6f)));
        if ((AID)spell.Action.ID == AID.RadiantFlourish)
        {
            _aoes.Clear();
            teleportcounter = 0;
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.TeleportFlame) //correct circle location if Flight happens 10 times instead of 8 times, ugly hack but i couldn't find a better difference in logs
        {
            if (++teleportcounter > 8)
            {
                teleportcounter = 0;
                _aoes.Add(new(circle, RotateAroundOrigin(90, module.Bounds.Center, _aoes[0].Origin), activation: _aoes[0].Activation.AddSeconds(1.4f)));
                _aoes.Add(new(circle, RotateAroundOrigin(90, module.Bounds.Center, _aoes[1].Origin), activation: _aoes[1].Activation.AddSeconds(1.4f)));
                _aoes.RemoveAt(0);
                _aoes.RemoveAt(0);
            }
        }
    }

    private static WPos RotateAroundOrigin(float rotatebydegrees, WPos origin, WPos caster) //TODO: consider moving to utils for future use
    {
        float x = MathF.Cos(rotatebydegrees * RadianConversion) * (caster.X - origin.X) - MathF.Sin(rotatebydegrees * RadianConversion) * (caster.Z - origin.Z);
        float z = MathF.Sin(rotatebydegrees * RadianConversion) * (caster.X - origin.X) + MathF.Cos(rotatebydegrees * RadianConversion) * (caster.Z - origin.Z);
        return new WPos(origin.X + x, origin.Z + z);
    }
}
