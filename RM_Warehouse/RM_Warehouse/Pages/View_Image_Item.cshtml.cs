using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RM_Warehouse.Pages
{
    // THIS CLASS IS FOR VIEWING IMAGE/PDF FILES IN NEW BROWSER TAB.
    public class View_Image_ItemModel : PageModel
    {

        public readonly IConfiguration _configuration;
        [BindProperty]
        public string file_name { get; set; }
        [BindProperty]
        public string extension { get; set; }

        public View_Image_ItemModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // THIS FUNCTION CONCATES VIRTUAL DIRECTORY WITH FILENAME.
        // HERE VIRTUAL DIRECTORY NAME IS item_pics.FILENAME filename IS PASSED TO THIS FUNCTION.

        public void OnGet(string filename)
        {
            file_name = "\\item_pics\\" + filename.Replace("%20", " ");
            extension = Path.GetExtension(filename);
            
        }
    }
}

