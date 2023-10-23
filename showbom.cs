using System;
using System.Collections.Generic;
using System.Text;
using Teamcenter.Services.Strong.Core;
using Teamcenter.Soa.Client.Model.Strong;
using Teamcenter.Soa.Common;
using Teamcenter.Soa.Client.Model;
using Teamcenter.Soa.Exceptions;
using Teamcenter.Services.Strong.Structuremanagement._2008_06.Structure;
using System.Linq;
using Teamcenter.Hello;


namespace Teamcenter.hello23
{

    class VectorDatos
    {
        public List<string> Vector = new List<string>();
        public void printVector()
        {
            Console.WriteLine("Vector: ");
            for (int k = 0; k < Vector.Count; k++)
            {
                Console.WriteLine(Vector[k]);
            }
        }
    }

    

    class ShowBOM
    {
        private String ItemUID; /*** user supplied Item UID with BOM structure ***/

        private Teamcenter.Services.Strong.Core.DataManagementService dmService;
        private Teamcenter.Services.Strong.Cad.StructureManagementService structureService;

        private Item item;

        

        public ShowBOM(string itemUID)
        {   
            this.ItemUID = itemUID;
            dmService = Teamcenter.Services.Strong.Core.DataManagementService.getService(Teamcenter.ClientX.Session.getConnection());
            structureService = Teamcenter.Services.Strong.Cad.StructureManagementService.getService(Teamcenter.ClientX.Session.getConnection());

            setObjectPolicy();
        }

        public bool loadItem()
        {
            ServiceData sData = dmService.LoadObjects(new string[] { ItemUID });

            if (sData.sizeOfPlainObjects() > 0)
            {
                ModelObject mObj = sData.GetPlainObject(0);

                if (mObj is Item)
                {
                    item = mObj as Item;
                    return true;
                }
            }

            return false;
        }
        /** public void printVector()
        {
            Console.WriteLine("Vector: ");
            for (int k = 0; k < vector.Count; k++)
            {              
                Console.WriteLine(vector[k]);
            }
        } **/
        public bool showBOMWindowStructure(List<ItemLine> registros)
        {
            try
            {
                ModelObject[] itemRevs = item.Revision_list;
                ModelObject[] bomViews = item.Bom_view_tags;

                foreach (ModelObject bomView in bomViews)
                {
                    foreach (ModelObject itemRev in itemRevs)
                    {
                        Teamcenter.Services.Strong.Cad._2007_01.StructureManagement.CreateBOMWindowsInfo bomWinInfo = new Teamcenter.Services.Strong.Cad._2007_01.StructureManagement.CreateBOMWindowsInfo();

                        bomWinInfo.Item = item;
                        bomWinInfo.ItemRev = (ItemRevision)itemRev;
                        bomWinInfo.BomView = (PSBOMView)bomView;

                        Teamcenter.Services.Strong.Cad._2007_01.StructureManagement.CreateBOMWindowsResponse bomResp = structureService.CreateBOMWindows(new Teamcenter.Services.Strong.Cad._2007_01.StructureManagement.CreateBOMWindowsInfo[] { bomWinInfo });

                        if (bomResp.Output.Length > 0)
                        {
                            BOMLine bomLine = bomResp.Output[0].BomLine;
                            

                            expandBOMLines(bomLine, registros);
                            //expandBOMAllLines(bomLine);
                            
                        }
                    }
                }

                return true;
            }
            catch (NotLoadedException e)
            {
                Console.WriteLine(e.StackTrace);
            }

            return false;
        }

