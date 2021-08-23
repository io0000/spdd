namespace watabou.utils
{
    public interface IBundlable
    {
        void RestoreFromBundle(Bundle bundle);

        void StoreInBundle(Bundle bundle);
    }
}