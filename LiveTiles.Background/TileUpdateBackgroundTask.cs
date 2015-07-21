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
            var data = await GetTileData();

            // Update the live tile with the feed items.
            ITileViewModel tile = new TileViewModel();
            tile.UpdateTile(data);

            var badge = new BadgeViewModel();
            badge.UpdateBadge(data);

            // Inform the system that the task is finished.
            deferral.Complete();
        }

        private static async Task<ITileDataModel> GetTileData()
        {

            var tileDataModel =  new TileDataModel();
            return await Task.FromResult<ITileDataModel>(tileDataModel);

        }

    }
    public interface ITileDataModel
    {
        string SmallImage { get; }
        string SquareImage { get; }
        string WideImage { get; }
        string Badge { get; }
        string[] Notifications { get; }
    }

    public sealed class TileDataModel: ITileDataModel
    {
         private int _invocationCount;

        public TileDataModel()
        {
            this._invocationCount = DateTime.Now.Second;
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

        public string SmallImage
        {
            get { throw new NotImplementedException(); }
        }

        public string SquareImage
        {
            get { throw new NotImplementedException(); }
        }

        public string WideImage
        {
            get { throw new NotImplementedException(); }
        }
    }

    public interface ITileViewModel
    {

        void UpdateTile(ITileDataModel data);
    }

    public sealed class TileViewModel : ITileViewModel
    {

        public void UpdateTile(ITileDataModel data)
        {            

            string tileXmlString01 = @"
                    <tile>
                        <visual version='3'>
                            <binding template='TileSquare71x71Image'>
                                <image id='1' src='ms-appx:///Assets/Square71x71Logo.png' alt='Gray image'/>
                            </binding>
                            <binding template='TileSquare150x150PeekImageAndText01' fallback='TileSquareImage'>
                                <image id='1' src='ms-appx:///Assets/Square150x150Logo.png' alt='Gray image'/>
                                <text id='1'>S: 111111</text>
                                <text id='2'>S: 222222</text>
                                <text id='3'>S: 333333</text>
                                <text id='4'>S: 444444</text>
                             </binding>
                            <binding template='TileWide310x150PeekImageAndText02' fallback='TileWideImageAndText01'>
                                <image id='1' src='ms-appx:///Assets/WideLogo.scale-100.png' alt='Red image'/>
                                <text id='1'>W: 11111</text>
                                <text id='2'>W: 22222</text>
                                <text id='3'>W: 33333</text>
                                <text id='4'>W: 44444</text>
                            </binding>
                        </visual>
                   </tile>";

            string tileXmlString02 = @"
                    <tile>
                        <visual version='3'>
                            <binding template='TileSquare71x71Image'>
                                <image id='1' src='ms-appx:///Assets/Square71x71Logo.png' alt='Gray image'/>
                            </binding>
                            <binding template='TileSquare150x150Image' fallback='TileSquareImage'>
                                <image id='1' src='ms-appx:///Assets/Square150x150Logo.png' alt='Gray image'/>
                             </binding>
                            <binding template='TileWide310x150ImageAndText01' fallback='TileWideImageAndText01'>
                                <image id='1' src='ms-appx:///Assets/WideLogo.scale-100.png' alt='Red image'/>
                                <text id='1'>This tile notification uses ms-appx images</text>
                            </binding>
                        </visual>
                   </tile>";



            // Create a new tile notification. 
            XmlDocument tileXml = new Windows.Data.Xml.Dom.XmlDocument();
            tileXml.LoadXml(tileXmlString01);

            // Create a tile update manager for the specified syndication feed.
            var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            updater.EnableNotificationQueue(true);
            updater.Clear();
            updater.Update(new TileNotification(tileXml));

        }
    }

    public sealed class BadgeViewModel
    {
        public void UpdateBadge (ITileDataModel data)
        {
            string tileXmlString01 = "<badge version='1' value='" + data.Badge + "'/>";



            // Create a new tile notification. 
            XmlDocument badgeXml = new Windows.Data.Xml.Dom.XmlDocument();
            badgeXml.LoadXml(tileXmlString01);
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(new BadgeNotification(badgeXml));
        }
    }

 
}
