using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
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
    public class ViewCrop : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UI Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //Get Document
            Document doc = uidoc.Document;

            try
            {
               // IEnumerable<ViewSheet> sheets = new FilteredElementCollector(doc).OfClass(typeof(ViewSheet)).Cast<ViewSheet>();
                
                ICollection<ElementId> viewsIds = null;
                foreach (ViewSheet sheet in new FilteredElementCollector(doc).OfClass(typeof(ViewSheet)).Cast<ViewSheet>())
                {
                    ElementId viewId = sheet.GetAllPlacedViews() as ElementId;
                    viewsIds.Add(viewId);
                }

                //TaskDialog.Show("asdf", viewsIds.);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }
    }
}
