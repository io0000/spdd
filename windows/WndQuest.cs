using spdd.actors.mobs.npcs;
using spdd.messages;

namespace spdd.windows
{
    public class WndQuest : WndTitledMessage
    {
        public WndQuest(NPC questgiver, string text)
            : base(questgiver.GetSprite(),
                  Messages.TitleCase(questgiver.Name()),
                  text)
        { }
    }
}