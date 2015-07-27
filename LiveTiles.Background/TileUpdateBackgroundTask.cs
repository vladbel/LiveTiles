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
            //IApplicationTilesViewModel tile = new ApplicationTilesViewModel( new Tile71x71() ,new Tile150x150(), new Tile310x150());
            IApplicationTilesViewModel tile = new ApplicationTilesViewModel(new Tile71x71(data), 
                                                                            new TileSquare150x150PeekImageAndText01(data),
                                                                            new TileWide310x150PeekImageAndText02(data));
            tile.UpdateTiles(data);

            var badge = new BadgeViewModel();
            badge.UpdateBadge(data);

            // Inform the system that the task is finished.
            deferral.Complete();
        }

        private static async Task<ITileDataModel> GetTileData()
        {

            ITileDataModel tileDataModel = new TileDataModel
            {
                SmallImage = "ms-appx:///Assets/AppIcons/LiveTiles/Square71x71Logo.scale-100.png",
                SquareImage = "ms-appx:///Assets/AppIcons/LiveTiles/Square150x150Logo.scale-100.png",
                WideImage = "ms-appx:///Assets/AppIcons/LiveTiles/Wide310x150Logo.scale-100.png",
                Badge = "6",
                Notifications = new string[] { "1.1: foo", "2.2: bar" }
            };
            return await Task.FromResult<ITileDataModel>(tileDataModel);

        }

    }
    public interface ITileDataModel
    {
        string SmallImage { get; set; }
        string SquareImage { get; set; }
        string WideImage { get; set; }
        string Badge { get; set; }
        string[] Notifications { get; set; }
    }

    public sealed class TileDataModel : ITileDataModel
    {

        public string SmallImage { get; set; }
        public string SquareImage { get; set; }
        public string WideImage { get; set; }
        public string Badge { get; set; }
        public string[] Notifications { get; set; }
    }

    public interface IApplicationTilesViewModel
    {
        void UpdateTiles(ITileDataModel data);
    }

    public sealed class ApplicationTilesViewModel : IApplicationTilesViewModel
    {
        private const string  TILE_XML = "<tile><visual version='3'>{0}{1}{2}</visual></tile>";

        private ITileBindingTemplate _small71x71;
        private ITileBindingTemplate _square150x150;
        private ITileBindingTemplate _wide310x150;

        public ApplicationTilesViewModel (ITileBindingTemplate small71x71, ITileBindingTemplate square150x150, ITileBindingTemplate wide310x150)
        {
            this._small71x71 = small71x71;
            this._square150x150 = square150x150;
            this._wide310x150 = wide310x150;
        }

        /// <summary>
        ///  Expected tile XML:
        ///              <tile>
        ///                    <visual version='3'>
        ///                        <binding template='TileSquare71x71Image'>
        ///                            <image id='1' src='ms-appx:///Assets/Square71x71Logo.png' alt='Gray image'/>
        ///                        </binding>
        ///                        <binding template='TileSquare150x150PeekImageAndText01' fallback='TileSquareImage'>
        ///                            <image id='1' src='ms-appx:///Assets/Square150x150Logo.png' alt='Gray image'/>
        ///                            <text id='1'>Tile text line 1</text>
        ///                            <text id='2'>Tile text line 2</text>
        ///                            <text id='3'>Tile text line 3</text>
        ///                            <text id='4'>Tile text line 4</text>
        ///                         </binding>
        ///                        <binding template='TileWide310x150PeekImageAndText02' fallback='TileWideImageAndText01'>
        ///                            <image id='1' src='ms-appx:///Assets/WideLogo.scale-100.png' alt='Red image'/>
        ///                            <text id='1'>Tile text line 1</text>
        ///                            <text id='2'>Tile text line 2</text>
        ///                            <text id='3'>Tile text line 3</text>
        ///                            <text id='4'>Tile text line 4</text>
        ///                        </binding>
        ///                    </visual>
        ///               </tile>
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private string GetTileXml(ITileDataModel data)
        {

            return String.Format(TILE_XML, 
                                this._small71x71.XmlString,
                                this._square150x150.XmlString, 
                                this._wide310x150.XmlString);
        }

        public void UpdateTiles(ITileDataModel data)
        {
            // Create a new tile notification. 
            XmlDocument tileXml = new Windows.Data.Xml.Dom.XmlDocument();
            tileXml.LoadXml(this.GetTileXml(data));

            // Create a tile update manager for the specified syndication feed.
            var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            updater.EnableNotificationQueue(true);
            updater.Clear();
            updater.Update(new TileNotification(tileXml));
        }
    }

    public interface IBadgeViewModel
    {
        void UpdateBadge(ITileDataModel data);
    }

    public sealed class BadgeViewModel
    {
        public void UpdateBadge(ITileDataModel data)
        {
            int badgeValue = 0;
            if ( data.Badge == null|| 
                !int.TryParse(data.Badge, out badgeValue))
            {
                return;
            }
            string tileXmlString01 = "<badge version='1' value='" + badgeValue.ToString() + "'/>";


            // Create a new tile notification. 
            XmlDocument badgeXml = new Windows.Data.Xml.Dom.XmlDocument();
            badgeXml.LoadXml(tileXmlString01);
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(new BadgeNotification(badgeXml));
        }
    }

    public interface ITileBindingTemplate
    {
        XmlElement XmlElement {get;}
        string XmlString { get; }
    }

    public sealed class Tile71x71 : ITileBindingTemplate
    {
        private string _bindingTemplateXml = "<binding template='TileSquare71x71Image'><image id='1' src='{0}' alt='Gray image'/></binding>";

        /// <summary>
        ///     <binding template='TileSquare71x71Image'>
        ///         <image id='1' src='ms-appx:///Assets/Square71x71Logo.png' alt='Gray image'/>
        ///     </binding>
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public  Tile71x71(ITileDataModel data)
        {
            _bindingTemplateXml = String.Format(_bindingTemplateXml, data.SmallImage);
        }

        public XmlElement XmlElement
        {
            get { throw new NotImplementedException(); }
        }

        public string XmlString
        {
            get { return this._bindingTemplateXml; }
        }
    }

    public sealed class TileSquare150x150PeekImageAndText01 : ITileBindingTemplate
    {
        private ITileBindingTemplate _builder;

        public TileSquare150x150PeekImageAndText01(ITileDataModel data)
        {
            this._builder = new TileBindingTemplateBuilder(TileTemplateType.TileSquare150x150PeekImageAndText01, 
                                                            data.Notifications, 
                                                            data.SquareImage);
        }

        public string GetBindingTemplateXml(ITileDataModel data)
        {
            throw new NotImplementedException();
        }


        public XmlElement XmlElement
        {
            get { return _builder.XmlElement;  }
        }

        public string XmlString
        {
            get { return _builder.XmlElement.GetXml(); }
        }
    }

    public sealed class TileWide310x150PeekImageAndText02 : ITileBindingTemplate
    {
        private ITileBindingTemplate _builder;

        public TileWide310x150PeekImageAndText02(ITileDataModel data)
        {
            this._builder = new TileBindingTemplateBuilder(TileTemplateType.TileWide310x150PeekImageAndText02, 
                                                            data.Notifications, 
                                                            data.WideImage);
        }

        public string GetBindingTemplateXml(ITileDataModel data)
        {
            throw new NotImplementedException();
        }


        public XmlElement XmlElement
        {
            get { return _builder.XmlElement;  }
        }

        public string XmlString
        {
            get { return _builder.XmlElement.GetXml(); }
        }
    }

    public sealed class TileBindingTemplateBuilder: ITileBindingTemplate
    {
        private XmlElement _bindingXmlElement;

        
        public TileBindingTemplateBuilder(TileTemplateType tileTemplateteType,
                                            [System.Runtime.InteropServices.WindowsRuntime.ReadOnlyArray()] string[] notifications, 
                                            string imageSource)
        {
            var tileXmlDocument = TileUpdateManager.GetTemplateContent(tileTemplateteType);

            var bindingXmlElements = tileXmlDocument.GetElementsByTagName("binding");

            
            if (bindingXmlElements == null || bindingXmlElements.Count != 1)
            {
                throw new Exception("Invalid binding template");
            }

            var bindingXmlElement = (XmlElement)bindingXmlElements[0];

            this.SetImage(bindingXmlElement, imageSource);
            this.SetText(bindingXmlElement, notifications);

            System.Diagnostics.Debug.WriteLine(bindingXmlElement.GetXml());
            this._bindingXmlElement = bindingXmlElement;
        }

        private void SetImage(XmlElement binding, string imageSource)
        {
            var imageElements = binding.GetElementsByTagName("image");
            var imageSourceAttribute = imageElements[0].Attributes.GetNamedItem("src");
            imageSourceAttribute.NodeValue = imageSource;
        }

        private void SetText (XmlElement binding, string[] notifications)
        {
            var textElements = binding.GetElementsByTagName("text");

            if (textElements == null || textElements.Count == 0)
            {
                return;
            }

            for (var i = 0; i < Math.Min(4, notifications.Length); i++)
            {
                textElements[i].InnerText = notifications[i];
            }
        }


        public string GetBindingTemplateXml(ITileDataModel data)
        {
            throw new NotImplementedException();
        }

        public XmlElement XmlElement
        {
            get { return this._bindingXmlElement; }
        }

        public string XmlString
        {
            get { return this._bindingXmlElement.GetXml(); }
        }
    }

}
