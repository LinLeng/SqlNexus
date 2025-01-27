using System;
using System.Collections;
using System.Data;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Data.Common;
using NexusInterfaces;
using BulkLoadEx;

[assembly: System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.RequestMinimum, Name = "FullTrust")]
namespace RowsetImportEngine
{
	public class RowsetTypes
	{
		// TODO: rewrite this (and KnownColumnTypes, and ReadRowsetPropertiesFromXml()) to use Types. 
		public TextRowset[] KnownRowsetTypes = new TextRowset[]
		{
				new TextRowset(),
				new InputbuffersRowset(),
				new DbccOpentranRowset(),
				new SimpleMessageRowset(),

		};
	}

	// Base textrowset class. In the base class all columns are assumed to be varchar. Also, the rowset is 
	// assumed to be terminated by an empty line, and each row in the rowset by "\n".  Derive from this class 
	// if any of these things must be changed. 
	public class TextRowset : NexusInterfaces.INexusImportedRowset
	{
        public BulkLoadEx.BulkLoadRowset Bulkload;
		// This collection is dynamically populated at runtime (in DefineRowsetColumns()) with the set of columns 
		// that was found in the text file rowset. 
		public System.Collections.ArrayList Columns = new ArrayList();
		/* Add any expected columns to the KnownColumns collection. Implied (rowset-level) columns must be 
		 * defined here, as well as any columns that need to be typed using something other than varchar.  
		 * Any columns that are in the input file rowset that are not listed here will still be added to the 
		 * Columns collection as with a default type of VarCharColumn. 
		*/
		public ArrayList KnownColumns = new ArrayList();
		public virtual string Name 
		{
			get 
			{
				return name;
			}
			set 
			{
				name = value.Trim();
			}
		}

		// true if rowset starts with "traditional" column header lines, e.g. 
		//    column1      column2  column3 
		//    ------------ -------- --------------------------------
		//    ...
		public virtual bool TabularRowset
		{
			get 
			{
				return true;
			}
		}

		public virtual bool IsEndOfRow (string RowText) 
		{
			// For typical recordsets every row in the text input file will be a distinct row in the rowset. 
			// This is checked in ProcessFile() after each line is read.  We always return true when this is 
			// called because in a default TextRowset every text row is assumed to be a complete rowset row. 
			// As currently implemented the end-of-row check only allows for multi-line rows, not multiple 
			// rows within a single text line. 
			return true;
		}

		public virtual bool IsEndOfRowset (string RowText, string PrevLine) 
		{
			// For typical recordsets every row in the text input file will be a distinct row in the rowset. 
			// The end of the rowset will be indicated by a blank line. This property is checked in ProcessFile(). 
			if (RowText == null)
                return true;
			if (RowText.Trim().Equals("") || RowText.StartsWith ("-- "))
				return true;	
			else
				return false;
		}

		protected string name;				// private storage for rowset name (e.g. "SYSPROCESSES")
		public string Runtime;				// Timestamp associated with the rowset (implied column, not part of the row data)
		public string Identifer;			// String in the input file that immediately precedes this rowset
		public string Spid;					// Spid # for inputbuffers (implied column, not part of the row data)
		public bool HasBeenEncountered = false;			// Set to true by the importer if we have seen this rowset in the input file
		private long rowsInserted = 0;		// Total # of rows imported by this rowset
        protected ILogger logger;
        public long RowsInserted
        {
            get
            {
                return rowsInserted;
            }
            set
            {
                rowsInserted = value;
            }
        }
		//public uint hConn = 0;				// Handle to BCP connection for this rowset's inserts
		public bool InBCPRowset = false;	// True if there is a BCP insert currently in-progress for this rowset
		public bool UsesTokens = false;		// True if any of the rowset's columns are supposed to define a token
        public TextRowset()
        {
        }
        public TextRowset(ILogger Logger) 
		{
            logger = Logger;
		}
		// Reset rowset properties for reuse if the same rowset appears later in the input file 
		public virtual void Clear()
		{
			Columns.Clear();
			this.Spid = "";
			this.Runtime = "";
		}

