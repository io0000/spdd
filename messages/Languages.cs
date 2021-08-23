using System.Globalization;
using System.Collections.Generic;

namespace spdd.messages
{
    public class Languages
    {
        public static Languages ENGLISH = new Languages("english", "en-US", Status.REVIEWED, null, null);
        public static Languages RUSSIAN = new Languages("русский", "ru-RU", Status.REVIEWED, new string[] { "ConsideredHamster", "Inevielle", "apxwn", "yarikonline" }, new string[] { "AttHawk46", "BlueberryShortcake", "HerrGotlieb", "HoloTheWise", "Ilbko", "JleHuBbluKoT", "MrXantar", "Nikets", "Originalej0name", "Raymundo", "Shamahan", "kirusyaga", "perefrazz", "roman.yagodin", "un_logic", "Вoвa" });
        public static Languages KOREAN = new Languages("한국어", "ko-KR", Status.UNREVIEWED, new string[] { "Cocoa", "Flameblast12", "GameConqueror", "Korean2017" }, new string[] { "WondarRabb1t", "ddojin0115", "eeeei", "enjuxx", "hancyel", "linterpreteur", "lsiebnie" });
        public static Languages CHINESE = new Languages("中文", "zh-CN", Status.UNREVIEWED, new string[] { "Jinkeloid(zdx00793)", "endlesssolitude" }, new string[] { "931451545", "Fatir", "Fishbone", "HoofBumpBlurryface", "Lery", "Lyn_0401", "ShatteredFlameBlast", "hmdzl001", "tempest102" });
        public static Languages POLISH = new Languages("polski", "pl-PL", Status.REVIEWED, new string[] { "Deksippos", "MrKukurykpl", "kuadziw", "szymex73" }, new string[] { "Chasseur", "Darden", "DarkKnightComes", "KarixDaii", "MJedi", "Odiihinia", "Peperos", "Scharnvirk", "VasteelXolotl", "bvader95", "dusakus", "michaub", "ozziezombie", "szczoteczka22", "transportowiec96" });
        public static Languages SPANISH = new Languages("español", "es-ES", Status.UNREVIEWED, new string[] { "Kiroto", "Kohru", "airman12", "grayscales" }, new string[] { "AdventurerKilly", "Alesxanderk", "CorvosUtopy", "Dewstend", "Dyrran", "Fervoreking", "Illyatwo2", "JPCHZ", "LastCry", "Marquezo_577_284", "STKmonoqui", "Sh4rkill3r", "alfongad", "benzarr410", "chepe567.jc", "ctrijueque", "damc0616", "dhg121", "javifs", "jonismack1", "magmax", "tres.14159" });
        public static Languages GERMAN = new Languages("deutsch", "de-DE", Status.REVIEWED, new string[] { "Dallukas", "KrystalCroft", "Wuzzy", "Zap0", "apxwn", "bernhardreiter", "davedude" }, new string[] { "Abracadabra", "Ceeee", "DarkPixel", "ErichME", "Faquarl", "LenzB", "Ordoviz", "Sarius", "SirEddi", "Sorpl3x", "ThunfischGott", "Topicranger", "azrdev", "carrageen", "gekko303", "jeinzi", "johannes.schobel", "karoshi42", "koryphea", "luciocarreras", "niemand", "oragothen", "spixi" });
        public static Languages FRENCH = new Languages("français", "fr-FR", Status.UNREVIEWED, new string[] { "Emether", "TheKappaDuWeb", "Xalofar", "canc42", "kultissim", "minikrob" }, new string[] { "Alsydis", "Axce", "Az_zahr", "Bastien72", "Basttee", "Dekadisk", "Draal", "Martin.Bellet", "Neopolitan", "Nyrnx", "Petit_Chat", "RomTheMareep", "RunningColours", "SpeagleZNT", "Tronche2Cake", "VRad", "Ygdrazil", "_nim_", "antoine9298", "clexanis", "go11um", "hydrasho", "levilbatard", "linterpreteur", "maeltur70", "marmous", "mluzarreta", "solthaar", "speagle", "vavavoum", "zM_" });
        public static Languages PORTUGUESE = new Languages("português", "pt-PT", Status.UNREVIEWED, new string[] { "Chacal.Ex", "TDF2001", "matheus208" }, new string[] { "Bigode935", "ChainedFreaK", "Helen0903", "JST", "MadHorus", "MarkusCoisa", "Matie", "Tio_P_(Krampus)", "ancientorange", "danypr23", "denis.gnl", "ismael.henriques12", "mfcord", "owenreilly", "rafazago", "try31" });
        public static Languages ITALIAN = new Languages("italiano", "it-IT", Status.UNREVIEWED, new string[] { "bizzolino", "funnydwarf" }, new string[] { "4est", "DaniMare", "Danzl", "Guiller124", "andrearubbino00", "nessunluogo", "righi.a", "umby000" });
        public static Languages CZECH = new Languages("čeština", "cs-CZ", Status.REVIEWED, new string[] { "ObisMike" }, new string[] { "AshenShugar", "Autony", "Buba237", "JStrange", "RealBrofessor", "chuckjirka" });
        public static Languages FINNISH = new Languages("suomi", "fi-FI", Status.INCOMPLETE, new string[] { "TenguTheKnight" }, new string[] { "Sautari" });
        public static Languages TURKISH = new Languages("Türkçe", "tr-TR", Status.UNREVIEWED, new string[] { "LokiofMillenium", "emrebnk" }, new string[] { "AGORAAA", "AcuriousPotato", "alikeremozfidan", "alpekin98", "denizakalin", "erdemozdemir98", "hasantahsin160", "immortalsamuraicn", "kayikyaki", "melezorus34", "mitux" });
        public static Languages HUNGARIAN = new Languages("magyar", "hu-HU", Status.UNREVIEWED, new string[] { "dorheim", "szalaik" }, new string[] { "Navetelen", "acszoltan111", "clarovani", "dhialub", "nanometer", "nardomaa", "savarall" });
        public static Languages JAPANESE = new Languages("日本語", "ja-JP", Status.INCOMPLETE, null, new string[] { "Gosamaru", "amama", "librada", "mocklike" });
        public static Languages INDONESIAN = new Languages("indonésien", "id-ID", Status.INCOMPLETE, new string[] { "rakapratama" }, new string[] { "ZangieF347", "esprogarap" });
        public static Languages UKRANIAN = new Languages("українська", "uk-UA", Status.REVIEWED, new string[] { "Oster" }, new string[] { "Sadsaltan1", "TheGuyBill", "Volkov", "ZverWolf", "ingvarfed", "oliolioxinfree", "romanokurg", "vlisivka" });
        public static Languages CATALAN = new Languages("català", "ca-AD", Status.REVIEWED, new string[] { "Illyatwo2" }, new string[] { "Elosy", "n1ngu" });
        public static Languages BASQUE = new Languages("euskara", "eu-ES", Status.INCOMPLETE, new string[] { "Deathrevenge", "Osoitz" }, null);
        public static Languages ESPERANTO = new Languages("esperanto", "eo", Status.REVIEWED, new string[] { "Verdulo" }, new string[] { "Raizin" });

