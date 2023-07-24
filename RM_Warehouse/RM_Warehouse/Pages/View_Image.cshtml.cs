using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RM_Warehouse.Pages
{
    // THIS CLASS IS FOR VIEWING PDF FILES IN NEW BROWSER TAB
    public class View_ImageModel : PageModel
    {

        public readonly IConfiguration _configuration;
        [BindProperty]
        public string file_name { get; set; }
        [BindProperty]
        public string extension { get; set; }

        public View_ImageModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // THIS FUNCTION CONCATES VIRTUAL DIRECTORY WITH FILENAME.
        // HERE VIRTUAL DIRECTORY NAME IS pdfs.FILENAME filename IS PASSED TO THIS FUNCTION.

        public void OnGet(string filename)
        {
            file_name = "\\pdfs\\"+filename.Replace("%20"," ");
            extension=Path.GetExtension(filename);
           
        }
    }
}