		// DefineRowsetColumns takes two lines like the below as input: 
		//    spid      blocked      waittype  
		//    --------- ------------ ---------------
		// and parses this to identify column names ("spid", "blocked", "waittype"), and the character 
		// width of each column.  For each column in the rowset header a Column object of the appropriate 
		// type is instantiated and added to the Columns collection. 
		public virtual void DefineRowsetColumns (string HeaderLines, string ColumnNames)
		{
			try
			{
				string[] SplitStrings;
				int i = 0;
				int extrachar = 0; // used to handle spaces as the first chars in the line

				// First add any implied rowset-level columns to the column list (these won't show up in the 
				// text rowset's column headers). 
				AddRowsetColumns();

				// osql/sqlcmd may add line numbers to the output, which prevents the column names 
				// from lining up with the ----'s in the column header.  For example, the output may 
				// look like this: 
				//
				//   1> 2> 1> 2> 3> 4> 5> 6>  databasename        dbid   
				//    -------------------- ------ 
				//    master                    1
 				//    AdventureWorks2000        7  
				// 
				// Detect and remove these line numbers so that the header + column names line up:  
				//
				//    databasename        dbid   
				//    -------------------- ------ 
				//    master                    1
				//    AdventureWorks2000        7  
				if (ColumnNames.Substring (0,3).Equals ("1> "))
					ColumnNames = ColumnNames.Substring(ColumnNames.IndexOf("  ") + 1);

				// Break apart the rowset header strings to define the actual row-level column metadata. 
				SplitStrings = HeaderLines.Split (new char[] {' '});
				foreach(String s in SplitStrings)
				{
					// See if the current "column" is actually just an empty space.  If so we'll add the 
					// space's width to the next column in the rowset.  osql & isql both insert a single 
					// space at the beginning of each row.
					if (0 == s.Length) 
					{
						extrachar++; 
						continue;
					}

					Column newcol = null;
					// Get the new column's length and name.
					int NewColumnLength = s.Length + extrachar;
                    int NegativeColumnLenght = 0;
					string NewColumnName = ColumnNames.Substring (i);
					if (NewColumnName.Length > NewColumnLength) NewColumnName = NewColumnName.Substring(0, NewColumnLength);
					NewColumnName = NewColumnName.Trim();
					
					// See if we have metadata for this column in the KnownColumns collection. 
					foreach (Column c in KnownColumns) 
					{
						if (NewColumnName == c.Name)
						{
							newcol = c;

                            // Set column length to width observed in this raw text data file (rowset). 
                            newcol.Length = NewColumnLength;

                            //Description on handling VARCHAR(MAX)
                            //if c.SqlColumnLength is -1 (meaning we want (MAX)) then don't need to do anything because it comes from the TextRowset.xml file already
                            //and it gets copied into newcol.SqlColumnLength

                            // Use data file rowset col length for SQL col length, unless a longer length 
                            // has been specified in rowset metadata (TextRowsets.xml)).
                            //Else the column definition is preserved based on TextRowset.xml

                            if ((c.SqlColumnLength != Column.SQL_MAX_LENGTH) && (newcol.SqlColumnLength <= newcol.Length))
								newcol.SqlColumnLength = NewColumnLength;

                            break;
						}
					}
					// Default to simple varchar if we didn't find the column's name in the rowset's KnownColumns list. 
					if (null == newcol)	newcol = new VarCharColumn(NewColumnName, NewColumnLength, NewColumnLength, false);
				
					// Add new column to rowset Columns collection. 
					this.Columns.Add (newcol);
					i += NewColumnLength + 1; // Move past the column (& space) in the text row containing column names.
					extrachar = 0; // Reset extrachar counter. 
				}
			}
			catch (Exception e)
			{
				ErrorDialog ed = new ErrorDialog(e, true, this.logger);
				ed.Handle();
			}
			return;
		}

