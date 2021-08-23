namespace watabou.utils
{
    public abstract class PlatformSupport
    {
        public abstract void UpdateDisplaySize();

        public abstract void UpdateSystemUI();

        public abstract bool ConnectedToUnmeteredNetwork();

        //FIXME this is currently used because no platform-agnostic text input has been implemented.
        //should look into doing that using either plain openGL or Libgdx's libraries
        //public abstract void promptTextInput(string title, string hintText, int maxLen, bool multiLine,
        //							 string posTxt, string negTxt, TextCallback callback);
        //
        //public static abstract class TextCallback
        //{
        //	public abstract void onSelect(bool positive, string text);
        //}

        //TODO should consider spinning this into its own class, rather than platform support getting ever bigger

        public abstract void SetupFontGenerators(int pageSize, bool systemFont);

        public abstract void ResetGenerators();
        
        public abstract BitmapFont GetFont(int size, string text);
        
        public abstract string[] SplitforTextBlock(string text, bool multiline);
    }
}