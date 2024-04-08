﻿namespace BossMod.Endwalker.Alliance.A22AlthykNymeia;

class SpinnersWheelSelect : BossComponent
{
    public enum Branch { None, Gaze, StayMove }

    public Branch SelectedBranch { get; private set; }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        var branch = (SID)status.ID switch
        {
            SID.ArcaneAttraction or SID.AttractionReversed => Branch.Gaze,
            SID.ArcaneFever or SID.FeverReversed => Branch.StayMove,
            _ => Branch.None
        };
        if (branch != Branch.None)
            SelectedBranch = branch;
    }
}

class SpinnersWheelGaze(bool inverted, AID aid, SID sid) : Components.GenericGaze(ActionID.MakeSpell(aid), inverted)
{
    private SID _sid = sid;
    private Actor? _source;
    private DateTime _activation;
    private BitMask _affected;

    public override void Init(BossModule module)
    {
        _source = module.Enemies(OID.Nymeia).FirstOrDefault();
    }

    public override IEnumerable<Eye> ActiveEyes(BossModule module, int slot, Actor actor)
    {
        if (_source != null && _affected[slot])
            yield return new(_source.Position, _activation);
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == _sid)
        {
            _activation = status.ExpireAt;
            _affected.Set(module.Raid.FindSlot(actor.InstanceID));
        }
    }
}

class SpinnersWheelArcaneAttraction() : SpinnersWheelGaze(false, AID.SpinnersWheelArcaneAttraction, SID.ArcaneAttraction);
class SpinnersWheelAttractionReversed() : SpinnersWheelGaze(true, AID.SpinnersWheelAttractionReversed, SID.AttractionReversed);

class SpinnersWheelStayMove : Components.StayMove
{
    public int ActiveDebuffs { get; private set; }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.ArcaneFever:
                if (module.Raid.FindSlot(actor.InstanceID) is var feverSlot && feverSlot >= 0 && feverSlot < Requirements.Length)
                    Requirements[feverSlot] = Requirement.Stay;
                break;
            case SID.FeverReversed:
                if (module.Raid.FindSlot(actor.InstanceID) is var revSlot && revSlot >= 0 && revSlot < Requirements.Length)
                    Requirements[revSlot] = Requirement.Move;
                break;
            case SID.Pyretic:
            case SID.FreezingUp:
                ++ActiveDebuffs;
                break;
        }
    }

    public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.Pyretic or SID.FreezingUp)
        {
            --ActiveDebuffs;
            if (module.Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0 && slot < Requirements.Length)
                Requirements[slot] = Requirement.None;
        }
    }
}
