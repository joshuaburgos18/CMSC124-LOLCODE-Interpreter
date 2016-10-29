using System;

namespace LOLCODE_ABC
{
	public partial class Input : Gtk.Dialog
	{
		public string input; // storage for the input in GIMMEH
		public Input ()
		{
			Build ();		// build the dialog box when GIMMEH is found
		}

		public string getInput(){
			input = scanner.Text;	// gets the input in the entry
			return input;
		}
	}
}

