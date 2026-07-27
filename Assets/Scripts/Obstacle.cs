public abstract class Obstacle : GridItem
{
    public bool IsCleared { get; protected set; }

    // affectedCount:
    // Adjacent blast için komþu patlayan küp sayýsý,
    // special explosion için etkilenen obstacle hücresi sayýsýdýr.
    public abstract bool ApplyDamage(
        ObstacleDamageSource source,
        int affectedCount);
}