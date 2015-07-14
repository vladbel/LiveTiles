using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

// Added during quickstart
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.Web.Syndication;

namespace LiveTiles.Background
{
    /// <summary>
    /// https://msdn.microsoft.com/en-us/library/windows/apps/xaml/jj991805.aspx
    /// </summary>
    public sealed class TileUpdateBackgroundTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            // Get a deferral, to prevent the task from closing prematurely 
            // while asynchronous code is still running.
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            //get tile view model
            var viewModel = await GetTileViewModel();

            // Update the live tile with the feed items.
            viewModel.UpdateTile();

            // Inform the system that the task is finished.
            deferral.Complete();
        }

        private static async Task<ITileViewModel> GetTileViewModel()
        {

            return new TileSquareTextViewModel();

            //return new TileWideImageAndTextViewModel();

        }

    }

    public interface ITileViewModel
    {
        string BackgroundImage { get; }
        string Badge { get; }
        string[] Notifications { get; }

        void UpdateTile();
    }

    public sealed class TileSquareTextViewModel : ITileViewModel
    {
        private int _invocationCount;

        public TileSquareTextViewModel()
        {
            this._invocationCount = DateTime.Now.Second;
        }

        public string BackgroundImage
        {
            get
            {
                return "";
            }
        }

        public string Badge
        {
            get
            {
                return (++_invocationCount).ToString();
            }
        }

        public string[] Notifications
        {
            get
            {
                string[] result = { String.Format("First: {0} ", (++_invocationCount).ToString()), 
                                    String.Format("Mid: {0} ", (++_invocationCount).ToString()), 
                                    String.Format("End: {0} ", (++_invocationCount).ToString()) };
                return result;
            }
        }

        public void UpdateTile()
        {
            // Create a tile update manager for the specified syndication feed.
            var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            updater.EnableNotificationQueue(true);
            updater.Clear();

            // Keep track of the number feed items that get tile notifications. 
            int itemCount = 0;

            // Create a tile notification for each feed item.
            var notifications = this.Notifications;
            foreach (var item in notifications)
            {

                // XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150Block); // works

                XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150Text01); //  works

                //XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare310x310ImageAndText01); // NOT

                //XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150ImageAndText02);

                tileXml.GetElementsByTagName("text")[0].InnerText = item;

                // Create a new tile notification. 
                updater.Update(new TileNotification(tileXml));

                // Don't create more than 5 notifications.
                if (itemCount++ > 5) break;
            }
        }
    }

    public sealed class TileWideImageAndTextViewModel : ITileViewModel
    {
        private int _invocationCount;

        public TileWideImageAndTextViewModel()
        {
            this._invocationCount = DateTime.Now.Second;
        }

        public string BackgroundImage
        {
            get
            {
                return "Assets\\WideLogo.scale-100.png";
            }
        }

        public string Badge
        {
            get
            {
                return (++_invocationCount).ToString();
            }
        }

        public string[] Notifications
        {
            get
            {
                string[] result = { String.Format("First: {0} ", (++_invocationCount).ToString()), 
                                    String.Format("Mid: {0} ", (++_invocationCount).ToString()), 
                                    String.Format("End: {0} ", (++_invocationCount).ToString()) };
                return result;
            }
        }

        public void UpdateTile()
        {
            // Create a tile update manager for the specified syndication feed.
            var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            updater.EnableNotificationQueue(true);
            updater.Clear();

            // Keep track of the number feed items that get tile notifications. 
            int itemCount = 0;

            // Create a tile notification for each feed item.
            var notifications = this.Notifications;


            XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150ImageAndText01);
            tileXml.GetElementsByTagName("text")[0].InnerText = this.Notifications[0];

            var tileImage = tileXml.GetElementsByTagName("image")[0] as XmlElement;
            tileImage.SetAttribute("src", "ms-appx:///Assets/WideLogo.scale-100.png");

            // Create a new tile notification. 
            updater.Update(new TileNotification(tileXml));



        }
    }
}
