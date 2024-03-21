using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using BioSero.GreenButtonGo.Scripting;
//using System.ComponentModel.DataAnnotations.Schema;

namespace GreenButtonGo.Scripting
{
    public class Barcoding_Setup_and_Check : BioSero.GreenButtonGo.GBGScript
    {

        public void Run(Dictionary<String, Object> variables, RuntimeInfo runtimeInfo)
        {
            // This script will ensure that the assay parameters input matches what the storage system requirements need to be as defined in the SOW. This script will also set variables within the process to control for where plates end up depending  on the input variables received. 
            // rules and conventions will be written down here as well. 

            // determine if plates that are selected to be barcoded are being barcoded directly from the scheduling process or not. If they are run via the standard barcoding process interface, then make sure to determine if they are source or assay plates being barcoded.

            string userSelection_AssaySource = variables["Barcoding_UsereSelectsSourceAssay"] as string;
            string assayBarcode_Prefix = variables["Barcoding_AssayPrefix"] as string;
            int numberOfPlatesToBarcode = Convert.ToInt32(variables["Barcoding_NumberOfPlatesInSkyline"]);
            bool bln_DoAllPlatesInZone = Convert.ToBoolean(variables["Barcoding_DoAllPlatesInZone"]);
            string[] carouselStorageHotelRows;
            string[] carouselStorageStackRows;
            string[] skyLineStorageRows;
            string GBGLabwareName_corning96 = "Corning 96";
            string GBGLabwareName_corning384 = "Corning 384";
            string GBGLabwareName_corning24 = "Corning 24";
            string GBGLabwareName_Celltreat24 = "CellTreat 24";
            string GBGLabwareName_EchoSourcePlate = "Echo LDV 384";
            string GBGLabwareName_Nunc96 = "Nunc 96";
            string GBGZoneName_blank96Well = "blank 96 well";
            string GBGZoneName_Blank384Well = "blank 384 well";
            string GBGZoneName_Blank24Well = "blank 24 well";
            string GBGZoneName_AEDES = "AEDES";
            string GBGZoneName_DIL4A = "DIL4A";
            string GBGZoneName_EchoLDV = "Echo LDV";
            string GBGZoneName_DIMFM = "DIMFM";
            string GBGZoneName_HCLDA = "HCLDA";
            string GBGZoneName_RSTCA = "RSTCA";
            string GBGZoneName_CFFCA = "CFFCA";
            string GBGZoneName_CFFIA = "CFFIA";

            int numberOf_CellTreat24Plates = 0;
            int numberOf_Corning24Plates = 0;
            int numberOf_Corning384Plates = 0;
            int numberOf_Corning96Plates = 0;
            int numberOf_Nunc96Plates = 0;
            int numberOf_EchoSourcePlates = 0;
            int numberOf_CarouselHotelFreeSpace = 0;
            int numberOf_AEDES_StackFreeSpace = 0;
            int totalAmount_AEDES_Stack = 30;
            int totalAmount_AEDES_CarouselStack = 20;// this is the total amount of plates possible to hold in a single stack of this assay type.This may be better to import from GBG
            int numberOf_DIMFM_StackFreeSpace = 0;
            int totalAmount_DIMFM_Stack = 30;
            int totalAmount_DIMFM_CarouselStack = 20;
            int numberOf_DIL4A_StackFreeSpace = 0;
            int totalAmount_DIL4A_Stack = 30;
            int totalAmount_DIL4A_CarouselStack = 20;
            int numberOf_HCLDA_StackFreeSpace = 0;
            int totalAmount_HCLDA_Stack = 30;
            int totalAmount_HCLDA_CarouselStack = 20;
            int numberOf_RSTCA_StackFreeSpace = 0;
            int totalAmount_RSTCA_Stack = 30;
            int totalAmount_RSTCA_CarouselStack = 20;
            int numberOf_CFFCA_StackFreeSpace = 0;
            int totalAmount_CFFCA_Stack = 30;
            int totalAmount_CFFCA_CarouselStack = 20;
            int numberOf_CFFIA_StackFreeSpace = 0;
            int totalAmount_CFFIA_Stack = 30;
            int totalAmount_CFFIA_CarouselStack = 20;

            // pull in storage tables from carousel and skyline via static method
            PullInAllCarouselStorage(out carouselStorageStackRows, out carouselStorageHotelRows, variables, runtimeInfo);
            pullInAllSkylineStorage(out skyLineStorageRows, variables, runtimeInfo);

            if (userSelection_AssaySource == "Assay Plates")//if you are not doing assay plates, you are just using source plates, which means you just need to look at carousel storage
            {

                //analyze the assay barcode prefix
                //AEDES = Corning 384 Plates, these are coming from skyline
                //DIMFM = Corning 384 Plates, These are coming from skyline
                //DIL4A = Corning 96 plates, these are coming from skyline
                //HCLDA = Corning 96 plates, these are coming from skyline
                //RSTCA = Nunc 96 and 4x 24 well corning (1:4 ratio), the nunc plate is coming carousel but the 24 well plate is coming form skyline
                //CFFCA = Nunc 96 and 4x 24 well cellTreat (1:4 ratio), the nunc plate is coming carousel but the 24 well plate is coming form skyline
                //CFFIA = Nunc 96 and 4x 24 well cellTreat (1:4 ratio), the nunc plate is coming carousel but the 24 well plate is coming form skyline

                foreach (var skylineStorageReadline_row in skyLineStorageRows)
                {
                    if (Convert.ToString(skylineStorageReadline_row[3]) == GBGLabwareName_Celltreat24 && Convert.ToString(skylineStorageReadline_row[2]) == GBGZoneName_Blank24Well && Convert.ToString(skylineStorageReadline_row[4]).ToLower() == "in")//evaluates how plates in the celltreat 24 well stack are present
                    {
                        numberOf_CellTreat24Plates = numberOf_CellTreat24Plates + Convert.ToInt32(skylineStorageReadline_row[1]);// this is typically always going to be convertable from the storage table, but it may not always. You need to use a try and catch method to avoid crashing the script entirely if using mixed data types and trying to convert directly.
                    }

                    if (Convert.ToString(skylineStorageReadline_row[3]) == GBGLabwareName_corning24 && Convert.ToString(skylineStorageReadline_row[2]) == GBGZoneName_Blank24Well && Convert.ToString(skylineStorageReadline_row[4]).ToLower() == "in")//evaluates how plates in the celltreat 24 well stack are present
                    {
                        numberOf_Corning24Plates = numberOf_Corning24Plates + Convert.ToInt32(skylineStorageReadline_row[1]);
                    }

                    if (Convert.ToString(skylineStorageReadline_row[3]) == GBGLabwareName_corning384 && Convert.ToString(skylineStorageReadline_row[2]) == GBGZoneName_Blank384Well && Convert.ToString(skylineStorageReadline_row[4]).ToLower() == "in")//evaluates how plates in the celltreat 24 well stack are present
                    {
                        numberOf_Corning384Plates = numberOf_Corning384Plates + Convert.ToInt32(skylineStorageReadline_row[1]);
                    }

                    if (Convert.ToString(skylineStorageReadline_row[3]) == GBGLabwareName_corning96 && Convert.ToString(skylineStorageReadline_row[2]) == GBGZoneName_blank96Well && Convert.ToString(skylineStorageReadline_row[4]).ToLower() == "in")//evaluates how plates in the celltreat 24 well stack are present
                    {
                        numberOf_Corning96Plates = numberOf_Corning96Plates + Convert.ToInt32(skylineStorageReadline_row[1]);
                    }

                    if (Convert.ToString(skylineStorageReadline_row[3]) == GBGLabwareName_EchoSourcePlate && Convert.ToString(skylineStorageReadline_row[2]) == GBGZoneName_EchoLDV && Convert.ToString(skylineStorageReadline_row[4]).ToLower() == "in")//evaluates how plates in the celltreat 24 well stack are present
                    {
                        numberOf_EchoSourcePlates = numberOf_EchoSourcePlates + Convert.ToInt32(skylineStorageReadline_row[1]);
                    }

                    //need to count available places in the skyline that can receive the assay plates. 
                    if (Convert.ToString(skylineStorageReadline_row[2]) == GBGZoneName_AEDES)//direction of storage will get inverted in the gbg process
                    {
                        //count the amount of plates present in the stack and then subtract from the total plates possible to hold in the stack based on assay
                        numberOf_AEDES_StackFreeSpace = numberOf_AEDES_StackFreeSpace + (totalAmount_AEDES_Stack - Convert.ToInt32(skylineStorageReadline_row[1]));


                    }

                    if (Convert.ToString(skylineStorageReadline_row[2]) == GBGZoneName_DIMFM)//direction of storage will get inverted in the gbg process
                    {
                        //count the amount of plates present in the stack and then subtract from the total plates possible to hold in the stack based on assay
                        numberOf_DIMFM_StackFreeSpace = numberOf_DIMFM_StackFreeSpace + (totalAmount_DIMFM_Stack - Convert.ToInt32(skylineStorageReadline_row[1]));


                    }

                    if (Convert.ToString(skylineStorageReadline_row[2]) == GBGZoneName_DIL4A)//direction of storage will get inverted in the gbg process
                    {
                        //count the amount of plates present in the stack and then subtract from the total plates possible to hold in the stack based on assay
                        numberOf_DIL4A_StackFreeSpace = numberOf_DIL4A_StackFreeSpace + (totalAmount_DIL4A_Stack - Convert.ToInt32(skylineStorageReadline_row[1]));


                    }

                    if (Convert.ToString(skylineStorageReadline_row[2]) == GBGZoneName_HCLDA)//direction of storage will get inverted in the gbg process
                    {
                        //count the amount of plates present in the stack and then subtract from the total plates possible to hold in the stack based on assay
                        numberOf_HCLDA_StackFreeSpace = numberOf_HCLDA_StackFreeSpace + (totalAmount_HCLDA_Stack - Convert.ToInt32(skylineStorageReadline_row[1]));


                    }
                }

                foreach (var carouselstackStorageReadline_row in carouselStorageStackRows)
                {
                    // check the other stacks in the carousel for the same labware that are in the skyline and then also check for nunc plates. We are going to check for nunc plates as well in the hotels.
                    if (Convert.ToString(carouselstackStorageReadline_row[3]) == GBGLabwareName_Nunc96 && Convert.ToString(carouselstackStorageReadline_row[2]) == GBGZoneName_blank96Well && Convert.ToString(carouselstackStorageReadline_row[4]).ToLower() == "in")
                    {
                        numberOf_Nunc96Plates = numberOf_Nunc96Plates + Convert.ToInt32(carouselstackStorageReadline_row[1]);
                    }
                    if (Convert.ToString(carouselstackStorageReadline_row[3]) == GBGLabwareName_Celltreat24 && Convert.ToString(carouselstackStorageReadline_row[2]) == GBGZoneName_Blank24Well && Convert.ToString(carouselstackStorageReadline_row[4]).ToLower() == "in")//evaluates how plates in the celltreat 24 well stack are present
                    {
                        numberOf_CellTreat24Plates = numberOf_CellTreat24Plates + Convert.ToInt32(carouselstackStorageReadline_row[1]);// this is typically always going to be convertable from the storage table, but it may not always. You need to use a try and catch method to avoid crashing the script entirely if using mixed data types and trying to convert directly.
                    }

                    if (Convert.ToString(carouselstackStorageReadline_row[3]) == GBGLabwareName_corning24 && Convert.ToString(carouselstackStorageReadline_row[2]) == GBGZoneName_Blank24Well && Convert.ToString(carouselstackStorageReadline_row[4]).ToLower() == "in")//evaluates how plates in the celltreat 24 well stack are present
                    {
                        numberOf_Corning24Plates = numberOf_Corning24Plates + Convert.ToInt32(carouselstackStorageReadline_row[1]);
                    }

                    if (Convert.ToString(carouselstackStorageReadline_row[3]) == GBGLabwareName_corning384 && Convert.ToString(carouselstackStorageReadline_row[2]) == GBGZoneName_Blank384Well && Convert.ToString(carouselstackStorageReadline_row[4]).ToLower() == "in")//evaluates how plates in the celltreat 24 well stack are present
                    {
                        numberOf_Corning384Plates = numberOf_Corning384Plates + Convert.ToInt32(carouselstackStorageReadline_row[1]);
                    }

                    if (Convert.ToString(carouselstackStorageReadline_row[3]) == GBGLabwareName_corning96 && Convert.ToString(carouselstackStorageReadline_row[2]) == GBGZoneName_blank96Well && Convert.ToString(carouselstackStorageReadline_row[4]).ToLower() == "in")//evaluates how plates in the celltreat 24 well stack are present
                    {
                        numberOf_Corning96Plates = numberOf_Corning96Plates + Convert.ToInt32(carouselstackStorageReadline_row[1]);
                    }

                    if (Convert.ToString(carouselstackStorageReadline_row[3]) == GBGLabwareName_EchoSourcePlate && Convert.ToString(carouselstackStorageReadline_row[2]) == GBGZoneName_EchoLDV && Convert.ToString(carouselstackStorageReadline_row[4]).ToLower() == "in")//evaluates how plates in the celltreat 24 well stack are present
                    {
                        numberOf_EchoSourcePlates = numberOf_EchoSourcePlates + Convert.ToInt32(carouselstackStorageReadline_row[1]);
                    }

                    // going to look for any free spaces for carousel stacks here

                    if (Convert.ToString(carouselstackStorageReadline_row[2]) == GBGZoneName_AEDES)//direction of storage will get inverted in the gbg process
                    {
                        //count the amount of plates present in the stack and then subtract from the total plates possible to hold in the stack based on assay
                        numberOf_AEDES_StackFreeSpace = numberOf_AEDES_StackFreeSpace + (totalAmount_AEDES_CarouselStack - Convert.ToInt32(carouselstackStorageReadline_row[1]));


                    }

                    if (Convert.ToString(carouselstackStorageReadline_row[2]) == GBGZoneName_DIMFM)//direction of storage will get inverted in the gbg process
                    {
                        //count the amount of plates present in the stack and then subtract from the total plates possible to hold in the stack based on assay
                        numberOf_DIMFM_StackFreeSpace = numberOf_DIMFM_StackFreeSpace + (totalAmount_DIMFM_CarouselStack - Convert.ToInt32(carouselstackStorageReadline_row[1]));


                    }

                    if (Convert.ToString(carouselstackStorageReadline_row[2]) == GBGZoneName_DIL4A)//direction of storage will get inverted in the gbg process
                    {
                        //count the amount of plates present in the stack and then subtract from the total plates possible to hold in the stack based on assay
                        numberOf_DIL4A_StackFreeSpace = numberOf_DIL4A_StackFreeSpace + (totalAmount_DIL4A_CarouselStack - Convert.ToInt32(carouselstackStorageReadline_row[1]));


                    }

                    if (Convert.ToString(carouselstackStorageReadline_row[2]) == GBGZoneName_HCLDA)//direction of storage will get inverted in the gbg process
                    {
                        //count the amount of plates present in the stack and then subtract from the total plates possible to hold in the stack based on assay
                        numberOf_HCLDA_StackFreeSpace = numberOf_HCLDA_StackFreeSpace + (totalAmount_HCLDA_CarouselStack - Convert.ToInt32(carouselstackStorageReadline_row[1]));


                    }

                    if (Convert.ToString(carouselstackStorageReadline_row[2]) == GBGZoneName_RSTCA)//direction of storage will get inverted in the gbg process
                    {
                        //count the amount of plates present in the stack and then subtract from the total plates possible to hold in the stack based on assay
                        numberOf_RSTCA_StackFreeSpace = numberOf_RSTCA_StackFreeSpace + (totalAmount_RSTCA_CarouselStack - Convert.ToInt32(carouselstackStorageReadline_row[1]));


                    }
                    if (Convert.ToString(carouselstackStorageReadline_row[2]) == GBGZoneName_CFFCA)//direction of storage will get inverted in the gbg process
                    {
                        //count the amount of plates present in the stack and then subtract from the total plates possible to hold in the stack based on assay
                        numberOf_CFFCA_StackFreeSpace = numberOf_CFFCA_StackFreeSpace + (totalAmount_CFFCA_CarouselStack - Convert.ToInt32(carouselstackStorageReadline_row[1]));


                    }
                    if (Convert.ToString(carouselstackStorageReadline_row[2]) == GBGZoneName_CFFIA)//direction of storage will get inverted in the gbg process
                    {
                        //count the amount of plates present in the stack and then subtract from the total plates possible to hold in the stack based on assay
                        numberOf_CFFCA_StackFreeSpace = numberOf_CFFIA_StackFreeSpace + (totalAmount_CFFIA_CarouselStack - Convert.ToInt32(carouselstackStorageReadline_row[1]));


                    }
                }

                foreach (var carouselStorageReadline_row in carouselStorageHotelRows)
                {
                    if (Convert.ToString(carouselStorageReadline_row[2]) == GBGLabwareName_Nunc96 && Convert.ToBoolean(carouselStorageReadline_row[3]) == false)
                    {
                        numberOf_Nunc96Plates++;//count up if the criteria is met that a nunc labware type is in storage and has not been run yet. 
                    }

                    //need to count the amount of free spaces in the hotel storage here as well
                }

                // all storage rows have been captured into string arrays made of arrays. 
                // 
                // make sure there are more than 0 plates in the correct zone based on the input. If the GBG variable requires a specific amount of plates from that zone, make sure that number match what was found
                if (bln_DoAllPlatesInZone == true)
                {
                    // set the global counter to the amount of plates present in the zone 
                }
                else
                {
                    // otherwise, make sure that the amount of plates present in the zone matches for the input and output. Othersie, just end and return a false.
                }

                // analyze where the 
            }

            else
            {
                // we are just analyzing the Echo LDV plate storage if its just source plates. There has to be enough room in the carousel to match the amount of plates being created for barcoding
            }

        }

