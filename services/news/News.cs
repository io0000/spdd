using System;
using System.Collections.Generic;
using watabou.noosa;
using spdd.ui;

namespace spdd.services.news
{
    public class NewsArticle
    {
        public string title;
        public DateTime date;
        public string summary;

        public string URL;

        //the icon is stored as a string here so it can be decoded to an image later
        //See News.java for supported formats
        public string icon;
    }

    public class News
    {
        //public static NewsService service;

        public static bool SupportsNews()
        {
            //return service != null;
            return false;
        }

        //private static Date lastCheck = null;
        //private static final long CHECK_DELAY = 1000 * 60 * 60; //1 hour

        public static void CheckForNews()
        {
            //if (!supportsNews()) return;
            //if (lastCheck != null && (new Date().getTime() - lastCheck.getTime()) < CHECK_DELAY) return;
            //
            //bool useHTTPS = true;
            //if (Gdx.app.getType() == Application.ApplicationType.Android && Gdx.app.getVersion() < 20)
            //{
            //	useHTTPS = false; //android versions below 5.0 don't support TLSv1.2 by default
            //}
            //service.checkForArticles(!SPDSettings.WiFi(), useHTTPS, new NewsService.NewsResultCallback() {
            //	@Override
            //	public void onArticlesFound(ArrayList<NewsArticle> articles)
            //	{
            //		lastCheck = new Date();
            //		News.articles = articles;
            //	}
            //
            //	@Override
            //		public void onConnectionFailed()
            //	{
            //		lastCheck = null;
            //		News.articles = null;
            //	}
            //});
        }

        private static List<NewsArticle> articles = new List<NewsArticle>();

        public static bool ArticlesAvailable()
        {
            return articles != null;
        }

        public static List<NewsArticle> Articles()
        {
            return new List<NewsArticle>(articles);
        }

        public static int UnreadArticles(DateTime lastRead)
        {
            //int unread = 0;
            //foreach (NewsArticle article in articles)
            //{
            //	if (article.date.after(lastRead)) 
            //		unread++;
            //}
            //return unread;
            return 0;
        }

        public static void ClearArticles()
        {
            //articles = null;
            //lastCheck = null;
        }

        public static Image ParseArticleIcon(NewsArticle article)
        {
            //try
            //{
            //
            //	//recognized formats are:
            //	//"ICON: <name of enum constant in Icons.java>"
            //	if (article.icon.startsWith("ICON: "))
            //	{
            //		return Icons.get(Icons.valueOf(article.icon.replace("ICON: ", "")));
            //		//"ITEM: <integer constant corresponding to values in ItemSpriteSheet.java>"
            //	}
            //	else if (article.icon.startsWith("ITEM: "))
            //	{
            //		return new ItemSprite(Integer.parseInt(article.icon.replace("ITEM: ", "")));
            //		//"<asset filename>, <tx left>, <tx top>, <width>, <height>"
            //	}
            //	else
            //	{
            //		string[] split = article.icon.split(", ");
            //		return new Image(split[0],
            //				Integer.parseInt(split[1]),
            //				Integer.parseInt(split[2]),
            //				Integer.parseInt(split[3]),
            //				Integer.parseInt(split[4]));
            //	}
            //
            //	//if we run into any formatting errors (or icon is null), default to the news icon
            //}
            //catch (Exception e)
            //{
            //	if (article.icon != null) ShatteredPixelDungeon.reportException(e);
            //	return Icons.get(Icons.NEWS);
            //}
            return Icons.NEWS.Get();
        }

        public static string ParseArticleDate(NewsArticle article)
        {
            //Calendar cal = Calendar.getInstance();
            //cal.setTime(article.date);
            //return cal.get(Calendar.YEAR)
            //		+ "-" + string.format("%02d", cal.get(Calendar.MONTH) + 1)
            //		+ "-" + string.format("%02d", cal.get(Calendar.DAY_OF_MONTH));
            return "";
        }
    }
}
