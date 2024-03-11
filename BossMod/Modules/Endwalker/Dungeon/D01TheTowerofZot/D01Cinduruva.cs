﻿using System;
using System.Collections.Generic;
using System.Linq;
using BossMod.Endwalker.Dungeon.D01TheTowerofZot.D01Sanduruva;
using Lumina.Data.Parsing.Layer;
using static BossMod.ActorCastEvent;

/*
everything we need to understand the mechanics is in here
https://www.thegamer.com/final-fantasy-14-endwalker-tower-of-zot-dungeon-guide-walkthrough/
*/

namespace BossMod.Endwalker.Dungeon.D01TheTowerofZot.D01Cindurava
{
    /*
    notes to self bnpcname has nameID, contentfindercondition has the CFC
    iconid is a row in lockon sheet
    tetherid is a row in channeling or w/e sheet
    */

    public enum OID : uint
    {
        Actor1ea1a1 = 0x1EA1A1, // R2.000, x7, EventObj type
        Actor1e8f2f = 0x1E8F2F, // R0.500, x2, EventObj type
        Sanduruva = 0x33F2, // R2.500, x1
        Cinduruva = 0x3610, // R0.500, x1
        Minduruva = 0x33F3, // R2.040, x1
        Boss = 0x33F1, // R2.200, x1
        Helper = 0x233C, // R0.500, x37, 523 type
    };

    public enum AID : uint
    {
        Ability_UnknownB3 = 25254,    // Sanduruva->location, no cast, single-target
        Attack = 871,                 // Sanduruva->none, no cast, single-target
        DeltaAttack = 25260,          // Minduruva->Boss, 5.0s cast, single-target
        DeltaAttack1= 25261,          // Minduruva->Boss, 5.0s cast, single-target
        DeltaAttack2 = 25262,         // Minduruva->Boss, 5.0s cast, single-target
        DeltaBlizzardIII1 = 25266,    //done. Helper->self, 3.0s cast, range 40+R ?-degree cone
        DeltaBlizzardIII2 = 25267,    //done. Helper->self, 3.0s cast, range 44 width 4 rect
        DeltaBlizzardIII3 = 25268,    //done. Helper->location, 5.0s cast, range 40 circle
        DeltaFireIII1 = 25263,        //done. Helper->self, 4.0s cast, range ?-40 donut
        DeltaFireIII2 = 25264,        //done. Helper->self, 3.0s cast, range 44 width 10 rect
        DeltaFireIII3 = 25265,        //done. Helper->player, 5.0s cast, range 6 circle
        DeltaThunderIII1 = 25269,     //done. Helper->location, 3.0s cast, range 3 circle
        DeltaThunderIII2 = 25270,     //done. Helper->location, 3.0s cast, range 5 circle
        DeltaThunderIII3 = 25271,     //done. Helper->self, 3.0s cast, range 40 width 10 rect
        DeltaThunderIII4 = 25272,     //done. Helper->none, 5.0s cast, range 5 circle
        Dhrupad = 25281,              //done. Minduruva->self, 4.0s cast, single-target
        IsitvaSiddhi = 25280,         // Sanduruva->none, 4.0s cast, single-target
        ManusyaBlizzard1 = 25283,     // Minduruva->none, no cast, single-target
        ManusyaBlizzard2 = 25288,     // Minduruva->player, 2.0s cast, single-target
        ManusyaFaith = 25258,         // Sanduruva->Minduruva, 4.0s cast, single-target
        ManusyaFire1 = 25282,         // Minduruva->player, no cast, single-target
        ManusyaFire2 = 25287,         // Minduruva->player, 2.0s cast, single-target
        ManusyaGlare = 25274,         // Boss->none, no cast, single-target
        ManusyaReflect = 25259,       // Boss->self, 4.2s cast, range 40 circle
        ManusyaThunder1 = 25284,      // Minduruva->player, no cast, single-target
        ManusyaThunder2 = 25289,      // Minduruva->none, 2.0s cast, single-target
        PraptiSiddhi = 25275,         //done. Sanduruva->self, 2.0s cast, range 40 width 4 rect
        Samsara = 25273,              // Boss->self, 3.0s cast, range 40 circle
    };
    public enum SID : uint
    {
        VulnerabilityUp = 1789, // Helper->player, extra=0x1
        Burns = 2082, // Minduruva->player/33CD, extra=0x0
        Frostbite = 2083, // Minduruva->33CD/316C, extra=0x0
        Electrocution = 2086, // Minduruva->316C/player, extra=0x0
    };


