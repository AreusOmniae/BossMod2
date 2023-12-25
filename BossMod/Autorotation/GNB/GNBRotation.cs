﻿
namespace BossMod.GNB
{
    public static class Rotation
    {
        // full state needed for determining next action
        public class State : CommonRotation.PlayerState
        {
            public int Ammo; // 0 to 100
            public int GunComboStep; // 0 to 2
            public float NoMercyLeft; // 0 if buff not up, max 20
            public bool ReadyToRip; // 0 if buff not up, max 10
            public bool ReadyToTear; // 0 if buff not up, max 10
            public bool ReadyToGouge; // 0 if buff not up, max 10
            public bool ReadyToBlast; // 0 if buff not up, max 10

            public int MaxCartridges;
            // upgrade paths
            public AID BestZone => Unlocked(AID.BlastingZone) ? AID.BlastingZone : AID.DangerZone;
            public AID BestHeart => Unlocked(AID.HeartOfCorundum) ? AID.HeartOfCorundum : AID.HeartOfStone;
            public AID BestContinuation => ReadyToRip ? AID.JugularRip : ReadyToTear? AID.AbdomenTear : ReadyToGouge ? AID.EyeGouge : ReadyToBlast ? AID.Hypervelocity : AID.Continuation;
            public AID ComboLastMove => (AID)ComboLastAction;

            public State(float[] cooldowns) : base(cooldowns) { }

            public bool Unlocked(AID aid) => Definitions.Unlocked(aid, Level, UnlockProgress);
            public bool Unlocked(TraitID tid) => Definitions.Unlocked(tid, Level, UnlockProgress);

            public override string ToString()
            {
                return $"ammo={Ammo}, ReadytoBlast={ReadyToBlast}, ReadytoGouge={ReadyToGouge}, ReadytoRip={ReadyToRip}, ReadytoTear={ReadyToTear}, roughdivide={CD(CDGroup.RoughDivide):f1}, RB={RaidBuffsLeft:f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}/{UnlockProgress}";
            }
        }

        // strategy configuration
        public class Strategy : CommonRotation.Strategy
        {
            public enum GaugeUse : uint
            {
                Automatic = 0, // spend gauge either under raid buffs or if next downtime is soon (so that next raid buff window won't cover at least 4 GCDs)

                [PropertyDisplay("Spend all gauge ASAP", 0x8000ff00)]
                Spend = 1, // spend all gauge asap, don't bother conserving

                [PropertyDisplay("Conserve unless under raid buffs", 0x8000ffff)]
                ConserveIfNoBuffs = 2, // spend under raid buffs, conserve otherwise (even if downtime is imminent)

                [PropertyDisplay("Conserve as much as possible", 0x800000ff)]
                Conserve = 3, // conserve even if under raid buffs (useful if heavy vuln phase is imminent)

                [PropertyDisplay("Use Lightning Shot if outside melee", 0x80c08000)]
                LightningShotIfNotInMelee = 4,

                [PropertyDisplay("Use combo, unless it can't be finished before downtime and unless gauge and/or ST would overcap", 0x80c0c000)]
                ComboFitBeforeDowntime = 5, // useful on late phases before downtime

                [PropertyDisplay("Use combo until second-last step, then spend gauge", 0x80400080)]
                PenultimateComboThenSpend = 6, // useful for ensuring ST extension is used right before long downtime
            }

            public enum PotionUse : uint
            {
                Manual = 0, // potion won't be used automatically

                [PropertyDisplay("Use ASAP, but delay slightly during opener", 0x8000ff00)]
                Immediate = 1,

                [PropertyDisplay("Delay until raidbuffs", 0x8000ffff)]
                DelayUntilRaidBuffs = 2,

                [PropertyDisplay("Use ASAP, even if without ST", 0x800000ff)]
                Force = 3,
            }

            public enum RoughDivideUse : uint
            {
                Automatic = 0, // always keep one charge reserved, use other charges under raidbuffs or prevent overcapping

                [PropertyDisplay("Forbid automatic use", 0x800000ff)]
                Forbid = 1, // forbid until window end

                [PropertyDisplay("Do not reserve charges: use all charges if under raidbuffs, otherwise use as needed to prevent overcapping", 0x8000ffff)]
                NoReserve = 2, // automatic logic, except without reserved charge

                [PropertyDisplay("Use all charges ASAP", 0x8000ff00)]
                Force = 3, // use all charges immediately, don't wait for raidbuffs

                [PropertyDisplay("Use all charges except one ASAP", 0x80ff0000)]
                ForceReserve = 4, // if 2+ charges, use immediately

