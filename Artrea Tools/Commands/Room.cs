using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ArtreaTools
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Room : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UI Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //Get Document
            Document doc = uidoc.Document;

            //Collecting linked modells
            Element linkElem = new FilteredElementCollector(doc).OfClass(typeof(RevitLinkInstance)).ToElements()[0];
            RevitLinkInstance linked_instance = linkElem as RevitLinkInstance;
            Document linked_doc = linked_instance.GetLinkDocument();




            //Collecting Room
            RoomFilter filter = new RoomFilter();
            List<Element> roomCollect = new FilteredElementCollector(linked_doc).WherePasses(filter).ToElements() as List<Element>;

            string roomName = null;
            foreach (Element room in roomCollect)
            {
                roomName = room.Name;
            }
            
            


            TaskDialog.Show("Room", roomCollect.ToString() );


            
            

            

                
            

            return Result.Succeeded;
        }
    }
}
