

// THIS .CS FILE CONTAINS CLASSES FOR DROPDOWNS USED IN THIS WEBSITE.


namespace RM_Warehouse.Pages
{
    // THIS CLASS IS FOR DROPDOWNS(PAGE NAMES) IN OUR WEBSITE 
    public class Page_Names_Only
    {
        public string Page_Name { get; set; }
    }
    // THIS CLASS IS FOR DROPDOWN(WAREHOUSE NAMES) FROM OUR DATABASE TABLE
    public class Warehouse_Names_Only
    {
        public string Warehouse { get; set; }
    }
    // THIS CLASS IS FOR DROPDOWN(VENDORS) FROM OUR DATABSE TABLE
    public class Vendor_Names
    {
        public long Vendor_ID { get; set; }
        public string Vendor_Name { get; set; }
    }
    
    // THIS CLASS IS FOR DROPDOWN(LOCATIONS) FROM OUR  DATABSE TABLE
    public class Location_Codes
    {
        public int Location_ID { get; set; }
        public string Location_Code { get; set; }
    }
    // THIS CLASS IS FOR DROPDOWN(ITEMS WITH CODES AND DESCRIPTION) FROM OUR DATABASE TABLE
    public class Item_Codes_Description
    {
        public int Item_ID { get; set; }
        public string Item_Code_Description { get; set;}
    }
    // THIS CLASS IS FOR DROPDOWN(ITEMS WITH CODES,DESCRIPTION AND QUANTITY_IN_HAND) FROM OUR DATABASE TABLE
    public class Item_Codes_Description_Qty_In_Hand
    {
        public int Item_ID { get; set; }
        public string Item_Code_Description_Qty_In_Hand { get; set; }
    }

}