                [PropertyDisplay("Reserve 2 charges, trying to prevent overcap", 0x80ffff00)]
                ReserveTwo = 5, // use only if about to overcap

                [PropertyDisplay("Use as gapcloser if outside melee range", 0x80ff00ff)]
                UseOutsideMelee = 6, // use immediately if outside melee range
            }

            public enum SpecialAction : uint
            {
                None = 0, // don't use any special actions

                [PropertyDisplay("LB3", 0x8000ff00)]
                LB3, // use LB3 if available
            }

            public GaugeUse GaugeStrategy; // how are we supposed to handle gauge
            public PotionUse PotionStrategy; // how are we supposed to use potions
            public OffensiveAbilityUse NoMercyUse; // how are we supposed to use IR
            public OffensiveAbilityUse ZoneUse; // how are we supposed to use upheaval
            public OffensiveAbilityUse BowUse; // how are we supposed to use PR
            public RoughDivideUse RoughDivideStrategy; // how are we supposed to use onslaught
            public SpecialAction SpecialActionUse; // any special actions we want to use
            public bool Aggressive; // if true, we use buffs and stuff at last possible moment; otherwise we make sure to keep at least 1 GCD safety net

            public override string ToString()
            {
                return $"";
            }

            // TODO: these bindings should be done by the framework...
            public void ApplyStrategyOverrides(uint[] overrides)
            {
                if (overrides.Length >= 7)
                {
                    GaugeStrategy = (GaugeUse)overrides[0];
                    PotionStrategy = (PotionUse)overrides[1];
                    NoMercyUse = (OffensiveAbilityUse)overrides[2];
                    ZoneUse = (OffensiveAbilityUse)overrides[3];
                    BowUse = (OffensiveAbilityUse)overrides[4];
                    RoughDivideStrategy = (RoughDivideUse)overrides[5];
                    SpecialActionUse = (SpecialAction)overrides[6];
                }
                else
                {
                    GaugeStrategy = GaugeUse.Automatic;
                    PotionStrategy = PotionUse.Manual;
                    NoMercyUse = OffensiveAbilityUse.Automatic;
                    ZoneUse = OffensiveAbilityUse.Automatic;
                    BowUse = OffensiveAbilityUse.Automatic;
                    RoughDivideStrategy = RoughDivideUse.Automatic;
                    SpecialActionUse = SpecialAction.None;
                }
            }
        }

        public static int GetSTComboLength(AID comboLastMove) => comboLastMove switch
        {
            AID.BrutalShell => 1,
            AID.KeenEdge => 2,
            _ => 3
        };

        public static int GetAOEComboLength(AID comboLastMove) => comboLastMove == AID.DemonSlice ? 1 : 2;

        public static AID GetNextBurtalShellComboAction(AID comboLastMove) => comboLastMove == AID.KeenEdge ? AID.BrutalShell : AID.KeenEdge;

        public static AID GetNextAOEComboAction(AID comboLastMove) => comboLastMove == AID.DemonSlice ? AID.DemonSlaughter : AID.DemonSlice;

        public static AID GetNextUnlockedComboAction(State state, bool aoe)
        {
            if (aoe && state.Unlocked(AID.DemonSlice))
            {
                return state.Unlocked(AID.DemonSlaughter) && state.ComboLastMove == AID.DemonSlice ? AID.DemonSlaughter : AID.DemonSlice;
            }
            else
            {
                return state.ComboLastMove switch
                {
                    AID.BrutalShell => state.Unlocked(AID.SolidBarrel) ? AID.SolidBarrel : AID.KeenEdge,
                    AID.KeenEdge => state.Unlocked(AID.BrutalShell) ? AID.BrutalShell : AID.KeenEdge,
                    _ => AID.KeenEdge
                };
            }
        }

