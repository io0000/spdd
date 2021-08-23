using spdd.effects;
using spdd.messages;

namespace spdd.actors.blobs
{
    public class Blizzard : Blob
    {
        protected override void Evolve()
        {
            base.Evolve();

            int cell;

            Fire fire = (Fire)Dungeon.level.GetBlob(typeof(Fire));
            Freezing freeze = (Freezing)Dungeon.level.GetBlob(typeof(Freezing));
            Inferno inf = (Inferno)Dungeon.level.GetBlob(typeof(Inferno));

            for (int i = area.left; i < area.right; ++i)
            {
                for (int j = area.top; j < area.bottom; ++j)
                {
                    cell = i + j * Dungeon.level.Width();

                    if (cur[cell] > 0)
                    {
                        if (fire != null) 
                            fire.Clear(cell);
                        if (freeze != null) 
                            freeze.Clear(cell);

                        if (inf != null && inf.volume > 0 && inf.cur[cell] > 0)
                        {
                            inf.Clear(cell);
                            off[cell] = cur[cell] = 0;
                            continue;
                        }

                        // TODO확인 : 두번 호출하는 것 맞나
                        // (눈보라는 서리 효과의 2배 속도로 대상을 얼린다. 물 타일 위라면 한 턴만에 빙결되며...)
                        Freezing.Freeze(cell);
                        Freezing.Freeze(cell);
                    }
                }
            }
        }

        public override void Use(BlobEmitter emitter)
        {
            base.Use(emitter);
            emitter.Pour(Speck.Factory(Speck.BLIZZARD, true), 0.4f);
        }

        public override string TileDesc()
        {
            return Messages.Get(this, "desc");
        }
    }
}