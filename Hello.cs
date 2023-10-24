//==================================================
// 
//  Copyright 2020 Siemens Digital Industries Software
//
//==================================================


using Teamcenter.hello23;
using System;
using Teamcenter.ClientX;
using Teamcenter.Soa.Client.Model;
using User = Teamcenter.Soa.Client.Model.Strong.User;
using System.Collections.Generic;
using Microsoft.Win32;
using OfficeOpenXml;

namespace Teamcenter.Hello
{   
    public class ItemLine
    {
        public string id { get; set; }
        public string name { get; set; }
        public int nivel { get; set; }
        public string aux { get; set; }

        public string padre { get; set; }

        public ItemLine(string id, string name, int nivel, string aux, string padre)

        {
            this.id = id;
            this.name = name;
            this.nivel = nivel;
            this.aux = aux;
            this.padre = padre;
        }

    }

    public class Hello
    {
       

        public static void Main(string[] args)
        {

            String serverHost = "http://Server-PIL:85/tc";
            List<ItemLine> registros = new List<ItemLine>();


            if (args.Length > 0)
            {
                if (args[0].Equals("-help") || args[0].Equals("-h"))
                {
                    System.Console.Out.WriteLine("usage: Hello [-host http://server:port/tc]");
                    return;
                }
                if (args[0].Equals("-host") && args.Length > 1)
                {
                    // Get optional host information
                    serverHost = args[1];
                }
            }



            try
            {

                Session session = new Session(serverHost);
                HomeFolder home = new HomeFolder();
                Query query = new Query();
                DataManagement dm = new DataManagement();





                // Establish a session with the Teamcenter Server
                User user = session.login();

                // Using the User object returned from the login service request
                // display the contents of the Home Folder

                //home.listHomeFolder(user);

                // Perform a simple query of the database
                ModelObject[] lista_objetos = query.queryItems();
                int y = lista_objetos.Length;
                for (int z = 0; z < y; z++)
                {

                    Console.WriteLine("-------*  UID  *-------");
                    string UniqueId = lista_objetos[z].Uid.ToString();
                    Console.WriteLine(UniqueId);
                    ShowBOM showBOM = new ShowBOM(UniqueId);
                    showBOM.loadItem();
                    showBOM.showBOMWindowStructure(registros); // se llena lista supuestamente
                }
                // Perform some basic data management functions
                //dm.createReviseAndDelete();
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Registros");

                    // Encabezados
                    worksheet.Cells["A1"].Value = "ITEM ID";
                    worksheet.Cells["B1"].Value = "NOMBRE";
                    worksheet.Cells["D1"].Value = "NIVEL";
                    worksheet.Cells["C1"].Value = "DESCRIPCION";
                    worksheet.Cells["E1"].Value = "PADRE";

                    // Llenar el archivo Excel con los datos de la lista de objetos
                    int row = 2;
                    foreach (var objeto in registros)
                    {
                        worksheet.Cells["A" + row].Value = objeto.id;
                        worksheet.Cells["B" + row].Value = objeto.name;
                        worksheet.Cells["C" + row].Value = objeto.aux;
                        worksheet.Cells["D" + row].Value = objeto.nivel;
                        worksheet.Cells["E" + row].Value = objeto.padre;
                        row++;
                    }

                    // Guardar el archivo Excel en disco
                    var file = new System.IO.FileInfo("Registros.xlsx");
                    package.SaveAs(file);
                }

                Console.WriteLine("El archivo Excel ha sido creado.");

                // Show BOM
                //ShowBOM showBOM = new ShowBOM("g6AAAEBeZOa2mD");
                //showBOM.loadItem();
                //showBOM.showBOMWindowStructure();
                // Terminate the session with the Teamcenter server

                session.logout();

                Console.WriteLine("-- Enter para cerrar --");
                Console.ReadLine();


            }
            catch (SystemException e)
            {
                Console.WriteLine(e.Message);
            }

        }
    }
}
    
    