		// ParseRow() takes an unparsed line as input, breaks it up on expected column boundaries, and uses each 
		// field to populate the Data property of each of the Column objects in the Columns collection. 
		public virtual void ParseRow (string RowText)
		{
			int strpos = 0;
			int colnum = 0;
			try 
			{
                //(this.Rows[RowsCached] as ArrayList).Clear();
                foreach (Column c in this.Columns)
                {
                    // Skip the special rowset-level columns (e.g. runtime) and columns that are supposed 
                    // to take on the value of a "token"; these don't appear in the input row's data. 
                    if ((c.RowsetLevel) || ("" != c.ValueToken))
                    {
                        colnum++;
                        continue;
                    }
                    // Avoid exception if row data is truncated for some reason. 
                    // Make column length the size of entire row minus the current string position, in case we jump over the entire row length
                    int len = c.Length;
                    if (RowText.Length < (strpos + c.Length))
                    {
                        len = RowText.Length - strpos;
                    }
                    //assign the stripped data to a column data field
                    if (len > 0)
                    {
                        c.Data = RowText.Substring(strpos, len).Trim();
                    }
                    else
                    { 
                        c.Data = null;
                    }

                    // Move past the current field data (plus the space separator) to the next field in the row.
					strpos += c.Length + 1;
					colnum++;
				}
			}
			catch (Exception e)
			{
				ErrorDialog ed = new ErrorDialog(e,true, this.logger);
				ed.Handle();
			}
			return;
		}

		// Step through the KnownColumns collection and add any rowset-level implied data columns (e.g. "runtime")
		// to the Columns collection. 
		protected virtual void AddRowsetColumns()
		{
			foreach (Column c in this.KnownColumns)
			{
				if (c.RowsetLevel || "" != c.ValueToken) 
				{
					this.Columns.Add(c);
					// Copy rowset-level data to the column. 
					if (c.Name == "runtime" && c.RowsetLevel) 
					{
						c.Data = this.Runtime;
					}
					if (c.Name == "spid" && c.RowsetLevel) 
					{
						c.Data = this.Spid;
					}
				}
			}
		}

		// Check to see if an encountered rowset is one of the rowsets that is handled by this class. 
		// Parameters are strings from the input file that look like this: 
		//		RowsetIdentifier	= "SYSPROCESSES"
		//		ColumnNames			= "spid        status             blocked   ... "
		//		ColumnLines			= "----------- ------------------ --------- ... "
		// The default implementation simply checks to see whether the string in the Identifier member variable 
		// exists as a substring of the RowsetIdentifier or ColumnNames input param strings. 
		public virtual bool CheckForRowsetStart (string ColumnLines, string ColumnNames, string RowsetIdentifier)
		{
			if (RowsetIdentifier != null)
			{
				if (RowsetIdentifier.IndexOf(this.Identifer) >= 0)
				{
					this.Clear();	// We're about to be reused.  Get rid of properties from last use. 
					return true;
				}
			} 
			if (ColumnNames != null)
			{
				if (ColumnNames.IndexOf(this.Identifer) >= 0)
				{
					this.Clear();	// We're about to be reused.  Get rid of properties from last use. 
					return true;
				}
			}
			return false;
		}

		// Makes a shallow (memberwise) copy of the object.  Used in TextRowsetImporter.ReadRowsetPropertiesFromXml().
		public virtual TextRowset Copy ()
		{
			TextRowset newrowset = new TextRowset();
			newrowset = (TextRowset)this.MemberwiseClone();
			// We want new column arrays, not copies of pointers to the existing arrays. 
			newrowset.KnownColumns = new ArrayList();
			newrowset.Columns = new ArrayList();
			return newrowset;
		}
	}


