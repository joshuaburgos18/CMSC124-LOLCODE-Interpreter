using Gtk;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public partial class MainWindow: Gtk.Window
{
	ListStore lexemes = new ListStore (typeof(string), typeof(String));
	ListStore symbols = new ListStore (typeof(string), typeof(String));
	Hashtable symbol_value = new Hashtable();
	Stack<string> operation = new Stack<string>();
	string[] codes;
	string[] tokens;
	string quote = "\"";
	 /***********************************************************************************
     * Builds the MainWindow on run.													*
     * Sets the title of the window to 'LOLCode Interpreter - [ABC]'					*
     * Change the background color of the console to black.								*
     * Change the text color in the console to white.									*
     * Append two columns to the tree view of lexemes (LEXEMES, CLASSIFICATION).		*
     * Append two columns to the tree view of symbols (IDENTIFIER, VALUE).				*
	 ************************************************************************************/
	public MainWindow () : base (Gtk.WindowType.Toplevel)
	{
		Build ();
		this.Title = "LOLCode Interpreter - [ABC]";

		consolearea.ModifyBase (StateType.Normal, new Gdk.Color (0x00, 0x00, 0x00));
		consolearea.ModifyText (StateType.Normal, new Gdk.Color (0xff, 0xff, 0xff));

		lexemearea.AppendColumn ("LEXEMES", new CellRendererText (), "text", 0);
		lexemearea.AppendColumn ("CLASSIFICATION", new CellRendererText (), "text", 1);
		lexemearea.Model = lexemes;

		symbolarea.AppendColumn ("IDENTIFIER", new CellRendererText (), "text", 0);
		symbolarea.AppendColumn ("VALUE", new CellRendererText (), "text", 1);
		symbolarea.Model = symbols;
	}
	 /***********************************************************************************
	 * Destroys the MainWindow on exit.													* 
	 ***********************************************************************************/
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}
	 /***********************************************************************************
	 * Allows the user to open a file thru the file chooser dialog box.					*					
	 * It will be loaded automatically in the code area section.						*
	 ***********************************************************************************/
	protected void openFile (object sender, EventArgs e)
	{
		int width, height;
		this.GetDefaultSize(out width,out height);
		this.Resize( width, height);

		/* Create and display the file chooser dialog box */
		FileChooserDialog openFilePopUp = new FileChooserDialog(
			"Select LOLCODE File (*.lol)",
			this,
			FileChooserAction.Open,
			"Cancel", ResponseType.Cancel,
			"Open", ResponseType.Accept);

		if(openFilePopUp.Run() == ( int )ResponseType.Accept )
		{
			/* Opens file for reading */
			System.IO.StreamReader file = System.IO.File.OpenText( openFilePopUp.Filename );
			codearea.Buffer.Text = file.ReadToEnd();
			file.Close();
		} 
			openFilePopUp.Destroy();
	}

	 /**************************************************************************************
	 * If the trash bin button is clicked (beside EXECUTE button), 						   *
	 * it will clear the code area, console area, lexeme table, symbol table, and hashmap. *
	 **************************************************************************************/
	protected void OnButton2Clicked (object sender, EventArgs e)
	{
		consolearea.Buffer.Text = "";
		codearea.Buffer.Text = "";
		lexemes.Clear ();
		symbol_value.Clear ();
		symbols.Clear ();
	}
	 /**************************************************************************************
	 * User-defined function, executed when there is a syntax error or semantic error 	   *
	 * it will clear the list of lexems, list of symbols, and hashmap. 					   *
	 **************************************************************************************/
	 private void clear(){			
		lexemes.Clear ();
		symbols.Clear ();
		symbol_value.Clear ();
	}
