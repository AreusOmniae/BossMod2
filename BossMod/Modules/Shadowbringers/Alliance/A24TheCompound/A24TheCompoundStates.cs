﻿namespace BossMod.Shadowbringers.Alliance.A24TheCompound;

class A24TheCompoundStates : StateMachineBuilder
{
    public A24TheCompoundStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MechanicalLaceration1>()
            .ActivateOnEnter<MechanicalDissection>()
            .ActivateOnEnter<MechanicalDecapitation>()
            .ActivateOnEnter<MechanicalContusionGround>()
            .ActivateOnEnter<MechanicalContusionSpread>()
            .ActivateOnEnter<IncongruousSpinAOE>();
    }
}