	public class InputbuffersRowset : TextRowset
	{
		public override bool IsEndOfRow (string RowText) 
		{
            // For typical recordsets every row in the text input file will be a distinct row in the rowset. 
            // For DBCC INPUTBUFFER output, however, the command always returns exactly one row, and the text 
            // for the row may span multiple lines.  The row and the rowset are terminated by "(1 row affected)" and 
            // "DBCC execution completed. If DBCC printed error messages, contact your system administrator.". 
            // Note: some versions of our query tools print "(1 rows(s) affected)" while others print 
            // "(1 row affected)".

            if (RowText == null)
                return true;
            return ContainsDBCCCompletedMessage(RowText);
		}
		public override bool IsEndOfRowset (string RowText, string PrevLine) 
		{
			return ContainsDBCCCompletedMessage (PrevLine);
		}

		public override bool CheckForRowsetStart (string ColumnLines, string ColumnNames, string RowsetIdentifier)
		{
			if (RowsetIdentifier != null)
			{
				if (RowsetIdentifier.IndexOf(this.Identifer) >= 0)
				{
					this.Clear();
					// Rowset identifier string looks like: 
					//   DBCC INPUTBUFFER FOR SPID 63
					// Capture the special rowset-level spid value. 
					try 
					{
						this.Spid = RowsetIdentifier.Split(' ')[4];
					} 
					catch 
					{
						this.Spid = null;
					}
					return true;
				}
				else
					return false;
			}
			else
				return false;
		}

		private bool ContainsDBCCCompletedMessage (string RowText) 
		{
			// HACK: This is ugly, but unavoidable.  Future standard data collection scripts should: 
			//  - Be captured as Unicode
			//  - Print unique end-of-rowset strings for non-standard "rowsets" to avoid non-deterministic 
			//    behavior like this that is difficult to account for programmatically. 
			// Unfortunately, neither of these is true for our current SQL 2000 blocker script. 

			// For typical recordsets every row in the text input file will be a distinct row in the rowset. 
			// For DBCC INPUTBUFFER output, however, the command always returns exactly one row, and the text 
			// for the row may span multiple lines.  The row and the rowset are terminated by "(1 row affected)" and 
			// "DBCC execution completed. If DBCC printed error messages, contact your system administrator.". 
			// Note: some versions of our query tools print "(1 rows(s) affected)" while others print 
			// "(1 row affected)".

			// TODO: Generalize this for other uses by providing a way to specify custom end-of-rowset indicators in TextRowsets.xml. 
			
			if (RowText == null) return true;
            if ((RowText.IndexOf("(1 row") >= 0) || (RowText.IndexOf("DBCC execution completed") >= 0)) //ENU
                return true;
            else if ((RowText.IndexOf("(1 ligne") >= 0) || (RowText.IndexOf("cution de DBCC termin") >= 0)) //FRA: Exécution de DBCC terminée
                return true;
            else if (RowText.IndexOf("do DBCC foi conclu") >= 0) //BRZ: do DBCC foi concluída
                return true;
            else if (RowText.IndexOf("DBCC ") >= 0) //CHS: DBCC 执行完毕。如果 
                return true;
            else if (RowText.IndexOf("DBCC ") >= 0) //CHT: DBCC 的執行已經完成。如果
                return true;
            else if (RowText.IndexOf("DBCC-uitvoering voltooid") >= 0) //DUT: DBCC-uitvoering voltooid
                return true;
            else if (RowText.IndexOf("n de DBCC completada") >= 0) //ESN: Ejecución de DBCC completada
                return true;
            else if (RowText.IndexOf("DBCC-Ausf") >= 0) //GER: DBCC-Ausführung abgeschlossen
                return true;
            else if (RowText.IndexOf("Esecuzione DBCC completata") >= 0) //ITA: Esecuzione DBCC completata
                return true;
            else if (RowText.IndexOf("DBCC ") >= 0) //JPN: DBCC の実行が完了しました。
                return true;
            else if (RowText.IndexOf("DBCC ") >= 0) //KOR: DBCC 실행이 완료되었습니다
                return true;
            else if ((RowText.IndexOf("DBCC-k") >= 0) && (RowText.IndexOf("rning klar") >= 0)) //SVE: DBCC-körning klar
                return true;
            else
                return false;
		}
	}