        public static IEnumerable<Languages> Values()
        {
            yield return ENGLISH;
            yield return RUSSIAN;
            yield return KOREAN;
            yield return CHINESE;
            yield return POLISH;
            yield return SPANISH;
            yield return GERMAN;
            yield return FRENCH;
            yield return PORTUGUESE;
            yield return ITALIAN;
            yield return CZECH;
            yield return FINNISH;
            yield return TURKISH;
            yield return HUNGARIAN;
            yield return JAPANESE;
            yield return INDONESIAN;
            yield return UKRANIAN;
            yield return CATALAN;
            yield return BASQUE;
            yield return ESPERANTO;
        }

        public enum Status
        {
            //below 80% complete languages are not added.
            INCOMPLETE, //80-99% complete
            UNREVIEWED, //100% complete
            REVIEWED //100% reviewed
        }

        private string name;
        private string code;    // ex) en-US, ru-RU, ko-KR,...
        private Status status;
        private string[] reviewers;
        private string[] translators;

        public Languages(string name, string code, Status status, string[] reviewers, string[] translators)
        {
            this.name = name;
            this.code = code;
            this.status = status;
            this.reviewers = reviewers;
            this.translators = translators;
        }

        public string NativeName()
        {
            return name;
        }

        public string Code()
        {
            return code;
        }

        public Status GetStatus()
        {
            return status;
        }

        public string[] Reviewers()
        {
            if (reviewers == null)
            {
                return new string[] { };
            }
            else
            {
                //return (string[])reviewers.Clone();
                return reviewers;
            }
        }

        public string[] Translators()
        {
            if (translators == null)
            {
                return new string[] { };
            }
            else
            {
                //return (string[])translators.Clone();
                return translators;
            }
        }

        public static Languages MatchLocale(CultureInfo cultureInfo)
        {
            // cultureInfo.Name: en-US, ru-RU, ko-KR, ...
            return MatchCode(cultureInfo.Name);
        }

        public static Languages MatchCode(string code)
        {
            foreach (Languages lang in Values())
            {
                if (lang.Code().Equals(code))
                {
                    return lang;
                }
            }
            return ENGLISH;
        }
    }
}