        private void expandBOMAllLines(BOMLine bomLine)
        {
            Teamcenter.Services.Strong.Cad._2007_01.StructureManagement.ExpandPSAllLevelsInfo info = new Teamcenter.Services.Strong.Cad._2007_01.StructureManagement.ExpandPSAllLevelsInfo();
            Teamcenter.Services.Strong.Cad._2007_01.StructureManagement.ExpandPSAllLevelsPref pref = new Teamcenter.Services.Strong.Cad._2007_01.StructureManagement.ExpandPSAllLevelsPref();

            info.ParentBomLines = new BOMLine[] { bomLine };
            info.ExcludeFilter = "None";

            pref.ExpItemRev = false;
            pref.Info = new Teamcenter.Services.Strong.Cad._2007_01.StructureManagement.RelationAndTypesFilter[0];

            Teamcenter.Services.Strong.Cad._2007_01.StructureManagement.ExpandPSAllLevelsResponse resp = structureService.ExpandPSAllLevels(info, pref);

            foreach (Teamcenter.Services.Strong.Cad._2007_01.StructureManagement.ExpandPSAllLevelsOutput data in resp.Output)
            {
                Console.WriteLine("Parent: " + data.Parent.BomLine.Bl_line_name); // + " -N " + data.Parent.BomLine.Bl_level_starting_0);

                if (data.Children.Length > 0)
                {
                    foreach (Teamcenter.Services.Strong.Cad._2007_01.StructureManagement.ExpandPSData child in data.Children)

                        Console.WriteLine("\tChild: " + child.BomLine.Bl_line_name); //+ " -N " + child.BomLine.Bl_level_starting_0);
                }
                else
                    Console.WriteLine("\tChildren: none");
            }
        }
        private void expandBOMLines(BOMLine bomLine, List<ItemLine> registros)
        {

            

            Teamcenter.Services.Strong.Cad._2007_01.StructureManagement.ExpandPSOneLevelInfo levelInfo = new Teamcenter.Services.Strong.Cad._2007_01.StructureManagement.ExpandPSOneLevelInfo();
            Teamcenter.Services.Strong.Cad._2007_01.StructureManagement.ExpandPSOneLevelPref levelPref = new Teamcenter.Services.Strong.Cad._2007_01.StructureManagement.ExpandPSOneLevelPref();

            levelInfo.ParentBomLines = new BOMLine[] { bomLine };
            levelInfo.ExcludeFilter = "None";

            levelPref.ExpItemRev = false;
            levelPref.Info = new Teamcenter.Services.Strong.Cad._2007_01.StructureManagement.RelationAndTypesFilter[0];

            Teamcenter.Services.Strong.Cad._2007_01.StructureManagement.ExpandPSOneLevelResponse levelResp = structureService.ExpandPSOneLevel(levelInfo, levelPref);
            
            //int contador = 0;
            try
            {
                ItemLine objeto = new ItemLine(bomLine.Bl_item_item_id, bomLine.Bl_line_name, bomLine.Bl_level_starting_0, bomLine.Bl_rev_object_desc);
                registros.Add(objeto);
                //Vector.Add("BOMLine: " + bomLine.Bl_line_name);
                //Vector.Add(" -N: " + bomLine.Bl_level_starting_0.ToString());
                
                
                //contador++;
                //Console.WriteLine("BOMLine: " + bomLine.Bl_line_name);
            }
            catch (NotLoadedException e)
            {
                Console.WriteLine("--* ERROR " + e.StackTrace);
            }

            if (levelResp.Output.Length > 0)
            {
                foreach (Teamcenter.Services.Strong.Cad._2007_01.StructureManagement.ExpandPSOneLevelOutput levelOut in levelResp.Output)
                {
                    foreach (Teamcenter.Services.Strong.Cad._2007_01.StructureManagement.ExpandPSData psData in levelOut.Children)
                    {
                        expandBOMLines(psData.BomLine, registros);
                    }
                }
            }
        }

        protected void setObjectPolicy()
        {
            SessionService session = SessionService.getService(Teamcenter.ClientX.Session.getConnection());
            ObjectPropertyPolicy policy = new ObjectPropertyPolicy();

            policy.AddType(new PolicyType("Item", new string[] { "bom_view_tags", "revision_list" }));
            //ACA HAY QUE AGREGAR LOS DATOS QUE QUEREMOS RECUPERAR DEL ITEM, PARA GUARDAR EN LA CLASE BOMLine
            policy.AddType(new PolicyType("BOMLine", new string[] { "bl_line_name", "bl_level_starting_0", "bl_item_item_id", "bl_rev_object_desc" }));

            session.SetObjectPropertyPolicy(policy);
        }
    }
}