        public static AID GetNextAmmoAction(State state, bool aoe)
        {
            if (state.CD(CDGroup.NoMercy) > 26)
            {
                if (state.GunComboStep == 0 && state.CD(CDGroup.GnashingFang) < 0.6 && state.Ammo >= 1)
                    return AID.GnashingFang;
            }

            if (state.NoMercyLeft > state.AnimationLock)
            {
                if (state.CD(CDGroup.SonicBreak) < state.GCD && state.CD(CDGroup.GnashingFang) > state.GCD && state.Unlocked(AID.SonicBreak))
                    return AID.SonicBreak;
                if (state.CD(CDGroup.DoubleDown) < state.GCD && state.CD(CDGroup.GnashingFang) > state.GCD && state.Unlocked(AID.DoubleDown) && state.Ammo >= 2)
                    return AID.DoubleDown;
                if (state.Ammo >= 1 && state.CD(CDGroup.GnashingFang) > state.GCD && state.CD(CDGroup.DoubleDown) > state.GCD && state.CD(CDGroup.SonicBreak) > state.GCD && state.Unlocked(AID.DoubleDown) && state.GunComboStep == 0)
                    return AID.BurstStrike;
                if (state.Ammo >= 1 && state.CD(CDGroup.GnashingFang) > state.GCD && state.CD(CDGroup.SonicBreak) > state.GCD && !state.Unlocked(AID.DoubleDown) && state.GunComboStep == 0)
                    return AID.BurstStrike;
                if (state.Ammo >= 1 && state.CD(CDGroup.GnashingFang) > state.GCD && !state.Unlocked(AID.DoubleDown) && !state.Unlocked(AID.SonicBreak) && state.GunComboStep == 0)
                    return AID.BurstStrike;
                if (state.Ammo >= 1 && !state.Unlocked(AID.DoubleDown) && !state.Unlocked(AID.SonicBreak) && !state.Unlocked(AID.GnashingFang))
                    return AID.BurstStrike;
                if (aoe && state.Ammo >= 1 && state.CD(CDGroup.GnashingFang) > state.GCD && state.CD(CDGroup.DoubleDown) > state.GCD && state.CD(CDGroup.SonicBreak) > state.GCD && state.Unlocked(AID.DoubleDown) && state.GunComboStep == 0)
                    return AID.FatedCircle;
                if (aoe && state.Ammo >= 1 && state.CD(CDGroup.GnashingFang) > state.GCD && state.CD(CDGroup.SonicBreak) > state.GCD && !state.Unlocked(AID.DoubleDown) && state.GunComboStep == 0)
                    return AID.FatedCircle;
                if (aoe && state.Ammo >= 1 && state.CD(CDGroup.GnashingFang) > state.GCD && !state.Unlocked(AID.DoubleDown) && !state.Unlocked(AID.SonicBreak) && state.GunComboStep == 0)
                    return AID.FatedCircle;
                if (aoe && state.Ammo >= 1 && !state.Unlocked(AID.DoubleDown) && !state.Unlocked(AID.SonicBreak) && !state.Unlocked(AID.GnashingFang))
                    return AID.FatedCircle;
            }

            if (state.GunComboStep > 0)
            {
                if (state.GunComboStep == 2)
                    return AID.WickedTalon;
                if (state.GunComboStep == 1)
                    return AID.SavageClaw;
            }

            if (state.ComboLastMove == AID.BrutalShell && state.Ammo == state.MaxCartridges)
            {
                return AID.BurstStrike;
            }

            if (aoe && state.ComboLastMove == AID.DemonSlice && state.Ammo == state.MaxCartridges)
            {
                return AID.FatedCircle;
            }

            // single-target gauge spender
            return GetNextUnlockedComboAction(state, aoe);
        }

        public static bool ShouldSpendGauge(State state, Strategy strategy, bool aoe) => strategy.GaugeStrategy switch
        {
            Strategy.GaugeUse.Automatic or Strategy.GaugeUse.LightningShotIfNotInMelee => (state.RaidBuffsLeft > state.GCD || strategy.FightEndIn <= strategy.RaidBuffsIn + 10),
            Strategy.GaugeUse.Spend => true,
            Strategy.GaugeUse.ConserveIfNoBuffs => state.RaidBuffsLeft > state.GCD,
            Strategy.GaugeUse.Conserve => false,
            Strategy.GaugeUse.ComboFitBeforeDowntime => strategy.FightEndIn <= state.GCD + 2.5f * ((aoe ? GetAOEComboLength(state.ComboLastMove) : GetSTComboLength(state.ComboLastMove)) - 1),
            Strategy.GaugeUse.PenultimateComboThenSpend => state.ComboLastMove is AID.BrutalShell or AID.DemonSlice,
            _ => true
        };

        public static bool ShouldUsePotion(State state, Strategy strategy) => strategy.PotionStrategy switch
        {
            Strategy.PotionUse.Manual => false,
            Strategy.PotionUse.Immediate => (state.ComboLastMove == AID.KeenEdge && state.Ammo == 0) || (state.CD(CDGroup.NoMercy) < 5.5 && strategy.CombatTimer > 30), // TODO: reconsider potion use during opener (delayed IR prefers after maim, early IR prefers after storm eye, to cover third IC on 13th GCD)
            Strategy.PotionUse.Force => true,
            _ => false
        };

