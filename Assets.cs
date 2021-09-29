namespace spdd
{
    public class Assets
    {
        public class Effects
        {
            public const string EFFECTS        = "effects/effects.png";
            public const string FIREBALL       = "effects/fireball.png";
            public const string SPECKS         = "effects/specks.png";
            public const string SPELL_ICONS    = "effects/spell_icons.png";
        }

        public class Environment
        {
            public const string TERRAIN_FEATURES = "environment/terrain_features.png";

            public const string VISUAL_GRID      = "environment/visual_grid.png";
            public const string WALL_BLOCKING    = "environment/wall_blocking.png";

            public const string TILES_SEWERS     = "environment/tiles_sewers.png";
            public const string TILES_PRISON     = "environment/tiles_prison.png";
            public const string TILES_CAVES      = "environment/tiles_caves.png";
            public const string TILES_CITY       = "environment/tiles_city.png";
            public const string TILES_HALLS      = "environment/tiles_halls.png";

            public const string WATER_SEWERS     = "environment/water0.png";
            public const string WATER_PRISON     = "environment/water1.png";
            public const string WATER_CAVES      = "environment/water2.png";
            public const string WATER_CITY       = "environment/water3.png";
            public const string WATER_HALLS      = "environment/water4.png";

            public const string WEAK_FLOOR       = "environment/custom_tiles/weak_floor.png";
            public const string SEWER_BOSS       = "environment/custom_tiles/sewer_boss.png";
            public const string PRISON_QUEST     = "environment/custom_tiles/prison_quests.png";
            public const string PRISON_EXIT_OLD  = "environment/custom_tiles/prison_exit_old.png";
            public const string PRISON_EXIT_NEW  = "environment/custom_tiles/prison_exit_new.png";
            public const string CAVES_BOSS       = "environment/custom_tiles/caves_boss.png";
            public const string CITY_BOSS        = "environment/custom_tiles/city_boss.png";
            public const string HALLS_SP         = "environment/custom_tiles/halls_special.png";
        }

        //TODO include other font assets here? Some are platform specific though...
        public class Fonts
        {
            public const string PIXELFONT= "fonts/pixel_font.png";
        }

        public class Interfaces
        {
            public const string ARCS_BG   = "interfaces/arcs1.png";
            public const string ARCS_FG   = "interfaces/arcs2.png";

            public const string BANNERS   = "interfaces/banners.png";
            public const string BADGES    = "interfaces/badges.png";
            public const string LOCKED    = "interfaces/locked_badge.png";

            public const string CHROME    = "interfaces/chrome.png";
            public const string ICONS     = "interfaces/icons.png";
            public const string STATUS    = "interfaces/status_pane.png";
            public const string MENU      = "interfaces/menu_button.png";
            public const string HP_BAR    = "interfaces/hp_bar.png";
            public const string SHLD_BAR  = "interfaces/shield_bar.png";
            public const string XP_BAR    = "interfaces/exp_bar.png";
            public const string TOOLBAR   = "interfaces/toolbar.png";
            public const string SHADOW    = "interfaces/shadow.png";
            public const string BOSSHP    = "interfaces/boss_hp.png";

            public const string SURFACE   = "interfaces/surface.png";

            public const string LOADING_SEWERS = "interfaces/loading_sewers.png";
            public const string LOADING_PRISON = "interfaces/loading_prison.png";
            public const string LOADING_CAVES  = "interfaces/loading_caves.png";
            public const string LOADING_CITY   = "interfaces/loading_city.png";
            public const string LOADING_HALLS  = "interfaces/loading_halls.png";
            public const string BUFFS_SMALL    = "interfaces/buffs.png";
            public const string BUFFS_LARGE    = "interfaces/large_buffs.png";
            public const string CONS_ICONS     = "interfaces/consumable_icons.png";
        }

        //these points to resource bundles, not raw asset files
        public class Messages
        {
            public const string ACTORS   = "messages/actors/actors";
            public const string ITEMS    = "messages/items/items";
            public const string JOURNAL  = "messages/journal/journal";
            public const string LEVELS   = "messages/levels/levels";
            public const string MISC     = "messages/misc/misc";
            public const string PLANTS   = "messages/plants/plants";
            public const string SCENES   = "messages/scenes/scenes";
            public const string UI       = "messages/ui/ui";
            public const string WINDOWS  = "messages/windows/windows";
        }

        public class Music
        {
            public const string GAME     = "music/game.ogg";
            public const string SURFACE  = "music/surface.ogg";
            public const string THEME    = "music/theme.ogg";
        }

        public class Sounds
        {
            public const string CLICK     = "sounds/click.wav";
            public const string BADGE     = "sounds/badge.wav";
            public const string GOLD      = "sounds/gold.wav";

            public const string OPEN      = "sounds/door_open.wav";
            public const string UNLOCK    = "sounds/unlock.wav";
            public const string ITEM      = "sounds/item.wav";
            public const string DEWDROP   = "sounds/dewdrop.wav";
            public const string STEP      = "sounds/step.wav";
            public const string WATER     = "sounds/water.wav";
            public const string GRASS     = "sounds/grass.wav";
            public const string TRAMPLE   = "sounds/trample.wav";
            public const string STURDY    = "sounds/sturdy.wav";

            public const string HIT              = "sounds/hit.wav";
            public const string MISS             = "sounds/miss.wav";
            public const string HIT_SLASH        = "sounds/hit_slash.wav";
            public const string HIT_STAB         = "sounds/hit_stab.wav";
            public const string HIT_CRUSH        = "sounds/hit_crush.wav";
            public const string HIT_MAGIC        = "sounds/hit_magic.wav";
            public const string HIT_STRONG       = "sounds/hit_strong.wav";
            public const string HIT_PARRY        = "sounds/hit_parry.wav";
            public const string HIT_ARROW        = "sounds/hit_arrow.wav";
            public const string ATK_SPIRITBOW    = "sounds/atk_spiritbow.wav";
            public const string ATK_CROSSBOW     = "sounds/atk_crossbow.wav";
            public const string HEALTH_WARN      = "sounds/health_warn.wav";
            public const string HEALTH_CRITICAL  = "sounds/health_critical.wav";

            public const string DESCEND    = "sounds/descend.wav";
            public const string EAT        = "sounds/eat.wav";
            public const string READ       = "sounds/read.wav";
            public const string LULLABY    = "sounds/lullaby.wav";
            public const string DRINK      = "sounds/drink.wav";
            public const string SHATTER    = "sounds/shatter.wav";
            public const string ZAP        = "sounds/zap.wav";
            public const string LIGHTNING  = "sounds/lightning.wav";
            public const string LEVELUP    = "sounds/levelup.wav";
            public const string DEATH      = "sounds/death.wav";
            public const string CHALLENGE  = "sounds/challenge.wav";
            public const string CURSED     = "sounds/cursed.wav";
            public const string TRAP       = "sounds/trap.wav";
            public const string EVOKE      = "sounds/evoke.wav";
            public const string TOMB       = "sounds/tomb.wav";
            public const string ALERT      = "sounds/alert.wav";
            public const string MELD       = "sounds/meld.wav";
            public const string BOSS       = "sounds/boss.wav";
            public const string BLAST      = "sounds/blast.wav";
            public const string PLANT      = "sounds/plant.wav";
            public const string RAY        = "sounds/ray.wav";
            public const string BEACON     = "sounds/beacon.wav";
            public const string TELEPORT   = "sounds/teleport.wav";
            public const string CHARMS     = "sounds/charms.wav";
            public const string MASTERY    = "sounds/mastery.wav";
            public const string PUFF       = "sounds/puff.wav";
            public const string ROCKS      = "sounds/rocks.wav";
            public const string BURNING    = "sounds/burning.wav";
            public const string FALLING    = "sounds/falling.wav";
            public const string GHOST      = "sounds/ghost.wav";
            public const string SECRET     = "sounds/secret.wav";
            public const string BONES      = "sounds/bones.wav";
            public const string BEE        = "sounds/bee.wav";
            public const string DEGRADE    = "sounds/degrade.wav";
            public const string MIMIC      = "sounds/mimic.wav";
            public const string DEBUFF     = "sounds/debuff.wav";
            public const string CHARGEUP   = "sounds/chargeup.wav";
            public const string GAS        = "sounds/gas.wav";
            public const string CHAINS     = "sounds/chains.wav";
            public const string SCAN       = "sounds/scan.wav";
            public const string SHEEP      = "sounds/sheep.wav";

            public static readonly string[] all = new string[] {
                CLICK, BADGE, GOLD,

                OPEN, UNLOCK, ITEM, DEWDROP, STEP, WATER, GRASS, TRAMPLE, STURDY,

                HIT, MISS, HIT_SLASH, HIT_STAB, HIT_CRUSH, HIT_MAGIC, HIT_STRONG, HIT_PARRY,
                HIT_ARROW, ATK_SPIRITBOW, ATK_CROSSBOW, HEALTH_WARN, HEALTH_CRITICAL,

                DESCEND, EAT, READ, LULLABY, DRINK, SHATTER, ZAP, LIGHTNING, LEVELUP, DEATH,
                CHALLENGE, CURSED, TRAP, EVOKE, TOMB, ALERT, MELD, BOSS, BLAST, PLANT, RAY, BEACON,
                TELEPORT, CHARMS, MASTERY, PUFF, ROCKS, BURNING, FALLING, GHOST, SECRET, BONES,
                BEE, DEGRADE, MIMIC, DEBUFF, CHARGEUP, GAS, CHAINS, SCAN, SHEEP
            };
        }

        public class Splashes
        {
            public const string WARRIOR    = "splashes/warrior.jpg";
            public const string MAGE       = "splashes/mage.jpg";
            public const string ROGUE      = "splashes/rogue.jpg";
            public const string HUNTRESS   = "splashes/huntress.jpg";
        }

        public class Sprites
        {
            public const string ITEMS        = "sprites/items.png";
            public const string ITEM_ICONS   = "sprites/item_icons.png";

            public const string WARRIOR   = "sprites/warrior.png";
            public const string MAGE      = "sprites/mage.png";
            public const string ROGUE     = "sprites/rogue.png";
            public const string HUNTRESS  = "sprites/huntress.png";
            public const string AVATARS   = "sprites/avatars.png";
            public const string PET       = "sprites/pet.png";
            public const string AMULET    = "sprites/amulet.png";

            public const string RAT       = "sprites/rat.png";
            public const string BRUTE     = "sprites/brute.png";
            public const string SPINNER   = "sprites/spinner.png";
            public const string DM300     = "sprites/dm300.png";
            public const string WRAITH    = "sprites/wraith.png";
            public const string UNDEAD    = "sprites/undead.png";
            public const string KING      = "sprites/king.png";
            public const string PIRANHA   = "sprites/piranha.png";
            public const string EYE       = "sprites/eye.png";
            public const string GNOLL     = "sprites/gnoll.png";
            public const string CRAB      = "sprites/crab.png";
            public const string GOO       = "sprites/goo.png";
            public const string SWARM     = "sprites/swarm.png";
            public const string SKELETON  = "sprites/skeleton.png";
            public const string SHAMAN    = "sprites/shaman.png";
            public const string THIEF     = "sprites/thief.png";
            public const string TENGU     = "sprites/tengu.png";
            public const string SHEEP     = "sprites/sheep.png";
            public const string KEEPER    = "sprites/shopkeeper.png";
            public const string BAT       = "sprites/bat.png";
            public const string ELEMENTAL = "sprites/elemental.png";
            public const string MONK      = "sprites/monk.png";
            public const string WARLOCK   = "sprites/warlock.png";
            public const string GOLEM     = "sprites/golem.png";
            public const string STATUE    = "sprites/statue.png";
            public const string SUCCUBUS  = "sprites/succubus.png";
            public const string SCORPIO   = "sprites/scorpio.png";
            public const string FISTS     = "sprites/yog_fists.png";
            public const string YOG       = "sprites/yog.png";
            public const string LARVA     = "sprites/larva.png";
            public const string GHOST     = "sprites/ghost.png";
            public const string MAKER     = "sprites/wandmaker.png";
            public const string TROLL     = "sprites/blacksmith.png";
            public const string IMP       = "sprites/demon.png";
            public const string RATKING   = "sprites/ratking.png";
            public const string BEE       = "sprites/bee.png";
            public const string MIMIC     = "sprites/mimic.png";
            public const string ROT_LASH  = "sprites/rot_lasher.png";
            public const string ROT_HEART = "sprites/rot_heart.png";
            public const string GUARD     = "sprites/guard.png";
            public const string WARDS     = "sprites/wards.png";
            public const string GUARDIAN  = "sprites/guardian.png";
            public const string SLIME     = "sprites/slime.png";
            public const string SNAKE     = "sprites/snake.png";
            public const string NECRO     = "sprites/necromancer.png";
            public const string GHOUL     = "sprites/ghoul.png";
            public const string RIPPER    = "sprites/ripper.png";
            public const string SPAWNER   = "sprites/spawner.png";
            public const string DM100     = "sprites/dm100.png";
            public const string PYLON     = "sprites/pylon.png";
            public const string DM200     = "sprites/dm200.png";
            public const string LOTUS     = "sprites/lotus.png";
        }
    }
}