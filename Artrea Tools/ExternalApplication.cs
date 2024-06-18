using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ArtreaTools
{
    class ExternalApplication : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            //Create Ribbon Tab
            application.CreateRibbonTab("Artrea Tools");

            //Create Panels
            //RibbonPanel general = application.CreateRibbonPanel("Artrea Tools", "General");
            RibbonPanel lVPanel = application.CreateRibbonPanel("Artrea Tools", "Low Voltage");

            //Create Buttons
            string path = Assembly.GetExecutingAssembly().Location;

            PushButtonData cableLenghtBtn = new PushButtonData("cableLenghtBtn", "Cable Lenght", path, "ArtreaTools.CableLenght");
            PushButtonData barrayBtn = new PushButtonData("barrayBtn", "Barray", path, "ArtreaTools.Barray");
            //PushButtonData roomBtn = new PushButtonData("roomBtn", "Room", path, "ArtreaTools.Room");
            //PushButtonData viewCropBtn = new PushButtonData("viewCropBtn", "View Crop", path, "ArtreaTools.ViewCrop");

            //Create Image
             Uri cblImagePath = new Uri(@"s:\Users\biro\_Dev\REVIT\Revitapi\Artrea - Tools\Resources\cblicon.png");
             BitmapImage cblImage = new BitmapImage(cblImagePath);

            Uri bArrayImagePath = new Uri(@"s:\Users\biro\_Dev\REVIT\Revitapi\Artrea - Tools\Resources\barrayicon.png");
            BitmapImage bArrayImage = new BitmapImage(bArrayImagePath);

            //Adding Buttons
            //General
            // PushButton roomPushBtn = general.AddItem(roomBtn) as PushButton;
            //PushButton viewCropPushBtn = general.AddItem(viewCropBtn) as PushButton;

            //LV
            PushButton cblPushBtn = lVPanel.AddItem(cableLenghtBtn) as PushButton;
            cblPushBtn.LargeImage = cblImage;

            PushButton barrayPushBtn = lVPanel.AddItem(barrayBtn) as PushButton;
            barrayPushBtn.LargeImage = bArrayImage;

            return Result.Succeeded;

        }
    }
}
