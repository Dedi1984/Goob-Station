using Content.Server.Radiation.Systems;

using Content.Shared.Interaction;
using Content.Shared.Hands.EntitySystems;

using Content.Shared.Interaction.Events;
using Content.Shared.Hands.Systems;

using Robust.Shared.Random;

namespace Content.Goobstation.Server.FateArcade;

[RegisterComponent]
public sealed partial class FateArcadeComponent : Component
{
    [DataField]
    public string BonePrototype = "BoneOfFate";

    [DataField]
    public float BoneChance = 0.05f;

    [DataField]
    public float Radiation = 30f;
}

public sealed class FateArcadeSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly RadiationSystem _radiation = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<FateArcadeComponent, InteractHandEvent>(OnInteract);
    }

    private void OnInteract(EntityUid uid, FateArcadeComponent component, InteractHandEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        var user = args.User;
        if (user == default)
            return;

        // apply radiation damage
        _radiation.IrradiateEntity(user, component.Radiation, 1f);

        // 5% chance to give bone of fate
        if (_random.NextFloat() <= component.BoneChance)
        {
            var bone = EntityManager.SpawnEntity(component.BonePrototype, Transform(user).Coordinates);
            _hands.TryPickupAnyHand(user, bone, checkActionBlocker: false);
        }
    }
}