        public static bool ShouldUseNoMercy(State state, Strategy strategy) => strategy.NoMercyUse switch
        {
            Strategy.OffensiveAbilityUse.Delay => false,
            Strategy.OffensiveAbilityUse.Force => true,
            _ => strategy.CombatTimer >= 0 && ((state.TargetingEnemy && state.ComboLastMove == AID.BrutalShell && state.Ammo == 0) || state.Ammo == 3 || state.CD(CDGroup.Bloodfest) < 15 && state.Ammo == 1)
        };

        public static bool ShouldUseZone(State state, Strategy strategy) => strategy.ZoneUse switch
        {
            Strategy.OffensiveAbilityUse.Delay => false,
            Strategy.OffensiveAbilityUse.Force => true,
            _ => strategy.CombatTimer >= 0 && state.TargetingEnemy && state.CD(CDGroup.GnashingFang) > state.AnimationLock && state.CD(CDGroup.SonicBreak) > state.AnimationLock
        };

        public static bool ShouldUseBow(State state, Strategy strategy) => strategy.BowUse switch
        {
            Strategy.OffensiveAbilityUse.Delay => false,
            Strategy.OffensiveAbilityUse.Force => true,
            _ => strategy.CombatTimer >= 0 && state.TargetingEnemy && state.CD(CDGroup.GnashingFang) > state.AnimationLock && state.CD(CDGroup.SonicBreak) > state.AnimationLock
        };

        public static bool ShouldUseRoughDivide(State state, Strategy strategy)
        {
            bool OnCD = state.CD(CDGroup.NoMercy) >= state.AnimationLock && state.CD(CDGroup.GnashingFang) >= state.AnimationLock && state.CD(CDGroup.SonicBreak) >= state.AnimationLock && state.CD(CDGroup.DoubleDown) >= state.AnimationLock;
            switch (strategy.RoughDivideStrategy)
            {
                case Strategy.RoughDivideUse.Forbid:
                    return false;
                case Strategy.RoughDivideUse.Force:
                    return true;
                case Strategy.RoughDivideUse.ForceReserve:
                    return state.CD(CDGroup.RoughDivide) <= 30 + state.AnimationLock;
                case Strategy.RoughDivideUse.ReserveTwo:
                    return state.CD(CDGroup.RoughDivide) - 30 <= state.GCD;
                case Strategy.RoughDivideUse.UseOutsideMelee:
                    return state.RangeToTarget > 3;
                default:
                    if (strategy.CombatTimer < 0)
                        return false; // don't use out of combat
                    if (state.RangeToTarget > 3)
                        return false; // don't use out of melee range to prevent fucking up player's position
                    if (strategy.PositionLockIn <= state.AnimationLock)
                        return false; // forbidden due to state flags
                    if (OnCD && state.NoMercyLeft > state.AnimationLock)
                        return true; // delay until Gnashing Sonic and Doubledown on CD, even if overcapping charges
                    float chargeCapIn = state.CD(CDGroup.RoughDivide) - 30;
                    if (chargeCapIn < state.GCD + 2.5)
                        return true; // if we won't onslaught now, we risk overcapping charges
                    if (strategy.RoughDivideStrategy != Strategy.RoughDivideUse.NoReserve && state.CD(CDGroup.RoughDivide) > 30 + state.AnimationLock)
                        return false; // strategy prevents us from using last charge
                    if (state.RaidBuffsLeft > state.AnimationLock)
                        return true; // use now, since we're under raid buffs
                    return chargeCapIn <= strategy.RaidBuffsIn; // use if we won't be able to delay until next raid buffs
            }
        }