        static void PullInAllCarouselStorage(out string[] CarouselStackStorageRows, out string[] CarouselHotelStorageRows, Dictionary<String, Object> variables, RuntimeInfo runtimeInfo)
        {
            //this section pulls in the storage tables for the carousel
            var carouselstackStorageTableName = "Carousel_Stacks";
            var carouselHotelStorageTableName = "Carousel_Hotels";
            var carousel_Stackheader = new string[] { "Stack", "Plates_In_Stack", "Zone", "Labware", "Direction" };
            var carousel_Hotelheader = new string[] { "Barcode", "Zone", "Labware", "Has_Been_Run" };
            CarouselStackStorageRows = Database.GetAllRows(carouselstackStorageTableName, carousel_Stackheader);
            CarouselHotelStorageRows = Database.GetAllRows(carouselHotelStorageTableName, carousel_Hotelheader);
            return;
        }

        static void pullInAllSkylineStorage(out string[] skylineStorageRows, Dictionary<String, Object> variables, RuntimeInfo runtimeInfo)
        {
            //This section just pulls in storage table information for the skyline
            var skylineStorageTableName = "Cytomat_Skyline_Stacks";
            var skylineHeaders = new string[] { "Stack", "Plates_In_Stack", "Zone", "Labware", "Direction" };
            skylineStorageRows = Database.GetAllRows(skylineStorageTableName, skylineHeaders);
            return;
        }

    }

}