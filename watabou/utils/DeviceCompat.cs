using watabou.noosa;

namespace watabou.utils
{
    //TODO migrate to platformSupport class
    public class DeviceCompat
    {
        public static bool SupportsFullScreen()
        {
            //switch (Gdx.app.getType()){
            //	case Android:
            //		//Android 4.4 KitKat and later, this is for immersive mode
            //		return Gdx.app.getVersion() >= 19;
            //	default:
            //		//TODO implement functionality for other platforms here
            //		return true;
            //}
            return true;
        }

        public static bool IsDesktop()
        {
            //	return Gdx.app.getType() == Application.ApplicationType.Desktop;
            return true;
        }

        public static bool HasHardKeyboard()
        {
            //return Gdx.input.isPeripheralAvailable(Input.Peripheral.HardwareKeyboard);
            return true;
        }

        //public static bool LegacyDevice()
        //{
        //    //switch (Gdx.app.getType()){
        //    //	case Android:
        //    //		//Devices prior to Android 4.1 Jelly Bean
        //    //		return Gdx.app.getVersion() < 16;
        //    //	default:
        //    //		//TODO implement functionality for other platforms here
        //    //		return false;
        //    //}
        //    return false;
        //}

        public static bool IsDebug()
        {
            return Game.version.Contains("INDEV");
        }

        public static void OpenURI(string URI)
        {
            //var intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse("http://" + Lnk));
            //Game.Instance.StartActivity(intent);

            //Gdx.net.openURI( URI );
        }

        public static void Log(string tag, string message)
        {
            //Gdx.app.log( tag, message );
        }
    }
}