        public static AID GetNextBestGCD(State state, Strategy strategy, bool aoe)
        {
            // prepull
            if (strategy.CombatTimer > -100 && strategy.CombatTimer < -0.7f)
                return AID.None;


            if (strategy.GaugeStrategy == Strategy.GaugeUse.LightningShotIfNotInMelee && state.RangeToTarget > 3 && state.GunComboStep == 0)
                return AID.LightningShot;
            if (strategy.GaugeStrategy == Strategy.GaugeUse.PenultimateComboThenSpend && state.ComboLastMove != AID.BrutalShell && state.ComboLastMove != AID.DemonSlice && (state.ComboLastMove != AID.BrutalShell || state.Ammo == state.MaxCartridges) && state.GunComboStep == 0)
                return aoe ? AID.DemonSlice : state.ComboLastMove == AID.KeenEdge ? AID.BrutalShell : AID.KeenEdge;

            if (state.Ammo >= state.MaxCartridges && state.ComboLastMove == AID.BrutalShell)
                return GetNextAmmoAction(state, aoe);

            if (state.NoMercyLeft > state.AnimationLock)
                return GetNextAmmoAction(state, aoe);

            if (state.CD(CDGroup.GnashingFang) < 0.6)
                return GetNextAmmoAction(state, aoe);

            if (state.GunComboStep > 0)
            {
                if (state.GunComboStep == 2)
                    return AID.WickedTalon;
                if (state.GunComboStep == 1)
                    return AID.SavageClaw;
            }

            if (strategy.GaugeStrategy == Strategy.GaugeUse.Spend && state.Ammo >= 1 && state.CD(CDGroup.GnashingFang) >= state.AnimationLock && state.CD(CDGroup.SonicBreak) >= state.AnimationLock && state.CD(CDGroup.DoubleDown) >= state.AnimationLock && state.GunComboStep == 0)
                return AID.BurstStrike;

            return GetNextUnlockedComboAction(state, aoe);
        }

        public static ActionID GetNextBestOGCD(State state, Strategy strategy, float deadline, bool aoe)
        {
            if (strategy.SpecialActionUse == Strategy.SpecialAction.LB3)
                return ActionID.MakeSpell(AID.GunmetalSoul);

            bool wantOnslaught = state.Unlocked(AID.RoughDivide) && state.TargetingEnemy && ShouldUseRoughDivide(state, strategy);
            if (wantOnslaught && state.RangeToTarget > 3)
                return ActionID.MakeSpell(AID.RoughDivide);

            if (ShouldUsePotion(state, strategy) && state.CanWeave(state.PotionCD, 1.1f, deadline))
                return CommonDefinitions.IDPotionStr;

            if (state.Unlocked(AID.NoMercy))
            {
                if (ShouldUseNoMercy(state, strategy) && state.CanWeave(CDGroup.NoMercy, 0.6f, deadline))
                    return ActionID.MakeSpell(AID.NoMercy);
            }

            if (state.Unlocked(AID.Bloodfest) && state.CanWeave(CDGroup.Bloodfest, 0.6f, deadline) && state.Ammo == 0 && state.NoMercyLeft > state.AnimationLock)
                return ActionID.MakeSpell(AID.Bloodfest);

            if (state.Unlocked(AID.DangerZone) && ShouldUseZone(state, strategy) && state.CanWeave(CDGroup.DangerZone, 0.6f, deadline))
                return ActionID.MakeSpell(state.BestZone);

            if (state.Unlocked(AID.BowShock) && ShouldUseBow(state, strategy) && state.CanWeave(state.CD(CDGroup.BowShock), 0.6f, deadline))
                return ActionID.MakeSpell(AID.BowShock);

            if ((state.ReadyToBlast) && state.CanWeave(CDGroup.Hypervelocity, 0.6f, deadline))
                return ActionID.MakeSpell(AID.Hypervelocity);
            if (state.ReadyToGouge && state.CanWeave(CDGroup.EyeGouge, 0.6f, deadline))
                return ActionID.MakeSpell(AID.EyeGouge);
            if (state.ReadyToTear && state.CanWeave(CDGroup.AbdomenTear, 0.6f, deadline))
                return ActionID.MakeSpell(AID.AbdomenTear);
            if (state.ReadyToRip && state.CanWeave(CDGroup.JugularRip, 0.6f, deadline))
                return ActionID.MakeSpell(AID.JugularRip);

            if (wantOnslaught && state.CanWeave(state.CD(CDGroup.RoughDivide) - 30, 0.8f, deadline) && state.NoMercyLeft > state.AnimationLock && state.CD(CDGroup.GnashingFang) > 10 && state.CD(CDGroup.SonicBreak) > 10 && state.CD(CDGroup.DoubleDown) > 10)
                return ActionID.MakeSpell(AID.RoughDivide);

            if (wantOnslaught && state.CanWeave(state.CD(CDGroup.RoughDivide), 0.8f, deadline) && state.NoMercyLeft < state.AnimationLock && state.NoMercyLeft > state.AnimationLock && state.CD(CDGroup.GnashingFang) > 10 && state.CD(CDGroup.SonicBreak) > 10 && state.CD(CDGroup.DoubleDown) > 10)
                return ActionID.MakeSpell(AID.RoughDivide);

            return new();
        }
    }
}