/***********************************************************************************************************************************************/
	protected void execute (object sender, EventArgs e)
	{
		clear ();
		consolearea.Buffer.Text = "";
		Boolean sb = false;
		dynamic IT = 0;
		/* stores the input code to code */
		string code = codearea.Buffer.Text; 

		if (Regex.IsMatch (code, @"^\s*HAI(,\s*|\s*\n\s*)?")) {
			if (Regex.IsMatch (code, @"\s*KTHXBYE\s*$")) {
				/* serves as indicator of code per line */
				code = code.Replace ("\n", "\n*\n");
				code = code.Replace (", ", "\n*\n");
				/* split words */
				code = parse (code);
				/* split code by new line or comma store it to string array codes */
				codes = Regex.Split (code, @"[\n,]+");
				/* split code by new line or comma store it to string array codes */
				tokens = Regex.Split (code, @"[\n,]+");
				/* add classification to the lexemes */
				addClassification ();
				/* initialize the hashmap for the symbol table */
				initializehashmap ();

				/* add lexemes to lexemearea treeview */
				for (int i = 0; i < codes.Length; i++) {
					if (Regex.IsMatch (codes [i], @"\s*\*\s*")) {					
					} else if (Regex.IsMatch (codes [i], @"^\s*$")) {
					} else if (Regex.IsMatch(codes[i], @"^!$")){
					} else {
						lexemes.AppendValues (codes [i], tokens [i]);
					}
				}
			
				/* add symbols to symbolarea treeview */
				foreach(DictionaryEntry de in symbol_value){
					symbols.AppendValues (de.Key, de.Value);
				}

				for (int i = 0; i < codes.Length; i++) {
					var temp = i;
					/* IF-THEN STATEMENT */
					if (codes [i].Equals ("NO WAI")) {
						for (; !codes [i].Contains ("OIC"); i++) {
						}
					} else if (codes [i].Equals ("O RLY?")) {
						/* checks if OIC exists */
						while (!codes [i].Equals ("OIC") && i <= codes.Length - 2) {
							i++;
						}
						if (codes [i].Equals ("OIC")) {
							i = temp;
							/* <varname>, O RLY? ; gets the value of <varname> and store it to IT*/
							if (codes [i - 1].Equals ("*") && codes [i - 3].Equals ("*") && tokens [i - 2].Equals ("VARIABLE-IDENTIFIER")) {
							
								if (symbol_value.ContainsKey (codes [i - 2])) {
									IT = symbol_value [codes [i - 2]];

								}
							}
							i += 2;
							if (IT.Equals ("WIN")) {

							} else {
								/* Skips YA RLY */
								for (; !codes [i].Contains ("NO WAI"); i++) {
								}

							}
						} else if (codes [i].Equals ("KTHXBYE")) {
							consolearea.Buffer.Text = "> Syntax error: No IF-ELSE DELIMITER. WER IZ OIC?";
							return;
						}
			 /***********************************************************************************
			 * KEYWORD: VISIBLE																	* 
			 ***********************************************************************************/
					} else if (tokens [i].Equals ("OUTPUT KEYWORD")) {

						/* VISIBLE "YARN" */
						if (tokens [i + 1].Equals ("STRING DELIMITER")) {
							i += 3;
							if (codes [i].Equals (quote)) {
								consolearea.Buffer.Text = consolearea.Buffer.Text + codes [i - 1];							
							}
							if (codes [i+1].Equals ("!")) {
								consolearea.Buffer.Text = consolearea.Buffer.Text + "\n";
								i++;
							}
						}
						/* VISIBLE SMOOSH ... */
						else if (codes [i + 1].Equals ("SMOOSH")) {
							IT = concat (i + 1);
							consolearea.Buffer.Text = consolearea.Buffer.Text + IT + "\n";							
							for (; !codes [i].Contains ("*"); i++) {
							}
						} 
						/* VISIBLE <varname> */
						else if (tokens [i + 1].Equals ("VARIABLE IDENTIFIER")) {
							if (Regex.IsMatch (codes [i+1], @"!\n*$")) {
								codes [i+1] = codes [i+1].TrimEnd ('\n');
								codes [i+1] = codes [i+1].TrimEnd ('!');
								codes [i+1] = codes [i+1].TrimEnd ('\n');

								if (symbol_value.ContainsKey (codes [i + 1])) {
									var x = symbol_value[codes[i+1]];
									symbol_value.Remove (codes [i + 1]);

									clear ();
									for (int l = 0; l < codes.Length; l++) {
										if (Regex.IsMatch (codes [l], @"\s*\*\s*")) {

										} else if (Regex.IsMatch (codes [l], @"^\s*$")) {

										} else if (Regex.IsMatch(codes[l], @"^!$")){

										} else {
											lexemes.AppendValues (codes [l], tokens [l]);
										}
									}

									symbol_value.Add (codes [i + 1], x);

									foreach(DictionaryEntry de in symbol_value){
										symbols.AppendValues (de.Key, de.Value);
									}

									consolearea.Buffer.Text = consolearea.Buffer.Text + symbol_value[codes [i+1]];
									consolearea.Buffer.Text = consolearea.Buffer.Text + "\n";

								} else {
									consolearea.Buffer.Text = codes [i + 1] + "< SYNTAX ERROR: Variable not found >";
									clear ();
									return;
								}
							} else {
								if (symbol_value.ContainsKey (codes [i + 1])) {
									consolearea.Buffer.Text = consolearea.Buffer.Text + symbol_value [codes [i + 1]];	
								} else {
									consolearea.Buffer.Text = codes [i + 1] + "< SYNTAX ERROR: Variable not found >";
									clear ();
									return;
								}
							}
						/* VISIBLE <NUMBR || NUMBAR || TROOF LITERALS> */ 														
						} else if (tokens [i + 1].Equals ("NUMBR") || tokens [i + 1].Equals ("NUMBAR") || tokens [i + 1].Equals ("TROOF")) {
							i += 1;
							if (Regex.IsMatch (codes [i], @"!\s$")) {
								codes [i] = codes [i].TrimEnd ('\n');
								codes [i] = codes [i].TrimEnd ('!');
								codes [i] = codes [i].TrimEnd ('\n');
								consolearea.Buffer.Text = consolearea.Buffer.Text + codes [i];
								consolearea.Buffer.Text = consolearea.Buffer.Text + "\n";

								lexemes.Clear ();
								for (int l = 0; l < codes.Length; l++) {
									if (Regex.IsMatch (codes [l], @"\s*\*\s*")) {

									} else if (Regex.IsMatch (codes [l], @"^\s*$")) {

									} else if (Regex.IsMatch(codes[l], @"^!$")){

									} else {
										lexemes.AppendValues (codes [l], tokens [l]);
									}
								}

							} else {
								consolearea.Buffer.Text = consolearea.Buffer.Text + codes [i];
							}
						/* VISIBLE ARITHMETIC OPERATION */
						} else if (tokens [i + 1].Equals ("ARITHMETIC OPERATOR")) {
							IT = executeoperation (i + 1);
							consolearea.Buffer.Text = consolearea.Buffer.Text + IT + "\n";	
							for (; !codes [i].Contains ("*"); i++) {
							}
						/* VISIBLE COMPARISON OPERATION */
						} else if(tokens [i + 1].Equals ("COMPARISON OPERATOR")) {
							IT = executecompare (i + 1);
							consolearea.Buffer.Text = consolearea.Buffer.Text + IT + "\n";	
							for (; !codes [i].Contains ("*"); i++) {
							}
						/* VISIBLE BOOLEAN OPERATION */
						} else if (tokens [i + 1].Equals ("BOOLEAN OPERATOR")) { 
							IT = executeboolean (i + 1);
							consolearea.Buffer.Text = consolearea.Buffer.Text + IT + "\n";	
							for (; !codes [i].Contains ("*"); i++) {
							}
						} 
						/* END OF VISIBLE */
						/* START OF GIMMEH */
					} else if (tokens [i].Equals ("INPUT KEYWORD")) {
						if (tokens [i + 1].Equals ("VARIABLE IDENTIFIER")) {
							if (symbol_value.ContainsKey (codes [i + 1])) {
								LOLCODE_ABC.Input scan = new LOLCODE_ABC.Input ();
								scan.Run ();

								symbol_value [codes [i + 1]] = scan.getInput ();

								symbols.Clear ();
								foreach (DictionaryEntry de in symbol_value) {
									symbols.AppendValues (de.Key, de.Value);
								}
								scan.Destroy ();
							} else {
								consolearea.Buffer.Text = codes [i + 1] + "< SYNTAX ERROR: Variable not found >";
								clear ();
								return;
							}
						}
						/* END OF GIMMEH */
						/* START OF ASSIGNMENT */
					} else if (tokens [i].Equals ("SWITCH DELIMITER")) {
						Boolean s = true;
						/* checks if OIC exists */
						while (!codes [i].Equals ("OIC") && i <= codes.Length - 2) {
							i++;
						}

						if (codes [i].Equals ("OIC")) {
							i = temp;
							/* <varname>, WTF?; stores value of <varname> to IT */
							if (codes [i - 1].Equals ("*") && codes [i - 3].Equals ("*") && tokens [i - 2].Equals ("VARIABLE-IDENTIFIER")) {
								if (symbol_value.ContainsKey (codes [i - 2])) {
									IT = symbol_value [codes [i - 2]];

								}
							}
							i+=2;
							while(!codes[i].Equals("OIC") && s){
								if (tokens [i].Equals ("CASE")) {
									if (tokens [i + 1].Equals ("STRING DELIMITER") || tokens [i + 1].Equals ("NUMBR") || tokens [i + 1].Equals ("NUMBAR") || tokens [i + 1].Equals ("TROOF")) {
										if (tokens [i + 1].Equals ("STRING DELIMITER"))
											i++;
										
										if (IT.Equals (codes [i + 1])) {
											int j = i + 1;
											while(!tokens[j].Equals("CASE") && !tokens[j].Equals("IF / SWITCH DELIMITER") && !tokens[j].Equals("DEFAULT CASE")){

												if(tokens[j].Equals("BREAK")){
													sb = true;	
													break;
												}
												j++;
											}

											i+=2;
											s = false;
											break;
										} else {
											i++;
											while(!codes [i].Equals ("OMG") && !codes [i].Equals ("OMGWTF")){
												i++;
											}
										} 
									}
								}
								else if(tokens [i].Equals ("DEFAULT CASE")){
									s= false;
								}	
							}
						} else if (codes [i].Equals ("KTHXBYE")) {
							consolearea.Buffer.Text = "> Syntax error: No IF-ELSE DELIMITER. WER IZ OIC?";
							return;
						}
					} else if(codes[i].Equals("OMG") || codes[i].Equals("OMGWTF")){
						i++;
						if (sb) {

							while (!codes [i].Equals ("OIC")) {
								i++;
							}	
						} else if(codes[i-1].Equals("OMGWTF")){
							
						}
					} 
					/* ASSIGNMENT */
					else if (codes [i].Equals ("R")) {
						if (tokens [i - 1].Equals ("VARIABLE IDENTIFIER")) {
							if (symbol_value.ContainsKey (codes [i - 1])) {
								/* <var> R <var1> */
								if (tokens [i + 1].Equals ("VARIABLE IDENTIFIER")) {
									if (symbol_value.ContainsKey (codes [i + 1])) {
										symbol_value [codes [i - 1]] = symbol_value [codes [i + 1]];
										symbols.Clear ();
										foreach (DictionaryEntry de in symbol_value) {
											symbols.AppendValues (de.Key, de.Value);
										}
									} else {
										consolearea.Buffer.Text = codes [i + 1] + "< SYNTAX ERROR: Variable not found >";
										clear ();
										return;
									}
								} 
								/* <var> R <literal> */
								else if (tokens [i + 1].Equals ("NUMBR") || tokens [i + 1].Equals ("NUMBAR") || tokens [i + 1].Equals ("TROOF")) {
									symbol_value [codes [i - 1]] = codes [i + 1];
									symbols.Clear ();
									foreach (DictionaryEntry de in symbol_value) {
										symbols.AppendValues (de.Key, de.Value);
									}
								/* <var> R <expression> */
								} else if(tokens [i + 1].Equals ("BOOLEAN OPERATOR")){
									symbol_value[codes[i - 1]] = executeboolean (i + 1);
									symbols.Clear ();

									foreach (DictionaryEntry de in symbol_value) {
										symbols.AppendValues (de.Key, de.Value);
									}

									for (; !codes [i].Contains ("*"); i++) {
									}

								} else if(tokens [i + 1].Equals ("COMPARISON OPERATOR")){
									symbol_value [codes [i - 1]] = executecompare (i + 1);
									symbols.Clear ();

									foreach (DictionaryEntry de in symbol_value) {
										symbols.AppendValues (de.Key, de.Value);
									}

									for (; !codes [i].Contains ("*"); i++) {
									}
								} else if (tokens [i + 1].Equals ("ARITHMETIC OPERATOR")) {
									IT = executeoperation (i + 1);
									symbol_value [codes [i - 1]] = Convert.ToString (IT);
									symbols.Clear ();
									foreach (DictionaryEntry de in symbol_value) {
										symbols.AppendValues (de.Key, de.Value);
									}
									for (; !codes [i].Contains ("*"); i++) {
									}
								} else {
									symbol_value [codes [i - 1]] = codes [i + 2];
									symbols.Clear ();
									foreach (DictionaryEntry de in symbol_value) {
										symbols.AppendValues (de.Key, de.Value);
									}
								}
							} else {
								consolearea.Buffer.Text = codes [i - 1] + "< SYNTAX ERROR: Variable not found >";
								clear ();
								return;
							}
						} else {
							consolearea.Buffer.Text = codes [i + 1] + "< SYNTAX ERROR: Invalid Assignment >";
							clear ();
							return;
						}
						/* END OF ASSIGNMENT */
						/* START OF SMOOSH */
					} else if (tokens [i].Equals ("CONCATENATION")) {
						IT = concat (i + 1);
						/* END OF SMOOSH */
						/* START OF NOT */
					} 
						/* START OF ARITHMETIC OP */
					else if (tokens [i].Equals ("ARITHMETIC OPERATOR")) {
						IT = executeoperation (i);
						for (; !codes [i].Contains ("*"); i++) {
						}
						/* END OF ARITHMETIC OP */

					} else if (tokens [i].Equals ("COMPARISON OPERATOR")) {
						IT = executecompare (i);
						for (; !codes [i].Contains ("*"); i++) {
						}
						/* START OF ITZ */
					} else if (tokens [i].Equals ("INITIALIZATION")) {
						if (tokens [i + 1].Equals ("ARITHMETIC OPERATOR")) {
							IT = executeoperation (i + 1);
							symbol_value [codes [i - 1]] = IT;
							symbols.Clear ();
							foreach (DictionaryEntry de in symbol_value) {
								symbols.AppendValues (de.Key, de.Value);
							}
							for (; !codes [i].Contains ("*"); i++) {
							}
						} else if (codes [i + 1].Equals ("SMOOSH")) {
							IT = concat (i + 1);
							symbol_value [codes [i - 1]] = Convert.ToString (IT);
							symbols.Clear ();
							foreach (DictionaryEntry de in symbol_value) {
								symbols.AppendValues (de.Key, de.Value);
							}
							for (; !codes [i].Contains ("*"); i++) {
							
							}
						} else if (tokens [i + 1].Equals ("BOOLEAN OPERATOR")) {
							IT = executeboolean (i + 1);
							symbol_value [codes [i - 1]] = IT;
							symbols.Clear ();
							foreach (DictionaryEntry de in symbol_value) {
								symbols.AppendValues (de.Key, de.Value);
							}
							for (; !codes [i].Contains ("*"); i++) {
							}
						} else if(tokens [i + 1].Equals ("COMPARISON OPERATOR")) {
							IT = executecompare (i + 1);
							symbol_value [codes [i - 1]] = IT;
						
							symbols.Clear ();
							foreach (DictionaryEntry de in symbol_value) {
								symbols.AppendValues (de.Key, de.Value);
							}
							for (; !codes [i].Contains ("*"); i++) {
							}
						} else{
							
						}
					/* END OF ITZ */
					/* START OF BOOLEAN OP */
					} else if (tokens [i].Equals ("BOOLEAN OPERATOR")) {
						IT = executeboolean (i);
						for (; !codes [i].Contains ("*"); i++) {
						}
					
					}
				} /* end of for loop */	
			} else {
				/* Ending keyword is missing */
				consolearea.Buffer.Text = "> Syntax error: No ending keyword. WER IZ KTHXBYE?";
				return;
			} 
		} else {
			/* Starting keyword is missing */
			consolearea.Buffer.Text = "> Syntax error: No starting keyword. WER IZ HAI?";
			return;
		}
	}	/* END of execute() */


	/***********************************************************
	* Function for SMOOSH operation. Concatenates strings.     *
	* Literals are treated as strings						   *
	***********************************************************/
	public string concat(int i){
		int initial = i;
		int ctr = 0;
		string x = "";
		string s1 = "";
		string s2 = "";
		for(;i < codes.Length;i++){
			if (Regex.IsMatch (codes [i], @"\s*\*\s*")) {
				for (int j = i - 1; true; j--) {
					if (codes[initial-1].Equals(codes[j])) {
						s1 = operation.Pop ();
						ctr--;
						for(;ctr > 0; ctr--){
							s2 = operation.Pop ();
							s1 = s1 + s2;
						}
						return (s1);
					}
					else if(codes [j].Equals ("AN")){
						continue;
					} else if(tokens[j].Equals("BOOLEAN OPERATOR")){
						operation.Clear ();
						ctr = 0;
						operation.Push(Convert.ToString(executeboolean (j)));
						ctr++; 
					} else if(tokens[j].Equals("COMPARISON OPERATOR")){
						operation.Clear ();
						ctr = 0;
						operation.Push(Convert.ToString(executecompare (j)));
						ctr++;
					} else if(tokens[j].Equals("ARITHMETIC OPERATOR")){
						operation.Clear ();
						ctr = 0;
						operation.Push(Convert.ToString(executeoperation (j)));
						ctr++;
					} else if(tokens[j].Equals("YARN")){
						operation.Push (codes [j]);
						ctr++;
					} else if(tokens[j].Equals("NUMBR") || tokens[j].Equals("TROOF") || tokens[j].Equals("NUMBAR")){
						codes [j] = codes [j].Replace (" ", "");
						operation.Push (Convert.ToString(codes [j]));
						ctr++;
					} else if(tokens[j].Equals("VARIABLE IDENTIFIER")){
						if (symbol_value.ContainsKey (codes [j])) {
							s1 = Convert.ToString(symbol_value [codes [j]]);
							operation.Push (s1);
							ctr++;	
						} else {
							consolearea.Buffer.Text = codes [i - 1] + "< SYNTAX ERROR: Variable not found >";
							clear ();
						}

					}
				}
			}	
		}
		return x;
	}

	/***********************************************************************************
	* Executes COMPARISON OPERATIONS												   *
	* BOTH SAEM - DIFFRINT 															   *
	* executecompare(i) performs comparison operations						   		   *
	************************************************************************************/
	public string executecompare(int i){
		String x="";
		int initial = i;
		string s1;
		string s2;
		for (; i < codes.Length; i++) {
			if (Regex.IsMatch (codes [i], @"\s*\*\s*")) {
				if(codes [i - 1].Equals ("AN") && tokens[i - 2].Equals("ARITHMETIC OPERATOR")){
					consolearea.Buffer.Text = codes [i + 1] + "< SYNTAX ERROR: .. AN >";
					clear ();
					return "";
				}	else if (codes [i - 1].Equals ("AN")) {
					consolearea.Buffer.Text = codes [i + 1] + "< SYNTAX ERROR: .. AN >";
					clear ();
					return "";

				}
					
				for (int j = i - 1; true; j--) {
						if (codes [initial - 1].Equals (codes [j])) {
							x = operation.Pop ();
							return (x);
					} else if(codes [j].Equals ("AN") && tokens[j - 1].Equals("ARITHMETIC OPERATOR")){
						consolearea.Buffer.Text = codes [i + 1] + "< SYNTAX ERROR: .. AN >";
						clear ();
						return "";
					} else if (codes [j].Equals ("AN")) {
							continue;
					} else if (tokens [j].Equals ("NUMBR") || tokens [j].Equals ("NUMBAR") || tokens [j].Equals ("YARN")) {
							operation.Push (codes [j]);

					} else if (tokens [j].Equals ("VARIABLE IDENTIFIER")) {
							try {
								operation.Push (Convert.ToString (symbol_value [codes [j]]));
						
							} catch {
								consolearea.Buffer.Text = codes [i + 1] + "< SYNTAX ERROR: Variable not found >";
								clear ();
							return "";
							}		
					}
					/**********************************************************
					* COMPARISON OPERATION: BOTH SAEM <x> BIGGR OF <x> an <y> *
					* COMPARISON OPERATION: BOTH SAEM <x> SMALLR OF <x> an <y>*
					* COMPARISON OPERATION: DIFFRINT <x> SMALLR OF <x> an <y> *
					* COMPARISON OPERATION: DIFFRINT <x> BIGGR OF <x> an <y> *
					***********************************************************/
					else if (tokens [j].Equals ("ARITHMETIC OPERATOR")) {
						if (codes [j].Equals ("BIGGR OF")) {
							s1 = operation.Pop ();
							s2 = operation.Pop ();

							if ((Regex.IsMatch (s1, @"^-?\d*\.\d+\s*$") || Regex.IsMatch (s1, @"^-?\d+\s*$"))) {
								if ((Regex.IsMatch (s2, @"^-?\d*\.\d+\s*$") || Regex.IsMatch (s2, @"^-?\d+\s*$"))) {
									float fl;
									if (Convert.ToSingle (s1) > Convert.ToSingle (s2)) {
										fl = Convert.ToSingle (s1);
									} else {
										fl = Convert.ToSingle (s2);
									}
									operation.Push (Convert.ToString (fl));
								} else {
									consolearea.Buffer.Text = "< SYNTAX ERROR: In arithmetic operation >";
									clear ();
									return "";							
								}
							} else {
								consolearea.Buffer.Text = "< SYNTAX ERROR: In arithmetic operation >";
								clear ();
								return "";
							}
						} 
						/***********************************
						* ARITHMETIC OPERATION: SMALLR OF  *
						***********************************/
						else if (codes [j].Equals ("SMALLR OF")) {
							s1 = operation.Pop ();
							s2 = operation.Pop ();

							if ((Regex.IsMatch (s1, @"^-?\d*\.\d+\s*$") || Regex.IsMatch (s1, @"^-?\d+\s*$"))) {
								if ((Regex.IsMatch (s2, @"^-?\d*\.\d+\s*$") || Regex.IsMatch (s2, @"^-?\d+\s*$"))) {
									float fl;
									if (Convert.ToSingle (s1) < Convert.ToSingle (s2)) {
										fl = Convert.ToSingle (s1);
									} else {
										fl = Convert.ToSingle (s2);
									}
									operation.Push (Convert.ToString (fl));
								} else {
									consolearea.Buffer.Text = "< SYNTAX ERROR: In arithmetic operation >";
									clear ();
									return "";							
								}
							} else {
								consolearea.Buffer.Text = "< SYNTAX ERROR: In arithmetic operation >";
								clear ();
								return "";
							}
						}
					} else if (tokens [j].Equals ("COMPARISON OPERATOR")) {
						/***********************************
						* COMPARISON OPERATION: BOTH SAEM  *
						***********************************/
						if (codes [j].Equals ("BOTH SAEM")) {
							if (operation.Pop () == operation.Pop ()) {
								x = "WIN";
							} else
								x = "FAIL";
							operation.Push (x);
						}
						/***********************************
						* COMPARISON OPERATION: DIFFRINT   *
						***********************************/
						else if (codes [j].Equals ("DIFFRINT")) {
							if (operation.Pop () != operation.Pop ()) {
								x = "WIN";
							} else
								x = "FAIL";
							operation.Push (x);
						} 
					}
				}
			}
		}
		return x;
	}

	/***********************************************************************************
	* Executes BOOLEAN OPERATIONS													   *
	* BOTH OF - EITHER OF - WON OF - ALL OF - ANY OF								   *
	* executeboolean(i) performs boolean operations							   		   *
	************************************************************************************/
	public string executeboolean(int i){
		int initial = i;
		int x = 0;
		int j = 0;
		string s1;
		string s2;
		for (; i < codes.Length; i++) {
			if(Regex.IsMatch(codes[i], @"\s*\*\s*")){
				if(codes [i - 1].Equals ("AN") && tokens[i - 2].Equals("ARITHMETIC OPERATOR")){
					consolearea.Buffer.Text = codes [i + 1] + "< SYNTAX ERROR: .. AN >";
					clear ();
				}	else if (codes [i - 1].Equals ("AN")) {
					consolearea.Buffer.Text = codes [i + 1] + "< SYNTAX ERROR: .. AN >";
					clear ();
				}

				for(j=i-1; true; j--){
					if (codes [initial - 1].Equals (codes [j])) {
						/********************************
						* BOOLEAN OPERATION: ALL OF     *
						********************************/
						if (codes [j + 1].Equals ("ALL OF")) {
							if (codes [i - 1].Equals ("MKAY")) {
								s1 = operation.Pop ();
								s2 = operation.Pop ();
								if (s1.Equals ("WIN") && s2.Equals ("WIN")) {
									return s1;
								} else {
									return "FAIL";
								}	
							} else {
								consolearea.Buffer.Text = codes [j] + "< SYNTAX ERROR: No MKAY detected >";
								clear ();
								return "";
							}
						/********************************
						* BOOLEAN OPERATION: ANY OF     *
						********************************/
						} else if (codes [j + 1].Equals ("ANY OF")) {
							if (codes [i - 1].Equals ("MKAY")) {
								s1 = operation.Pop ();
								s2 = operation.Pop ();
								if (s1.Equals ("FAIL") && s2.Equals ("FAIL")) {
									return s1;
								} else {
									return "WIN";
								}	
							} else {
								consolearea.Buffer.Text = codes [j] + "< SYNTAX ERROR: No MKAY detected >";
								clear ();
								return "";
							}

						} else {
							s1 = operation.Pop ();
							return s1;
						}
					} else if (codes [j].Equals ("AN") || (codes [j].Equals ("MKAY") && codes [i - 1].Equals ("MKAY"))) {
						continue;
					} else if (j != i - 1 && codes [j].Equals ("MKAY")) {
						consolearea.Buffer.Text = codes [j] + "< SYNTAX ERROR: Invalid position of MKAY >";
						clear ();
						return "";
					} else if (tokens [j].Equals ("VARIABLE IDENTIFIER")) {
						if (symbol_value.ContainsKey (codes [j])) {
							operation.Push (Convert.ToString (symbol_value [codes [j]]));
						} else {
							consolearea.Buffer.Text = codes [i - 1] + "< SYNTAX ERROR: Variable not found >";
							clear ();
							return null;
						}
					} else if (tokens [j].Equals ("TROOF")) {
						operation.Push (codes [j]);
					} else if (tokens [j].Equals ("NUMBR") || tokens [j].Equals ("NUMBAR") || tokens [j].Equals ("YARN")) {
						consolearea.Buffer.Text = codes [i] + "< SYNTAX ERROR: In boolean operation >";
						clear ();
						return null;					
					} else if (tokens [j].Equals ("BOOLEAN OPERATOR")) {
						/********************************
						* BOOLEAN OPERATION: BOTH OF    *
						********************************/
						if (codes [j].Equals ("BOTH OF")) {
							s1 = operation.Pop ();
							s2 = operation.Pop ();
				
							if(Regex.IsMatch (s1, @"^WIN$") && Regex.IsMatch (s2, @"^WIN$")){
								operation.Push ("WIN");
							} else if (Regex.IsMatch (s1, @"^FAIL$") || Regex.IsMatch (s2, @"^FAIL$")) {

								operation.Push ("FAIL");	
							} else {
								consolearea.Buffer.Text = codes [i] + "< SYNTAX ERROR: In boolean operation >";
								clear ();
								return "";
							}
						} else if ((codes [j].Equals ("ALL OF") && j != initial) || (codes [j].Equals ("ANY OF") && j != initial)) {
							consolearea.Buffer.Text = codes [j] + "< SYNTAX ERROR: Invalid position operation >";
							clear ();
							return "";
						} 
						/********************************
						* BOOLEAN OPERATION: EITHER OF  *
						********************************/
						else if (codes [j].Equals ("EITHER OF")) {
							s1 = operation.Pop ();
							s2 = operation.Pop ();

							if(Regex.IsMatch (s1, @"^WIN$") || Regex.IsMatch (s2, @"^WIN$")){
								operation.Push ("WIN");
							} else if (Regex.IsMatch (s1, @"^FAIL$") && Regex.IsMatch (s2, @"^FAIL$")) {
								operation.Push ("FAIL");	
							} else {
								consolearea.Buffer.Text = codes [i] + "< SYNTAX ERROR: In boolean operation >";
								clear ();
								return "";
							}
						} 
						/********************************
						* BOOLEAN OPERATION: NOT        *
						********************************/
						else if(codes [j].Equals ("NOT")){
							s1 = operation.Pop ();
							if(s1.Equals("FAIL")){
								operation.Push ("WIN");
							} else if(s1.Equals("WIN")){
								operation.Push ("FAIL");
							}
						} 
						/********************************
						* BOOLEAN OPERATION: WON OF     *
						********************************/
						else if (codes [j].Equals ("WON OF")) {
							s1 = operation.Pop ();
							s2 = operation.Pop ();
						

							if(Regex.IsMatch (s1, @"^WIN$") && Regex.IsMatch (s2, @"^FAIL$")){
								operation.Push ("WIN");
							} else if(Regex.IsMatch (s2, @"^WIN$") && Regex.IsMatch (s1, @"^FAIL$")){
								operation.Push ("WIN");
							} else if ((Regex.IsMatch (s2, @"^FAIL$") && Regex.IsMatch (s1, @"^FAIL$")) || (Regex.IsMatch (s1, @"^FAIL$") && Regex.IsMatch (s2, @"^FAIL$"))) {
								operation.Push ("FAIL");	
							} else if ((Regex.IsMatch (s2, @"^WIN$") && Regex.IsMatch (s1, @"^WIN$")) || (Regex.IsMatch (s1, @"^WIN$") && Regex.IsMatch (s2, @"^WIN$"))) {
								operation.Push ("FAIL");	
							} else {
								consolearea.Buffer.Text = codes [i] + "< SYNTAX ERROR: In boolean operation >";
								clear ();
								return "";
							}
						}
					} 
				}
			}
		}
		return Convert.ToString(x);
	}

	/***********************************************************************************
	* Executes ARITHMETIC OPERATIONS												   *
	* SUM OF - DIFF OF - PRODUKT OF - QUOSHUNT OF - MOD OF - BIGGR OF -SMALLR OF	   *
	* executeoperation(i) performs arithmetic operations							   *
	************************************************************************************/
	public string executeoperation(int i){
		int x = 0;
		int j = 0;
		int initial = i;
		string s1;
		string s2;
		for (; i < codes.Length; i++) {
			if(Regex.IsMatch(codes[i], @"\s*\*\s*")){
				/* Excess AN */
				if(codes [i - 1].Equals ("AN") && tokens[i - 2].Equals("ARITHMETIC OPERATOR")){
					consolearea.Buffer.Text = codes [i + 1] + "< SYNTAX ERROR: .. AN >";
					clear ();
				}	else if (codes [i - 1].Equals ("AN")) {
					consolearea.Buffer.Text = codes [i + 1] + "< SYNTAX ERROR: .. AN >";
					clear ();
				} 
				for(j=i-1; true; j--){
					if (codes[initial-1].Equals(codes[j])) {
						s1 = operation.Pop ();
						return s1;
					} else if(codes [j].Equals ("AN") && tokens[j - 1].Equals("ARITHMETIC OPERATOR")){
						consolearea.Buffer.Text = codes [i + 1] + "< SYNTAX ERROR: .. AN >";
						clear ();
					} else if (codes [j].Equals ("AN")) {
						continue;
					} else if (codes [j].Contains (quote)) {
						j--;
						operation.Push (codes [j]);
						j--;
					} else if(tokens [j].Equals ("VARIABLE IDENTIFIER")){
						if (symbol_value.ContainsKey (codes [j])) {
							operation.Push (Convert.ToString(symbol_value[codes[j]]));
						} else {
							consolearea.Buffer.Text = codes [i - 1] + "< SYNTAX ERROR: Variable not found >";
							clear ();
						}
					} else if(tokens[j].Equals("TROOF")){

						consolearea.Buffer.Text = "< SYNTAX ERROR: In arithmetic operation >";
						clear ();
						return "";
					} else if (tokens [j].Equals ("NUMBR") || tokens [j].Equals ("NUMBAR")) {
						operation.Push (codes [j]);

					} else if (tokens [j].Equals ("ARITHMETIC OPERATOR")) {
						/********************************
						* ARITHMETIC OPERATION: SUM OF  *
						********************************/
						if (codes [j].Equals ("SUM OF")) {
							s1 = operation.Pop ();
							s2 = operation.Pop ();
						
							if ((Regex.IsMatch (s1, @"^-?\d*\.\d+\s*$") || Regex.IsMatch (s1, @"^-?\d+\s*$"))) {
								if ((Regex.IsMatch (s2, @"^-?\d*\.\d+\s*$") || Regex.IsMatch (s2, @"^-?\d+\s*$"))) {
									float fl;
									fl = Convert.ToSingle (s1) + Convert.ToSingle (s2); 
									operation.Push (Convert.ToString (fl));
								} else {
									consolearea.Buffer.Text = "< SYNTAX ERROR: In arithmetic operation >";
									clear ();
									return "";							
								}
							} else {
								consolearea.Buffer.Text = "< SYNTAX ERROR: In arithmetic operation >";
								clear ();
								return "";
							}
						}
						/********************************
						* ARITHMETIC OPERATION: DIFF OF *
						********************************/
						else if (codes [j].Equals ("DIFF OF")) {
							s1 = operation.Pop ();
							s2 = operation.Pop ();

							if ((Regex.IsMatch (s1, @"^-?\d*\.\d+\s*$") || Regex.IsMatch (s1, @"^-?\d+\s*$"))) {
								if ((Regex.IsMatch (s2, @"^-?\d*\.\d+\s*$") || Regex.IsMatch (s2, @"^-?\d+\s*$"))) {
									float fl;
									fl = Convert.ToSingle (s1) - Convert.ToSingle (s2); 
									operation.Push (Convert.ToString (fl));
								} else {
									consolearea.Buffer.Text = "< SYNTAX ERROR: In arithmetic operation >";
									clear ();
									return "";							
								}
							} else {
								consolearea.Buffer.Text = "< SYNTAX ERROR: In arithmetic operation >";
								clear ();
								return "";
							}
						}
						/***********************************
						* ARITHMETIC OPERATION: PRODUKT OF *
						***********************************/ 
						else if (codes [j].Equals ("PRODUKT OF")) {
							s1 = operation.Pop ();
							s2 = operation.Pop ();

							if ((Regex.IsMatch (s1, @"^-?\d*\.\d+\s*$") || Regex.IsMatch (s1, @"^-?\d+\s*$"))) {
								if ((Regex.IsMatch (s2, @"^-?\d*\.\d+\s*$") || Regex.IsMatch (s2, @"^-?\d+\s*$"))) {
									float fl;
									fl = Convert.ToSingle (s1) * Convert.ToSingle (s2); 
									operation.Push (Convert.ToString (fl));
								} else {
									consolearea.Buffer.Text = "< SYNTAX ERROR: In arithmetic operation >";
									clear ();
									return "";							
								}
							} else {
								consolearea.Buffer.Text = "< SYNTAX ERROR: In arithmetic operation >";
								clear ();
								return "";
							}
						} 
						/************************************
						* ARITHMETIC OPERATION: QUOSHUNT OF *
						************************************/
						else if (codes [j].Equals ("QUOSHUNT OF")) {
							s1 = operation.Pop ();
							s2 = operation.Pop ();

							if ((Regex.IsMatch (s1, @"^-?\d*\.\d+\s*$") || Regex.IsMatch (s1, @"^-?\d+\s*$"))) {
								if ((Regex.IsMatch (s2, @"^-?\d*\.\d+\s*$") || Regex.IsMatch (s2, @"^-?\d+\s*$"))) {
									float fl;
									fl = Convert.ToSingle (s1) / Convert.ToSingle (s2); 
									operation.Push (Convert.ToString (fl));
								} else {
									consolearea.Buffer.Text = "< SYNTAX ERROR: In arithmetic operation >";
									clear ();
									return "";							
								}
							} else {
								consolearea.Buffer.Text = "< SYNTAX ERROR: In arithmetic operation >";
								clear ();
								return "";
							}
						}
						/*******************************
						* ARITHMETIC OPERATION: MOD OF *
						*******************************/
						else if (codes [j].Equals ("MOD OF")) {
							s1 = operation.Pop ();
							s2 = operation.Pop ();

							if ((Regex.IsMatch (s1, @"^-?\d*\.\d+\s*$") || Regex.IsMatch (s1, @"^-?\d+\s*$"))) {
								if ((Regex.IsMatch (s2, @"^-?\d*\.\d+\s*$") || Regex.IsMatch (s2, @"^-?\d+\s*$"))) {
									float fl;
									fl = Convert.ToSingle (s1) % Convert.ToSingle (s2); 
									operation.Push (Convert.ToString (fl));
								} else {
									consolearea.Buffer.Text = "< SYNTAX ERROR: In arithmetic operation >";
									clear ();
									return "";							
								}
							} else {
								consolearea.Buffer.Text = "< SYNTAX ERROR: In arithmetic operation >";
								clear ();
								return "";
							}
						} 
						/***********************************
						* ARITHMETIC OPERATION: BIGGR OF   *
						***********************************/
						else if (codes [j].Equals ("BIGGR OF")) {
							s1 = operation.Pop ();
							s2 = operation.Pop ();

							if ((Regex.IsMatch (s1, @"^-?\d*\.\d+\s*$") || Regex.IsMatch (s1, @"^-?\d+\s*$"))) {
								if ((Regex.IsMatch (s2, @"^-?\d*\.\d+\s*$") || Regex.IsMatch (s2, @"^-?\d+\s*$"))) {
									float fl;
									if (Convert.ToSingle (s1) > Convert.ToSingle (s2)) {
										fl = Convert.ToSingle (s1);
									} else {
										fl = Convert.ToSingle (s2);
									}
									operation.Push (Convert.ToString (fl));
								} else {
									consolearea.Buffer.Text = "< SYNTAX ERROR: In arithmetic operation >";
									clear ();
									return "";							
								}
							} else {
								consolearea.Buffer.Text = "< SYNTAX ERROR: In arithmetic operation >";
								clear ();
								return "";
							}
						} 
						/***********************************
						* ARITHMETIC OPERATION: SMALLR OF  *
						***********************************/
						else if (codes [j].Equals ("SMALLR OF")) {
							s1 = operation.Pop ();
							s2 = operation.Pop ();

							if ((Regex.IsMatch (s1, @"^-?\d*\.\d+\s*$") || Regex.IsMatch (s1, @"^-?\d+\s*$"))) {
								if ((Regex.IsMatch (s2, @"^-?\d*\.\d+\s*$") || Regex.IsMatch (s2, @"^-?\d+\s*$"))) {
									float fl;
									if (Convert.ToSingle (s1) < Convert.ToSingle (s2)) {
										fl = Convert.ToSingle (s1);
									} else {
										fl = Convert.ToSingle (s2);
									}
									operation.Push (Convert.ToString (fl));
								} else {
									consolearea.Buffer.Text = "< SYNTAX ERROR: In arithmetic operation >";
									clear ();
									return "";							
								}
							} else {
								consolearea.Buffer.Text = "< SYNTAX ERROR: In arithmetic operation >";
								clear ();
								return "";
							}
						}

					} 
				}
			}
		}
		return Convert.ToString(x);
	} 
	/******************************************************************
	* parse() inserts new line in between the found keywords in the	  *
	* code so that we can easily split the code by new line. This will*
	* be seen on the lexemes column in the lexeme tree view			  *
	*****************************************************************/
	/* Start of parse() */
	private string parse(String code){
		code = code.Replace ("\"", "\n\"\n");
		code = code.Replace ("WIN ", "\nWIN\n");
		code = code.Replace ("FAIL ", "\nFAIL\n");
		code = code.Replace ("HAI", "HAI\n");
		code = code.Replace ("KTHXBYE", "\nKTHXBYE");
		code = code.Replace ("OBTW ", "OBTW\n");
		code = code.Replace ("OBTW\t", "\nOBTW\n");
		code = code.Replace ("TLDR", "\nTLDR\n");
		code = code.Replace (" BTW ", "\nBTW\n");
		code = code.Replace ("BTW ", "\nBTW\n");
		code = code.Replace (" I HAS A ", "\nI HAS A\n");
		code = code.Replace ("I HAS A ", "\nI HAS A\n");
		code = code.Replace (" ITZ ", "\nITZ\n");
		code = code.Replace (" R ", "\nR\n");
		code = code.Replace ("SUM OF ", "\nSUM OF\n");
		code = code.Replace ("DIFF OF ", "\nDIFF OF\n");
		code = code.Replace ("PRODUKT OF ", "\nPRODUKT OF\n");
		code = code.Replace ("QUOSHUNT OF ", "\nQUOSHUNT OF\n");
		code = code.Replace ("MOD OF ", "\nMOD OF\n");
		code = code.Replace ("BIGGR OF ", "\nBIGGR OF\n");
		code = code.Replace ("SMALLR OF ", "\nSMALLR OF\n");
		code = code.Replace ("BOTH OF ", "\nBOTH OF\n");
		code = code.Replace ("EITHER OF ", "\nEITHER OF\n");
		code = code.Replace ("WON OF ", "\nWON OF\n");
		code = code.Replace ("NOT ", "\nNOT\n");
		code = code.Replace ("ALL OF ", "\nALL OF\n");
		code = code.Replace ("ANY OF ", "\nANY OF\n");
		code = code.Replace ("BOTH SAEM ", "\nBOTH SAEM\n");
		code = code.Replace ("DIFFRINT ", "\nDIFFRINT\n");
		code = code.Replace ("SMOOSH ", "\nSMOOSH\n");
		code = code.Replace (" MKAY", "\nMKAY\n");
		code = code.Replace ("MAEK ", "\nMAEK\n");
		code = code.Replace ("AN ", "\nAN\n");
		code = code.Replace (" AN ", "\nAN\n");
		code = code.Replace ("IS NOW A ", "\nIS NOW A\n");
		code = code.Replace ("VISIBLE ", "\nVISIBLE\n");
		code = code.Replace ("GIMMEH ", "\nGIMMEH\n");
		code = code.Replace ("O RLY?", "\nO RLY?\n");
		code = code.Replace ("YA RLY", "\nYA RLY\n");
		code = code.Replace ("MEBBE", "\nMEBBE\n");
		code = code.Replace ("NO WAI", "\nNO WAI\n");
		code = code.Replace ("OIC", "\nOIC\n");
		code = code.Replace ("WTF?", "\nWTF?\n");
		code = code.Replace ("OMG ", "\nOMG\n");
		code = code.Replace ("OMGWTF ", "\nOMGWTF\n");
		code = code.Replace ("GTFO", "\nGTFO\n");
		code = code.Replace ("IM IN YR ", "\nIM IN YR\n");
		code = code.Replace ("UPPIN ", "\nUPPIN\n");
		code = code.Replace ("NERFIN ", "\nNERFIN\n");
		code = code.Replace ("YR ", "\nYR\n");
		code = code.Replace ("TIL ", "\nTIL\n");
		code = code.Replace ("WILE ", "\nWILE\n");
		code = code.Replace ("IM OUTTA YR ", "\nIM OUTTA YR\n");
		return code;
	} /* End of parse() */

	/**************************************************************************
	* addClassification function adds classification to each lexeme found in  *
	* the code. It can be seen in the column classification in the lexeme tree*
	* view.																	  *
	***************************************************************************/
	/* Start of addClassification() */
	private void addClassification(){
		/* traverse the string array of lexemes */
		for (int k = 0; k < codes.Length; k++) {
			/* Note: \* is our line indicator */
			if (Regex.IsMatch (codes [k], @"\s*\*\s*"))	continue;
			/* if lexeme is the starting keyword */
			if (Regex.IsMatch (codes [k], @"^\s*HAI\s*$")) {
				/* if position of the lexeme is at the start */
				if (k == 0) {
					codes [k] = "HAI";
					tokens [k] = "CODE DELIMITER";
					/* else, it produces syntax error */
				} else if (k != 0) {
					var chk = k;
					while (chk - 1 >= 0 && Regex.IsMatch (codes [chk - 1], @"^\s*[\*]?\s*$")) { 
						chk--;
					}

					if (chk == 0) {
						codes [k] = "HAI";
						tokens [k] = "CODE DELIMITER";
					} else {
						consolearea.Buffer.Text = consolearea.Buffer.Text + "> Syntax error: Multiple declaration of HAI. R U INZEYN?? \n";
						clear ();
					}
				} else {
					consolearea.Buffer.Text = consolearea.Buffer.Text + "> Syntax error: Multiple declaration of HAI. R U INZEYN? \n";
					clear ();
				}
			} 
			/* if lexeme is the ending keyword */
			else if (Regex.IsMatch (codes [k], @"^\s*KTHXBYE\s*$")) {
				/* if position of the lexeme is at the end of the code */ 
				codes [k] = "KTHXBYE";
				tokens [k] = "CODE DELIMITER";
				if (k != codes.Length - 1) {
					while (k != codes.Length - 1 && Regex.IsMatch (codes [k + 1], @"^\s*[*]?\s*$")) {
						k++;
					}
					if (k == codes.Length - 1) {
					
					} else {
						consolearea.Buffer.Text = consolearea.Buffer.Text + "> Syntax error: Words after KTHXBYE \n";
						clear ();
					}
				}
			}
			/* if lexeme is the indicator of one line comment */
			else if (Regex.IsMatch (codes [k], @"^BTW$")) {
				tokens [k] = "COMMENT DELIMITER";
				k++;
				tokens [k] = "COMMENT";
				var temp = k;
				while (!Regex.IsMatch (codes [k + 1], @"\s*\*\s*")) {
					codes [temp] = codes [temp] + codes [k + 1];
					codes [k + 1] = "";
					tokens [k + 1] = "";
					k++;
				}
			} else if (Regex.IsMatch (codes [k], @"^\s*OBTW\s*$")) {
				if (codes [k - 1].Equals ("*")) {
					codes [k] = "OBTW";
					tokens [k] = "COMMENT DELIMITER";
					while (codes [k + 1] != "TLDR" && k < codes.Length - 2) {
						k++;
						tokens [k] = "COMMENT";
					}
						
					if (k == codes.Length - 2) {
						consolearea.Buffer.Text = "< SYNTAX ERROR: NO TLDR >";
						clear ();
						return;
					}

					if (codes [k].Equals ("*") && codes [k + 2].Equals ("*")) {

					} else {
						consolearea.Buffer.Text = "< SYNTAX ERROR: TLDR should be alone on its line. >";
						clear ();
						return;
					}


				} else {
					consolearea.Buffer.Text = consolearea.Buffer.Text + "> Syntax error: Invalid position of OBTW. \n";
					clear ();					
				}
			} else if (Regex.IsMatch (codes [k], @"^TLDR$")) {
				tokens [k] = "COMMENT DELIMITER";
			} else if (Regex.IsMatch (codes [k], @"^WIN!?$") || Regex.IsMatch (codes [k], @"^FAIL!?$")) {
				if (codes [k].EndsWith ("!")) {
					codes [k] = codes [k].TrimEnd ('!');
					codes [k] = codes [k] + "\n!\n";
					codes [k].Replace (" ", "");
					tokens [k] = "TROOF";
				} else {
					codes [k].Replace (" ", "");
					tokens [k] = "TROOF";
				}
			} else if (Regex.IsMatch (codes [k], @"^I HAS A$")) {
				tokens [k] = "VARIABLE DECLARATION";
				k++;
				if (Regex.IsMatch (codes [k], @"^(?!HAI|KTHXBYE|BTW|OBTW|TLDR|I HAS A|ITZ|R|SUM OF|DIFF OF|PRODUKT OF|QUOSHUNT OF|MOD OF|BIGGR OF|SMALLR OF|BOTH OF|EITHER OF|WON OF|NOT|ALL OF|ANY OF|BOTH SAEM|DIFFRINT|SMOOSH|MAEK|AN|IS NOW A|VISIBLE|GIMMEH|O RLY?|YA RLY|MEBBEH|NO WAI|OIC|WTF?|OMG|OMGWTF|GTFO|IM IN YR|UPPIN|NERFIN|YR|TIL|WILE|IM OUTTA YR)[a-zA-Z][a-zA-Z0-9_]*\s*$")) {
					tokens [k] = "VARIABLE IDENTIFIER";
					codes [k] = codes [k].TrimStart ('\t');
					codes [k] = codes [k].TrimStart (' ');
					codes [k] = codes [k].TrimEnd ('\t');
					codes [k] = codes [k].TrimEnd (' ');
				} else {
					consolearea.Buffer.Text = codes [k] + "< SYNTAX ERROR: Invalid Variable Identifier >";
					clear ();
				}
			} else if (Regex.IsMatch (codes [k], @"^ITZ$"))
				tokens [k] = "INITIALIZATION";
			else if (Regex.IsMatch (codes [k], @"^R$"))
				tokens [k] = "ASSIGNMENT";
			else if (Regex.IsMatch (codes [k], @"^SUM OF$"))
				tokens [k] = "ARITHMETIC OPERATOR";
			else if (Regex.IsMatch (codes [k], @"^DIFF OF$"))
				tokens [k] = "ARITHMETIC OPERATOR";
			else if (Regex.IsMatch (codes [k], @"^PRODUKT OF$"))
				tokens [k] = "ARITHMETIC OPERATOR";
			else if (Regex.IsMatch (codes [k], @"^QUOSHUNT OF$"))
				tokens [k] = "ARITHMETIC OPERATOR";
			else if (Regex.IsMatch (codes [k], @"^MOD OF$"))
				tokens [k] = "ARITHMETIC OPERATOR";
			else if (Regex.IsMatch (codes [k], @"^BIGGR OF$"))
				tokens [k] = "ARITHMETIC OPERATOR";
			else if (Regex.IsMatch (codes [k], @"^SMALLR OF$"))
				tokens [k] = "ARITHMETIC OPERATOR";
			else if (Regex.IsMatch (codes [k], @"^BOTH OF$"))
				tokens [k] = "BOOLEAN OPERATOR";
			else if (Regex.IsMatch (codes [k], @"^EITHER OF$"))
				tokens [k] = "BOOLEAN OPERATOR";
			else if (Regex.IsMatch (codes [k], @"^WON OF$"))
				tokens [k] = "BOOLEAN OPERATOR";
			else if (Regex.IsMatch (codes [k], @"^NOT$"))
				tokens [k] = "BOOLEAN OPERATOR";
			else if (Regex.IsMatch (codes [k], @"^ALL OF$"))
				tokens [k] = "BOOLEAN OPERATOR";
			else if (Regex.IsMatch (codes [k], @"^ANY OF$"))
				tokens [k] = "BOOLEAN OPERATOR";
			else if (Regex.IsMatch (codes [k], @"^BOTH SAEM$"))
				tokens [k] = "COMPARISON OPERATOR";
			else if (Regex.IsMatch (codes [k], @"^DIFFRINT$"))
				tokens [k] = "COMPARISON OPERATOR";
			else if (Regex.IsMatch (codes [k], @"^SMOOSH$"))
				tokens [k] = "STRING CONCATENATION";
			else if (Regex.IsMatch (codes [k], @"^MKAY$"))
				tokens [k] = "INFINITE ARITY TERMINATOR";
			else if (Regex.IsMatch (codes [k], @"^MAEK$"))
				tokens [k] = "DATA TYPE CONVERSION";
			else if (Regex.IsMatch (codes [k], @"^AN$"))
				tokens [k] = "EXPRESSION SEPARATOR";
			else if (Regex.IsMatch (codes [k], @"^IS NOW A$"))
				tokens [k] = "DATA TYPE CONVERSION";
			else if (Regex.IsMatch (codes [k], @"^VISIBLE$")) {
				tokens [k] = "OUTPUT KEYWORD";
			} else if (Regex.IsMatch (codes [k], @"^GIMMEH$")) {
				if (codes [k - 1].Equals ("*") && Regex.IsMatch (codes [k + 1], @"^[a-zA-Z][a-zA-Z0-9_]*") && (codes [k + 2].Equals ("*") || codes [k + 2].Equals ("BTW"))) {
					tokens [k] = "INPUT KEYWORD";
				} else {
					consolearea.Buffer.Text = consolearea.Buffer.Text + "> Syntax error: GIMMEH <varname>" + "\n";
					lexemes.Clear ();
					symbols.Clear ();
					symbol_value.Clear ();
					clear ();
					return;
				}
			} else if (Regex.IsMatch (codes [k], @"^O RLY\?$"))
				tokens [k] = "IF-ELSE DELIMITER";
			else if (Regex.IsMatch (codes [k], @"^YA RLY$"))
				tokens [k] = "IF <WIN>";
			else if (Regex.IsMatch (codes [k], @"^MEBBE$"))
				tokens [k] = "ELSE IF";
			else if (Regex.IsMatch (codes [k], @"^NO WAI$"))
				tokens [k] = "ELSE <FAIL>";
			else if (Regex.IsMatch (codes [k], @"^OIC$"))
				tokens [k] = "IF / SWITCH DELIMITER";
			else if (Regex.IsMatch (codes [k], @"^WTF\?$"))
				tokens [k] = "SWITCH DELIMITER";
			else if (Regex.IsMatch (codes [k], @"^OMG$"))
				tokens [k] = "CASE";
			else if (Regex.IsMatch (codes [k], @"^OMGWTF$"))
				tokens [k] = "DEFAULT CASE";
			else if (Regex.IsMatch (codes [k], @"^GTFO$"))
				tokens [k] = "BREAK";
			else if (Regex.IsMatch (codes [k], @"^IM IN YR$"))
				tokens [k] = "LOOP DELIMITER";
			else if (Regex.IsMatch (codes [k], @"^UPPIN$"))
				tokens [k] = "INCREMENT";
			else if (Regex.IsMatch (codes [k], @"^NERFIN$"))
				tokens [k] = "DECREMENT";
			else if (Regex.IsMatch (codes [k], @"^YR$"))
				tokens [k] = "LITERAL INDICATOR";
			else if (Regex.IsMatch (codes [k], @"^TIL$"))
				tokens [k] = "LOOP CONDITION";
			else if (Regex.IsMatch (codes [k], @"^WILE$"))
				tokens [k] = "LOOP CONDITION";
			else if (Regex.IsMatch (codes [k], @"^IM OUTTA YR$"))
				tokens [k] = "LOOP DELIMITER";
			/* if lexemes is a yarn */
			else if (codes [k].Contains (quote)) {
				tokens [k] = "STRING DELIMITER";	
				k++;
				tokens [k] = "YARN";
				/* capitalize the yarn */
				codes [k] = codes [k].ToUpper ();
				var temp = k;
				k++;
				while (!codes [k].Contains (quote) && k > codes [k].Length) {
					k++;
				}
				/* if \" is found */
				if (codes [k].Contains (quote)) {
					tokens [k] = "STRING DELIMITER";
					codes [k] = codes [k].Replace ("!", "\n!\n");
					var n = temp;
					while (n + 1 < k) {
						codes [temp] = codes [n] + codes [n + 1];
						codes [n + 1] = "";
						n++;
					}
				} else {
					/* if there is no string delimiter */
					consolearea.Buffer.Text = consolearea.Buffer.Text + "> Syntax error: String delimeter not found. WER IZ \" ? " + "\n";
					clear ();
				}
			}
			/* if lexeme is a float */
			else if (Regex.IsMatch (codes [k], @"^\s*-?\d*\.\d+!?\s*$")) {
				/* disregard whitespaces at the start and end */
				if (codes [k].EndsWith ("!")) {
					codes [k] = codes [k].TrimEnd ('!');
					codes [k] = codes [k] + "\n!\n";
					codes [k].Replace (" ", "");
					tokens [k] = "NUMBAR";
				} else {
					codes [k].Replace (" ", "");
					tokens [k] = "NUMBAR";
				}
				/* if lexeme is an integer */
			} else if (Regex.IsMatch (codes [k], @"^\s*-?\d+!?\s*$")) {	
				/* disregard whitespaces at the start and end */
				if (codes [k].EndsWith ("!")) {
					codes [k] = codes [k].TrimEnd ('!');
					codes [k] = codes [k] + "\n!\n";
					codes [k].Replace (" ", "");
					tokens [k] = "NUMBR";
				} else {
					codes [k].Replace (" ", "");
					tokens [k] = "NUMBR";
				}
				/* skip whitespaces */
			} else if (Regex.IsMatch (codes [k], @"^\s*$")) {
			} 
			/* if lexeme is a variable identifier */
			else if (Regex.IsMatch (codes [k], @"^\s*[a-zA-Z][a-zA-Z0-9_]*!?\s*$")) {
				/* disregard whitespaces at the start and end */
				codes [k].Replace (" ", "");
				/* variable identifier is not usually found at the start of the line */
				if (!codes [k - 1].Equals ("*")) {
					tokens [k] = "VARIABLE IDENTIFIER";
					codes [k] = codes [k].TrimStart ('\t');
					codes [k] = codes [k].TrimStart (' ');
					codes [k] = codes [k].TrimEnd ('\t');
					codes [k] = codes [k].TrimEnd (' ');
					/* if variable identifier is positioned at the start of the line */
				} else if (codes [k + 1].Equals ("R")) {
					tokens [k] = "VARIABLE IDENTIFIER";
					codes [k] = codes [k].TrimStart ('\t');
					codes [k] = codes [k].TrimStart (' ');
					codes [k] = codes [k].TrimEnd ('\t');
					codes [k] = codes [k].TrimEnd (' ');
				} else if(codes[k-1].Equals("*") && codes[k+1].Equals("*") && codes[k+2].Equals("O RLY?")){
					tokens [k] = "VARIABLE-IDENTIFIER";
					codes [k] = codes [k].TrimStart ('\t');
					codes [k] = codes [k].TrimStart (' ');
					codes [k] = codes [k].TrimEnd ('\t');
					codes [k] = codes [k].TrimEnd (' ');
				}else if(codes[k-1].Equals("*") && codes[k+1].Equals("*") && codes[k+2].Equals("WTF?")){
					tokens [k] = "VARIABLE-IDENTIFIER";
					codes [k] = codes [k].TrimStart ('\t');
					codes [k] = codes [k].TrimStart (' ');
					codes [k] = codes [k].TrimEnd ('\t');
					codes [k] = codes [k].TrimEnd (' ');
				}else {
					consolearea.Buffer.Text = consolearea.Buffer.Text + "> Syntax error: " + codes [k] + "\n";
					clear ();
				}
			} else if (codes [k].Equals ("!")) {
			
			}
			/* else */
			else {
				consolearea.Buffer.Text = consolearea.Buffer.Text + "> Syntax error: " + codes[k] + "\n";
				clear ();
			} 
		} /* end of for loop */
		return;	/* return to caller */
	} /* End of addClassification() */

	 /***********************************************************************************
     * Initializes the hash table.		<key, value> == <variablename, value>			*
     * Uinitialized: I HAS A <variablename>												*
     * Initialized with LITERAL: I HAS A <var> ITZ <[numbr, numbar, yarn, troof, var1]>	*
     * 		I HAS A <varname> ITZ 5; I HAS A <varname> ITZ 5.05; 						*
     * 		I HAS A <varname> ITZ "five"; I HAS A <varname> ITZ WIN						*	
     * Initialized with EXPRESSION: 													*
     * I HAS A <varname> ITZ <[arithmetic,boolean,comparison,smoosh,nested expression]> *													
	 * 		I HAS A <varname> ITZ SUM OF 5 AN 10										*
	 *		I HAS A <varname> ITZ BOTH OF WIN AN WIN									*
	 *		I HAS A <varname> ITZ BOTH SAEM 5 AN 5										*
	 * 		I HAS A <varname> ITZ SMOOSH "HI" AN "HELLO"								*
	 * 		I HAS A <varname> ITZ PRODUKT OF SUM OF 5 AN 10 AN 2						*
	 ***********************************************************************************/
	private void initializehashmap(){
		/* traverse the string array of lexemes */
		for (int i = 0; i < codes.Length; i++) {
			/* find for variable declaration */
			if (Regex.IsMatch (codes [i], @"^\s*I HAS A\s*$") && tokens[i].Equals("VARIABLE DECLARATION")) {
				i++; /* skip the I HAS A */
				/***********************************************************
				*						UNINITIALIZED					   *
				***********************************************************/
				/* if variable declaration has no initialization */
				if(!Regex.IsMatch (codes [i+1], @"^ITZ$")){
					try {
						/* Add the variable name as key to the hashtable without a value */
						symbol_value.Add(codes[i],"");	
					} catch {
						/* Catch error if variable name (key) already exists */
						consolearea.Buffer.Text = consolearea.Buffer.Text + "> Syntax error: Multiple declaration of variable. (" + codes[i] + ")\n";
						clear ();
					}
				}
				/* if variable declaration has initialization */
				else {
					i += 2; /* skip the VARIABLE NAME and ITZ */
					/***********************************************************
					* INITIALIZED WITH LITERAL(YARN): I HAS A <var> ITZ "YARN" *
					***********************************************************/
					/* if value's data type is string (yarn) */
					if (codes [i].Contains (quote)) {
						i++;	/* skip open quotation */
						try {
							/* Add the variable name as key to the hashtable with a value corresponding to the string */
							symbol_value.Add (codes [i - 3], codes [i]);
						} catch {
							/* Catch error if variable name (key) already exists */
							consolearea.Buffer.Text = consolearea.Buffer.Text + "> Syntax error: Multiple declaration of variable. (" + codes[i - 3] + ")\n";
							clear ();
						}
					}
					/***********************************************************
					*INITIALIZED WITH LITERAL(TROOF):I HAS A <var> ITZ WIN/FAIL*
					***********************************************************/
					/* if value's data type is boolean (troof) */
					else if(Regex.IsMatch(codes[i], @"^WIN$") || Regex.IsMatch(codes[i], @"^FAIL$")){
						try {
							/* Add the variable name as key to the hashtable with a value (WIN or FAIL) */
							symbol_value.Add (codes [i - 2], codes [i]);	
						} catch{
							/* Catch error if variable name (key) already exists */
							consolearea.Buffer.Text = consolearea.Buffer.Text + "> Syntax error: Multiple declaration of variable. (" + codes[i - 2] + ")\n";
							clear ();
						}
					}
					/***********************************************************
					*INITIALIZED WITH LITERAL(VAR):I HAS A <var> ITZ <othervar>*
					***********************************************************/
					/* if value is from another variable */
					else if(Regex.IsMatch (codes [i], @"^(?!SUM OF|SMOOSH|DIFF OF|PRODUKT OF|QUOSHUNT OF|MOD OF|DIFFRINT|BOTH SAEM|BIGGR OF|SMALLR OF|BOTH OF|EITHER OF|WON OF|NOT|ALL OF|ANY OF)[a-zA-Z][a-zA-Z0-9_]*$")){
						/* checks if the variable from where the value will come from exists */
						if (symbol_value.ContainsKey(codes [i])) {
							try{
								/* Add the variable name as key to the hashtable, getting its value from the variable declared */
								symbol_value.Add (codes [i - 2], symbol_value[codes [i]]);
							} catch {
								/* Catch error if variable name (key) already exists */
								consolearea.Buffer.Text = consolearea.Buffer.Text + "> Syntax error: Multiple declaration of variable. (" + codes[i - 2] + ")\n";
								clear ();
							}		
						}
						/* if the variable from where the value will come from does not exists */
						else {
							consolearea.Buffer.Text = consolearea.Buffer.Text + "> Syntax error: Variable not found. (" + codes[i] + ")\n";
							clear ();
						}
					}
					/***********************************************************
					* INITIALIZED WITH EXPRESSION (SMOOSH)					   *
					***********************************************************/
					/* if value is a result of concatenation */
					else if(tokens[i].Equals("STRING CONCATENATION")){
						try {
							/* Add the variable name as key to the hashtable; 
						 	 * Note: The value is not yet added to the hashtable, it will be executed later. */
							symbol_value.Add (codes [i - 2], "");
							/* skips the initialization */
							for(;!codes[i].Contains("*");i++) {	}
						} catch {
							/* Catch error if variable name (key) already exists */
							consolearea.Buffer.Text = consolearea.Buffer.Text + "> Syntax error: Multiple declaration of variable. (" + codes[i - 2] + ")\n";
							clear ();
						}	
					} 
					/* else */
					else {
						try{
							/***********************************************************
							* INITIALIZED WITH LITERAL(NUMBR): I HAS A <var> ITZ 18	   *
							* INITIALIZED WITH LITERAL(NUMBAR): I HAS A <var> ITZ 18.15*
							***********************************************************/
							/* if the value's data type is integer(NUMBR) or float(NUMBAR) */
							if(tokens[i].Equals("NUMBAR") || tokens[i].Equals("NUMBR")){
								/* Add the variable name as key to the hashtable with a value of either integer or float */
								symbol_value.Add (codes [i - 2], codes [i]);	
							}
							/***********************************************************
							* INITIALIZED WITH EXPRESSION							   *
							* (ARITHMETIC, BOOLEAN, COMPARISON)                        *
							***********************************************************/
							/* else if the value is from an expression
							 * ARITHMETIC OPERATION
							 * BOOLEAN OPERATION
							 * COMPARISON OPERATION */
							else{
								/* Add the variable name as key to the hashtable; 
						 		 * Note: The value is not yet added to the hashtable, it will be executed later. */
								symbol_value.Add (codes [i - 2], "");
								/* skips the initialization */
								for(;!codes[i].Contains("*");i++){	}																
							}
						} catch {
							/* Catch error if variable name (key) already exists */
							consolearea.Buffer.Text = consolearea.Buffer.Text + "> Syntax error: Multiple declaration of variable. (" + codes[i - 2] + ")\n";
							clear ();
						}
					} /* end of else; if value is immediate value or a result of expression */
				} /* end of else; if variable declaration has initialization */
			} /* end of if; if the lexeme is variable declaration (I HAS A) */
		} /* end of for loop */
		return; /* return to caller */
	} /* end of initializehashmap() */
/**********************************************************************************************************************************************/

}