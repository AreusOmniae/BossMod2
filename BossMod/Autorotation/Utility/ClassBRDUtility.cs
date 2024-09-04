﻿namespace BossMod.Autorotation;

public sealed class ClassBRDUtility(RotationModuleManager manager, Actor player) : RoleRangedUtility(manager, player)
{
    public enum Track { WardensPaean = SharedTrack.Count, Troubadour, NaturesMinne }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(BRD.AID.SagittariusArrow);

    public enum TroubOption { Use87, Use88, DontUse }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: BRD", "Planner support for utility actions", "veyn", RotationModuleQuality.WIP, BitMask.Build((int)Class.BRD), 100);
        DefineShared(res, IDLimitBreak3);

        // TODO: repelling shot (not sure how it can be planned really...)
        DefineSimpleConfig(res, Track.WardensPaean, "WardensPaean", "Dispel", -10, BRD.AID.WardensPaean, 30);

        res.Define(Track.Troubadour).As<TroubOption>("Troubadour", "Troub", 500)
            .AddOption(TroubOption.Use87, "Use", "Use Troubadour", 120, 15, ActionTargets.Self, 62, 87)
            .AddOption(TroubOption.Use88, "Use", "Use Troubadour", 90, 15, ActionTargets.Self, 88)
            .AddOption(TroubOption.DontUse, "None", "Do not use automatically")
            .AddAssociatedActions(BRD.AID.Troubadour);

        DefineSimpleConfig(res, Track.NaturesMinne, "NaturesMinne", "Minne", 400, BRD.AID.NaturesMinne, 15);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3);
        ExecuteSimple(strategy.Option(Track.WardensPaean), BRD.AID.WardensPaean, Player);
        ExecuteSimple(strategy.Option(Track.Troubadour), BRD.AID.Troubadour, Player);
        ExecuteSimple(strategy.Option(Track.NaturesMinne), BRD.AID.NaturesMinne, Player);
    }
}
