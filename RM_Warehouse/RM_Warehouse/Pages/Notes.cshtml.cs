using DAL.CRUD;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Reflection.Emit;

namespace RM_Warehouse.Pages
{
    // THIS CLASS IS FOR NOTES PAGE
    public class NotesModel : PageModel
    {
        [BindProperty]
        public static string Note_Description_Complete { get; set; }
        [BindProperty]
        public static int Note_ID_Complete { get; set; }
        public string Msg_Complete { get; set; }
        [BindProperty]
        public string Complete_Comments { get; set; }
        [BindProperty]
        public bool flag_complete { get; set; }
        [BindProperty]
        public static int Note_ID { get; set; }
        public string Msg { get; set; }
        [BindProperty]
        public bool flag_notes { get; set; }
        [BindProperty]
        public bool flag_notes_form { get; set; }

        [BindProperty]
        public static DataTable DT { get; set; }

        [BindProperty]
        public string Note_Description { get; set; }


        public IActionResult OnGet()
        {

            bool flag_username = string.IsNullOrEmpty(HttpContext.Session.GetString("username"));

            if (flag_username)
            {
                return RedirectToPage("Index");
            }

            Notes notes = new Notes();
            DT = notes.GetNotes();
            
            flag_notes = true;
       
            return Page();
        }

        // THIS FUNCTION DOES SUBMIT FOR NEW NOTE OR UPDATE NOTE.

        public IActionResult OnPostSubmit()
        {
            if (!Check_Input())
            {
                flag_notes_form = true;
                return Page();
            }
            Notes notes=new Notes();

            string note_by = HttpContext.Session.GetString("username");

            if (Note_ID==0)
            {
                notes.AddNote(Note_Description, note_by);
            }           
            else
            {
                notes.UpdateNote(Note_ID,Note_Description);
            }
            // reseting the form

            Reset_Form();
            
            DT = notes.GetNotes();

            flag_notes = true;

            return Page();
        }

        // THIS FUNCTION DOES CANCEL FOR NOTES FORM. 

        public IActionResult OnPostCancel()
        {
            Reset_Form();
            flag_notes_form = false;
            flag_notes = true;
            return Page();
        }

        // THIS FUNCTION DOES RESET FOR NOTES FORM.

        public void Reset_Form()
        {
            Note_Description = null;
            Note_ID=0;
            ModelState.Clear();
        }

        // THIS FUNCTION IS CALLED FORM NOTES GRID'S ROW EDIT BUTTON.IT OPENS NOTES FORM WITH VALUES.

        public IActionResult OnPostEdit(int id,string note_desc_1)
        {
            Note_Description=note_desc_1;
            Note_ID= id;
            flag_notes_form = true;
            return Page();
        }

        // THIS FUNCTION CHECKS USER INPUT FOR NOTES FORM.

        public bool Check_Input()
        {
            if (Note_Description == null)
            {
                Msg = "Please enter Note Description.";
                return false;
            }
            
            return true;
        }

        // THIS FUNCTION IS CALLED FORM NEW NOTE BUTTON.IT OPENS NOTES FORM WITH RESET FIELDS.

        public IActionResult OnPostAdd_Note()
        {
            Reset_Form();
            flag_notes_form = true;
            flag_notes = false;
            return Page();
        }

        // THIS FUNCTION DOES SUBMIT FOR NOTES COMPLETE FORM.

        public IActionResult OnPostSubmitComments()
        {
            if (!Check_Input_Comments())
            {
                flag_complete = true;
                return Page();
            }
            Notes notes = new Notes();

            string complete_by = HttpContext.Session.GetString("username");

            notes.CompleteNote(Note_ID_Complete,Complete_Comments,complete_by);

            TempData["ConfirmationMessage"] = "Note ID:" + Note_ID_Complete + " is Completed";

            Reset_Complete_Form();

            DT = notes.GetNotes();

            

            flag_notes = true;

            return Page();
        }

        // THIS FUNCTION DOES CANCEL FOR NOTES COMPLETE FORM.

        public IActionResult OnPostCancelComments()
        {
            Reset_Complete_Form();
            flag_complete = false;
            flag_notes = true;
            return Page();
        }

        // THIS FUNCTION CHECKS USER INPUTS FOR NOTES COMPLETE FORM.

        public bool Check_Input_Comments()
        {
            if (Complete_Comments == null)
            {
                Msg_Complete = "Please enter Complete Comments.";
                return false;
            }

            return true;
        }

        // THIS FUNCTION DOES RESET VALUES FOR NOTES COMPLETE FORM.

        public void Reset_Complete_Form()
        {
            Complete_Comments = null;
            Note_ID_Complete = 0;
            Note_Description_Complete = null;
            ModelState.Clear();
        }

        // THIS FUNCTION IS CALLED FORM NOTES GRID'S ROW "COMPLETE NOTE" BUTTON.IT OPENS NOTES COMPLETE FORM AND
        // FILLS FIELDS WITH PASSED VALUES.

        public IActionResult OnPostComplete(int id, string note_desc_1)
        {
            Note_Description_Complete = note_desc_1;
            Note_ID_Complete = id;
            flag_complete = true;
            return Page();
        }
    }
}
