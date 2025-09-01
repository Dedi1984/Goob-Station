using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Inventory;
using Content.Shared.Movement.Components;
using Content.Server.Damage.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Stunnable;
using Content.Server.Inventory;
using Content.Server.Polymorph.Systems;
using Content.Server.Access.Systems;
using Content.Shared.Access;
using Content.Shared.Movement.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Linq;

namespace Content.Goobstation.Server.FateArcade;

[RegisterComponent]
public sealed partial class BoneOfFateComponent : Component
{
}

public sealed class BoneOfFateSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly ServerInventorySystem _inventory = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly AccessSystem _access = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _move = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BoneOfFateComponent, UseInHandEvent>(OnUse);
    }

    private void OnUse(EntityUid uid, BoneOfFateComponent component, UseInHandEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        var user = args.User;
        var roll = _random.Next(1, 21);

        switch (roll)
        {
            case 1:
                _popup.PopupEntity("You are obliterated!", uid, user);
                _explosion.QueueExplosion(user, ExplosionSystem.DefaultExplosionPrototypeId, 100, 3, 10);
                break;
            case 2:
                _popup.PopupEntity("You die instantly!", uid, user);
                _damage.TryChangeDamage(user, new DamageSpecifier(_proto.Index<DamageGroupPrototype>("Brute"), 1000), true);
                break;
            case 3:
                _popup.PopupEntity("A pack of monsters appears!", uid, user);
                for (var i = 0; i < 5; i++)
                    EntityManager.SpawnEntity("MobCarp", Transform(user).Coordinates);
                break;
            case 4:
                _popup.PopupEntity("Your equipment disintegrates!", uid, user);
                if (TryComp<InventoryComponent>(user, out var inv))
                {
                    var enumerator = new InventorySystem.InventorySlotEnumerator(inv);
                    while (enumerator.NextItem(out var item))
                        QueueDel(item);
                }
                break;
            case 5:
                _popup.PopupEntity("You turn into a monkey!", uid, user);
                _polymorph.PolymorphEntity(user, "Monkey");
                break;
            case 6:
                _popup.PopupEntity("You feel sluggish...", uid, user);
                var speed = EnsureComp<MovementSpeedModifierComponent>(user);
                _move.ChangeBaseSpeed(user,
                    speed.BaseWalkSpeed * 0.5f,
                    speed.BaseSprintSpeed * 0.5f,
                    speed.BaseAcceleration,
                    speed);
                _move.RefreshMovementSpeedModifiers(user, speed);
                break;
            case 7:
                _popup.PopupEntity("A painful shock hits you!", uid, user);
                _stun.TryParalyze(user, TimeSpan.FromSeconds(5), true);
                _damage.TryChangeDamage(user, new DamageSpecifier(_proto.Index<DamageGroupPrototype>("Brute"), 50), true);
                break;
            case 8:
                _popup.PopupEntity("You explode!", uid, user);
                _explosion.QueueExplosion(user, ExplosionSystem.DefaultExplosionPrototypeId, 20, 3, 5, user: user);
                break;
            case 9:
                _popup.PopupEntity("You catch a cold.", uid, user);
                _damage.TryChangeDamage(user, new DamageSpecifier(_proto.Index<DamageTypePrototype>("Cold"), 5), true);
                break;
            case 10:
                _popup.PopupEntity("Nothing happens...", uid, user);
                break;
            case 11:
                _popup.PopupEntity("A cookie pops out!", uid, user);
                EntityManager.SpawnEntity("FoodCookie", Transform(user).Coordinates);
                break;
            case 12:
                _popup.PopupEntity("You feel rejuvenated!", uid, user);
                if (TryComp<DamageableComponent>(user, out var damage))
                    _damage.SetAllDamage(user, damage, 0);
                break;
            case 13:
                _popup.PopupEntity("You receive some cash!", uid, user);
                EntityManager.SpawnEntity("SpaceCash", Transform(user).Coordinates);
                break;
            case 14:
                _popup.PopupEntity("A revolver materializes!", uid, user);
                EntityManager.SpawnEntity("WeaponRevolverPython", Transform(user).Coordinates);
                break;
            case 15:
                _popup.PopupEntity("A spellbook appears!", uid, user);
                EntityManager.SpawnEntity("WizardsGrimoire", Transform(user).Coordinates);
                break;
            case 16:
                _popup.PopupEntity("A warden's locker materializes!", uid, user);
                EntityManager.SpawnEntity("LockerWardenFilled", Transform(user).Coordinates);
                break;
            case 17:
                _popup.PopupEntity("A suspicious beacon offers syndicate gear!", uid, user);
                EntityManager.SpawnEntity("ClothingBackpackDuffelSyndicateEVABundle", Transform(user).Coordinates);
                EntityManager.SpawnEntity("ClothingBackpackDuffelSyndicateMedicalBundleFilled", Transform(user).Coordinates);
                EntityManager.SpawnEntity("ClothingBackpackDuffelZombieBundle", Transform(user).Coordinates);
                break;
            case 18:
                _popup.PopupEntity("You suddenly have full access!", uid, user);
                var allAccess = _proto.EnumeratePrototypes<AccessLevelPrototype>()
                    .Select(p => new ProtoId<AccessLevelPrototype>(p.ID)).ToArray();
                _access.TrySetTags(user, allAccess);
                break;
            case 19:
                _popup.PopupEntity("Your body hardens against harm!", uid, user);
                _damage.SetDamageModifierSetId(user, "BoneOfFateHalfDamage");
                break;
            case 20:
                _popup.PopupEntity("Arcane power fills you!", uid, user);
                EntityManager.SpawnEntity("ClothingHeadHatWizard", Transform(user).Coordinates);
                EntityManager.SpawnEntity("ClothingOuterWizard", Transform(user).Coordinates);
                EntityManager.SpawnEntity("ClothingShoesWizard", Transform(user).Coordinates);
                EntityManager.SpawnEntity("ClothingUniformJumpsuitColorDarkBlue", Transform(user).Coordinates);
                EntityManager.SpawnEntity("WizardsGrimoire", Transform(user).Coordinates);
                break;
            default:
                _popup.PopupEntity($"Nothing happens ({roll}).", uid, user);
                break;
        }

        Del(uid);
    }
}
