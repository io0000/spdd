using spdd.sprites;

namespace spdd.items.weapon.melee
{
    public class BattleAxe : MeleeWeapon
    {
        public BattleAxe()
        {
            image = ItemSpriteSheet.BATTLE_AXE;
            hitSound = Assets.Sounds.HIT_SLASH;
            hitSoundPitch = 0.9f;

            tier = 4;
            ACC = 1.24f; //24% boost to accuracy
        }

        public override int Max(int lvl)
        {
            return 4 * (tier + 1) +    //20 base, down from 25
                   lvl * (tier + 1);   //scaling unchanged
        }
    }
}