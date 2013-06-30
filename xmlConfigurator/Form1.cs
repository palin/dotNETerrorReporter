using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;

namespace ErrorReporter
{
  public partial class Form1 : Form
  {
    public static RichTextBox r;
    public static ErrorReporter er;
    public int x = 1;
    public int y = 2;
    public string text = "This is my report of exception in Form1 class";
    
    public Form1()
    {
      InitializeComponent();
      r = richTextBox1;

      // creates new object of ErrorReporter class
      er = new ErrorReporter("http://xmlreporter.herokuapp.com/reports", "POST", false, false);
      
      object o2 = null;
      
      try
      {
        int i2 = (int)o2;
      }
      catch(Exception e)
      {
        //sends report if error occurs
        er.Report(typeof(Form1), this, text, new System.Diagnostics.StackFrame(), e);
      }
    }
  }
}