    public enum IconID : uint
    {
        Icon_198 = 198, // 33CE
        Icon_62 = 62, // 33CD
        Icon_139 = 139, // player/33CE/33CD/316C
    };
    class Dhrupad : Components.SingleTargetCast
    {
        public Dhrupad() : base(ActionID.MakeSpell(AID.Dhrupad)) { }
    }
    class DeltaThunderIII1 : Components.LocationTargetedAOEs
    {
        public DeltaThunderIII1() : base(ActionID.MakeSpell(AID.DeltaThunderIII1), 3) { }
    }
    class DeltaThunderIII2 : Components.LocationTargetedAOEs
    {
        public DeltaThunderIII2() : base(ActionID.MakeSpell(AID.DeltaThunderIII2), 5) { }
    }
    class DeltaThunderIII3 : Components.SelfTargetedAOEs
    {
        public DeltaThunderIII3() : base(ActionID.MakeSpell(AID.DeltaThunderIII3), new AOEShapeRect(40, 5)) { }
    }
    class DeltaThunderIII4 : Components.SpreadFromCastTargets
    {
        public DeltaThunderIII4() : base(ActionID.MakeSpell(AID.DeltaThunderIII4), 5) { }
    }
    class DeltaBlizzardIII1 : Components.SelfTargetedAOEs
    { 
        public DeltaBlizzardIII1() : base(ActionID.MakeSpell(AID.DeltaBlizzardIII1), new AOEShapeCone(40, 10.Degrees())) { } //might be different than the boss1 one
    }
    class DeltaBlizzardIII2 : Components.SelfTargetedAOEs
    { 
        public DeltaBlizzardIII2() : base(ActionID.MakeSpell(AID.DeltaBlizzardIII2), new AOEShapeRect(44, 5)) { }
    }
    class DeltaBlizzardIII3 : Components.ChargeAOEs //or raidwide?
    { 
        public DeltaBlizzardIII3() : base(ActionID.MakeSpell(AID.DeltaBlizzardIII3),20) { }
    }
    class DeltaFireIII3 : Components.SpreadFromCastTargets
    {
        public DeltaFireIII3() : base(ActionID.MakeSpell(AID.DeltaFireIII3),6) { }
    }
    class DeltaFireIII1 : Components.SelfTargetedAOEs
    {
        public DeltaFireIII1() : base(ActionID.MakeSpell(AID.DeltaFireIII1), new AOEShapeDonut(4, 40)) { }
    }
    class DeltaFireIII2 : Components.SelfTargetedAOEs
    {
        public DeltaFireIII2() : base(ActionID.MakeSpell(AID.DeltaFireIII2), new AOEShapeRect(44, 5)) { }
    }
    class PraptiSiddhi : Components.SelfTargetedAOEs
    {
        public PraptiSiddhi() : base(ActionID.MakeSpell(AID.PraptiSiddhi), new AOEShapeRect(40, 4)) { }
    }
    class D01CinduravaStates : StateMachineBuilder
    {
        public D01CinduravaStates(BossModule module) : base(module)
        {
            TrivialPhase()
            .ActivateOnEnter<PraptiSiddhi>()
            .ActivateOnEnter<DeltaFireIII1>()
            .ActivateOnEnter<DeltaFireIII2>()
            .ActivateOnEnter<DeltaFireIII3>()
            .ActivateOnEnter<DeltaThunderIII1>()
            .ActivateOnEnter<DeltaThunderIII2>()
            .ActivateOnEnter<DeltaThunderIII3>()
            .ActivateOnEnter<DeltaThunderIII4>()
            //.ActivateOnEnter<Dhrupad>()
            .ActivateOnEnter<DeltaBlizzardIII1>()
            .ActivateOnEnter<DeltaBlizzardIII2>()
            .ActivateOnEnter<DeltaBlizzardIII3>();
        }
    }

    /*missing stuff
     * stack marker
     * delayed bombs on ground
     * goes to opposite spot from the donut on some donuts? (first one i think in 2024 03 10 first recording)
     */
    /*    notes to self bnpcname has nameID, contentfindercondition has the CFC
    */

    [ModuleInfo(CFCID = 783, NameID = 10259)]

    class D01Cindurava : BossModule
    {
        public D01Cindurava(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-28, -48), 13)) { }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
        }

    }
    

}