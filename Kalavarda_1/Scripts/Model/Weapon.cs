using System;

namespace Assets.Scripts.Model
{
    public interface IWeapon
    {
        float MaxDistance { get; }

        /// <summary>
        /// Сила удара (абсолютная)
        /// </summary>
        float Power { get; }
    }

    public abstract class Weapon
    {
        public abstract TimeSpan ShootPeriod { get; }

        public DateTime LastShootTime { get; private set; }

        public bool CanShoot => DateTime.Now - LastShootTime > ShootPeriod;

        public abstract float MaxDistance { get; }

        public static Weapon Empty { get; } = new EmptyWeapon();

        public bool Shoot(IHealth enemy, float ratio, float distance)
        {
            if (distance > MaxDistance)
                return false;

            var shoot = ShootImpl(enemy, ratio);

            if (shoot)
                LastShootTime = DateTime.Now;

            return shoot;
        }

        protected abstract bool ShootImpl(IHealth enemy, float ratio);
    }

    public sealed class EmptyWeapon: Weapon, IWeapon
    {
        public override TimeSpan ShootPeriod => TimeSpan.FromSeconds(0.1);

        public override float MaxDistance => 0;

        public float Power => 0;

        protected override bool ShootImpl(IHealth enemy, float ratio)
        {
            return false;
        }
    }

    public class Stick : Weapon, ISpawned, IThing, IWeapon
    {
        public override TimeSpan ShootPeriod => TimeSpan.FromSeconds(1);
        
        public override float MaxDistance => 2;

        public float Power => 3;

        protected override bool ShootImpl(IHealth enemy, float ratio)
        {
            //enemy.DecreaseHP((int)(Power * ratio), null);
            return true;
        }

        public bool SpendOnUse => false;

        public IThingPrototype Prototype => StickPrototype.Instance;
    }

    public class StickPrototype : IThingPrototype
    {
        public static StickPrototype Instance { get; } = new StickPrototype();

        public string Name => "Палка";

        public int BagMaxStackCount => 1;

        public IThing CreateInstance()
        {
            return new Stick();
        }
    }

    public class Axe : Weapon, IThing
    {
        private const int Power = 9;
        private const int Distance = 2;

        public override TimeSpan ShootPeriod => TimeSpan.FromSeconds(1.5);

        public override float MaxDistance => Distance;

        protected override bool ShootImpl(IHealth enemy, float ratio)
        {
            //enemy.DecreaseHP((int)(Power * ratio), null);
            return true;
        }

        public bool SpendOnUse => false;

        public IThingPrototype Prototype => AxePrototype.Instance;
    }

    public class AxePrototype : IThingPrototype
    {
        public static AxePrototype Instance { get; } = new AxePrototype();

        public string Name => "Топор";

        public int BagMaxStackCount => 1;

        public IThing CreateInstance()
        {
            return new Axe();
        }
    }

    public class Fist: IWeapon
    {
        public float MaxDistance => 1.55f;

        public float Power => 1;
    }
}
