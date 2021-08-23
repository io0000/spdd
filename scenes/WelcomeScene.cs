using System;
using watabou.noosa;
using watabou.glwrap;
using spdd.ui;
using spdd.effects;
using spdd.messages;

namespace spdd.scenes
{
    public class WelcomeScene : PixelScene
    {
        private const int LATEST_UPDATE = ShatteredPixelDungeonDash.v0_8_2;

        public override void Create()
        {
            base.Create();

            int previousVersion = SPDSettings.Version();

            if (ShatteredPixelDungeonDash.versionCode == previousVersion && !SPDSettings.Intro())
            {
                ShatteredPixelDungeonDash.SwitchNoFade(typeof(TitleScene));
                return;
            }

            uiCamera.visible = false;

            int w = Camera.main.width;
            int h = Camera.main.height;

            Image title = BannerSprites.Get(BannerSprites.Type.PIXEL_DUNGEON);
            title.Brightness(0.6f);
            Add(title);

            float topRegion = Math.Max(title.height - 6, h * 0.45f);

            title.x = (w - title.Width()) / 2f;
            title.y = 2 + (topRegion - title.Height()) / 2f;

            Align(title);

            var signs = new SignsImage(BannerSprites.Get(BannerSprites.Type.PIXEL_DUNGEON_SIGNS));
            signs.x = title.x + (title.Width() - signs.Width()) / 2f;
            signs.y = title.y;
            Add(signs);

            var okay = new OkayButton(Chrome.Type.GREY_BUTTON_TR, Messages.Get(this, "continue"));
            okay.previousVersion = previousVersion;
            okay.scene = this;
            float buttonY = Math.Min(topRegion + (PixelScene.Landscape() ? 60 : 120), h - 24);

            if (previousVersion != 0 && !SPDSettings.Intro())
            {
                var changes = new ChangesButton(Chrome.Type.GREY_BUTTON_TR, Messages.Get(typeof(TitleScene), "changes"));
                changes.previousVersion = previousVersion;
                changes.scene = this;

                okay.SetRect(title.x, buttonY, (title.Width() / 2) - 2, 20);
                Add(okay);

                changes.SetRect(okay.Right() + 2, buttonY, (title.Width() / 2) - 2, 20);
                changes.Icon(Icons.CHANGES.Get());
                Add(changes);
            }
            else
            {
                okay.Text(Messages.Get(typeof(TitleScene), "enter"));
                okay.SetRect(title.x, buttonY, title.Width(), 20);
                okay.Icon(Icons.ENTER.Get());
                Add(okay);
            }

            RenderedTextBlock text = PixelScene.RenderTextBlock(6);
            string message;
            if (previousVersion == 0 || SPDSettings.Intro())
            {
                message = Messages.Get(this, "welcome_msg");
            }
            else if (previousVersion <= ShatteredPixelDungeonDash.versionCode)
            {
                if (previousVersion < LATEST_UPDATE)
                {
                    message = Messages.Get(this, "update_intro");
                    message += "\n\n" + Messages.Get(this, "update_msg");
                }
                else
                {
                    //TODO: change the messages here in accordance with the type of patch.
                    message = Messages.Get(this, "patch_intro");
                    message += "\n";
                    message += "\n" + Messages.Get(this, "patch_balance");
                    message += "\n" + Messages.Get(this, "patch_bugfixes");
                    message += "\n" + Messages.Get(this, "patch_translations");
                }
            }
            else
            {
                message = Messages.Get(this, "what_msg");
            }
            text.Text(message, w - 20);
            float textSpace = okay.Top() - topRegion - 4;
            text.SetPos((w - text.Width()) / 2f, (topRegion + 2) + (textSpace - text.Height()) / 2);
            Add(text);
        }

        private class SignsImage : Image
        {
            private float time;

            public SignsImage(Image src)
                : base(src)
            { }

            public override void Update()
            {
                base.Update();
                am = Math.Max(0f, (float)Math.Sin(time += Game.elapsed));
                if (time >= 1.5f * Math.PI)
                    time = 0;
            }

            public override void Draw()
            {
                Blending.SetLightMode();
                base.Draw();
                Blending.SetNormalMode();
            }
        }  // SignsImage

        // 계속버튼
        private class OkayButton : StyledButton
        {
            internal int previousVersion;
            internal WelcomeScene scene;

            public OkayButton(Chrome.Type type, string label)
                : base(type, label)
            { }

            protected override void OnClick()
            {
                base.OnClick();
                if (previousVersion == 0 || SPDSettings.Intro())
                {
                    SPDSettings.Version(ShatteredPixelDungeonDash.versionCode);
                    GamesInProgress.selectedClass = null;
                    GamesInProgress.curSlot = 1;
                    ShatteredPixelDungeonDash.SwitchScene(typeof(HeroSelectScene));
                }
                else
                {
                    //scene.UpdateVersion(previousVersion);
                    ShatteredPixelDungeonDash.SwitchScene(typeof(TitleScene));
                }
            }
        }  // OkayButton

        // 변경버튼( 픽던 갱신용 변경 )
        private class ChangesButton : StyledButton
        {
            internal int previousVersion;
            internal WelcomeScene scene;

            public ChangesButton(Chrome.Type type, string label)
                : base(type, label)
            { }

            protected override void OnClick()
            {
                base.OnClick();
                //scene.UpdateVersion(previousVersion);
                //ShatteredPixelDungeon.SwitchScene(typeof(ChangesScene));
            }
        }  // ChangesButton

        //private void UpdateVersion(int previousVersion)
        //{
        //    //update rankings, to update any data which may be outdated
        //    if (previousVersion < LATEST_UPDATE)
        //    {
        //        try
        //        {
        //            Rankings.Instance.Load();
        //            foreach (Rankings.Record rec in Rankings.Instance.records.ToArray())
        //            {
        //                try
        //                {
        //                    Rankings.Instance.LoadGameData(rec);
        //                    Rankings.Instance.SaveGameData(rec);
        //                }
        //                catch (Exception e)
        //                {
        //                    //if we encounter a fatal per-record error, then clear that record
        //                    Rankings.Instance.records.Remove(rec);
        //                    ShatteredPixelDungeon.ReportException(e);
        //                }
        //            }
        //            Rankings.Instance.Save();
        //        }
        //        catch (Exception e)
        //        {
        //            //if we encounter a fatal error, then just clear the rankings
        //            FileUtils.DeleteFile(Rankings.RANKINGS_FILE);
        //            ShatteredPixelDungeon.ReportException(e);
        //        }
        //    }
        //
        //    SPDSettings.Version(ShatteredPixelDungeon.versionCode);
        //}
    }
}