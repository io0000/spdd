using spdd.sprites;

namespace spdd.items.armor
{
    public class MailArmor : Armor
    {
        public MailArmor()
            : base(3)
        {
            image = ItemSpriteSheet.ARMOR_MAIL;
        }
    }
}