	public class DbccOpentranRowset : SimpleMessageRowset
	{
		public override void DefineRowsetColumns (string HeaderLines, string ColumnNames)
		{
			/* In the blocking script, DBCC OPENTRAN output looks like this: 
			
					DBCC OPENTRAN FOR DBID 7
					Transaction information for database 'eas'.

					Oldest active transaction:
						SPID (server process ID) : 66
						UID (user ID) : 13
						Name          : user_transaction
						LSN           : (186442:123009:1)
						Start time    : Feb  3 2005  8:18:32:810AM
					DBCC execution completed. If DBCC printed error messages, contact your system administrator.
					
			   Import all of this as a single varchar column. 
			*/
			try
			{
				base.DefineRowsetColumns(HeaderLines, ColumnNames);
				// Find the dbid column and assign the dbid from the "DBCC OPENTRAN FOR DBID 7" header to the column
				foreach (Column c in this.KnownColumns)
				{
					if ("dbid" == c.Name)
					{
						if (HeaderLines.IndexOf("tempdb database")>0)	//"DBCC OPENTRAN FOR tempdb database"
							c.Data = 2;
						else
							c.Data = HeaderLines.Substring(HeaderLines.IndexOf("DBCC OPENTRAN FOR DBID") + 22);
					}
				}
			}
			catch (Exception e)
			{
                ErrorDialog ed = new ErrorDialog(e, true, this.logger);
				ed.Handle();
			}
			return;
		}
		public override bool IsEndOfRow (string RowText) 
		{
            if ((RowText.IndexOf("(1 row") >= 0) || (RowText.IndexOf("DBCC execution completed") >= 0) || (RowText.IndexOf("DBCC ") >= 0))
			{
				HaveReachedEndOfRow = true;
				return true;	
			}
			else
                return false;
		}
	}

	// A SimpleMessageRowset is a single-line, single-field value that we are supposed to insert 
	// into a single-column table.  For ex., in the blocker script we have "Start time: <timestamp>" 
	// messages.  We want to import the timestamps into a tbl_RUNTIMES table with a single [runtime] 
	// column. Another use is for DebugPrint messages that we may want to import for quick analysis. 
	public class SimpleMessageRowset : TextRowset
	{
		protected bool HaveReachedEndOfRow = false;
		public override bool IsEndOfRow (string RowText) 
		{
			// There's only one line and one row in this type of "rowset" -- once we've 
			// read it, we're done with the rowset. 
			HaveReachedEndOfRow = true;
			return true;
		}
		public override bool IsEndOfRowset (string RowText, string PrevLine)
		{
			return HaveReachedEndOfRow;
		}
		public override bool TabularRowset
		{
			get 
			{
				return false;
			}
		}
		public override bool CheckForRowsetStart (string ColumnLines, string ColumnNames, string RowsetIdentifier)
		{
			if (ColumnLines != null)
			{
				if (ColumnLines.IndexOf(this.Identifer) >= 0)
				{
					this.Clear();	// We're about to be reused.  Get rid of properties from last use. 
					return true;
				}
			}
			return false;
		}
		public override void DefineRowsetColumns (string HeaderLines, string ColumnNames)
		{
			try
			{
				foreach (Column c in this.KnownColumns)
				{
					this.Columns.Add(c);
					if (c.RowsetLevel) 
					{
						// Copy rowset-level data to the column. 
						if (c.Name == "runtime" && c.RowsetLevel) 
						{
							c.Data = this.Runtime;
						}
						if (c.Name == "spid" && c.RowsetLevel) 
						{
							c.Data = this.Spid;
						}
					}
				}
			}
			catch (Exception e)
			{
                ErrorDialog ed = new ErrorDialog(e, true, this.logger);
				ed.Handle();
			}
		}
		public override void ParseRow (string RowText)
		{
			base.ParseRow(RowText.Substring(RowText.IndexOf(this.Identifer)+this.Identifer.Length));
		}
		public override void Clear()
		{
			HaveReachedEndOfRow = false;
			base.Clear();
		}
	}
}

